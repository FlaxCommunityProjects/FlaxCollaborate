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
            if (bs.ReadBoolean())
            {
                // is MultiUndoAction
                int count = bs.ReadInt32();
                IUndoAction[] actions = new IUndoAction[count];

                for (int i = 0; i < count; i++)
                {
                    actions[i] = ReadAction(bs);
                }

                _action = new MultiUndoAction(actions);
            }
            else
            {
                // just a simple IUndoAction
                _action = ReadAction(bs);
            }
        }

        private IUndoAction ReadAction(BinaryReader bs)
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
                    var callbackProp = typeof(SelectionChangeAction).GetField("_callback",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    var callbackMethod = typeof(SceneEditingModule).GetMethod("OnSelectionUndo",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                    Action<FlaxEditor.SceneGraph.SceneGraphNode[]> callbackLambda =
                        (param) => callbackMethod.Invoke(Editor.Instance.SceneEditing, new object[] {param});

                    callbackProp.SetValue(selectionChangeAction, callbackLambda);


                    EditingSessionPlugin.Instance.Session.GetUserById(Author).Selection =
                        selectionChangeAction.Data.After;

                    return selectionChangeAction;
                }
                else
                {
                    (undoAction as IUndoAction).Do();
                    return (IUndoAction) undoAction;
                }
            }
        }


        public override void Write(BinaryWriter bw)
        {
            if (this._action is MultiUndoAction multiUndoAction)
            {
                bw.Write(true);
                int actionCount = multiUndoAction.Actions.Length;
                bw.Write(actionCount);
                // bw.Write(multiUndoAction.ActionString) // TODO: Action string
                foreach (var action in multiUndoAction.Actions)
                {
                    WriteAction(bw, action);
                }
            }
            else
            {
                bw.Write(false);
                WriteAction(bw, this._action);
            }
        }

        private void WriteAction(BinaryWriter bw, IUndoAction action)
        {
            if (action is UndoActionObject undoActionObject)
            {
                bw.Write(this.ObjectDiffPacket);
                bw.Write(UndoActionObjectSerializer.ToJson(undoActionObject));
            }
            else
            {
                string typeName = _action.GetType().AssemblyQualifiedName;
                string data = FlaxEngine.Json.JsonSerializer.Serialize(action);
                bw.Write(typeName);
                bw.Write(data);
            }
        }
    }
}