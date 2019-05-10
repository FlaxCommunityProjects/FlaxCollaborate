using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CollaboratePlugin;
using FlaxEditor.SceneGraph;
using FlaxEngine.Utilities;

namespace CollaboratePlugin
{
    public abstract class EditingSession
    {
        public EditingUser User { get; protected set; }

        private List<EditingUser> _users = new List<EditingUser>();

        protected readonly LinkedList<Packet> _backlog = new LinkedList<Packet>();

        public ReadOnlyCollection<EditingUser> Users
        {
            get { return _users.AsReadOnly(); }
        }

        public SessionSettings Settings { get; protected set; }

        public abstract bool IsHosting { get; }

        public abstract Task<bool> Start(SessionSettings settings);

        public abstract bool SendPacket(Packet packet);

        // TODO: IDisposeable?
        public abstract void Close();

        protected void ExecuteBacklog()
        {
            foreach (var packet in _backlog)
            {
                packet.Execute();
            }
            _backlog.Clear();
        }

        public EditingUser GetUserById(int id)
        {
            try
            {
                return Users.First((user) => user.Id == id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool CanSelect(Guid id)
        {
            bool match = true;
            Users.ForEach((user) =>
            {
                if (user.Id != User.Id && user.Selection != null)
                    foreach (var sceneGraphNode in user.Selection)
                    {
                        if (sceneGraphNode.ID.Equals(id))
                            match = false;
                    }
            });

            return match;
        }

        public void RemoveUser(int id)
        {
            var u = GetUserById(id);
            u.Close();
            _users.Remove(u);
        }

        public void RemoveUser(EditingUser user)
        {
            _users.Remove(user);
        }

        public void AddUser(EditingUser u)
        {
            _users.Add(u);
        }

        public void UpdateUsers(List<EditingUser> users)
        {
            _users = users;
        }
    }
}