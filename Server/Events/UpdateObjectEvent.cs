using System.Threading.Tasks;
using Server.Model;

namespace Server.Events
{
    public sealed class UpdateObjectEvent : IEvent
    {
        private readonly int _objectId;
        private readonly World _world;

        public UpdateObjectEvent(int objectId, World world)
        {
            _objectId = objectId;
            _world = world;
        }
        public async Task ProcessAsync(IEventContainer eventContainer)
        {
            if (await _world.UpdateAsync(_objectId))
            {
                eventContainer.AddEvent(this, 5);
            }
        }
    }
}