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
using MUCY_DE.Interface;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Web;
using System.Threading;

namespace MUCY_DE.Dialogs
{
    [Serializable]
    [LuisModel("32bd82a4-8c24-44f4-ae89-e3dd43c0f5c7", "eb7c516583574a018d247f071883488f")]
    public class LUIS_Dialog_DE : LuisDialog<object>
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
            var response = Responses.none;
            await context.PostAsync(response);
            context.Wait(MessageReceived);
        }


        [LuisIntent("Hallo")]
        public async Task Hallo(IDialogContext context, LuisResult result)
        {
            var resultMessage = context.MakeMessage();
            resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            resultMessage.Attachments = new List<Attachment>();

            ThumbnailCard card = new ThumbnailCard()
            {
                Title = Responses.helloTitle,
                Text = Responses.helloText,
                Images = new List<CardImage>()
                {
                 new CardImage() { Url = "http://www.designtagebuch.de/wp-content/uploads/mediathek//2013/11/flughafen_muenchen-markenzeichen1-700x622.jpg" }
                },
                Buttons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Title = "Hilfe",
                        Type = "imBack",
                        Value = "Hilfe"
                    }
                }
            };
            resultMessage.Attachments.Add(card.ToAttachment());
            await context.PostAsync(resultMessage);
            context.Wait(MessageReceived);
        }


        [LuisIntent("GetMyFlight")]
        public async Task GetMyFlight(IDialogContext context, LuisResult result)
        {
            var entities = new List<EntityRecommendation>(result.Entities);

            if (entities.Any((entity) => entity.Type == "Flugnummer"))
            {
                var flugnummer = entities.Where((entity) => entity.Type == "Flugnummer").First();
                flightNoStr = flugnummer.Entity ?? null;

                airlineStr = flightNoStr.Substring(0, 2);
                numberStr = flightNoStr.Substring(2, flightNoStr.Length - 2);

                var restURL = "https://a2textpaid.azurewebsites.net/";
                var url = restURL + "GetMyFlight?airline=" + airlineStr + "&flightnumber=" + numberStr + "&language=de-DE";
                string responseBody = GET(url);

                GetMyFlight.RootObject flightDetails = JsonConvert.DeserializeObject<GetMyFlight.RootObject>(responseBody);

                if (flightDetails.text.Contains("falsch") || flightDetails.text.Contains("wiederholen"))
                {
                    await context.PostAsync(flightDetails.text);
                }

                else
                {
                    await context.PostAsync(flightDetails.text);

                    var resultMessage = context.MakeMessage();
                    resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    resultMessage.Attachments = new List<Attachment>();

                    HeroCard heroCard = new HeroCard()
                    {
                        Title = flightNoStr.ToUpper(),
                        Subtitle = "Von " + flightDetails.content.departureAirport + " nach " + flightDetails.content.arrivalAirport,
                        Images = new List<CardImage>()
                    {
                     new CardImage() { Url = "http://bilder.t-online.de/b/77/27/34/42/id_77273442/610/tid_da/auftrieb-und-geschwindigkeit-sorgen-dafuer-dass-das-flugzeug-in-der-luft-bleibt-.jpg" }
                    },
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "Mehr",
                                Type = ActionTypes.OpenUrl,
                                Value = $"https://www.bing.com/search?q=" + HttpUtility.UrlEncode(flightNoStr)
                            }
                        }
                    };
                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                    await context.PostAsync(resultMessage);
                }
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
                    var url = restURL + "GetMyFlight?airline=" + airlineStr + "&flightnumber=" + numberStr + "&language=de-DE";
                    string responseBody = GET(url);

                    GetMyFlight.RootObject flightDetails = JsonConvert.DeserializeObject<GetMyFlight.RootObject>(responseBody);

                    if(flightDetails.text.Contains("falsch") || flightDetails.text.Contains("wiederholen"))
                    {
                        await context.PostAsync(flightDetails.text);
                    }

                    else
                    {
                        await context.PostAsync(flightDetails.text);

                        var resultMessage = context.MakeMessage();
                        resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        resultMessage.Attachments = new List<Attachment>();

                        HeroCard heroCard = new HeroCard()
                        {
                            Title = airlineStr + numberStr,
                            Subtitle = "Von " + flightDetails.content.departureAirport + " nach " + flightDetails.content.arrivalAirport,
                            Images = new List<CardImage>()
                            {
                             new CardImage() { Url = "http://bilder.t-online.de/b/77/27/34/42/id_77273442/610/tid_da/auftrieb-und-geschwindigkeit-sorgen-dafuer-dass-das-flugzeug-in-der-luft-bleibt-.jpg" }
                            },
                            Buttons = new List<CardAction>()
                            {
                                new CardAction()
                                {
                                    Title = "Mehr",
                                    Type = ActionTypes.OpenUrl,
                                    Value = $"https://www.bing.com/search?q=" + HttpUtility.UrlEncode(airlineStr + numberStr)
                                }
                            }
                        };
                        resultMessage.Attachments.Add(heroCard.ToAttachment());
                        await context.PostAsync(resultMessage);
                    }
                }      
            }

            else if (flightNoStr == null || airlineStr == null)
            {
                await context.PostAsync("Bitte sag mir deine Flugnummer.");
            }
            context.Wait(MessageReceived);
        }


        [LuisIntent("Gate")]
        public async Task Gate(IDialogContext context, LuisResult result)
        {
            var entities = new List<EntityRecommendation>(result.Entities);

            if (entities.Any((entity) => entity.Type == "Flugnummer"))
            {
                var flugnummer = entities.Where((entity) => entity.Type == "Flugnummer").First();
                flightNoStr = flugnummer.Entity ?? null;

                airlineStr = flightNoStr.Substring(0, 2);
                numberStr = flightNoStr.Substring(2, flightNoStr.Length - 2);

                var restURL = "https://a2textpaid.azurewebsites.net/";
                var url = restURL + "GetMyFlight?airline=" + airlineStr + "&flightnumber=" + numberStr + "&language=de-DE";
                string responseBody = GET(url);

                GetMyFlight.RootObject flightDetails = JsonConvert.DeserializeObject<GetMyFlight.RootObject>(responseBody);

                if (flightDetails.text.Contains("falsch") || flightDetails.text.Contains("wiederholen"))
                {
                    await context.PostAsync(flightDetails.text);
                }

                else
                {
                    await context.PostAsync("Der Flug " + flightNoStr.ToUpper() + " ist am Gate " + flightDetails.content.departure.gate);
                }
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
                    var url = restURL + "GetMyFlight?airline=" + airlineStr + "&flightnumber=" + numberStr + "&language=de-DE";
                    string responseBody = GET(url);

                    GetMyFlight.RootObject flightDetails = JsonConvert.DeserializeObject<GetMyFlight.RootObject>(responseBody);

                    if (flightDetails.text.Contains("falsch") || flightDetails.text.Contains("wiederholen"))
                    {
                        await context.PostAsync(flightDetails.text);
                    }

                    else
                    {
                        await context.PostAsync("Der Flug " + airlineStr + " " + numberStr + " ist am Gate " + flightDetails.content.departure.gate);
                    }
                }
            }

            else if (flightNoStr == null || airlineStr == null)
            {
                await context.PostAsync("Bitte sag mir deine Flugnummer.");
            }
            context.Wait(MessageReceived);
        }


        [LuisIntent("GetNextFlightTo")]
        public async Task GetNextFlightTo(IDialogContext context, LuisResult result)
        {
            var resultMessage = Flight_Dialogs.NextFlightTo(result);
            await context.PostAsync(resultMessage);
            context.Wait(MessageReceived);
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
                var url = restURL + "GetNextFlight" + "?arrivaldeparture=departure&length=" + number + "&language=de-DE";
                string responseBody = GET(url);

                GetNextFlight.RootObject flightDetails = JsonConvert.DeserializeObject<GetNextFlight.RootObject>(responseBody);

                foreach (GetNextFlight.Content c in flightDetails.content)
                {
                    await context.PostAsync("bald verfügbar.");
                }

                context.Wait(MessageReceived);
            }

            else
            {
                var restURL = "https://a2textpaid.azurewebsites.net/";
                var url = restURL + "GetNextFlight" + "?arrivaldeparture=departure&length=5&language=de-DE";
                string responseBody = GET(url);

                GetNextFlight.RootObject flightDetails = JsonConvert.DeserializeObject<GetNextFlight.RootObject>(responseBody);

                await context.PostAsync("bald verfügbar.");
                context.Wait(MessageReceived);
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
                var url = restURL + "GetServiceBySearch" + "?search=" + serviceStr + "&language=de-DE";
                string responseBody = GET(url);

                GetServiceBySearch.RootObject serviceDetails = JsonConvert.DeserializeObject<GetServiceBySearch.RootObject>(responseBody);

                await context.PostAsync(serviceDetails.text + " " + serviceDetails.content.text);
            }

            else
            {
                await context.PostAsync("Welchen Service möchtest du?");
            }
            context.Wait(MessageReceived);
        }


        [LuisIntent("GetRecommendedServices")]
        public async Task GetRecommendedServices(IDialogContext context, LuisResult result)
        {
            var restURL = "https://a2textpaid.azurewebsites.net/";
            var url = restURL + "GetRecommendedServices?language=de-DE";
            string responseBody = GET(url);

            GetRecommendedServices.RootObject serviceDetails = JsonConvert.DeserializeObject<GetRecommendedServices.RootObject>(responseBody);

            var resultMessage = context.MakeMessage();
            resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            resultMessage.Attachments = new List<Attachment>();

            foreach (GetRecommendedServices.Content c in serviceDetails.content)
            {
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
                                Title = "Mehr",
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
                                Title = "Mehr",
                                Type = ActionTypes.OpenUrl,
                                Value = $"https://www.munich-airport.de/suchergebnisseite-75217?utf8=%E2%9C%93&search_form_presenter[commit]=1&search_form_presenter[profile]=site_search&search_form_presenter[search_term]="+c.title
                            }
                        }
                    };
                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                }

            }
            await context.PostAsync(resultMessage);
            context.Wait(MessageReceived);
        }


        [LuisIntent("Parkplatz")]
        public async Task Parkplatz(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Du kannst ganz einfach auf unserer Internetseite einen Parkplatz buchen.");
            Thread.Sleep(2000);
            await context.PostAsync("Hier ist der Link dazu: https://www.munich-airport.de/parken-89995");
            context.Wait(MessageReceived);
        }


        [LuisIntent("Karriere")]
        public async Task Karriere(IDialogContext context, LuisResult result)
        {
            var resultMessage = context.MakeMessage();
            resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            resultMessage.Attachments = new List<Attachment>();

            HeroCard heroCardKarriere = new HeroCard()
            {
                Title = "Karriere am Flughafen",
                Images = new List<CardImage>()
                    {
                     new CardImage() { Url = "https://www.munich-airport.de/_b/0000000000000000293718bb582da737/it-mitarbeiter-computer.jpg" }
                    },
                Buttons = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Title = "Einsteiger/Erfahrene",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/berufseinsteiger-erfahrene-94612"
                        },
                        new CardAction()
                        {
                            Title = "Trainee",
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
                            Title = "Schüler",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/schuler-94701"
                        },
                        new CardAction()
                        {
                            Title = "Stellenangebote",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/stellenangebote-94732"
                        }
                    }
            };
            resultMessage.Attachments.Add(heroCardKarriere.ToAttachment());
            await context.PostAsync(resultMessage);
            context.Wait(MessageReceived);
        }


        [LuisIntent("Navigation")]
        public async Task Navigation(IDialogContext context, LuisResult result)
        {
            var entities = new List<EntityRecommendation>(result.Entities);

            if (entities.Any((entity) => entity.Type == "Ort"))
            {
                var location = entities.Where((entity) => entity.Type == "Ort").First();
                locationStr = location.Entity ?? null;

                await context.PostAsync("Deine Route wird erstellt.");
                Thread.Sleep(2000);
                await context.PostAsync("https://www.google.de/maps/dir/" + locationStr + "/Flughafen+Muenchen");
            }

            else
            {
                await context.PostAsync("Von welcher Stadt aus willst du zum Flughafen?");
            }

            context.Wait(MessageReceived);
        }


        [LuisIntent("Events")]
        public async Task Events(IDialogContext context, LuisResult result)
        {
            var resultMessage = context.MakeMessage();
            resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            resultMessage.Attachments = new List<Attachment>();

            HeroCard card1 = new HeroCard()
            {
                Title = "Aussichtsterrasse",
                Text = "Im Terminal 2 gibt es eine kostenlose Aussichtsterrasse mit Blick auf das Vorfeld und das neue Satellitengebäude.",
                Images = new List<CardImage>()
                {
                 new CardImage() { Url = "https://www.munich-airport.de/_b/0000000000000000968171bb587df491/ATF9913.jpg?t=eyJoZWlnaHQiOjU5NCwid2lkdGgiOjEwNDAsInF1YWxpdHkiOjc1fQ==--8bc9b0db14da84b92a0700825434d855749ba85f" }
                }
            };
            HeroCard card2 = new HeroCard()
            {
                Title = "Besucherpark",
                Text = "Besucherhügel - Tante Ju - Erlebnisspielplatz. Der nächste Familienausflug kann kommen! Für Souveniers ist auch gesorgt.",
                Images = new List<CardImage>()
                {
                 new CardImage() { Url = "https://www.munich-airport.de/_b/0000000000000000308893bb582ef909/besucherpark-luftbild.jpg?t=eyJoZWlnaHQiOjU5NCwid2lkdGgiOjEwNDAsInF1YWxpdHkiOjc1fQ==--8bc9b0db14da84b92a0700825434d855749ba85f" }
                }
            };
            HeroCard card3 = new HeroCard()
            {
                Title = "Airport-Touren",
                Text = "Wie kommt der Koffer in den Flieger? Wie wird getankt? Woher kommt das Frischwasser? Bei den Airport-Touren bist du mittendrin!",
                Images = new List<CardImage>()
                {
                 new CardImage() { Url = "https://www.munich-airport.de/_b/0000000000000001562517bb58c7c9b5/airporttour-bus1.jpg?t=eyJoZWlnaHQiOjU5NCwid2lkdGgiOjEwNDAsInF1YWxpdHkiOjc1fQ==--8bc9b0db14da84b92a0700825434d855749ba85f" }
                },
                Buttons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Title = "Touren",
                        Type = ActionTypes.OpenUrl,
                        Value = $"https://www.munich-airport.de/airport-touren-90392"
                    }
                }
            };
            HeroCard card4 = new HeroCard()
            {
                Title = "Events",
                Text = "Wintermarkt, Surf & Style, Public Viewing... Auf dem Campus gibt es das ganze Jahr tolle Attraktionen!",
                Images = new List<CardImage>()
                {
                 new CardImage() { Url = "https://www.munich-airport.de/_b/0000000000000000966385bb587de620/2013-08-10-surf-style-02-flo-hagena.jpg" }
                },
                Buttons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Title = "Events",
                        Type = ActionTypes.OpenUrl,
                        Value = $"https://www.munich-airport.de/events-90512"
                    }
                }
            };
            resultMessage.Attachments.Add(card1.ToAttachment());
            resultMessage.Attachments.Add(card2.ToAttachment());
            resultMessage.Attachments.Add(card3.ToAttachment());
            resultMessage.Attachments.Add(card4.ToAttachment());
            await context.PostAsync(resultMessage);
            context.Wait(MessageReceived);
        }


        [LuisIntent("Service")]
        public async Task Service(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Dir ist langweilig?");
            Thread.Sleep(2000);
            await context.PostAsync("Da weiß ich einiges!");
            Thread.Sleep(1000);

            var resultMessage = context.MakeMessage();
            resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            resultMessage.Attachments = new List<Attachment>();

            HeroCard card1 = new HeroCard()
            {
                Title = "Shopping",
                Text = "Egal ob Souvenirs, Fashion oder Duty Free Schnäppchen - unsere rund 170 Shops lassen keine Wünsche offen!",
                Images = new List<CardImage>()
                {
                 new CardImage() { Url = "https://www.munich-airport.de/_b/0000000000000000398383bb583d7ba6/shopping-assistant.jpg?t=eyJoZWlnaHQiOjU5NCwid2lkdGgiOjEwNDAsInF1YWxpdHkiOjc1fQ==--8bc9b0db14da84b92a0700825434d855749ba85f" }
                },
                Buttons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Title = "Shops",
                        Type = ActionTypes.OpenUrl,
                        Value = $"https://www.munich-airport.de/shops-90739"
                    }
                }
            };
            HeroCard card2 = new HeroCard()
            {
                Title = "Essen & Trinken",
                Text = "Ca. 50 Restaurants, Cafés und Bistros bieten eine große Auswahl an Speisen. Von bayerisch bis asiatisch ist für jeden was dabei.",
                Images = new List<CardImage>()
                {
                 new CardImage() { Url = "https://www.munich-airport.de/_b/0000000000000000276318bb582c2405/gastronomie_flughafen1.jpg?t=eyJoZWlnaHQiOjU5NCwid2lkdGgiOjEwNDAsInF1YWxpdHkiOjc1fQ==--8bc9b0db14da84b92a0700825434d855749ba85f" }
                },
                Buttons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Title = "Restaurants",
                        Type = ActionTypes.OpenUrl,
                        Value = $"https://www.munich-airport.de/restaurants-90769"
                    }
                }
            };
            HeroCard card3 = new HeroCard()
            {
                Title = "Events",
                Text = "Ein Ausflug zum Besucherpark oder doch lieber eine Tour durch den Flughafen? Hier sind alle Möglichkeiten im Überblick.",
                Images = new List<CardImage>()
                {
                 new CardImage() { Url = "https://www.munich-airport.de/_b/0000000000000000760907bb585d0e23/familie-auf-besucherhuegel.jpg?t=eyJoZWlnaHQiOjM0Mywid2lkdGgiOjYwMCwicXVhbGl0eSI6NzV9--386e466d2d6748ab82a85b7bb62f54d37c93007b" }
                },
                Buttons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Title = "Events",
                        Type = "imBack",
                        Value = "Events"
                    }
                }
            };
            resultMessage.Attachments.Add(card1.ToAttachment());
            resultMessage.Attachments.Add(card2.ToAttachment());
            resultMessage.Attachments.Add(card3.ToAttachment());
            await context.PostAsync(resultMessage);
            context.Wait(MessageReceived);
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
                Text = "Was willst du wissen?",
                Images = new List<CardImage>()
                {
                 new CardImage() { Url = "https://www.munich-airport.de/_b/0000000000000001220017bb58904389/flughafen-muenchen-konzern11.jpg?t=eyJoZWlnaHQiOjQ1Nywid2lkdGgiOjgwMCwicXVhbGl0eSI6NzV9--8e1738114267304cbf3c9b0e695f3619db058d76" }
                },
                Buttons = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Title = "Wichtigste Themen?",
                            Type = "imBack",
                            Value = "Was sind die derzeit wichtigsten strategischen Themen für den Flughafen?"
                        },
                        new CardAction()
                        {
                            Title = "Digitalisierung?",
                            Type = "imBack",
                            Value = "Was ist eigentlich Digitalisierung?"
                        },
                        new CardAction()
                        {
                            Title = "Digitalisierung für FMG?",
                            Type = "imBack",
                            Value = "Was bedeutet Digitalisierung für den Flughafen?"
                        },
                }
            };
            resultMessage.Attachments.Add(heroCardFMG.ToAttachment());
            await context.PostAsync(resultMessage);

            context.Wait(MessageReceived);
        }


        [LuisIntent("FMGThemen")]
        public async Task FMGThemen(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("(1) Die Erweiterung von Terminal 1 ");
            Thread.Sleep(2000);
            await context.PostAsync("(2) Die Entwicklung von Lab Campus");
            Thread.Sleep(2000);
            await context.PostAsync("(3) Die Digitalisierung");

            context.Wait(MessageReceived);
        }


        [LuisIntent("Digitalisierung")]
        public async Task Digitalisierung(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Der Begriff Digitalisierung bezeichnet die Überführung analoger Größen in diskrete (abgestufte) Werte," +
                " zu dem Zweck, sie elektronisch zu speichern oder zu verarbeiten. Das Endprodukt oder Ergebnis der Digitalisierung wird" +
                " mitunter als Digitalisat bezeichnet. Im weiteren Sinne wird mit dem Begriff auch der Wandel hin zu elektronisch gestützten" +
                " Prozessen mittels Informations- und Kommunikationstechnik bezeichnet.");

            context.Wait(MessageReceived);
        }


        [LuisIntent("FMGDigitalisierung")]
        public async Task FMGDigitalisierung(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("(1) B2C - Veränderung der Kommunikation zum Passagier");
            Thread.Sleep(2000);
            await context.PostAsync("(2) IoT - IT und Technik-Systeme wachsen zusammen");
            Thread.Sleep(2000);
            await context.PostAsync("(3) Data Fusion & Analytics bekommt einen viel größeren Stellenwert");
            Thread.Sleep(2000);
            await context.PostAsync("(4) Automatisierung wird erheblich zunehmen");

            context.Wait(MessageReceived);
        }


        [LuisIntent("Alter")]
        public async Task Alter(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Ich wurde Anfang 2017 entwickelt. Den Flughafen gibt es allerdings schon seit dem 17. Mai 1992!");
            context.Wait(MessageReceived);
        }


        [LuisIntent("Hobbys")]
        public async Task Hobbys(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Ich liebe es, zu reisen. Aber mein Lieblingsort bleibt immer noch der Flughafen München." +
                " Dort beobachte ich gerne die Flugzeuge, wenn mir langweilig ist.");
            context.Wait(MessageReceived);
        }


        [LuisIntent("Wohnort")]
        public async Task Wohnort(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Wie du dir sicher vorstellen kannst, befinde ich mich am Flughafen München.");
            context.Wait(MessageReceived);
        }


        [LuisIntent("Befinden")]
        public async Task Befinden(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Danke, mir geht es gut. Mit der 3. Startbahn würde ich mich jedoch noch wohler fühlen.");
            context.Wait(MessageReceived);
        }


        [LuisIntent("Freuen")]
        public async Task Freuen(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Ich freue mich sehr auf das Mitarbeiterfest am 17. Mai. Das ist mein 25. Geburtstag. In der" +
                " ganzen Woche werden dich Überraschungen erwarten zu diesem tollen Anlass. :)");
            context.Wait(MessageReceived);
        }


        [LuisIntent("Stolz")]
        public async Task Stolz(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Ich bin stolz darauf, der erste 5-Sterne-Flughafen Europas zu sein und das aktuell beste Terminal weltweit zu haben.");
            context.Wait(MessageReceived);
        }


        [LuisIntent("Fakten")]
        public async Task Fakten(IDialogContext context, LuisResult result)
        {
            string[] facts =
                {
                    "Das Terminal 2 hat am 29. Juni 2003 seine Türen für die Passagiere geöffnet.",
                    "Ca. 60 % des Flughafengeländes besteht aus Grünfläche.",
                    "Der Flughafen München wurde 2015 zum ersten 5-Sterne-Flughafen Europas gekürt.",
                    "2016 lag der Passagierrekord bei über 42 Millionen Fluggästen.",
                    "Am 26. April 2016 ging das Satellitengebäude in Betrieb",
                    "Auf dem gesamten Campus werden in etwa 35.000 Mitarbeiter beschäftigt.",
                    "Der Tower ist 78 Meter hoch.",
                    "Der Flughafen hat eine eigene Brauerei namens Airbräu.",
                    "Es gibt ca. 50 Restaurants, Bars und Cafés sowie 170 Shops.",
                    "Der Flughafen hat 6 Schwesterflüghäfen, u. a. 'Singapore Changi Airport' und 'Airports Company South Africa'.",
                };

            int i = rnd.Next(0, facts.Length);

            await context.PostAsync(facts[i]);
            context.Wait(MessageReceived);
        }


        [LuisIntent("Groesse")]
        public async Task Groesse(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Ich bin ca. 1.575 Hektar groß.");
            context.Wait(MessageReceived);
        }


        [LuisIntent("Arbeit")]
        public async Task Arbeit(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Oh ja, ich liebe sie! Ich habe mit so vielen Menschen zu tun, das ist großartig.");
            context.Wait(MessageReceived);
        }


        [LuisIntent("Zukunft")]
        public async Task Zukunft(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("In einem Liegestuhl vor einer frisch fertiggestellten 3. Startbahn." +
                " Ich möchte als Erster die Flugzeuge auf der neuen Bahn landen sehen. :)");
            context.Wait(MessageReceived);
        }


        [LuisIntent("Zeit")]
        public async Task Zeit(IDialogContext context, LuisResult result)
        {
            DateTime time;
            time = DateTime.Now;
            string timeStr = time.ToString("HH:mm");

            await context.PostAsync("Im Moment ist es " + timeStr + ".");
            context.Wait(MessageReceived);
        }


        [LuisIntent("Danke")]
        public async Task Thank(IDialogContext context, LuisResult result)
        {
            string[] thanks =
                {
                    "Nichts zu Danken. :)",
                    "Mach ich doch gern. :)",
                    "Gern geschehen. :)",
                    "Keine Ursache. :)",
                    "Dafür bin ich da. :)"
                };

            int i = rnd.Next(0, thanks.Length);

            await context.PostAsync(thanks[i]);
            context.Wait(MessageReceived);
        }


        [LuisIntent("Hilfe")]
        public async Task Hilfe(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hier habe ich Vorschläge für dich, welche Fragen du mir stellen kannst. Du kannst auch etwas anderes versuchen.");


            var resultMessage = context.MakeMessage();
            resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            resultMessage.Attachments = new List<Attachment>();

            HeroCard heroCardFlight = new HeroCard()
            {
                Title = "Flug",
                Images = new List<CardImage>()
                {
                 new CardImage() { Url = "https://www.munich-airport.de/_b/0000000000000000309917bb582f0564/besucherterrasse-3.jpg?t=eyJoZWlnaHQiOjU5NCwid2lkdGgiOjEwNDAsInF1YWxpdHkiOjc1fQ==--8bc9b0db14da84b92a0700825434d855749ba85f" }
                },
                Buttons = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Title = "Flugdetails",
                            Type = "imBack",
                            Value = "Details zu meinem Flug"
                        },
                        new CardAction()
                        {
                            Title = "Nächste Ablüge",
                            Type = "imBack",
                            Value = "Was sind die nächsten Abflüge?"
                        },
                        new CardAction()
                        {
                            Title = "Nächster Flug nach...",
                            Type = "imBack",
                            Value = "Nächster Flug nach..."
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
                            Title = "Service Empfehlungen",
                            Type = "imBack",
                            Value = "Service Empfehlungen"
                        },
                        new CardAction()
                        {
                            Title = "Servicesuche",
                            Type = "imBack",
                            Value = "Suche nach..."
                        },
                        new CardAction()
                        {
                            Title = "Events",
                            Type = "imBack",
                            Value = "Events"
                        },
                    }
            };

            HeroCard heroCardKarriere = new HeroCard()
            {
                Title = "Karriere am Flughafen",
                Images = new List<CardImage>()
                    {
                     new CardImage() { Url = "https://www.munich-airport.de/_b/0000000000000000293718bb582da737/it-mitarbeiter-computer.jpg" }
                    },
                Buttons = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Title = "Einsteiger/Erfahrene",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/berufseinsteiger-erfahrene-94612"
                        },
                        new CardAction()
                        {
                            Title = "Trainee",
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
                            Title = "Schüler",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/schuler-94701"
                        },
                        new CardAction()
                        {
                            Title = "Stellenangebote",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/stellenangebote-94732"
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
                            Title = "Parkplatz",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/parken-89995"
                        },
                        new CardAction()
                        {
                            Title = "Auto Route",
                            Type = "imBack",
                            Value = "Navigiere mich von..."
                        },
                        new CardAction()
                        {
                            Title = "Bahn/Bus",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.munich-airport.de/offentliche-verkehrsmittel-90215"
                        },
                }
            };
            resultMessage.Attachments.Add(heroCardFlight.ToAttachment());
            resultMessage.Attachments.Add(heroCardService.ToAttachment());
            resultMessage.Attachments.Add(heroCardNavigation.ToAttachment());
            resultMessage.Attachments.Add(heroCardKarriere.ToAttachment());
            await context.PostAsync(resultMessage);

            context.Wait(MessageReceived);
        }


        // Returns JSON string
        public static string GET(string url)
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