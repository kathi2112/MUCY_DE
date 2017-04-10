using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using MUCY_DE.Dialogs;
using System;

namespace MUCY_DE
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        internal static IDialog<object> MakeRoot()
        {
            return Chain.From(() => new LUIS_Dialog_DE());
        }
        internal static IDialog<object> MakeRootEN()
        {
            return Chain.From(() => new LUIS_Dialog_EN());
        }
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                Console.WriteLine("UserData " + userData.Data);

                userData.SetProperty("UserData", DateTime.Today.ToString());

                await Conversation.SendAsync(activity, MakeRoot);

                await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
        
        
        /*public async Task StartAsync(IDialogContext context)
        {
            
            if(message.Text == "deutsch")
            {
                MakeRoot();
            }
            else
            {
                MakeRootEN();
            }
            //context.Wait(Microsoft.Bot.Builder.Dialogs.LuisDialog.MessageRecieved);
        }*/
    }
}