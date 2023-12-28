using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using RzdHack_Robot.Core;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace RzdHack_Robot.App
{
    public static class TCP
    {
        private const int PORT_LISTENING= 4444;
        private const int PORT_SENDING= 2222;
        private const string SERVER_IP = "127.0.0.1";
        public static void Listening()
        {
            IPEndPoint ipEndPoint = new(IPAddress.Parse(SERVER_IP), PORT_LISTENING);
            TcpListener server = new TcpListener(ipEndPoint);

            try
            {
                server.Start();
                Console.WriteLine("Server start. Waiting for connections...");

                while (true)
                {
                    byte[] buffer = new byte[1024];
                    using var tcpClient = server.AcceptTcpClient();
                    Console.WriteLine($"Время: {DateTime.Now.TimeOfDay}\nВходящее подключение: {tcpClient.Client.RemoteEndPoint}");
                    var stream = tcpClient.GetStream();
                    int bytes;
                    var data = new StringBuilder();
                    do
                    {
                        bytes = stream.Read(buffer);
                        data.Append(Encoding.UTF8.GetString(buffer, 0, bytes));
                    }
                    while (bytes > 0);
                                                
                    Console.WriteLine($"Сообщение:{data}");
                    var dataRailwayInput = JsonConvert.DeserializeObject<RailwayInput>(data.ToString());
                    new Steps().Notification(dataRailwayInput);
                }
            }
            
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

            finally{server.Stop();}
        }

        public static void Sending(ResponseToUser response)
        {
            IPEndPoint ipEndPoint = new(IPAddress.Parse(SERVER_IP), PORT_SENDING);
            using Socket client = new Socket(
                ipEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            try
            {
                client.Connect(ipEndPoint);

                // Forming JSON from data
                var responeJson = JsonSerializer.Serialize(response);
                // Send message.
                var messageBytes = Encoding.UTF8.GetBytes(responeJson);
                _ = client.Send(messageBytes, SocketFlags.None);
                Console.WriteLine($"Socket client sent message: \"{responeJson}\"");
            }

            catch(Exception ex) {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}

//Old
//public static async Task SendMessage_old(string message)
//{
//    int PORT_NO = 2222;
//    string SERVER_IP = "127.0.0.1";

//    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2221);
//    using Socket tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

//    try
//    {
//        tcpListener.Bind(ipPoint);
//        tcpListener.Listen();    // запускаем сервер
//        Console.WriteLine("Сервер запущен. Ожидание подключений... ");

//        while (true)
//        {
//            // определяем данные для отправки - текущее время
//            byte[] data = Encoding.UTF8.GetBytes(message);
//            // получаем входящее подключение
//            using var tcpClient = tcpListener.Accept();
//            // отправляем данные
//            await tcpClient.SendAsync(data,SocketFlags.None);
//            Console.WriteLine($"Клиенту {tcpClient.RemoteEndPoint} отправлены данные");
//        }
//    }

//    catch (Exception ex)
//    {
//        Console.WriteLine(ex.Message);
//    }
//}
//public static async Task SendMessage_old1(string message)
//{
//    int PORT_NO = 2222;
//    string SERVER_IP = "127.0.0.1";

//    //---listen at the specified IP and port no.---
//    IPAddress localAdd = IPAddress.Parse(SERVER_IP);
//    TcpListener listener = new TcpListener(localAdd, PORT_NO);
//    Console.WriteLine("Listening...");
//    listener.Start();

//    //---incoming client connected---
//    TcpClient client = listener.AcceptTcpClient();

//    //---get the incoming data through a network stream---
//    NetworkStream nwStream = client.GetStream();
//    byte[] buffer = new byte[client.ReceiveBufferSize];

//    //---read incoming stream---
//    int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

//    //---convert the data received into a string---
//    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
//    Console.WriteLine("Received : " + dataReceived);

//    //---write back the text to the client---
//    Console.WriteLine("Sending back : " + dataReceived);
//    nwStream.Write(buffer, 0, bytesRead);
//    client.Close();
//    listener.Stop();
//    Console.ReadLine();
//}
//public static void Listening_old()
//{
//    int PORT_NO = 4444;
//    string SERVER_IP = "127.0.0.1";
//    try
//    {
//        IPEndPoint ipEndPoint = new(IPAddress.Parse(SERVER_IP), PORT_NO);
//        using Socket server = new Socket(
//            ipEndPoint.AddressFamily,
//            SocketType.Stream,
//            ProtocolType.Tcp);

//        server.Bind(ipEndPoint);
//        server.Listen();    // запускаем сервер
//        Console.WriteLine("Сервер запущен");
//        while (true)
//        {
//            // получаем входящее подключение
//            using var tcpClient = server.Accept();
//            using var stream = new NetworkStream(server);
//            // буфер для получения данных
//            var responseData = new byte[512];
//            // получаем данные
//            var bytes = stream.Read(responseData);
//            // преобразуем полученные данные в строку
//            string response = Encoding.UTF8.GetString(responseData, 0, bytes);
//            // выводим данные на консоль
//            Console.WriteLine(response);
//            Console.WriteLine($"Клиент {tcpClient.RemoteEndPoint}");
//        }
//    }

//    catch (Exception ex)
//    {
//        Console.WriteLine(ex.Message);
//        throw;
//    }
//}