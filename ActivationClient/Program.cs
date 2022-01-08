using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Synchronization;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("http://localhost:5000");
            //using var channel = GrpcChannel.ForAddress("http://18.191.29.36:5000");
            var client = new Commands.Activator.ActivatorClient(channel);

            var createResponse = await client.CreateAsync(new Commands.CreateRequest());

            var response = await client.ActivateAsync(new Commands.ActivationRequest() {Id = createResponse.Id});
            Console.WriteLine(response.Message);

            var syncClient = new Synchronizer.SynchronizerClient(channel);
            var duplex = syncClient.GetUpdates();
            var responseStream = duplex.ResponseStream;
            var requestStream = duplex.RequestStream;

            long lastReadChunk = -1;
            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    await requestStream.WriteAsync(new SynchronizerRequest()
                    {
                        LastReadChunk = lastReadChunk,
                        RegionOfInterest = new SynchronizerRequest.Types.RegionOfInterest()
                        {
                            Xmin = 0, Xmax = 2000, Ymin = 0, Ymax = 2000
                        },
                    });
                }
                
            });

            var objectById = new Dictionary<int, SynchronizerResponse.Types.Object>();

            int i = 0;
            long maxDelay = 0;
            var sw = new Stopwatch();
            sw.Start();
            await foreach (var r in responseStream.ReadAllAsync(cancellationToken))
            {
                i++;
                lastReadChunk = r.Chunk;
                var o = r.Objects[0];
                objectById[o.Id] = o;
                var sb = new StringBuilder();
                foreach (var data in objectById)
                {
                    sb.Append($"{data.Key} (X = {data.Value.X} Y= {data.Value.Y}) ");
                }

                var elapsed = sw.ElapsedMilliseconds;
                sw.Restart();
                if (i > 100)
                {
                    maxDelay = Math.Max(maxDelay, elapsed);
                }

                sb.Append($"elapsed = {elapsed} max = {maxDelay}");
                sb.Append("                                      \r");
                Console.Write(sb.ToString());
                //await Task.Delay(200);
            }
        }
    }
}
