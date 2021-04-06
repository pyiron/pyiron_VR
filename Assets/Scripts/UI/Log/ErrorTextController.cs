using UnityEngine;
using UnityEngine.UI;

// Component of MainVACanvas/Panel/ErrorText
namespace UI.Log
{
    public class ErrorTextController : LogListener
    {
        // the reference to the attached Text Object
        private static Text textObject;
        private static string errorMsg;
        private static float shrinkTimer;
        private static int activeTime = 12;

        private void Awake()
        {
            LogManager.RegisterListener(this);
        }

        void Start()
        {
            textObject = GetComponent<Text>();
            shrinkTimer = activeTime;
        }

        void Update()
        {
            if (errorMsg != "")
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
                        textObject.text = "";
                    }
                }
            }
        }

        public override void OnNewLogEntry(string msg, LogManager.ErrorSeverity severity)
        {
        
            textObject.text = msg;
            textObject.color = LogManager.colorCodes[severity];
        
            transform.localScale = Vector3.one;
            shrinkTimer = activeTime;
        }
    }
}
