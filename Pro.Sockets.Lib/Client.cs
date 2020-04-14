using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;

namespace Pro.Sockets.Lib
{
    public class Client
    {
        public static async Task SendAndReceiveAsync(string hostname, int port)
        {
            try
            {
                IPHostEntry ipHost = await Dns.GetHostEntryAsync(hostname);
                IPAddress ipAddress = ipHost.AddressList.Where(
                    address => address.AddressFamily == AddressFamily.InterNetwork).First();
                if (ipAddress == null)
                {
                    Console.WriteLine("no IPv4 address");
                    return;
                }
                using (var client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp))
                {
                    client.Connect(ipAddress, port);
                    Console.WriteLine("client succesfully connected");
                    var stream = new NetworkStream(client);
                    var cts = new CancellationTokenSource();
                    Task tSender = SenderAsync(stream, cts);
                    Task tReceiver = ReceiverAsync(stream, cts.Token);
                    await Task.WhenAll(tSender, tReceiver);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task SenderAsync(NetworkStream stream, CancellationTokenSource cts)
        {
            Console.WriteLine("Sender Task");
            while (true)
            {
                Console.WriteLine("enter a string to send, shutdown to exit");
                string line = Console.ReadLine();
                byte[] buffer = Encoding.UTF8.GetBytes($"{line}\r\n");
                await stream.WriteAsync(buffer, 0, buffer.Length);
                await stream.FlushAsync();
                if (string.Compare(line, "shutdown", ignoreCase: true) == 0)
                {
                    cts.Cancel();
                    Console.WriteLine("sender task closes");
                    break;
                }
            }
        }

        private const int ReadBufferSize = 1024;

        public static async Task ReceiverAsync(NetworkStream stream, CancellationToken token)
        {
            try
            {
                stream.ReadTimeout = 50000;
                Console.WriteLine("Receiver task");
                byte[] readBuffer = new byte[ReadBufferSize];
                while (true)
                {
                    Array.Clear(readBuffer, 0, ReadBufferSize);
                    int read = await stream.ReadAsync(readBuffer, 0, ReadBufferSize, token);
                    string receivedLine = Encoding.UTF8.GetString(readBuffer, 0, read);
                    Console.WriteLine($"Received {receivedLine}");
                }
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
