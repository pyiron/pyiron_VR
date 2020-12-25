using System.Collections;
using System.Net.Sockets;
using System.Threading.Tasks;
using HTC.UnityPlugin.Vive;
using UnityEngine;
using UnityEngine.UI;

namespace Networking
{
	public class TCPClientConnector : MonoBehaviour
	{
		public static TCPClientConnector Inst;
    
		private string _host;
		public const int PORT = 65432;
    
		// needed for asynchronous (lag free) connecting to the server
		public static Task ConnectionStatus;
		// the time until the connection is declared as not existent
		private static float connTimeOut = 3;
		private float _connectionTimer = connTimeOut;

		private void Awake()
		{
			Inst = this;
		}

		/// <summary>
		/// Try to connect to a host server with the given IP
		/// </summary>
		/// <param name="host">The IP of the host server.</param>
		public void ConnectWithHost(string host)
		{
			// if no new host is specified, try to load the one we have been connected to before 
			// (needed for temporary internet outages)
			if (host != null)
			{
				// save the host
				_host = host;
			}

			Connect(false);
		}

		private void Connect(bool isReconnectAttempt)
		{
			print("Trying to connect to " + _host);
		
			// show that the program is loading
			AnimatedText.Instances[TextInstances.LoadingText].Activate();
		
			// after connecting to a server or while trying to connect to one, connecting to another one is not possible
			if (TCPClient.SocketConnection != null || ConnectionStatus != null) return;
		
			try
			{
				TCPClient.SocketConnection = new TcpClient();
				ConnectionStatus = TCPClient.SocketConnection.ConnectAsync(_host, PORT);
				StartCoroutine(TryToConnect(isReconnectAttempt));
			}
			catch
			{
				ConnectionError(isReconnectAttempt);
			
				// the loading is over, deactivate the loading text
				AnimatedText.Instances[TextInstances.LoadingText].Deactivate();
			}
		}
	
		/// <summary>
		/// Should be called when the connection breaks to try to establish a new connection.
		/// </summary>
		public static void TryReconnect()
		{
			AnimatedText reconnectText = AnimatedText.Instances[TextInstances.ReconnectingText];
			reconnectText.transform.SetParent(ModeController.currentMode.controller.transform);
			reconnectText.transform.localPosition = Vector3.up * 260;
			reconnectText.Activate();
		
			Utilities.DeactivateInteractables();
		
			if (TCPClient.SocketConnection != null)
			{
				TCPClient.SocketConnection.Close();
				TCPClient.SocketConnection = null;
			}

			Inst.Connect(true);
		}

		private IEnumerator TryToConnect(bool isReconnectAttempt)
		{
			// the time between each update if there has been an update to the connection status
			float updateTime = 0.4f;
		
			while (_connectionTimer > 0)
			{
				_connectionTimer -= updateTime;
				if (ConnectionStatus.IsFaulted || ConnectionStatus.IsCanceled)
				{
					// error
					_connectionTimer = connTimeOut;
					print("Connection is Faulted: " + ConnectionStatus.IsFaulted);
					print("Connection is Canceled: " + ConnectionStatus.IsCanceled);
					ConnectionError(isReconnectAttempt);
					break;
				}
				if (ConnectionStatus.IsCompleted)
				{
					// success
					_connectionTimer = connTimeOut;
					print("Successfully connected to the server");
					ConnectionSuccess(isReconnectAttempt);
					break;
				}
				yield return new WaitForSeconds(updateTime);
			}
			if (_connectionTimer <= 0)
			{
				// the server didn't respond in time
				_connectionTimer = connTimeOut;
				ConnectionTimeout(isReconnectAttempt);
			}
		
			// the loading is over, deactivate the loading text
			AnimatedText.Instances[TextInstances.LoadingText].Deactivate();
		}
	
		#region ConnectionOutcome
		/// <summary>
		/// Handles Connection Success by discarding the connection status and calling functions that should be
		/// executed after the connection has been established.
		/// </summary>
		/// <param name="isReconnectAttempt">Decides which elements should be called and updated.</param>
		private void ConnectionSuccess(bool isReconnectAttempt)
		{
			ConnectionStatus.Dispose();
			ConnectionStatus = null;

			if (isReconnectAttempt)
			{
				AnimatedText.Instances[TextInstances.ReconnectingText].Deactivate();
			
				// activate all interactable elements in the scene
				Utilities.ActivateInteractables();
			}
			else
			{

				NetworkMenuController.Inst.keyboard.SetActive(false);

				// load the content of the start path (which is defined in the Python script)
				// ExplorerMenuController.Inst.LoadPathContent();

				ModeController.inst.SetMode(Modes.Structure);

				Boundingbox.Inst.gameObject.SetActive(true);
			}
		}

		/// <summary>
		/// Handles Connection Errors by closing the connection.
		/// </summary>
		/// <param name="isReconnectAttempt">If set to true, another connection attempt will be started</param>
		private void ConnectionError(bool isReconnectAttempt)
		{
			// connection failure
			print("Failed to connect");

			ConnectionStatus?.Dispose();

			ConnectionStatus = null;
			TCPClient.SocketConnection = null;
		
			if (isReconnectAttempt)
			{
				// try to connect again
				Connect(true);
			}
			else
			{
				ErrorTextController.inst.ShowMsg("Couldn't connect to the server.");
			}
		}

		/// <summary>
		/// Handles Connection Timeouts by closing the connection.
		/// </summary>
		/// <param name="isReconnectAttempt">If set to true, another connection attempt will be started</param>
		private void ConnectionTimeout(bool isReconnectAttempt)
		{
			// connection failure caused by timeout
			print("Failed to connect due to timeout");
		
			ConnectionStatus = null;
			TCPClient.SocketConnection.Close();
			TCPClient.SocketConnection = null;
		
			if (isReconnectAttempt)
			{
				// try to connect again
				Connect(true);
			}
			else
			{
				ErrorTextController.inst.ShowMsg("Couldn't connect to the server");
			}
		}
		#endregion
	}
}
