using AutoMapper;
using Moq;
using MusicWebAPI.Application.Commands.Handlers;
using MusicWebAPI.Application.Features.Properties.Commands;
using MusicWebAPI.Application.Features.Properties.Commands.Handlers;
using MusicWebAPI.Application.Features.Properties.Commands.Subscription;
using MusicWebAPI.Application.Features.Properties.Queries;
using MusicWebAPI.Application.Features.Properties.Queries.Handlers;
using MusicWebAPI.Application.Features.Properties.Queries.Subscription;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Entities.Subscription_Models;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;

namespace MusicWebAPI.UnitTests.TDD
{
    [TestFixture]
    public class SubscriptionTests
    {
        private Mock<IServiceManager> _serviceManagerMock;
        private Mock<IMapper> _mapperMock;
        private VerifySubscriptionCommandHandler _verifySubscribeCommandHandler;
        private SubscribeCommandHandler _subscribeCommandHandler;
        private GetSubscriptionsQueryHandler _getSubscriptionsQueryHandler;

        [SetUp]
        public void Setup()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _mapperMock = new Mock<IMapper>();
            _getSubscriptionsQueryHandler = new GetSubscriptionsQueryHandler(_serviceManagerMock.Object);
            _verifySubscribeCommandHandler = new VerifySubscriptionCommandHandler(_serviceManagerMock.Object);
            _subscribeCommandHandler = new SubscribeCommandHandler(_serviceManagerMock.Object);
        }

        [Test]
        public async Task GetSubscriptionPlans_When_Successful()
        {
            // Arrange

            var plans = new List<SubscriptionPlan>
            {
                new SubscriptionPlan { Id = Guid.NewGuid(), Name = "Basic", Price = 9.99M },
                new SubscriptionPlan { Id = Guid.NewGuid(), Name = "Premium", Price = 19.99M }
            };

            _serviceManagerMock
           .Setup(s => s.Subscription.GetPlans(It.IsAny<CancellationToken>()))
           .ReturnsAsync(plans);

            // Act
            var result = await _getSubscriptionsQueryHandler.Handle(new GetSubscriptionsQuery(), It.IsAny<CancellationToken>());

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(plans));
        }

        [Test]
        public async Task GetSubscriptionPlans_When_NotSuccessful()
        {
            // Arrange

            _serviceManagerMock
           .Setup(s => s.Subscription.GetPlans(It.IsAny<CancellationToken>()))
           .ThrowsAsync(new Exception("Error in GetPlans"));

            // Act 
            var exception = Assert.ThrowsAsync<Exception>(async () =>
            {
                await _getSubscriptionsQueryHandler.Handle(new GetSubscriptionsQuery(), CancellationToken.None);
            });

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Error in GetPlans"));
        }

        [Test]
        public async Task Subscribe_When_Successful()
        {
            // Arrange 
            var userId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var callbackBaseUrl = "https://example.com";
            var stripeUrl = "https://checkout.stripe.com/session_id";

            _serviceManagerMock
               .Setup(s => s.Subscription.Subscribe(planId, userId, It.IsAny<CancellationToken>(), callbackBaseUrl))
               .ReturnsAsync(stripeUrl);

            var command = new SubscribeCommand(planId, userId, callbackBaseUrl);

            // Act
            var url = await _subscribeCommandHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(url, Is.Not.Null);
            Assert.That(url, Does.Contain("stripe"));
        }

        [Test]
        public void Subscribe_When_NotSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var callbackBaseUrl = "https://example.com";

            _serviceManagerMock
                .Setup(s => s.Subscription.Subscribe(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>(), callbackBaseUrl))
                .ThrowsAsync(new LogicException("You are already subscribed."));

            var command = new SubscribeCommand(planId, userId, callbackBaseUrl);

            // Act 
            var exception = Assert.ThrowsAsync<LogicException>(async () =>
            {
                await _subscribeCommandHandler.Handle(command, CancellationToken.None);
            });

            // Assert
            Assert.That(exception.Message, Is.EqualTo("You are already subscribed."));
        }

        [Test]
        public async Task VerifyPayment_When_Successful()
        {
            // Arrange
            var sessionId = "valid_session";

            _serviceManagerMock
                .Setup(s => s.Subscription.VerifyPayment(sessionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _verifySubscribeCommandHandler.Handle(new VerifySubscriptionCommand(sessionId), It.IsAny<CancellationToken>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task VerifyPayment_When_NotSuccessful()
        {
            // Arrange
            var sessionId = "valid_session";

            _serviceManagerMock
                .Setup(s => s.Subscription.VerifyPayment(sessionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _verifySubscribeCommandHandler.Handle(new VerifySubscriptionCommand(sessionId), It.IsAny<CancellationToken>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result);
        }

    }
}
