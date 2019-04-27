using System.Collections.Generic;
using System.IO;

namespace MultiUsersEditingPlugin
{
    public class UsersListPacket : Packet
    {
        public List<EditingUser> UsersList;
        
        public UsersListPacket()
        {
            
        }
        
        public UsersListPacket(List<EditingUser> usersList)
        {
            
        }
        
        public override void Read(BinaryReader bs)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(BinaryWriter bw)
        {
            UsersList.ForEach((user) =>
            {
                
            });
        }
    }
}