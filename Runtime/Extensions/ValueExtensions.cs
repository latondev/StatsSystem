using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSystems.Stats
{
	public static class ValueExtensions
	{
		public static IReadOnlyValue<TResult> Select<TSource, TResult>(this IValue<TSource> source, Func<TSource, TResult> selector)
		{
			return new ComputedValue<TSource, TResult>(source, selector);
		}

		public static IReadOnlyValue<TResult> Zip<TFirst, TSecond, TResult>(
			this IValue<TFirst> first,
			IValue<TSecond> second,
			Func<TFirst, TSecond, TResult> resultSelector)
		{
			return new ZippedValue<TFirst, TSecond, TResult>(first, second, resultSelector);
		}

		public static void EnableAfter<T>(this IModifier<T> modifier, TimeSpan delay)
		{
			modifier.Enabled = false;
			var timer = new System.Timers.Timer(delay.TotalMilliseconds);
			timer.AutoReset = false;
			timer.Elapsed += (s, e) =>
			{
				modifier.Enabled = true;
				timer.Dispose();
			};
			timer.Start();
		}

		public static void DisableAfter<T>(this IModifier<T> modifier, TimeSpan delay)
		{
			var timer = new System.Timers.Timer(delay.TotalMilliseconds);
			timer.AutoReset = false;
			timer.Elapsed += (s, e) =>
			{
				modifier.Enabled = false;
				timer.Dispose();
			};
			timer.Start();
		}

		public static void DisableAfterTurns<T>(this IModifier<T> modifier, int turns, TurnTracker turnTracker)
		{
			if (turnTracker == null || turns <= 0) return;

			int startTurn = turnTracker.CurrentTurn;

			turnTracker.RegisterTurnEndCallback((currentTurn) =>
			{
				if (currentTurn - startTurn >= turns)
				{
					modifier.Enabled = false;
				}
			});
		}

		public static void EnableAfterTurns<T>(this IModifier<T> modifier, int turns, TurnTracker turnTracker)
		{
			if (turnTracker == null || turns <= 0) return;

			modifier.Enabled = false;
			int startTurn = turnTracker.CurrentTurn;

			turnTracker.RegisterTurnEndCallback((currentTurn) =>
			{
				if (currentTurn - startTurn >= turns)
				{
					modifier.Enabled = true;
				}
			});
		}

		public static void DisableAfterTurns(this ITurnBasedModifier modifier, TurnTracker turnTracker)
		{
			if (turnTracker == null) return;

			turnTracker.RegisterTurnEndCallback((currentTurn) =>
			{
				modifier.DecrementTurns();

				if (modifier.RemainingTurns <= 0)
				{
					if (modifier is IModifier<float> mod)
					{
						mod.Enabled = false;
					}
					else if (modifier is IModifier<int> modInt)
					{
						modInt.Enabled = false;
					}
					else if (modifier is IModifier<double> modDouble)
					{
						modDouble.Enabled = false;
					}
				}
			});
		}

		public static void EnableForTurns(this ITurnBasedModifier modifier, int totalTurns, TurnTracker turnTracker)
		{
			if (turnTracker == null || totalTurns <= 0) return;

			modifier.ResetTurns();
			modifier.RemainingTurns = totalTurns;

			if (modifier is IModifier<float> mod)
			{
				mod.Enabled = true;
			}
			else if (modifier is IModifier<int> modInt)
			{
				modInt.Enabled = true;
			}
			else if (modifier is IModifier<double> modDouble)
			{
				modDouble.Enabled = true;
			}

			DisableAfterTurns(modifier, turnTracker);
		}
	}

	internal class ComputedValue<TSource, TResult> : IReadOnlyValue<TResult>
	{
		private readonly IValue<TSource> _source;
		private readonly Func<TSource, TResult> _selector;
		private TResult _cachedValue;

		public TResult Value
		{
			get
			{
				_cachedValue = _selector(_source.Value);
				return _cachedValue;
			}
		}

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		public ComputedValue(IValue<TSource> source, Func<TSource, TResult> selector)
		{
			_source = source;
			_selector = selector;

			if (_source is System.ComponentModel.INotifyPropertyChanged notify)
			{
				notify.PropertyChanged += (s, e) =>
				{
					_cachedValue = _selector(_source.Value);
					PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Value)));
				};
			}
		}
	}

	internal class ZippedValue<TFirst, TSecond, TResult> : IReadOnlyValue<TResult>
	{
		private readonly IValue<TFirst> _first;
		private readonly IValue<TSecond> _second;
		private readonly Func<TFirst, TSecond, TResult> _selector;
		private TResult _cachedValue;

		public TResult Value
		{
			get
			{
				_cachedValue = _selector(_first.Value, _second.Value);
				return _cachedValue;
			}
		}

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		public ZippedValue(IValue<TFirst> first, IValue<TSecond> second, Func<TFirst, TSecond, TResult> selector)
		{
			_first = first;
			_second = second;
			_selector = selector;

			var notify = _first as System.ComponentModel.INotifyPropertyChanged;
			if (notify != null)
			{
				notify.PropertyChanged += (s, e) =>
				{
					_cachedValue = _selector(_first.Value, _second.Value);
					PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Value)));
				};
			}

			var notify2 = _second as System.ComponentModel.INotifyPropertyChanged;
			if (notify2 != null)
			{
				notify2.PropertyChanged += (s, e) =>
				{
					_cachedValue = _selector(_first.Value, _second.Value);
					PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Value)));
				};
			}
		}
	}
}
