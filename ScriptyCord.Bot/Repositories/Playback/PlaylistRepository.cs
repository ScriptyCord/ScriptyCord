using NHibernate;
using ScriptCord.Bot.Models.Playback;
using ScriptCord.Core.Persistency;
using System.Data;

namespace ScriptCord.Bot.Repositories.Playback
{
    public interface IPlaylistRepository : IRepository<Playlist>
    {
    }

    public class PlaylistRepository : PostgreBaseRepository<Playlist>, IPlaylistRepository
    {
        public PlaylistRepository(ISessionFactory sessionFactory) : base(sessionFactory) {}
    }
}
