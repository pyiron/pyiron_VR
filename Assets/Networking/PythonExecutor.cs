using System;
using UnityEngine;

namespace Networking
{
    // component of Settings
    // this script handles everything related to Python, e.g. it receives the data from Python, formats it and can send data to Python
    public class PythonExecutor : MonoBehaviour {
        /// <summary>
        /// Send an order to Python.
        /// script is the script that should execute the order.
        /// type defines how the order should be handled. E.g. does exec mean, that it will be executed with the exec() statement.
        /// order contains the data that should be set or the order that should be executed.
        /// </summary>

        private static string ProcessOrder(PythonScript script, string order)
        {
            string fullOrder = order;

            if (script != PythonScript.None)
            {
                fullOrder = script + "." + order;
            }
            
            print(fullOrder);
            
            return fullOrder;
        }
    
        /// <summary>
        /// Send a command that will be executed in python and wait for the response. The command should be from
        /// PythonCmd so that it is possible to see which Python calls are used.\n
        /// Warning: this method might result in a screen freeze.
        /// </summary>
        /// <param name="script">The python script that should execute the command.</param>
        /// <param name="hasReturnValue">Defines whether the command should be executed in Python using exec (=false) or eval</param>
        /// <param name="order">The command that should be executed.</param>
        /// <returns></returns>
        public static string SendOrderSync(PythonScript script, bool hasReturnValue, string order)//, bool handleInput=true)
        {
            string fullOrder = ProcessOrder(script, order);

            return TCPClient.SendMsgToPythonSync(hasReturnValue, fullOrder);
        }

        /// <summary>
        /// Send a command that will be executed in python and wait for the response. The command should be from
        /// PythonCmd so that it is possible to see which Python calls are used.\n
        /// </summary>
        /// <param name="script">The python script that should execute the command.</param>
        /// <param name="hasReturnValue">Defines whether the command should be executed in Python using exec (=false) or eval</param>
        /// <param name="order">The command that should be executed.</param>
        /// <param name="onReceiveCallback">The method that will be called when the response from Python arrives.</param>
        /// <returns></returns>
        public static void SendOrderAsync(PythonScript script, bool hasReturnValue, string order, 
            Action<string> onReceiveCallback)
        {
            // show that the program is loading
            AnimatedText.Instances[TextInstances.LoadingText].Activate();
            // deactivate the scene while loading
            Utilities.DeactivateInteractables();
        
            string fullOrder = ProcessOrder(script, order);

            // send the message using TCP
            TCPClient.SendMsgToPython(hasReturnValue, fullOrder, callback:onReceiveCallback);
        }
    }

    public enum PythonScript
    {
        None, unityManager, executor, structure
    }
}