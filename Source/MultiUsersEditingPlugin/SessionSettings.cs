using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;

namespace MultiUsersEditingPlugin
{
    public class SessionSettings
    {
        public String Username { get; set;  }

        public Color SelectionColor { get; set; }
        
        public virtual string Host { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 25874;
    }

    public class ServerSessionSettings : SessionSettings
    {
        [HideInEditor]
        public override string Host { get => base.Host; set => base.Host = value; }

        public ServerSessionSettings()
        {
            Host = "localhost";
        }
    }
}