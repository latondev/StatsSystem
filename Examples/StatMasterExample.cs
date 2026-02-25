using UnityEngine;
using GameSystems.Stats;
using System;

public class StatMasterExample : MonoBehaviour
{
	[SerializeField] private UnitStatController unitStats;

	void Start()
	{
		unitStats.UnitName = "Hero";

		DemonstrateModifiers();
		DemonstratePriority();
		DemonstrateTimeBasedModifier();
		DemonstrateComputedValues();
		DemonstrateBoundedValue();

		Debug.Log("📊 StatMaster-Style System Ready!");
	}

	void DemonstrateModifiers()
	{
		Debug.Log("\n🔧 BASIC MODIFIERS");

		var attack = new ModifiableValue<float>(100f);
		attack.PropertyChanged += (s, e) => Debug.Log($"Attack changed: {attack.Value}");

		attack.Modifiers.Add(Modifier.Plus(10f, 0,"+10 base"));
		Debug.Log($"After +10: {attack.Value}");

		attack.Modifiers.Add(Modifier.Times(1.5f,0, "+50%"));
		Debug.Log($"After +50%: {attack.Value}");

		attack.Modifiers.Add(Modifier.Divide(2f,0, "/2"));
		Debug.Log($"After /2: {attack.Value}");
	}

	void DemonstratePriority()
	{
		Debug.Log("\n⚡ PRIORITY SYSTEM");

		var health = new ModifiableValue<float>(100f);
		health.PropertyChanged += (s, e) => Debug.Log($"Health: {health.Value}");

		health.Modifiers.Add(Modifier.Plus(50f, priority: 100));
		health.Modifiers.Add(Modifier.Times(2f, priority: 0));

		Debug.Log($"Times(2) first, then Plus(50): {health.Value}");
	}

	void DemonstrateTimeBasedModifier()
	{
		Debug.Log("\n⏱️ TIME-BASED MODIFIER");

		var defense = new ModifiableValue<float>(20f);
		defense.PropertyChanged += (s, e) => Debug.Log($"Defense: {defense.Value}");

		var shield = Modifier.Plus(30f, 0,"Shield Buff");
		defense.Modifiers.Add(shield);

		Debug.Log($"With shield: {defense.Value}");

		shield.DisableAfter(TimeSpan.FromSeconds(3f));
		Debug.Log("Shield will disable in 3 seconds...");
	}

	void DemonstrateComputedValues()
	{
		Debug.Log("\n🔗 COMPUTED VALUES");

		var strength = new ModifiableValue<int>(10);
		var agility = new ModifiableValue<int>(8);
		var level = new PropertyValue<int>(5);

		var totalStats = strength.Zip(agility, (s, a) => s + a);
		totalStats.PropertyChanged += (s, e) => Debug.Log($"Total Stats: {totalStats.Value}");

		strength.Modifiers.Add(Modifier.Plus(2));
		Debug.Log($"After strength +2: {totalStats.Value}");

		IReadOnlyValue<float> hpAdjustment = strength.Select(s => (float)s * 10);
		ModifiableValue<float> maxHealth = new ModifiableValue<float>(100f);
		maxHealth.Modifiers.Add(Modifier.Create(hpAdjustment, 0));

		Debug.Log($"Max HP with strength({strength.Value}): {maxHealth.Value}");
	}

	void DemonstrateBoundedValue()
	{
		Debug.Log("\n📦 BOUNDED VALUE");

		var maxHP = new ModifiableValue<float>(100f);
		var currentHP = new BoundedValue<float>(0f, 100f, maxHP);

		currentHP.PropertyChanged += (s, e) => Debug.Log($"HP: {currentHP.Value}/{maxHP.Value}");

		currentHP.Value = 50f;
		currentHP.Value = 150f;

		maxHP.Modifiers.Add(Modifier.Plus(50f));
		Debug.Log($"After max HP +50: {currentHP.Value}/{maxHP.Value}");
	}

	void DemonstrateStatIntegration()
	{
		Debug.Log("\n🎮 INTEGRATION WITH UNIT STATS");

		var attack = unitStats.StatData.GetStatById("attack");
		attack.Modifiers.Add(Modifier.Plus(10f,0, "Weapon Bonus"));
		attack.Modifiers.Add(Modifier.Times(1.2f, 0,"Berserk"));

		var hp = unitStats.StatData.GetStatById("hp");
		var maxHP = unitStats.StatData.GetStatById("mp");
		maxHP.Modifiers.Add(Modifier.Plus(50f, 0,"Vitality Boost"));

		Debug.Log($"HP: {hp.CurrentValue}/{hp.MaxValue}");
		Debug.Log($"Attack: {attack.GetFinalValue()}");
	}
}
