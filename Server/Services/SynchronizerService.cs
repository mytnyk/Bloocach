using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Server.Model;
using Synchronization;

namespace Server.Services
{
    public sealed class SynchronizerService : Synchronizer.SynchronizerBase, IDisposable
    {
        private readonly World _world;
        private readonly ILogger<SynchronizerService> _logger;

        // 1. may be it is just enough to have the list of object IDs that have changed.
        // but then we need somehow read these objects concurrently
        // 2. each client will have its own synchronizer, so these queues can be similar
        // but for different regions of interest they will have different objects.
        //private readonly BlockingCollection<ObjectState> _queue = new();
        public SynchronizerService(World world, ILogger<SynchronizerService> logger)
        {
            _world = world;
            _logger = logger;
            // subscribe to changes in world
            //_world.OnChanged += OnWorldChanged;
        }
        /*
        private void OnWorldChanged(object _, ObjectState state)
        {
            _queue.Add(state);
        }*/

        public override async Task GetUpdates(IAsyncStreamReader<SynchronizerRequest> requestStream, 
            IServerStreamWriter<SynchronizerResponse> responseStream, 
            ServerCallContext context)
        {
            using var _queue = new BlockingCollection<ObjectState>();
            void OnWorldChanged(object _, ObjectState state)
            {
                _queue.Add(state);
            }
            _world.OnChanged += OnWorldChanged;

            try
            {
                // ReSharper disable once NotAccessedVariable : todo: use it to sync speed of updates
                long lastReadChunk = 0;
                SynchronizerRequest.Types.RegionOfInterest regionOfInterest = null;
                var inputTask = Task.Run(async () =>
                {
                    var input = requestStream.ReadAllAsync(context.CancellationToken);
                    await foreach (var r in input)
                    {
                        Interlocked.Exchange(ref lastReadChunk, r.LastReadChunk);
                        regionOfInterest = r.RegionOfInterest;
                    }
                });

                int i = 0;
                var sw = new Stopwatch();
                var sw2 = new Stopwatch();
                sw.Start();
                long delayMax = 0;
                long chunk = 0;
                foreach (var obj in _queue.GetConsumingEnumerable(context.CancellationToken))
                {
                    i++;
                    /*
                    if (regionOfInterest == null)
                    {
                        continue;
                    }
    
                    if (regionOfInterest.Xmin > obj.X || regionOfInterest.Xmax < obj.X ||
                        regionOfInterest.Ymin > obj.Y || regionOfInterest.Ymax < obj.Y )
                    {
                        continue;
                    }*/

                    var lrc = Interlocked.Read(ref lastReadChunk);
                    if (lrc + 100 < chunk)
                    {
                        _logger.LogWarning("TODO: squash changes for one object because client is slow");
                    }

                    sw2.Start();
                    // write changes to response:
                    await responseStream.WriteAsync(new SynchronizerResponse()
                    {
                        Chunk = chunk++,
                        Objects =
                        {
                            new SynchronizerResponse.Types.Object()
                            {
                                Id = obj.Id,
                                X = obj.X,
                                Y = obj.Y,
                                State = obj.State,
                                Type = obj.Type,
                            }
                        }
                    });
                    sw2.Stop();

                    var elapsed = sw.ElapsedMilliseconds;
                    if (delayMax < elapsed && i > 40)
                    {
                        delayMax = elapsed;
                        _logger.LogInformation($"Max delay {delayMax} {sw2.ElapsedMilliseconds}");
                    }

                    sw.Restart();
                }
            }
            finally
            {
                _world.OnChanged -= OnWorldChanged;
                _queue.CompleteAdding();
            }
        }

        public void Dispose()
        {
            //_world.OnChanged -= OnWorldChanged;
            //_queue.CompleteAdding();
        }
    }
}