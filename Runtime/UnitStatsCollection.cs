using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameSystems.Stats
{
    /// <summary>
    /// Standalone collection for Unit Stats - No external IteratorData dependency
    /// </summary>
    [Serializable]
    public class UnitStatsCollection
    {
        [SerializeField] private List<Stat> stats = new List<Stat>();
        [SerializeField] private int currentIndex = -1;
        [SerializeField] private int totalIterations = 0;

        // Iterator state
        private int iteratorIndex = 0;

        public List<Stat> Stats => stats;
        public int CurrentIndex => currentIndex;
        public int Count => stats.Count;
        public int TotalIterations => totalIterations;

        public Stat CurrentStat => currentIndex >= 0 && currentIndex < stats.Count ? stats[currentIndex] : null;

        #region Collection Operations

        public void Add(Stat stat)
        {
            stats.Add(stat);
        }

        public void AddItem(Stat stat)
        {
            Add(stat);
        }

        public void Remove(Stat stat)
        {
            stats.Remove(stat);
        }

        public void Clear()
        {
            stats.Clear();
            ResetIterator();
        }

        public bool Contains(Stat stat)
        {
            return stats.Contains(stat);
        }

        #endregion

        #region Iterator Operations

        public void Initialize()
        {
            ResetIterator();
        }

        public void ResetIterator()
        {
            currentIndex = -1;
            iteratorIndex = 0;
        }

        public Stat First()
        {
            if (stats.Count == 0) return null;
            currentIndex = 0;
            iteratorIndex = 0;
            totalIterations++;
            return stats[currentIndex];
        }

        public Stat Next()
        {
            if (stats.Count == 0) return null;

            iteratorIndex = (iteratorIndex + 1) % stats.Count;
            currentIndex = iteratorIndex;
            totalIterations++;

            // Stop after one full cycle
            if (iteratorIndex == 0)
                return null;

            return stats[currentIndex];
        }

        public Stat Previous()
        {
            if (stats.Count == 0) return null;

            if (iteratorIndex == 0)
                iteratorIndex = stats.Count;

            iteratorIndex--;
            currentIndex = iteratorIndex;
            totalIterations++;

            return stats[currentIndex];
        }

        public Stat Last()
        {
            if (stats.Count == 0) return null;
            currentIndex = stats.Count - 1;
            iteratorIndex = currentIndex;
            totalIterations++;
            return stats[currentIndex];
        }

        public Stat GetAt(int index)
        {
            if (index < 0 || index >= stats.Count) return null;
            return stats[index];
        }

        public int GetCurrentIteratorIndex()
        {
            return currentIndex;
        }

        public int GetCurrentIndex() => currentIndex;

        public int GetTotalIterations() => totalIterations;

        #endregion

        #region Query Methods

        /// <summary>
        /// Gets next stat of specific type
        /// </summary>
        public Stat NextOfType(StatType type)
        {
            if (stats.Count == 0) return null;

            int startIndex = iteratorIndex;

            do
            {
                Stat stat = Next();
                if (stat != null && stat.StatType == type)
                {
                    return stat;
                }

                if (iteratorIndex == startIndex)
                    break;

            } while (iteratorIndex != startIndex);

            return null;
        }

        /// <summary>
        /// Gets next depleted stat
        /// </summary>
        public Stat NextDepleted()
        {
            if (stats.Count == 0) return null;

            int startIndex = iteratorIndex;

            do
            {
                Stat stat = Next();
                if (stat != null && stat.IsDepleted())
                {
                    return stat;
                }

                if (iteratorIndex == startIndex)
                    break;

            } while (iteratorIndex != startIndex);

            return null;
        }

        /// <summary>
        /// Gets next stat that can regenerate
        /// </summary>
        public Stat NextRegenerable()
        {
            if (stats.Count == 0) return null;

            int startIndex = iteratorIndex;

            do
            {
                Stat stat = Next();
                if (stat != null && stat.CanRegenerate && !stat.IsAtMax())
                {
                    return stat;
                }

                if (iteratorIndex == startIndex)
                    break;

            } while (iteratorIndex != startIndex);

            return null;
        }

        /// <summary>
        /// Gets stats by type
        /// </summary>
        public List<Stat> GetStatsByType(StatType type)
        {
            return stats.Where(stat => stat.StatType == type).ToList();
        }

        /// <summary>
        /// Gets all vital stats (HP, MP, Stamina)
        /// </summary>
        public List<Stat> GetVitalStats()
        {
            return stats.Where(stat =>
                stat.StatType == StatType.Health ||
                stat.StatType == StatType.Mana ||
                stat.StatType == StatType.Stamina
            ).ToList();
        }

        /// <summary>
        /// Gets all combat stats
        /// </summary>
        public List<Stat> GetCombatStats()
        {
            return stats.Where(stat =>
                stat.StatType == StatType.Attack ||
                stat.StatType == StatType.Defense ||
                stat.StatType == StatType.Speed ||
                stat.StatType == StatType.CriticalRate ||
                stat.StatType == StatType.CriticalDamage
            ).ToList();
        }

        /// <summary>
        /// Gets stat by ID
        /// </summary>
        public Stat GetStatById(string statId)
        {
            return stats.FirstOrDefault(stat => stat.StatId == statId);
        }

        #endregion

        #region Sorting

        /// <summary>
        /// Sorts stats by type
        /// </summary>
        public void SortByType()
        {
            stats.Sort((a, b) => a.StatType.CompareTo(b.StatType));
            Initialize();
        }

        /// <summary>
        /// Sorts stats by current value
        /// </summary>
        public void SortByValue()
        {
            stats.Sort((a, b) => b.CurrentValue.CompareTo(a.CurrentValue));
            Initialize();
        }

        #endregion
    }
}
