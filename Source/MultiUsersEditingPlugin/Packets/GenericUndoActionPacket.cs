using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FlaxEditor;
using FlaxEditor.History;
using FlaxEditor.Modules;
using FlaxEditor.Utilities;
using FlaxEngine;

namespace MultiUsersEditingPlugin
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
        }

        public override void Read(BinaryReader bs)
        {
            int count = bs.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                ReadSingle(bs);
            }
        }

        private void ReadSingle(BinaryReader bs)
        {
            string typeName = bs.ReadString();
            string data = bs.ReadString();
            try
            {
                if (typeName == this.ObjectDiffPacket)
                {
                    Debug.Log(data);
                    IUndoAction undoAction = UndoActionObjectSerializer.FromJson(data);
                    undoAction.Do();
                }
                else
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
                    // If you ever want this to happen, you'll have to add some more special handling for the MultiUndoAction
                    //Editor.Instance.Undo.AddAction(undoAction as IUndoAction);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public override void Write(BinaryWriter bw)
        {
            int actionCount = 1;
            if (this._action is MultiUndoAction multiUndoAction)
            {
                actionCount = multiUndoAction.Actions.Length;
                bw.Write(actionCount);
                // bw.Write(multiUndoAction.ActionString) // TODO: Action string
                foreach (var action in multiUndoAction.Actions)
                {
                    WriteSingleAction(bw, action);
                }
            }
            else
            {
                bw.Write(actionCount);
                WriteSingleAction(bw, this._action);
            }

            //FlaxEngine.Json.JsonSerializer.SerializeDiff
            /*if (this._action is MultiUndoAction multiAction)
                {
                    if (multiAction.Actions[0] is UndoActionObject undoActionObject)
                    {
                        var field = typeof(UndoActionObject).GetField("Members", BindingFlags.NonPublic | BindingFlags.Instance);
                        Debug.LogError("MEMBERS:");
                        var members = (FlaxEditor.Utilities.MemberInfoPath[])field.GetValue(undoActionObject);
                        Debug.LogError(members.Length);
                        Debug.LogError(string.Join(",", members.Select(m => m.ToString())));
                    }
                }*/
        }

        private void WriteSingleAction(BinaryWriter bw, IUndoAction action)
        {
            try
            {
                if (action is UndoActionObject undoActionObject)
                {
                    bw.Write(this.ObjectDiffPacket);

                    bw.Write(UndoActionObjectSerializer.ToJson(undoActionObject));
                    //bw.Write(FlaxEngine.Json.JsonSerializer.Serialize(new ))

                    /*string typeName = this.ObjectDiffPacket;
                    var targetInstanceField = typeof(UndoActionObject).GetField("TargetInstance", BindingFlags.NonPublic | BindingFlags.Instance);

                    object targetInstance = targetInstanceField.GetValue(undoActionObject);
                    object otherInstance = // That's the issue, I can't actually get the otherInstance trivially.
                        // This makes FlaxEngine.Json.JsonSerializer.SerializeDiff impossible
                    // private readonly object TargetInstance <== TODO: Grab this one
                    // diff.SetMemberValue(data.TargetInstance, diff.Value2); <== diff.Value2  is  data.Values2[i].Value
                    // diff.Value1 <== Needed for *UNDO*

                    //
                    //string data = undoActionObject.*/
                }
                else
                {
                    string typeName = _action.GetType().AssemblyQualifiedName;
                    string data = FlaxEngine.Json.JsonSerializer.Serialize(action);
                    bw.Write(typeName);
                    bw.Write(data);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}