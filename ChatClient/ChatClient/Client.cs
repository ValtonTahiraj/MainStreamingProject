using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace ChatClient
{
    internal class Client
    {
        static void Main(string[] args)
        {
            Client.Connect("127.0.0.1");
        }

        static void Connect(String server)
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = 13000;
                TcpClient client = new TcpClient(server, port);
                NetworkStream stream = client.GetStream();
                new Listener(client);
                string username = Console.ReadLine();
                // Translate the passed message into ASCII and store it as a Byte array.
                // Get a client stream for reading and writing.
                new Transmitter(username, stream);
                string toSend = "";
                while (true)
                {
                    Console.Write(username+": ");
                    toSend = Console.ReadLine();
                    new Transmitter(toSend, stream);
                }
                Console.WriteLine("Connessione interrotta");
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    abstract class BaseThread
    {
        private Thread _thread;

        protected BaseThread()
        {
            _thread = new Thread(new ThreadStart(this.RunThread));
        }

        // Thread methods / properties
        public void Start() => _thread.Start();
        public void Join() => _thread.Join();
        public bool IsAlive => _thread.IsAlive;

        // Override in base class
        public abstract void RunThread();
    }

    internal class Listener : BaseThread
    {

        private NetworkStream stream;
        private TcpClient socket;
        public Listener(TcpClient socket) : base()
        {
            this.socket = socket;
            this.stream = socket.GetStream();
            this.Start();
        }

        public override void RunThread()
        {
            try
            {
                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                string receivedMessage = "";

                while (!receivedMessage.Contains("/exit"))
                {
                    int j;
                    // Loop to receive all the data sent by the client.
                    while ((j = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        receivedMessage = System.Text.Encoding.ASCII.GetString(bytes, 0, j);
                        Console.WriteLine("\n"+receivedMessage);
                        
                    }             

                }
                Console.WriteLine("Connessione interrotta");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                stream.Close();
                socket.Close();
                Environment.Exit(0);
            }

        }

    }
    internal class Transmitter : BaseThread
    {
        private string message;
        private NetworkStream stream;
        public Transmitter(String message, NetworkStream socket) : base()
        {

            this.stream = socket;
            this.message = message;
            this.Start();
        }

        public override void RunThread()
        {
            try
            {
                sendMessage(message, stream);
            }
            catch (Exception e)
            {
                //Da migliorare
                Console.WriteLine(e.ToString());
                Environment.Exit(0);
            }

        }
        static void sendMessage(string message, NetworkStream stream)
        {

            byte[] msg = System.Text.Encoding.ASCII.GetBytes(message);
            // Send back a response.
            stream.Write(msg, 0, msg.Length);
        }
    }

}














