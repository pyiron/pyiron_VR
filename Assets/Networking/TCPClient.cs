using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UI.Log;
using UnityEngine;

namespace Networking
{
    public class TCPClient : MonoBehaviour
    {
        private static TCPClient Inst;
    
        /// <summary>
        /// The current connection. Needed to send and receive messages. Established by the TCPClientConnector.
        /// </summary>
        public static TcpClient SocketConnection;
        private static NetworkStream _stream;
        //private Thread clientReceiveThread;

        [Tooltip("How many blocks should be read in one frame when reading asynchronous? " +
                 "Higher blocks_per_frame means shorter loading times, but less fps while loading")]
        [SerializeField] private int blocksPerFrame = 2;

        /// <summary>
        /// Shows how many messages have been received. If TaskNumOut > TaskNumIn, then the client is still
        /// waiting for a response from the server. It functions as a unique id for each message received from Python.
        /// </summary>
        public static int TaskNumIn;
        /// <summary>
        /// Shows how many messages have been send. If TaskNumOut > TaskNumIn, then the client is still
        /// waiting for a response from the server. It functions as a unique id for each order send to Python.
        /// </summary>
        public static int TaskNumOut;

        /// <summary>
        /// The last message received by the server.
        /// </summary>
        public static string ReturnedMsg;

        //public static StringBuilder ReceivedMsg;

        // a buffer for the received data
        private static Byte[] _block;

        // the task waiting for new data
        private static Task<int> _socketReadTask;

        // the length of the msg that should be read next (0 if it is unknown)
        private static int _msgLen;

        // the size of message-packets send from Python to Unity. Should be the same as in Python
        private static int BLOCKSIZE = 4096;

        // buffer all incoming data. Needed to deal with the TCP stream
        private static StringBuilder recBuffer = new StringBuilder();

        private static Queue<Action<string>> _callbacks = new Queue<Action<string>>();

        #region Monobehaviour Callbacks

        private void Awake()
        {
            Inst = this;
        }

        private void Update()
        {
            // after sending a message to Python, check if a response arrived
            if (TaskNumIn > TaskNumOut)
            {
                // check if new data has arrived, without blocking
                ReturnedMessage input = ListenForInput(shouldReturn: true);
                if (input.msg != "")
                {
                    // a new message from python has arrived. Call the callback method
                    if (_callbacks.Count == 0)
                    {
                        Debug.LogWarning("Empty callback queue. Message gets discarded.");
                    }
                    else
                    {
                        Action<string> callback = _callbacks.Dequeue();
                        if (callback != null)
                        {
                            callback(input.msg);
                        }
                    }
                }
            }
        }

        private void OnApplicationQuit()
        {
            CloseServer();
            ProgramSettings.programIsRunning = false;
        }

        #endregion

        #region ReceiveMessages
        
        // delete the beginning of a string
        private static void RemoveString(int len)
        {
            if (recBuffer.Length > len)
            {
                string cutOff = recBuffer.ToString(len, recBuffer.Length - len);
                recBuffer.Clear();
                recBuffer.Append(cutOff);
            }
            else
            {
                recBuffer.Clear();
            }
        }

        private static string GetMsgAsync(NetworkStream stream)
        {
            // Read incoming stream into byte array. 
            if (_socketReadTask == null)
            {
                _block = new Byte[BLOCKSIZE + 1];
                _socketReadTask = stream.ReadAsync(_block, 0, BLOCKSIZE);
            }

            if (!_socketReadTask.IsCompleted)
            {
                return "";
            }

            int len;
            try
            {
                len = _socketReadTask.Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "";
            }
            _socketReadTask.Dispose();
            _socketReadTask = null;

            // Convert byte array to string message. 
            if (len == 0) return "";
            _block[len] = 0;
            return Encoding.ASCII.GetString(_block, 0, len);
        }

