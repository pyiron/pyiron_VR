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
            {ErrorSeverity.Error, new Color(250, 15, 15)},
        };
    
        public static void RegisterListener(LogListener listener)
        {
            _listeners.Add(listener);
        }

        public static void ReceiveLogMsg(string msg, ErrorSeverity severity=ErrorSeverity.Error)
        {
            // notify all listeners
            foreach (var listener in _listeners)
            {
                listener.OnNewLogEntry(msg, severity);
            }
        }
    
        public enum ErrorSeverity
        {
            Info, Warning, Error
        }
    }
}

