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
    public class UserRegistrationHandler : AsyncRequestHandler<UserRegistrationRequest>
    {
        private const string Monitoring = "monitoring";
        private const string AccountUserRegistrationKey = "account.user.registration";

        private readonly IMessageBusFactory _busFactory;
        private readonly ILogger<UserRegistrationHandler> _logger;

        public UserRegistrationHandler(IMessageBusFactory busFactory, ILogger<UserRegistrationHandler> logger)
        {
            _busFactory = busFactory;
            _logger = logger;
        }

        protected override Task Handle(UserRegistrationRequest request, CancellationToken cancellationToken)
        {
            using (var producer = _busFactory.CreateProducer())
            {
                var basicProperties = producer.CreateBasicProperties();
                basicProperties.SetDefaultValues();
                basicProperties.Type = nameof(UserRegistration);

                try
                {
                    producer.Publish(Monitoring, routingKey:AccountUserRegistrationKey, properties:basicProperties,
                        JsonSerializer.Serialize(new UserRegistration()
                        {
                            FullName = request.FullName,
                            EmailAddress = request.Email,
                            Callback = request.Callback
                        }));
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