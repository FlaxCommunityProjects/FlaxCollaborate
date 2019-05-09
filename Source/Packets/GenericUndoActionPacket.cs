using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FlaxEditor;
using FlaxEditor.Actions;
using FlaxEditor.History;
using FlaxEditor.Modules;
using FlaxEditor.SceneGraph;
using FlaxEditor.Utilities;
using FlaxEngine;

namespace CollaboratePlugin
{
    internal class GenericUndoActionPacket : Packet
    {
        private readonly string ObjectDiffPacket = "ObjectDiffPacket";

        private IUndoAction _action;

        public GenericUndoActionPacket()
        {
        }

        public GenericUndoActionPacket(IUndoAction action)
        {
            this._action = action;
            IsBroadcasted = true;
        }

        /*
         * DATA LAYOUT
         *
         * is multi undo action = true
         * actions count
         * undo action
         * undo action
         * ...
         *
         * is multi undo action = false
         * undo action
         */

        public override void Read(BinaryReader bs)
        {
            _action = ReadAction(bs);
        }

        /// <summary>
        /// Recursively reads the <see cref="IUndoAction"/>s
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        private IUndoAction ReadAction(BinaryReader bs)
        {
            if (bs.ReadBoolean())
            {
                // is MultiUndoAction
                int count = bs.ReadInt32();
                IUndoAction[] actions = new IUndoAction[count];

                for (int i = 0; i < count; i++)
                {
                    actions[i] = ReadAction(bs);
                }

                return new MultiUndoAction(actions);
            }
            else
            {
                // just a simple IUndoAction
                return ReadSimpleAction(bs);
            }
        }

        /// <summary>
        /// Reads a simple action. It will never be a <see cref="MultiUndoAction"/>
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        private IUndoAction ReadSimpleAction(BinaryReader bs)
        {
            string typeName = bs.ReadString();
            string data = bs.ReadString();
            //Debug.Log(data);

            // Deserialize the undo action
            IUndoAction undoAction = DeserializeUndoAction(typeName, data);
            if (undoAction == null)
            {
                Debug.LogError($"UndoAction is null. Deserialisation failed for {typeName} - {data}");
            }

            // Selection change actions need special handling as well
            if (undoAction is SelectionChangeAction selectionChangeAction)
            {
                // Don't execute the action, instead do stuff
                // TODO: What if some action depends on the current selection?
                EditingSessionPlugin.Instance.Session.GetUserById(Author).Selection =
                    selectionChangeAction.Data.After;

                return selectionChangeAction;
            }
            else
            {
                // Execute the undo action
                try
                {
                    undoAction.Do();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                return undoAction;
            }
        }

        public IUndoAction DeserializeUndoAction(string typeName, string data)
        {
            // Handle object modifications, special case
            if (typeName == this.ObjectDiffPacket)
            {
                return UndoActionObjectSerializer.FromJson(data);
            }

            // Otherwise, generically deserialize it
            object undoAction =
                    System.Runtime.Serialization.FormatterServices.GetUninitializedObject(Type.GetType(typeName));

            FlaxEngine.Json.JsonSerializer.Deserialize(undoAction, data);

            // Special deserialisation cases for a few undo actions
            if (undoAction is DeleteActorsAction deleteAction)
            {
                // Deleting
                // Get the actor Guids
                FieldInfo actorDataField = typeof(DeleteActorsAction).GetField("_data", BindingFlags.NonPublic | BindingFlags.Instance);
                byte[] actorData = actorDataField.GetValue(deleteAction) as byte[];
                Guid[] deletedActorGuids = Actor.TryGetSerializedObjectsIds(actorData);

                // Get the nodes in the scene graph
                var actorNodes = new List<ActorNode>(deletedActorGuids.Length);
                for (int i = 0; i < deletedActorGuids.Length; i++)
                {
                    var foundNode = SceneGraphFactory.FindNode(deletedActorGuids[i]);
                    if (foundNode is ActorNode node)
                    {
                        actorNodes.Add(node);
                    }
                }

                // Set the node parents field (used internally when deleting actors)
                FieldInfo nodeParentsField = typeof(DeleteActorsAction).GetField("_nodeParents", BindingFlags.NonPublic | BindingFlags.Instance);
                List<ActorNode> nodeParents = nodeParentsField.GetValue(deleteAction) as List<ActorNode>;
                actorNodes.BuildNodesParents(nodeParents);
            }
            else if (undoAction is SelectionChangeAction selectionChangeAction)
            {
                // Selection change action custom handling
                var callbackProp = typeof(SelectionChangeAction).GetField("_callback",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var callbackMethod = typeof(SceneEditingModule).GetMethod("OnSelectionUndo",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                Action<FlaxEditor.SceneGraph.SceneGraphNode[]> callbackLambda =
                    (param) => callbackMethod.Invoke(Editor.Instance.SceneEditing, new object[] { param });

                callbackProp.SetValue(selectionChangeAction, callbackLambda);
            }

            return undoAction as IUndoAction;
        }

        public override void Write(BinaryWriter bw)
        {
            WriteAction(bw, this._action);
        }

        /// <summary>
        /// Recursively writes the <see cref="IUndoAction"/>s to the output stream
        /// </summary>
        /// <param name="bw"></param>
        /// <param name="action"></param>
        private void WriteAction(BinaryWriter bw, IUndoAction action)
        {
            if (action is MultiUndoAction multiUndoAction)
            {
                bw.Write(true); // It's a MultiUndoAction
                int actionCount = multiUndoAction.Actions.Length;
                bw.Write(actionCount);
                // bw.Write(multiUndoAction.ActionString) // TODO: Action string
                foreach (var subAction in multiUndoAction.Actions)
                {
                    WriteAction(bw, subAction);
                }
            }
            else if (action is UndoActionObject undoActionObject)
            {
                bw.Write(false);
                bw.Write(this.ObjectDiffPacket);
                bw.Write(UndoActionObjectSerializer.ToJson(undoActionObject));
            }
            else
            {
                bw.Write(false);
                string typeName = action.GetType().AssemblyQualifiedName;
                string data = FlaxEngine.Json.JsonSerializer.Serialize(action);
                bw.Write(typeName);
                bw.Write(data);
            }
        }
    }
}