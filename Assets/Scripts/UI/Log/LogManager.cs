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
    
        public static void RegisterListener(LogListener listener)
        {
            _listeners.Add(listener);
            print(listener.name + " registered.");
        }

        public static void ReceiveLogMsg(string msg, ErrorSeverity severity=ErrorSeverity.Error)
        {
            print(msg + " with " + severity);
            // notify all listeners
            foreach (var listener in _listeners)
            {
                listener.OnNewLogEntry(msg, severity);
            }
        }
    
        public enum ErrorSeverity
        {
            Info, Warning, Error, Status
        }
    }
}

