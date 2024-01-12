using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PlanningAlgorithmInterface.Socket4DataTrans
{
    public class SocketClient
    {
        public Int32 Port { get; set; } = 12223;
        public IPAddress serverIP { get; set; } = IPAddress.Loopback;
        protected IPEndPoint localEndPoint;
        public Socket ClientObj { get; set; }


//        private int timeout;

        public SocketClient()
        {
            Init();
        }

        public SocketClient(IPAddress serverIp, Int32 port)
        {
            Port = port;
            serverIP = serverIp;
            Init();
        }
        
        public bool Connect(int millisecondsTimeout = 1000, int connect_time = 10)
        {
            int count = 0;
            while (count++ < connect_time)
            {
                try
                {
                    if (TryConnection(millisecondsTimeout))
                    {
//                        ClientObj = new Socket(serverIP.AddressFamily,
//                            SocketType.Stream, ProtocolType.Tcp);
//                        ClientObj.Connect(localEndPoint);
                        // We print EndPoint information 
                        // that we are connected
                        Trace.WriteLine("Socket connected to -> {0} ",
                            ClientObj.RemoteEndPoint.ToString());
                        return true;
                    }
                    else
                    {
                        Trace.WriteLine($"Remain times:{count}");
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine($"Remain times:{count}, Exception:{e}");
                }
            }
            return false;
        }

        public string DataTransfer(string transfer_data)
        {
            try
            {
                // Creation of messagge that
                // we will send to Server

                // Data buffer
                var trans_bytes = Encoding.UTF8.GetBytes(transfer_data);

                ClientObj.Send(trans_bytes);
                //                var asy1 = ClientObj.BeginSend(trans_bytes, 0, trans_bytes.Length, SocketFlags.None,null,null);
                //                asy1.AsyncWaitHandle.WaitOne(SocketConst.DataTransferMillionSecondsTimeout);


                // We receive the messagge using 
                // the method Receive(). This 
                // method returns number of bytes
                // received, that we'll use to 
                // convert them to string


                //                Trace.WriteLine(asy1.IsCompleted);
                //                ClientObj.EndSend(asy1);
                //                Trace.WriteLine(asy1.IsCompleted);

                byte[] message_received_byte = new byte[SocketConst.ByteBufferLen];

                int byteRecv = ClientObj.Receive(message_received_byte);
                return Encoding.UTF8.GetString(message_received_byte,
                    0, byteRecv);
            }

            // Manage of Socket's Exceptions
            catch (ArgumentNullException ane)
            {

                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }

            catch (SocketException se)
            {

                Console.WriteLine("SocketException : {0}", se.ToString());
            }

            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

            return null;
        }


        public void Close()
        {
            DataTransfer(SocketConst.StrEOF);
            // Close Socket using 
            // the method Close()
            ClientObj.Shutdown(SocketShutdown.Both);
            ClientObj.Close();
        }


        protected void Init()
        {

            localEndPoint = new IPEndPoint(serverIP, Port);
            // Creation TCP/IP Socket using 
            // Socket Class Constructor
            ClientObj = new Socket(serverIP.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
        }

        protected bool TryConnection(int millisecondsTimeout = 10)
        {
            try
            {
                ClientObj = new Socket(serverIP.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                var ar = ClientObj.BeginConnect(localEndPoint, null, null);
                ar.AsyncWaitHandle.WaitOne(millisecondsTimeout);
                if (!ClientObj.Connected)
                    ClientObj.Close();
                else
                {
                    ClientObj.Send(Encoding.UTF8.GetBytes(SocketConst.StrConnTest));
                    byte[] message_received_byte = new byte[SocketConst.ByteBufferLen];
                    ClientObj.Receive(message_received_byte);
                }
                    
            
                return ClientObj.Connected;
            }
            catch (Exception e)
            {
                throw e;
            }

        }

    }
}
