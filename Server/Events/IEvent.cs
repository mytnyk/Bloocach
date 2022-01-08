using System.Threading.Tasks;

namespace Server.Events
{
    public interface IEvent
    {
        /// <summary>
        /// It must be fast processing. For the long operations run sub-tasks.
        /// Called from Tape loop.
        /// </summary>
        /// <param name="eventContainer"> Event can add other events</param>
        Task ProcessAsync(IEventContainer eventContainer);
    }
}