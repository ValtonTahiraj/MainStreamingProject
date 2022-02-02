
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    public class Server : BaseThread
    {
        public Server()
        {
            this.Start();
        }
        public override void RunThread()
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


    public class Partecipant
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

        public override string ToString()
        {
            return "username: " + username + ", id : " + id;
        }


    }

    public class Message
    {
        private Partecipant partecipant;
        private string message;

        public Message(Partecipant partecipant, string message)
        {
            this.partecipant = partecipant;
            this.message = message;
        }

        public Partecipant getPartecipant()
        {
            return partecipant;
        }

        public string getMessage()
        {
            return message;
        }

        public override string ToString()
        {
            return partecipant.getUsername() + " : " + message;
        }
    }

    public abstract class BaseThread
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

     public class Listener : BaseThread
     {
        private TcpClient client;
        public static List<Partecipant> connectedUsers = new List<Partecipant>();
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
                byte[] msg = System.Text.Encoding.ASCII.GetBytes("What username do you want to use?");
                // Send back a response.
                stream.Write(msg, 0, msg.Length);
                //Console.WriteLine("Sent: {0} Username invalid");
                Byte[] bytes = new Byte[256];
                string receivedMessage = null;
                int j = stream.Read(bytes, 0, bytes.Length);
                // Translate data bytes to a ASCII string.
                string username = System.Text.Encoding.ASCII.GetString(bytes, 0, j);
                Console.WriteLine("Received: {0}", receivedMessage);

                if (userIsConnected(username))
                {
                    msg = System.Text.Encoding.ASCII.GetBytes("There is already a user with that username, connection closed. /exit");
                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                    //Console.WriteLine("Sent: {0} Username invalid");
                }
                else
                {
                    Partecipant newUser = new Partecipant(username, client, id);
                    Console.WriteLine("User " + username + " connected with id " + id);
                    lock (connectedUsers)
                    {
                        connectedUsers.Add(newUser);
                    }
                    
                    while (true)
                    {
                        j = stream.Read(bytes, 0, bytes.Length);
                        // Loop to receive all the data sent by the client.
                        // Translate data bytes to a ASCII string.
                        receivedMessage = System.Text.Encoding.ASCII.GetString(bytes, 0, j);
                        if (receivedMessage.Contains("/exit"))
                        {
                            break;
                        }
                        new Transmitter(receivedMessage, newUser, true);
                    }
                    lock (connectedUsers)
                    {
                        removeClient(client);
                    }
                    msg = System.Text.Encoding.ASCII.GetBytes("/exit");
                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                }
            } catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());

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
            private Partecipant user;
            private Boolean broadcast;
            public static List<Message> messages = new List<Message>();
            public Transmitter(String message, Partecipant user, Boolean broadcast) : base()
            {
                this.user = user;
                this.stream = user.getClient().GetStream();
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
                        string toSend = user.getUsername() + ": " + message;
                        sendBroadcast(toSend, stream);
                        lock (messages)
                        {
                            messages.Add(new Message(user,message));
                        }
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
