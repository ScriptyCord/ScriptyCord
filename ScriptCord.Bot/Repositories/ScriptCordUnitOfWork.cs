using MicroOrm.Dapper.Repositories.DbContext;
using ScriptCord.Bot.Repositories.Playback;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Repositories
{
    public interface IScriptCordUnitOfWork : IDapperDbContext
    {
        IPlaylistRepository PlaylistRepository { get; }
    }

    public class ScriptCordUnitOfWork : DapperDbContext, IScriptCordUnitOfWork
    {
        public ScriptCordUnitOfWork(IDbConnection connection) : base(connection)
        {
            // Maybe should use PostgreSqlConnectionString instead of IDbConnection
        }

        private IPlaylistRepository _playlistRepository;
        public IPlaylistRepository PlaylistRepository => _playlistRepository ??= _playlistRepository = new PlaylistRepository(Connection);
    }
}
