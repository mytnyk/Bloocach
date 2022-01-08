using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Model;

namespace Server.Services
{
    /// <summary>
    /// TODO: remove it.. just for test
    /// </summary>
    public sealed class ScreeningService : BackgroundService
    {
        private readonly World _world;
        private readonly ILogger<ScreeningService> _logger;

        public ScreeningService(World world, ILogger<ScreeningService> logger)
        {
            _world = world;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var sw = new Stopwatch();
            await Task.Yield();// necessary to ensure async processing
            while (!stoppingToken.IsCancellationRequested)
            {
                sw.Restart();
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);//~1010ms
                //sw.Restart();
                for (int i = 0; i < _world.Objects.Length; i++)
                {
                    var worldObject = _world.Objects[i];
                    if (worldObject != null)
                    {
                        _logger.LogInformation($"World object {i}: {worldObject.ToString()}");
                    }
                }
                _logger.LogInformation($"Time passed:  {sw.ElapsedMilliseconds}");
            }
        }
    }
}
