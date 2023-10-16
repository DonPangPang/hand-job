using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HandJob.Domain.Base;
using HandJob.WebApi.Data;
using Microsoft.EntityFrameworkCore;

namespace HandJob.WebApi.Services;

public interface IUnitOfWork
{
    DbContext Db { get; }
    IQueryable<T> Query<T>() where T : class, IEntity;
    Task<bool> CommitAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly HandJobDbContext _dbContext;

    public DbContext Db { get; }

    public UnitOfWork(HandJobDbContext dbContext)
    {
        _dbContext = dbContext;
        Db = _dbContext;
    }

    public IQueryable<T> Query<T>() where T : class, IEntity
    {
        return _dbContext.Set<T>();
    }

    public async Task<bool> CommitAsync()
    {
        return await _dbContext.SaveChangesAsync() > 0;
    }
}
