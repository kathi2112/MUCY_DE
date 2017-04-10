using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using MUCY_DE.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MUCY_DE.Dialogs
{
    public class Flight_Dialogs
    {
        private static string locationStr;
        private static string flightNoStr;
        private static string airlineStr;
        private static string numberStr;

        public static string NextFlightTo(LuisResult result)
        {
            string resultMessage;

            var entities = new List<EntityRecommendation>(result.Entities);

            if (entities.Any((entity) => entity.Type == "Ort"))
            {
                var location = entities.Where((entity) => entity.Type == "Ort").First();
                locationStr = location.Entity ?? null;

                var restURL = "https://a2textpaid.azurewebsites.net/";
                var url = restURL + "GetNextFlightTo" + "?to=" + locationStr + "&language=de-DE";
                string responseBody = LUIS_Dialog_DE.GET(url);

                GetNextFlightTo.RootObject flightDetails = JsonConvert.DeserializeObject<GetNextFlightTo.RootObject>(responseBody);

                resultMessage = flightDetails.text;
            }
            else
            {
                resultMessage = "Wo willst du hin?";
            }
            return resultMessage;
        }     
    }
}