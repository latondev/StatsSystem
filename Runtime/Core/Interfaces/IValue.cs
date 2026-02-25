using System.Collections.Generic;
using System.ComponentModel;

namespace GameSystems.Stats
{
	public interface IValue<T> : INotifyPropertyChanged
	{
		T Value { get; }
	}

	public interface IReadOnlyValue<T> : IValue<T>
	{
	}
}
