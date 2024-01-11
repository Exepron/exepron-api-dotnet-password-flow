using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PasswordAuthenticationFlowExample
{
    class Program
    {
        public static async Task Main()
        {
            Console.Title = "Exepron password grant";

            // here is where we will do the authentication and get the token.
            var response = await RequestTokenAsync();
            
            // lets call APIs
            await CallServiceAsync(response.AccessToken);
        }

        static async Task<TokenResponse> RequestTokenAsync()
        {
            var client = new HttpClient();

            // get the discovery URI from the configuration file
            var discoveryUri = ConfigurationManager.AppSettings["discoveryUri"];

            
            var disco = await client.GetDiscoveryDocumentAsync(discoveryUri);
            if (disco.IsError) throw new Exception(disco.Error);

            // create the authentication request.
            // this settings should be set in a configuration file:
            var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "<CLIENT_ID>",
                ClientSecret = "<CLIENT_SECRET>",
                Scope = "openid profile exepron.restapi",
                UserName = "<EXEPRON_USERNAME>",
                Password = "<EXEPRON_PASSWORD>"
            });

            if (response.IsError) throw new Exception(response.Error);
            return response;
        }

        /// <summary>
        /// Fetch User Active Tasks
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        static async Task CallServiceAsync(string token)
        {
            // get the baseAddress from the configuration file
            var baseAddress = ConfigurationManager.AppSettings["baseAddress"];

            var client = new HttpClient()
            {
                BaseAddress = new Uri(baseAddress)
            };

            // set the authentication token in the headers of the requests 
            client.SetBearerToken(token);


            // Get The User id and the account Id I am querying about: 
            // EXEPRON API Supports OData so we can just query the fields that we want to see using the $select keyword:
            var userResponse = await client.GetStringAsync("Accounts/Users/GetLoggedinUser?$select=userId,accountId");
            // OPTIONAL: Deserialize to the class you want to use in C#, you can also use the text received to parse the query.
            var exepronUsers = JsonSerializer.Deserialize<ExepronUser[]>(userResponse);
            var exepronUser = exepronUsers[0];            
            
            // Get some tasks from exepron.
            var activeTasksRequest = await client.GetStringAsync($"Accounts/{exepronUser.accountId}/Users/{exepronUser.userId}/GetUserActiveTasks?$select=taskId,taskNumber,taskName,remainingDuration");
            var activeTasks = JsonSerializer.Deserialize<List<ExepronTask>>(activeTasksRequest);

            // loop through the active tasks and write them to the console.
            foreach (var task in activeTasks)
            {
                Console.WriteLine($"task: {task.taskNumber}, taskId: {task.taskId}, taskName: '{task.taskName}', Remaining: {task.remainingDuration}");
            }
        }
    }
}
