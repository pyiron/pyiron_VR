using System;
using System.Collections;
using System.Collections.Generic;
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
	private string[] HOSTS = {"192.168.0.198", "192.168.0.196", "127.0.0.1", "130.183.226.32"};
	// private const string HOST = "192.168.0.196";// "localhost"
	private const int PORT = 65432;
	
	private static int BLOCKSIZE = 1024;
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

	private static string HandleInput()
	{
		Byte[] block = new Byte[BLOCKSIZE];
		NetworkStream stream = socketConnection.GetStream();
		int length;
		// Read incoming stream into byte arrary. 		
		int nlen = "num_blocks: ".Length + 10; // has to be the same in python
		
		if ((stream.Read(block, 0, nlen)) == 0) return ""; // might be wrong // block.Length
		PythonExecuter.incomingChanges += 1;
		var incomingData = new byte[nlen];
		Array.Copy(block, 0, incomingData, 0, nlen);
		// Convert byte array to string message. 						
		string data = Encoding.ASCII.GetString(incomingData);
		string[] d_lst = data.Split(':');
		if (d_lst[0] != "num_blocks") return data;
		if (!int.TryParse(d_lst[1], out int numBlocks))
		{
			Debug.LogWarning("Couldn't parse " + d_lst[1]);
		}

		//print("Num Blocks: " + numBlocks);
		data = "";
		for (int i = 1; i <= numBlocks; i++)
		{
			if ((length = stream.Read(block, 0, block.Length)) == 0) continue; // might be wrong
			incomingData = new byte[length];
			Array.Copy(block, 0, incomingData, 0, length);
			// Convert byte array to string message. 						
			string data_b = Encoding.ASCII.GetString(incomingData);
			data += data_b;
		}
		return data;
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
	
	private void OnApplicationQuit()
	{
		/*string rec = SendMsgToPython(ExecuteType.exec,"end server", out bool succ);
		if (!succ)
		{
			Debug.Log("Warning:" + rec);
		}*/
	}
}
