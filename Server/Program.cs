using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.ComponentModel;

namespace Server
{
    internal class Program
    {
        public static List<Client> clients = new List<Client>();

        public static void Main(String[] args)
        {
            StartServer();
            //var data = "#MSG|ip#";
            //var split = data.Split('#', '|');
            //Console.WriteLine(split[2]);
            //var serialize = JsonSerializer.Serialize(lista);
            //Console.WriteLine(serialize);
            //var des = JsonSerializer.Deserialize<List<string>>(serialize);
            //foreach (var item in des)
            //{
            //    Console.WriteLine(item);
            //}
            //var c1 = new Client("mario", "11.3.44.3");
            //var c2 = new Client("luigi", "11.3.232423.3");
            //var c3 = new Client("luca", "1r444.3.44.3");
            //var clienti = new List<Client>();
            //clienti.Add(c1); clienti.Add(c2);
            //    clienti.Add(c3);
            //List<string> managerList = clienti.Select(m => m.Nome).ToList();
            //foreach (var item in managerList)
            //{
            //    Console.WriteLine(item);
            //}
        }

        public static void StartServer()
        {
            // Get Host IP Address that is used to establish a connection
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1
            // If a host has multiple addresses, you will get a list of addresses
            //IPHostEntry host = Dns.GetHostEntry("localhost");
            //IPAddress ipAddress = host.AddressList[0];
            IPAddress ipAddress = IPAddress.Parse("10.100.0.126");
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

                    // Wait for the worker thread to finish
                    //thread.Join();

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

            string name = getClientName(handler);

            var cliente = new Client(name, handler);
            clients.Add(cliente);

            sendToAll(cliente, $"{cliente.ToString()}: is connected \\(^O^)/");

            try
            {

                while (true)
                {
                    byte[] bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    string data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    Console.WriteLine($"msg ricevuto: {data}");
                    switch (data)
                    {
                        case string s when s.StartsWith("#MSG|"):
                            var split = data.Split('#', '|');
                            sendToAll(cliente, $"{cliente.ToString()} said: {split[2]}");
                            break;
                        case string s when s.StartsWith("#MSGTO|"):
                            sentToAnotherClient(cliente, data);
                            break;
                        case "#LIST#":
                            sendListClient(cliente);
                            break;
                        default:
                            break;
                    }

                    //Stop communication when receives string <STOP> or cliente close connection
                    if (data == "#STOP#" || bytesRec == 0)
                    {
                        break;
                    }

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
            byte[] msg = Encoding.ASCII.GetBytes($"#LIST#{serialize}");
            cliente.handler.Send(msg);

            Console.WriteLine($"Lista clienti mandata a ${cliente.Nome}");
        }

        private static void sentToAnotherClient(Client cliente, string data)
        {
            var split = data.Split('#', '|');
            foreach (var item in clients)
            {
                if (item.IP == split[2])
                {
                    byte[] msg = Encoding.ASCII.GetBytes($"{cliente.ToString()} send to you: {split[3]}");
                    cliente.handler.Send(msg);
                    return;
                }
            }
        }

        private static string getClientName(Socket handler)
        {
            string data = "";
            try
            {
                byte[] bytes = new byte[1024];
                int bytesRec = handler.Receive(bytes);
                data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            }
            catch (Exception)
            {

            }
            return data;
        }

        private static void sendToAll(Client cliente, string data)
        {
            Console.WriteLine(data);

            byte[] msg = Encoding.ASCII.GetBytes(data);
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