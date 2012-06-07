using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.IO;
using System.Text;
using ItineraryBuilder.Models;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using BLLModels = ItineraryBuilder.Models;
using BLLRepository = ItineraryBuilder.Repository;
using ItineraryBuilder.Helpers;

namespace ItineraryBuilder.Controllers
{
    public class ItineraryBuilderController : Controller
    {
        private readonly BLLRepository.IDbRepository _dbRepository;

        public ItineraryBuilderController(BLLRepository.IDbRepository dbRepository)
        {
            _dbRepository = dbRepository;
        }

        [HttpGet]
        public ActionResult Index(string opportunityId, string flightRequestID, int passengers)
        {
            ViewData["opportunityId"] = opportunityId;
            ViewData["flightRequestID"] = flightRequestID;
            ViewData["passengers"] = passengers;

            List<BLLModels.ItinerarySearch> listIS = _dbRepository.GetItinerarySearch(opportunityId, flightRequestID);

            if (listIS.Count == 0)
            {
                listIS.Add(new ItinerarySearch { FlightID = 1 });
            }

            ViewData["ItinerarySearch"] = listIS;

            List<BLLModels.FlightStat> model = new List<FlightStat>();
            model = _dbRepository.GetFlightStatByOppFrID(opportunityId, flightRequestID);
            if (model != null && model.Count > 0)
            {
                ViewData["Result"] = _dbRepository.GetFlightData(opportunityId, flightRequestID);
            }
            return View();
        }

