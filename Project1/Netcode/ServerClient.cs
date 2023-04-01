using System;
using System.Net.Sockets;

namespace Project1.Netcode
{
    /// <summary>
    /// The ServerClient class, which is the representation of a Client on the Server itself
    /// </summary>
    public class ServerClient
    {
        // The Id of the ServerClient
        public int Id { get; set; }

        // The TCP Object of the ServerClient
        public TCP Tcp { get; set; }

        // The DataBufferSize of the ServerClient's Send and Receive Buffers (4 Megabytes)
        public static int DataBufferSize = 4096;

        /// <summary>
        /// The Constructor for the ServerClient class
        /// </summary>
        /// <param name="id">The Id of the new ServerClient</param>
        public ServerClient(int id)
        {
            Id = id;
            Tcp = new TCP(id);
        }

        /// <summary>
        /// The TCP class, which represents an interface for the client to communicate via TCP with the server
        /// </summary>
        public class TCP
        {
            // The Socket object, which will represent the client connection on the server
            public TcpClient Socket { get; set; }

            // The Id of the Client
            private readonly int _id;

            // The stream of data being transferred over the network
            private NetworkStream _stream;

            // The receiveBuffer, which is a buffer of the received bytes
            private byte[] _receiveBuffer;

            /// <summary>
            /// The Constructor for the TCP class
            /// </summary>
            /// <param name="id">The Id of the ServerClient</param>
            public TCP(int id)
            {
                _id = id;
            }

            /// <summary>
            /// Receives the incoming TCP connection request from the client and establishes a connection with the server
            /// </summary>
            /// <param name="socket">The incoming TCP Connection</param>
            public void Connect(TcpClient socket)
            {
                // Set up the TCP Socket connection
                Socket = socket;
                Socket.ReceiveBufferSize = DataBufferSize;
                Socket.SendBufferSize = DataBufferSize;

                // Get the Stream of incoming bytes
                _stream = socket.GetStream();
                _receiveBuffer = new byte[DataBufferSize];

                // Read the incoming stream into the receiveBuffer
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
                    Console.WriteLine($"ServerClient error receiving TCP data: {ex}");
                }
            }
        }
    }
}
