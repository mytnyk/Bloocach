using Server.Events;

namespace Server.Commands
{
    public interface ICommand
    {
        IEvent CreateEvent();
    }
}