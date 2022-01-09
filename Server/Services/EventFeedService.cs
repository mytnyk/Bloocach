using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Server.Services
{
    public sealed class EventFeedService : BackgroundService
    {
        private readonly CommandService _commandService;
        private readonly ILogger<EventFeedService> _logger;
        private const long FramesPerSecond = 50; // 50 frames per second
        private const long MillisecondsPerFrame = 1000 / FramesPerSecond; // 20 ms per frame

        private readonly Stopwatch _stopwatch = new ();
        private long _processedFrames = 0;
        private long _processedMilliseconds = 0;

        public EventFeedService(CommandService commandService, ILogger<EventFeedService> logger)
        {
            _commandService = commandService;
            _logger = logger;
        }
        /// <summary>
        /// The reason why we keep it here as method is to avoid closure and many lambda allocations
        /// </summary>
        private bool SpinUntilCondition()
        {
            return _processedMilliseconds < _stopwatch.ElapsedMilliseconds;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();// necessary to ensure async processing

            const long tapeLength = FramesPerSecond * 100; // Tape is allocated for 100 seconds
            var tape = new Tape(tapeLength);
            //tape.AddEvent(new HeartBeatEvent(_logger), 0);
            
            _stopwatch.Start();
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // read commands
                    _commandService.AddEvents(tape);

                    // todo: we may want periodically reset stopwatch timer and _processedFrames
                    _processedMilliseconds = _processedFrames * MillisecondsPerFrame;
                    SpinWait.SpinUntil(SpinUntilCondition);
                    await tape.ProcessFrameAsync();
                    _processedFrames++;
                }
            }
            finally
            {
                _stopwatch.Stop();
            }
        }
    }
}
