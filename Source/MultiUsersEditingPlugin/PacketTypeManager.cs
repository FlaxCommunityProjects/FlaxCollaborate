using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MultiUsersEditingPlugin
{
    public class PacketTypeManager
    {
        public static IEnumerable<Type> SubclassTypes = Assembly
            .GetAssembly(typeof(Packet))
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Packet)));
    }
}