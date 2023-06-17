// See https://aka.ms/new-console-template for more information
using System.Net.Sockets;
using System.Net;
using System.Text;

internal class Program
{
    private static async global::System.Threading.Tasks.Task Main(string[] args)
    {
        int port = 8005; // порт для приема входящих запросов
                         //получаем адреса для запуска сокета
        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

        using Socket listener = new(  ipPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        listener.Bind(ipPoint);
        listener.Listen(100);

        async Task Echo()
        {
            var handler = await listener.AcceptAsync();
            // Receive message.
            var buffer = new byte[1_024];
            var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, received);

            Console.WriteLine(
                $"Socket server received message:{response}");

            var echoBytes = Encoding.UTF8.GetBytes(response);
            await handler.SendAsync(echoBytes, 0);
            Console.WriteLine($"Socket server sent response");
        }

        async Task SendAsync(byte[] echoBytes, List<Socket> connectionList)
        {
            foreach (var handler in connectionList)
            {
                handler.SendAsync(echoBytes, 0);
            }
        }
        async Task Listen(Socket handler, List<Socket> connectionList)
        {
            while (true)
            {
                var buffer = new byte[1_024];
                var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);

                Console.WriteLine(
                    $"Socket server received message:{response}");

                var echoBytes = Encoding.UTF8.GetBytes(response);
                await SendAsync(echoBytes, connectionList);
                Console.WriteLine($"Socket server sent response to all clients");
            }
        }

        List<Socket> connectionList = new List<Socket>();
        while (true)
        {
            try
            {
                var handler = await listener.AcceptAsync();
                connectionList.Add(handler);
                Listen(handler, connectionList);
            }
            catch { }
        }
    }
}
