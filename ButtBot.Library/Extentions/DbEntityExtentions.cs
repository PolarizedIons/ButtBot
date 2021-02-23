using System;
using ButtBot.Library.Models.Database;

namespace ButtBot.Library.Extentions
{
    public static class DbEntityExtentions
    {
        public static T MarkDeleted<T>(this T entity) where T : DbEntity
        {
            entity.DeletedAt = DateTime.UtcNow;
            return entity;
        }
    }
}
