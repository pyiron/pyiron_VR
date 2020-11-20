## public attributes
### TcpClient SocketConnection
The current connection. Needed to send and receive messages. Established by the TCPClientConnector.

### int TaskNumIn & int TaskNumOut
Shows how many messages have been received and send. If TaskNumOut > TaskNumIn, then the client is still waiting for a response from the server.

### string ReturnedMsg
The last message received by the server.
