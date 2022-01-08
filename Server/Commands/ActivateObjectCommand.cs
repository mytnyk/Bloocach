using Server.Events;
using Server.Model;

namespace Server.Commands
{
    public sealed class ActivateObjectCommand : ICommand
    {
        public int ObjectId { get; set; }
        public World World { get; set; }
        public IEvent CreateEvent()
        {
            return new UpdateObjectEvent(ObjectId, World);
        }
    }
    /*
    public sealed class CreateObjectCommand : ICommand
    {
        public World World { get; set; }
        public IEvent CreateEvent()
        {
            throw new System.NotImplementedException();
        }
    }*/
}