using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Description;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Threading;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using TranslationBot.Dialogs.Model;
using Microsoft.Bot.Builder;


namespace TranslationBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private const string Host = "https://api.cognitive.microsofttranslator.com";
        private const string Path = "/translate?api-version=3.0";
        private const string UriParams = "&to=en&category=generalnn";

        private static HttpClient _client = new HttpClient();

        private const string key = "YOUR_API_KEY";
        //private string tolangcode = "en";
        //private bool check = false;
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                var input = activity.Text;
         
                    Task.Run(async () =>
                    {
                        var output = await TranslateText(input);
                        Console.WriteLine(output);

                        Activity reply = activity.CreateReply($"Your text is translated into English => '{output}'");
                        await connector.Conversations.ReplyToActivityAsync(reply);

                    }).Wait();
                
                
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        static async Task<string> TranslateText(string text, CancellationToken cancellationToken = default(CancellationToken))
        {
            var body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var request = new HttpRequestMessage())
            {
                var uri = Host + Path+UriParams;
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);

                var response = await _client.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<TranslatorResponse[]>(responseBody);

                return result?.FirstOrDefault()?.Translations?.FirstOrDefault()?.Text;
            }


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
    }
}
