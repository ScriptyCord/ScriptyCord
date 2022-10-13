using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using ScriptCord.Bot;
using ScriptCord.Bot.Models.Playback;
using ScriptCord.Bot.Repositories.Playback;
using ScriptCord.Bot.Services.Playback;
using ScriptyCord.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ScriptyCord.Tests.Services.Playback
{
    public class PlaylistServiceTests
    {
        [Fact]
        [Trait("Services", "PlaylistService")]
        public async Task PlaylistService_fail_on_rename_playlist_as_unprivileged_user()
        {
            // Arrange
            Mock<ILoggerFacade<IPlaylistService>> mockLogger = new Mock<ILoggerFacade<IPlaylistService>>();
            Mock<IPlaylistRepository> mockPlaylistRepository = new Mock<IPlaylistRepository>();
            mockPlaylistRepository
                .Setup(x => x.CountAsync(It.IsAny<Expression<Func<Playlist, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);
            mockPlaylistRepository
                .Setup(x => x.GetSingleAsync(It.IsAny<Expression<Func<Playlist, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Playlist
                {
                    GuildId = 1,
                    Name = "old",
                    IsDefault = false,
                    AdminOnly = true
                });
            Mock<IPlaylistEntriesRepository> mockPlaylistEntriesRepository = new Mock<IPlaylistEntriesRepository>();
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(MockConfiguration.DefaultConfiguration)
                .Build();

            IPlaylistService service = new PlaylistService(
                mockLogger.Object,
                mockPlaylistRepository.Object,
                null,
                configuration,
                null,
                null
            );

            // Act
            var result = await service.RenamePlaylist(1, "old", "new", false);

            // Assert
            result.IsSuccess.Should().Be(false);
            result.Error.Should().BeEquivalentTo("You must be an admin in order to perform this action.");
        }

        [Fact]
        [Trait("Services", "PlaylistService")]
        public async Task PlaylistService_fail_on_playlist_with_given_name_taken_in_guild()
        {
            // Arrange
            Mock<ILoggerFacade<IPlaylistService>> mockLogger = new Mock<ILoggerFacade<IPlaylistService>>();
            Mock<IPlaylistRepository> mockPlaylistRepository = new Mock<IPlaylistRepository>();
            mockPlaylistRepository
                .Setup(x => x.CountAsync(It.IsAny<Expression<Func<Playlist, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            Mock<IPlaylistEntriesRepository> mockPlaylistEntriesRepository = new Mock<IPlaylistEntriesRepository>();
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(MockConfiguration.DefaultConfiguration)
                .Build();

            IPlaylistService service = new PlaylistService(
                mockLogger.Object,
                mockPlaylistRepository.Object,
                null,
                configuration,
                null,
                null
            );

            // Act
            var result = await service.RenamePlaylist(1, "old", "new", false);

            // Assert
            result.IsSuccess.Should().Be(false);
            result.Error.Should().BeEquivalentTo("A playlist with the chosen name already exists in this server!");
        }

        [Fact]
        [Trait("Services", "PlaylistService")]
        public async Task PlaylistService_fail_on_remove_playlist_as_unprivileged_user()
        {
            // Arrange
            Mock<ILoggerFacade<IPlaylistService>> mockLogger = new Mock<ILoggerFacade<IPlaylistService>>();
            Mock<IPlaylistRepository> mockPlaylistRepository = new Mock<IPlaylistRepository>();
            mockPlaylistRepository
                .Setup(x => x.GetSingleAsync(It.IsAny<Expression<Func<Playlist, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Playlist
                {
                    GuildId = 1,
                    Name = "playlist",
                    IsDefault = false,
                    AdminOnly = true
                });
            Mock<IPlaylistEntriesRepository> mockPlaylistEntriesRepository = new Mock<IPlaylistEntriesRepository>();
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(MockConfiguration.DefaultConfiguration)
                .Build();

            IPlaylistService service = new PlaylistService(
                mockLogger.Object,
                mockPlaylistRepository.Object,
                null,
                configuration,
                null,
                null
            );

            // Act
            var result = await service.RemovePlaylist(1, "playlist", false);

            // Assert
            result.IsSuccess.Should().Be(false);
            result.Error.Should().BeEquivalentTo("Only an admin can remove this playlist");
        }

        [Fact]
        [Trait("Services", "PlaylistService")]
        public async Task PlaylistService_fail_read_admin_playlist_details_as_unprivileged_user()
        {
            // Arrange
            Mock<ILoggerFacade<IPlaylistService>> mockLogger = new Mock<ILoggerFacade<IPlaylistService>>();
            Mock<IPlaylistRepository> mockPlaylistRepository = new Mock<IPlaylistRepository>();
            mockPlaylistRepository
                .Setup(x => x.GetSingleAsync(It.IsAny<Expression<Func<Playlist, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Playlist
                {
                    GuildId = 1,
                    Name = "playlist",
                    IsDefault = false,
                    AdminOnly = true
                });
            Mock<IPlaylistEntriesRepository> mockPlaylistEntriesRepository = new Mock<IPlaylistEntriesRepository>();
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(MockConfiguration.DefaultConfiguration)
                .Build();

            IPlaylistService service = new PlaylistService(
                mockLogger.Object,
                mockPlaylistRepository.Object,
                null,
                configuration,
                null,
                null
            );

            // Act
            var result = await service.GetPlaylistDetails(1, "playlist", false);

            // Assert
            result.IsSuccess.Should().Be(false);
            result.Error.Should().BeEquivalentTo("Only a guild administrator can access information about this playlist!");
        }

        [Fact]
        [Trait("Services", "PlaylistService")]
        public async Task PlaylistService_change_default_playlist_on_removal_of_default_playlist()
        {
            // Arrange
            Mock<ILoggerFacade<IPlaylistService>> mockLogger = new Mock<ILoggerFacade<IPlaylistService>>();
            Mock<IPlaylistRepository> mockPlaylistRepository = new Mock<IPlaylistRepository>();
            IList<Playlist> playlists = new List<Playlist>
            {
                new Playlist
                {
                    GuildId = 1,
                    Name = "playlist",
                    IsDefault = true,
                    AdminOnly = false,
                    PlaylistEntries = new List<PlaylistEntry>()
                },
                new Playlist
                {
                    GuildId = 1,
                    Name = "playlist 2",
                    IsDefault = false,
                }
            };
            mockPlaylistRepository
                .Setup(x => x.GetSingleAsync(It.IsAny<Expression<Func<Playlist, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(playlists[0]);
            mockPlaylistRepository
                .Setup(x => x.GetFirstAsync(It.IsAny<Expression<Func<Playlist, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(playlists[1]);
            mockPlaylistRepository.Setup(x => x.DeleteAsync(It.IsAny<Playlist>(), It.IsAny<CancellationToken>()))
                .Callback(() => playlists.RemoveAt(0))
                .ReturnsAsync(Result.Success());
            Mock<IPlaylistEntriesRepository> mockPlaylistEntriesRepository = new Mock<IPlaylistEntriesRepository>();
            mockPlaylistEntriesRepository.Setup(x => x.UpdateAsync(It.IsAny<PlaylistEntry>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());
            mockPlaylistEntriesRepository.Setup(x => x.DeleteManyAsync(It.IsAny<Expression<Func<PlaylistEntry, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(MockConfiguration.DefaultConfiguration)
                .Build();

            IPlaylistService service = new PlaylistService(
                mockLogger.Object,
                mockPlaylistRepository.Object,
                mockPlaylistEntriesRepository.Object,
                configuration,
                null,
                null
            );

            // Act
            var result = await service.RemovePlaylist(1, "playlist", false);

            // Assert
            result.IsSuccess.Should().Be(true);
            playlists[0].IsDefault.Should().Be(true);
        }
    }
}
