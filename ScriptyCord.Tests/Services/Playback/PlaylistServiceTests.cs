using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using ScriptCord.Bot;
using ScriptCord.Bot.Repositories.Playback;
using ScriptCord.Bot.Services.Playback;
using ScriptCord.Bot.Workers.Playback;
using ScriptyCord.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ScriptyCord.Tests.Services.Playback
{
    public class PlaylistServiceTests
    {
        [Fact]
        [Trait("Playback", "PlaylistService")]
        public void PlaylistService_read_currently_playing_metadata_correctly()
        {
            // The below test is not ready. It's just a template for future reference.

            // Arrange
            Mock<ILoggerFacade<IPlaylistService>> mockLogger = new Mock<ILoggerFacade<IPlaylistService>>();
            Mock<IPlaylistRepository> mockPlaylistRepository = new Mock<IPlaylistRepository>();
            Mock<IPlaylistEntriesRepository> mockPlaylistEntriesRepository = new Mock<IPlaylistEntriesRepository>();
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(MockConfiguration.DefaultConfiguration)
                .Build();

            Mock<PlaybackWorker> mockPlaybackWorker = new Mock<PlaybackWorker>(new Mock<ILoggerFacade<PlaybackWorker>>().Object, configuration, null);
            IPlaylistService playlistService = new PlaylistService(
                mockLogger.Object,
                mockPlaylistRepository.Object,
                mockPlaylistEntriesRepository.Object,
                configuration,
                mockPlaybackWorker.Object,
                null
            );

            // Act
            // ...
            Result result = Result.Success();

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }
}
