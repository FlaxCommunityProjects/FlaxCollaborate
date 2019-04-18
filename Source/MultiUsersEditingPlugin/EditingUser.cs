using System;
using FlaxEditor.SceneGraph;

namespace MultiUsersEditingPlugin
{
    public struct EditingUser
    {
        public int Id;

        public SceneGraphNode[] Selection
        {
            set
            {
                // Update selection outline
            }

            get { return Selection; }
        }

    }
}