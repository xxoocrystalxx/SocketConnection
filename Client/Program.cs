using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using CryptoHelper;

namespace Client
{
    internal class Program
    {
        public static List<string> clientList = new List<string>();
        public static string nome;
        public static bool isConnected = false;
        public static void Main(String[] args)
        {
            StartClient();

        }

        public static void StartClient()
        {
            byte[] bytes = new byte[1024];

            try
            {
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                //IPAddress ipAddress = IPAddress.Parse("10.100.0.137");
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                // Create a TCP/IP  socket.
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.

                try
                {
                    // Connect to Remote EndPoint
                    sender.Connect(remoteEP);
                    Console.WriteLine($"Socket connected to {sender.RemoteEndPoint}");

                    nome = Domanda("Inserisci il nome");
                    SendMsgToClient(sender, nome);

                    Thread send = new Thread(h => WorkerSender((Socket)h));
                    send.Start(sender);

                    Thread receive = new Thread(h => WorkerReceiver((Socket)h));
                    receive.Start(sender);


                }
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

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void WorkerSender(Socket sender)
        {
            bool stop = true;
            printMenu();
            while (stop && sender.Connected)
            {
                try
                {
                    stop = ElaboraScelta(sender);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }

            }
            Console.WriteLine("comunicazione chiusa");
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        private static bool ElaboraScelta(Socket sender)
        {

            string scelta = Domanda("Seleziona funzione: ");
            switch (scelta)
            {
                case "1":
                    SendMsgToClient(sender, "#MSGt#" + Domanda("Scrivi messaggio da mandare: "));
                    break;

                case "2":
                    sendMstToAnotherClient(sender);
                    break;

                case "3":
                    SendMsgToClient(sender, "#LIST#");
                    break;

                case "x":
                    SendMsgToClient(sender, "#STOP#");
                    return false;
                default:
                    Console.WriteLine("comando non riconosciuto, riprova");
                    break;
            }
            return true;
        }

        private static void sendMstToAnotherClient(Socket sender)
        {
            SendMsgToClient(sender, "#LIST#");
            Thread.Sleep(200);

            if (clientList.Count == 0)
            {
                Console.WriteLine("Non ci sono altri utenti");
                return;
            }
            int num;
            while (true)
            {
                try
                {
                    num = Convert.ToInt32(Domanda("Scegli l'utente: "));
                    if (num >= 0 && num < clientList.Count)
                        break;
                }
                catch (Exception)
                {


                }
                Console.WriteLine("Seleziona tra i numeri mostrati, riprova.");
            }

            SendMsgToClient(sender, $"#MSGTO|{clientList[num]}|{Domanda("Scrivi il messaggio da mandare: ")}");
        }

        public static void printMenu()
        {
            Console.Clear(); //pulisco schermo
            Console.WriteLine("\tCiao\t" + nome + "\n\n");
            Console.WriteLine("\tMenu\t\n");
            Console.WriteLine("1\tInvio messaggio a tutti\n");
            Console.WriteLine("2\tInvia messaggio a un client\n");
            Console.WriteLine("3\tLista Client\n");
            Console.WriteLine("x\tChiudi\n");
            Console.WriteLine("Selezionare funzione fra quelle indicate: ");
        }

        private static void SendMsgToClient(Socket sender, string messaggio)
        {
            try
            {
                var crypted = Crypto.Encrypt(messaggio);
                // Encode the data string into a byte array
                byte[] msg = Encoding.ASCII.GetBytes(crypted);


                // Send the data through the socket.
                int bytesSent = sender.Send(msg);
            }
            catch (Exception)
            {
            }

        }

        private static void WorkerReceiver(Socket sender)
        {

            while (true)
            {
                try
                {
                    byte[] bytes = new byte[1024];
                    int bytesRec = sender.Receive(bytes);
                    //host remoto arresta la connessione con shutdown e tutti i dati sono disponibili sono ricevuti
                    //sender.recevice restituirà 0      
                    if (bytesRec == 0) break;

                    string data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    data = Crypto.Decrypt(data);

                    if (data.StartsWith("#LIST#"))
                    {
                        stampaLista(data);
                    }
                    else
                    {
                        Console.WriteLine(data);
                    }

                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.Message);
                    break;
                }

            }
        }

        private static void stampaLista(string data)
        {
            clientList = JsonSerializer.Deserialize<List<string>>(data.Replace("#LIST#", string.Empty));
            Console.WriteLine();

            for (int i = 0; i < clientList.Count; i++)
            {
                Console.WriteLine(i + ") " + clientList[i]);
            }
        }

        public static string Domanda(string msg)
        {
            Console.WriteLine(msg);
            return Console.ReadLine();
        }
    }
}