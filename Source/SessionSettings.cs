using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;

namespace CollaboratePlugin
{
    public class SessionSettings
    {
        public string Username { get; set; } = "Client";

        public Color SelectionColor { get; set; } = new Color(1);
        
        public string Host { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 25874;
    }

    public class ServerSessionSettings : SessionSettings
    {
        public ServerSessionSettings()
        {
            Host = "localhost";
            Username = "Host";
        }
    }
}