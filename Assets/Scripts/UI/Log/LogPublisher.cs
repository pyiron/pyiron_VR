using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI.Log
{
    public class LogPublisher : MonoBehaviour
    {
        //private static List<LogSubscriber> _listeners = new List<LogSubscriber>();
        private static Dictionary<LogSubscriber, ErrorSeverity[]> _listeners =
            new Dictionary<LogSubscriber, ErrorSeverity[]>();

        public static Dictionary<ErrorSeverity, Color> colorCodes = new Dictionary<ErrorSeverity, Color>()
        {
            {ErrorSeverity.Info, Color.white},
            {ErrorSeverity.Warning, Color.yellow},
            {ErrorSeverity.Error, new Color(250/255f, 15/255f, 15/255f)},
            {ErrorSeverity.Status, new Color(0/255f, 213/255f, 255/255f)},
        };

        public static readonly string LoadingMsg = "Loading";
        public static readonly string ReconnectMsg = "Disconnected!\nTrying to reconnect";
    
        /// <summary>
        /// Adds a subscriber to the list of subscribers.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="msgTypes">The message types to which the subscriber is subscribed.</param>
        public static void RegisterSubscriber(LogSubscriber subscriber, ErrorSeverity[] msgTypes)
        {
            _listeners.Add(subscriber, msgTypes);
        }

        /// <summary>
        /// Receives a message and forwards it to all listening subscribers.
        /// </summary>
        /// <param name="msg">The message that should be forwarded.</param>
        /// <param name="severity">The type of the message.</param>
        public static void ReceiveLogMsg(string msg, ErrorSeverity severity=ErrorSeverity.Error)
        {
            // notify all listeners that are listening to this type
            foreach (var listener in _listeners)
            {
                if (listener.Value.Contains(severity))
                {
                    listener.Key.OnNewLogEntry(msg, severity);
                }
            }
        }
    
        /// <summary>
        /// Determines the type of a message
        /// </summary>
        public enum ErrorSeverity
        {
            Info, Warning, Error, Status
        }
    }
}

