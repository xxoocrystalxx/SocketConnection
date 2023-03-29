using System.Net.Sockets;

namespace Server
{
    internal class Client
    {
        public string Nome { get; set; }
        public string IP { get; set; }
        public Socket handler { get; set; }

        public Client(string nome, Socket handler)
        {
            Nome = nome;
            IP = handler.RemoteEndPoint.ToString();

            this.handler = handler;
        }

        public override string ToString() {
            return $"{Nome}({IP})";
        }
    }
}
