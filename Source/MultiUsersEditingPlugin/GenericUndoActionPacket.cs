using System;
using System.IO;
using System.Reflection;
using FlaxEditor;
using FlaxEditor.Modules;
using FlaxEngine;

namespace MultiUsersEditingPlugin
{
    internal class GenericUndoActionPacket : Packet
    {
        private IUndoAction _action;

        public GenericUndoActionPacket()
        {
        }

        public GenericUndoActionPacket(IUndoAction action)
        {
            this._action = action;
        }

        public override void Read(BinaryReader bs)
        {
            string typeName = bs.ReadString();
            string data = bs.ReadString();

            try
            {
                object undoAction = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(Type.GetType(typeName));

                Debug.Log(typeName);
                Debug.Log(data);

                FlaxEngine.Json.JsonSerializer.Deserialize(undoAction, data);

                // I feel so dirty after writing this:
                if (undoAction is SelectionChangeAction selectionChangeAction)
                {
                    // This undo action specifically needs some special treatment
                    var callbackProp = typeof(SelectionChangeAction).GetField("_callback", BindingFlags.NonPublic | BindingFlags.Instance);
                    var callbackMethod = typeof(SceneEditingModule).GetMethod("OnSelectionUndo", BindingFlags.NonPublic | BindingFlags.Instance);

                    Action<FlaxEditor.SceneGraph.SceneGraphNode[]> callbackLambda =
                        (param) => callbackMethod.Invoke(Editor.Instance.SceneEditing, new object[] { param });

                    callbackProp.SetValue(selectionChangeAction, callbackLambda);
                }

                (undoAction as IUndoAction).Do();

                // This might be a slightly bad idea :tm:
                // Commenting it out for now, to prevent an infinite loop
                //Editor.Instance.Undo.AddAction(undoAction as IUndoAction);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public override void Write(BinaryWriter bw)
        {
            try
            {
                string data = FlaxEngine.Json.JsonSerializer.Serialize(this._action);
                bw.Write(_action.GetType().AssemblyQualifiedName);
                bw.Write(data);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}