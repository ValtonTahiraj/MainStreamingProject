using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static ChatServer.Listener;

namespace ChatServer
{
    internal class Server
    {
        // Creating the list of partecipants
        static List<String> partecipants = new List<String>();
        static void Main(string[] args)
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();
                int id = 0;

                // Buffer for reading data

                // Enter the listening loop.
                while (true)
                {
                    Console.WriteLine("Waiting for a connection... ");
                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");
                    new Listener(client,id);
                    id++;

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
        private TcpClient client;
        private Partecipant partecipant;
        private static List<Partecipant> connectedUsers = new List<Partecipant>();
        private int id;
        private NetworkStream stream;
        
        public Listener(TcpClient client,int id) : base()
        {
            this.client = client;
            stream = client.GetStream();
            this.id = id;
            this.Start();
        }

        public override void RunThread()
        {
            try
            {   
                // Buffer for reading data
                new Transmitter("Quale username vorresti usare?",stream,false);
                Byte[] bytes = new Byte[256];
                string receivedMessage = null;
                int j = stream.Read(bytes, 0, bytes.Length);
                // Translate data bytes to a ASCII string.
                string username = System.Text.Encoding.ASCII.GetString(bytes, 0, j);
                Console.WriteLine("Received: {0}", receivedMessage);

                if (userIsConnected(username))
                {
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes("There is already a user with that username, connection closed. /exit");
                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                    //Console.WriteLine("Sent: {0} Username invalid");
                }
                else
                {
                    
                    Console.WriteLine("User " + username + " connected with id " + id);
                    lock (connectedUsers)
                    {
                        connectedUsers.Add(new Partecipant(username, client, id));
                    }
                    
                    while (true)
                    {
                        j = stream.Read(bytes, 0, bytes.Length);
                        // Loop to receive all the data sent by the client.
                        // Translate data bytes to a ASCII string.
                        receivedMessage = System.Text.Encoding.ASCII.GetString(bytes, 0, j);
                        if (receivedMessage.Contains("/exit"))
                        {
                            new Transmitter("/exit", stream, false);
                            break;
                        }
                        Console.WriteLine("Received: {0}", receivedMessage);
                        // Process the data sent by the client.
                        string toSend = username + ": " + receivedMessage;
                        Console.WriteLine(toSend);
                        new Transmitter(toSend, stream, true);

                    }
                    lock (connectedUsers)
                    {
                        removeClient(client);
                    }
                }
            } catch(Exception ex)
            {
                Console.WriteLine("Errore");

            }
            finally
            {
             

                stream.Close();
                client.Close();

            }


            //Method that checks if a user is currently connected with that username
            static Boolean userIsConnected(string username)
            {
                Boolean found = false;
                foreach (Partecipant p in connectedUsers)
                {
                    if (p.getUsername().Equals(username))
                    {
                        found = true;
                        break;
                    }
                }
                return found;
            }

            static void removeUser(string username)
            {
                foreach(Partecipant p in connectedUsers)
                {
                    if (p.getUsername() == username)
                    { 
                      connectedUsers.Remove(p);
                      break;
                    }
                }
            }

            static void removeClient(TcpClient client)
            {
                foreach (Partecipant p in connectedUsers)
                {
                    if (p.getClient() == client)
                    {
                        connectedUsers.Remove(p);
                        break;
                    }
                }
            }

        }


        internal class Transmitter : BaseThread
        {
            private string message;
            private NetworkStream stream;
            private Boolean broadcast;
            public Transmitter(String message, NetworkStream socket, Boolean broadcast) : base()
            {

                this.stream = socket;
                this.message = message;
                this.broadcast = broadcast;
                this.Start();
            }

            public override void RunThread()
            {
                try
                {
                    if (broadcast)
                    {
                        sendBroadcast(message, stream);
                    }
                    else 
                    { 
                        sendMessage(message, stream);   
                    }
                }
                catch (IOException e)
                {
                    Console.Write(e.ToString());
                }


            }
        }

            static void sendMessage(string message, NetworkStream stream)
         {
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(message);
            // Send back a response.
            stream.Write(msg, 0, msg.Length);
        }

        static void sendBroadcast(string message, NetworkStream stream)
        {
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
