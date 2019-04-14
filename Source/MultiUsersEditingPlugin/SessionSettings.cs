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
		public virtual string Host { get; set; }

		public int Port { get; set; }
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