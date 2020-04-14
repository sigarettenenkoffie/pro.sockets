using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace Pro.Sockets.Lib
{
    public class Server
    {
        public static void Listener(int port)
        {
            // To use UDP communication set SocketType to Dgram and ProtocolType to Udp
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.ReceiveTimeout = 50000; // Receive timeout 5 seconds
            listener.SendTimeout = 50000; // Send timeout 5 seconds

            listener.Bind(new IPEndPoint(IPAddress.Any, port));
            listener.Listen(backlog: 20); // how many clients can connect concurrently before their connection is dealt with

            Console.WriteLine($"Listener started on port {port}");

            var cts = new CancellationTokenSource();
            var tf = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);
            tf.StartNew(() => // listener task
            {
                Console.WriteLine("listener task started");
                while (true) // after client connects, this method needs to be invoked again
                {
                    if (cts.Token.IsCancellationRequested)
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        break;
                    }
                    Console.WriteLine("Waiting for accept");

                    Socket client = listener.Accept(); // this method blocks the thread until a client connects
                    if (!client.Connected)
                    {
                        Console.WriteLine("not connected");
                        continue;
                    }
                    Console.WriteLine($"client connected local address {((IPEndPoint)client.LocalEndPoint).Address} and port {((IPEndPoint)client.LocalEndPoint).Port}, remote address {((IPEndPoint)client.RemoteEndPoint).Address} and port {((IPEndPoint)client.RemoteEndPoint).Port}");
                    Task t = CommunicateWithClientUsingSocketAsync(client); // Task used to read and write using the socket
                }
                listener.Dispose();
                Console.WriteLine("Listener task closing");

            }, cts.Token);
            Console.WriteLine("Press return to exit");
            Console.ReadKey();
            cts.Cancel();
        }

        // 
        private static Task CommunicateWithClientUsingSocketAsync(Socket socket)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (socket)
                    {
                        bool completed = false;
                        do
                        {
                            byte[] readBuffer = new byte[1024];
                            int read = socket.Receive(readBuffer, 0, 1024, SocketFlags.None);
                            string fromClient = Encoding.UTF8.GetString(readBuffer, 0, read);
                            Console.WriteLine($"read {read} bytes: {fromClient}");
                            if (string.Compare(fromClient, "shutdown", ignoreCase: true) == 0)
                            {
                                completed = true;
                            }
                            byte[] writeBuffer = Encoding.UTF8.GetBytes($"echo {fromClient}");
                            int send = socket.Send(writeBuffer);
                            Console.WriteLine($"sent {send} bytes");
                        } while (!completed);
                    }
                    Console.WriteLine("closed stream and client socket");
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            });
        }
    }
}
