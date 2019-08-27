using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPClient : MonoBehaviour
{
	public static TCPClient inst;
	#region private members 	
	private static TcpClient socketConnection; 	
	private Thread clientReceiveThread;

	// might crash if set to false at the moment
	public static bool isAsync = true;
	// the ip address of the server. Warning: testing out multiple servers can lead to severe loading times (eg. 90s)
	private string[] HOSTS = {"130.183.226.32"}; //"192.168.0.198", "192.168.0.197", "127.0.0.1", "130.183.212.100", "130.183.212.82"};
	// private const string HOST = "192.168.0.196";// "localhost"
	private const int PORT = 65432;
	
	private static int BLOCKSIZE = 1024;
	// buffer all incoming data. Needed to deal with the TCP stream
	private static String recBuffer = "";
	public static float st;
	private bool clientIsStarted;
	// the last time the program tried to connect to a host
	private float lastStartTimer;
	#endregion

	#region Monobehaviour Callbacks

	private void Awake()
	{
		inst = this;
	}

	private void StartClient()
	{
		if (PythonExecuter.useServer)
		{
			ConnectWithHost();
			if (!clientIsStarted) return;
			if (isAsync)
			{
				try
				{
					clientReceiveThread = new Thread(ListenForData);
					clientReceiveThread.IsBackground = true;
					clientReceiveThread.Start();
				}
				catch (Exception e)
				{
					Debug.Log("On client connect exception " + e);
				}
			}
			PythonExecuter.SendOrder(PythonScript.None, PythonCommandType.eval, "self.send_group()");
		}
	}

	// Use this for initialization 	
	void Start () {
		StartClient();
	}

	private void Update()
	{
		if (clientIsStarted) return;
		if (Time.time - lastStartTimer > 10)
		{
			StartClient();
			lastStartTimer = Time.time;
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

	private void ConnectWithHost()
	{
		foreach (string host in HOSTS)
		{
			try
			{
				socketConnection = new TcpClient(host, PORT);
				clientIsStarted = true;
				print("Successfully connected with HOST " + host);
				return;
			}
			catch
			{
				print("Couldn't connect with Host " + host);
			}
		}
	}

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
		int len = stream.Read(block, 0, BLOCKSIZE);
		print("Len is " + len + ", programIsRunning is " + ProgramSettings.programIsRunning);
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
			recBuffer += GetMsg(stream);
			if ((headerLen = recBuffer.IndexOf(';')) != -1)
			{
				break;
			}

			if (!isAsync || !ProgramSettings.programIsRunning) return "";
			// todo: busy waiting for the input might be bad for the performance. Some tests should be done with
			// larger structures or at least when implementing interactive structures
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
			print("Waiting for the msg with len " + msg_len);
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

		st = Time.time;
		try
		{
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite) {                 
				string clientMessage = exType + ":" + msg;
				clientMessage = clientMessage.Length + ";" + clientMessage;
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage); 				
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);  
				//Debug.Log("Client sent his message - should be received by server");  
			}
			else
			{
				Debug.LogError("stream not writable");
				return "Stream not writable";
			}	
		} 		
		catch (SocketException socketException) {             
			Debug.LogError("Socket exception: " + socketException);      
			return "Socket Exc: " + socketException;
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
		string rec = SendMsgToPython(PythonCommandType.exec,"end server");
		print("Closed the server: " + rec);
	}
}
