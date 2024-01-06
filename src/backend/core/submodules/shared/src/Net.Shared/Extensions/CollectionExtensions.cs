using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Net.Shared.Extensions;

public static class CollectionExtensions
{
    #region Pagination

    private const int MaxPaginatedResult = 500;

    public static IEnumerable<T> PaginateAsc<T, TSelector>(this IEnumerable<T> collection, int page, int limit, Func<T, TSelector> selector)
    {
        page = SetPaginatePage(page);
        limit = SetPaginateLimit(limit);
        return collection.OrderBy(selector).Skip((page - 1) * limit).Take(limit);
    }
    public static IEnumerable<T> PaginateDesc<T, TSelector>(this IEnumerable<T> collection, int page, int limit, Func<T, TSelector> selector)
    {
        page = SetPaginatePage(page);
        limit = SetPaginateLimit(limit);
        return collection.OrderByDescending(selector).Skip((page - 1) * limit).Take(limit);
    }

    public static IQueryable<T> PaginateAsc<T, TSelector>(this IQueryable<T> collection, int page, int limit, Expression<Func<T, TSelector>> selector)
    {
        page = SetPaginatePage(page);
        limit = SetPaginateLimit(limit);
        return collection.OrderBy(selector).Skip((page - 1) * limit).Take(limit);
    }
    public static IQueryable<T> PaginateDesc<T, TSelector>(this IQueryable<T> collection, int page, int limit, Expression<Func<T, TSelector>> selector)
    {
        page = SetPaginatePage(page);
        limit = SetPaginateLimit(limit);
        return collection.OrderByDescending(selector).Skip((page - 1) * limit).Take(limit);
    }

    private static int SetPaginatePage(int page) => page <= 0 ? 1 : page >= int.MaxValue ? int.MaxValue : page;
    private static int SetPaginateLimit(int limit) => limit <= 0 ? MaxPaginatedResult : limit > MaxPaginatedResult ? limit == int.MaxValue ? int.MaxValue : MaxPaginatedResult : limit;

    #endregion
}
