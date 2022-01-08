using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Server.Commands;
using Server.Events;

namespace Server.Services
{
    public sealed class CommandService
    {
        private readonly ILogger<CommandService> _logger;
        private readonly ConcurrentQueue<ICommand> _commandQueue = new();

        public CommandService(ILogger<CommandService> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// 'Send' can be called concurrently by different clients.
        /// </summary>
        public void Send(ICommand command)
        {
            _commandQueue.Enqueue(command);
        }
        /// <summary>
        /// Called from Tape loop.
        /// </summary>
        public void AddEvents(IEventContainer eventContainer)
        {
            while (_commandQueue.TryDequeue(out var command))
            {
                eventContainer.AddEvent(command.CreateEvent(), 0);
            }
        }
    }
}
