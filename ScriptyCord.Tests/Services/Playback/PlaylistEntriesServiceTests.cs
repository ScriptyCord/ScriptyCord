using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using ScriptCord.Bot;
using ScriptCord.Bot.Repositories.Playback;
using ScriptCord.Bot.Services.Playback;
using ScriptCord.Bot.Workers.Playback;
using ScriptyCord.Tests.Helpers;
using Xunit;

namespace ScriptyCord.Tests.Services.Playback
{
    public class PlaylistEntriesServiceTests
    {
        //[Fact]
        //[Trait("Services", "PlaylistEntriesService")]
        //public void PlaylistService_add_two_entries_at_once()
        //{
        //    // The below test is not ready. It's just a template for future reference.

        //    // Arrange
        //    Mock<ILoggerFacade<IPlaylistEntriesService>> mockLogger = new Mock<ILoggerFacade<IPlaylistEntriesService>>();
        //    Mock<IPlaylistRepository> mockPlaylistRepository = new Mock<IPlaylistRepository>();
        //    Mock<IPlaylistEntriesRepository> mockPlaylistEntriesRepository = new Mock<IPlaylistEntriesRepository>();
        //    IConfiguration configuration = new ConfigurationBuilder()
        //        .AddInMemoryCollection(MockConfiguration.DefaultConfiguration)
        //        .Build();

        //    Mock<PlaybackWorker> mockPlaybackWorker = new Mock<PlaybackWorker>(new Mock<ILoggerFacade<IPlaybackWorker>>().Object, configuration, null);
        //    IPlaylistEntriesService playlistEntriesService = new PlaylistEntriesService(
        //        mockLogger.Object,
        //        mockPlaylistRepository.Object,
        //        mockPlaylistEntriesRepository.Object,
        //        configuration,
        //        null
        //    );
        //    ulong guildId = 0;

        //    // Act
        //    // ...
        //    var result1 = playlistEntriesService.AddEntryFromUrlToPlaylistByName(guildId, "default", "https://youtu.be/kpwNjdEPz7E", false);
        //    var result2 = playlistEntriesService.AddEntryFromUrlToPlaylistByName(guildId, "default", "https://youtu.be/2ZIpFytCSVc", false);

        //    Result result = Result.Success();

        //    // Assert
        //    result.IsSuccess.Should().BeTrue();
        //}
    }
}
