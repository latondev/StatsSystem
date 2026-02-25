using System;
using System.ComponentModel;

namespace GameSystems.Stats
{
	public interface IModifier<T> : INotifyPropertyChanged
	{
		bool Enabled { get; set; }
		int Priority { get; set; }
		string Name { get; }
		T Modify(T given);
	}
}
