using System;
using System.Collections.Generic;
using BLLModels = ItineraryBuilder.Models;

namespace ItineraryBuilder.Repository
{
    public interface IDbRepository : IBaseRepository
    {
        BLLModels.FlightStat GetFlightStat(long Id);

        List<BLLModels.ItinerarySearch> GetItinerarySearch(string oppID, string frID);

        List<BLLModels.FlightStat> GetFlightStatByOppFrID(string oppID, string frID);

        List<BLLModels.FlightStat> GetFlightStat(string oppID, string frID, string airlineCode, string flightNumber,DateTime from);

        List<BLLModels.FlightStatHistory> GetFlightStatHistory(string airlineCode, string flightNumber, DateTime from);

        List<BLLModels.ItinerarySearch> GetFlightData(string oppID, string frID);
        
        List<BLLModels.FlightStat> GetFlightDataByItineraryID(long id);

        bool SaveItinerarySearch(BLLModels.ItinerarySearch model);

        bool SaveFlightStat(BLLModels.FlightStat model);

        bool SaveFlightStatHiostory(BLLModels.FlightStatHistory model);

        bool DeleteItinerarySearch(List<BLLModels.ItinerarySearch> model, string oppID, string frID);

        bool DeleteItinerarySearch(long id);

        bool DeleteFlightStatByID(long id);

        bool DeleteFlightStatByItinerarySearchID(long id);

        bool DeleteFlightStat(string ids, long id);

        List<BLLModels.ItinerarySearch> GetLatestId();
    }
}