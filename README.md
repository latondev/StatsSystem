# Stats System

A flexible and powerful stat management system for Unity, supporting modifiers, time-based calculations, and turn-based gameplay.

## 📋 Description

Stats System provides a complete solution for managing game stats like HP, MP, Attack, Defense with features for:

- **Flexible Modifiers**: Add, multiply, divide, replace values with priority ordering
- **Turn-based**: Support for turn-limited modifiers
- **Time-based**: Auto enable/disable modifiers based on time
- **Bounded Values**: Restrict values within min/max range
- **Computed Values**: Calculate values from other stats
- **Event-driven**: Real-time change notifications

## 🗂️ Folder Structure

```
Stats System/
├── Runtime/                    # Runtime code
│   ├── Core/
│   │   ├── Interfaces/         # Main interfaces
│   │   │   ├── IValue<T>              # Basic value interface
│   │   │   ├── IModifiableValue<T>    # Modifiable value interface
│   │   │   ├── IModifier<T>           # Modifier interface
│   │   │   └── ITurnBasedModifier     # Turn-based modifier interface
│   │   ├── Values/             # Implementations
│   │   │   ├── PropertyValue<T>       # Read-only mutable value
│   │   │   ├── ModifiableValue<T>     # Modifiable value with modifiers
│   │   │   └── BoundedValue<T>        # Min/max bounded value
│   │   └── Operators/          # Modifier implementations
│   │       ├── Operator<T>             # Basic operators (+, -, *, /)
│   │       ├── Modifier.cs             # Modifier factory
│   │       ├── TurnBasedModifier.cs    # Turn-based modifier
│   │       └── TurnTracker.cs          # Turn tracking system
│   ├── Extensions/           # Extensions
│   │   └── ValueExtensions.cs          # Computed values, time-based helpers
│   ├── Stat.cs               # Unity Stat class
│   ├── UnitStatController.cs # Controller for unit stats
│   └── UnitStatIteratorData.cs # Iterator for stats
├── Editor/                   # Editor scripts
│   └── UnitStatControllerEditor.cs
├── Tests/                    # Tests
│   ├── Editor/
│   └── Runtime/
└── Examples/                 # Examples
    ├── StatMasterExample.cs
    └── TurnBasedExample.cs
```

## 🚀 Quick Start

### Installation

Package is automatically installed in Unity via Package Manager.

### Basic Usage

```csharp
using GameSystems.Stats;

// Create simple stat
var attack = new ModifiableValue<float>(100f);
attack.PropertyChanged += (s, e) => Debug.Log($"Attack: {attack.Value}");

// Add modifiers
attack.Modifiers.Add(Modifier.Plus(10f, 0, "+10 base"));      // 100 + 10 = 110
attack.Modifiers.Add(Modifier.Times(1.5f, 0, "+50%"));       // 110 * 1.5 = 165

// Or use UnitStatController
var unitStats = GetComponent<UnitStatController>();
unitStats.UnitName = "Hero";

// Get stat
var hp = unitStats.StatData.GetStatById("hp");
Debug.Log($"HP: {hp.CurrentValue}/{hp.MaxValue}");

// Modify stat
hp.Add(20f);                    // +20 HP
hp.Subtract(10f);              // -10 HP
hp.RestoreToMax();             // Restore to max
```

## 📚 Features

### 1. Modifiers

Modifiers are applied in `Priority` order (lower number = first):

```csharp
var value = new ModifiableValue<float>(100f);

// Priority 100 applied after
value.Modifiers.Add(Modifier.Plus(50f, priority: 100, name: "Bonus"));

// Priority 0 applied before
value.Modifiers.Add(Modifier.Times(2f, priority: 0, name: "Double"));

// Result: (100 * 2) + 50 = 250
```

**Modifier Types:**

- `Modifier.Plus(value, priority, name)` - Add value
- `Modifier.Minus(value, priority, name)` - Subtract value
- `Modifier.Times(value, priority, name)` - Multiply value
- `Modifier.Divide(value, priority, name)` - Divide value
- `Modifier.Substitute(value, priority, name)` - Replace value
- `Modifier.Create(func, priority, name)` - Custom function

