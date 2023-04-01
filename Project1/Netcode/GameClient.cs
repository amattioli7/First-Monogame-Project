using System;
using System.Net.Sockets;

namespace Project1.Netcode
{
    /// <summary>
    /// The GameClient class, which is a Singleton (only instantiated once per app instance) class representing the TCP Client
    /// </summary>
    public class GameClient
    {
        // The Singleton instance of the GameClient class
        public static GameClient Instance;

        // The DataBufferSize of the GameClient's Send and Receive Buffers (4 Megabytes)
        public static int DataBufferSize = 4096;

        // The IP Address of the Server
        public string ServerIP = "127.0.0.1"; // localhost for now

        // The Port that the Server is listening on
        public int Port = 26950;

        // The Id of the GameClient
        public int Id;

        // The TCP Object of the GameClient
        public TCP Tcp;

        /// <summary>
        /// The Constructor for the GameClient class
        /// </summary>
        protected GameClient()
        {
            Tcp = new TCP();
        }

        /// <summary>
        /// Returns a new GameClient instance if one does not already exist, or returns the existing GameClient instance
        /// </summary>
        /// <returns></returns>
        public static GameClient GetGameClientInstance()
        {
            if (Instance == null)
            {
                Instance = new GameClient();
            }

            return Instance;
        }

        /// <summary>
        /// Connects the GameClient to the Server
        /// </summary>
        public void ConnectToServer()
        {
            Tcp.Connect();
        }

        /// <summary>
        /// The TCP class, which represents an interface for the client to communicate via TCP with the server
        /// </summary>
        public class TCP
        {
            // The Socket object, which will represent the client connection on the server
            public TcpClient Socket;

            // The stream of data being transferred over the network
            private NetworkStream _stream;

            // The receiveBuffer, which is a buffer of the received bytes
            private byte[] _receiveBuffer;

            /// <summary>
            /// Sends a TCP connection request out to the server
            /// </summary>
            public void Connect()
            {
                // Set up the TCP Socket connection
                Socket = new TcpClient();
                Socket.ReceiveBufferSize = DataBufferSize;
                Socket.SendBufferSize = DataBufferSize;

                // Begin the connection with the Server
                _receiveBuffer = new byte[DataBufferSize];
                Socket.BeginConnect(Instance.ServerIP, Instance.Port, ConnectCallback, Socket);
            }

            /// <summary>
            /// Called after a TCP connection attempt with the server is made
            /// </summary>
            /// <param name="result"></param>
            private void ConnectCallback(IAsyncResult result)
            {
                // End the connection attempt
                Socket.EndConnect(result);

                // If the GameClient failed to connect to the server, return
                if (!Socket.Connected)
                {
                    Console.WriteLine($"Failed to connect to server at {Socket.Client.RemoteEndPoint}");
                    return;
                }

                // Begin reading the stream from the server into the receiveBuffer
                _stream = Socket.GetStream();
                _stream.BeginRead(_receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
            }

            /// <summary>
            /// Called after the BeginRead method is complete
            /// </summary>
            /// <param name="result">The result of the stream being read in</param>
            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    // Get the number of bytes that were read into the receiveBuffer
                    int byteLength = _stream.EndRead(result);

                    // If nothing was read in, return to skip 
                    if (byteLength <= 0)
                    {
                        return;
                    }

                    // Copy the receiveBuffer contents to the data array
                    byte[] data = new byte[byteLength];
                    Array.Copy(_receiveBuffer, data, byteLength);

                    // I am assuming we will process the received message here

                    // Continue reading data from the stream
                    _stream.BeginRead(_receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GameClient error receiving TCP data: {ex}");
                }
            }
        }

    }
}
