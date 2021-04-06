using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Log
{
    public class LogPanelManager : LogListener
    {
        private Queue<GameObject> _logEntries = new Queue<GameObject>();

        [SerializeField] private GameObject logHolder;
        [SerializeField] private GameObject logTemplate;
        [Tooltip("How many objects are allowed in this Log")]
        [SerializeField] private int maxLogNum = 6;
    
        private void Awake()
        {
            LogManager.RegisterListener(this);
        }

        public override void OnNewLogEntry(string msg, LogManager.ErrorSeverity severity)
        {
            GameObject newEntry;
            // if the log is full, delete the oldest log entry
            if (_logEntries.Count >= maxLogNum)
            {
                GameObject oldestEntry = _logEntries.Dequeue();
                // reuse the object for better performance
                newEntry = oldestEntry;
            }
            else
            {
                newEntry = Instantiate(logTemplate, logHolder.transform, true);
                newEntry.SetActive(true);
            }

            Text entryText = newEntry.GetComponentInChildren<Text>();
            entryText.text = msg;
            entryText.color = LogManager.colorCodes[severity];
        
            _logEntries.Enqueue(newEntry);
        }
    }
}
