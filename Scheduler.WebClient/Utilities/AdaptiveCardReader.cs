using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Scheduler.WebClient.Utilities
{
    public class AdaptiveCardReader
    {
        public static string LoadConfirmApptActionableMessageBody()
        {
            var cardJson = JObject.Parse(System.IO.File.ReadAllText(@".\Resources\AdaptiveCards\ConfirmApptAdaptiveCard.json"));

            // Check type
            // First, try "@type", which is the key MessageCard uses
            var cardType = cardJson.SelectToken("@type");
            if (cardType == null)
            {
                // Maybe it's Adaptive, try "type"
                cardType = cardJson.SelectToken("type");
            }

            if (cardType == null || (cardType.ToString() != "MessageCard" && cardType.ToString() != "AdaptiveCard"))
            {
                throw new ArgumentException("The payload in ConfirmApptAdaptiveCard.json is missing a valid @type or type property.");
            }

            string scriptType = cardType.ToString() == "MessageCard" ? "application/ld+json" : "application/adaptivecard+json";


            // Insert the JSON into the HTML
            return string.Format(System.IO.File.ReadAllText(@".\Resources\AdaptiveCards\ConfirmAppointment.html"), scriptType, cardJson.ToString());
        }
    }
}
