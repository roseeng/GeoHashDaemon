using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PushoverClient;

namespace GeoHashDaemon
{
    public class PushoverImpl
    {
        public static string apiToken = "";
        public static string userToken = "";

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
            // Needs https://github.com/roseeng/Pushover.NET

            Pushover pclient = new Pushover(apiToken, userToken);
            var response = await pclient.PushAsync(title, message, priority: priority, notificationSound: notificationSound);

            if (response.Status != 1)
            {
                if (response.Errors != null && response.Errors.Count > 0)
                    throw new ApplicationException("Pushover error: " + response.Errors[0]);
                else
                    throw new ApplicationException("Pushover error: " + response.Status);
            }

            //if (Verbose)
            //    _logger.LogWarning("Pushover notification sent to : " + userToken);            
        }
    }
}
