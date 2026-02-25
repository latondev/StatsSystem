using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace GameSystems.Stats
{
	[Serializable]
	public class BoundedValue<T> : ModifiableValue<T>
	{
		private readonly IReadOnlyValue<T> _minValue;
		private readonly IReadOnlyValue<T> _maxValue;
		private readonly IOperator<T> _op;

		public BoundedValue(T initialValue, T minValue, IReadOnlyValue<T> maxValue) : base(initialValue)
		{
			_minValue = new PropertyValue<T>(minValue);
			_maxValue = maxValue;
			_op = Operator<T>.Instance;

			if (_maxValue is INotifyPropertyChanged notify)
			{
				notify.PropertyChanged += (s, e) => OnPropertyChanged(nameof(Value));
			}

			EnsureBounds();
		}

		public BoundedValue(IReadOnlyValue<T> minValue, T initialValue, IReadOnlyValue<T> maxValue) : base(initialValue)
		{
			_minValue = minValue;
			_maxValue = maxValue;
			_op = Operator<T>.Instance;

			if (_minValue is INotifyPropertyChanged minNotify)
			{
				minNotify.PropertyChanged += (s, e) => OnPropertyChanged(nameof(Value));
			}

			if (_maxValue is INotifyPropertyChanged maxNotify)
			{
				maxNotify.PropertyChanged += (s, e) => OnPropertyChanged(nameof(Value));
			}

			EnsureBounds();
		}

		public new T Value
		{
			get => base.Value;
			set
			{
				InitialValue = _op.Clamp(value, _minValue.Value, _maxValue.Value);
			}
		}

		private void EnsureBounds()
		{
			T clamped = _op.Clamp(InitialValue, _minValue.Value, _maxValue.Value);
			if (!EqualityComparer<T>.Default.Equals(InitialValue, clamped))
			{
				InitialValue = clamped;
			}
		}
	}

	internal static class OperatorExtensions
	{
		public static T Clamp<T>(this IOperator<T> op, T value, T min, T max)
		{
			if (Compare(value, min) < 0)
				return min;
			if (Compare(value, max) > 0)
				return max;
			return value;
		}

		public static int Compare<T>(T a, T b)
		{
			var comparer = System.Collections.Generic.Comparer<T>.Default;
			return comparer.Compare(a, b);
		}
	}
}
