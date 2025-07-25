using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Project.DAL;
using Project.DTO.Common;
using System.Linq.Expressions;

namespace Project.BLL.Connection
{
    public class RepoDataBase : IDisposable
    {
        readonly DataBaseContext db;
        private readonly IHttpContextAccessor _context;

        public RepoDataBase(IConfiguration Config, IHttpContextAccessor context)
        {
            db = new DataBaseContext(Config, context);
            _context = context;

        }

        public DatabaseFacade GetInfo()
        {
            return db.Database;
        }

        public T Find<T>(object Id) where T : TableBase
        {
            try
            {
                return db.Set<T>().Find(Id);
            }
            finally { }
        }

        public async Task<T> FindAsync<T>(object Id) where T : TableBase
        {
            try
            {
                return await db.Set<T>().FindAsync(Id);
            }
            finally { }
        }


        public IQueryable<T> Get<T>(Expression<Func<T, bool>> where = null, params Expression<Func<T, object>>[] includes) where T : TableBase
        {
            try
            {
                IQueryable<T> query = db.Set<T>();
                foreach (var includeProperty in includes) { query = query.Include(includeProperty); }
                return where == null ? query : query.Where(where);
            }
            finally { }
        }



        public IQueryable<T> GetNoEntity<T>(Expression<Func<T, bool>> where = null, params Expression<Func<T, object>>[] includes) where T : BaseNoEntity
        {
            try
            {
                IQueryable<T> query = db.Set<T>();
                foreach (var includeProperty in includes) { query = query.Include(includeProperty); }
                return where == null ? query : query.Where(where);
            }
            finally { }
        }




        public int Add<T>(T entity, bool trace = true) where T : TableBase
        {
            try
            {
                if (trace)
                {
                    entity.DATA_CRIACAO = entity.DATA_CRIACAO == DateTimeOffset.MinValue ? DateTimeOffset.UtcNow : entity.DATA_CRIACAO;
                    entity.USUARIO_CRIACAO ??= _context?.HttpContext?.User?.Identity?.GetUserId() ?? "svc_apl_api";

                    entity.DATA_MODIFICACAO = DateTimeOffset.UtcNow;
                    entity.USUARIO_MODIFICACAO = _context?.HttpContext?.User?.Identity?.GetUserId() ?? "svc_apl_api";
                }

                entity.ID = entity.ID == Guid.Empty ? Guid.NewGuid() : entity.ID;
                entity.ATIVO = true;
                db.Set<T>().Add(entity);
                return db.SaveChanges();
            }
            finally
            {

            }
        }


        public async Task<int> AddAsync<T>(T entity, bool trace = true) where T : TableBase
        {
            try
            {

                if (trace)
                {
                    entity.DATA_CRIACAO = entity.DATA_CRIACAO == DateTimeOffset.MinValue ? DateTimeOffset.UtcNow : entity.DATA_CRIACAO;
                    entity.USUARIO_CRIACAO ??= _context?.HttpContext?.User?.Identity?.GetUserId() ?? "svc_apl_api";

                    entity.DATA_MODIFICACAO = entity.DATA_CRIACAO;
                    entity.USUARIO_MODIFICACAO = entity.USUARIO_CRIACAO;
                }
                entity.ID = entity.ID == Guid.Empty ? Guid.NewGuid() : entity.ID;
                entity.ATIVO = true;

                db.Set<T>().Add(entity);
                return await db.SaveChangesAsync();
            }
            finally
            {

            }
        }


        public int Edit<T>(T entity, bool trace = true) where T : TableBase
        {
            try
            {

                if (trace)
                {
                    entity.DATA_MODIFICACAO = DateTimeOffset.UtcNow;
                    entity.USUARIO_MODIFICACAO ??= _context?.HttpContext?.User?.Identity?.GetUserId() ?? "svc_apl_api";
                }
                db.Entry(entity).State = EntityState.Modified;
                return db.SaveChanges();
            }
            finally
            {

            }
        }


        public int EditRange<T>(IQueryable<T> entityList, bool trace = true) where T : TableBase
        {
            try
            {
                foreach (var entity in entityList)
                {
                    if (trace)
                    {
                        entity.DATA_MODIFICACAO = DateTimeOffset.UtcNow;
                        entity.USUARIO_MODIFICACAO ??= _context?.HttpContext?.User?.Identity?.GetUserId() ?? "svc_apl_api";
                    }
                    db.Entry(entity).State = EntityState.Modified;
                }
                return db.SaveChanges();
            }
            finally { }
        }


        public async Task<int> EditAsync<T>(T entity, bool trace = true) where T : TableBase
        {
            try
            {
                if (trace)
                {
                    entity.DATA_MODIFICACAO = DateTimeOffset.UtcNow;
                    entity.USUARIO_MODIFICACAO ??= _context?.HttpContext?.User?.Identity?.GetUserId() ?? "svc_apl_api";
                }
                db.Entry(entity).State = EntityState.Modified;
                return await db.SaveChangesAsync();
            }
            finally { }
        }


        public int Patch<T>(JsonPatchDocument<T> model, T entity, bool trace = true) where T : TableBase
        {
            try
            {

                if (trace)
                {
                    entity.DATA_MODIFICACAO = DateTimeOffset.UtcNow;
                    entity.USUARIO_MODIFICACAO ??= _context?.HttpContext?.User?.Identity?.GetUserId() ?? "svc_apl_api";
                }


                model.ApplyTo(entity);
                return db.SaveChanges();
            }
            finally
            {

            }
        }


        public int Delete<T>(T entity) where T : TableBase
        {
            try
            {
                db.Entry(entity).State = EntityState.Deleted;
                return db.SaveChanges();
            }
            finally { }
        }

        public async Task<int> DeleteAsync<T>(T entity) where T : TableBase
        {
            try
            {
                db.Entry(entity).State = EntityState.Deleted;
                return await db.SaveChangesAsync();
            }
            finally { }
        }


        public int SetDeleted<T>(T entity, bool trace = true) where T : TableBase
        {
            try
            {

                entity.ATIVO = false;
                if (trace)
                {
                    entity.DATA_MODIFICACAO = DateTimeOffset.UtcNow;
                    entity.USUARIO_MODIFICACAO ??= _context?.HttpContext?.User?.Identity?.GetUserId() ?? "svc_apl_api";
                }
                db.Entry(entity).State = EntityState.Modified;
                return db.SaveChanges();
            }
            finally { }
        }


        public async Task<int> SetDeletedAsync<T>(T entity, bool trace = true) where T : TableBase
        {
            try
            {
                entity.ATIVO = false;
                if (trace)
                {
                    entity.DATA_MODIFICACAO = DateTimeOffset.UtcNow;
                    entity.USUARIO_MODIFICACAO ??= _context?.HttpContext?.User?.Identity?.GetUserId() ?? "svc_apl_api";
                }
                db.Entry(entity).State = EntityState.Modified;
                return await db.SaveChangesAsync();
            }
            finally { }
        }


        public void Dispose()
        {
            db.Dispose();
        }
    }

    public static class LinqExtensions
    {
        public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool condition, Expression<Func<TSource, bool>> predicate)
        {
            if (!condition) return source;
            return source.Where(predicate);
        }


    }
}
