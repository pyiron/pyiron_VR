using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Log
{
    public class StatusPanelManager : LogSubscriber
    {
        [SerializeField] private Text text;
        
        private float _timer;
        private readonly float _interval = 0.5f;

        private int state = 1;

        private static readonly string[] dotCombis = {"   ", ".  ", ".. ", "..."};

        private void Awake()
        {
            // Subscribe to status messages of the log publisher
            LogPublisher.ErrorSeverity[] subscribedTypes = 
                {LogPublisher.ErrorSeverity.Status};
            LogPublisher.RegisterSubscriber(this, subscribedTypes);
            
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Replaces the last chars of a string with newEnd.
        /// </summary>
        /// <param name="original">The original string of which the last chars should be replaced.</param>
        /// <param name="newEnd">The new replacement chars.</param>
        /// <returns></returns>
        private string ReplaceStringEnd(string original, string newEnd)
        {
            return original.Substring(0, original.Length - newEnd.Length) + newEnd;
        }

        /// <summary>
        /// Updates the loading animation (increasing/decreasing the amount of loading dots)
        /// </summary>
        void Update()
        {
            if (_timer > 0)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0)
                {
                    _timer = _interval;
                    if (state < 3)
                    {
                        // add a dot
                        state++;
                    }
                    else
                    {
                        // go back to zero dots
                        state = 0;
                    }
                    
                    text.text = ReplaceStringEnd(text.text, dotCombis[state]);
                }
            }
        }

        /// <summary>
        /// Display a new status message
        /// </summary>
        /// <param name="msg">The message that should be displayed. If it is empty, this display gets deactivated.</param>
        /// <param name="severity">The type of the message.</param>
        public override void OnNewLogEntry(string msg, LogPublisher.ErrorSeverity severity)
        {
            //if (severity == LogPublisher.ErrorSeverity.Status)
            //{
                if (msg == "")
                {
                    // deactivate this gameObject
                    gameObject.SetActive(false);
                    _timer = -1;
                }
                else
                {
                    // display the new text
                    gameObject.SetActive(true);
                    text.text = msg + dotCombis[0];
                    _timer = _interval;
                    state = 0;
                }
            /*}
            else
            {
                gameObject.SetActive(false);
            }*/
        }
    }
}
