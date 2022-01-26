using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ChatServer
{
    internal class Server
    {
        // Creating the list of partecipants
        static List<String> partecipants = new List<String>();
        static void Main(string[] args)
        {
            TcpListener server = null;
            int i = 0;
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String username = null;


                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine(client.Client.RemoteEndPoint.AddressFamily.ToString(),
                                      "Connected!");

                    username = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int j;

                    // Loop to receive all the data sent by the client.
                    while ((j = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        username = System.Text.Encoding.ASCII.GetString(bytes, 0, j);
                        Console.WriteLine("Received: {0}", username);

                        // Process the data sent by the client.
                        username = username.ToUpper();

                        if(userIsConnected(username))
                        {
                            byte[] msg = System.Text.Encoding.ASCII.GetBytes("There is already a user with that username, connection closed.");

                            // Send back a response.
                            stream.Write(msg, 0, msg.Length);
                            Console.WriteLine("Sent: {0}", username);
                            client.Close();
                        }
                        else 
                        {
                            ++i;
                            new Listener(new Partecipant(username, client, i));

                        }

                        
                    }

                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.

                // Shutdown and end connection
                server.Stop();
            }

            //Console.WriteLine("\nHit enter to continue...");
            //Console.Read();

        }

        
        //Method that checks if a user is currently connected with that username
        static Boolean userIsConnected(string username)
        {
            Boolean found = false;
            foreach(String p in partecipants)
            {
                if(p.Equals(username))
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
    }


    internal class Partecipant
    {
        private string username;
        private TcpClient client;
        private int id;

        public Partecipant(string username, TcpClient client, int id)
        {

            this.username = username;
            this.client = client;
            this.id = id;

        }

        public string getUsername()
        {
            return username;
        }

        public TcpClient getClient()
        {
            return client;
        }

        public int getId()
        {
            return id;  
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

    internal class  Listener : BaseThread
    {
        private Partecipant clientInfo;
        private static List<Partecipant> connectedUsers = new List<Partecipant>();
        private NetworkStream stream;
        
        public Listener(Partecipant p) : base()
        {
            clientInfo = p;
            stream = p.getClient().GetStream();
            lock (connectedUsers)
            {
                connectedUsers.Add(p);
            }
            sendMessage(("You succesfully joined the chat, you can now chat."),stream);
            RunThread();


        }

        public override void RunThread()
        {
            try
            {
                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                string receivedMessage = null;

                while (true)
                {
                    int j;
                    // Loop to receive all the data sent by the client.
                    while ((j = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        receivedMessage = System.Text.Encoding.ASCII.GetString(bytes, 0, j);
                        Console.WriteLine("Received: {0}", receivedMessage);
                        // Process the data sent by the client.
                        receivedMessage = receivedMessage.ToUpper();
                        string toSend = clientInfo.getUsername() + ": " + receivedMessage;
                        sendBroadcast(toSend, stream);
                    }

                }

                clientInfo.getClient().Close();
                lock (connectedUsers)
                {
                    connectedUsers.Remove(clientInfo);
                }
            } catch(Exception ex)
            {
                Console.WriteLine("Errore");
                clientInfo.getClient().Close();
                lock (connectedUsers)
                {
                    connectedUsers.Remove(clientInfo);
                }
            }
            
        }



        static void sendMessage(string message, NetworkStream stream)
        {
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(message);

            // Send back a response.
            stream.Write(msg, 0, msg.Length);
            Console.WriteLine("Sent: {0}", stream.ToString());

        }

        static void sendBroadcast(string message, NetworkStream stream)
        {
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(message);

            foreach (Partecipant p in connectedUsers)
            {
                NetworkStream recipient = p.getClient().GetStream();    
                if(recipient != stream)
                {
                    sendMessage(message, recipient);         
                }
            } 

        }
    }
    

}
