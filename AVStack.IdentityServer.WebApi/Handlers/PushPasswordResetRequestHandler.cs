using AVStack.IdentityServer.WebApi.Models.Requests;
using AVStack.MessageBus.Abstraction;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AVStack.MessageBus.Extensions;

namespace AVStack.IdentityServer.WebApi.Handlers
{
    public class PushPasswordResetRequestHandler : AsyncRequestHandler<PushPasswordResetRequest>
    {
        private const string Monitoring = "monitoring";
        private const string AccountPasswordResetKey = "account.user.password-reset";

        private readonly IMessageBusFactory _busFactory;
        private readonly ILogger<PushUserSignUpRequestHandler> _logger;

        public PushPasswordResetRequestHandler(IMessageBusFactory busFactory, ILogger<PushUserSignUpRequestHandler> logger)
        {
            _busFactory = busFactory;
            _logger = logger;
        }

        protected override Task Handle(PushPasswordResetRequest request, CancellationToken cancellationToken)
        {
            using (var producer = _busFactory.CreateProducer())
            {
                var basicProperties = producer.CreateBasicProperties();
                basicProperties.SetDefaultValues();

                try
                {
                    producer.Publish(Monitoring, routingKey:AccountPasswordResetKey, properties:basicProperties,
                        JsonSerializer.Serialize(new { request.FullName, request.EmailAddress, request.Callback }));

                    _logger.LogInformation("Message was published successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Something went wrong while publishing message.");
                    return Task.FromResult(false);
                }
            }

            return Task.CompletedTask;
        }
    }
}