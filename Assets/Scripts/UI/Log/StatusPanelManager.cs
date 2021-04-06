using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Log
{
    public class StatusPanelManager : LogListener
    {
        [SerializeField] private Text text;
        
        private float _timer;
        private readonly float _interval = 0.5f;

        private int state = 1;

        private static readonly string[] dotCombis = new[] {"   ", ".  ", ".. ", "..."};

        private void Awake()
        {
            LogManager.RegisterListener(this);
            gameObject.SetActive(false);
        }

        private string ReplaceStringEnd(string original, string newEnd)
        {
            return original.Substring(0, original.Length - newEnd.Length) + newEnd;
        }

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
                        state++;
                    }
                    else
                    {
                        state = 0;
                        //text.text = text.text.Substring(0, text.text.Length - 3);
                    }
                    
                    text.text = ReplaceStringEnd(text.text, dotCombis[state]);
                }
            }
        }

        public override void OnNewLogEntry(string msg, LogManager.ErrorSeverity severity)
        {
            if (severity == LogManager.ErrorSeverity.Status)
            {
                if (msg == "")
                {
                    gameObject.SetActive(false);
                    _timer = -1;
                }
                else
                {
                    gameObject.SetActive(true);
                    text.text = msg + dotCombis[0];
                    _timer = _interval;
                    state = 0;
                }
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
