using System;
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
	private Thread clientReceiveThread;
	
	// needed for asynchronous (lag free) connecting to the server
	public static Task connStatus;
	private float connTimer = 1;

	// being set to false will lead to Unity hanging while loading
	public static bool isAsync = true;
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
		if (PythonExecuter.IsLoading())
		{
			
			// todo
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
		connStatus = null;
		socketConnection = null;
		ErrorTextController.inst.ShowMsg("Timeout: Couldn't connect to the server.");
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
		Byte[] block = new Byte[BLOCKSIZE + 1];
		// Read incoming stream into byte array. 
		int len = stream.ReadAsync(block, 0, BLOCKSIZE).Result;
		// Convert byte array to string message. 
		if (len == 0) return "";
		block[len] = 0;
		return Encoding.ASCII.GetString(block, 0, len);
	}

	private static string HandleInput()
	{
		// get the input stream
		NetworkStream stream = socketConnection.GetStream();
		
		// get the length of the message that will be send afterwards
		int headerLen;
		while (true)
		{
			string newMsg = GetMsg(stream);
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
		int msg_len = int.Parse(header);
		if (msg_len == -1) return "";
		
		// Receive data until at least the whole message has been received. Additional data will be pufferred
		String msg = "";
		while (true)
		{
			if (recBuffer.Length >= msg_len)
			{
				msg += recBuffer.Substring(0, msg_len - msg.Length);
				RemoveString(msg.Length);
				break;
			}
			recBuffer += GetMsg(stream);
		}
		return msg;
	}
	  	
	/// <summary> 	
	/// Send message to server using socket connection. 	
	/// </summary> 	
	public static string SendMsgToPython(PythonCommandType exType, string msg)
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
			return HandleInput();
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
