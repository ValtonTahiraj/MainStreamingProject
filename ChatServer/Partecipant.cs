using System;
using System.Net.Sockets;

public class Partecipant
{
	private string username;
	private TcpClient data;

	public Partecipant(string username, TcpClient data){

		this.username = username;
		this.data = data;

	}

	public string getUsername()
    {
		return username;
    }

	public TcpClient getData()
    {
		return data;
    }


}
