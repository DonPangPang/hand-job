using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using HandJob.Domain.Base;

namespace HandJob.WebApi.Extensions;

public static class EfExtensions
{
    public static IQueryable<T> FilterCurrentUser<T>(this IQueryable<T> query, Guid userId)
    {
        if (query is IQueryable<ICreator> temp)
        {
            temp.Where(x => x.CreatorId == userId);

            return (temp as IQueryable<T>)!;
        }

        return query;
    }

    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool @if, Expression<Func<T, bool>> exp)
    {
        return @if ? query.Where(exp) : query;
    }
}
