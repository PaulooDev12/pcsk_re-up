using pcsk1.src.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pcsk1.src.Events
{
        public readonly struct AttributeChangedEvent<T> : IEvent
        {
        public string TargetId { get; }
        public string AttributeName { get; }
        public T Value { get; }
        public AttributeChangedEvent(string targetId, string attributeName, T value)
        {
            TargetId = targetId;
            AttributeName = attributeName;
            Value = value;
            }
        }
        public struct PlayerDeathEvent : IEvent
        {
            public bool EnemyNeedsToReset { get; set; }

            public PlayerDeathEvent(bool enemyNeedsToReset)
            {
                EnemyNeedsToReset = enemyNeedsToReset;
        }
        

    }
    public readonly struct TileInteractEvent<T, E> : IEvent
    {
        public Action<T> OnActive { get; }
        public E AffectedEntity { get; }

        public TileInteractEvent(Action<T> onActive, E affected)
        {
            OnActive = onActive;
            AffectedEntity = affected;
        }
    }
}
