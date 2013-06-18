namespace Microsoft.Build.Framework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    public class ProjectStartedEventArgs : BuildStatusEventArgs
    {
        public const int InvalidProjectId = -1;
        [NonSerialized]
        private IEnumerable items;
        [OptionalField(VersionAdded=2)]
        private BuildEventContext parentProjectBuildEventContext;
        private string projectFile;
        [OptionalField(VersionAdded=2)]
        private int projectId;
        [NonSerialized]
        private IEnumerable properties;
        private string targetNames;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ProjectStartedEventArgs()
        {
        }

        public ProjectStartedEventArgs(string message, string helpKeyword, string projectFile, string targetNames, IEnumerable properties, IEnumerable items) : this(message, helpKeyword, projectFile, targetNames, properties, items, DateTime.UtcNow)
        {
        }

        public ProjectStartedEventArgs(string message, string helpKeyword, string projectFile, string targetNames, IEnumerable properties, IEnumerable items, DateTime eventTimestamp) : base(message, helpKeyword, "MSBuild", eventTimestamp)
        {
            this.projectFile = projectFile;
            if (targetNames == null)
            {
                this.targetNames = string.Empty;
            }
            else
            {
                this.targetNames = targetNames;
            }
            this.properties = properties;
            this.items = items;
        }

        public ProjectStartedEventArgs(int projectId, string message, string helpKeyword, string projectFile, string targetNames, IEnumerable properties, IEnumerable items, BuildEventContext parentBuildEventContext) : this(projectId, message, helpKeyword, projectFile, targetNames, properties, items, parentBuildEventContext, DateTime.UtcNow)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ProjectStartedEventArgs(int projectId, string message, string helpKeyword, string projectFile, string targetNames, IEnumerable properties, IEnumerable items, BuildEventContext parentBuildEventContext, DateTime eventTimestamp) : this(message, helpKeyword, projectFile, targetNames, properties, items, eventTimestamp)
        {
            this.parentProjectBuildEventContext = parentBuildEventContext;
            this.projectId = projectId;
        }

        internal override void CreateFromStream(BinaryReader reader)
        {
            base.CreateFromStream(reader);
            this.projectId = reader.ReadInt32();
            if (reader.ReadByte() == 0)
            {
                this.parentProjectBuildEventContext = null;
            }
            else
            {
                int nodeId = reader.ReadInt32();
                int submissionId = reader.ReadInt32();
                int projectInstanceId = reader.ReadInt32();
                int projectContextId = reader.ReadInt32();
                int targetId = reader.ReadInt32();
                int taskId = reader.ReadInt32();
                this.parentProjectBuildEventContext = new BuildEventContext(submissionId, nodeId, projectInstanceId, projectContextId, targetId, taskId);
            }
            if (reader.ReadByte() == 0)
            {
                this.projectFile = null;
            }
            else
            {
                this.projectFile = reader.ReadString();
            }
            this.targetNames = reader.ReadString();
            if (reader.ReadByte() == 0)
            {
                this.properties = null;
            }
            else
            {
                int capacity = reader.ReadInt32();
                ArrayList list = new ArrayList(capacity);
                for (int i = 0; i < capacity; i++)
                {
                    string key = reader.ReadString();
                    string str2 = reader.ReadString();
                    if ((key != null) && (str2 != null))
                    {
                        DictionaryEntry entry = new DictionaryEntry(key, str2);
                        list.Add(entry);
                    }
                }
                this.properties = list;
            }
        }

        private Dictionary<string, string> GeneratePropertyList()
        {
            if (this.properties == null)
            {
                return null;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (DictionaryEntry entry in this.properties)
            {
                object key = entry.Key;
                object obj2 = entry.Value;
                if ((entry.Key != null) && (entry.Value != null))
                {
                    dictionary.Add((string) entry.Key, (string) entry.Value);
                }
            }
            return dictionary;
        }

        [OnDeserialized]
        private void SetDefaultsAfterSerialization(StreamingContext sc)
        {
            if (this.parentProjectBuildEventContext == null)
            {
                this.parentProjectBuildEventContext = new BuildEventContext(-2, -1, -2, -1);
            }
        }

        [OnDeserializing]
        private void SetDefaultsBeforeSerialization(StreamingContext sc)
        {
            this.projectId = -1;
            this.parentProjectBuildEventContext = null;
        }

        internal override void WriteToStream(BinaryWriter writer)
        {
            base.WriteToStream(writer);
            writer.Write(this.projectId);
            if (this.parentProjectBuildEventContext == null)
            {
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);
                writer.Write(this.parentProjectBuildEventContext.NodeId);
                writer.Write(this.parentProjectBuildEventContext.SubmissionId);
                writer.Write(this.parentProjectBuildEventContext.ProjectInstanceId);
                writer.Write(this.parentProjectBuildEventContext.ProjectContextId);
                writer.Write(this.parentProjectBuildEventContext.TargetId);
                writer.Write(this.parentProjectBuildEventContext.TaskId);
            }
            if (this.projectFile == null)
            {
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);
                writer.Write(this.projectFile);
            }
            writer.Write(this.targetNames);
            Dictionary<string, string> dictionary = this.GeneratePropertyList();
            if ((dictionary == null) || (dictionary.Count == 0))
            {
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);
                writer.Write(dictionary.Count);
                foreach (KeyValuePair<string, string> pair in dictionary)
                {
                    writer.Write(pair.Key);
                    writer.Write(pair.Value);
                }
            }
        }

        public IEnumerable Items
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.items;
            }
        }

        public BuildEventContext ParentProjectBuildEventContext
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parentProjectBuildEventContext;
            }
        }

        public string ProjectFile
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.projectFile;
            }
        }

        public int ProjectId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.projectId;
            }
        }

        public IEnumerable Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.properties;
            }
        }

        public string TargetNames
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.targetNames;
            }
        }
    }
}

