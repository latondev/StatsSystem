using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystems.Stats
{
	[Serializable]
	public class Stat
	{
		[SerializeField] private string statId;
		[SerializeField] private string statName;
		[SerializeField] private StatType statType;
		[SerializeField] private bool canRegenerate;
		[SerializeField] private float regenRate;

		private ModifiableValue<float> _modifiableValue;
		private ModifiableValue<float> _maxValue;
		private BoundedValue<float> _currentValue;

		public string StatId => statId;
		public string StatName => statName;
		public StatType StatType => statType;
		public bool CanRegenerate => canRegenerate;
		public float RegenRate => regenRate;

		public float BaseValue => _modifiableValue.InitialValue;
		public float MaxValue => _maxValue.Value;
		public float CurrentValue => _currentValue.Value;

		public ModifiableValue<float> ModifiableValue => _modifiableValue;
		public ModifiableValue<float> MaxModifiableValue => _maxValue;
		public ICollection<IModifier<float>> Modifiers => _modifiableValue.Modifiers;
		public ICollection<IModifier<float>> MaxModifiers => _maxValue.Modifiers;

		public event System.Action<float> OnValueChanged;

		public Stat(string id, string name, StatType type, float baseValue, float maxValue = -1, bool canRegen = false, float regenRate = 0f)
		{
			this.statId = id;
			this.statName = name;
			this.statType = type;
			this.canRegenerate = canRegen;
			this.regenRate = regenRate;

			_modifiableValue = new ModifiableValue<float>(baseValue);
			_maxValue = new ModifiableValue<float>(maxValue > 0 ? maxValue : baseValue);
			_currentValue = new BoundedValue<float>(0f, baseValue, _maxValue);

			_maxValue.PropertyChanged += (s, e) =>
			{
				OnValueChanged?.Invoke(_currentValue.Value);
			};

			_currentValue.PropertyChanged += (s, e) =>
			{
				OnValueChanged?.Invoke(_currentValue.Value);
			};
		}

		public float GetFinalValue()
		{
			return _modifiableValue.Value;
		}

		public float GetValueWithPending()
		{
			return _currentValue.Value;
		}

		public float GetPercentage()
		{
			if (MaxValue <= 0) return 0f;
			return CurrentValue / MaxValue;
		}

		public bool IsDepleted()
		{
			return CurrentValue <= 0f;
		}

		public bool IsAtMax()
		{
			return Mathf.Approximately(CurrentValue, MaxValue);
		}

		public void Add(float amount)
		{
			_currentValue.Value += amount;
		}

		public void Subtract(float amount)
		{
			_currentValue.Value -= amount;
		}

		public void SetCurrent(float value)
		{
			_currentValue.Value = value;
		}

		public void RestoreToMax()
		{
			_currentValue.Value = MaxValue;
			Debug.Log($"<color=cyan>Restored</color> {statName} to max ({MaxValue})");
		}

		public void IncreaseMax(float amount)
		{
			_maxValue.InitialValue += amount;
			Debug.Log($"<color=green>Max {statName}</color> increased by {amount} → {MaxValue}");
		}

		public void LevelUp(float baseIncrease, float maxIncrease)
		{
			_modifiableValue.InitialValue += baseIncrease;
			_maxValue.InitialValue += maxIncrease;
			_currentValue.Value += baseIncrease;
			Debug.Log($"<color=yellow>⬆ Level Up!</color> {statName} +{baseIncrease} → {_modifiableValue.InitialValue}");
		}

		public override string ToString()
		{
			string icon = GetStatIcon();
			if (MaxValue > 0)
			{
				return $"{icon} {statName}: {CurrentValue:F0}/{MaxValue:F0}";
			}
			return $"{icon} {statName}: {GetFinalValue():F0}";
		}

		public string GetStatIcon()
		{
			return statType switch
			{
				StatType.Health => "❤️",
				StatType.Mana => "💙",
				StatType.Stamina => "💚",
				StatType.Attack => "⚔️",
				StatType.Defense => "🛡️",
				StatType.Speed => "⚡",
				StatType.CriticalRate => "🎯",
				StatType.CriticalDamage => "💥",
				StatType.Accuracy => "🔍",
				StatType.Evasion => "💨",
				_ => "📊"
			};
		}

		public Color GetStatColor()
		{
			return statType switch
			{
				StatType.Health => new Color(1f, 0.3f, 0.3f),
				StatType.Mana => new Color(0.3f, 0.5f, 1f),
				StatType.Stamina => new Color(0.3f, 1f, 0.3f),
				StatType.Attack => new Color(1f, 0.5f, 0.2f),
				StatType.Defense => new Color(0.5f, 0.7f, 1f),
				StatType.Speed => new Color(1f, 1f, 0.3f),
				StatType.CriticalRate => new Color(1f, 0.8f, 0.3f),
				StatType.CriticalDamage => new Color(1f, 0.3f, 0.8f),
				StatType.Accuracy => new Color(0.7f, 0.9f, 1f),
				StatType.Evasion => new Color(0.8f, 1f, 0.8f),
				_ => Color.white
			};
		}
	}

	public enum StatType
	{
		Health,
		Mana,
		Stamina,
		Attack,
		Defense,
		Speed,
		CriticalRate,
		CriticalDamage,
		Accuracy,
		Evasion
	}
}
