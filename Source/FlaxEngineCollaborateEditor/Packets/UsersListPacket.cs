using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

        public UsersListPacket(ReadOnlyCollection<EditingUser> usersList)
        {
            UsersList = usersList.AsEnumerable().ToList();
        }

        public override void Read(BinaryReader bs)
        {
            UsersList = new List<EditingUser>();
            int count = bs.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string suser = bs.ReadString();
                var user = FlaxEngine.Json.JsonSerializer.Deserialize<EditingUser>(suser);
                UsersList.Add(user);
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

        public override void Execute()
        {
            Debug.Log("users count : " + UsersList.Count);
            EditingSessionPlugin.Instance.Session.ClearUsers();
            foreach (var user in UsersList)
            {
                
                EditingSessionPlugin.Instance.Session.AddUser(user);

                user.Outliner = Object.New<CustomOutliner>();
                user.Outliner.UserId = user.Id;
            }
            
            EditingSessionPlugin.Instance.CollaborateWindow.Rebuild();
        }
    }
}