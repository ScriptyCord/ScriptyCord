using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Core.Persistency
{
    public interface IRepository<TEntity> where TEntity : Entity
    {
        Task<Result> SaveAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));
        Task<Result<IEnumerable<TEntity>>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<Result<IEnumerable<TEntity>>> GetFiltered(Func<TEntity, bool> filters, CancellationToken cancellationToken = default(CancellationToken));
        Task<Result<TEntity>> GetSingleAsync(Expression<Func<TEntity, bool>> filters, CancellationToken cancellationToken = default(CancellationToken));
        Task<Result<TEntity>> GetFirstAsync(Expression<Func<TEntity, bool>> filters, CancellationToken cancellationToken = default(CancellationToken));
        Task<Result<int>> CountAsync(Expression<Func<TEntity, bool>> filters, CancellationToken cancellationToken = default(CancellationToken));
        Task<Result> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));
        Task<Result> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));
        Task<Result> DeleteManyAsync(Expression<Func<TEntity, bool>> filters, CancellationToken cancellationToken = default(CancellationToken));
        Task<Result> TransactionAsync(Action action, CancellationToken cancellationToken = default(CancellationToken));
    }
}
