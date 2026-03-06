using UnityEngine;
using System.Collections.Generic;
using System.Timers;
using System;
using Timer = System.Timers.Timer;

namespace GameSystems.Stats
{
	public class UnitStatController : MonoBehaviour
	{
		[SerializeField] private string unitName = "Hero";
		[SerializeField] private int level = 1;
		[SerializeField] private bool debugMode = true;

		[SerializeField] private UnitStatsCollection statsCollection = new UnitStatsCollection();

		[Header("Runtime Info")]
		[SerializeField] private int currentIndex = -1;
		[SerializeField] private int totalIterations = 0;

		private Timer regenTimer;
		private readonly object regenLock = new object();

		private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		private long GetCurrentTimeTicks()
		{
			return (long)(DateTime.UtcNow - epoch).TotalMilliseconds;
		}

		public string UnitName
		{
			get => unitName;
			set => unitName = value;
		}

		public int Level => level;
		public UnitStatsCollection Stats => statsCollection;
		public Stat CurrentStat => statsCollection.CurrentStat;

		public event System.Action<Stat> OnStatChanged;
		public event System.Action<Stat> OnStatDepleted;
		public event System.Action OnLevelUp;
		public event System.Action<Stat> OnRegenComplete;

		void Start()
		{
			if (statsCollection.Stats.Count == 0)
			{
				SetupDefaultStats();
			}

			statsCollection.Initialize();

			SetupRegenTimer();

			UpdateRuntimeInfo();
			LogDebug("✅ Unit stats ready!");
		}

		void Update()
		{
			HandleInput();
		}

		#region Polling

		public float GetStatValue(string statId)
		{
			Stat stat = statsCollection.GetStatById(statId);
			return stat?.GetValueWithPending() ?? 0f;
		}

		public float GetStatValue(Stat stat)
		{
			return stat?.GetValueWithPending() ?? 0f;
		}

		public float GetCurrentStatValue()
		{
			return CurrentStat?.GetValueWithPending() ?? 0f;
		}

		#endregion

		#region Setup

		private void SetupDefaultStats()
		{
			statsCollection.AddItem(new Stat("hp", "Health", StatType.Health, 100f, 100f, true, 1f));
			statsCollection.AddItem(new Stat("mp", "Mana", StatType.Mana, 50f, 50f, true, 2f));
			statsCollection.AddItem(new Stat("stamina", "Stamina", StatType.Stamina, 100f, 100f, true, 5f));

			statsCollection.AddItem(new Stat("attack", "Attack", StatType.Attack, 20f));
			statsCollection.AddItem(new Stat("defense", "Defense", StatType.Defense, 10f));
			statsCollection.AddItem(new Stat("speed", "Speed", StatType.Speed, 15f));
		}

		private void SetupRegenTimer()
		{
			regenTimer = new Timer(100);
			regenTimer.Elapsed += OnRegenTimerElapsed;
			regenTimer.AutoReset = true;
			regenTimer.Start();
		}

		private void OnRegenTimerElapsed(object sender, ElapsedEventArgs e)
		{
			foreach (var stat in statsCollection.Stats)
			{
				if (stat.CanRegenerate && stat.CurrentValue < stat.MaxValue)
				{
					float regen = stat.RegenRate * 0.1f;
					float newValue = Mathf.Min(stat.CurrentValue + regen, stat.MaxValue);
					stat.SetCurrent(newValue);

					if (stat.IsAtMax())
					{
						lock (regenLock)
						{
							OnRegenComplete?.Invoke(stat);
							LogDebug($"<color=green>✨ {stat.StatName} fully regenerated!</color>");
						}
					}
				}
			}
		}

		#endregion

		#region Navigation

		public void Next()
		{
			Stat stat = statsCollection.Next();
			UpdateRuntimeInfo();
			LogDebug($"→ {stat}");
		}

		public void Previous()
		{
			Stat stat = statsCollection.Previous();
			UpdateRuntimeInfo();
			LogDebug($"← {stat}");
		}

		public void First()
		{
			Stat stat = statsCollection.First();
			UpdateRuntimeInfo();
			LogDebug($"⏮ {stat}");
		}

		public void Last()
		{
			Stat stat = statsCollection.Last();
			UpdateRuntimeInfo();
			LogDebug($"⏭ {stat}");
		}

		#endregion

		#region Stat Modification

		public void IncreaseCurrent(float amount)
		{
			Stat stat = CurrentStat;
			if (stat != null)
			{
				stat.Add(amount);
				OnStatChanged?.Invoke(stat);
			}
		}

