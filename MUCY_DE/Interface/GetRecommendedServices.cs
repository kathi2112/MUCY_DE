using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MUCY_DE.Interface
{
    public class GetRecommendedServices
    {
        public class ServiceBulletpoint
        {
            public string title { get; set; }
            public int order { get; set; }
            public List<string> elements { get; set; }
        }

        public class TitleImage
        {
            public string low { get; set; }
            public string medium { get; set; }
            public string high { get; set; }
        }

        public class TitleImageSmall
        {
        }

        public class DesciptionImageVertical
        {
            public string low { get; set; }
            public string medium { get; set; }
            public string high { get; set; }
        }

        public class DescriptionImageHorizontal
        {
            public string low { get; set; }
            public string medium { get; set; }
            public string high { get; set; }
        }

        public class Contact
        {
            [JsonProperty("extended-address")]
            public string extendedAddress { get; set; }
            [JsonProperty("street-address")]
            public string streetAddress { get; set; }
            public string locality { get; set; }
            [JsonProperty("postal-code")]
            public string postalCode { get; set; }
            [JsonProperty("country-name")]
            public string countryName { get; set; }
        }


        public class Provider
        {
            public string phone { get; set; }
            public Contact contact { get; set; }
            public string email { get; set; }
        }

        public class MapImage
        {
            public string low { get; set; }
            public string medium { get; set; }
            public string high { get; set; }
        }

        public class OpeningHour
        {
            public string column1 { get; set; }
            public string column2 { get; set; }
        }

        public class Location
        {
            public int x { get; set; }
            public int locationId { get; set; }
            public int y { get; set; }
            public int z { get; set; }
            public string humanReadable { get; set; }
            public string area { get; set; }
            public string externalId { get; set; }
            public MapImage mapImage { get; set; }
            public List<OpeningHour> openingHours { get; set; }
        }

        public class Content
        {
            public string title { get; set; }
            public string subTitle { get; set; }
            public string description { get; set; }
            public bool isBookable { get; set; }
            public List<ServiceBulletpoint> serviceBulletpoints { get; set; }
            public List<object> serviceTables { get; set; }
            public List<object> serviceTexts { get; set; }
            public TitleImage titleImage { get; set; }
            public TitleImageSmall titleImage_small { get; set; }
            public DesciptionImageVertical desciptionImage_vertical { get; set; }
            public DescriptionImageHorizontal descriptionImage_horizontal { get; set; }
            public List<object> downloads { get; set; }
            public List<object> specials { get; set; }
            public Provider provider { get; set; }
            public int serviceId { get; set; }
            public int airportId { get; set; }
            public List<Location> locations { get; set; }
            public string text { get; set; }
        }

        public class RootObject
        {
            public string type { get; set; }
            public string language { get; set; }
            public string text { get; set; }
            public List<Content> content { get; set; }
        }
    }
}