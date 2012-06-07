using System.Configuration;
using System.Data;
using ItineraryBuilder.Models;
using ItineraryBuilder.Repository;
using MySql.Data.MySqlClient;
using StructureMap.Configuration.DSL;
using StructureMap.Pipeline;

namespace ItineraryBuilder.IOC
{
    public class MainRegistry : Registry
    {
        public MainRegistry()
        {
            For<IDbRepository>().Use<Repository.DbRepository>();

            For<IDbConnection>()
                .Use<MySqlConnection>().SetValue(typeof(string), ConfigurationManager.ConnectionStrings["MySql"].ConnectionString, CannotFindProperty.ThrowException);
        }
    }
}