using UnityEngine;
using GameSystems.Stats;
using System;

public class TurnBasedExample : MonoBehaviour
{
	private TurnTracker turnTracker;
	private ModifiableValue<float> attack;
	private ModifiableValue<float> defense;

	void Start()
	{
		SetupTurnTracker();
		SetupBasicTurnBasedModifiers();
		SetupTurnBasedWrapper();
		SetupMixedTimeAndTurnBased();

		Debug.Log("🎮 Turn-Based System Ready!");
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			turnTracker.NextTurn();
			Debug.Log($"<color=yellow>=== Turn {turnTracker.CurrentTurn} ===</color>");
			Debug.Log($"Attack: {attack.Value} (Base: {attack.InitialValue})");
			Debug.Log($"Defense: {defense.Value} (Base: {defense.InitialValue})");
		}

		if (Input.GetKeyDown(KeyCode.R))
		{
			ResetDemo();
		}
	}

	void SetupTurnTracker()
	{
		turnTracker = new TurnTracker();

		turnTracker.OnTurnStart += (turn) =>
		{
			Debug.Log($"<color=cyan>📢 Turn {turn} Started</color>");
		};

		turnTracker.OnTurnEnd += (turn) =>
		{
			Debug.Log($"<color=gray>   Turn {turn} Ended</color>");
		};
	}

	void SetupBasicTurnBasedModifiers()
	{
		Debug.Log("\n⚔️ BASIC TURN-BASED MODIFIERS");

		attack = new ModifiableValue<float>(20f);
		defense = new ModifiableValue<float>(10f);

		attack.PropertyChanged += (s, e) => Debug.Log($"Attack changed: {attack.Value}");
		defense.PropertyChanged += (s, e) => Debug.Log($"Defense changed: {defense.Value}");

		var rageBuff = Modifier.Plus(15f, priority: 100, name: "Rage Buff");
		rageBuff.DisableAfterTurns(3, turnTracker);

		var shieldBuff = Modifier.Plus(20f, priority: 0, name: "Shield Buff");
		shieldBuff.EnableAfterTurns(2, turnTracker);

		attack.Modifiers.Add(rageBuff);
		defense.Modifiers.Add(shieldBuff);

		Debug.Log($"Initial - Attack: {attack.Value}, Defense: {defense.Value}");
	}

	void SetupTurnBasedWrapper()
	{
		Debug.Log("\n🔄 TURN-BASED MODIFIER WRAPPER");

		var speed = new ModifiableValue<float>(15f);
		speed.PropertyChanged += (s, e) => Debug.Log($"Speed changed: {speed.Value}");

		var speedBoost = TurnBasedModifierFactory.Times(1.5f, turns: 5, priority: 50, name: "Speed Boost");
		speed.Modifiers.Add(speedBoost as IModifier<float>);

		turnTracker.RegisterTurnEndCallback((turn) =>
		{
			if (speedBoost is TurnBasedModifierWrapper<float> wrapper)
			{
				Debug.Log($"   Speed Boost: {wrapper.RemainingTurns} turns remaining");
			}
		});

		Debug.Log($"Speed with boost: {speed.Value}");
	}

	void SetupMixedTimeAndTurnBased()
	{
		Debug.Log("\n⏰ MIXED TIME & TURN-BASED");

		var stamina = new ModifiableValue<float>(100f);
		stamina.PropertyChanged += (s, e) => Debug.Log($"Stamina changed: {stamina.Value}");

		var fatigueDebuff = Modifier.Minus(10f, priority: 0, name: "Fatigue");
		fatigueDebuff.DisableAfterTurns(3, turnTracker);

		var adrenalineBuff = Modifier.Plus(20f, priority: 100, name: "Adrenaline");
		adrenalineBuff.DisableAfter(TimeSpan.FromSeconds(10f));

		stamina.Modifiers.Add(fatigueDebuff);
		stamina.Modifiers.Add(adrenalineBuff);

		Debug.Log($"Stamina (mixed): {stamina.Value}");
		Debug.Log($"  Press <color=yellow>Space</color> to advance turns");
		Debug.Log($"  Press <color=yellow>R</color> to reset demo");
	}

	void ResetDemo()
	{
		attack.Modifiers.Clear();
		defense.Modifiers.Clear();

		turnTracker.Reset();
		turnTracker.ClearCallbacks();

		SetupTurnTracker();
		SetupBasicTurnBasedModifiers();

		Debug.Log("<color=green>🔄 Demo Reset!</color>");
	}

	void DemonstrateTurnBasedPriority()
	{
		Debug.Log("\n🎯 TURN-BASED PRIORITY");

		var damage = new ModifiableValue<float>(100f);

		var critBuff = TurnBasedModifierFactory.Times(2f, turns: 2, priority: 100, name: "Critical");
		var baseBuff = TurnBasedModifierFactory.Plus(50f, turns: 4, priority: 0, name: "Base Boost");

		damage.Modifiers.Add(critBuff as IModifier<float>);
		damage.Modifiers.Add(baseBuff as IModifier<float>);

		turnTracker.RegisterTurnEndCallback((turn) =>
		{
			if (turn <= 4)
			{
				Debug.Log($"Turn {turn}: Damage = {damage.Value}");

				if (critBuff is TurnBasedModifierWrapper<float> critWrapper)
				{
					Debug.Log($"  Crit: {critWrapper.RemainingTurns} turns left");
				}

				if (baseBuff is TurnBasedModifierWrapper<float> baseWrapper)
				{
					Debug.Log($"  Base: {baseWrapper.RemainingTurns} turns left");
				}
			}
		});
	}

	void DemonstrateConditionalTurnModifiers()
	{
		Debug.Log("\n🔀 CONDITIONAL TURN MODIFIERS");

		var health = new ModifiableValue<float>(100f);

		var healOverTurns = Modifier.Plus(10f, priority: 50, name: "Heal Over Turns");
		healOverTurns.DisableAfterTurns(5, turnTracker);

		health.Modifiers.Add(healOverTurns);

		turnTracker.RegisterTurnEndCallback((turn) =>
		{
			if (health.Value >= health.InitialValue + 50)
			{
				healOverTurns.Enabled = false;
				Debug.Log($"<color=green>Heal stopped at turn {turn} (max health reached)</color>");
			}
		});
	}
}
