using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PlanningAlgorithmInterface.Socket4DataTrans
{
    public class SocketTestServer
    {
        public static void ExecuteServer()
        {
            // Establish the local endpoint 
            // for the socket. Dns.GetHostName
            // returns the name of the host 
            // running the application.
            IPAddress ipAddr = IPAddress.IPv6Loopback; ;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 22222);

            // Creation TCP/IP Socket using 
            // Socket Class Costructor
            Socket listener = new Socket(ipAddr.AddressFamily,
                         SocketType.Stream, ProtocolType.Tcp);

            try
            {

                // Using Bind() method we associate a
                // network address to the Server Socket
                // All client that will connect to this 
                // Server Socket must know this network
                // Address
                listener.Bind(localEndPoint);

                // Using Listen() method we create 
                // the SocketClient list that will want
                // to connect to Server
                listener.Listen(10);

                while (true)
                {

                    Console.WriteLine("Waiting connection ... ");

                    // Suspend while waiting for
                    // incoming connection Using 
                    // Accept() method the server 
                    // will accept connection of client
                    Socket clientSocket = listener.Accept();

                    // Data buffer
                    byte[] bytes = new Byte[SocketConst.ByteBufferLen];
                    string data = null;

                    while (true)
                    {

                        int numByte = clientSocket.Receive(bytes);

                        data = Encoding.ASCII.GetString(bytes,
                                                   0, numByte);

                        Console.WriteLine("Text received -> {0} ", data);

                        if (data == SocketConst.StrConnTest)
                        {
                            byte[] message = Encoding.ASCII.GetBytes(SocketConst.StrConnTest);
                            clientSocket.Send(message);
                        }
                        else
                        {
                            // Send a message to SocketClient 
                            // using Send() method

                            var c =  Console.ReadLine();

                            Thread.Sleep(100); //simulate the worknig progress

                            byte[] message = Encoding.ASCII.GetBytes(c.Trim());
                            clientSocket.Send(message);
                        }

                        if (data.IndexOf(SocketConst.StrEOF) > -1)
                            break;
                    }


                    // Close client Socket using the
                    // Close() method. After closing,
                    // we can use the closed Socket 
                    // for a new SocketClient Connection
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}