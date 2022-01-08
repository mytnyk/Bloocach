namespace Server.Events
{
    public interface IEventContainer
    {
        /// <summary>
        /// Add a continuation event.
        /// </summary>
        /// <param name="event">Event in future.</param>
        /// <param name="future">Must be more than zero (when called from process method).</param>
        void AddEvent(IEvent @event, long future);
    }
}