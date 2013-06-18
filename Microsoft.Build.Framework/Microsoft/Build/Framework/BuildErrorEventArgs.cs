namespace Microsoft.Build.Framework
{
    using System;
    using System.IO;
    using System.Runtime;

    [Serializable]
    public class BuildErrorEventArgs : LazyFormattedBuildEventArgs
    {
        private string code;
        private int columnNumber;
        private int endColumnNumber;
        private int endLineNumber;
        private string file;
        private int lineNumber;
        private string projectFile;
        private string subcategory;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected BuildErrorEventArgs()
        {
        }

        public BuildErrorEventArgs(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName) : this(subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, helpKeyword, senderName, DateTime.UtcNow)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public BuildErrorEventArgs(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName, DateTime eventTimestamp) : this(subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, helpKeyword, senderName, eventTimestamp, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public BuildErrorEventArgs(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName, DateTime eventTimestamp, params object[] messageArgs) : base(message, helpKeyword, senderName, eventTimestamp, messageArgs)
        {
            this.subcategory = subcategory;
            this.code = code;
            this.file = file;
            this.lineNumber = lineNumber;
            this.columnNumber = columnNumber;
            this.endLineNumber = endLineNumber;
            this.endColumnNumber = endColumnNumber;
        }

        internal override void CreateFromStream(BinaryReader reader)
        {
            base.CreateFromStream(reader);
            if (reader.ReadByte() == 0)
            {
                this.subcategory = null;
            }
            else
            {
                this.subcategory = reader.ReadString();
            }
            if (reader.ReadByte() == 0)
            {
                this.code = null;
            }
            else
            {
                this.code = reader.ReadString();
            }
            if (reader.ReadByte() == 0)
            {
                this.file = null;
            }
            else
            {
                this.file = reader.ReadString();
            }
            if (reader.ReadByte() == 0)
            {
                this.projectFile = null;
            }
            else
            {
                this.projectFile = reader.ReadString();
            }
            this.lineNumber = reader.ReadInt32();
            this.columnNumber = reader.ReadInt32();
            this.endLineNumber = reader.ReadInt32();
            this.endColumnNumber = reader.ReadInt32();
        }

        internal override void WriteToStream(BinaryWriter writer)
        {
            base.WriteToStream(writer);
            if (this.subcategory == null)
            {
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);
                writer.Write(this.subcategory);
            }
            if (this.code == null)
            {
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);
                writer.Write(this.code);
            }
            if (this.file == null)
            {
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);
                writer.Write(this.file);
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
            writer.Write(this.lineNumber);
            writer.Write(this.columnNumber);
            writer.Write(this.endLineNumber);
            writer.Write(this.endColumnNumber);
        }

        public string Code
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.code;
            }
        }

        public int ColumnNumber
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.columnNumber;
            }
        }

        public int EndColumnNumber
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.endColumnNumber;
            }
        }

        public int EndLineNumber
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.endLineNumber;
            }
        }

        public string File
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.file;
            }
        }

        public int LineNumber
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.lineNumber;
            }
        }

        public string ProjectFile
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.projectFile;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.projectFile = value;
            }
        }

        public string Subcategory
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.subcategory;
            }
        }
    }
}

