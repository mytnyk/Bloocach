using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Server.Events
{
    public sealed class HeartBeatEvent : IEvent
    {
        private readonly ILogger _logger;
        private readonly Stopwatch _stopwatch = new();
        public HeartBeatEvent(ILogger logger)
        {
            _logger = logger;
            _stopwatch.Start();
        }
        public Task ProcessAsync(IEventContainer eventContainer)
        {
            _logger.LogInformation($"Heart beat! {_stopwatch.ElapsedMilliseconds}");
            eventContainer.AddEvent(this, 100);
            return Task.CompletedTask;
        }
    }
}