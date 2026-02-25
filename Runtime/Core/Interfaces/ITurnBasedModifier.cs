using System;

namespace GameSystems.Stats
{
	public interface ITurnBasedModifier
	{
		int RemainingTurns { get; set; }
		int TotalTurns { get; }
		bool IsActiveOnTurn(int turn);
		void DecrementTurns();
		void ResetTurns();
	}
}
