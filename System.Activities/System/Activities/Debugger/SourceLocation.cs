namespace System.Activities.Debugger
{
    using System;
    using System.Activities;
    using System.Diagnostics;

    [Serializable, DebuggerNonUserCode]
    public class SourceLocation
    {
        private int endColumn;
        private int endLine;
        private string fileName;
        private int startColumn;
        private int startLine;

        public SourceLocation(string fileName, int line) : this(fileName, line, 1, line, 0x7fffffff)
        {
        }

        public SourceLocation(string fileName, int startLine, int startColumn, int endLine, int endColumn)
        {
            if (startLine <= 0)
            {
                throw FxTrace.Exception.Argument("startLine", System.Activities.SR.InvalidSourceLocationLineNumber("startLine", startLine));
            }
            if (startColumn <= 0)
            {
                throw FxTrace.Exception.Argument("startColumn", System.Activities.SR.InvalidSourceLocationColumn("startColumn", startColumn));
            }
            if (endLine <= 0)
            {
                throw FxTrace.Exception.Argument("endLine", System.Activities.SR.InvalidSourceLocationLineNumber("endLine", endLine));
            }
            if (endColumn <= 0)
            {
                throw FxTrace.Exception.Argument("endColumn", System.Activities.SR.InvalidSourceLocationColumn("endColumn", endColumn));
            }
            if (startLine > endLine)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("endLine", endLine, System.Activities.SR.OutOfRangeSourceLocationEndLine(startLine));
            }
            if ((startLine == endLine) && (startColumn > endColumn))
            {
                throw FxTrace.Exception.ArgumentOutOfRange("endColumn", endColumn, System.Activities.SR.OutOfRangeSourceLocationEndColumn(startColumn));
            }
            this.fileName = (fileName != null) ? fileName.ToUpperInvariant() : null;
            this.startLine = startLine;
            this.endLine = endLine;
            this.startColumn = startColumn;
            this.endColumn = endColumn;
        }

        public override bool Equals(object obj)
        {
            SourceLocation location = obj as SourceLocation;
            if (location == null)
            {
                return false;
            }
            if (this.FileName != location.FileName)
            {
                return false;
            }
            return (((this.StartLine == location.StartLine) && (this.StartColumn == location.StartColumn)) && ((this.EndLine == location.EndLine) && (this.EndColumn == location.EndColumn)));
        }

        public override int GetHashCode()
        {
            return ((this.FileName.GetHashCode() ^ this.StartLine.GetHashCode()) ^ this.StartColumn.GetHashCode());
        }

        internal static bool IsValidRange(int startLine, int startColumn, int endLine, int endColumn)
        {
            if (((startLine <= 0) || (startColumn <= 0)) || ((endLine <= 0) || (endColumn <= 0)))
            {
                return false;
            }
            return ((startLine < endLine) || ((startLine == endLine) && (startColumn < endColumn)));
        }

        public int EndColumn
        {
            get
            {
                return this.endColumn;
            }
        }

        public int EndLine
        {
            get
            {
                return this.endLine;
            }
        }

        public string FileName
        {
            get
            {
                return this.fileName;
            }
        }

        public bool IsSingleWholeLine
        {
            get
            {
                return (((this.endColumn == 0x7fffffff) && (this.startLine == this.endLine)) && (this.startColumn == 1));
            }
        }

        public int StartColumn
        {
            get
            {
                return this.startColumn;
            }
        }

        public int StartLine
        {
            get
            {
                return this.startLine;
            }
        }
    }
}

