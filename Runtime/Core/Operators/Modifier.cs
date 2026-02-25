using System;
using System.ComponentModel;

namespace GameSystems.Stats
{
	public static class Modifier
	{
		public static IModifier<T> Plus<T>(T value, int priority = 0, string name = null)
		{
			return new AddModifier<T>(value, name, priority);
		}

		public static IModifier<T> Minus<T>(T value, int priority = 0, string name = null)
		{
			return new SubtractModifier<T>(value, name, priority);
		}

		public static IModifier<T> Times<T>(T value, int priority = 0, string name = null)
		{
			return new MultiplyModifier<T>(value, name, priority);
		}

		public static IModifier<T> Divide<T>(T value, int priority = 0, string name = null)
		{
			return new DivideModifier<T>(value, name, priority);
		}

		public static IModifier<T> Substitute<T>(T value, int priority = 0, string name = null)
		{
			return new SubstituteModifier<T>(value, name, priority);
		}

		public static IModifier<T> Create<T>(Func<T, T> modifyFunc, int priority = 0, string name = null)
		{
			return new FuncModifier<T>(modifyFunc, name, priority);
		}

		public static IModifier<T> Create<T>(IReadOnlyValue<T> value, int priority = 0, string name = null)
		{
			return new ValueModifier<T>(value, name, priority);
		}
	}

	internal class AddModifier<T> : IModifier<T>
	{
		private readonly T _value;
		private readonly IOperator<T> _op;

		public bool Enabled { get; set; } = true;
		public int Priority { get; set; }
		public string Name { get; }

		public event PropertyChangedEventHandler PropertyChanged;

		public AddModifier(T value, string name, int priority)
		{
			_value = value;
			Name = name ?? $"+{_value}";
			Priority = priority;
			_op = Operator<T>.Instance;
		}

		public T Modify(T given)
		{
			if (!Enabled) return given;
			return _op.Add(given, _value);
		}
	}

	internal class SubtractModifier<T> : IModifier<T>
	{
		private readonly T _value;
		private readonly IOperator<T> _op;

		public bool Enabled { get; set; } = true;
		public int Priority { get; set; }
		public string Name { get; }

		public event PropertyChangedEventHandler PropertyChanged;

		public SubtractModifier(T value, string name, int priority)
		{
			_value = value;
			Name = name ?? $"-{_value}";
			Priority = priority;
			_op = Operator<T>.Instance;
		}

		public T Modify(T given)
		{
			if (!Enabled) return given;
			return _op.Subtract(given, _value);
		}
	}

	internal class MultiplyModifier<T> : IModifier<T>
	{
		private readonly T _value;
		private readonly IOperator<T> _op;

		public bool Enabled { get; set; } = true;
		public int Priority { get; set; }
		public string Name { get; }

		public event PropertyChangedEventHandler PropertyChanged;

		public MultiplyModifier(T value, string name, int priority)
		{
			_value = value;
			Name = name ?? $"×{_value}";
			Priority = priority;
			_op = Operator<T>.Instance;
		}

		public T Modify(T given)
		{
			if (!Enabled) return given;
			return _op.Multiply(given, _value);
		}
	}

	internal class DivideModifier<T> : IModifier<T>
	{
		private readonly T _value;
		private readonly IOperator<T> _op;

		public bool Enabled { get; set; } = true;
		public int Priority { get; set; }
		public string Name { get; }

		public event PropertyChangedEventHandler PropertyChanged;

		public DivideModifier(T value, string name, int priority)
		{
			_value = value;
			Name = name ?? $"/{_value}";
			Priority = priority;
			_op = Operator<T>.Instance;
		}

		public T Modify(T given)
		{
			if (!Enabled) return given;
			return _op.Divide(given, _value);
		}
	}

	internal class SubstituteModifier<T> : IModifier<T>
	{
		private readonly T _value;

		public bool Enabled { get; set; } = true;
		public int Priority { get; set; }
		public string Name { get; }

		public event PropertyChangedEventHandler PropertyChanged;

		public SubstituteModifier(T value, string name, int priority)
		{
			_value = value;
			Name = name ?? $"={_value}";
			Priority = priority;
		}

		public T Modify(T given)
		{
			if (!Enabled) return given;
			return _value;
		}
	}

	internal class FuncModifier<T> : IModifier<T>
	{
		private readonly Func<T, T> _func;

		public bool Enabled { get; set; } = true;
		public int Priority { get; set; }
		public string Name { get; }

		public event PropertyChangedEventHandler PropertyChanged;

		public FuncModifier(Func<T, T> func, string name, int priority)
		{
			_func = func;
			Name = name ?? "Custom";
			Priority = priority;
		}

		public T Modify(T given)
		{
			if (!Enabled) return given;
			return _func(given);
		}
	}

	internal class ValueModifier<T> : IModifier<T>
	{
		private readonly IReadOnlyValue<T> _value;

		public bool Enabled { get; set; } = true;
		public int Priority { get; set; }
		public string Name { get; }

		public event PropertyChangedEventHandler PropertyChanged;

		public ValueModifier(IReadOnlyValue<T> value, string name, int priority)
		{
			_value = value;
			Name = name ?? $"Value({_value.Value})";
			Priority = priority;

			if (_value is INotifyPropertyChanged notify)
			{
				notify.PropertyChanged += (s, e) => PropertyChanged?.Invoke(this, e);
			}
		}

		public T Modify(T given)
		{
			if (!Enabled) return given;
			return _value.Value;
		}
	}
}
