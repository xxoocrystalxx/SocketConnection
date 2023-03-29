using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using CryptoHelper;

namespace Server
{
    internal class Program
    {
        public static List<Client> clients = new List<Client>();

        public static void Main(String[] args)
        {
            StartServer();

        }

        public static void StartServer()
        {
            // Get Host IP Address that is used to establish a connection
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1
            // If a host has multiple addresses, you will get a list of addresses
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            //IPAddress ipAddress = IPAddress.Parse("10.100.0.126");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            try
            {

                // Create a Socket that will use Tcp protocol
                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                listener.Bind(localEndPoint);

                listener.Listen(10);

                Console.WriteLine("Waiting for a connection...");

                while (true)
                {
                    Socket handler = listener.Accept();

                    Thread thread = new Thread(h => Worker((Socket)h));
                    thread.Start(handler);

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            //Console.WriteLine("\n Press any key to continue...");
            //Console.ReadKey();
        }

        static void Worker(Socket handler)
        {
            // Do some work in the worker thread

            string name = receiveMsg(handler);

            var cliente = new Client(name, handler);
            clients.Add(cliente);

            sendToAll(cliente, $"{cliente.ToString()}: is connected \\(^O^)/");

            try
            {

                while (true)
                {

                    string data = receiveMsg(handler);

                    if (data.StartsWith("#MSG#"))
                    {
                        sendToAll(cliente, $"{cliente.ToString()} said: {data.Replace("#MSG#", string.Empty)}");

                    }
                    else if (data.StartsWith("#MSGTO|"))
                    {
                        sentToAnotherClient(cliente, data);

                    }
                    else if (data == "#LIST#")
                    {
                        sendListClient(cliente);
                    }
                    else if (data == "#STOP#" || data == "")
                    {
                        break;
                    }
                    else
                    {
                        sendMsg(handler, "Server: Comando non riconosciuto! Riprova");
                    }

                    //if (data == "#STOP#" || bytesRec == 0)
                }
            }
            catch (Exception)
            {

            }

            sendToAll(cliente, $"{cliente.ToString()}: has left \\(T_T)/");
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
            clients.Remove(cliente);
        }

        private static void sendListClient(Client cliente)
        {
            var nameList = clients.Select(m => m.Nome).ToList();
            nameList.Remove(cliente.Nome);

            var serialize = JsonSerializer.Serialize(nameList);

            sendMsg(cliente.handler, $"#LIST#{serialize}");

            Console.WriteLine($"Lista clienti mandata a {cliente.Nome}");
        }

        private static void sentToAnotherClient(Client cliente, string data)
        {
            var split = data.Split('#', '|');
            foreach (var item in clients)
            {
                if (item.Nome == split[2])
                {
                    sendMsg(item.handler, $"{cliente.Nome}(PRIVATO) send to you: {split[3]}");

                    return;
                }
            }
            sendMsg(cliente.handler, "Server: Cliente non trovato. Ricordiamo che la formattazione deve essere #MSGTO|nomeCliente|messaggio#");
        }

        private static void sendMsg(Socket handler, string message)
        {
            try
            {
                var cryptedData = Crypto.Encrypt(message);
                byte[] msg = Encoding.ASCII.GetBytes(cryptedData);
                handler.Send(msg);
            }
            catch (Exception)
            {

            }

        }

        public static string receiveMsg(Socket handler)
        {
            string decryptedData = "";
            try
            {
                byte[] bytes = new byte[1024];
                int bytesRec = handler.Receive(bytes);
                string data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                decryptedData = Crypto.Decrypt(data);
            }
            catch (Exception)
            {

            }
            return decryptedData;
        }

        private static void sendToAll(Client cliente, string data)
        {
            Console.WriteLine(data);

            var cryptedData = Crypto.Encrypt(data);
            byte[] msg = Encoding.ASCII.GetBytes(cryptedData);

            try
            {
                foreach (var item in clients)
                {
                    if (item.handler != cliente.handler)
                    {
                        item.handler.Send(msg);
                    }

                }
            }
            catch (Exception)
            {
            }

        }
    }

}