### 2. Turn-based Modifiers

For games with turn-based mechanics:

```csharp
var turnTracker = new TurnTracker();
var attack = new ModifiableValue<float>(20f);

// Create 3-turn modifier
var rageBuff = TurnBasedModifierFactory.Plus(15f, turns: 3, 
    priority: 100, name: "Rage Buff");
attack.Modifiers.Add(rageBuff as IModifier<float>);

// Activate tracker
rageBuff.DisableAfterTurns(turnTracker);

// Each turn
turnTracker.NextTurn();
// Turn 1: Attack = 35
// Turn 2: Attack = 35  
// Turn 3: Attack = 35
// Turn 4: Attack = 20 (buff expired)
```

### 3. Time-based Modifiers

Auto enable/disable after time duration:

```csharp
var defense = new ModifiableValue<float>(20f);

var shield = Modifier.Plus(30f, 0, "Shield Buff");
defense.Modifiers.Add(shield);

// Disable after 3 seconds
shield.DisableAfter(TimeSpan.FromSeconds(3f));

// Or enable after 1 second
var adrenaline = Modifier.Plus(20f, 0, "Adrenaline");
defense.Modifiers.Add(adrenaline);
adrenaline.EnableAfter(TimeSpan.FromSeconds(1f));
```

### 4. Bounded Values

Values always stay within min/max range:

```csharp
var maxHP = new ModifiableValue<float>(100f);
var currentHP = new BoundedValue<float>(0f, 100f, maxHP);

// Values are always clamped
currentHP.Value = 150f;  // → 100 (clamped to max)
currentHP.Value = -50f;   // → 0 (clamped to min)

// When maxHP changes
maxHP.Modifiers.Add(Modifier.Plus(50f));
// maxHP = 150, currentHP still bounded in [0, 150]
```

### 5. Computed Values

Calculate from other stats:

```csharp
var strength = new ModifiableValue<int>(10);
var agility = new ModifiableValue<int>(8);

// Sum
var totalStats = strength.Zip(agility, (s, a) => s + a);
Debug.Log(totalStats.Value);  // 18

// Type conversion
var hpAdjustment = strength.Select(s => (float)s * 10);
Debug.Log(hpAdjustment.Value);  // 100f

// Create modifier from computed value
var maxHP = new ModifiableValue<float>(100f);
maxHP.Modifiers.Add(Modifier.Create(hpAdjustment, 0));
Debug.Log(maxHP.Value);  // 200f
```

### 6. Unity Integration

**Stat Class:**

```csharp
// Create stat
var hp = new Stat(
    id: "hp",
    name: "Health", 
    type: StatType.Health,
    baseValue: 100f,
    maxValue: 100f,
    canRegen: true,
    regenRate: 1f
);

// Access properties
hp.CurrentValue;     // Current value
hp.MaxValue;         // Max value
hp.GetPercentage();  // Percentage (0-1)
hp.IsDepleted();     // Check if depleted

// Modify
hp.Add(10f);
hp.Subtract(5f);
hp.RestoreToMax();
hp.LevelUp(10f, 10f);  // Increase base and max on level up
```

**UnitStatController:**

```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private UnitStatController unitStats;
    
    void Start()
    {
        unitStats.UnitName = "Player";
        
        // Add modifier
        unitStats.AddModifier("attack", Modifier.Plus(10f, 0, "Weapon"));
        unitStats.AddMaxModifier("hp", Modifier.Plus(50f, 0, "Vitality"));
        
        // Listen to events
        unitStats.OnStatDepleted += OnStatDepleted;
        unitStats.OnLevelUp += OnLevelUp;
    }
    
    void TakeDamage(float damage)
    {
        var hp = unitStats.StatData.GetStatById("hp");
        hp.Subtract(damage);
    }
    
    void OnStatDepleted(Stat stat)
    {
        if (stat.StatType == StatType.Health)
        {
            Debug.Log("Player died!");
        }
    }
}
```

**StatType Enum:**

