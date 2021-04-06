using System;
using UI.Log;
using UnityEngine;

namespace Networking
{
    // component of Settings
    // this script handles everything related to Python, e.g. it receives the data from Python, formats it and can send data to Python
    public class PythonExecutor : MonoBehaviour {
        /// <summary>
        /// Send a command that will be executed in python and wait for the response. The command should be from
        /// PythonCmd so that it is possible to see which Python calls are used.\n
        /// Warning: this method might result in a screen freeze.
        /// </summary>
        /// <param name="hasReturnValue">Defines whether the command should be executed in Python using exec (=false) or eval</param>
        /// <param name="order">The command that should be executed.</param>
        /// <returns></returns>
        public static string SendOrderSync(bool hasReturnValue, string order)
        {
            return TCPClient.SendMsgToPythonSync(hasReturnValue, order);
        }

        /// <summary>
        /// Send a command that will be executed in python and wait for the response. The command should be from
        /// PythonCmd so that it is possible to see which Python calls are used.\n
        /// </summary>
        /// <param name="hasReturnValue">Defines whether the command should be executed in Python using exec (=false) or eval</param>
        /// <param name="order">The command that should be executed.</param>
        /// <param name="onReceiveCallback">The method that will be called when the response from Python arrives.</param>
        /// <returns></returns>
        public static void SendOrderAsync(bool hasReturnValue, string order, Action<string> onReceiveCallback)
        {
            // show that the program is loading
            LogManager.ReceiveLogMsg(LogManager.LoadingMsg, LogManager.ErrorSeverity.Status);
            //AnimatedText.Instances[TextInstances.LoadingText].Activate();
            // deactivate the scene while loading
            Utilities.DeactivateInteractables();

            // send the message using TCP
            TCPClient.SendMsgToPython(hasReturnValue, order, callback:onReceiveCallback);
        }
    }

    public enum PythonScript
    {
        None, unityManager, executor, structure
    }
}