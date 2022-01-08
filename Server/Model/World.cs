using System;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Model
{
    /// <summary>
    /// MUST BE THREAD-SAFE.
    /// </summary>
    public sealed class World
    {
        public event EventHandler<ObjectState> OnChanged;

        public IObject[] Objects { get; } = new IObject[100000];
        private int _numberOfObjects = 0;
        /// <summary>
        /// Returns whether to continue updates or not..
        /// the change of one object can lead to changes in another objects?
        /// </summary>
        public Task<bool> UpdateAsync(int objectId)
        {
            var obj = Objects[objectId];
            if (obj == null)
            {
                throw new Exception($"Object {objectId} does not exist!");
            }
            var retval = obj.UpdateAsync();
            OnChanged?.Invoke(this, obj.ToObjectState());
            return retval;
        }

        public int CreateCircleObject()
        {
            var objectId = Interlocked.Increment(ref _numberOfObjects) - 1;
            Objects[objectId] = new CircleObject(objectId);
            return objectId;
        }
    }
}