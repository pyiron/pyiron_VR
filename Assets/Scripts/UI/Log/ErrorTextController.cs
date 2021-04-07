using UnityEngine;
using UnityEngine.UI;

namespace UI.Log
{
    // Component of MainVACanvas/Panel/MessageDisplay
    public class ErrorTextController : LogSubscriber
    {
        // the reference to the attached Text Object
        [SerializeField] private Text messageDisplay;
        private static float shrinkTimer;
        private static int activeTime = 12;

        private void Awake()
        {
            // Subscribe to error, warning and info messages
            LogPublisher.ErrorSeverity[] subscribedTypes = 
                {LogPublisher.ErrorSeverity.Info, LogPublisher.ErrorSeverity.Warning, LogPublisher.ErrorSeverity.Error};
            LogPublisher.RegisterSubscriber(this, subscribedTypes);
            
            // initialize this object
            gameObject.SetActive(false);
            messageDisplay.text = "";
            shrinkTimer = activeTime;
        }

        // Update the timer, and if it is up shrink this gameObject
        void Update()
        {
            if (messageDisplay.text != "")
            {
                if (shrinkTimer > 0)
                {
                    shrinkTimer -= Time.deltaTime;
                }
                else
                {
                    transform.localScale -= Vector3.one * Time.deltaTime;
                    if (transform.localScale.x <= 0)
                    {
                        messageDisplay.text = "";
                        gameObject.SetActive(false);
                    }
                }
            }
        }

        // Update the text when receiving a new message
        public override void OnNewLogEntry(string msg, LogPublisher.ErrorSeverity severity)
        {
            // ignore status messages
            //if (severity == LogPublisher.ErrorSeverity.Status) return;

            gameObject.SetActive(true);
            messageDisplay.text = msg;
            messageDisplay.color = LogPublisher.colorCodes[severity];
        
            transform.localScale = Vector3.one;
            shrinkTimer = activeTime;
        }

        /// <summary>
        /// Closes the panel with a shrinking animation.
        /// </summary>
        public void Close()
        {
            shrinkTimer = 0;
        }
    }
}
