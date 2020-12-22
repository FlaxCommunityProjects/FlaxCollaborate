using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FlaxEditor.History;
using FlaxEditor.Scripting;
using FlaxEditor.Utilities;
using FlaxEngine;

namespace CollaboratePlugin
{
    public static class UndoActionObjectSerializer
    {
        public class SerializedUndoActionObject
        {
            public object TargetInstance;
            public string Data;

            // TODO: ActionString, this one is tricky since you have to get the backing field

            // See FlaxEditor.Utilities.MemberInfoPath
            public SerializedStackEntry[][] MemberStacks;

            public class SerializedStackEntry
            {
                public object Index;

                public Type MemberInfoType;
                public string AssemblyQualifiedName;
                public string Path;
            }
        }

        private static readonly FieldInfo TargetInstanceField =
            typeof(UndoActionObject).GetField("TargetInstance", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo MembersField =
            typeof(UndoActionObject).GetField("Members", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo StackField
            = typeof(MemberInfoPath).GetField("_stack", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo DataField
            = typeof(UndoActionObject).GetField("_data", BindingFlags.NonPublic | BindingFlags.Instance);

        public static string ToJson(UndoActionObject undoActionObject)
        {
            //
            // Here be dragons. Thou art forewarned
            //

            var serialized = new SerializedUndoActionObject();

            // Serialize the target object
            serialized.TargetInstance = TargetInstanceField.GetValue(undoActionObject);
            if (serialized.TargetInstance != null && !(serialized.TargetInstance is FlaxEngine.Object))
            {
                throw new Exception("Expected targetInstance to be a 'GUID reference type'!");
            }

            // Serialize the Data
            serialized.Data = (string)DataField.GetValue(undoActionObject);

            // Serialize the MemberInfoPaths (which properties/fields got modified)
            var members = (MemberInfoPath[])MembersField.GetValue(undoActionObject);

            serialized.MemberStacks = new SerializedUndoActionObject.SerializedStackEntry[members.Length][];

            // The members need a lot of special handling
            for (int i = 0; i < members.Length; i++)
            {
                var stack = (MemberInfoPath.Entry[])StackField.GetValue(members[i]);
                serialized.MemberStacks[i] = new SerializedUndoActionObject.SerializedStackEntry[stack.Length];

                // Now handle those stack entries correctly
                for (int j = 0; j < stack.Length; j++)
                {
                    serialized.MemberStacks[i][j] = new SerializedUndoActionObject.SerializedStackEntry();

                    if (stack[j].Member.IScriptMemberInfo != null)
                    {
                        Debug.LogError("Serializing IScriptMemberInfo not supported");
                    }

                    if (stack[j].Member.Type is MethodInfo)
                    {
                        serialized.MemberStacks[i][j].MemberInfoType = typeof(MethodInfo);
                    }
                    else if (stack[j].Member.Type is FieldInfo)
                    {
                        serialized.MemberStacks[i][j].MemberInfoType = typeof(FieldInfo);
                    }
                    else if (stack[j].Member.Type is PropertyInfo)
                    {
                        serialized.MemberStacks[i][j].MemberInfoType = typeof(PropertyInfo);
                    }
                    else
                    {
                        throw new Exception("Unexpected entry.Member.Type: " + stack[j].Member.Type?.DeclaringType?.AssemblyQualifiedName);
                    }


                    // serialized.MemberStacks[i][j].AssemblyQualifiedName = stack[j].Member.DeclaringType.AssemblyQualifiedName;
                    serialized.MemberStacks[i][j].Path = stack[j].Member.Name;
                    serialized.MemberStacks[i][j].Index = stack[j].Index;
                }
            }

            return FlaxEngine.Json.JsonSerializer.Serialize(serialized);
        }

        public static UndoActionObject FromJson(string json)
        {
            //
            // Here be dragons. Thou art forewarned
            //
            var serialized = FlaxEngine.Json.JsonSerializer.Deserialize<SerializedUndoActionObject>(json);

            UndoActionObject undoActionObject = (UndoActionObject)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(UndoActionObject));

            TargetInstanceField.SetValue(undoActionObject, serialized.TargetInstance);
            DataField.SetValue(undoActionObject, serialized.Data);

            var memberInfoPaths = new MemberInfoPath[serialized.MemberStacks.Length];
            for (int i = 0; i < memberInfoPaths.Length; i++)
            {
                // Prepare the stack entries
                var entries = new MemberInfoPath.Entry[serialized.MemberStacks[i].Length];
                for (int j = 0; j < entries.Length; j++)
                {
                    var serializedEntry = serialized.MemberStacks[i][j];
                    Type type = Type.GetType(serializedEntry.AssemblyQualifiedName);
                    MemberInfo memberInfo;
                    if (serializedEntry.MemberInfoType == typeof(MethodInfo))
                    {
                        memberInfo = type.GetMethod(serializedEntry.Path);
                    }
                    else if (serializedEntry.MemberInfoType == typeof(FieldInfo))
                    {
                        memberInfo = type.GetField(serializedEntry.Path);
                    }
                    else if (serializedEntry.MemberInfoType == typeof(PropertyInfo))
                    {
                        memberInfo = type.GetProperty(serializedEntry.Path);
                    }
                    else
                    {
                        throw new Exception("Unexpected MemberInfoType: " + serializedEntry.MemberInfoType);
                    }

                    entries[j] = new MemberInfoPath.Entry(new ScriptMemberInfo(memberInfo), serializedEntry.Index);
                }

                // MemberInfoPath is a struct, we have to jump through some hoops to set a private field
                object boxedMemberInfoPath = memberInfoPaths[i];
                StackField.SetValue(boxedMemberInfoPath, entries);
                memberInfoPaths[i] = (MemberInfoPath)boxedMemberInfoPath;
            }
            MembersField.SetValue(undoActionObject, memberInfoPaths);

            return undoActionObject;
        }
    }
}