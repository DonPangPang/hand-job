using System.Linq.Dynamic.Core;
using HandJob.Domain.Base;
using HandJob.Domain.Responses;
using HandJob.Domain.ViewModels;

namespace HandJob.WebApi.Extensions;

public static class IQueryableExtensions
{
    /// <summary>
    /// 进行排序
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    /// <param name="source">  </param>
    /// <param name="orderBy"> </param>
    /// <returns> </returns>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> source, string orderBy) where T : class
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return source;
        }

        var orderByAfterSplit = orderBy.Split(",");

        foreach (var orderByClause in orderByAfterSplit.Reverse())
        {
            var trimedOrderByClause = orderByClause.Trim();

            var orderDescending = trimedOrderByClause.EndsWith(" desc");

            var indexOfFirstSpace = trimedOrderByClause.IndexOf(" ", StringComparison.Ordinal);

            var propertyName = indexOfFirstSpace == -1
                ? trimedOrderByClause
                : trimedOrderByClause.Remove(indexOfFirstSpace);

            source = source.OrderBy(propertyName
                                    + (orderDescending ? " descending" : " ascending"));
        }

        return source;
    }

    /// <summary>
    /// 分页
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    /// <param name="source"> </param>
    /// <param name="paging"> </param>
    /// <returns> </returns>
    public static async Task<VPagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, IPaging paging) where T : class
    {
        return await PagedList<T>.CreateAsync(source, paging.Page, paging.PageSize);
    }

    /// <summary>
    /// 排序并分页
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    /// <param name="source">     </param>
    /// <param name="parameters"> </param>
    /// <returns> </returns>
    public static async Task<VPagedList<T>> QueryAsync<T>(this IQueryable<T> source, IParameterBase parameters) where T : class
    {
        if (parameters is ISorting sorting)
            source = source.ApplySort(sorting.OrderBy ?? "");

        if (parameters is IPaging paging && paging.PageSize > 0)
            return await source.ToPagedListAsync(paging);
        else
            return await PagedList<T>.CreateAsync(source, 1, 999);
    }

    /// <summary>
    /// 转为树形结构
    /// </summary>
    /// <param name="data"> </param>
    /// <typeparam name="T"> </typeparam>
    /// <returns> </returns>
    public static IEnumerable<T> ToTree<T>(this IEnumerable<T> data) where T : ITree<T>
    {
        var tree = new List<T>();
        foreach (var item in data)
        {
            if (item.ParentId == null || item.ParentId == Guid.Empty)
            {
                item.Level = 0;
                tree.Add(item);
            }
            //else
            //{
            //    var parent = data.FirstOrDefault(x => x.Id == item.ParentId);
            //    if (parent != null)
            //    {
            //        if (parent.Children == null)
            //        {
            //            parent.Children = new List<T>();
            //        }
            //        parent.Children.Append(item);
            //    }
            //}

            foreach (var it in data)
            {
                if (it.ParentId == item.Id)
                {
                    if (item.Children == null)
                    {
                        item.Children = new List<T>();
                    }
                    it.Level = item.Level + 1;
                    item.Children.Add(it);
                }
            }
        }
        return tree;
    }
}