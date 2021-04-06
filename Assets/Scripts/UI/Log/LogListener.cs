using UnityEngine;

namespace UI.Log
{
    public abstract class LogListener : MonoBehaviour
    {
        public abstract void OnNewLogEntry(string msg, LogManager.ErrorSeverity severity);
    }
}
