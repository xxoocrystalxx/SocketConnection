using System.Net.Sockets;
using System.Net;
using System.Text;


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
               
               while(true)
                { 
                    byte[] bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    string data = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    //Stop communication when receives string <STOP> or cliente close connection
                    if (data == "<STOP>" || bytesRec==0)
                    {
                        break;
                    }
                  
                    sendToAll(cliente, $"{cliente.ToString()} said: {data}");

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
            foreach (var item in clients)
            {
                if (item.handler != cliente.handler)
                {
                    item.handler.Send(msg);
                }

            }
        }
    }

}