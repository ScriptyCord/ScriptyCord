using CSharpFunctionalExtensions;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Core.Persistency
{
    public class PostgreBaseRepository<TEntity> : IRepository<TEntity> where TEntity : Entity
    {
        protected readonly ISession _session;

        public PostgreBaseRepository(ISession session)
            => _session = session;

        public async Task<Result> SaveAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            using(var transaction = _session.BeginTransaction())
            {
                try
                {
                    await _session.SaveOrUpdateAsync(entity);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result.Failure(e.Message);
                }
                
            }
            return Result.Success();
        }

        public async Task<Result<IEnumerable<TEntity>>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<TEntity> entities = null;
            try
            {
                entities = await _session.Query<TEntity>().ToListAsync();
            }
            catch (Exception e)
            {
                return Result.Failure<IEnumerable<TEntity>>(e.Message);
            }

            return Result.Success(entities);
        }

        public async Task<Result<IEnumerable<TEntity>>> GetFiltered(Func<TEntity, bool> filters, CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<TEntity> entities = null;
            try
            {
                entities = _session.Query<TEntity>().Where(filters);
            }
            catch (Exception e)
            {
                return Result.Failure<IEnumerable<TEntity>>(e.Message);
            }

            return Result.Success(entities);
        }

        public async Task<Result<TEntity>> GetSingleAsync(Expression<Func<TEntity, bool>> filters, CancellationToken cancellationToken = default(CancellationToken))
        {
            TEntity entity = null;
            try
            {
                entity = await _session.Query<TEntity>().SingleAsync(filters, cancellationToken);
            }
            catch (Exception e)
            {
                return Result.Failure<TEntity>(e.Message);
            }

            return Result.Success(entity);

        }
        public async Task<Result<TEntity>> GetFirstAsync(Expression<Func<TEntity, bool>> filters, CancellationToken cancellationToken = default(CancellationToken))
        {
            TEntity entity = null;
            try
            {
                entity = await _session.Query<TEntity>().FirstAsync(filters, cancellationToken);
            }
            catch (Exception e)
            {
                return Result.Failure<TEntity>(e.Message);
            }

            return Result.Success(entity);
        }

        public async Task<Result<int>> CountAsync(Expression<Func<TEntity, bool>> filters, CancellationToken cancellationToken = default(CancellationToken))
        {
            int count = -1;
            try
            {
                count = await _session.Query<TEntity>().CountAsync(filters, cancellationToken);
            }
            catch (Exception e)
            {
                return Result.Failure<int>(e.Message);
            }

            return Result.Success(count);
        }

        public async Task<Result> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            using(var transaction = _session.BeginTransaction())
            {
                try
                {
                    await _session.UpdateAsync(entity);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result.Failure(e.Message);
                }
            }
            return Result.Success();
        }

        public async Task<Result> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            using (var transaction = _session.BeginTransaction())
            {
                try
                {
                    await _session.DeleteAsync(entity, cancellationToken);
                    //_session.Query<TEntity>().Where(x => true).DeleteAsync(cancellationToken);
                    await transaction.CommitAsync();
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result.Failure(e.Message);
                }
                
            }
            return Result.Success();
        }

        public async Task<Result> DeleteManyAsync(Expression<Func<TEntity, bool>> filters, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var transaction = _session.BeginTransaction())
            {
                try
                {
                    await _session.Query<TEntity>().Where(filters).DeleteAsync(cancellationToken);
                    await transaction.CommitAsync();
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result.Failure(e.Message);
                }

            }
            return Result.Success();
        }

        public async Task<Result> TransactionAsync(Action action, CancellationToken cancellationToken = default)
        {
            using (var transaction = _session.BeginTransaction())
            {
                try
                {
                    action();
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result.Failure(e.Message);
                }
            }
            return Result.Success();
        }
    }
}
