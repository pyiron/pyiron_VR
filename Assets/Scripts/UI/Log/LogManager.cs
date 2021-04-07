using System.Collections.Generic;
using UnityEngine;

namespace UI.Log
{
    public class LogManager : MonoBehaviour
    {
        private static List<LogListener> _listeners = new List<LogListener>();

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
        /// Adds a subscriber to the list of subscribers
        /// </summary>
        /// <param name="listener"></param>
        public static void RegisterSubscriber(LogListener listener)
        {
            _listeners.Add(listener);
            print(listener.name + " registered.");
        }

        /// <summary>
        /// Receives a message and forwards it to all listening subscribers.
        /// </summary>
        /// <param name="msg">The message that should be forwarded.</param>
        /// <param name="severity">The type of the message.</param>
        public static void ReceiveLogMsg(string msg, ErrorSeverity severity=ErrorSeverity.Error)
        {
            // notify all listeners
            foreach (var listener in _listeners)
            {
                listener.OnNewLogEntry(msg, severity);
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

