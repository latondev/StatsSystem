using System.Collections.Generic;

namespace GameSystems.Stats
{
	public interface IModifiableValue<T> : IReadOnlyValue<T>
	{
		T InitialValue { get; set; }
		new T Value { get; }
		ICollection<IModifier<T>> Modifiers { get; }
	}
}
