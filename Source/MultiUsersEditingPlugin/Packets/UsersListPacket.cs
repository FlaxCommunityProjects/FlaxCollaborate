using System.Collections.Generic;
using System.IO;
using FlaxEditor;
using FlaxEditor.SceneGraph;
using FlaxEngine;
using MultiUsersEditing.Source.MultiUsersEditingPlugin;

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
            UsersList = usersList;
        }
        
        public override void Read(BinaryReader bs)
        {
            EditingSessionPlugin.Instance.Session.Users.ForEach((user) =>
                {
                    Editor.Instance.Windows.EditWin.Viewport.Task.CustomPostFx.Remove(user.Outline);
                });
            
            EditingSessionPlugin.Instance.Session.Users.Clear();
            int count = bs.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string suser = bs.ReadString();
                var user = FlaxEngine.Json.JsonSerializer.Deserialize<EditingUser>(suser);
                EditingSessionPlugin.Instance.Session.Users.Add(user);
                
                Scripting.InvokeOnUpdate(() =>
                {
                    user.Outline = FlaxEngine.Object.New<CustomOutliner>();
                    user.Outline.UserId = user.Id;
                    Editor.Instance.Windows.EditWin.Viewport.Task.CustomPostFx.Add(user.Outline);
                });
            }


            /* 

            for (int i = 0; i < count; i++)
            {
                var id = bs.ReadInt32();
                var name = bs.ReadString();
                Color color = new Color();
                color.R = bs.ReadSingle();
                color.G = bs.ReadSingle();
                color.B = bs.ReadSingle();
                bool isServer = bs.ReadBoolean();
                
                SceneGraphNode[] selec = FlaxEngine.Json.JsonSerializer.Deserialize<SceneGraphNode[]>(bs.ReadString());

                EditingUser user = new EditingUser(id, name, color, isServer);
                user.Selection = selec;
                
                EditingSessionPlugin.Instance.Session.Users.Add(user);
            }*/
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(UsersList.Count);
            UsersList.ForEach((user) =>
            {
                string data = FlaxEngine.Json.JsonSerializer.Serialize(user);
                bw.Write(data);
            });
            
            /*
            UsersList.ForEach((user) =>
            {
                bw.Write(user.Id);
                bw.Write(user.Name);
                bw.Write(user.SelectionColor.R);
                bw.Write(user.SelectionColor.G);
                bw.Write(user.SelectionColor.B);
                bw.Write(user.IsServer);


                string data = FlaxEngine.Json.JsonSerializer.Serialize(user.Selection);
                bw.Write(data);
            });*/
        }
    }
}