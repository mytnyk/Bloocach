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
        public EventFeedService(CommandService commandService, ILogger<EventFeedService> logger)
        {
            _commandService = commandService;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();// necessary to ensure async processing

            long processedFrames = 0;
            const long tapeLength = FramesPerSecond * 100; // Tape is allocated for 100 seconds
            var tape = new Tape(tapeLength);
            //tape.AddEvent(new HeartBeatEvent(_logger), 0);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // read commands
                    _commandService.AddEvents(tape);

                    // todo: we may want periodically reset stopwatch timer and processedFrames
                    var processedMilliseconds = processedFrames * MillisecondsPerFrame;
                    SpinWait.SpinUntil(() => processedMilliseconds < stopwatch.ElapsedMilliseconds);
                    await tape.ProcessFrameAsync();
                    processedFrames++;
                }
            }
            finally
            {
                stopwatch.Stop();
            }
        }
    }
}
