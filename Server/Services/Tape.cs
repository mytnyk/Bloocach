using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Events;

namespace Server.Services
{
    /// <summary>
    /// NOT THREAD SAFE. Accessible only from Tape loop.
    /// </summary>
    public sealed class Tape : IEventContainer
    {
        private readonly struct Frame
        {
            public readonly IList<IEvent> Events { get; init; }
        }
        private readonly long _tapeLength;
        private long _head;
        private readonly Frame[] _frames;
        public Tape(long tapeLength)
        {
            _tapeLength = tapeLength;
            _frames = new Frame[tapeLength];
            for (int i = 0; i < tapeLength; i++)
            {
                _frames[i] = new Frame() {Events = new List<IEvent>()};
            }
        }

        public void AddEvent(IEvent @event, long future)
        {
            if (future < 0)
            {
                throw new Exception("Time exception. Cannot get back into the future.");
            }
            if (future > _tapeLength - 1)
            {
                throw new Exception("Time exception. Cannot add event in future more than the length of the tape.");
            }

            var frameIndex = (_head + future) % _tapeLength;
            _frames[frameIndex].Events.Add(@event);
        }
        public async Task ProcessFrameAsync()
        {
            var frameEvents = _frames[_head].Events;
            foreach (var frameEvent in frameEvents)
            {
                await frameEvent.ProcessAsync(this);
            }
            frameEvents.Clear(); // dispose processed frame

            _head = (_head + 1) % _tapeLength;
        }
    }
}