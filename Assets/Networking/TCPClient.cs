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
        //public static int blocksPerFrame = 5;

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
        public static int _msgLen;

        // the size of message-packets send from Python to Unity. Should be the same as in Python
        public static int BLOCKSIZE = 4096;
        
        // the amount of bytes that get used to send how long the following message is
        public static int MSG_LEN_BYTES = 4;

        // buffer incoming string data. Needed to deal with the TCP stream
        private static StringBuilder recBuffer = new StringBuilder();
        
        // buffer incoming byte data. Needed to deal with the TCP stream
        private static byte[] byteBuffer;
        // counts until which value the byteBuffer is filled
        private static int byteCount;
        // indicates whether the client currently should return byte or string data
        public static bool returnBytes = false;

        private static Queue<CallbackFunction> _callbacks = new Queue<CallbackFunction>();
        
        public static CallbackFunction currentCallback;

        #region Monobehaviour Callbacks

        private void Awake()
        {
            Inst = this;
        }

        public static bool IsLoading()
        {
            return TaskNumOut != TaskNumIn && StructureLoader.isFirstDatapart;
        }

        private void Update()
        {
            // after sending a message to Python, check if a response arrived
            if (!returnBytes && TaskNumIn > TaskNumOut)
            {
                if (currentCallback.callback == null)
                {
                    // a new message from python has arrived. Call the callback method
                    if (_callbacks.Count == 0)
                    {
                        Debug.LogWarning("Empty callback queue. Message gets discarded.");
                        return;
                    }
                       
                    currentCallback = _callbacks.Dequeue();
                }
                
                // check if new data has arrived, without blocking
                ReturnedMessage input = ListenForInput(shouldReturn: true, returnIncompleteMsgs:currentCallback.callIfIncomplete);
                if (input.msg != "")
                {
                    if (input.msgIsComplete || currentCallback.callIfIncomplete)
                    {
                        if (currentCallback.callback != null)
                        {
                            currentCallback.callback(input);
                            if (input.msgIsComplete && !currentCallback.returnByteData)
                            {
                                currentCallback.callback = null;
                            }
                        }
                    }
                }
            }
            else if (returnBytes)
            {
                ReturnedMessage input = ListenForByteInput(shouldReturn: true, returnIncompleteMsgs:currentCallback.callIfIncomplete, readAsync:true);
                
                if (input.msgIsComplete || (currentCallback.callIfIncomplete && input.structureData != null && input.structureData.Length != 0))
                {
                    print("Completed download");
                    currentCallback.callback(input);
                    if (input.msgIsComplete)
                    {
                        currentCallback.callback = null;
                        returnBytes = false;
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

        private static async Task<int> GetByteMsgAsyncAwait(NetworkStream stream, byte[] buffer, int offset, int remainingLen)
        {
            int length = Math.Min(remainingLen, BLOCKSIZE);
            
            // Read incoming stream into byte array. 
            return await stream.ReadAsync(buffer, offset, length);
        }
        
        private static int GetByteMsgAsync(NetworkStream stream, byte[] buffer, int offset, int remainingLen)
        {
            //int length = Math.Min(remainingLen, BLOCKSIZE);
            int length = remainingLen;
            // Read incoming stream into byte array. 
            if (_socketReadTask == null)
            {
                //_block = new Byte[length];
                _socketReadTask = stream.ReadAsync(buffer, offset, length);
                //_socketReadTask = GetByteMsgAsyncAwait(stream, buffer, offset, remainingLen);
            }

            if (!_socketReadTask.IsCompleted)
            {
                //print("Result is " + _socketReadTask.Result);
                return 0;
            }

            int len;
            try
            {
                len = _socketReadTask.Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
            _socketReadTask.Dispose();
            _socketReadTask = null;

            /*if (_block.Length != len)
            {
                print("Resizing " + _block.Length + " to " + len);
                Array.Resize(ref _block, len);
            }
            else
            {
                print("Keeping " + _block.Length);
            }*/

            return len;
        }
        
        /*private static byte[] GetByteMsgAsync(NetworkStream stream, int remainingLen)
        {
            int length = Math.Min(remainingLen, BLOCKSIZE);
            // Read incoming stream into byte array. 
            if (_socketReadTask == null)
            {
                _block = new Byte[length];
                _socketReadTask = stream.ReadAsync(_block, 0, length);
            }

            if (!_socketReadTask.IsCompleted)
            {
                return null;
            }

            int len;
            try
            {
                len = _socketReadTask.Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
            _socketReadTask.Dispose();
            _socketReadTask = null;

            if (_block.Length != len)
            {
                print("Resizing " + _block.Length + " to " + len);
                Array.Resize(ref _block, len);
            }
            else
            {
                print("Keeping " + _block.Length);
            }

            return _block;

            // Convert byte array to string message. 
            //if (len == 0) return "";
            //_block[len] = 0;
            //return Encoding.ASCII.GetString(_block, 0, len);
        }*/

        private static string GetMsgAsync(NetworkStream stream, int remainingLen)
        {
            int length = Math.Min(remainingLen, BLOCKSIZE);
            // Read incoming stream into byte array. 
            if (_socketReadTask == null)
            {
                _block = new Byte[length + 1];
                _socketReadTask = stream.ReadAsync(_block, 0, length);
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

        public static byte[] GetByteData(int len)
        {
            // get the input stream
            /*if (_socketReadTask == null)
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

                    return null;
                }
            }*/
            
            byte[] data = new byte[len];
            int bytesRead = 0;
            while (bytesRead < len)
            {
                byte[] newBytes = GetByteMsgSync(_stream, len - bytesRead);
                Array.Copy(newBytes, 0, data, bytesRead, newBytes.Length);
                bytesRead += newBytes.Length;
            }

            return data;
        }
        
        private static int GetByteMsgSync(NetworkStream stream, byte[] buffer, int offset, int remainingLen)
        {
            int length = Math.Min(remainingLen, BLOCKSIZE);
            //int length = remainingLen;
            //_block = new Byte[length];
            // Read incoming stream into byte array. 
            int len = stream.Read(buffer, offset, length);

            //byte[] newBlock = new Byte[len];
            //Array.Resize(ref _block, len);
            return len;
        }
        
        private static byte[] GetByteMsgSync(NetworkStream stream, int remainingLen)
        {
            int length = Math.Min(remainingLen, BLOCKSIZE);
            _block = new Byte[length];
            // Read incoming stream into byte array. 
            int len = stream.Read(_block, 0, length);

            //byte[] newBlock = new Byte[len];
            Array.Resize(ref _block, len);
            return _block;
        }

        private static string GetMsgSync(NetworkStream stream, int remainingLen)
        {
            int length = Math.Min(remainingLen, BLOCKSIZE);
            _block = new Byte[length + 1];
            // Read incoming stream into byte array. 
            int len = stream.Read(_block, 0, length);

            // Convert byte array to string message. 
            if (len == 0) return "";
            _block[len] = 0;
            return Encoding.ASCII.GetString(_block, 0, len);
        }

        private static ReturnedMessage
            ListenForByteInput(bool readAsync = true, bool shouldReturn = false,
                bool returnIncompleteMsgs = false) // doesnt freeze if shouldReturn=true
        {
            int count = Inst.blocksPerFrame;
            if (byteBuffer == null || returnIncompleteMsgs)
            {
                //byteBuffer = new byte[Math.Min(_msgLen, BLOCKSIZE)];
                byteBuffer = new byte[_msgLen];
            }

            // Receive data until at least the whole message has been received. Additional data will be bufferred
            // Warning: If the message is really long and the connection really slow this loop could lead to a temporary
            // screen freeze.
            while (true)
                //for (int i = 0; i < 100000; i++)	
            {
                // check if the complete message has been read
                if (byteCount >= _msgLen)
                {
                    // the whole message arrived
                    //ReturnedMsg = recBuffer.ToString(0, _msgLen);
                    if (byteCount != _msgLen)
                    {
                        print("Recbuffer len does not match msg len: " + byteBuffer.Length + " - " + _msgLen);
                        //RemoveString(ReturnedMsg.Length);
                    }

                    break;
                }
                
                // return from time to time
                if (readAsync)
                {
                    if (count <= 0)
                    {
                        //print("Bytecount " + byteCount);
                        ReturnedMessage retUncomplete = new ReturnedMessage("", false, byteBuffer, byteCount);

                        //ReturnedMsg = recBuffer.ToString();
                        if (returnIncompleteMsgs)
                        {
                            print("Returning incomplete msg");
                            //byteBuffer.Clear();
                            _msgLen -= ReturnedMsg.Length;
                        }

                        return retUncomplete;
                    }

                    count--;
                }

                // try to read input from the stream
                //byte[] newMsg;
                int lenNewBytes = 0;
                if (true)//readAsync)
                {
                    lenNewBytes = GetByteMsgAsync(_stream, byteBuffer, byteCount, _msgLen - byteCount);
                    //newMsg = GetByteMsgAsync(_stream, _msgLen - byteCount);
                }
                else
                {
                    lenNewBytes = GetByteMsgSync(_stream, byteBuffer, byteCount, _msgLen - byteCount);
                    //newMsg = GetByteMsgSync(_stream, _msgLen - byteCount);
                }

                byteCount += lenNewBytes;

                // check if the server disconnected
                /*if (lenNewBytes == 0)
                //if (newMsg == null || newMsg.Length == 0)
                {
                    //print("Getting no response from the server");
                    return new ReturnedMessage("");
                }*/

                //Array.Copy(newMsg, 0, byteBuffer, byteCount, newMsg.Length);
                //byteCount += newMsg.Length;
                //recBuffer.Append(newMsg);
            }

            _msgLen = 0;
            byteCount = 0;
            ReturnedMessage ret = new ReturnedMessage("", true, byteBuffer);
            byteBuffer = null;
            //recBuffer.Clear();

            return ret;
        }

        /// <summary>
        /// Listens on the connection to the Python server if input arrived. Will block if shouldReturn is set to false.
        /// Returns the received input.
        /// </summary>
        /// <param name="readAsync"></param>
        /// <param name="shouldReturn"></param>
        /// <returns></returns>
        private static ReturnedMessage
            ListenForInput(bool readAsync = true, bool shouldReturn = false, bool returnIncompleteMsgs=false) // doesnt freeze if shouldReturn=true
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
                if (byteBuffer == null || byteBuffer.Length != MSG_LEN_BYTES)
                {
                    byteBuffer = new byte[MSG_LEN_BYTES];
                    byteCount = 0;
                }
                
                // get the length of the message that will be send from the python program afterwards
                //int headerLen;
                while (true)
                {
                    byte[] receivedBytes;
                    int lenNewBytes = 0;
                    //string newMsg;
                    if (readAsync)
                    {
                        lenNewBytes = GetByteMsgAsync(_stream, byteBuffer, byteCount, MSG_LEN_BYTES - byteCount);
                        //receivedBytes = GetByteMsgAsync(_stream, MSG_LEN_BYTES - byteCount);
                        //newMsg = GetMsgAsync(_stream);
                    }
                    else
                    {
                        //newMsg = GetMsgSync(_stream, MSG_LEN_BYTES);
                        //receivedBytes = GetByteMsgSync(_stream, MSG_LEN_BYTES - byteCount);
                        lenNewBytes = GetByteMsgSync(_stream, byteBuffer, byteCount, MSG_LEN_BYTES - byteCount);
                    }

                    byteCount += lenNewBytes;

                    // check if the server disconnected
                    //if (receivedBytes == null || receivedBytes.Length == 0)
                    if (lenNewBytes == 0)
                    //if (newMsg == "")
                    {
                        //print("Currently getting no response from the server");
                        return new ReturnedMessage("");
                    }

                    //Array.Copy(receivedBytes, 0, byteBuffer, byteCount, receivedBytes.Length);
                    //byteCount += receivedBytes.Length;
                    //recBuffer.Append(newMsg);
                    // before the semicolon the length of the following message gets send, after it the message
                    //if (receivedBytes.Length == MSG_LEN_BYTES)
                    if (lenNewBytes == MSG_LEN_BYTES)
                    //if ((headerLen = recBuffer.ToString().IndexOf(';')) != -1)
                    {
                        break;
                    }

                    if (shouldReturn || !ProgramSettings.programIsRunning) return new ReturnedMessage("");
                    // todo: some performance tests should be done
                }

                //string header = recBuffer.ToString(0, headerLen);
                //RemoveString(headerLen + 1);
                //_msgLen = int.Parse(header);
                
                // bytes are always send in little Endian
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(byteBuffer);
                }

                _msgLen = BitConverter.ToInt32(byteBuffer, 0);
                
                byteBuffer = null;
                byteCount = 0;
                
                if (_msgLen == -1)
                {
                    _msgLen = 0;
                    return new ReturnedMessage("");
                }

                ReturnedMsg = "";
            }

            int count = Inst.blocksPerFrame;
            //int count = blocksPerFrame;

            // Receive data until at least the whole message has been received. Additional data will be bufferred
            // Warning: If the message is really long and the connection really slow this loop could lead to a temporary
            // screen freeze.
            while (true)
                //for (int i = 0; i < 100000; i++)	
            {
                // check if the complete message has been read
                if (recBuffer.Length >= _msgLen)
                {
                    // the whole message arrived
                    ReturnedMsg = recBuffer.ToString(0, _msgLen);
                    if (recBuffer.Length != ReturnedMsg.Length)
                    {
                        print("Recbuffer len does not match msg len: " + recBuffer.Length + " - " + ReturnedMsg.Length);
                        RemoveString(ReturnedMsg.Length);
                    }

                    break;
                }
                
                // return from time to time
                if (readAsync)
                {
                    if (count <= 0)
                    {
                        ReturnedMsg = recBuffer.ToString();
                        if (returnIncompleteMsgs)
                        {
                            recBuffer.Clear();
                            _msgLen -= ReturnedMsg.Length;
                        }

                        return new ReturnedMessage(ReturnedMsg);
                    }

                    count--;
                }

                // try to read input from the stream
                string newMsg;
                if (readAsync)
                {
                    newMsg = GetMsgAsync(_stream, _msgLen - recBuffer.Length);
                }
                else
                {
                    newMsg = GetMsgSync(_stream, _msgLen - recBuffer.Length);
                }

                // check if the server disconnected
                if (newMsg == "")
                {
                    print("Getting no response from the server");
                    return new ReturnedMessage("");
                }

                recBuffer.Append(newMsg);
            }

            _msgLen = 0;
            recBuffer.Clear();
            print("Finished TASK!");

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

            //print("Ret " + ReturnedMsg);
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
        public static string SendMsgToPython(bool hasReturnValue, string msg, bool sendAsync = true,
            Action<ReturnedMessage> callback=null, bool returnIncompleteMsgs=false)
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
                        _callbacks.Enqueue(new CallbackFunction(callback, returnIncompleteMsgs));
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
        public byte[] structureData;
        public int byteCount;
        
        public ReturnedMessage(string returnedMsg)
        {
            msg = returnedMsg;
            msgIsComplete = false;
            structureData = null;
            byteCount = 0;
        }

        public ReturnedMessage(string returnedMsg, bool isComplete)
        {
            msg = returnedMsg;
            msgIsComplete = isComplete;
            structureData = null;
            byteCount = 0;
        }
        
        public ReturnedMessage(string returnedMsg, bool isComplete, byte[] structureData)
        {
            msg = returnedMsg;
            msgIsComplete = isComplete;
            this.structureData = structureData;
            byteCount = structureData.Length;
        }
        
        public ReturnedMessage(string returnedMsg, bool isComplete, byte[] structureData, int byteCount)
        {
            msg = returnedMsg;
            msgIsComplete = isComplete;
            this.structureData = structureData;
            this.byteCount = byteCount;
        }
    }

    public struct CallbackFunction
    {
        public Action<ReturnedMessage> callback;
        public bool callIfIncomplete;
        public bool returnByteData;

        public CallbackFunction(Action<ReturnedMessage> callback, bool callIfIncomplete)
        {
            this.callback = callback;
            this.callIfIncomplete = callIfIncomplete;
            returnByteData = false;
        }
        
        public CallbackFunction(Action<ReturnedMessage> callback, bool callIfIncomplete, bool returnByteData)
        {
            this.callback = callback;
            this.callIfIncomplete = callIfIncomplete;
            this.returnByteData = returnByteData;
        }
    }
}