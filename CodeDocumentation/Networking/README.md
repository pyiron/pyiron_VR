# General

Calculations can be performed on a HPC Cluster, while the visualization can run on another computer, e.g. on a mobile Oculus Quest. To achieve this, a server has to be started on a HPC cluster with pyiron installed, and the client has to run on the VR Headset. Using TCP, these sockets then connect to each other. In this documentation, this process is explained for the Unity Side.

## Structure

Currently, 4 scripts manage the networking. The _TCPClientConnector_ establishes the connection between the server and the socket and handles all cases that can happen in this process, e.g. connecting to an invalid server. The _TCPClient_ handles the connection when it has been established, allowing to send and receive messages. The _PythonCommand_ script allows to create the strings that should be send to the server, containing Python Syntax. It is needed to have an overview over all currently used Python commands, e.g. to see which Python functions are not in use anymore. The _PythonExecutor_ handles send and receive calls and handles events that should happen when a message is send, e.g. activating a loading message. It delegates the calls to the TCPClient, so it should be used when sending messages.

## Sending messages

Messages can be send either synchronous or asynchronous. Sending them synchronous is faster, but creates a screen freeze until the response arrives. The time for the response depends on the time for the calculation of the server and on the travelling time of the message. This means, that long calculations or loading large amounts of data have to be send asynchronously. Small amounts can be send synchronously, but still bare the risk of creating a screen freeze. Therefore, asynchronous cummunication should be used in almost all cases, although it might bring some code and performance overhead.

To send messages, the functions _SendOrderSync_ and _SendOrderAsync_ of the Script _PythonExecutor_ should be used. _SendOrderSync_ waits and returns the message response directly. _SendOrderAsync_ calls the method _onReceiveCallback_ once the response of the server has arrived and passes the response over as an function argument.

## Internet outage handling

Internet outage or similar errors can be detected. If such an error is detected, the programm will asynchronously try to reconnect every few seconds, and display a message to the user notifying him on the problem and the recconnection attempts.
