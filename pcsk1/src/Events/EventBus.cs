#nullable enable
using System;

namespace pcsk1.src.Events
{
    public interface IEvent
    {
    }
    public static class EventBus<T> where T : IEvent
    {
        private static Action<T>? _subscribers;
        public static void Subscribe(Action<T> handler) => _subscribers += handler;
        public static void Unsubscribe(Action<T> handler) => _subscribers -= handler;
        public static void Publish(T @event) => _subscribers?.Invoke(@event);
    }
}