        [HttpPost]
        public ActionResult GetFlightData(long id, int flightID, string ailreline, string flightNumber, string fromDate, ItineraryBuilder.Models.ClassTypes flightClass, string opportunityId, string flightRequestID, int passengers, bool? isDelete, string rowID)
        {
            string message = string.Empty;
            bool isInsert = false;
            DateTime? dd = null;
            ViewData["RowID"] = rowID;
            if (!string.IsNullOrEmpty(fromDate))
            {
                try
                {
                    dd = Convert.ToDateTime(fromDate);
                }
                catch
                {
                    message = "Incorrect From Date";
                }
            }
            if (string.IsNullOrEmpty(message))
            {
                List<ItinerarySearch> list = new List<ItinerarySearch>();
                if (!string.IsNullOrEmpty(fromDate))
                    dd = Convert.ToDateTime(fromDate);
                list.Add(new ItinerarySearch { Id = id, FlightID = flightID, AirlineCode = ailreline, FlightNumber = flightNumber, From = dd, FromDate = fromDate, Class = flightClass });

                if (string.IsNullOrEmpty(opportunityId))
                    opportunityId = "006d0000005nU0DAAU";
                if (string.IsNullOrEmpty(flightRequestID))
                    flightRequestID = "a00d0000003pSE2AAM";

                ViewData["opportunityId"] = opportunityId;
                ViewData["flightRequestID"] = flightRequestID;
                SchedulesConnectionsService.Login lgn = new SchedulesConnectionsService.Login();
                lgn.UserId = System.Configuration.ConfigurationManager.AppSettings["username"];
                lgn.Password = System.Configuration.ConfigurationManager.AppSettings["password"];
                lgn.AccountId = System.Configuration.ConfigurationManager.AppSettings["accountid"];
                if (isDelete.GetValueOrDefault())
                {
                    _dbRepository.DeleteItinerarySearch(id);
                }
                else
                {
                    if (list != null)
                    {
                        foreach (var item in list)
                        {
                            var model = _dbRepository.GetFlightStat(opportunityId, flightRequestID, item.AirlineCode, item.FlightNumber, item.From.GetValueOrDefault());

                            if (model == null || model.Count == 0)
                            {
                                if (!string.IsNullOrEmpty(item.AirlineCode) && !string.IsNullOrEmpty(item.FlightNumber) && item.From != null)
                                {
                                    #region Get Data from History if available
                                    List<FlightStatHistory> listFSH = _dbRepository.GetFlightStatHistory(item.AirlineCode, item.FlightNumber, item.From.GetValueOrDefault());
                                    if (listFSH != null && listFSH.Count > 0)
                                    {
                                        #region Insert Serach Parameters to have available next time
                                        ItinerarySearch its = new ItinerarySearch();
                                        if (item.Id == 0)
                                            isInsert = true;
                                        its.Id = item.Id;
                                        its.FlightID = item.FlightID;
                                        its.OpportunityId = opportunityId;
                                        its.FlightRequestID = flightRequestID;
                                        its.AirlineCode = item.AirlineCode;
                                        its.FlightNumber = item.FlightNumber;
                                        its.From = item.From;
                                        its.FromDate = item.FromDate;
                                        its.Class = item.Class;
                                        _dbRepository.SaveItinerarySearch(its);
                                        ViewData["Id"] = its.Id;
                                        #endregion
                                        int j = 1;
                                        foreach (FlightStatHistory fsh in listFSH)
                                        {
                                            #region Insert FlightStat to have available next time
                                            if (!isInsert)
                                            {
                                                _dbRepository.DeleteFlightStatByItinerarySearchID(its.Id);
                                                isInsert = true;
                                            }
                                            FlightStat fs = new FlightStat();
                                            fs.ItinerarySearchID = its.Id;
                                            fs.ArrivalDateAdjustment = fsh.ArrivalDateAdjustment;

                                            fs.DepartureDateFrom = fsh.DepartureDateFrom;
                                            fs.DepartureDateTo = fsh.DepartureDateTo;

                                            fs.DepartureTime = fsh.DepartureTime;
                                            fs.ArrivalTime = fsh.ArrivalTime;

                                            fs.FlightDurationMinutes = fsh.FlightDurationMinutes;
                                            fs.DistanceMiles = fsh.DistanceMiles;

                                            fs.FlightNumber = fsh.FlightNumber;
                                            fs.CarrierName = fsh.CarrierName;
                                            fs.CarrierAirlineCode = fsh.CarrierAirlineCode;

                                            fs.DepartureAirportName = fsh.DepartureAirportName;
                                            fs.DepartureAirportCode = fsh.DepartureAirportCode;
                                            fs.DepartureAirportFAACode = fsh.DepartureAirportFAACode;
                                            fs.DepartureAirportCity = fsh.DepartureAirportCity;
                                            fs.DepartureAirportStateCode = fsh.DepartureAirportStateCode;
                                            fs.DepartureAirportCountryCode = fsh.DepartureAirportCountryCode;

                                            fs.ArrivalAirportName = fsh.ArrivalAirportName;
                                            fs.ArrivalAirportCode = fsh.ArrivalAirportCode;
                                            fs.ArrivalAirportFAACode = fsh.ArrivalAirportFAACode;
                                            fs.ArrivalAirportCity = fsh.ArrivalAirportCity;
                                            fs.ArrivalAirportStateCode = fsh.ArrivalAirportStateCode;
                                            fs.ArrivalAirportCountryCode = fsh.ArrivalAirportCountryCode;

                                            fs.AircraftTypeName = fsh.AircraftTypeName;
                                            fs.AircraftTypeCode = fsh.AircraftTypeCode;

                                            fs.WideBody = fsh.WideBody;
                                            fs.Jet = fsh.Jet;
                                            fs.DepartureDaysOfWeek = fsh.DepartureDaysOfWeek;

                                            fs.Seat = fsh.Seat;

                                            _dbRepository.SaveFlightStat(fs);
                                            j += 1;
                                            #endregion
                                        }
                                    }
                                    #endregion
                                    #region Get XML Response from Flight Stat
                                    else
                                    {
                                        string url = @"http://www.pathfinder-xml.com/development/xml?Service=SchedulesConnectionsService"
                                                      + "&login.accountID=" + lgn.AccountId + "&login.userID=" + lgn.UserId + "&login.password=" + lgn.Password
                                                      + "&carriers[0].airlineCode=" + item.AirlineCode + "&flightNumber=" + item.FlightNumber
                                                      + "&from=" + item.From.GetValueOrDefault().ToString("yyyy-MM-ddT00:00") + "&serviceType=PASSENGER_ONLY&flightType=NON_STOP";
                                        string result = string.Empty;
                                        WebRequest req = null;
                                        WebResponse rsp = null;
                                        try
                                        {
                                            req = WebRequest.Create(url);
                                            req.Method = "POST";
                                            req.ContentType = "text/xml";
                                            req.Timeout = 100000000;

                                            StreamWriter writer = new StreamWriter(req.GetRequestStream());
                                            writer.WriteLine(url);
                                            writer.Close();
                                            // Send the data to the webserver
                                            rsp = req.GetResponse();

                                            Stream st = rsp.GetResponseStream();
                                            StreamReader str = new StreamReader(st);

                                            result = str.ReadToEnd();
                                            XmlDocument xdoc = new XmlDocument();
                                            xdoc.LoadXml(result);
                                            xdoc.Save(Server.MapPath("/XMls" + "myfilename.xml"));

                                            XmlSerializer xs = new XmlSerializer(typeof(SchedulesConnectionsResponse));

                                            TextReader textReader = new StreamReader(Server.MapPath("/XMls" + "myfilename.xml"));

                                            SchedulesConnectionsResponse obj = (SchedulesConnectionsResponse)xs.Deserialize(textReader);
                                            textReader.Close();
                                            System.IO.File.Delete(Server.MapPath("/XMls" + "myfilename.xml"));

                                            if (obj.Items != null)
                                            {
                                                #region Insert Serach Parameters to have available next time
                                                ItinerarySearch its = new ItinerarySearch();
                                                if (item.Id == 0)
                                                    isInsert = true;
                                                its.Id = item.Id;
                                                its.FlightID = item.FlightID;
                                                its.OpportunityId = opportunityId;
                                                its.FlightRequestID = flightRequestID;
                                                its.AirlineCode = item.AirlineCode;
                                                its.FlightNumber = item.FlightNumber;
                                                its.From = item.From;
                                                its.FromDate = item.FromDate;
                                                its.Class = item.Class;
                                                _dbRepository.SaveItinerarySearch(its);
                                                ViewData["Id"] = its.Id;
                                                #endregion
                                                int j = 1;
                                                int day = 0;
                                                foreach (SchedulesConnectionsResponseFlight item1 in obj.Items)
                                                {
                                                    foreach (var fl in item1.FlightLeg)
                                                    {
                                                        foreach (var fi in fl.FlightId)
                                                        {
                                                            #region Insert FlightStat to have available next time
                                                            if (!isInsert)
                                                            {
                                                                _dbRepository.DeleteFlightStatByItinerarySearchID(its.Id);
                                                                isInsert = true;
                                                            }
                                                            FlightStat fs = new FlightStat();
                                                            fs.ItinerarySearchID = its.Id;

                                                            fs.ArrivalDateAdjustment = (Convert.ToInt32(item1.ArrivalDateAdjustment) + day).ToString();
                                                            day = Convert.ToInt32(item1.ArrivalDateAdjustment);

                                                            if (!string.IsNullOrEmpty(item1.DepartureDateFrom))
                                                            {
                                                                fs.DepartureDateFrom = Convert.ToDateTime(item1.DepartureDateFrom);
                                                            }
                                                            if (!string.IsNullOrEmpty(item1.DepartureDateTo))
                                                            {
                                                                fs.DepartureDateTo = Convert.ToDateTime(item1.DepartureDateTo);
                                                            }

                                                            fs.DepartureTime = item1.DepartureTime;
                                                            fs.ArrivalTime = item1.ArrivalTime;

                                                            fs.FlightDurationMinutes = item1.FlightDurationMinutes;
                                                            fs.DistanceMiles = item1.DistanceMiles;

                                                            fs.FlightNumber = fi.FlightNumber;
                                                            fs.CarrierName = fi.Carrier[0].Name;
                                                            fs.CarrierAirlineCode = fi.Carrier[0].AirlineCode;


                                                            fs.DepartureAirportName = fl.DepartureAirport[0].Name;
                                                            fs.DepartureAirportCode = fl.DepartureAirport[0].AirportCode;
                                                            fs.DepartureAirportFAACode = fl.DepartureAirport[0].FAACode;
                                                            fs.DepartureAirportCity = fl.DepartureAirport[0].City;
                                                            fs.DepartureAirportStateCode = fl.DepartureAirport[0].StateCode;
                                                            fs.DepartureAirportCountryCode = fl.DepartureAirport[0].CountryCode;

                                                            fs.ArrivalAirportName = fl.ArrivalAirport[0].Name;
                                                            fs.ArrivalAirportCode = fl.ArrivalAirport[0].AirportCode;
                                                            fs.ArrivalAirportFAACode = fl.ArrivalAirport[0].FAACode;
                                                            fs.ArrivalAirportCity = fl.ArrivalAirport[0].City;
                                                            fs.ArrivalAirportStateCode = fl.ArrivalAirport[0].StateCode;
                                                            fs.ArrivalAirportCountryCode = fl.ArrivalAirport[0].CountryCode;

                                                            fs.AircraftTypeName = fl.Equipment[0].AircraftTypeName;
                                                            fs.AircraftTypeCode = fl.Equipment[0].AircraftTypeCode;

                                                            fs.WideBody = fl.Equipment[0].WideBody;
                                                            fs.Jet = fl.Equipment[0].Jet;
                                                            fs.DepartureDaysOfWeek = item1.DepartureDaysOfWeek;

                                                            fs.Seat = RandomNumber(passengers, passengers + 2);

                                                            _dbRepository.SaveFlightStat(fs);
                                                            #endregion

                                                            #region Save Flight Stat result in History table to get it next time
                                                            FlightStatHistory fsh = new FlightStatHistory();

                                                            if (!string.IsNullOrEmpty(item1.DepartureDateFrom))
                                                            {
                                                                fsh.DepartureDateFrom = Convert.ToDateTime(item1.DepartureDateFrom);
                                                            }
                                                            if (!string.IsNullOrEmpty(item1.DepartureDateTo))
                                                            {
                                                                fsh.DepartureDateTo = Convert.ToDateTime(item1.DepartureDateTo);
                                                            }

                                                            fsh.ArrivalDateAdjustment = fs.ArrivalDateAdjustment;

                                                            fsh.DepartureTime = item1.DepartureTime;
                                                            fsh.ArrivalTime = item1.ArrivalTime;

                                                            fsh.FlightDurationMinutes = item1.FlightDurationMinutes;
                                                            fsh.DistanceMiles = item1.DistanceMiles;

                                                            fsh.FlightNumber = fi.FlightNumber;
                                                            fsh.CarrierName = fi.Carrier[0].Name;
                                                            fsh.CarrierAirlineCode = fi.Carrier[0].AirlineCode;

                                                            fsh.DepartureAirportName = fl.DepartureAirport[0].Name;
                                                            fsh.DepartureAirportCode = fl.DepartureAirport[0].AirportCode;
                                                            fsh.DepartureAirportFAACode = fl.DepartureAirport[0].FAACode;
                                                            fsh.DepartureAirportCity = fl.DepartureAirport[0].City;
                                                            fsh.DepartureAirportStateCode = fl.DepartureAirport[0].StateCode;
                                                            fsh.DepartureAirportCountryCode = fl.DepartureAirport[0].CountryCode;

                                                            fsh.ArrivalAirportName = fl.ArrivalAirport[0].Name;
                                                            fsh.ArrivalAirportCode = fl.ArrivalAirport[0].AirportCode;
                                                            fsh.ArrivalAirportFAACode = fl.ArrivalAirport[0].FAACode;
                                                            fsh.ArrivalAirportCity = fl.ArrivalAirport[0].City;
                                                            fsh.ArrivalAirportStateCode = fl.ArrivalAirport[0].StateCode;
                                                            fsh.ArrivalAirportCountryCode = fl.ArrivalAirport[0].CountryCode;

                                                            fsh.AircraftTypeName = fl.Equipment[0].AircraftTypeName;
                                                            fsh.AircraftTypeCode = fl.Equipment[0].AircraftTypeCode;

                                                            fsh.From = item.From;

                                                            fsh.WideBody = fl.Equipment[0].WideBody;
                                                            fsh.Jet = fl.Equipment[0].Jet;
                                                            fsh.DepartureDaysOfWeek = item1.DepartureDaysOfWeek;

                                                            fsh.Seat = fs.Seat;

                                                            _dbRepository.SaveFlightStatHiostory(fsh);
                                                            j += 1;
                                                            #endregion
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                message = "No data found!";
                                            }
                                        }
                                        catch (WebException webEx)
                                        {
                                            result = "Error: " + webEx.Message + " Stack Trace: " + webEx.StackTrace;
                                        }
                                        catch (Exception ex)
                                        {
                                            result = "Error: " + ex.Message + " Stack Trace: " + ex.StackTrace;
                                        }
                                    }
                                    #endregion
                                }
                            }
                            else
                            {
                                ItinerarySearch its = new ItinerarySearch();
                                its.Id = item.Id;

                                its.FlightID = item.FlightID;
                                its.OpportunityId = opportunityId;
                                its.FlightRequestID = flightRequestID;
                                its.AirlineCode = item.AirlineCode;
                                its.FlightNumber = item.FlightNumber;
                                its.From = item.From;
                                its.FromDate = item.FromDate;
                                its.Class = item.Class;
                                if (item.Id == 0)
                                {
                                    message = "Duplicate Record!";
                                }
                                else
                                {
                                    _dbRepository.SaveItinerarySearch(its);
                                }
                            }
                        }
                    }
                }
            }
            ViewData["isInsert"] = isInsert;
            ViewData["Message"] = message;
            return View("UcItineraryBuilder", _dbRepository.GetFlightData(opportunityId, flightRequestID));
        }

        [HttpPost]
        public ActionResult RemoveFlightData(List<ItinerarySearch> list, string opportunityId, string flightRequestID, int passengers, bool? isDelete)
        {
            string message = string.Empty;
            bool isInsert = false;
            if (string.IsNullOrEmpty(opportunityId))
                opportunityId = "006d0000005nU0DAAU";
            if (string.IsNullOrEmpty(flightRequestID))
                flightRequestID = "a00d0000003pSE2AAM";

            ViewData["opportunityId"] = opportunityId;
            ViewData["flightRequestID"] = flightRequestID;
            SchedulesConnectionsService.Login lgn = new SchedulesConnectionsService.Login();
            lgn.UserId = System.Configuration.ConfigurationManager.AppSettings["username"];
            lgn.Password = System.Configuration.ConfigurationManager.AppSettings["password"];
            lgn.AccountId = System.Configuration.ConfigurationManager.AppSettings["accountid"];

            _dbRepository.DeleteItinerarySearch(list, opportunityId, flightRequestID);
            List<BLLModels.FlightStat> model = new List<FlightStat>();
            if (list != null)
            {
                foreach (var item in list)
                {
                    DateTime? dd = null;
                    if (!string.IsNullOrEmpty(item.FromDate))
                    {
                        dd = Convert.ToDateTime(item.FromDate);
                        item.From = dd;
                    }
                    var model1 = _dbRepository.GetFlightStat(opportunityId, flightRequestID, item.AirlineCode, item.FlightNumber, item.From.GetValueOrDefault());

                    if (model1 == null || model1.Count == 0)
                    {
                        if (!string.IsNullOrEmpty(item.AirlineCode) && !string.IsNullOrEmpty(item.FlightNumber) && item.From != null)
                        {
                            #region Get Data from History if available
                            List<FlightStatHistory> listFSH = _dbRepository.GetFlightStatHistory(item.AirlineCode, item.FlightNumber, item.From.GetValueOrDefault());
                            if (listFSH != null && listFSH.Count > 1)
                            {
                                #region Insert Serach Parameters to have available next time
                                ItinerarySearch its = new ItinerarySearch();
                                if (item.Id == 0)
                                    isInsert = true;
                                its.Id = item.Id;
                                its.FlightID = item.FlightID;
                                its.OpportunityId = opportunityId;
                                its.FlightRequestID = flightRequestID;
                                its.AirlineCode = item.AirlineCode;
                                its.FlightNumber = item.FlightNumber;
                                its.From = item.From;
                                its.FromDate = item.FromDate;
                                its.Class = item.Class;
                                _dbRepository.SaveItinerarySearch(its);
                                ViewData["Id"] = its.Id;
                                #endregion
                                int j = 1;
                                foreach (FlightStatHistory fsh in listFSH)
                                {
                                    #region Insert FlightStat to have available next time
                                    if (!isInsert)
                                    {
                                        _dbRepository.DeleteFlightStatByItinerarySearchID(its.Id);
                                        isInsert = true;
                                    }
                                    FlightStat fs = new FlightStat();
                                    fs.ItinerarySearchID = its.Id;

                                    fs.ArrivalDateAdjustment = fsh.ArrivalDateAdjustment;

                                    fs.DepartureDateFrom = fsh.DepartureDateFrom;
                                    fs.DepartureDateTo = fsh.DepartureDateTo;

                                    fs.DepartureTime = fsh.DepartureTime;
                                    fs.ArrivalTime = fsh.ArrivalTime;

                                    fs.FlightDurationMinutes = fsh.FlightDurationMinutes;
                                    fs.DistanceMiles = fsh.DistanceMiles;

                                    fs.FlightNumber = fsh.FlightNumber;
                                    fs.CarrierName = fsh.CarrierName;
                                    fs.CarrierAirlineCode = fsh.CarrierAirlineCode;

                                    fs.DepartureAirportName = fsh.DepartureAirportName;
                                    fs.DepartureAirportCode = fsh.ArrivalAirportCode;
                                    fs.DepartureAirportFAACode = fsh.DepartureAirportFAACode;
                                    fs.DepartureAirportCity = fsh.DepartureAirportCity;
                                    fs.DepartureAirportStateCode = fsh.DepartureAirportStateCode;
                                    fs.DepartureAirportCountryCode = fsh.DepartureAirportCountryCode;

                                    fs.ArrivalAirportName = fsh.ArrivalAirportName;
                                    fs.ArrivalAirportCode = fsh.ArrivalAirportCode;
                                    fs.ArrivalAirportFAACode = fsh.ArrivalAirportFAACode;
                                    fs.ArrivalAirportCity = fsh.ArrivalAirportCity;
                                    fs.ArrivalAirportStateCode = fsh.ArrivalAirportStateCode;
                                    fs.ArrivalAirportCountryCode = fsh.ArrivalAirportCountryCode;

                                    fs.AircraftTypeName = fsh.AircraftTypeName;
                                    fs.AircraftTypeCode = fsh.AircraftTypeCode;

                                    fs.Seat = fsh.Seat;

                                    _dbRepository.SaveFlightStat(fs);
                                    j += 1;
                                    #endregion
                                }
                            }
                            #endregion
                            #region Get XML Response from Flight Stat
                            else
                            {
                                string url = @"http://www.pathfinder-xml.com/development/xml?Service=SchedulesConnectionsService"
                                              + "&login.accountID=" + lgn.AccountId + "&login.userID=" + lgn.UserId + "&login.password=" + lgn.Password
                                              + "&carriers[0].airlineCode=" + item.AirlineCode + "&flightNumber=" + item.FlightNumber
                                              + "&from=" + item.From.GetValueOrDefault().ToString("yyyy-MM-ddT00:00") + "&serviceType=PASSENGER_ONLY&flightType=NON_STOP";
                                string result = string.Empty;
                                WebRequest req = null;
                                WebResponse rsp = null;
                                try
                                {
                                    req = WebRequest.Create(url);
                                    req.Method = "POST";
                                    req.ContentType = "text/xml";
                                    req.Timeout = 100000000;

                                    StreamWriter writer = new StreamWriter(req.GetRequestStream());
                                    writer.WriteLine(url);
                                    writer.Close();
                                    // Send the data to the webserver
                                    rsp = req.GetResponse();

                                    Stream st = rsp.GetResponseStream();
                                    StreamReader str = new StreamReader(st);

                                    result = str.ReadToEnd();
                                    XmlDocument xdoc = new XmlDocument();
                                    xdoc.LoadXml(result);
                                    xdoc.Save(Server.MapPath("/XMls" + "myfilename.xml"));

                                    XmlSerializer xs = new XmlSerializer(typeof(SchedulesConnectionsResponse));

                                    TextReader textReader = new StreamReader(Server.MapPath("/XMls" + "myfilename.xml"));

                                    SchedulesConnectionsResponse obj = (SchedulesConnectionsResponse)xs.Deserialize(textReader);
                                    textReader.Close();
                                    System.IO.File.Delete(Server.MapPath("/XMls" + "myfilename.xml"));

                                    if (obj.Items != null)
                                    {
                                        #region Insert Serach Parameters to have available next time
                                        ItinerarySearch its = new ItinerarySearch();
                                        if (item.Id == 0)
                                            isInsert = true;
                                        its.Id = item.Id;
                                        its.FlightID = item.FlightID;
                                        its.OpportunityId = opportunityId;
                                        its.FlightRequestID = flightRequestID;
                                        its.AirlineCode = item.AirlineCode;
                                        its.FlightNumber = item.FlightNumber;
                                        its.From = item.From;
                                        its.FromDate = item.FromDate;
                                        its.Class = item.Class;
                                        _dbRepository.SaveItinerarySearch(its);
                                        ViewData["Id"] = its.Id;
                                        #endregion
                                        int j = 1;
                                        int day = 0;
                                        foreach (SchedulesConnectionsResponseFlight item1 in obj.Items)
                                        {
                                            foreach (var fl in item1.FlightLeg)
                                            {
                                                foreach (var fi in fl.FlightId)
                                                {
                                                    #region Insert FlightStat to have available next time
                                                    if (!isInsert)
                                                    {
                                                        _dbRepository.DeleteFlightStatByItinerarySearchID(its.Id);
                                                        isInsert = true;
                                                    }
                                                    FlightStat fs = new FlightStat();
                                                    fs.ItinerarySearchID = its.Id;

                                                    fs.ArrivalDateAdjustment = (Convert.ToInt32(item1.ArrivalDateAdjustment) + day).ToString();
                                                    day = Convert.ToInt32(item1.ArrivalDateAdjustment);

                                                    if (!string.IsNullOrEmpty(item1.DepartureDateFrom))
                                                    {
                                                        fs.DepartureDateFrom = Convert.ToDateTime(item1.DepartureDateFrom);
                                                    }
                                                    if (!string.IsNullOrEmpty(item1.DepartureDateTo))
                                                    {
                                                        fs.DepartureDateTo = Convert.ToDateTime(item1.DepartureDateTo);
                                                    }

                                                    fs.DepartureTime = item1.DepartureTime;
                                                    fs.ArrivalTime = item1.ArrivalTime;

                                                    fs.FlightDurationMinutes = item1.FlightDurationMinutes;
                                                    fs.DistanceMiles = item1.DistanceMiles;

                                                    fs.FlightNumber = fi.FlightNumber;
                                                    fs.CarrierName = fi.Carrier[0].Name;
                                                    fs.CarrierAirlineCode = fi.Carrier[0].AirlineCode;

                                                    fs.DepartureAirportName = fl.DepartureAirport[0].Name;
                                                    fs.DepartureAirportCode = fl.ArrivalAirport[0].AirportCode;
                                                    fs.DepartureAirportFAACode = fl.DepartureAirport[0].FAACode;
                                                    fs.DepartureAirportCity = fl.DepartureAirport[0].City;
                                                    fs.DepartureAirportStateCode = fl.DepartureAirport[0].StateCode;
                                                    fs.DepartureAirportCountryCode = fl.DepartureAirport[0].CountryCode;

                                                    fs.ArrivalAirportName = fl.ArrivalAirport[0].Name;
                                                    fs.ArrivalAirportCode = fl.ArrivalAirport[0].AirportCode;
                                                    fs.ArrivalAirportFAACode = fl.ArrivalAirport[0].FAACode;
                                                    fs.ArrivalAirportCity = fl.ArrivalAirport[0].City;
                                                    fs.ArrivalAirportStateCode = fl.ArrivalAirport[0].StateCode;
                                                    fs.ArrivalAirportCountryCode = fl.ArrivalAirport[0].CountryCode;

                                                    fs.AircraftTypeName = fl.Equipment[0].AircraftTypeName;
                                                    fs.AircraftTypeCode = fl.Equipment[0].AircraftTypeCode;

                                                    fs.Seat = RandomNumber(passengers, passengers + 2);

                                                    fs.WideBody = fl.Equipment[0].WideBody;
                                                    fs.Jet = fl.Equipment[0].Jet;
                                                    fs.DepartureDaysOfWeek = item1.DepartureDaysOfWeek;

                                                    _dbRepository.SaveFlightStat(fs);
                                                    #endregion

                                                    #region Save Flight Stat result in History table to get it next time
                                                    FlightStatHistory fsh = new FlightStatHistory();

                                                    if (!string.IsNullOrEmpty(item1.DepartureDateFrom))
                                                    {
                                                        fsh.DepartureDateFrom = Convert.ToDateTime(item1.DepartureDateFrom);
                                                    }
                                                    if (!string.IsNullOrEmpty(item1.DepartureDateTo))
                                                    {
                                                        fsh.DepartureDateTo = Convert.ToDateTime(item1.DepartureDateTo);
                                                    }

                                                    fsh.ArrivalDateAdjustment = fs.ArrivalDateAdjustment;

                                                    fsh.DepartureTime = item1.DepartureTime;
                                                    fsh.ArrivalTime = item1.ArrivalTime;

                                                    fsh.FlightDurationMinutes = item1.FlightDurationMinutes;
                                                    fsh.DistanceMiles = item1.DistanceMiles;

                                                    fsh.FlightNumber = fi.FlightNumber;
                                                    fsh.CarrierName = fi.Carrier[0].Name;
                                                    fsh.CarrierAirlineCode = fi.Carrier[0].AirlineCode;

                                                    fsh.DepartureAirportName = fl.DepartureAirport[0].Name;
                                                    fsh.DepartureAirportCode = fl.ArrivalAirport[0].AirportCode;
                                                    fsh.DepartureAirportFAACode = fl.DepartureAirport[0].FAACode;
                                                    fsh.DepartureAirportCity = fl.DepartureAirport[0].City;
                                                    fsh.DepartureAirportStateCode = fl.DepartureAirport[0].StateCode;
                                                    fsh.DepartureAirportCountryCode = fl.DepartureAirport[0].CountryCode;

                                                    fsh.ArrivalAirportName = fl.ArrivalAirport[0].Name;
                                                    fsh.ArrivalAirportCode = fl.ArrivalAirport[0].AirportCode;
                                                    fsh.ArrivalAirportFAACode = fl.ArrivalAirport[0].FAACode;
                                                    fsh.ArrivalAirportCity = fl.ArrivalAirport[0].City;
                                                    fsh.ArrivalAirportStateCode = fl.ArrivalAirport[0].StateCode;
                                                    fsh.ArrivalAirportCountryCode = fl.ArrivalAirport[0].CountryCode;

                                                    fsh.AircraftTypeName = fl.Equipment[0].AircraftTypeName;
                                                    fsh.AircraftTypeCode = fl.Equipment[0].AircraftTypeCode;

                                                    fsh.From = item.From;

                                                    fsh.WideBody = fl.Equipment[0].WideBody;
                                                    fsh.Jet = fl.Equipment[0].Jet;
                                                    fsh.DepartureDaysOfWeek = item1.DepartureDaysOfWeek;

                                                    fsh.Seat = fs.Seat;

                                                    _dbRepository.SaveFlightStatHiostory(fsh);
                                                    j += 1;
                                                    #endregion
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        message = "No data found!";
                                    }
                                }
                                catch (WebException webEx)
                                {
                                    result = "Error: " + webEx.Message + " Stack Trace: " + webEx.StackTrace;
                                }
                                catch (Exception ex)
                                {
                                    result = "Error: " + ex.Message + " Stack Trace: " + ex.StackTrace;
                                }
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        ItinerarySearch its = new ItinerarySearch();
                        its.Id = item.Id;
                        its.FlightID = item.FlightID;
                        its.OpportunityId = opportunityId;
                        its.FlightRequestID = flightRequestID;
                        its.AirlineCode = item.AirlineCode;
                        its.FlightNumber = item.FlightNumber;
                        its.From = item.From;
                        its.FromDate = item.FromDate;
                        its.Class = item.Class;
                        _dbRepository.SaveItinerarySearch(its);
                        //ViewData["Id"] = its.Id;
                        model.AddRange(model1);
                    }
                }
            }

            ViewData["Message"] = message;
            return View("UcItineraryBuilder", _dbRepository.GetFlightData(opportunityId, flightRequestID));
        }

        //[HttpPost]
        public ActionResult SaveSelectedFlightData(ItinerarySearch its, List<FlightStat> listFS, string id, string fsID, long ItinerarySearchID, string opportunityId, string flightRequestID)
        {
            #region Insert Serach Parameters to have available next time
            _dbRepository.SaveItinerarySearch(its);
            ViewBag.Id = its.Id;

            string[] ids = id.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            foreach (var item in listFS)
            {
                //if (ids.Contains(i.ToString()))
                //{
                item.ItinerarySearchID = its.Id;
                _dbRepository.SaveFlightStat(item);
                i++;
                //}
            }
            #endregion
            //_dbRepository.DeleteFlightStat(fsID, ItinerarySearchID);
            return View("UcItineraryBuilder", _dbRepository.GetFlightData(opportunityId, flightRequestID));
        }

        public void CancelSelectedFlightData(long ItinerarySearchID)
        {
            _dbRepository.DeleteItinerarySearch(ItinerarySearchID);
            //return View("UcItineraryBuilder", _dbRepository.GetFlightData(opportunityId, flightRequestID));
        }

        public string BuildItinerary(string oppId, string frID)
        {

            List<BLLModels.ItinerarySearch> list = _dbRepository.GetFlightData(oppId, frID);


            StringBuilder sb = new StringBuilder();
            foreach (var grp in list.GroupBy(p => p.FlightID))
            {

                string pcText = TemplateText();

                foreach (var its in list.Where(p => p.FlightID == grp.Key))
                {
                    UtilReadEmailHelper obj = new UtilReadEmailHelper(pcText);
                    obj.TagAndValues.Add("Flightid", grp.Key.ToString());
                    obj.TagAndValues.Add("Day", its.FlightStat.DepartureDateFrom.ToString("ddd"));
                    obj.TagAndValues.Add("Date", its.FlightStat.DepartureDateFrom.ToString("ddMMM"));

                    TimeSpan t = TimeSpan.FromMinutes(its.FlightStat.FlightDurationMinutes);
                    double hours = t.TotalHours;

                    obj.TagAndValues.Add("FlightDurationMinutes", (its.FlightStat.FlightDurationMinutes / 60).ToString());
                    obj.TagAndValues.Add("DistanceMiles", its.FlightStat.DistanceMiles.ToString());
                    obj.TagAndValues.Add("FlightNumber", its.FlightStat.FlightNumber);
                    obj.TagAndValues.Add("Flight", its.FlightStat.CarrierName);

                    DateTime DepartureTime = Convert.ToDateTime(its.FlightStat.DepartureTime);

                    obj.TagAndValues.Add("DepartureTime", DepartureTime.ToString("hh:mm tt"));
                    obj.TagAndValues.Add("DepartureAirportName", its.FlightStat.DepartureAirportName);
                    obj.TagAndValues.Add("DepartureAirportCity", its.FlightStat.DepartureAirportCity);
                    obj.TagAndValues.Add("DepartureAirportStateCode", its.FlightStat.DepartureAirportStateCode);
                    obj.TagAndValues.Add("DepartureAirportCountryCode", its.FlightStat.DepartureAirportCountryCode);

                    DateTime ArrivalTime = Convert.ToDateTime(its.FlightStat.ArrivalTime);

                    obj.TagAndValues.Add("ArrivalTime", ArrivalTime.ToString("hh:mm tt"));
                    obj.TagAndValues.Add("ArrivalAirportName", its.FlightStat.ArrivalAirportName);
                    obj.TagAndValues.Add("ArrivalAirportCity", its.FlightStat.ArrivalAirportCity);
                    obj.TagAndValues.Add("ArrivalAirportStateCode", its.FlightStat.ArrivalAirportStateCode);
                    obj.TagAndValues.Add("ArrivalAirportCountryCode", its.FlightStat.ArrivalAirportCountryCode);
                    obj.TagAndValues.Add("AircraftTypeName", its.FlightStat.AircraftTypeName);

                    sb.Append(obj.GetEmailTemplateBodyTxt());
                }
            }
            return sb.ToString();
        }

        string TemplateText()
        {
            string text = string.Empty;
            string dir = HttpContext.Server.MapPath("~/Content");
            string path = dir + "/" + "Template_FlightStat.htm";

            if (System.IO.File.Exists(path))
                text = System.IO.File.ReadAllText(path);

            return text;
        }

        public ActionResult DeleteFlightStat(long id, string opportunityId, string flightRequestID)
        {
            _dbRepository.DeleteFlightStatByID(id);
            return View("UcItineraryBuilder", _dbRepository.GetFlightData(opportunityId, flightRequestID));

        }

        private int RandomNumber(int min, int max)
        {
            Random random = new Random();
            int r = random.Next(min, max);
            return r;
        }

        public ActionResult GetCustomerView(string opportunityId, string flightRequestID)
        {
            return View("UcItineraryCustomerView", _dbRepository.GetFlightData(opportunityId, flightRequestID));
        }
    }
}
