using AutoMapper;
using Moq;
using MusicWebAPI.Application.Commands.Handlers;
using MusicWebAPI.Application.Features.Properties.Queries;
using MusicWebAPI.Application.Features.Properties.Queries.Handlers;
using MusicWebAPI.Core;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using MusicWebAPI.Infrastructure.Caching.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Application.ViewModels.HomeViewModel;

namespace MusicWebAPI.UnitTests.TDD
{
    public class SongTests
    {
        private Mock<ICacheService> _cacheServiceMock;
        private Mock<IServiceManager> _serviceManagerMock;
        private Mock<IMapper> _mapperMock; 
        private GetSongsQueryHandler _getSongsHandler;

        [SetUp]
        public void Setup()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _mapperMock = new Mock<IMapper>(); 
            _getSongsHandler = new GetSongsQueryHandler(_serviceManagerMock.Object, _cacheServiceMock.Object);
        }
        [Test]
        public async Task Handle_GetSongs_ReturnSongs()
        {
            //Arrange
            var query = new GetSongsQuery(term: "", 10, 1);

            _serviceManagerMock.Setup(s => s.Home.GetSongs(query.Term, query.PageSize, query.PageNumber, CancellationToken.None))
                 .ReturnsAsync(new Core.PaginatedResult<object>());

            //Act 
            var result = await _getSongsHandler.Handle(query, CancellationToken.None);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<PaginatedResult<GetSongsViewModel>>(result);
        }

        [Test]
        public async Task Handle_GetSongs_ThrowException()
        {
            //Arrange
            var query = new GetSongsQuery(term: "", 10, 1);

            _serviceManagerMock.Setup(s => s.Home.GetSongs(query.Term, query.PageSize, query.PageNumber, CancellationToken.None))
                 .ThrowsAsync(new Exception("Exception"));

            //Act 

            //Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => await _getSongsHandler.Handle(query, CancellationToken.None));
            Assert.That(ex.Message, Is.EqualTo("Exception"));
        }
    }
}
