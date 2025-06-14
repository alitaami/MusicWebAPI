using Moq;
using AutoMapper;
using MusicWebAPI.Application.Commands.Handlers;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Application.Features.Properties.Commands.Auth;
using MusicWebAPI.Application.Features.Properties.Commands.Handlers;

namespace MusicWebAPI.UnitTests.TDD
{
    [TestFixture]
    public class UserTests
    {
        private Mock<IServiceManager> _serviceManagerMock;
        private Mock<IMapper> _mapperMock;
        private RegisterUserHandler _registerUserHandler;
        private LoginUserHandler _loginUserHandler;
        private GoogleLoginCommandHandler _googleLoginCommandHandler;

        [SetUp]
        public void Setup()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _mapperMock = new Mock<IMapper>();
            _registerUserHandler = new RegisterUserHandler(_serviceManagerMock.Object, _mapperMock.Object);
            _loginUserHandler = new LoginUserHandler(_serviceManagerMock.Object);
            _googleLoginCommandHandler = new GoogleLoginCommandHandler(_serviceManagerMock.Object);
        }

        [Test]
        public async Task Handle_RegisterUser_When_Successful()
        {
            // Arrange
            var command = new RegisterUserCommand
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Secure123!",
                FullName = "Test User",
                IsArtist = false
            };

            var expectedToken = "some-jwt-token";

            _serviceManagerMock.Setup(s => s.User.RegisterUser(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(expectedToken);

            // Act
            var result = await _registerUserHandler.Handle(command, CancellationToken.None);

            // Assert
            _serviceManagerMock.Verify(service => service.User.RegisterUser(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedToken, result.Token);
        }

        [Test]
        public void Handle_RegisterUser_When_Not_Successful()
        {
            // Arrange
            var command = new RegisterUserCommand
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Secure123!",
                FullName = "Test User",
                IsArtist = false
            };

            _serviceManagerMock.Setup(s => s.User.RegisterUser(It.IsAny<User>(), It.IsAny<string>()))
                .ThrowsAsync(new LogicException(Resource.DuplicateUserError));

            // Act 
            var ex = Assert.ThrowsAsync<LogicException>(async () => await _registerUserHandler.Handle(command, CancellationToken.None));

            // Assert
            _serviceManagerMock.Verify(service => service.User.RegisterUser(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            Assert.AreEqual(Resource.DuplicateUserError, ex.Message);
        }

        [Test]
        public async Task Handle_LoginUser_When_Successful()
        {
            // Arrange
            var loginCommand = new LoginUserCommand
            {
                Email = "user@example.com",
                Password = "password123"
            };

            // Mock the LoginUser method to return a token
            var expectedToken = "some-jwt-token";
            _serviceManagerMock
                .Setup(service => service.User.LoginUser(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(expectedToken);

            // Act
            var result = await _loginUserHandler.Handle(loginCommand, CancellationToken.None);

            // Assert
            _serviceManagerMock.Verify(service => service.User.LoginUser(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.NotNull(result);
            Assert.AreEqual(expectedToken, result.Token);
        }

        [Test]
        public async Task Handle_LoginUser_When_Not_Successful()
        {
            // Arrange
            var loginCommand = new LoginUserCommand
            {
                Email = "invaliduser@example.com",
                Password = "wrongpassword"
            };

            // Mock the LoginUser method to throw an exception
            _serviceManagerMock
                .Setup(service => service.User.LoginUser(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Invalid credentials"));

            // Act 
            var ex = Assert.ThrowsAsync<Exception>(async () => await _loginUserHandler.Handle(loginCommand, CancellationToken.None));

            // Assert
            _serviceManagerMock.Verify(service => service.User.LoginUser(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.AreEqual("Invalid credentials", ex.Message);
        }

        [Test]
        public async Task Handle_GoogleLogin_When_Successful()
        {
            // Arrange

            var expectedToken = "some-jwt-token";
            _serviceManagerMock
                .Setup(service => service.User.GoogleLogin(It.IsAny<string>()))
                .ReturnsAsync(expectedToken);

            // Act
            var result = await _googleLoginCommandHandler.Handle(new GoogleLoginCommand(It.IsAny<string>()), CancellationToken.None);

            // Assert
            _serviceManagerMock.Verify(service => service.User.GoogleLogin(It.IsAny<string>()), Times.Once);
            Assert.NotNull(result);
            Assert.AreEqual(expectedToken, result);
        }

        [Test]
        public async Task Handle_GoogleLogin_When_NotSuccessful()
        {
            // Arrange

            _serviceManagerMock
                .Setup(service => service.User.GoogleLogin(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Error"));

            // Act
            var ex = Assert.ThrowsAsync<Exception>(async () => await _googleLoginCommandHandler.Handle(new GoogleLoginCommand(It.IsAny<string>()), CancellationToken.None));

            // Assert
            _serviceManagerMock.Verify(service => service.User.GoogleLogin(It.IsAny<string>()), Times.Once);
            Assert.AreEqual("Error", ex.Message);
        }
    }
}

