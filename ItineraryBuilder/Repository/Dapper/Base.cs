using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using ItineraryBuilder.Models;
using MySql.Data.MySqlClient;

namespace ItineraryBuilder.Repository.Dapper
{
    public class Base
    {
        public IDbConnection _conn;
        public string sql = string.Empty;

        public static string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["MySql"].ConnectionString;
            }
        }

        public Base()
        {
            //_conn = conn;
            //if (_conn.State == ConnectionState.Open)
            //{
            //    _conn.Close();
            //}
        }

        public IDbConnection GetOpenConnection()
        {
            var cnn = new MySqlConnection(ConnectionString);
            return cnn;
        }

        public List<T> FindAll<T>() where T : class, IActiveRecord
        {
            List<T> retval;
            using (_conn = GetOpenConnection())
            {
                _conn.Open();
                retval = _conn.Get<T>();
            }

            return retval;
        }

        public T Find<T>(long id) where T : class, IActiveRecord
        {
            T retval;
            using (_conn = GetOpenConnection())
            {
                _conn.Open();
                retval = _conn.Get<T>(id);
            }

            return retval;
        }

        public T Insert<T>(T record) where T : class, IActiveRecord
        {
            using (_conn = GetOpenConnection())
            {
                _conn.Open();
                long id = _conn.Insert(record);

                record.Id = id;
            }

            return record;
        }

        public bool Update<T>(T record) where T : class, IActiveRecord
        {
            bool retval = false;

            using (_conn = GetOpenConnection())
            {
                _conn.Open();
                retval = _conn.Update(record);
            }

            return retval;
        }

        public bool Delete<T>(T record) where T : class, IActiveRecord
        {
            bool retval = false;

            using (_conn = GetOpenConnection())
            {
                _conn.Open();
                retval = _conn.Delete(record);
            }

            return retval;
        }

        public bool ExecuteNonQuery(string query)
        {
            int cnt = 0;

            using (_conn = GetOpenConnection())
            {
                _conn.Open();
                cnt = _conn.Execute(query);
            }

            return cnt > 0;
        }

        public bool ExecuteNonScaler(string query)
        {
            int cnt = 0;

            using (_conn = GetOpenConnection())
            {
                _conn.Open();
                var count = _conn.Query(query);
                cnt = (int)count.ElementAt(0).Cnt;
            }

            return cnt > 0;
        }

        public List<T> ExecuteScaler<T>(string query)
        {
            List<T> list = new List<T>();

            using (_conn = GetOpenConnection())
            {
                _conn.Open();
                list = _conn.Query<T>(query).ToList();
            }

            return list;
        }
    }
}