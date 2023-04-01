using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Project1.Netcode
{
    /// <summary>
    /// The Server class, which contains the net-code logic responsible for hosting the game server
    /// </summary>
    public class Server
    {
        // The max number of players for this server
        public static int MaxPlayers { get; set; }

        // The port this server will listen on
        public static int Port { get; set; }

        // The TCP Listener this server uses to listen to network traffic
        private static TcpListener _tcpListener;

        // The Dictionary of ServerClient objects
        public static Dictionary<int, ServerClient> ServerClients = new Dictionary<int, ServerClient>();

        /// <summary>
        /// Starts the server with a maximum amount of players, and begins listening for TCP traffic from any IP address over the specified port
        /// </summary>
        /// <param name="maxPlayers">The maximum amount of players this server will allow</param>
        /// <param name="port">The port this server will listen on</param>
        public static void Start(int maxPlayers, int port)
        {
            // Set the private data
            MaxPlayers = maxPlayers;
            Port = port;

            // Initialize the Dictionary of ServerClients with the amount from maxPlayers
            Console.WriteLine("Starting server...");
            InitializeServerData();

            // Create a new TcpListener which will listen on the specified port and look for traffic from any IP address
            _tcpListener = new TcpListener(IPAddress.Any, Port);

            // Start the TcpListener, and begin accepting Tcp connections from clients
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Server Started on port {Port}.");
        }

        /// <summary>
        /// Called when a new Tcp Connection is made with the server
        /// </summary>
        /// <param name="result">The incoming connection</param>
        private static void TCPConnectCallback(IAsyncResult result)
        {
            // Gain access to the connection with the incoming client
            TcpClient client = _tcpListener.EndAcceptTcpClient(result);

            // Start up the listener again to listen for additional clients
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            // Assign the connection to the ServerClients Dictionary if there is still room in the server
            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (ServerClients[i].Tcp.Socket == null)
                {
                    // Establish the connection, and return
                    ServerClients[i].Tcp.Connect(client);
                    return;
                }
            }

            // If we reach the end of the loop, the server is full
            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server is full!");
        }

        /// <summary>
        /// Initializes the Server's ServerClients Dictionary with new ServerClients
        /// </summary>
        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                ServerClients.Add(i, new ServerClient(i));
            }
        }
    }
}
