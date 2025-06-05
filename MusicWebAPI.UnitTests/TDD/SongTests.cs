using AutoMapper;
using Moq;
using MusicWebAPI.Application.Features.Properties.Commands;
using MusicWebAPI.Application.Features.Properties.Commands.Handlers;
using MusicWebAPI.Application.Features.Properties.Queries;
using MusicWebAPI.Application.Features.Properties.Queries.Handlers;
using MusicWebAPI.Core;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Domain.External.Caching;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using static MusicWebAPI.Application.ViewModels.HomeViewModel;
using static MusicWebAPI.Application.ViewModels.SongsViewModel;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;

namespace MusicWebAPI.UnitTests.TDD
{
    public class SongTests
    {
        private Mock<ICacheService> _cacheServiceMock;
        private Mock<IServiceManager> _serviceManagerMock;
        private Mock<IMapper> _mapperMock;

        private GetSongsQueryHandler _getSongsHandler;
        private GetPopularSongsQueryHandler _getPopularSongsHandler;
        private GetPlaylistsQueryHandler _getPlaylistsHandler;
        private AddToPlaylistCommandHandler _addToPlaylistHandler;
        private DeletePlayListCommandHandler _deletePlayListHandler;
        private DeletePlayListSongCommandHandler _deletePlayListSongHandler;
        private ListenToSongCommandHandler _listenToSongHandler;

        [SetUp]
        public void Setup()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _cacheServiceMock = new Mock<ICacheService>();
            _mapperMock = new Mock<IMapper>();

            _getSongsHandler = new GetSongsQueryHandler(_serviceManagerMock.Object, _cacheServiceMock.Object);
            _getPopularSongsHandler = new GetPopularSongsQueryHandler(_serviceManagerMock.Object, _cacheServiceMock.Object);
            _getPlaylistsHandler = new GetPlaylistsQueryHandler(_serviceManagerMock.Object);
            _addToPlaylistHandler = new AddToPlaylistCommandHandler(_serviceManagerMock.Object);
            _deletePlayListHandler = new DeletePlayListCommandHandler(_serviceManagerMock.Object);
            _deletePlayListSongHandler = new DeletePlayListSongCommandHandler(_serviceManagerMock.Object);
            _listenToSongHandler = new ListenToSongCommandHandler(_serviceManagerMock.Object, _cacheServiceMock.Object);
        }

        [Test]
        public async Task Handle_GetSongs_When_ReturnSongs()
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
        public async Task Handle_GetPopularSongs_When_ReturnSongs()
        {
            //Arrange
            var query = new GetPopularSongsQuery(10, 1);

            _serviceManagerMock.Setup(s => s.Home.GetPopularSongs(query.PageSize, query.PageNumber, CancellationToken.None))
                 .ReturnsAsync(new Core.PaginatedResult<object>());

            //Act
            var result = await _getPopularSongsHandler.Handle(query, CancellationToken.None);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<PaginatedResult<GetSongsViewModel>>(result);

        }

        [Test]
        public async Task Handle_GetPlaylistsSongs_When_ReturnPlaylists()
        {
            //Arrange
            var query = new GetPlaylistsQuery(Guid.NewGuid());

            _serviceManagerMock.Setup(s => s.User.GetPlaylists(query.UserId, CancellationToken.None))
                 .ReturnsAsync(new List<object>());

            //Act
            var result = await _getPlaylistsHandler.Handle(query, CancellationToken.None);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<PlaylistViewModel>>(result);

        }