        private static string GetMsgSync(NetworkStream stream)
        {
            _block = new Byte[BLOCKSIZE + 1];
            // Read incoming stream into byte array. 
            int len = stream.Read(_block, 0, BLOCKSIZE);

            // Convert byte array to string message. 
            if (len == 0) return "";
            _block[len] = 0;
            return Encoding.ASCII.GetString(_block, 0, len);
        }

        /// <summary>
        /// Listens on the connection to the Python server if input arrived. Will block if shouldReturn is set to false.
        /// Returns the received input.
        /// </summary>
        /// <param name="readAsync"></param>
        /// <param name="shouldReturn"></param>
        /// <returns></returns>
        private static ReturnedMessage
            ListenForInput(bool readAsync = true, bool shouldReturn = false) // doesnt freeze if shouldReturn=true
        {
            // Message protocol: len_of_message;messagelen_of_next_message;next_message
            // Example: 13;first message4;done
            // One message will be read per loop

            // get the input stream
            if (_socketReadTask == null)
            {
                try
                {
                    _stream = SocketConnection.GetStream();
                }
                catch (InvalidOperationException e)
                {
                    Debug.LogError(e.Message + "\n" + e.StackTrace);
                    LogPublisher.ReceiveLogMsg("Couldn't read the TCP stream. Please check that you are still connected to" +
                                             " the internet!");

                    return new ReturnedMessage("");
                }
            }

//		stream.ReadTimeout = 1000;
            if (_msgLen == 0)
            {
                // get the length of the message that will be send from the python program afterwards
                int headerLen;
                while (true)
                {
                    string newMsg;
                    if (readAsync)
                    {
                        newMsg = GetMsgAsync(_stream);
                    }
                    else
                    {
                        newMsg = GetMsgSync(_stream);
                    }

                    // check if the server disconnected
                    if (newMsg == "")
                    {
                        print("Currently getting no response from the server");
                        return new ReturnedMessage("");
                    }

                    recBuffer.Append(newMsg);
                    // before the semicolon the length of the following message gets send, after it the message
                    if ((headerLen = recBuffer.ToString().IndexOf(';')) != -1)
                    {
                        break;
                    }

                    if (shouldReturn || !ProgramSettings.programIsRunning) return new ReturnedMessage("");
                    // todo: some performance tests should be done
                }

                string header = recBuffer.ToString(0, headerLen);
                //PythonExecuter.incomingChanges += 1;
                RemoveString(headerLen + 1);
                _msgLen = int.Parse(header);
                if (_msgLen == -1)
                {
                    _msgLen = 0;
                    return new ReturnedMessage("");
                }

                ReturnedMsg = "";
            }

            int count = Inst.blocksPerFrame;

            // Receive data until at least the whole message has been received. Additional data will be bufferred
            // Warning: If the message is really long and the connection really slow this loop could lead to a temporary
            // screen freeze.
            while (true)
                //for (int i = 0; i < 100000; i++)	
            {
                if (readAsync)
                {
                    if (count <= 0)
                    {
                        return new ReturnedMessage("");
                    }

                    count--;
                }

                // check if the complete message has been read
                if (recBuffer.Length >= _msgLen)
                {
                    // the whole message arrived
                    ReturnedMsg = recBuffer.ToString(0, _msgLen);
                    RemoveString(ReturnedMsg.Length);
                    break;
                }

                // try to read input from the stream
                string newMsg;
                /*if (readAsync)
            {
                newMsg = GetMsgAsync(stream);
            }
            else
            {*/
                newMsg = GetMsgSync(_stream);
                //}

                // check if the server disconnected
                if (newMsg == "")
                {
                    print("Getting no response from the server");
                    return new ReturnedMessage("");
                }

                recBuffer.Append(newMsg);
            }

            _msgLen = 0;

            TaskNumOut += 1;
            if (TaskNumOut == TaskNumIn)
            {
                // all requested messages got loaded, so the loading text can be deactivated
                LogPublisher.ReceiveLogMsg("", LogPublisher.ErrorSeverity.Status);
                //AnimatedText.Instances[TextInstances.LoadingText].Deactivate();
                if (readAsync)
                {
                    Utilities.ActivateInteractables();
                }
            }

            print("Ret " + ReturnedMsg);
            return new ReturnedMessage(ReturnedMsg, true);
        }
        #endregion

