using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;

namespace Cpb.Database;

public static class Extensions
{
    public static IQueryable<T> ExcludeDeleted<T>(this IQueryable<T> query) where T : DbEntity =>
        query.Where(u => u.DeleteDate == null);

    public static async Task<ImmutableList<T>> ToImmutableListAsync<T>(this IQueryable<T> query)
    {
        var listed = await query.ToListAsync();
        return listed.ToImmutableList();
    }
}