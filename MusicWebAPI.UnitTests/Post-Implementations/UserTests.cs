using Moq;
using AutoMapper;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Application.Features.Properties.Auth.Commands.GoogleLogin;
using MusicWebAPI.Application.Features.Properties.Auth.Commands.Login;
using MusicWebAPI.Application.Features.Properties.Auth.Commands.Register;
using MusicWebAPI.Application.Features.Properties.Auth.Commands.ForgetPassword;
using MusicWebAPI.Domain.External.Caching;
using MusicWebAPI.Application.Features.Properties.Auth.Commands.ResetPassword;

namespace MusicWebAPI.UnitTests.TDD
{
    [TestFixture]
    public class UserTests
    {
        private Mock<IServiceManager> _serviceManagerMock;
        private Mock<ICacheService> _cacheServiceMock;
        private Mock<IMapper> _mapperMock;
        private RegisterCommandHandler _registerUserHandler;
        private LoginCommandHandler _loginUserHandler;
        private GoogleLoginCommandHandler _googleLoginCommandHandler;
        private ForgetPasswordCommandHandler _forgetPassCommandHandler;
        private ResetPasswordCommandHandler _resetPassCommandHandler;

        [SetUp]
        public void Setup()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _cacheServiceMock = new Mock<ICacheService>();
            _mapperMock = new Mock<IMapper>();
            _registerUserHandler = new RegisterCommandHandler(_serviceManagerMock.Object, _mapperMock.Object);
            _loginUserHandler = new LoginCommandHandler(_serviceManagerMock.Object);
            _googleLoginCommandHandler = new GoogleLoginCommandHandler(_serviceManagerMock.Object);
            _forgetPassCommandHandler = new ForgetPasswordCommandHandler(_serviceManagerMock.Object, _cacheServiceMock.Object);
            _resetPassCommandHandler = new ResetPasswordCommandHandler(_serviceManagerMock.Object, _cacheServiceMock.Object);
        }

        [Test]
        public async Task Handle_RegisterUser_When_Successful()
        {
            // Arrange
            var command = new RegisterCommand
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
            var command = new RegisterCommand
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
            var loginCommand = new LoginCommand
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
            var loginCommand = new LoginCommand
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

        [Test]
        public async Task Handle_ForgetPassword_When_Successful()
        {
            // Arrange
            string email = "test@gmail.com";
            string otp = "123456";

            _serviceManagerMock
                   .Setup(service => service.User.ForgetPassword(email))
                   .ReturnsAsync(otp);

            // Act
            await _forgetPassCommandHandler.Handle(new ForgetPasswordCommand(email), CancellationToken.None);

            // Assert
            _serviceManagerMock.Verify(service => service.User.ForgetPassword(email), Times.Once);
        }

        [Test]
        public async Task Handle_ForgetPassword_When_UserNotFound()
        {
            // Arrange
            string email = "test@gmail.com";
            string otp = "123456";

            _serviceManagerMock
                 .Setup(service => service.User.ForgetPassword(email))
                 .ThrowsAsync(new NotFoundException(Resource.UserNotFound));

            // Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await _forgetPassCommandHandler.Handle(new ForgetPasswordCommand(email), CancellationToken.None));

            // Assert
            _serviceManagerMock.Verify(service => service.User.ForgetPassword(email), Times.Once);
            Assert.AreEqual(Resource.UserNotFound, ex.Message);
        }

        [Test]
        public async Task Handle_ForgetPassword_When_NotSuccessful()
        {
            // Arrange
            string email = "test@gmail.com";
            string otp = "123456";

            _serviceManagerMock
                 .Setup(service => service.User.ForgetPassword(email))
                 .ThrowsAsync(new Exception("Error"));

            // Act
            var ex = Assert.ThrowsAsync<Exception>(async () => await _forgetPassCommandHandler.Handle(new ForgetPasswordCommand(email), CancellationToken.None));

            // Assert
            _serviceManagerMock.Verify(service => service.User.ForgetPassword(email), Times.Once);
            Assert.AreEqual("Error", ex.Message);
        }

        [Test]
        public async Task Handle_ResetPassword_When_Successful()
        {
            // Arrange
            string email = "test@gmail.com";
            string otp = "123456";
            string newPass = "@Litaami1985";
            string cacheKey = $"{Resource.OtpCode_CacheKey}{email}";

            _cacheServiceMock
                .Setup(c => c.GetAsync<string>(cacheKey))
                .ReturnsAsync(otp);

            _serviceManagerMock
                .Setup(service => service.User.ResetPassword(email, newPass))
                .Returns(Task.CompletedTask);

            // Act
            await _resetPassCommandHandler.Handle(new ResetPasswordCommand(email, otp, newPass), CancellationToken.None);

            // Assert
            _serviceManagerMock.Verify(service => service.User.ResetPassword(email, newPass), Times.Once);
            _cacheServiceMock.Verify(c => c.RemoveAsync(cacheKey), Times.Once);
        }


        [Test]
        public async Task Handle_ResetPassword_When_UserNotFound()
        {
            // Arrange
            string email = "test@gmail.com";
            string otp = "123456";
            string newPass = "@Litaami1985";
            string cacheKey = $"{Resource.OtpCode_CacheKey}{email}";

            _cacheServiceMock
                .Setup(c => c.GetAsync<string>(cacheKey))
                .ReturnsAsync(otp);

            _serviceManagerMock
                 .Setup(service => service.User.ResetPassword(email, newPass))
                 .ThrowsAsync(new NotFoundException(Resource.UserNotFound));

            // Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await _resetPassCommandHandler.Handle(new ResetPasswordCommand(email, otp, newPass), CancellationToken.None));

            // Assert
            _serviceManagerMock.Verify(service => service.User.ResetPassword(email, newPass), Times.Once);
            Assert.AreEqual(Resource.UserNotFound, ex.Message);
        }

        [Test]
        public async Task Handle_ResetPassword_When_OtpIsNullOrWrong()
        {
            // Arrange
            string email = "test@gmail.com";
            string otp = "123456";
            string newPass = "@Litaami1985";

            string cacheKey = $"{Resource.OtpCode_CacheKey}{email}";
            _cacheServiceMock
                .Setup(c => c.GetAsync<string>(cacheKey))
                .ReturnsAsync((string?)null);

            // Act
            var ex = Assert.ThrowsAsync<LogicException>(async () => await _resetPassCommandHandler.Handle(new ResetPasswordCommand(email, otp, newPass), CancellationToken.None));

            // Assert
            Assert.AreEqual(Resource.WrongOtpCodeError, ex.Message);
        }

        [Test]
        public async Task Handle_ResetPassword_When_NotSuccessful()
        {
            // Arrange
            string email = "test@gmail.com";
            string otp = "123456";
            string newPass = "@Litaami1985";
            string cacheKey = $"{Resource.OtpCode_CacheKey}{email}";

            _cacheServiceMock
                .Setup(c => c.GetAsync<string>(cacheKey))
                .ReturnsAsync(otp);

            _serviceManagerMock
                 .Setup(service => service.User.ResetPassword(email, newPass))
                 .ThrowsAsync(new Exception("Error"));

            // Act
            var ex = Assert.ThrowsAsync<Exception>(async () => await _resetPassCommandHandler.Handle(new ResetPasswordCommand(email, otp, newPass), CancellationToken.None));

            // Assert
            _serviceManagerMock.Verify(service => service.User.ResetPassword(email, newPass), Times.Once);
            Assert.AreEqual("Error", ex.Message);
        }
    }
}