```csharp
public enum StatType
{
    Health,           // ❤️ HP
    Mana,             // 💙 MP
    Stamina,          // 💚 Stamina
    Attack,           // ⚔️ Attack damage
    Defense,          // 🛡️ Defense
    Speed,            // ⚡ Speed
    CriticalRate,     // 🎯 Crit rate
    CriticalDamage,   // 💥 Crit damage
    Accuracy,         // 🔍 Accuracy
    Evasion           // 💨 Evasion
}
```

## 🎮 Examples

### StatMasterExample.cs

```csharp
[SerializeField] private UnitStatController unitStats;

void Start()
{
    // Demo modifiers
    var attack = new ModifiableValue<float>(100f);
    attack.Modifiers.Add(Modifier.Plus(10f));
    attack.Modifiers.Add(Modifier.Times(1.5f));
    Debug.Log(attack.Value);  // 165
    
    // Demo computed values
    var strength = new ModifiableValue<int>(10);
    var hpFromStrength = strength.Select(s => s * 10);
    Debug.Log(hpFromStrength.Value);  // 100
    
    // Demo bounded value
    var maxHP = new ModifiableValue<float>(100f);
    var currentHP = new BoundedValue<float>(0f, 100f, maxHP);
    currentHP.Value = 150f;  // → 100
}
```

### TurnBasedExample.cs

```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.Space))
    {
        turnTracker.NextTurn();
        Debug.Log($"Turn {turnTracker.CurrentTurn}");
        Debug.Log($"Attack: {attack.Value}");
    }
}
```

## 🔧 API Reference

### Interfaces

#### `IValue<T>`
```csharp
public interface IValue<T> : INotifyPropertyChanged
{
    T Value { get; }
}
```

#### `IModifiableValue<T>`
```csharp
public interface IModifiableValue<T> : IReadOnlyValue<T>
{
    T InitialValue { get; set; }
    new T Value { get; }
    ICollection<IModifier<T>> Modifiers { get; }
}
```

#### `IModifier<T>`
```csharp
public interface IModifier<T> : INotifyPropertyChanged
{
    bool Enabled { get; set; }
    int Priority { get; set; }
    string Name { get; }
    T Modify(T given);
}
```

### Classes

#### `ModifiableValue<T>`
```csharp
var value = new ModifiableValue<float>(100f);
value.AddModifier(modifier);
value.RemoveModifier(modifier);
value.ClearModifiers();
```

#### `BoundedValue<T>`
```csharp
var max = new ModifiableValue<float>(100f);
var current = new BoundedValue<float>(0f, 100f, max);
```

#### `TurnTracker`
```csharp
var tracker = new TurnTracker();
tracker.NextTurn();
tracker.OnTurnStart += (turn) => { /* ... */ };
tracker.OnTurnEnd += (turn) => { /* ... */ };
```

### Extensions

```csharp
// Enable/Disable after time
modifier.DisableAfter(TimeSpan.FromSeconds(3f));
modifier.EnableAfter(TimeSpan.FromSeconds(1f));

// Enable/Disable after turns
modifier.DisableAfterTurns(3, turnTracker);
modifier.EnableAfterTurns(2, turnTracker);

// Computed values
var computed = source.Select(x => x * 2);
var zipped = first.Zip(second, (a, b) => a + b);
```

## 📦 Dependencies

- Unity 6000.0+
- .NET Framework 4.7.1
- DesignPatterns.Iterator (Iterator pattern implementation)

## 🧪 Tests

Tests are located in `Tests/` folder:

- **Editor Tests**: `Tests/Editor/Stats System.Tests.Editor.asmdef`
- **Runtime Tests**: `Tests/Runtime/Stats System.Tests.Runtime.asmdef`

Run tests via Unity Test Runner.

## 📝 Notes

- System uses `PropertyChanged` event for change notifications
- Modifiers are sorted by `Priority` before applying
- Values are cached and only recalculated when needed (dirty flag)
- Supports generic types: `int`, `float`, `double`

## 👨‍💻 Author

**latondev**

## 📄 License

Package: com.latondev.stats-system
Version: 1.0.0
