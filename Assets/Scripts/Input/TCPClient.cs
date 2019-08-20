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

	public static bool isAsync = true;
	// the ip address of the server
	private string[] HOSTS = {"192.168.0.198", "192.168.0.197", "127.0.0.1", "130.183.212.100", "130.183.226.32"};
	// private const string HOST = "192.168.0.196";// "localhost"
	private const int PORT = 65432;
	
	private static int BLOCKSIZE = 1024;
	// buffer all incoming data. Needed to deal with the TCP stream
	private static String recBuffer = "";
	#endregion

	#region Monobehaviour Callbacks

	private void Awake()
	{
		inst = this;
	}

	// Use this for initialization 	
	void Start () {
		if (PythonExecuter.useServer)
		{
			ConnectToTcpServer();
			// PythonExecuter.SendOrder(PythonScript.None, PythonCommandType.exec_l, "pr = Project('.')");
			// PythonExecuter.SendOrder(PythonScript.None, PythonCommandType.exec_l, "unity_manager = UM.UnityManager()");
			PythonExecuter.SendOrder(PythonScript.None, PythonCommandType.eval, "self.send_group()");
		}
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
				print("Successfully connected with HOST " + host);
				return;
			}
			catch
			{
				print("Couldn't connect with Host " + host);
			}
		}
	}
	
	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	
	private void ConnectToTcpServer () {
		
		ConnectWithHost();
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
		// Convert byte array to string message. 
		if (len == 0) return "";
		block[len] = 0;
		string res = Encoding.ASCII.GetString(block, 0, len);
		//print("res " + res);
		return res;
	}

	private static int GetMsgLen(NetworkStream stream)
	{
		int headerLen;
		while (true)
		{
			recBuffer += GetMsg(stream);
			if ((headerLen = recBuffer.IndexOf(';')) != -1)
			{
				break;
			}
			// todo: fix this busy waiting, which is not good for the performance
		}

		String header = recBuffer.Substring(0, headerLen);
		int msg_len = int.Parse(header);
		PythonExecuter.incomingChanges += 1;
		RemoveString(headerLen + 1);
		Debug.Log("Msg len is " + msg_len + ", recbuffer is " + recBuffer + ", len " + recBuffer.Length);
		return msg_len;
	}

	private static string HandleInput()
	{
		//print("got into HandleInput");
		// get the length of the message
		NetworkStream stream = socketConnection.GetStream();
		int msg_len = GetMsgLen(stream);
		if (msg_len == -1) return "";
		
		// Receive data until at least the whole message has been received
		String msg = "";

		while (true)
		{
			//print("rl " + recBuffer.Length);
			if (recBuffer.Length >= msg_len)
			{
				//print("Now recbuffer is " + recBuffer + ", len " + recBuffer.Length);
				msg += recBuffer.Substring(0, msg_len - msg.Length);
				RemoveString(msg.Length);
				break;
			}
			recBuffer += GetMsg(stream);
		}
		//print("returned end " + msg);
		return msg;
	}
	
	private static string Receive()
	{	
		return HandleInput();
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
			if (stream.CanWrite) {                 
				string clientMessage = exType + ":" + msg; 				
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
			return Receive();
		}
		return "async";
		// return receive(); 
	}

	public static void CloseServer()
	{
		string rec = SendMsgToPython(PythonCommandType.exec,"end server");
		print("Closed the server: " + rec);
	}
	
	private void OnApplicationQuit()
	{
		CloseServer();
	}
}
