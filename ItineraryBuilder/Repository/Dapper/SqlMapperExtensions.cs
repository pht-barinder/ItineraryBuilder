using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ItineraryBuilder.Repository.Dapper
{
    public static class SqlMapperExtensions
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> KeyProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> TypeProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> GetQueries = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> TypeTableName = new ConcurrentDictionary<RuntimeTypeHandle, string>();

        public static IEnumerable<TFirst> Map<TFirst, TSecond, TKey>(this SqlMapper.GridReader reader, Func<TFirst, TKey> firstKey, Func<TSecond, TKey> secondKey, Action<TFirst, IEnumerable<TSecond>> addChildren)
        {
            var first = reader.Read<TFirst>().ToList();
            var childMap = reader
                .Read<TSecond>()
                .GroupBy(s => secondKey(s))
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            foreach (var item in first)
            {
                IEnumerable<TSecond> children;
                if (childMap.TryGetValue(firstKey(item), out children))
                {
                    addChildren(item, children);
                }
            }

            return first;
        }

        private static IEnumerable<PropertyInfo> KeyPropertiesCache(Type type)
        {
            if (KeyProperties.ContainsKey(type.TypeHandle))
            {
                return KeyProperties[type.TypeHandle];
            }

            var allProperties = TypePropertiesCache(type);
            var keyProperties = allProperties.Where(p => p.GetCustomAttributes(true).Any(a => a is KeyAttribute)).ToList();

            if (keyProperties.Count == 0)
            {
                var idProp = allProperties.Where(p => p.Name.ToLower() == "id").FirstOrDefault();
                if (idProp != null)
                {
                    keyProperties.Add(idProp);
                }
            }

            KeyProperties[type.TypeHandle] = keyProperties;
            return keyProperties;
        }

        private static IEnumerable<PropertyInfo> TypePropertiesCache(Type type)
        {
            if (TypeProperties.ContainsKey(type.TypeHandle))
            {
                return TypeProperties[type.TypeHandle];
            }

            var properties = type.GetProperties();
            TypeProperties[type.TypeHandle] = properties;
            return properties;
        }

        private static IEnumerable<PropertyInfo> IgnorePropertiesCache(Type type)
        {
            if (TypeProperties.ContainsKey(type.TypeHandle))
            {
                return TypeProperties[type.TypeHandle].Where(p => p.GetCustomAttributes(true).Any(a => a is IgnoreMappingAttribute)).ToList();
            }

            var properties = type.GetProperties().Where(p => p.GetCustomAttributes(true).Any(a => a is IgnoreMappingAttribute)).ToList();
            TypeProperties[type.TypeHandle] = properties;
            return properties;
        }

        /// <summary>
        /// Returns all the properties excluding ignored properties
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            var allProperties = TypePropertiesCache(type);

            var ignoreProperties = IgnorePropertiesCache(type);

            if (ignoreProperties != null && ignoreProperties.Count() > 0)
            {
                foreach (PropertyInfo item in ignoreProperties)
                {
                    List<PropertyInfo> allprops = new List<PropertyInfo>();
                    allprops = allProperties.ToList();
                    PropertyInfo pinfo = allprops.ToList().Where(p => p.Name == item.Name).FirstOrDefault();
                    allprops.Remove(pinfo);
                    allProperties = allprops.AsEnumerable();
                }
            }
            return allProperties;
        }

        /// <summary>
        /// Returns a single entity by a single id from table "Ts". T must be of interface type.
        /// Id must be marked with [Key] attribute.
        /// Created entity is tracked/intercepted for changes and used by the Update() extension.
        /// </summary>
        /// <typeparam name="T">Interface type to create and populate</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="id">Id of the entity to get, must be marked with [Key] attribute</param>
        /// <returns>Entity of T</returns>
        public static T Get<T>(this IDbConnection connection, object id) where T : class
        {
            var type = typeof(T);
            string sql;
            if (!GetQueries.TryGetValue(type.TypeHandle, out sql))
            {
                var keys = KeyPropertiesCache(type);
                if (keys.Count() > 1)
                    throw new DataException("Get<T> only supports an entity with a single [Key] property");
                if (keys.Count() == 0)
                    throw new DataException("Get<T> only supports en entity with a [Key] property");

                var onlyKey = keys.First();

                var name = GetTableName(type);

                // TODO: pluralizer
                // TODO: query information schema and only select fields that are both in information schema and underlying class / interface
                sql = "select * from " + name + " where " + onlyKey.Name + " = @id";
                GetQueries[type.TypeHandle] = sql;
            }

            var dynParms = new DynamicParameters();
            dynParms.Add("@id", id);

            T obj = null;

            if (type.IsInterface)
            {
                var res = connection.Query(sql, dynParms).FirstOrDefault() as IDictionary<string, object>;

                if (res == null)
                    return (T)((object)null);

                obj = ProxyGenerator.GetInterfaceProxy<T>();

                foreach (var property in TypePropertiesCache(type))
                {
                    var val = res[property.Name];
                    property.SetValue(obj, val, null);
                }

                ((IProxy)obj).IsDirty = false;   //reset change tracking and return
            }
            else
            {
                obj = connection.Query<T>(sql, dynParms).FirstOrDefault();
            }
            return obj;
        }

        public static List<T> Get<T>(this IDbConnection connection) where T : class
        {
            var type = typeof(T);
            string sql;
            if (!GetQueries.TryGetValue(type.TypeHandle, out sql))
            {
                var keys = KeyPropertiesCache(type);
                if (keys.Count() > 1)
                    throw new DataException("Get<T> only supports an entity with a single [Key] property");
                if (keys.Count() == 0)
                    throw new DataException("Get<T> only supports en entity with a [Key] property");

                var onlyKey = keys.First();

                var name = GetTableName(type);

                sql = "select * from " + name;
                GetQueries[type.TypeHandle] = sql;
            }

            List<T> obj = null;

            obj = connection.Query<T>(sql).ToList();
            return obj;
        }

        private static string GetTableName(Type type)
        {
            string name;
            if (!TypeTableName.TryGetValue(type.TypeHandle, out name))
            {
                if (type.IsInterface && name.StartsWith("I"))
                {
                    name = name.Substring(1);
                }

                name = type.Name.ToLower();

                //NOTE: This as dynamic trick should be able to handle both our own Table-attribute as well as the one in EntityFramework
                var tableattr = type.GetCustomAttributes(false).Where(attr => attr.GetType().Name == "TableAttribute").SingleOrDefault() as
                    dynamic;
                if (tableattr != null)
                    name = tableattr.Name;
                TypeTableName[type.TypeHandle] = name;
            }
            return String.Format("`{0}`", name);
        }

        /// <summary>
        /// Inserts an entity into table "Ts" and returns identity id.
        /// </summary>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToInsert">Entity to insert</param>
        /// <returns>Identity of inserted entity</returns>
        public static long Insert<T>(this IDbConnection connection, T entityToInsert) where T : class
        {
            using (var tx = connection.BeginTransaction())
            {
                var type = typeof(T);

                var name = GetTableName(type);

                var sb = new StringBuilder(null);
                sb.AppendFormat("insert into {0} (", name);

                var allProperties = GetProperties(type);
                var keyProperties = KeyPropertiesCache(type);

                for (var i = 0; i < allProperties.Count(); i++)
                {
                    var property = allProperties.ElementAt(i);

                    if (keyProperties.Contains(property)) continue;

                    sb.Append("`" + property.Name + "`");
                    if (i < allProperties.Count() - 1)
                        sb.Append(", ");
                }
                sb.Append(") values (");
                for (var i = 0; i < allProperties.Count(); i++)
                {
                    var property = allProperties.ElementAt(i);
                    if (keyProperties.Contains(property)) continue;

                    sb.AppendFormat("@{0}", property.Name);
                    if (i < allProperties.Count() - 1)
                        sb.Append(", ");
                }
                sb.Append(") ");
                connection.Execute(sb.ToString(), entityToInsert);
                //NOTE: would prefer to use IDENT_CURRENT('tablename') or IDENT_SCOPE but these are not available on SQLCE
                //var r = connection.Query("select @@IDENTITY id");
                var r = connection.Query("select LAST_INSERT_ID() as id");
                tx.Commit();

                return (int)((dynamic)r.First()).id;
            }
        }

        /// <summary>
        /// Updates entity in table "Ts", checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <typeparam name="T">Type to be updated</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToUpdate">Entity to be updated</param>
        /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
        public static bool Update<T>(this IDbConnection connection, T entityToUpdate) where T : class
        {
            var proxy = entityToUpdate as IProxy;
            if (proxy != null)
            {
                if (!proxy.IsDirty) return false;
            }

            var type = typeof(T);

            var keyProperties = KeyPropertiesCache(type);
            if (keyProperties.Count() == 0)
                throw new ArgumentException("Entity must have at least one [Key] property");

            var name = GetTableName(type);

            var sb = new StringBuilder();
            sb.AppendFormat("update {0} set ", name);

            var allProperties = GetProperties(type);
            var nonIdProps = allProperties.Where(a => !keyProperties.Contains(a));

            for (var i = 0; i < nonIdProps.Count(); i++)
            {
                var property = nonIdProps.ElementAt(i);
                sb.AppendFormat("`{0}` = @{1}", property.Name, property.Name);
                if (i < nonIdProps.Count() - 1)
                    sb.AppendFormat(", ");
            }
            sb.Append(" where ");
            for (var i = 0; i < keyProperties.Count(); i++)
            {
                var property = keyProperties.ElementAt(i);
                sb.AppendFormat("`{0}` = @{1}", property.Name, property.Name);
                if (i < keyProperties.Count() - 1)
                    sb.AppendFormat(" and ");
            }
            var updated = connection.Execute(sb.ToString(), entityToUpdate);
            return updated > 0;
        }

        /// <summary>
        /// Delete entity in table "Ts".
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToDelete">Entity to delete</param>
        /// <returns>true if deleted, false if not found</returns>
        public static bool Delete<T>(this IDbConnection connection, T entityToDelete) where T : class
        {
            var type = typeof(T);

            var keyProperties = KeyPropertiesCache(type);
            if (keyProperties.Count() == 0)
                throw new ArgumentException("Entity must have at least one [Key] property");

            var name = GetTableName(type);

            var sb = new StringBuilder();
            sb.AppendFormat("delete from {0} where ", name);

            for (var i = 0; i < keyProperties.Count(); i++)
            {
                var property = keyProperties.ElementAt(i);
                sb.AppendFormat("`{0}` = @{1}", property.Name, property.Name);
                if (i < keyProperties.Count() - 1)
                    sb.AppendFormat(" and ");
            }
            var deleted = connection.Execute(sb.ToString(), entityToDelete);
            return deleted > 0;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public TableAttribute(string tableName)
        {
            Name = tableName;
        }

        public string Name { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreMappingAttribute : Attribute
    {
        public IgnoreMappingAttribute()
        {
        }
    }
}