using System;
using System.Collections.Generic;
using System.Linq;
using DesignPatterns.Iterator;

namespace GameSystems.Stats
{
    /// <summary>
    /// Specialized iterator data for Unit Stats
    /// </summary>
    [Serializable]
    public class UnitStatIteratorData : IteratorData<Stat>
    {
        /// <summary>
        /// Gets next stat of specific type
        /// </summary>
        public Stat NextOfType(StatType type)
        {
            if (CurrentIterator == null) return null;

            int startIndex = CurrentIterator.CurrentIndex;
            
            while (HasNext())
            {
                Stat stat = Next();
                if (stat != null && stat.StatType == type)
                {
                    return stat;
                }
                
                if (CurrentIterator.CurrentIndex == startIndex)
                    break;
            }

            return null;
        }

        /// <summary>
        /// Gets next depleted stat
        /// </summary>
        public Stat NextDepleted()
        {
            if (CurrentIterator == null) return null;

            int startIndex = CurrentIterator.CurrentIndex;
            
            while (HasNext())
            {
                Stat stat = Next();
                if (stat != null && stat.IsDepleted())
                {
                    return stat;
                }
                
                if (CurrentIterator.CurrentIndex == startIndex)
                    break;
            }

            return null;
        }

        /// <summary>
        /// Gets next stat that can regenerate
        /// </summary>
        public Stat NextRegenerable()
        {
            if (CurrentIterator == null) return null;

            int startIndex = CurrentIterator.CurrentIndex;
            
            while (HasNext())
            {
                Stat stat = Next();
                if (stat != null && stat.CanRegenerate && !stat.IsAtMax())
                {
                    return stat;
                }
                
                if (CurrentIterator.CurrentIndex == startIndex)
                    break;
            }

            return null;
        }

        /// <summary>
        /// Gets stats by type
        /// </summary>
        public List<Stat> GetStatsByType(StatType type)
        {
            return Collection.Where(stat => stat.StatType == type).ToList();
        }

        /// <summary>
        /// Gets all vital stats (HP, MP, Stamina)
        /// </summary>
        public List<Stat> GetVitalStats()
        {
            return Collection.Where(stat => 
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
            return Collection.Where(stat => 
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
            return Collection.FirstOrDefault(stat => stat.StatId == statId);
        }

        /// <summary>
        /// Sorts stats by type
        /// </summary>
        public void SortByType()
        {
            Collection.Sort((a, b) => a.StatType.CompareTo(b.StatType));
            Initialize();
        }

        /// <summary>
        /// Sorts stats by current value
        /// </summary>
        public void SortByValue()
        {
            Collection.Sort((a, b) => b.CurrentValue.CompareTo(a.CurrentValue));
            Initialize();
        }

        private bool HasNext()
        {
            return CurrentIterator != null && CurrentIterator.HasNext();
        }
    }
}
