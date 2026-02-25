using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GameSystems.Stats
{
	[Serializable]
	public class ModifiableValue<T> : IModifiableValue<T>
	{
		private T _initialValue;
		private T _cachedValue;
		private bool _isDirty = true;
		private readonly List<IModifier<T>> _modifiers;
		private readonly IOperator<T> _op;

		public T InitialValue
		{
			get => _initialValue;
			set
			{
				if (EqualityComparer<T>.Default.Equals(_initialValue, value)) return;
				_initialValue = value;
				MarkDirty();
				OnPropertyChanged(nameof(InitialValue));
			}
		}

		public T Value
		{
			get
			{
				if (_isDirty)
				{
					_cachedValue = CalculateValue();
					_isDirty = false;
				}
				return _cachedValue;
			}
		}

		public ICollection<IModifier<T>> Modifiers => _modifiers;

		public event PropertyChangedEventHandler PropertyChanged;

		public ModifiableValue(T initialValue)
		{
			_initialValue = initialValue;
			_cachedValue = initialValue;
			_modifiers = new List<IModifier<T>>();
			_op = Operator<T>.Instance;
		}

		public ModifiableValue(T initialValue, params IModifier<T>[] modifiers) : this(initialValue)
		{
			foreach (var modifier in modifiers)
			{
				AddModifier(modifier);
			}
		}

		public void AddModifier(IModifier<T> modifier)
		{
			if (modifier == null) return;

			modifier.PropertyChanged += OnModifierChanged;
			_modifiers.Add(modifier);
			MarkDirty();
		}

		public void RemoveModifier(IModifier<T> modifier)
		{
			if (modifier == null) return;

			modifier.PropertyChanged -= OnModifierChanged;
			_modifiers.Remove(modifier);
			MarkDirty();
		}

		public void ClearModifiers()
		{
			foreach (var modifier in _modifiers)
			{
				modifier.PropertyChanged -= OnModifierChanged;
			}
			_modifiers.Clear();
			MarkDirty();
		}

		private T CalculateValue()
		{
			T value = _initialValue;

			var sortedModifiers = _modifiers.OrderBy(m => m.Priority);
			foreach (var modifier in sortedModifiers)
			{
				if (modifier.Enabled)
				{
					value = modifier.Modify(value);
				}
			}

			return value;
		}

		private void OnModifierChanged(object sender, PropertyChangedEventArgs e)
		{
			MarkDirty();
		}

		private void MarkDirty()
		{
			_isDirty = true;
			OnPropertyChanged(nameof(Value));
		}

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
