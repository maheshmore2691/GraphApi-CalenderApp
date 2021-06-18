
namespace Scheduler.WebClient.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Graph;
    using Newtonsoft.Json;
    using Scheduler.WebClient.Interfaces;
    using Scheduler.WebClient.Models;
    using Scheduler.WebClient.Utilities;

    public class GraphApiClient : IGraphApiClient
    {
        private const string GrantType = "client_credentials";
        private const string ClientDefaultScope = "https://graph.microsoft.com/.default";
        private const string BaseUrl = "https://graph.microsoft.com/v1.0";

        private readonly GraphTokenResponse _tokenResponse;
        private readonly GraphServiceClient _graphClient;
        private readonly IConfig _config;

        public GraphApiClient(IConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _tokenResponse = GetAccessTokenAsync()
                           .GetAwaiter().GetResult();

            _graphClient = new GraphServiceClient(BaseUrl, GetAuthProvider());
        }

        #region Public methods

        public async Task<ICalendarEventsCollectionPage> GetCalenderEventAsync(string emailAddress)
        {
            try
            {
                var events = await _graphClient.Users[emailAddress]
                    .Calendar
                    .Events
                    .Request()
                    .GetAsync();

                return events;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Event> CreateCalenderEventAsync(string emailAddress, DateTime appointmentDateTime, List<string> participants)
        {
            try
            {
                var attendees = new List<Attendee>();

                foreach (var participant in participants)
                {
                    attendees.Add(new Attendee
                    {
                        EmailAddress = new EmailAddress
                        { Address = participant, Name = participant }
                    });
                }
                var meetingEvent = new Event
                {
                    Attendees = attendees,
                    Body = new ItemBody 
                            { 
                                    Content = "This is sample test meeting using client credential work flow.",
                                    ContentType = BodyType.Text
                            },
                    Organizer = new Recipient { 
                                    EmailAddress = new EmailAddress 
                                                { Address = emailAddress, Name = "Test User" } 
                                },
                    Subject = "Demo meeting - Graph API",
                    Start = new DateTimeTimeZone { DateTime = appointmentDateTime.ToString(), TimeZone = TimeZoneInfo.Local.StandardName },
                    End = new DateTimeTimeZone { DateTime = appointmentDateTime.AddHours(1).ToString(), TimeZone = TimeZoneInfo.Local.StandardName }
                };

                var events = await _graphClient.Users[emailAddress]
                    .Calendar
                    .Events
                    .Request()
                    .AddAsync(meetingEvent);

                return events;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public async Task<bool> SendAppointmentConfirmationEmail(string emailAddress, DateTime appointmentDateTime)
        {
            try
            {
                var actionableMessage = new Message
                {
                    Subject = $"Schedule appointment with Carrier XYZ on {appointmentDateTime.ToShortDateString()} @ {appointmentDateTime.Hour}:{appointmentDateTime.Minute}",
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Html,
                        Content = AdaptiveCardReader.LoadConfirmApptActionableMessageBody()
                    },
                    ToRecipients = new List<Recipient>()
                    {
                        new Recipient
                        {
                            EmailAddress = new EmailAddress
                            {
                                Address = emailAddress
                            }
                        }
                    },
                };

                var saveToSentItems = true;
                var appointmentSchedulerUser = _graphClient.Users[_config.AzureConfigs.AppointmentSchedulerEmailId];
                var emailRequest = appointmentSchedulerUser.SendMail(actionableMessage, saveToSentItems).Request();
                await emailRequest.PostAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex?.Message}");
                return false;
            }
        }

        #endregion

        #region Private methods

        private async Task<GraphTokenResponse> GetAccessTokenAsync()
        {
            Uri tokenUri = new Uri($"https://login.microsoftonline.com/{_config.AzureConfigs.TenantId}/oauth2/v2.0/token");
            string content = string.Empty;
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.PostAsync(tokenUri, GetDataForAccessTokenRequest()).ConfigureAwait(true);
            if (response.IsSuccessStatusCode)
            {
                content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return await Task.Run(() => JsonConvert.DeserializeObject<GraphTokenResponse>(content)).ConfigureAwait(false);
            }

            GraphTokenResponse errorContent = JsonConvert.DeserializeObject<GraphTokenResponse>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            return null;
        }

        private FormUrlEncodedContent GetDataForAccessTokenRequest()
        {
            return new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _config.AzureConfigs.ClientId),
                new KeyValuePair<string, string>("scope", ClientDefaultScope),
                new KeyValuePair<string, string>("client_secret", _config.AzureConfigs.ClientSecret),
                new KeyValuePair<string, string>("grant_type", GrantType)
            });
        }

        private DelegateAuthenticationProvider GetAuthProvider()
        {
            return new DelegateAuthenticationProvider(async (requestMessage) =>
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenResponse.access_token);
            });
        }

        #endregion
    }
}
