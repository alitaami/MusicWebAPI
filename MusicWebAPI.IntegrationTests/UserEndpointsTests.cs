using Microsoft.AspNetCore.Mvc.Testing;
using MusicWebAPI.Application.Commands;
using System.Net.Http.Json;
using System.Net;
using NUnit.Framework;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace MusicWebAPI.IntegrationTests
{
    [TestFixture]
    public class UserEndpointsTests
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;

        // This will run once per class, similar to xUnit's IClassFixture
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Test"); // Set the environment to "Test"
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        // Optionally add test-specific configuration here
                        config.AddJsonFile("appsettings.json", optional: true); // Default config
                    });
                });
        }

        // This will run before each test
        [SetUp]
        public void SetUp()
        {
            _client = _factory.CreateClient();
        }

        [Test]
        public async Task RegisterUser_ValidData_Returns200()
        {
            var request = new RegisterUserCommand
            {
                UserName = "newuser",
                Email = "newuser@example.com",
                Password = "Secure123!",
                FullName = "New User",
                IsArtist = false
            };

            var response = await _client.PostAsJsonAsync("/api/register", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task RegisterUser_InvalidData_Returns400()
        {
            var request = new RegisterUserCommand
            {
                UserName = "",
                Email = "invalid_email",
                Password = "short",
                FullName = "User",
                IsArtist = false
            };

            var response = await _client.PostAsJsonAsync("/api/register", request);

            Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task LoginUser_ValidData_Returns200()
        {
            var request = new LoginUserCommand
            {
                Email = "newuser@example.com",
                Password = "Secure123!"
            };

            var response = await _client.PostAsJsonAsync("/api/login", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task LoginUser_InvalidData_Returns400()
        {
            var request = new LoginUserCommand
            {
                Email = "invalid_email",
                Password = "short"
            };

            var response = await _client.PostAsJsonAsync("/api/login", request);

            Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.OK));
        }

        // This will run after each test
        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
        }

        // This will run once after all tests
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _factory.Dispose();
        }
    }
}