        [Test]
        public async Task Handle_GetSongs_When_ThrowException()
        {
            //Arrange
            var query = new GetSongsQuery(term: "", 10, 1);

            _serviceManagerMock.Setup(s => s.Home.GetSongs(query.Term, query.PageSize, query.PageNumber, CancellationToken.None))
                 .ThrowsAsync(new Exception("Exception"));

            //Act 
            var ex = Assert.ThrowsAsync<Exception>(async () => await _getSongsHandler.Handle(query, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo("Exception"));
        }

        [Test]
        public async Task Handle_GetPopularSongs_When_ThrowExcepton()
        {
            //Arrange
            var query = new GetPopularSongsQuery(10, 1);

            _serviceManagerMock.Setup(s => s.Home.GetPopularSongs(query.PageSize, query.PageNumber, CancellationToken.None))
                 .ThrowsAsync(new Exception("Exception"));

            //Act 
            var ex = Assert.ThrowsAsync<Exception>(async () => await _getPopularSongsHandler.Handle(query, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo("Exception"));
        }

        [Test]
        public async Task Handle_GetPlaylists_When_ThrowExcepton()
        {
            //Arrange
            var query = new GetPlaylistsQuery(Guid.NewGuid());

            _serviceManagerMock.Setup(s => s.User.GetPlaylists(query.UserId, CancellationToken.None))
                 .ThrowsAsync(new Exception("Exception"));

            //Act 
            var ex = Assert.ThrowsAsync<Exception>(async () => await _getPlaylistsHandler.Handle(query, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo("Exception"));
        }


        [Test]
        public async Task Handle_AddToPlaylist_With_PlaylistId_When_Successful()
        {
            // Arrange
            var command = new AddToPlaylistCommand(
                songId: Guid.NewGuid(),
                userId: Guid.NewGuid(),
                playlistId: Guid.NewGuid(),
                playlistName: "My Favorites"
            );

            _serviceManagerMock.Setup(s => s.User.AddToPlaylist(
                command.SongId,
                command.UserId,
                command.PlaylistId,
                command.PlaylistName,
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);

            // Act
            await _addToPlaylistHandler.Handle(command, CancellationToken.None);

            // Assert
            _serviceManagerMock.Verify(s => s.User.AddToPlaylist(
                command.SongId,
                command.UserId,
                command.PlaylistId,
                command.PlaylistName,
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Test]
        public async Task Handle_AddToPlaylist_Without_PlaylistId_When_Successful()
        {
            // Arrange
            var command = new AddToPlaylistCommand(
                songId: Guid.NewGuid(),
                userId: Guid.NewGuid(),
                playlistName: "My Favorites"
            );

            _serviceManagerMock.Setup(s => s.User.AddToPlaylist(
                command.SongId,
                command.UserId,
                null,
                command.PlaylistName,
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);

            // Act
            await _addToPlaylistHandler.Handle(command, CancellationToken.None);

            // Assert
            _serviceManagerMock.Verify(s => s.User.AddToPlaylist(
                command.SongId,
                command.UserId,
                command.PlaylistId,
                command.PlaylistName,
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Test]
        public void Handle_Should_Throw_Exception_When_AddToPlaylist_Fails()
        {
            // Arrange
            var command = new AddToPlaylistCommand(
                songId: Guid.NewGuid(),
                userId: Guid.NewGuid(),
                playlistName: "My Playlist"
            );

            _serviceManagerMock.Setup(s => s.User.AddToPlaylist(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            )).ThrowsAsync(new Exception("Something went wrong"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _addToPlaylistHandler.Handle(command, CancellationToken.None));

            Assert.That(ex.Message, Is.EqualTo("Something went wrong"));
            Assert.IsInstanceOf<Exception>(ex);
        }

        [Test]
        public async Task Handle_DeletePlayList_When_Successful()
        {
            // Arrange
            var playListId = Guid.NewGuid();
            var command = new DeletePlayListCommand(playListId);

            _serviceManagerMock.Setup(s => s.User.DeletePlayList(playListId, It.IsAny<CancellationToken>()))
                               .Returns(Task.CompletedTask);

            // Act
            await _deletePlayListHandler.Handle(command, CancellationToken.None);

            // Assert
            _serviceManagerMock.Verify(s => s.User.DeletePlayList(playListId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void Handle_DeletePlayList_When_Fails()
        {
            // Arrange
            var playListId = Guid.NewGuid();
            var command = new DeletePlayListCommand(playListId);

            _serviceManagerMock.Setup(s => s.User.DeletePlayList(playListId, It.IsAny<CancellationToken>()))
                               .ThrowsAsync(new Exception("Failed to delete playlist"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _deletePlayListHandler.Handle(command, CancellationToken.None));

            Assert.That(ex.Message, Is.EqualTo("Failed to delete playlist"));
        }

        [Test]
        public async Task Handle_DeletePlayListSong_When_Successful()
        {
            // Arrange
            var command = new DeletePlayListSongCommand(
                songId: Guid.NewGuid(),
                playListId: Guid.NewGuid()
            );

            _serviceManagerMock.Setup(s => s.User.DeleteSongFromPlayList(
                command.songId,
                command.playListId,
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);

            // Act
            await _deletePlayListSongHandler.Handle(command, CancellationToken.None);

            // Assert
            _serviceManagerMock.Verify(s => s.User.DeleteSongFromPlayList(
                command.songId,
                command.playListId,
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Test]
        public void Handle_DeletePlayListSong_When_ThrowException()
        {
            // Arrange
            var command = new DeletePlayListSongCommand(
                songId: Guid.NewGuid(),
                playListId: Guid.NewGuid()
            );

            _serviceManagerMock.Setup(s => s.User.DeleteSongFromPlayList(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            )).ThrowsAsync(new Exception("Delete failed"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => await _deletePlayListSongHandler.Handle(command, CancellationToken.None));
            Assert.That(ex.Message, Is.EqualTo("Delete failed"));
        }

        [Test]
        public async Task Handle_ListenToSong_When_Successful()
        {
            // Arrange
            var command = new ListenToSongCommand(
                songId: Guid.NewGuid(),
                userId: Guid.NewGuid()
            );

            _serviceManagerMock.Setup(s => s.User.ListenToSong(
                command.songId,
                command.userId,
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);

            // Act
            await _listenToSongHandler.Handle(command, CancellationToken.None);

            // Assert
            _serviceManagerMock.Verify(s => s.User.ListenToSong(
                command.songId,
                command.userId,
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Test]
        public void Handle_ListenToSong_When_UserNotFound()
        {
            // Arrange
            var command = new ListenToSongCommand(
                songId: Guid.NewGuid(),
                userId: Guid.NewGuid()
            );

            _serviceManagerMock.Setup(s => s.User.ListenToSong(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            )).ThrowsAsync(new NotFoundException(Resource.UserNotFound));

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await _listenToSongHandler.Handle(command, CancellationToken.None));
            Assert.That(ex.Message, Is.EqualTo(Resource.UserNotFound));
        }

        [Test]
        public void Handle_ListenToSong_When_SongNotFound()
        {
            // Arrange
            var command = new ListenToSongCommand(
                songId: Guid.NewGuid(),
                userId: Guid.NewGuid()
            );

            _serviceManagerMock.Setup(s => s.User.ListenToSong(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            )).ThrowsAsync(new NotFoundException(Resource.SongNotFound));

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await _listenToSongHandler.Handle(command, CancellationToken.None));
            Assert.That(ex.Message, Is.EqualTo(Resource.SongNotFound));
        }

        [Test]
        public void Handle_ListenToSong_When_ThrowException()
        {
            // Arrange
            var command = new ListenToSongCommand(
                songId: Guid.NewGuid(),
                userId: Guid.NewGuid()
            );

            _serviceManagerMock.Setup(s => s.User.ListenToSong(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            )).ThrowsAsync(new Exception("Error Occured"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => await _listenToSongHandler.Handle(command, CancellationToken.None));
            Assert.That(ex.Message, Is.EqualTo("Error Occured"));
        }
    }
}
