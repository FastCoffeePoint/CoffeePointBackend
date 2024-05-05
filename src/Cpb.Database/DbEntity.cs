namespace Cpb.Database;

public class DbEntity
{
    public DateTimeOffset CreateDate { get; set; }
    public DateTimeOffset? DeleteDate { get; set; }

    public bool IsDeleted => DeleteDate != null;
}

public static class DbEntityExtensions
{
    public static T MarkCreated<T>(this T entity) where T : DbEntity
    {
        entity.CreateDate = DateTimeOffset.Now;
        return entity;
    }
    
    public static T MarkDeleted<T>(this T entity) where T : DbEntity
    {
        entity.DeleteDate = DateTimeOffset.Now;
        return entity;
    }
}