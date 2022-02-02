using Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Server.Listener;

namespace Test1.Controllers
{
    public class ServerController : Controller
    {
        private readonly ILogger<ServerController> _logger;

        public ServerController(ILogger<ServerController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        [Route("users")]
        public IActionResult getUsers()
        {
            List<Partecipant> toProcess = Listener.connectedUsers;
            string toShow = "";
            foreach ( Partecipant p in toProcess)
            {
                toShow += p.ToString() + "\n";
            }
            return Ok(toShow);
        }
        [HttpGet]
        [Route("messages")]
        public IActionResult getMessages()
        {
            List<Message> toProcess = Transmitter.messages;
            string toShow = "";
            foreach (Message p in toProcess)
            {
                toShow += p.ToString() + "\n";
            }
            return Ok(toShow);
        }
        [HttpGet]
        [Route("messages/{id}")]
        public IActionResult getMessagesById(int id)
        {
            int numMessages = 0;
            List<Message> toProcess = Transmitter.messages;
            foreach (Message p in toProcess)
            {
                if (p.getPartecipant().getId() == id)
                {
                    ++numMessages;
                }
            }
            return Ok(numMessages);
        } 
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

    }


}