        #region SendMessages

        /// <summary>
        /// Send the message to the python script and execute it there.
        /// </summary>
        /// <param name="hasReturnValue">Does executing the message return a value?</param>
        /// <param name="msg">The message that should be send.</param>
        /// <returns></returns>
        public static string SendMsgToPythonSync(bool hasReturnValue, string msg)
        {
            string msgRes = SendMsgToPython(hasReturnValue, msg, sendAsync: false);
            if (msgRes.StartsWith("Error"))
            {
                return msgRes;
            }

            ReturnedMessage returnedMsg = ListenForInput(readAsync: false);
            if (!returnedMsg.msgIsComplete)
            {
                return "";
            }
            return returnedMsg.msg;
        }

        /// <summary> 	
        /// Send a message to server using socket connection. 	
        /// </summary> 	
        public static string SendMsgToPython(bool hasReturnValue, string msg, bool sendAsync = true, Action<string> callback=null)
        {
            print("Sending: " + msg);
            
            if (SocketConnection == null || !SocketConnection.Connected)
            {
                string problem = "Socket currently not connected!";
                Debug.LogWarning(problem + " socketConnection is " + SocketConnection);
                LogPublisher.ReceiveLogMsg(problem);
                TCPClientConnector.TryReconnect();
                return "Error: " + problem;
            }

            try
            {
                NetworkStream stream = SocketConnection.GetStream();
                stream.WriteTimeout = 1000;
                if (stream.CanWrite)
                {
                    string execOrEval = hasReturnValue ? "eval" : "exec";
                    string clientMessage = execOrEval + ":" + msg;
                    clientMessage = clientMessage.Length + ";" + clientMessage;
                    // Convert string message to byte array.                 
                    byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
                    // Write byte array to socketConnection stream.    
                    if (sendAsync)
                    {
                        _callbacks.Enqueue(callback);
                        stream.WriteAsync(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                    }
                    else
                    {
                        stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                    }

                    TaskNumIn++;
                }
                else
                {
                    Debug.LogError("stream not writable");
                    return "Error: Stream not writable";
                }
            }
            // can throw a SocketException, InvalidOperationException or System.IO.IOException,
            // main cause is a failure of internet or that the server stopped
//		catch (SocketException ex)
//		{
//			Debug.LogError("InvalidOperationException: " + ex);
//			ErrorTextController.inst.ShowMsg("Socket exception: " + ex);
//			return "Socket Exc: " + ex;
//		}
//		catch (InvalidOperationException ex)
//		{
//			ErrorTextController.inst.ShowMsg("InvalidOperationException: " + ex);
//			Debug.LogError("InvalidOperationException: " + ex + "\n" +
//			               "Check that the server is still running and you have internet connection");
//			return "Socket Exc: " + ex;
//		}
//		catch (System.IO.IOException ex)
//		{
//			ErrorTextController.inst.ShowMsg("IOException: " + ex);
//			Debug.LogError("IOException: " + ex + "\n" +
//			               "Check that the server is still running and you have internet connection");
//			return "IO Exc: " + ex;
//		}
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
                LogPublisher.ReceiveLogMsg("The Action could not be Executed. Please try again");
                TCPClientConnector.TryReconnect();
                return "Error: " + ex;
            }

            return "async";
            // return receive(); 
        }

        #endregion

        public static void CloseServer()
        {
            if (SocketConnection != null)
            {
                string rec = SendMsgToPython(false, "end server");
                print("Closed the server: " + rec);
            }
        }
    }

    public struct ReturnedMessage
    {
        public string msg;
        public bool msgIsComplete;
        
        public ReturnedMessage(string returnedMsg)
        {
            msg = returnedMsg;
            msgIsComplete = false;
        }

        public ReturnedMessage(string returnedMsg, bool isComplete)
        {
            msg = returnedMsg;
            msgIsComplete = isComplete;
        }
    }
}