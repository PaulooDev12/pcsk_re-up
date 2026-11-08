#nullable enable


using Microsoft.Xna.Framework;
using pcsk1.src.Entities;
using pcsk1.src.Events;
using System;

namespace pcsk1.src.World
{
    public class Tile<T> where T : struct, IEvent
    {
        public ushort Id { get; set; }
        public bool IsSolid { get; set; }
        public Rectangle SourceRectangle { get; set; }
        public bool IsInteractable { get; set; }
        public bool HasBeenInteracted { get; private set; } = false;

        private IEntity _target;
        public Action<T> OnTileInteract { get; init; }
        public Point Position { get; set; }

        public Tile(ushort id, bool isSolid, Rectangle sourceRect, Point position, bool isInteractable = false)
        {
            Id = id;
            IsSolid = isSolid;
            SourceRectangle = sourceRect;
            Position = position;
            IsInteractable = isInteractable;
        }

        public void Interact(IEntity interactor)
        {
            if (!IsInteractable) return;
            _target = interactor;
            EventBus<TileInteractEvent<T, IEntity>>.Publish(new TileInteractEvent<T, IEntity>(OnTileInteract, _target));

        }
    }
}
