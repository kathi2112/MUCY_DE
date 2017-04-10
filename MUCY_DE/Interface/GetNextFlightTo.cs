using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MUCY_DE.Interface
{
    public class GetNextFlightTo
    {
        public class OperatingAirline
        {
            public string iataCode { get; set; }
            public string icaoCode { get; set; }
            public string name { get; set; }
        }

        public class AircraftType
        {
            public string icaoCode { get; set; }
            public string modelName { get; set; }
        }

        public class FlightNumber
        {
            public string airlineCode { get; set; }
            public string trackNumber { get; set; }
        }

        public class CodeShare
        {
            public string airlineCode { get; set; }
            public string trackNumber { get; set; }
        }

        public class CheckinInfo
        {
            public string checkinLocation { get; set; }
        }

        public class Departure
        {
            public string scheduled { get; set; }
            public string actual { get; set; }
            public string terminal { get; set; }
            public string gate { get; set; }
            public CheckinInfo checkinInfo { get; set; }
            public List<object> boardingTime { get; set; }
        }

        public class BaggageClaim
        {
        }

        public class Arrival
        {
            public string scheduled { get; set; }
            public string actual { get; set; }
            public BaggageClaim baggageClaim { get; set; }
        }

        public class Content
        {
            public OperatingAirline operatingAirline { get; set; }
            public AircraftType aircraftType { get; set; }
            public FlightNumber flightNumber { get; set; }
            public List<CodeShare> codeShares { get; set; }
            public string departureAirport { get; set; }
            public string arrivalAirport { get; set; }
            public string originDate { get; set; }
            public Departure departure { get; set; }
            public Arrival arrival { get; set; }
            public string flightStatus { get; set; }
            public List<object> via { get; set; }
        }

        public class RootObject
        {
            public string type { get; set; }
            public string language { get; set; }
            public string text { get; set; }
            public Content content { get; set; }
        }
    }
}