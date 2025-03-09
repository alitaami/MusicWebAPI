using Microsoft.AspNetCore.Mvc.Testing;
using MusicWebAPI.Application.Commands;
using System.Net.Http.Json;
using System.Net;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NUnit.Framework;

namespace MusicWebAPI.IntegrationTests
{
    public class UserEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public UserEndpointsTests(WebApplicationFactory<Program> factory)
        {
            // Arrange
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");  // Set to Development for testing
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    // Ensure app settings are properly configured for testing
                    config.AddJsonFile("appsettings.Development.json", optional: true);
                });
            }).CreateClient();
        }

        [Fact]
        public async Task RegisterUser_ValidData_Returns200()
        {
            // Arrange
            var request = new RegisterUserCommand
            {
                UserName = "newuser",
                Email = "newuser@example.com",
                Password = "Secure123!",
                FullName = "New User",
                IsArtist = false
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/register", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task RegisterUser_InvalidData_Returns400()
        {
            // Arrange
            var request = new RegisterUserCommand
            {
                UserName = "",  // Invalid
                Email = "invalid_email", // Invalid
                Password = "short",  // Invalid
                FullName = "User",
                IsArtist = false
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/register", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task LoginUser_ValidData_Returns200()
        {
            // Arrange
            var request = new LoginUserCommand
            {
                Email = "newuser@example.com",
                Password = "Secure123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/login", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task LoginUser_InvalidData_Returns400()
        {
            // Arrange
            var request = new LoginUserCommand
            {
                Email = "invalid_email", // Invalid
                Password = "short"  // Invalid
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/login", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
