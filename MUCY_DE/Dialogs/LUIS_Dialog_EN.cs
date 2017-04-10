namespace MUCY_DE.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using Microsoft.Bot.Connector;
    using System.Net;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;
    using System.Web;
    using System.Threading;
    using MUCY_DE.Interface;

    [Serializable]
    [LuisModel("beb7fad7-eef4-4ceb-bcac-b36321213339", "eb7c516583574a018d247f071883488f")]
    public class LUIS_Dialog_EN : LuisDialog<object>
    {
        Random rnd = new Random();
        private string flightNoStr;
        private string locationStr;
        private string airlineStr;
        private string numberStr;
        private string serviceStr;
        

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        { 
            await context.PostAsync("Sorry, I didn't understand what you wanted.");
            context.Wait(MessageReceived);
        }


        [LuisIntent("GetMyFlight")]
        public async Task GetMyFlight(IDialogContext context, LuisResult result)
        {
            var entities = new List<EntityRecommendation>(result.Entities);

            if (entities.Any((entity) => entity.Type == "Flightnumber"))
            {
                var flugnummer = entities.Where((entity) => entity.Type == "Flightnumber").First();
                flightNoStr = flugnummer.Entity ?? null;

                airlineStr = flightNoStr.Substring(0, 2);
                numberStr = flightNoStr.Substring(2, flightNoStr.Length - 2);

                var restURL = "https://a2textpaid.azurewebsites.net/";
                var url = restURL + "GetMyFlight?airline=" + airlineStr + "&flightnumber=" + numberStr + "&language=en-US";
                string responseBody = GET(url);

                GetMyFlight.RootObject flightDetails = JsonConvert.DeserializeObject<GetMyFlight.RootObject>(responseBody);

                await context.PostAsync(flightDetails.text);

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                HeroCard heroCard = new HeroCard()
                {
                    Title = flightNoStr.ToUpper(),
                    Subtitle = "From " + flightDetails.content.departureAirport + " to " + flightDetails.content.arrivalAirport,
                    Images = new List<CardImage>()
                    {
                     new CardImage() { Url = "http://bilder.t-online.de/b/77/27/34/42/id_77273442/610/tid_da/auftrieb-und-geschwindigkeit-sorgen-dafuer-dass-das-flugzeug-in-der-luft-bleibt-.jpg" }
                    },
                    Buttons = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Title = "More details",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.bing.com/search?q=" + HttpUtility.UrlEncode(flightNoStr)
                        }
                    }
                };
                resultMessage.Attachments.Add(heroCard.ToAttachment());
                await context.PostAsync(resultMessage);
            }

            if (entities.Any((entity) => entity.Type == "Airline"))
            {
                var airline = entities.Where((entity) => entity.Type == "Airline").First();
                airlineStr = airline.Entity ?? null;

                if (entities.Any((entity) => entity.Type == "builtin.number"))
                {
                    var number = entities.Where((entity) => entity.Type == "builtin.number").First();
                    numberStr = number.Entity ?? null;

                    var restURL = "https://a2textpaid.azurewebsites.net/";
                    var url = restURL + "GetMyFlight?airline=" + airlineStr + "&flightnumber=" + numberStr + "&language=en-US";
                    string responseBody = GET(url);

                    GetMyFlight.RootObject flightDetails = JsonConvert.DeserializeObject<GetMyFlight.RootObject>(responseBody);

                    await context.PostAsync(flightDetails.text);

                    var resultMessage = context.MakeMessage();
                    resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    resultMessage.Attachments = new List<Attachment>();

                    HeroCard heroCard = new HeroCard()
                    {
                        Title = airlineStr + numberStr,
                        Subtitle = "From " + flightDetails.content.departureAirport + " to " + flightDetails.content.arrivalAirport,
                        Images = new List<CardImage>()
                    {
                     new CardImage() { Url = "http://bilder.t-online.de/b/77/27/34/42/id_77273442/610/tid_da/auftrieb-und-geschwindigkeit-sorgen-dafuer-dass-das-flugzeug-in-der-luft-bleibt-.jpg" }
                    },
                        Buttons = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Title = "More details",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.bing.com/search?q=" + HttpUtility.UrlEncode(airlineStr + numberStr)
                        }
                    }
                    };
                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                    await context.PostAsync(resultMessage);
                }
            }

            else if(flightNoStr == null || airlineStr == null)
            {
                await context.PostAsync("Please tell me your flightnumber.");
            }
            context.Wait(this.MessageReceived);
            flightNoStr = null;
            airlineStr = null;
            numberStr = null;
        }


        [LuisIntent("GetNextFlightTo")]
        public async Task GetNextFlightTo(IDialogContext context, LuisResult result)
        {
            var entities = new List<EntityRecommendation>(result.Entities);

            if (entities.Any((entity) => entity.Type == "Location"))
            {
                var location = entities.Where((entity) => entity.Type == "Location").First();
                locationStr = location.Entity ?? null;

                var restURL = "https://a2textpaid.azurewebsites.net/";
                var url = restURL + "GetNextFlightTo" + "?to=" + locationStr + "&language=en-US";
                string responseBody = GET(url);

                GetNextFlightTo.RootObject flightDetails = JsonConvert.DeserializeObject<GetNextFlightTo.RootObject>(responseBody);

                await context.PostAsync(flightDetails.text);
            }
            else
            {
                await context.PostAsync("To which location do you want to go?");
            }
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("GetNextFlight")]
        public async Task GetNextFlight(IDialogContext context, LuisResult result)
        {
            var entities = new List<EntityRecommendation>(result.Entities);

            if (entities.Any((entity) => entity.Type == "builtin.number"))
            {
                var number = entities.Where((entity) => entity.Type == "builtin.number").First();
                numberStr = number.Entity ?? null;

                var restURL = "https://a2textpaid.azurewebsites.net/";
                var url = restURL + "GetNextFlight" + "?arrivaldeparture=departure&length=" + number + "&language=en-US";
                string responseBody = GET(url);

                GetNextFlight.RootObject flightDetails = JsonConvert.DeserializeObject<GetNextFlight.RootObject>(responseBody);

                foreach (GetNextFlight.Content c in flightDetails.content)
                {
                    await context.PostAsync(c.flightNumber.ToString());
                }
                
                context.Wait(this.MessageReceived);
            }   
            
            else
            {
                var restURL = "https://a2textpaid.azurewebsites.net/";
                var url = restURL + "GetNextFlight" + "?arrivaldeparture=departure&length=5&language=en-US";
                string responseBody = GET(url);

                GetNextFlight.RootObject flightDetails = JsonConvert.DeserializeObject<GetNextFlight.RootObject>(responseBody);

                await context.PostAsync(flightDetails.text + " ...available soon.");
                context.Wait(this.MessageReceived);
            }
        }


        [LuisIntent("GetServiceBySearch")]
        public async Task GetServiceBySearch(IDialogContext context, LuisResult result)
        {
            var entities = new List<EntityRecommendation>(result.Entities);

            if (entities.Any((entity) => entity.Type == "Services"))
            {
                var service = entities.Where((entity) => entity.Type == "Services").First();
                serviceStr = service.Entity ?? null;

                var restURL = "https://a2textpaid.azurewebsites.net/";
                var url = restURL + "GetServiceBySearch" + "?search=" + serviceStr + "&language=en-US";
                string responseBody = GET(url);

                GetServiceBySearch.RootObject serviceDetails = JsonConvert.DeserializeObject<GetServiceBySearch.RootObject>(responseBody);

                await context.PostAsync(serviceDetails.text + ": " + serviceDetails.content.text);
            }

            else
            {
                await context.PostAsync("What service do you want to search for?");
            }
                context.Wait(this.MessageReceived);
        }


        [LuisIntent("GetRecommendedServices")]
        public async Task GetRecommendedServices(IDialogContext context, LuisResult result)
        {
            var restURL = "https://a2textpaid.azurewebsites.net/";
            var url = restURL + "GetRecommendedServices?language=en-US";
            string responseBody = GET(url);

            GetRecommendedServices.RootObject serviceDetails = JsonConvert.DeserializeObject<GetRecommendedServices.RootObject>(responseBody);

            var resultMessage = context.MakeMessage();
            resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            resultMessage.Attachments = new List<Attachment>();

            List<CardAction> cards = new List<CardAction>();
            foreach (GetRecommendedServices.Content c in serviceDetails.content)
            {
                cards.Add(new CardAction()
                {
                    Title = c.title
                });
                if (c.descriptionImage_horizontal.low != null)
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = c.title,
                        Text = c.description,
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = c.descriptionImage_horizontal.low }
                        },
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "More details",
                                Type = ActionTypes.OpenUrl,
                                Value = $"https://www.munich-airport.de/suchergebnisseite-75217?utf8=%E2%9C%93&search_form_presenter[commit]=1&search_form_presenter[profile]=site_search&search_form_presenter[search_term]="+c.title
                            }
                        }
                    };
                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                }
                else
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = c.title,
                        Text = c.description,
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = c.titleImage.low }
                        },
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "More details",
                                Type = ActionTypes.OpenUrl,
                                Value = $"https://www.munich-airport.de/suchergebnisseite-75217?utf8=%E2%9C%93&search_form_presenter[commit]=1&search_form_presenter[profile]=site_search&search_form_presenter[search_term]="+c.title
                            }
                        }
                    };
                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                }
                
            }
                await context.PostAsync(resultMessage);
                context.Wait(this.MessageReceived);   
        }


        [LuisIntent("Age")]
        public async Task Age(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Well, I was implemented in 2017 at the Munich Airport.");
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("Hobbies")]
        public async Task Hobbies(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I love to travel around. But my favorite place is still Munich Airport.");
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("Location")]
        public async Task Location(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("As you can imagine, I live at the airport in Munich. I'm placed in the IT.");
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("State")]
        public async Task State(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Thanks for asking, I'm doing well at the moment. :)");
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("Time")]
        public async Task Time(IDialogContext context, LuisResult result)
        {
            DateTime time;
            time = DateTime.Now;
            string timeStr = time.ToString("HH:mm");

            await context.PostAsync("At the moment it is " + timeStr + ".");
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("ParkingReservation")]
        public async Task ParkingReservation(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("You can easily reserve a parking place on our homepage.");
            Thread.Sleep(2000);
            await context.PostAsync("Here, I give you the link: https://www.munich-airport.de/parken-89995");
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("Career")]
        public async Task Career(IDialogContext context, LuisResult result)
        {
            var resultMessage = context.MakeMessage();
            resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            resultMessage.Attachments = new List<Attachment>();

            HeroCard heroCard = new HeroCard()
            {
                Title = "Career at Munich Airport",
                Subtitle = "Here are the offered career options for ...",
                Images = new List<CardImage>()
                    {
                     new CardImage() { Url = "https://www.munich-airport.de/_b/0000000000000000293718bb582da737/it-mitarbeiter-computer.jpg" }
                    },
                Buttons = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Title = "Career starter/experienced",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/berufseinsteiger-erfahrene-94612"
                        },
                        new CardAction()
                        {
                            Title = "University graduate",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/trainee-94642"
                        },
                        new CardAction()
                        {
                            Title = "Student",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/studenten-94672"
                        },
                        new CardAction()
                        {
                            Title = "Pupil",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/schuler-94701"
                        },
                        new CardAction()
                        {
                            Title = "Job offers",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/stellenangebote-94732"
                        }
                    }
            };
            resultMessage.Attachments.Add(heroCard.ToAttachment());
            await context.PostAsync(resultMessage);
        }


        [LuisIntent("Navigation")]
        public async Task Navigation(IDialogContext context, LuisResult result)
        {
            var entities = new List<EntityRecommendation>(result.Entities);

            if (entities.Any((entity) => entity.Type == "Location"))
            {
                var location = entities.Where((entity) => entity.Type == "Location").First();
                locationStr = location.Entity ?? null;

                await context.PostAsync("Your route will be charged.");
                Thread.Sleep(2000);
                await context.PostAsync("https://www.google.de/maps/dir/" + locationStr + "/Flughafen+Muenchen");
            }

            else
            {
                await context.PostAsync("From where do you want to go to the airport?");
            }
            
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("Events")]
        public async Task Events(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Here are the actual events at Munich Airport.");
            Thread.Sleep(2000);
            await context.PostAsync("https://www.munich-airport.com/events-1455138 - Have fun! :)");
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("FMG")]
        public async Task FMG(IDialogContext context, LuisResult result)
        {
            var resultMessage = context.MakeMessage();
            resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            resultMessage.Attachments = new List<Attachment>();

            string FMG = Convert.ToString("FMG");

            HeroCard heroCardFMG = new HeroCard()
            {
                Title = "FMG",
                Text = "What do you want to ask?",
                Images = new List<CardImage>()
                {
                 new CardImage() { Url = "https://www.munich-airport.de/_b/0000000000000001220017bb58904389/flughafen-muenchen-konzern11.jpg?t=eyJoZWlnaHQiOjQ1Nywid2lkdGgiOjgwMCwicXVhbGl0eSI6NzV9--8e1738114267304cbf3c9b0e695f3619db058d76" }
                },
                Buttons = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Title = "Most important issues",
                            Type = "imBack",
                            Value = "What are currently the most important strategic issues for the airport?"
                        },
                        new CardAction()
                        {
                            Title = "What's digitization?",
                            Type = "imBack",
                            Value = "What is digitization?"
                        },
                        new CardAction()
                        {
                            Title = "Digitization for FMG",
                            Type = "imBack",
                            Value = "What means digitization for the airport?"
                        },
                }                          
            };
            resultMessage.Attachments.Add(heroCardFMG.ToAttachment());
            await context.PostAsync(resultMessage);

            context.Wait(this.MessageReceived);
        }


        [LuisIntent("FMGSubjects")]
        public async Task FMGSubjects(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("(1) The expansion of terminal 1");
            Thread.Sleep(2000);
            await context.PostAsync("(2) The development of Lab Campus");
            Thread.Sleep(2000);
            await context.PostAsync("(3) The digitization");

            context.Wait(this.MessageReceived);
        }


        [LuisIntent("Digitalization")]
        public async Task Digitalization(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("The term 'digitization' refers to the conversion of analog quantities into discrete (graded) values,"+
                " for the purpose of storing or processing them electronically. The end product or result of the digitization is sometimes" +
                " referred to as digitization. In a broader sense, the term is also referred to as the change to electronically supported" +
                " processes by means of information and communication technology.");

            context.Wait(this.MessageReceived);
        }


        [LuisIntent("FMGDigitalization")]
        public async Task FMGDigitalization(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("(1) B2C - Change of communication with the passenger");
            Thread.Sleep(2000);
            await context.PostAsync("(2) IoT -  IT and technology systems are growing together");
            Thread.Sleep(2000);
            await context.PostAsync("(3) Data Fusion & Analytics gets much more value");
            Thread.Sleep(2000);
            await context.PostAsync("(4) Automation will increase significantly");

            context.Wait(this.MessageReceived);
        }


        [LuisIntent("Thank")]
        public async Task Thank(IDialogContext context, LuisResult result)
        {
            string[] thanks =
                {
                    "You are welcome. :)",
                    "My pleasure. :)",
                    "Don't mention it. :)"
                };

            int i = rnd.Next(0, thanks.Length);

            await context.PostAsync(thanks[i]);
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("Joke")]
        public async Task Joke(IDialogContext context, LuisResult result)
        {
            string[] jokes =
                {
                    "What do you call a plane that's about to crash?",
                    "Why don`t ducks tell jokes when they fly?"
                };
            string[] answers =
                {
                    "An 'Error Plane'.",
                    "Because they would quack up."
                };

            int i = rnd.Next(0, jokes.Length);

            await context.PostAsync(jokes[i]);
            Thread.Sleep(5000);
            await context.PostAsync(answers[i]);
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hi! I'm MUCY. Give me your flightnumber for information. " +
              "I can also navigate you to our homepage for booking a parking place or for career information. Or you can try asking me something else. :)");


            var resultMessage = context.MakeMessage();
            resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            resultMessage.Attachments = new List<Attachment>();

            HeroCard heroCardFlight = new HeroCard()
            {
                Title = "Flight",
                Images = new List<CardImage>()
                {
                 new CardImage() { Url = "https://www.munich-airport.de/_b/0000000000000000309917bb582f0564/besucherterrasse-3.jpg?t=eyJoZWlnaHQiOjU5NCwid2lkdGgiOjEwNDAsInF1YWxpdHkiOjc1fQ==--8bc9b0db14da84b92a0700825434d855749ba85f" }
                },
                Buttons = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Title = "Details for my flight",
                            Type = "imBack",
                            Value = "Details for my flight"
                        },
                        new CardAction()
                        {
                            Title = "Next departure flights",
                            Type = "imBack",
                            Value = "Next departure flights"
                        },
                        new CardAction()
                        {
                            Title = "Next flight to...",
                            Type = "imBack",
                            Value = "Next flight to..."
                        }
                    }
            };

            HeroCard heroCardService = new HeroCard()
            {
                Title = "Services",
                Images = new List<CardImage>()
                    {
                     new CardImage() { Url = "https://www.munich-airport.de/_b/0000000000000000300370bb582dbbae/gastro-dallmayr-t2-kellner.jpg?t=eyJoZWlnaHQiOjU5NCwid2lkdGgiOjEwNDAsInF1YWxpdHkiOjc1fQ==--8bc9b0db14da84b92a0700825434d855749ba85f" }
                    },
                Buttons = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Title = "Recommended services",
                            Type = "imBack",
                            Value = "Recommended services"
                        },
                        new CardAction()
                        {
                            Title = "Search service for ...",
                            Type = "imBack",
                            Value = "Search service for ..."
                        },
                        new CardAction()
                        {
                            Title = "Events",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.com/events-1455138"
                        },
                    }
            };

            HeroCard heroCardCareer = new HeroCard()
            {
                Title = "Career at Munich Airport",
                Images = new List<CardImage>()
                    {
                     new CardImage() { Url = "https://www.munich-airport.de/_b/0000000000000000293718bb582da737/it-mitarbeiter-computer.jpg" }
                    },
                Buttons = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Title = "Career starter/experienced",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/berufseinsteiger-erfahrene-94612"
                        },
                        new CardAction()
                        {
                            Title = "University graduate",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/trainee-94642"
                        },
                        new CardAction()
                        {
                            Title = "Student",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/studenten-94672"
                        },
                        new CardAction()
                        {
                            Title = "Pupil",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/schuler-94701"
                        }
                    }
            };

            HeroCard heroCardNavigation = new HeroCard()
            {
                Title = "Navigation",
                Images = new List<CardImage>()
                    {
                     new CardImage() { Url = "https://www.munich-airport.com/_b/0000000000000000416174bb58413a89/anreise-mit-transferdienst.jpg?t=eyJoZWlnaHQiOjgwMCwid2lkdGgiOjE0MDAsInF1YWxpdHkiOjc1fQ==--10cc77c47a7474cc6d92316be562f49e0d955ef7" }
                    },
                Buttons = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Title = "Parking place",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/parken-89995"
                        },
                        new CardAction()
                        {
                            Title = "Navigate me from ...",
                            Type = "imBack",
                            Value = "Navigate me from ..."
                        },
                        new CardAction()
                        {
                            Title = "Travel by train/bus",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/offentliche-verkehrsmittel-90215"
                        },
                }
            };
            resultMessage.Attachments.Add(heroCardFlight.ToAttachment());
            resultMessage.Attachments.Add(heroCardService.ToAttachment());
            resultMessage.Attachments.Add(heroCardNavigation.ToAttachment());
            resultMessage.Attachments.Add(heroCardCareer.ToAttachment());
            await context.PostAsync(resultMessage);

            context.Wait(this.MessageReceived);
        }

        

        // Returns JSON string
        string GET(string url)
        {
            Console.WriteLine(url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("apiKey:Oh3FXkqV0B7lOyZSy7gYdsLYNKTeTXNfCRm");
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    string errorText = reader.ReadToEnd();
                    // log errorText
                }
                throw;
            }
        }

    }
}