		public void DecreaseCurrent(float amount)
		{
			Stat stat = CurrentStat;
			if (stat != null)
			{
				stat.Subtract(amount);

				if (stat.IsDepleted())
				{
					OnStatDepleted?.Invoke(stat);
					LogDebug($"<color=red>⚠️ {stat.StatName} depleted!</color>");
				}

				OnStatChanged?.Invoke(stat);
			}
		}

		public void RestoreCurrent()
		{
			Stat stat = CurrentStat;
			if (stat != null)
			{
				stat.RestoreToMax();
				OnStatChanged?.Invoke(stat);
			}
		}

		public void RestoreAll()
		{
			foreach (var stat in statsCollection.Stats)
			{
				stat.RestoreToMax();
			}
			LogDebug("<color=green>✨ All stats restored!</color>");
		}

		#endregion

		#region Modifiers

		public void AddModifier(string statId, IModifier<float> modifier)
		{
			Stat stat = statsCollection.GetStatById(statId);
			if (stat != null)
			{
				stat.Modifiers.Add(modifier);
				LogDebug($"<color=cyan>+ Added modifier:</color> {modifier}");
			}
		}

		public void AddMaxModifier(string statId, IModifier<float> modifier)
		{
			Stat stat = statsCollection.GetStatById(statId);
			if (stat != null)
			{
				stat.MaxModifiers.Add(modifier);
				LogDebug($"<color=cyan>+ Added max modifier:</color> {modifier}");
			}
		}

		public void ClearAllModifiers()
		{
			foreach (var stat in statsCollection.Stats)
			{
				stat.Modifiers.Clear();
				stat.MaxModifiers.Clear();
			}
			LogDebug("<color=yellow>Cleared all modifiers</color>");
		}

		#endregion

		#region Level Up

		public void LevelUp()
		{
			level++;

			foreach (var stat in statsCollection.Stats)
			{
				float baseIncrease = GetStatIncreaseForLevel(stat.StatType);
				float maxIncrease = GetMaxIncreaseForLevel(stat.StatType);
				stat.LevelUp(baseIncrease, maxIncrease);
			}

			OnLevelUp?.Invoke();
			LogDebug($"<color=yellow>🎉 LEVEL UP! → Level {level}</color>");
		}

		private float GetStatIncreaseForLevel(StatType type)
		{
			return type switch
			{
				StatType.Health => 10f,
				StatType.Mana => 5f,
				StatType.Stamina => 5f,
				StatType.Attack => 2f,
				StatType.Defense => 1f,
				StatType.Speed => 1f,
				StatType.CriticalRate => 0.01f,
				StatType.CriticalDamage => 0.05f,
				StatType.Accuracy => 0.01f,
				StatType.Evasion => 0.01f,
				_ => 1f
			};
		}

		private float GetMaxIncreaseForLevel(StatType type)
		{
			return type switch
			{
				StatType.Health => 10f,
				StatType.Mana => 5f,
				StatType.Stamina => 5f,
				_ => 0f
			};
		}

		#endregion

		#region Sorting

		public void SortByType()
		{
			statsCollection.SortByType();
			UpdateRuntimeInfo();
			LogDebug("Sorted by Type");
		}

		public void SortByValue()
		{
			statsCollection.SortByValue();
			UpdateRuntimeInfo();
			LogDebug("Sorted by Value");
		}

		#endregion

		#region Input

		private void HandleInput()
		{
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				Next();
			}
			else if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				Previous();
			}
			else if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				IncreaseCurrent(10f);
			}
			else if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				DecreaseCurrent(10f);
			}
			else if (Input.GetKeyDown(KeyCode.R))
			{
				RestoreCurrent();
			}
			else if (Input.GetKeyDown(KeyCode.F))
			{
				RestoreAll();
			}
			else if (Input.GetKeyDown(KeyCode.L))
			{
				LevelUp();
			}
		}

		#endregion

		#region Info

		private void UpdateRuntimeInfo()
		{
			currentIndex = statsCollection.GetCurrentIndex();
			totalIterations = statsCollection.GetTotalIterations();
		}

		public void ShowStatsInfo()
		{
			Debug.Log("\n<color=cyan>═══════════════════════════════════════</color>");
			Debug.Log($"<color=yellow>📊 {unitName} Stats (Level {level}) 📊</color>");
			Debug.Log("<color=cyan>═══════════════════════════════════════</color>");

			foreach (var stat in statsCollection.Stats)
			{
				Debug.Log($"  {stat}");
			}

			Debug.Log("<color=cyan>═══════════════════════════════════════</color>\n");
		}

		private void LogDebug(string message)
		{
			if (debugMode)
			{
				Debug.Log($"<color=magenta>[{unitName}]</color> {message}");
			}
		}

		private void OnDestroy()
		{
			if (regenTimer != null)
			{
				regenTimer.Stop();
				regenTimer.Dispose();
			}
		}

		#endregion
	}
}
