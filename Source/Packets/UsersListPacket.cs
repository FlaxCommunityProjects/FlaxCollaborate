using System.Collections.Generic;
using System.IO;
using FlaxEditor;
using FlaxEditor.SceneGraph;
using FlaxEngine;
using MultiUsersEditing.Source.MultiUsersEditingPlugin;

namespace CollaboratePlugin
{
    public class UsersListPacket : Packet
    {
        public List<EditingUser> UsersList;
        
        public UsersListPacket()
        {
            
        }
        
        public UsersListPacket(List<EditingUser> usersList)
        {
            UsersList = usersList;
        }
        
        public override void Read(BinaryReader bs)
        {
            EditingSessionPlugin.Instance.Session.Users.Clear();
            int count = bs.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string suser = bs.ReadString();
                var user = FlaxEngine.Json.JsonSerializer.Deserialize<EditingUser>(suser);
                EditingSessionPlugin.Instance.Session.Users.Add(user);
                
                Scripting.InvokeOnUpdate(() =>
                {
                    user.Outliner = Object.New<CustomOutliner>();
                    user.Outliner.UserId = user.Id;
                });
            }
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(UsersList.Count);
            UsersList.ForEach((user) =>
            {
                string data = FlaxEngine.Json.JsonSerializer.Serialize(user);
                bw.Write(data);
            });
        }
    }
}