using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Greet;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Test;

namespace Client
{
    class Program
    {
        //private static readonly object _lock = new object();
        //private static long _sum = 0;
        //private static long _count = 0;
        //private static long _average = 0;
        private static readonly ConcurrentQueue<long> _delays = new ConcurrentQueue<long>();

        static void AddDelay(long ms)
        {
            _delays.Enqueue(ms);
            while (_delays.Count > 100)
            {
                _delays.TryDequeue(out _);
            }/*
            lock (_lock)
            {
                _sum += ms;
                _count++;
                _average = _sum / _count;
            }*/
        }
        static async Task Main(string[] args)
        {
            var count = 2;
            var tasks = new Task[count + 1];
            for (int i = 0; i < count; i++)
            {
                tasks[i] = Task.Run(Do);
            }

            tasks[count] = Task.Run(async () =>
            {
                var el = new long[100];
                while (true)
                {
                    _delays.CopyTo(el, 0);
                    var average = (long) el.Average();
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    Console.Write($"[{average} ms]                             \r");
                }
            });
            await Task.WhenAll(tasks);
        }
        static async Task Do()
        {
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("http://18.191.29.36:5000");
            //ec2-18-191-29-36.us-east-2.compute.amazonaws.com
            //using var channel = GrpcChannel.ForAddress("https://ec2-18-191-29-36.us-east-2.compute.amazonaws.com:5001");
            var client = new Sync.SyncClient(channel);

            var sw = new Stopwatch();
            sw.Start();
            var duplex = client.GetUpdates();
            var responseStream = duplex.ResponseStream;
            var requestStream = duplex.RequestStream;
            Console.WriteLine($"[{sw.ElapsedMilliseconds} ms] Got first response.");
            sw.Restart();
            var random = new Random();
            var key = random.Next(100);
            await requestStream.WriteAsync(new InputRequest() {Id = key.ToString()});
            await foreach(var r in responseStream.ReadAllAsync(CancellationToken.None))
            {
                var state = responseStream.Current;
                if (state.Message != $"Current ID is {key.ToString()}")
                {
                    throw new Exception("Broken!");
                }

                AddDelay(sw.ElapsedMilliseconds);
                //Console.Write($"[{sw.ElapsedMilliseconds} ms] {state.Message}                            \r");
                sw.Restart();
                key = random.Next(100);
                await requestStream.WriteAsync(new InputRequest() { Id = key.ToString() });
            }
            //sw.Stop();
            //Console.WriteLine($"[{sw.ElapsedMilliseconds} ms] Greeting: " + reply.Message);
            //Console.WriteLine("Press any key to exit...");
            //Console.ReadKey();
        }
    }
}
