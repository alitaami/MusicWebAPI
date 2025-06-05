using NUnit.Framework;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MusicWebAPI.Application.Commands;
using MusicWebAPI.Application.Commands.Handlers;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using static MusicWebAPI.Application.ViewModels.UserViewModel;
using System.Reflection.Metadata;
using MusicWebAPI.Application.Features.Properties.Queries.Handlers;
using MusicWebAPI.Application.Features.Properties.Queries;
using MusicWebAPI.Core;
using static MusicWebAPI.Application.ViewModels.HomeViewModel;

namespace MusicWebAPI.UnitTests.TDD
{
    [TestFixture]
    public class UserTests
    {
        private Mock<IServiceManager> _serviceManagerMock;
        private Mock<IMapper> _mapperMock;
        private RegisterUserHandler _registerUserHandler;
        private LoginUserHandler _loginUserHandler;

        [SetUp]
        public void Setup()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _mapperMock = new Mock<IMapper>();
            _registerUserHandler = new RegisterUserHandler(_serviceManagerMock.Object, _mapperMock.Object);
            _loginUserHandler = new LoginUserHandler(_serviceManagerMock.Object);
        }
         
        [Test]
        public async Task Handle_RegisterUser_ReturnsMappedViewModel()
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

            var userEntity = new User
            {
                Id = "123",
                UserName = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                IsArtist = false,
                Bio = "test"
            };

            var expectedToken = "some-jwt-token";
             
            _serviceManagerMock.Setup(s => s.User.RegisterUser(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(expectedToken);
             
            // Act
            var result = await _registerUserHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedToken, result.Token);
            _serviceManagerMock.Verify(service => service.User.RegisterUser(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void Handle_RegisterUser_ThrowsException_ReturnsError()
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
                .ThrowsAsync(new Exception("User already exists"));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => await _registerUserHandler.Handle(command, CancellationToken.None));
        }

        [Test]
        public async Task Handle_ShouldReturnToken_WhenCredentialsAreValid()
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
            Assert.NotNull(result);
            Assert.AreEqual(expectedToken, result.Token);
            _serviceManagerMock.Verify(service => service.User.LoginUser(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task Handle_ShouldThrowException_WhenInvalidCredentials()
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

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => await _loginUserHandler.Handle(loginCommand, CancellationToken.None));
            Assert.AreEqual("Invalid credentials", ex.Message);
        }
    }
}

