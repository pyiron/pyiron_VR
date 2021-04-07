using UnityEngine;
using UnityEngine.UI;

// Component of MainVACanvas/Panel/ErrorText
namespace UI.Log
{
    public class ErrorTextController : LogListener
    {
        // the reference to the attached Text Object
        [SerializeField] private Text textObject;
        private static float shrinkTimer;
        private static int activeTime = 12;

        private void Awake()
        {
            LogManager.RegisterListener(this);
            gameObject.SetActive(false);
            textObject.text = "";
            shrinkTimer = activeTime;
        }

        void Update()
        {
            if (textObject.text != "")
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
                        gameObject.SetActive(false);
                    }
                }
            }
        }

        public override void OnNewLogEntry(string msg, LogManager.ErrorSeverity severity)
        {
            if (severity == LogManager.ErrorSeverity.Status) return;

            gameObject.SetActive(true);
            textObject.text = msg;
            print("text is " + textObject.text);
            textObject.color = LogManager.colorCodes[severity];
        
            transform.localScale = Vector3.one;
            shrinkTimer = activeTime;
        }

        public void Close()
        {
            shrinkTimer = 0;
        }
    }
}
