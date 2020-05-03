using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HTC.UnityPlugin.Vive;
using UnityEngine;
using UnityEngine.UI;

public class TCPClient : MonoBehaviour
{
	public static TCPClient Inst;
	
	private static TcpClient socketConnection;
	private static NetworkStream stream;
	private Thread clientReceiveThread;
	
	// needed for asynchronous (lag free) connecting to the server
	public static Task connStatus;
	// the time until the connection is declared as not existent
	private static float connTimeOut = 2;
	private float connTimer = connTimeOut;

	// will be increased by 1 each time an order gets send. It functions as a unique id for each order send to Python
	public static int taskNumIn = 0;
	public static int taskNumOut = 0;
	
	// the List of functions and scripts the Update function should return received messages to
	public static string returnedMsg;
	// a buffer for the received data
	private static Byte[] block;
	// the task waiting for new data
	private static Task<int> socketReadTask;
	// the length of the msg that should be read next (0 if it is unknown)
	private static int msgLen;

	private string HOST;
	public const int PORT = 65432;
	
	// the size of message-packets send from Python to Unity. Should be the same as in Python
	private static int BLOCKSIZE = 1024;
	// buffer all incoming data. Needed to deal with the TCP stream
	private static String recBuffer = "";

	#region Monobehaviour Callbacks

	private void Awake()
	{
		Inst = this;
	}

	private void Update()
	{
		// after sending a message to Python, check if a response arrived
		if (taskNumIn > taskNumOut)
		{
			// check if new data has arrived, without blocking
			ListenForInput(shouldReturn:true);
		}
	}

	private void OnApplicationQuit()
	{
		CloseServer();
		ProgramSettings.programIsRunning = false;
	}

	#endregion

	public static bool IsReady()
	{
		return socketConnection != null;
	}

	#region ConnectToServer

	// called by the Input Field for the server address on the network panel
	public void ConnectTo(InputField inputField)
	{
		// save the Ip adress
		PlayerPrefs.SetString("ServerIp", inputField.text);
		ConnectWithHost(inputField.text);
	}
	
	// called by the Input Field for the server address on the network panel
	public void ConnectTo(Text btnText)
	{
		// The vibration does not get triggered on the Quest, but it should work on the Vive
		// If needed on the Quest, the function of the OVR Plugin could be used
		ViveInput.TriggerHapticVibration(HandRole.LeftHand, 0.2f);
		ViveInput.TriggerHapticVibration(HandRole.RightHand, 0.2f);
		
		ConnectWithHost(btnText.text);
	}

	public void ConnectWithHost(string host)
	{
		// show that the program is loading
		LoadingText.Inst.Activate();

		// if no new host is specified, try to load the one we have been connected to before 
		// (needed for temporary internet outages)
		if (host == null)
		{
			host = HOST;
		}

		// save the host
		HOST = host;
		
		print("Trying to connect to " + HOST);
		
		// after connecting to a server or while trying to connect to one, connecting to another one is not possible
		if (socketConnection != null || connStatus != null) return;
		
		try
		{
			socketConnection = new TcpClient();
			connStatus = socketConnection.ConnectAsync(host, PORT);
			StartCoroutine(TryToConnect());
		}
		catch
		{
			ConnectionError();
			
			// the loading is over, deactivate the loading text
			LoadingText.Inst.Deactivate();
		}
	}

	private IEnumerator TryToConnect()
	{
		// the time between each update if there has been an update to the connection status
		float updateTime = 0.4f;
		
		while (connTimer > 0)
		{
			connTimer -= updateTime;
			if (connStatus.IsFaulted || connStatus.IsCanceled)
			{
				// error
				connTimer = connTimeOut;
				print("Connection is Faulted: " + connStatus.IsFaulted);
				print("Connection is Canceled: " + connStatus.IsCanceled);
				ConnectionError();
				break;
			}
			if (connStatus.IsCompleted)
			{
				// success
				connTimer = connTimeOut;
				print("Successfully connected to the server");
				ConnectionSuccess();
				break;
			}
			yield return new WaitForSeconds(updateTime);
		}
		if (connTimer <= 0)
		{
			// the server didn't respond in time
			connTimer = connTimeOut;
			ConnectionTimeout();
		}
		
		// the loading is over, deactivate the loading text
		LoadingText.Inst.Deactivate();
	}

	private void ConnectionSuccess()
	{
		connStatus.Dispose();
		connStatus = null;
		
		NetworkMenuController.Inst.keyboard.SetActive(false);
		
		// load the content of the start path (which is defined in the Python script)
		// ExplorerMenuController.Inst.LoadPathContent();
		
		ModeController.inst.SetMode(Modes.Structure);

		Boundingbox.Inst.gameObject.SetActive(true);
	}

	private void ConnectionError()
	{
		// connection failure
		print("Failed to connect");
		connStatus.Dispose();
		connStatus = null;
		socketConnection = null;
		ErrorTextController.inst.ShowMsg("Couldn't connect to the server.");
	}

	private void ConnectionTimeout()
	{
		// connection failure caused by timeout
		print("Failed to connect due to timeout");
		connStatus = null;
		socketConnection.Close();
		socketConnection = null;
		ErrorTextController.inst.ShowMsg("Couldn't connect to the server");
	}

	#endregion

	// delete the beginning of a string
	private static void RemoveString(int len)
	{
		if (recBuffer.Length > len)
		{
			recBuffer = recBuffer.Substring(len);
		}
		else
		{
			recBuffer = "";
		}
	}

