using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Client
{
    internal class Program
    {
        public static void Main(String[] args)
        {
            StartClient();
        }

        public static void StartClient()
        {
            byte[] bytes = new byte[1024];

            try
            {
                //IPHostEntry host = Dns.GetHostEntry("localhost");
                //IPAddress ipAddress = host.AddressList[0];
                IPAddress ipAddress = IPAddress.Parse("10.100.0.126");
                //IPAddress ipAddress = IPAddress.Parse("10.100.0.188");
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

                    SendMsgToClient(sender, Domanda("Inserisci il nome"));

                    Thread send = new Thread(h => SendMsg((Socket)h));
                    send.Start(sender);

                    Thread receive = new Thread(h => ReceiveMsg((Socket)h));
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

        private static void SendMsg(Socket sender)
        {
            while (true)
            {
                try
                {
                    string messaggio = Domanda("Scrivi messaggio da inviare(<STOP> per fermare la comunicazione): ");

                    //// Encode the data string into a byte array
                    //byte[] msg = Encoding.ASCII.GetBytes(messaggio);

                    //// Send the data through the socket.
                    //int bytesSent = sender.Send(msg);
                    SendMsgToClient(sender, messaggio);

                    //Stop communication when send string <STOP>
                    if (messaggio == "<STOP>")
                        break;
                }
                catch (Exception)
                {
                    break;
                }

            }
            Console.WriteLine("comunicazione chiusa");
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        private static void SendMsgToClient(Socket sender, string messaggio)
        {
            // Encode the data string into a byte array
            byte[] msg = Encoding.ASCII.GetBytes(messaggio);

            // Send the data through the socket.
            int bytesSent = sender.Send(msg);
        }

        private static void ReceiveMsg(Socket sender)
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
                    Console.WriteLine(Encoding.ASCII.GetString(bytes, 0, bytesRec));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }

            }
        }

        public static string Domanda(string msg)
        {
            Console.WriteLine(msg);
            return Console.ReadLine();
        }
    }
}