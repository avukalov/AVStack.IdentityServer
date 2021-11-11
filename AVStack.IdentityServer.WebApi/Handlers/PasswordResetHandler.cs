using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AVStack.IdentityServer.Common.Models;
using AVStack.IdentityServer.WebApi.Models.Commands;
using AVStack.MessageBus.Abstraction;
using AVStack.MessageBus.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AVStack.IdentityServer.WebApi.Handlers
{
    public class PasswordResetHandler : AsyncRequestHandler<PasswordResetRequest>
    {
        private const string Monitoring = "monitoring";
        private const string AccountPasswordResetKey = "account.user.password-reset";

        private readonly IMessageBusFactory _busFactory;
        private readonly ILogger<UserRegistrationHandler> _logger;

        public PasswordResetHandler(IMessageBusFactory busFactory, ILogger<UserRegistrationHandler> logger)
        {
            _busFactory = busFactory;
            _logger = logger;
        }

        protected override Task Handle(PasswordResetRequest request, CancellationToken cancellationToken)
        {
            using (var producer = _busFactory.CreateProducer())
            {
                var basicProperties = producer.CreateBasicProperties();
                basicProperties.SetDefaultValues();
                basicProperties.Type = nameof(PasswordReset);

                try
                {
                    producer.Publish(Monitoring, routingKey:AccountPasswordResetKey, properties:basicProperties,
                        JsonSerializer.Serialize(new PasswordReset()
                        {
                            FullName = request.FullName,
                            EmailAddress = request.EmailAddress,
                            Callback = request.Callback
                        }));

                    _logger.LogInformation("Message was published successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Something went wrong while publishing message.");
                    throw;
                }
            }

            return Task.CompletedTask;
        }
    }
}