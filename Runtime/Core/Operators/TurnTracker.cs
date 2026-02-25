using System;
using System.Collections.Generic;

namespace GameSystems.Stats
{
	public class TurnTracker
	{
		private int _currentTurn;
		private readonly List<Action<int>> _onTurnStartCallbacks;
		private readonly List<Action<int>> _onTurnEndCallbacks;

		public int CurrentTurn => _currentTurn;

		public event Action<int> OnTurnStart;
		public event Action<int> OnTurnEnd;

		public TurnTracker()
		{
			_currentTurn = 0;
			_onTurnStartCallbacks = new List<Action<int>>();
			_onTurnEndCallbacks = new List<Action<int>>();
		}

		public void NextTurn()
		{
			_currentTurn++;

			OnTurnStart?.Invoke(_currentTurn);

			foreach (var callback in _onTurnStartCallbacks)
			{
				callback?.Invoke(_currentTurn);
			}

			OnTurnEnd?.Invoke(_currentTurn);

			foreach (var callback in _onTurnEndCallbacks)
			{
				callback?.Invoke(_currentTurn);
			}
		}

		public void Reset()
		{
			_currentTurn = 0;
		}

		public void RegisterTurnStartCallback(Action<int> callback)
		{
			if (callback != null)
			{
				_onTurnStartCallbacks.Add(callback);
			}
		}

		public void RegisterTurnEndCallback(Action<int> callback)
		{
			if (callback != null)
			{
				_onTurnEndCallbacks.Add(callback);
			}
		}

		public void ClearCallbacks()
		{
			_onTurnStartCallbacks.Clear();
			_onTurnEndCallbacks.Clear();
		}
	}
}
