using MicroOrm.Dapper.Repositories;
using ScriptCord.Bot.Models.Playback;
using System.Data;

namespace ScriptCord.Bot.Repositories.Playback
{
    public interface IPlaylistRepository : IDapperRepository<Playlist>
    {
    }

    public class PlaylistRepository : DapperRepository<Playlist>, IPlaylistRepository
    {
        public PlaylistRepository(IDbConnection connection) : base(connection) {}
    }
}
