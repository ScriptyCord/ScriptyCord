using NHibernate;
using ScriptCord.Bot.Models.Playback;
using ScriptCord.Core.Persistency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Repositories.Playback
{
    public interface IPlaylistEntriesRepository : IRepository<PlaylistEntry>
    {}

    public class PlaylistEntriesRepository : PostgreBaseRepository<PlaylistEntry>, IPlaylistEntriesRepository
    {
        public PlaylistEntriesRepository(ISessionFactory sessionFactory) : base(sessionFactory) {}
    }
}
