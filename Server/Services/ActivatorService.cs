using System.Threading.Tasks;
using Commands;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Server.Commands;
using Server.Model;
using Activator = Commands.Activator;

namespace Server.Services
{
    public class ActivatorService : Activator.ActivatorBase
    {
        private readonly CommandService _commandService;
        private readonly World _world;
        private readonly ILogger<ActivatorService> _logger;
        public ActivatorService(World world, CommandService commandService, ILogger<ActivatorService> logger)
        {
            _commandService = commandService;
            _world = world;
            _logger = logger;
        }

        public override Task<ActivationResponse> Activate(ActivationRequest request, ServerCallContext context)
        {
            _commandService.Send(new ActivateObjectCommand()
            {
                World = _world,
                ObjectId = request.Id
            });
            return Task.FromResult(new ActivationResponse
            {
                Message = "Activated object " + request.Id
            });
        }

        public override Task<CreateResponse> Create(CreateRequest request, ServerCallContext context)
        {
            var id = _world.CreateCircleObject();
            return Task.FromResult(new CreateResponse
            {
                Id = id
            });
        }
    }
}
