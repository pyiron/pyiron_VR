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
	private TcpClient socketConnection; 	
	private Thread clientReceiveThread;
	private bool isAsync = false;
	// for async communication. Warning: not fully implemented
	public static int msgsIn = 0;
	public static int msgsOut;
	// the ip address of the server
	private const string HOST = "192.168.0.196";// "localhost"
	private const int PORT = 65432;
	
	private static int BLOCKSIZE = 1024;
	#endregion

	private void Awake()
	{
		inst = this;
	}

	// Use this for initialization 	
	void Start () {
		ConnectToTcpServer();     
	}  	
	// Update is called once per frame
	void Update () {         
		if (Input.GetKeyDown(KeyCode.Space)) {             
			string rec = SendMsgToPython(ExecuteType.eval, "'yay'", out bool succ);    
			if (!succ)
			{
				Debug.Log("Warning:" + rec);
			}
			else
			{
				print(rec);
			}
		}     
	} 
	
	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	
	private void ConnectToTcpServer () {
		if (isAsync)
		{
			try
			{
				clientReceiveThread = new Thread(new ThreadStart(ListenForData));
				clientReceiveThread.IsBackground = true;
				clientReceiveThread.Start();
			}
			catch (Exception e)
			{
				Debug.Log("On client connect exception " + e);
			}
		}
		else
		{
			socketConnection = new TcpClient(HOST, PORT);  
		}
	}

	#region Async

	/// <summary> 	
	/// Runs in background clientReceiveThread; Listens for incomming data. 	
	/// </summary>     
	private void ListenForData() { 		
		try { 			
			socketConnection = new TcpClient(HOST, PORT);  			
			Byte[] bytes = new Byte[1024];             
			while (true) { 				
				// Get a stream object for reading 				
				using (NetworkStream stream = socketConnection.GetStream()) { 					
					int length; 					
					// Read incoming stream into byte array. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 						
						var incomingData = new byte[length]; 						
						Array.Copy(bytes, 0, incomingData, 0, length); 						
						// Convert byte array to string message. 						
						string serverMessage = Encoding.ASCII.GetString(incomingData); 						
						Debug.Log("server message received as: " + serverMessage); 					
					} 				
				} 			
			}         
		}         
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
			Debug.Log("Has the Server been started?");
		}     
	}	

	#endregion
	
	private string Receive()
	{
		Byte[] block = new Byte[BLOCKSIZE];
		NetworkStream stream = socketConnection.GetStream();
		int length;
		// Read incoming stream into byte arrary. 		
		int nlen = "num_blocks: ".Length + 10; // has to be the same in python
		if ((stream.Read(block, 0, nlen)) == 0) return ""; // might be wrong // block.Length
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
	  	
	/// <summary> 	
	/// Send message to server using socket connection. 	
	/// </summary> 	
	public string SendMsgToPython(ExecuteType exType, string msg, out bool succ)
	{
		succ = false;
		if (socketConnection == null) {     
			//Debug.LogWarning("Error: No socket connection");
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
				//print("Error: stream not writable");
				return "Stream not writable";
			}	
		} 		
		catch (SocketException socketException) {             
			//Debug.Log("Socket exception: " + socketException);      
			return "Socket Exc: " + socketException;
		}

		if (!isAsync)
		{
			succ = true;
			return Receive();
		}

		return "Async communication not implemented!";
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

public enum ExecuteType
{
	exec, eval
}