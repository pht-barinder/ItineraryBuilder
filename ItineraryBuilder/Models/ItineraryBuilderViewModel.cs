using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ItineraryBuilder.Repository.Dapper;
using ItineraryBuilder.Util;

namespace ItineraryBuilder.Models
{
    public enum ClassTypes
    {
        [UtilEnumStringValue("Choose")]
        Choose = 1,
        [UtilEnumStringValue("Economy")]
        Economy = 2,
        [UtilEnumStringValue("Premium Economy")]
        PremiumEconomy = 3,
        [UtilEnumStringValue("Business Class")]
        BusinessClass,
        [UtilEnumStringValue("First Class")]
        FirstClass
    }
    public class FlightStat : IActiveRecord
    {
        public long Id { get; set; }
        public long ItinerarySearchID { get; set; }
        public string ArrivalDateAdjustment { get; set; }
        public string ArrivalTime { get; set; }
        public DateTime DepartureDateFrom { get; set; }
        public DateTime DepartureDateTo { get; set; }
        public string DepartureDaysOfWeek { get; set; }
        public string DepartureTime { get; set; }
        public int DistanceMiles { get; set; }
        public int FlightDurationMinutes { get; set; }
        public string FlightType { get; set; }
        public int LayoverDurationMinutes { get; set; }
        public string ServiceType { get; set; }
        public string DepartureAirportCode { get; set; }
        public string DepartureAirportCity { get; set; }
        public string DepartureAirportCountryCode { get; set; }
        public string DepartureAirportFAACode { get; set; }
        public string DepartureAirportIATACode { get; set; }
        public string DepartureAirportICAOCode { get; set; }
        public string DepartureAirportName { get; set; }
        public string DepartureAirportStateCode { get; set; }
        public string ArrivalAirportCode { get; set; }
        public string ArrivalAirportCity { get; set; }
        public string ArrivalAirportCountryCode { get; set; }
        public string ArrivalAirportFAACode { get; set; }
        public string ArrivalAirportIATACode { get; set; }
        public string ArrivalAirportICAOCode { get; set; }
        public string ArrivalAirportName { get; set; }
        public string ArrivalAirportStateCode { get; set; }
        public Boolean Codeshare { get; set; }
        public int FlightLegArrivalDateAdjustment { get; set; }
        public string FlightLegArrivalTime { get; set; }
        public int FlightLegDepartureDateAdjustment { get; set; }
        public string FlightLegDepartureTerminal { get; set; }
        public string FlightLegDepartureTime { get; set; }
        public int FlightLegDistanceMiles { get; set; }
        public int FlightLegFlightDurationMinutes { get; set; }
        public int FlightLegLayoverDurationMinutes { get; set; }
        public string FlightNumber { get; set; }
        public string CarrierAirlineCode { get; set; }
        public string CarrierIATACode { get; set; }
        public string CarrierICAOCode { get; set; }
        public string CarrierName { get; set; }
        public string AircraftTypeCode { get; set; }
        public string AircraftTypeName { get; set; }
        public string Jet { get; set; }
        public string Regional { get; set; }
        public string Turboprop { get; set; }
        public string WideBody { get; set; }
        public int Seat { get; set; }
    }

    public class ItinerarySearch : IActiveRecord
    {
        public long Id { get; set; }

        public int FlightID { get; set; }
        [Required(ErrorMessage = "AirlineCode is required")]
        public string AirlineCode { get; set; }
        [Required(ErrorMessage = "FlightNumber is required")]
        public string FlightNumber { get; set; }
        [Required(ErrorMessage = "Class is required")]
        public ClassTypes Class { get; set; }
        [Required(ErrorMessage = "FromDate is required")]
        public DateTime? From { get; set; }

        public string FromDate { get; set; }
        public string OpportunityId { get; set; }
        public string FlightRequestID { get; set; }

        [IgnoreMapping]
        public FlightStat FlightStat { get; set; }

    }

    public class FlightStatHistory : IActiveRecord
    {
        public long Id { get; set; }
        public string ArrivalDateAdjustment { get; set; }
        public string ArrivalTime { get; set; }
        public DateTime DepartureDateFrom { get; set; }
        public DateTime DepartureDateTo { get; set; }
        public string DepartureDaysOfWeek { get; set; }
        public string DepartureTime { get; set; }
        public int DistanceMiles { get; set; }
        public int FlightDurationMinutes { get; set; }
        public string FlightType { get; set; }
        public int LayoverDurationMinutes { get; set; }
        public string ServiceType { get; set; }
        public string DepartureAirportCode { get; set; }
        public string DepartureAirportCity { get; set; }
        public string DepartureAirportCountryCode { get; set; }
        public string DepartureAirportFAACode { get; set; }
        public string DepartureAirportIATACode { get; set; }
        public string DepartureAirportICAOCode { get; set; }
        public string DepartureAirportName { get; set; }
        public string DepartureAirportStateCode { get; set; }
        public string ArrivalAirportCode { get; set; }
        public string ArrivalAirportCity { get; set; }
        public string ArrivalAirportCountryCode { get; set; }
        public string ArrivalAirportFAACode { get; set; }
        public string ArrivalAirportIATACode { get; set; }
        public string ArrivalAirportICAOCode { get; set; }
        public string ArrivalAirportName { get; set; }
        public string ArrivalAirportStateCode { get; set; }
        public Boolean Codeshare { get; set; }
        public int FlightLegArrivalDateAdjustment { get; set; }
        public string FlightLegArrivalTime { get; set; }
        public int FlightLegDepartureDateAdjustment { get; set; }
        public string FlightLegDepartureTerminal { get; set; }
        public string FlightLegDepartureTime { get; set; }
        public int FlightLegDistanceMiles { get; set; }
        public int FlightLegFlightDurationMinutes { get; set; }
        public int FlightLegLayoverDurationMinutes { get; set; }
        public string FlightNumber { get; set; }
        public string CarrierAirlineCode { get; set; }
        public string CarrierIATACode { get; set; }
        public string CarrierICAOCode { get; set; }
        public string CarrierName { get; set; }
        public string AircraftTypeCode { get; set; }
        public string AircraftTypeName { get; set; }
        public string Jet { get; set; }
        public string Regional { get; set; }
        public string Turboprop { get; set; }
        public string WideBody { get; set; }
        public int Seat { get; set; }

        public DateTime? From { get; set; }
    }
}