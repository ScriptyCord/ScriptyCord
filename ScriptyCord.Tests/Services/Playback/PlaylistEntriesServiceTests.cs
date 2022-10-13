using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using ScriptCord.Bot;
using ScriptCord.Bot.Models.Playback;
using ScriptCord.Bot.Repositories.Playback;
using ScriptCord.Bot.Services.Playback;
using ScriptCord.Bot.Workers.Playback;
using ScriptyCord.Tests.Helpers;
using System.Linq.Expressions;
using Xunit;

namespace ScriptyCord.Tests.Services.Playback
{
    public class PlaylistEntriesServiceTests
    {
        [Fact]
        [Trait("Services", "PlaylistEntriesService")]
        public async Task PlaylistService_fail_on_add_entry_as_unprivileged_user()
        {
            // Arrange
            Mock<ILoggerFacade<IPlaylistEntriesService>> mockLogger = new Mock<ILoggerFacade<IPlaylistEntriesService>>();
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

            IPlaylistEntriesService playlistEntriesService = new PlaylistEntriesService(
                mockLogger.Object,
                mockPlaylistRepository.Object,
                mockPlaylistEntriesRepository.Object,
                configuration,
                null
            );
            ulong guildId = 0;

            // Act
            var result = await playlistEntriesService.AddEntryFromUrlToPlaylistByName(guildId, "playlist", "https://youtu.be/kpwNjdEPz7E", false);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().BeEquivalentTo("You must be the administrator of the guild to add an entry to this playlist");
        }

        [Fact]
        [Trait("Services", "PlaylistEntriesService")]
        public async Task PlaylistService_fail_on_add_entries_from_remote_playlist_as_unprivileged_user()
        {
            // Arrange
            Mock<ILoggerFacade<IPlaylistEntriesService>> mockLogger = new Mock<ILoggerFacade<IPlaylistEntriesService>>();
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

            IPlaylistEntriesService playlistEntriesService = new PlaylistEntriesService(
                mockLogger.Object,
                mockPlaylistRepository.Object,
                mockPlaylistEntriesRepository.Object,
                configuration,
                null
            );
            ulong guildId = 0;

            // Act
            var result = await playlistEntriesService.AddEntriesFromPlaylistUrl(guildId, "playlist", "https://youtu.be/kpwNjdEPz7E", (x,y,z) => { }, (x, y, z) => { }, false);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().BeEquivalentTo("You must be the administrator of the guild to add an entry to this playlist");
        }

        [Fact]
        [Trait("Services", "PlaylistEntriesService")]
        public async Task PlaylistService_fail_on_remove_entry_from_remote_playlist_as_unprivileged_user()
        {
            // Arrange
            Mock<ILoggerFacade<IPlaylistEntriesService>> mockLogger = new Mock<ILoggerFacade<IPlaylistEntriesService>>();
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

            IPlaylistEntriesService playlistEntriesService = new PlaylistEntriesService(
                mockLogger.Object,
                mockPlaylistRepository.Object,
                mockPlaylistEntriesRepository.Object,
                configuration,
                null
            );
            ulong guildId = 0;

            // Act
            var result = await playlistEntriesService.RemoveEntryFromPlaylistByName(guildId, "playlist", "https://youtu.be/kpwNjdEPz7E", false);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().BeEquivalentTo("You must be the administrator of the guild to remove an entry from this playlist");
        }
    }
}
