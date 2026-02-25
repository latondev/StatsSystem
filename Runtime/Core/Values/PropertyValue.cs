using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace GameSystems.Stats
{
	[Serializable]
	public class PropertyValue<T> : IReadOnlyValue<T>
	{
		private T _value;

		public T Value
		{
			get => _value;
			set
			{
				if (EqualityComparer<T>.Default.Equals(_value, value)) return;
				_value = value;
				OnPropertyChanged(nameof(Value));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public PropertyValue(T value)
		{
			_value = value;
		}

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