	private static string GetMsgAsync(NetworkStream stream)
	{			
		
		// Read incoming stream into byte array. 
		if (socketReadTask == null)
		{
			block = new Byte[BLOCKSIZE + 1];
			socketReadTask = stream.ReadAsync(block, 0, BLOCKSIZE);
		}

		if (!socketReadTask.IsCompleted)
		{
			return "";
		}
		int len = socketReadTask.Result;
		socketReadTask.Dispose();
		socketReadTask = null;
		
		// Convert byte array to string message. 
		if (len == 0) return "";
		block[len] = 0;
		return Encoding.ASCII.GetString(block, 0, len);
	}
	
	private static string GetMsgSync(NetworkStream stream)
	{			
		block = new Byte[BLOCKSIZE + 1];
		// Read incoming stream into byte array. 
		int len = stream.Read(block, 0, BLOCKSIZE);
		
		// Convert byte array to string message. 
		if (len == 0) return "";
		block[len] = 0;
		return Encoding.ASCII.GetString(block, 0, len);
	}

	/// <summary>
	/// Listens on the connection to the Python server if input arrived. Will block if shouldReturn is set to false.
	/// Returns the received input.
	/// </summary>
	/// <param name="readAsync"></param>
	/// <param name="shouldReturn"></param>
	/// <returns></returns>
	private static string ListenForInput(bool readAsync=true, bool shouldReturn=false) // doesnt freeze if shouldReturn=true
	{
		// Message protocol: len_of_message;messagelen_of_next_message;next_message
		// Example: 13;first message4;done
		// One message will be read per loop
		
		// get the input stream
		if (socketReadTask == null)
		{
			try
			{
				stream = socketConnection.GetStream();
			} catch (InvalidOperationException e)
			{
				Debug.LogError(e.Message + "\n" + e.StackTrace);
				ErrorTextController.inst.ShowMsg("Couldn't read the TCP stream. Check that you are still connected to" +
				                                 " the internet!");
				
				return "";
			}
		}

//		stream.ReadTimeout = 1000;
		int headerLen;
		if (msgLen == 0)
		{
			// get the length of the message that will be send from the python program afterwards
			while (true)
			{
				string newMsg;
				if (readAsync)
				{
					newMsg = GetMsgAsync(stream);
				}
				else
				{
					newMsg = GetMsgSync(stream);
				}



				// check if the server disconnected
				if (newMsg == "")
				{
					print("Getting no response from the server");
					return "";
				}

				recBuffer += newMsg;
				// before the semicolon the length of the following message gets send, after it the message
				if ((headerLen = recBuffer.IndexOf(';')) != -1)
				{
					break;
				}

				if (shouldReturn || !ProgramSettings.programIsRunning) return "";
				// todo: some performance tests should be done
			}

			String header = recBuffer.Substring(0, headerLen);
			PythonExecuter.incomingChanges += 1;
			RemoveString(headerLen + 1);
			msgLen = int.Parse(header);
			if (msgLen == -1)
			{
				msgLen = 0;
				return "";
			}
			
			returnedMsg = "";
		}

		// Receive data until at least the whole message has been received. Additional data will be bufferred
		// Warning: If the message is really long and the connection really slow this loop could lead to a temporary
		// screen freeze.
		while (true)
		{
			if (recBuffer.Length >= msgLen)
			{
				// the whole message arrived
				returnedMsg += recBuffer.Substring(0, msgLen - returnedMsg.Length);
				RemoveString(returnedMsg.Length);
				break;
			}

			// try to read input from the stream
			string newMsg;
			if (readAsync)
			{
				newMsg = GetMsgAsync(stream);
			}
			else
			{
				newMsg = GetMsgSync(stream);
			}

			// check if the server disconnected
			if (newMsg == "")
			{
				print("Getting no response from the server");
				return "";
			}
			recBuffer += newMsg;
		}

		msgLen = 0;

		taskNumOut += 1;
		if (taskNumOut == taskNumIn)
		{
			// all requested messages got loaded, so the loading text can be deactivated
			LoadingText.Inst.Deactivate();
		}
		return returnedMsg;
	}

	#region SendMessage

	public static string SendMsgToPythonSync(PythonCommandType exType, string msg)
	{
		string msgRes = SendMsgToPython(exType, msg, sendAsync:false);
		print(msgRes);
		if (msgRes.StartsWith("Error"))
		{
			return msgRes;
		}

		return ListenForInput(readAsync:false);
	}

	private static void TryReconnect()
	{
		if (socketConnection != null)
		{
			socketConnection.Close();
			socketConnection = null;
		}

		Inst.ConnectWithHost(null);
	}
	
	/// <summary> 	
	/// Send message to server using socket connection. 	
	/// </summary> 	
	public static string SendMsgToPython(PythonCommandType exType, string msg, bool sendAsync=true)
	{
		if (socketConnection == null || !socketConnection.Connected)
		{
			string problem = "Socket currently not connected!";
			Debug.LogWarning(problem + " socketConnection is " + socketConnection);
			ErrorTextController.inst.ShowMsg(problem);
			TryReconnect();
			return "Error: " + problem;
		}

		try
		{
			NetworkStream stream = socketConnection.GetStream();
			stream.WriteTimeout = 1000;
			if (stream.CanWrite)
			{
				string clientMessage = exType + ":" + msg;
				clientMessage = clientMessage.Length + ";" + clientMessage;
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
				// Write byte array to socketConnection stream.    
				if (sendAsync)
				{
					stream.WriteAsync(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				}
				else
				{
					stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				}
				
				taskNumIn++;

				Debug.Log(Time.time + ": Client sent his message " + msg);
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
			ErrorTextController.inst.ShowMsg(ex.ToString());
			TryReconnect();
			return "Error: " + ex;
		}
		return "async";
		// return receive(); 
	}

	#endregion

	public static void CloseServer()
	{
		if (socketConnection != null)
		{
			string rec = SendMsgToPython(PythonCommandType.exec_l, "end server");
			print("Closed the server: " + rec);
		}
	}
}
