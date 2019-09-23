using System;
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
	public static TCPClient inst;
	
	#region private members 	
	private static TcpClient socketConnection;
	private static NetworkStream stream;
	private Thread clientReceiveThread;
	
	// needed for asynchronous (lag free) connecting to the server
	public static Task connStatus;
	private float connTimer = 1;
	
	// the List of functions and scripts the Update function should return received messages to
	private static List<String> returnFunctions = new List<string>();
	private static List<MonoBehaviour> returnScripts = new List<MonoBehaviour>();
	public static string returnedMsg;
	// a buffer for the received data
	private static Byte[] block;
	// the task waiting for new data
	private static Task<int> socketReadTask;

	// being set to false will lead to Unity hanging while loading
	public static bool isAsync = false;
	// the ip address of the server. Warning: testing out multiple servers can lead to long loading times
	//private string[] HOSTS = {"130.183.226.32"}; //"192.168.0.198", "192.168.0.197", "127.0.0.1", "130.183.212.100", "130.183.212.82"};

	//private string HOST = "130.183.226.32";
	// private const string HOST = "192.168.0.196";// "localhost"
	public const int PORT = 65432;
	
	private static int BLOCKSIZE = 1024;
	// buffer all incoming data. Needed to deal with the TCP stream
	private static String recBuffer = "";
	// the last time the program tried to connect to a host
	private float lastStartTimer;
	#endregion

	#region Monobehaviour Callbacks

	private void Awake()
	{
		inst = this;
	}

	private void Update()
	{
		// if the TCPClient is trying to connect to the server, check if the connection could be established
		if (connStatus != null)
		{
			if (connTimer > 0)
			{
				connTimer -= Time.deltaTime;
				if (connStatus.IsFaulted || connStatus.IsCanceled)
				{
					// error
					connTimer = 1;
					print("Connection is Faulted: " + connStatus.IsFaulted);
					print("Connection is Canceled: " + connStatus.IsCanceled);
					ConnectionError();
				}
				else if (connStatus.IsCompleted)
				{
					// success
					connTimer = 1;
					print("Successfully connected to the server");
					ConnectionSuccess();
				}
			}
			else
			{
				// the server didn't respond in time
				connTimer = 1;
				ConnectionTimeout();
			}
		}

		// after sending a message to Python, check if a response arrived
		if (returnFunctions.Count > 0)
		{
			// check if new data has arrived
			HandleInput();
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
		print("Trying to connect to " + host);
		// after connecting to a server or while trying to connect to one, connecting to another one is not possible
		if (socketConnection != null || connStatus != null) return;
		
		if (PythonExecuter.useServer)
		{
			try
			{
				socketConnection = new TcpClient();
				connStatus = socketConnection.ConnectAsync(host, PORT);
			}
			catch
			{
				ConnectionError();
			}
		}
	}

	private void ConnectionSuccess()
	{
		connStatus.Dispose();
		connStatus = null;
		ModeData.inst.SetMode(Modes.Explorer);
			
		if (isAsync)
		{
			try
			{
				clientReceiveThread = new Thread(ListenForData) {IsBackground = true};
				clientReceiveThread.Start();
			}
			catch (Exception e)
			{
				Debug.Log("On client connect exception " + e);
			}
		}
		PythonExecuter.SendOrder(PythonScript.None, PythonCommandType.eval, "self.send_group()");
	}

	private void ConnectionError()
	{
		// connection failure
		print("Failed to connect");
		connStatus.Dispose();
		connStatus = null;
		socketConnection = null;
		ErrorTextController.inst.ShowMsg("Timeout: Couldn't connect to the server.");
	}

	private void ConnectionTimeout()
	{
		// connection failure caused by timeout
		print("Failed to connect due to timeout");
		connStatus.Dispose();
		connStatus = null;
		socketConnection.Close();
		socketConnection = null;
		ErrorTextController.inst.ShowMsg("Couldn't connect to the server");
	}

	#endregion

	#region Async

	/// <summary> 	
	/// Runs in background clientReceiveThread; Listens for incomming data. 	
	/// </summary>     
	private void ListenForData() { 		
		try
		{           
			while (true)
			{
				PythonExecuter.ReadInput(HandleInput());
				if (!ProgramSettings.programIsRunning)
				{
					return;
				}
			}         
		}         
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
			Debug.Log("Has the Server been started?");
		}     
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

	private static string GetMsg(NetworkStream stream)
	{			
		
		// Read incoming stream into byte array. 
		//int len = stream.Read(block, 0, BLOCKSIZE);
		if (socketReadTask == null)
		{
			block = new Byte[BLOCKSIZE + 1];
			socketReadTask = stream.ReadAsync(block, 0, BLOCKSIZE);
		}

		while (!socketReadTask.IsCompleted)
		{
			print("Still waiting ");
			return "";
		}
		// todo: why is block in ascii ""?
		int len = socketReadTask.Result;
		socketReadTask.Dispose();
		socketReadTask = null;
		
		// Convert byte array to string message. 
		if (len == 0) return "";
		block[len] = 0;
		return Encoding.ASCII.GetString(block, 0, len);
	}

	private static string HandleInput()
	{
		// get the input stream
		if (socketReadTask == null)
		{
			stream = socketConnection.GetStream();
		}

		// get the length of the message that will be send afterwards
		int headerLen;
		while (true)
		{
			string newMsg = GetMsg(stream);
			if (newMsg == "" && !isAsync) return "";
			recBuffer += newMsg;
			if ((headerLen = recBuffer.IndexOf(';')) != -1)
			{
				break;
			}

			if (!isAsync || !ProgramSettings.programIsRunning) return "";
			// todo: busy waiting for the input might be bad for the performance. Some tests should be done with
			// larger structures or at least when implementing interactive structures

			// this might help a little
			if (newMsg.Length == 0)
			{
				Thread.Sleep(10);
			}
		}

		String header = recBuffer.Substring(0, headerLen);
		PythonExecuter.incomingChanges += 1;
		RemoveString(headerLen + 1);
		print("Header is " + header);
		int msgLen = int.Parse(header);
		if (msgLen == -1) return "";
		
		// Receive data until at least the whole message has been received. Additional data will be pufferred
		//String msg = "";
		returnedMsg = "";
		while (true)
		{
			if (recBuffer.Length >= msgLen)
			{
				returnedMsg += recBuffer.Substring(0, msgLen - returnedMsg.Length);
				RemoveString(returnedMsg.Length);
				break;
			}
			recBuffer += GetMsg(stream);
		}

		if (returnFunctions.Count > 0)
		{
			returnScripts[0].Invoke(returnFunctions[0], 0);
			returnScripts.RemoveAt(0);
			returnFunctions.RemoveAt(0);
		}
		return returnedMsg;
	}
	  	
	/// <summary> 	
	/// Send message to server using socket connection. 	
	/// </summary> 	
	public static string SendMsgToPython(PythonCommandType exType, string msg,
		MonoBehaviour retScript=null, string retMeth="ReadReceivedInput")
	{
		if (socketConnection == null) {     
			Debug.LogError("No socket connection");
			return "No socket connection";         
		}

		try
		{
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite)
			{
				string clientMessage = exType + ":" + msg;
				clientMessage = clientMessage.Length + ";" + clientMessage;
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
				// Write byte array to socketConnection stream.    
				// todo: if WriteAsync is used, it might make sense to wait until it has been executed and check for errors
				stream.WriteAsync(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				Debug.Log(Time.time + ": Client sent his message " + msg);
			}
			else
			{
				Debug.LogError("stream not writable");
				return "Stream not writable";
			}
		}
		catch (SocketException ex)
		{
			Debug.LogError("InvalidOperationException: " + ex);
			ErrorTextController.inst.ShowMsg("Socket exception: " + ex);
			return "Socket Exc: " + ex;
		}
		catch (InvalidOperationException ex)
		{
			ErrorTextController.inst.ShowMsg("InvalidOperationException: " + ex);
			Debug.LogError("InvalidOperationException: " + ex + "\n" +
			               "Check that the server is still running and you have internet connection");
			return "Socket Exc: " + ex;
		}

		if (!isAsync)
		{
			returnFunctions.Add(retMeth);
			if (retScript == null)
			{
				returnScripts.Add(PythonExecuter.inst);
			}
			else
			{
				returnScripts.Add(retScript);
			}
			//return HandleInput();
		}
		return "async";
		// return receive(); 
	}

	public static void CloseServer()
	{
		if (socketConnection != null)
		{
			string rec = SendMsgToPython(PythonCommandType.exec, "end server");
			print("Closed the server: " + rec);
		}
	}
}
