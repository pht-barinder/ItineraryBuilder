using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ItineraryBuilder.Models;
using MySql.Data.MySqlClient;
using ItineraryBuilder.Repository.Dapper;

using BLLModels = ItineraryBuilder.Models;

namespace ItineraryBuilder.Repository
{
    public class DbRepository : Base, IDbRepository
    {
        public BLLModels.FlightStat GetFlightStat(long Id)
        {
            return FindAll<BLLModels.FlightStat>().Where(x => x.Id == Id).FirstOrDefault();
        }

        public List<BLLModels.ItinerarySearch> GetItinerarySearch(string oppID, string frID)
        {
            List<BLLModels.ItinerarySearch> listFS = null;

            sql = string.Format(@"select * from {0} its
                                        where its.OpportunityId='{1}' and its.FlightRequestID='{2}' order by FlightID",
                                        TableName.ItinerarySearch, oppID, frID);

            using (_conn = new MySqlConnection(ConnectionString))
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    _conn.Close();
                _conn.Open();

                listFS = ExecuteScaler<BLLModels.ItinerarySearch>(sql).ToList();
            }
            return listFS;
        }

        public List<BLLModels.ItinerarySearch> GetItinerarySearch(long id)
        {
            List<BLLModels.ItinerarySearch> listFS = null;

            using (_conn = new MySqlConnection(ConnectionString))
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    _conn.Close();
                _conn.Open();
                sql = string.Format(@"select * from {0} where Id={1}",
                                        TableName.ItinerarySearch, id);
                listFS = ExecuteScaler<BLLModels.ItinerarySearch>(sql).ToList();
            }
            return listFS;
        }

        public List<BLLModels.FlightStat> GetFlightStatByOppFrID(string oppID, string frID)
        {
            List<BLLModels.FlightStat> listFS = null;

            using (_conn = new MySqlConnection(ConnectionString))
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    _conn.Close();
                _conn.Open();
                sql = string.Format(@"select * from {0} its
                                        Inner Join {1} fs on fs.ItinerarySearchID = its.id
                                        where its.OpportunityId='{2}' and its.FlightRequestID='{3}' order by its.FlightID",
                                        TableName.ItinerarySearch, TableName.FlightStat, oppID, frID);
                listFS = ExecuteScaler<BLLModels.FlightStat>(sql).ToList();
            }
            return listFS;
        }

        public List<BLLModels.FlightStat> GetFlightStat(string oppID, string frID, string airlineCode, string flightNumber, DateTime from)
        {
            List<BLLModels.FlightStat> listFS = null;

            using (_conn = new MySqlConnection(ConnectionString))
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    _conn.Close();
                _conn.Open();
                sql = string.Format(@"select * from {0} its
                                        Inner Join {1} fs on fs.ItinerarySearchID = its.id
                                        where its.OpportunityId='{2}' and its.FlightRequestID='{3}'
                                        and its.AirlineCode='{4}'and its.FlightNumber='{5}' and its.from ='{6}' ",
                                         TableName.ItinerarySearch, TableName.FlightStat, oppID, frID, airlineCode, flightNumber, from.ToString("yyyy-MM-dd"));
                listFS = ExecuteScaler<BLLModels.FlightStat>(sql).ToList();
            }
            return listFS;
        }

        public List<BLLModels.FlightStatHistory> GetFlightStatHistory(string airlineCode, string flightNumber, DateTime from)
        {
            List<BLLModels.FlightStatHistory> listFSH = null;

            using (_conn = new MySqlConnection(ConnectionString))
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    _conn.Close();
                _conn.Open();
                sql = string.Format(@"select * from {0} fsh
                                        where fsh.CarrierAirlineCode='{1}'and fsh.FlightNumber='{2}' and fsh.From ='{3}' ",
                                        TableName.FlightStatHiostory, airlineCode, flightNumber, from.ToString("yyyy-MM-dd"));
                listFSH = ExecuteScaler<BLLModels.FlightStatHistory>(sql).ToList();
            }
            return listFSH;
        }

        public List<BLLModels.ItinerarySearch> GetFlightData(string oppID, string frID)
        {
            List<BLLModels.ItinerarySearch> listFS = null;

            sql = string.Format(@"select * from {0} its
                                        Inner Join {1} fs on fs.ItinerarySearchID = its.id
                                        where its.OpportunityId='{2}' and its.FlightRequestID='{3}' order by its.FlightID",
                                        TableName.ItinerarySearch, TableName.FlightStat, oppID, frID);

            using (_conn = new MySqlConnection(ConnectionString))
            {
                _conn.Open();

                listFS = _conn.Query<BLLModels.ItinerarySearch, BLLModels.FlightStat, BLLModels.ItinerarySearch>(sql, (its, fs) =>
                {
                    its.FlightStat = fs;
                    return its;
                }).ToList();
            }
            return listFS;
        }

        public List<BLLModels.FlightStat> GetFlightDataByItineraryID(long id)
        {
            List<BLLModels.FlightStat> fs = null;

            using (_conn = new MySqlConnection(ConnectionString))
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    _conn.Close();
                _conn.Open();
                sql = string.Format(@"select * from {0} fs where fs.ItinerarySearchID = {1}",
                                        TableName.FlightStat, id);
                fs = ExecuteScaler<BLLModels.FlightStat>(sql).ToList();
            }
            return fs;
        }

        public List<BLLModels.ItinerarySearch> GetLatestId()
        {
            List<BLLModels.ItinerarySearch> listFS = null;

            using (_conn = new MySqlConnection(ConnectionString))
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    _conn.Close();
                _conn.Open();
                sql = string.Format(@"select Id from {0} order by Id desc Limit 0,1",
                                        TableName.ItinerarySearch);
                listFS = ExecuteScaler<BLLModels.ItinerarySearch>(sql).ToList();
            }
            return listFS;
        }

        public bool SaveItinerarySearch(BLLModels.ItinerarySearch model)
        {
            if (GetItinerarySearch(model.Id).Count > 0)
            {
                Update<BLLModels.ItinerarySearch>(model);
            }
            else
            {
                Insert<BLLModels.ItinerarySearch>(model);
            }
            return model.Id > 0;
        }

        public bool SaveFlightStat(BLLModels.FlightStat model)
        {
            if (GetFlightStat(model.Id) != null)
            {
                Update<BLLModels.FlightStat>(model);
            }
            else
            {
                Insert<BLLModels.FlightStat>(model);
            }
            return model.Id > 0;
        }

        public bool SaveFlightStatHiostory(BLLModels.FlightStatHistory model)
        {
            if (GetFlightStat(model.Id) != null)
            {
                Update<BLLModels.FlightStatHistory>(model);
            }
            else
            {
                Insert<BLLModels.FlightStatHistory>(model);
            }
            return model.Id > 0;
        }

        public bool DeleteItinerarySearch(List<BLLModels.ItinerarySearch> model, string oppID, string frID)
        {
            if (model != null)
            {
                string ids = string.Join(",", model.Select(p => p.Id));
                return ExecuteNonQuery(string.Format(@"delete from {0} where Id not in ({1})
                                                and OpportunityId='{2}' and FlightRequestID='{3}'",
                                                    TableName.ItinerarySearch, ids, oppID, frID));
            }
            else
            {
                return ExecuteNonQuery(string.Format(@"delete from {0} where OpportunityId='{1}' and FlightRequestID='{2}'",
                                                    TableName.ItinerarySearch, oppID, frID));
            }
        }

        public bool DeleteItinerarySearch(long id)
        {
            return ExecuteNonQuery(string.Format(@"delete from {0} where Id={1}",
                                                TableName.ItinerarySearch, id));
        }

        public bool DeleteFlightStatByID(long id)
        {
            return ExecuteNonQuery(string.Format(@"delete from {0} where Id = ({1})",
                                                TableName.FlightStat, id));
        }

        public bool DeleteFlightStat(string fsID, long id)
        {
            return ExecuteNonQuery(string.Format(@"delete from {0} where Id not in ({1})
                                                and ItinerarySearchID ={2}",
                                                TableName.FlightStat, fsID, id));
        }

        public bool DeleteFlightStatByItinerarySearchID(long id)
        {
            return ExecuteNonQuery(string.Format(@"delete from {0} where ItinerarySearchID={1}",
                                                TableName.FlightStat, id));
        }
    }
}