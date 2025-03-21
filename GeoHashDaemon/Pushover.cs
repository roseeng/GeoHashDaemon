using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PushoverClient;

namespace GeoHashDaemon
{
    public class PushoverImpl
    {
        public static void SendAlert(string title, string message)
        {

            SendAlertAsync(title, message).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// A bit less intrusive than the Alert
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public static void SendNotification(string title, string message)
        {

            SendAlertAsync(title, message, priority: Priority.Low, notificationSound: NotificationSound.Pushover).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static async Task SendAlertAsync(string title, string message, Priority priority = Priority.Normal, NotificationSound notificationSound = NotificationSound.Magic)
        {
            // Needs https://www.nuget.org/packages/PushoverNET/1.0.25/

            var apiToken = "a2xdge8mhgc8jtrc2rdmmjn39zwi9o";
            var userToken = "gku93zjrfyvwz6844oj3eb42sqhqf2";

            Pushover pclient = new Pushover(apiToken, userToken);
            var response = await pclient.PushAsync(title, message, priority: priority, notificationSound: notificationSound);

            if (response.Status != 1)
                throw new ApplicationException("Pushover error: " + response.Errors?.Count);

            //if (Verbose)
            //    _logger.LogWarning("Pushover notification sent to : " + userToken);            
        }
    }
}
