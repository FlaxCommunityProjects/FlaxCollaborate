using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FlaxEditor;
using FlaxEditor.History;
using FlaxEditor.Modules;
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

            if (typeName == this.ObjectDiffPacket)
            {
                IUndoAction a = UndoActionObjectSerializer.FromJson(data);
                a.Do();
                return a;
            }
            else
            {
                object undoAction =
                    System.Runtime.Serialization.FormatterServices.GetUninitializedObject(Type.GetType(typeName));

                FlaxEngine.Json.JsonSerializer.Deserialize(undoAction, data);

                if (undoAction is SelectionChangeAction selectionChangeAction)
                {
                    // Selection change action custom handling
                    var callbackProp = typeof(SelectionChangeAction).GetField("_callback",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    var callbackMethod = typeof(SceneEditingModule).GetMethod("OnSelectionUndo",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                    Action<FlaxEditor.SceneGraph.SceneGraphNode[]> callbackLambda =
                        (param) => callbackMethod.Invoke(Editor.Instance.SceneEditing, new object[] { param });

                    callbackProp.SetValue(selectionChangeAction, callbackLambda);

                    // Don't execute the action, instead do stuff
                    // TODO: What if some action depends on the current selection?
                    EditingSessionPlugin.Instance.Session.GetUserById(Author).Selection =
                        selectionChangeAction.Data.After;

                    return selectionChangeAction;
                }
                else
                {
                    try
                    {
                        (undoAction as IUndoAction).Do();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                    return (IUndoAction)undoAction;
                }
            }
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