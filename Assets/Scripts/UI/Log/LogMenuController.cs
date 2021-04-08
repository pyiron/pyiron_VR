using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Log
{
    public class LogMenuController : MenuController, LogSubscriber
    {
        private Queue<GameObject> _logEntries = new Queue<GameObject>();

        [SerializeField] private GameObject logHolder;
        [SerializeField] private GameObject logTemplate;
        [Tooltip("How many objects are allowed in this Log")]
        [SerializeField] private int maxLogNum = 6;
    
        private void Awake()
        {
            // Subscribe to error, warning and info messages
            LogPublisher.ErrorSeverity[] subscribedTypes = 
                {LogPublisher.ErrorSeverity.Info, LogPublisher.ErrorSeverity.Warning, LogPublisher.ErrorSeverity.Error};
            LogPublisher.RegisterSubscriber(this, subscribedTypes);
            
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Adds a new message to the log, if it is not a status message.
        /// </summary>
        /// <param name="msg">The message that should be added to the log.</param>
        /// <param name="severity">The type of the message.</param>
        public void OnNewLogEntry(string msg, LogPublisher.ErrorSeverity severity)
        {
            //if (severity == LogPublisher.ErrorSeverity.Status) return;
            
            GameObject newEntry;
            if (_logEntries.Count >= maxLogNum)
            {
                // if the log is full, dequeue the oldest log entry
                GameObject oldestEntry = _logEntries.Dequeue();
                // reuse the object for better performance
                newEntry = oldestEntry;
            }
            else
            {
                // create a new entry object
                newEntry = Instantiate(logTemplate, logHolder.transform, true);
                newEntry.SetActive(true);
            }

            // set the text to the new message
            Text entryText = newEntry.GetComponentInChildren<Text>();
            entryText.text = msg;
            entryText.color = LogPublisher.colorCodes[severity];
        
            _logEntries.Enqueue(newEntry);
        }
    }
}
