using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HandJob.Domain.Base;
using HandJob.WebApi.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;

namespace HandJob.WebApi.Data;

public class HandJobDbContext : DbContext
{
    public HandJobDbContext(DbContextOptions<HandJobDbContext> options) : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var types = typeof(IEntity).Assembly.GetTypes().AsEnumerable()
                .Where(x => !x.IsInterface && !x.IsAbstract && x.IsAssignableTo(typeof(IEntity)));

        foreach (var type in types)
        {
            if (modelBuilder.Model.FindEntityType(type) is null)
            {
                modelBuilder.Model.AddEntityType(type);
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}
