using UnityEngine;

namespace UI.Log
{
    public abstract class LogSubscriber : MonoBehaviour
    {
        /// <summary>
        /// Receives a message from the publisher.
        /// </summary>
        /// <param name="msg">The message that got received.</param>
        /// <param name="severity">The type of the message.</param>
        public abstract void OnNewLogEntry(string msg, LogPublisher.ErrorSeverity severity);
    }
}
