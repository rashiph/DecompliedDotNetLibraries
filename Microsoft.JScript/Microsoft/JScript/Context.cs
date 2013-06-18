namespace Microsoft.JScript
{
    using System;
    using System.Reflection.Emit;

    public class Context
    {
        internal DocumentContext document;
        internal int endLineNumber;
        internal int endLinePos;
        internal int endPos;
        internal int errorReported;
        internal int lineNumber;
        internal string source_string;
        internal int startLinePos;
        internal int startPos;
        internal JSToken token;

        internal Context(DocumentContext document, string source_string)
        {
            this.document = document;
            this.source_string = source_string;
            this.lineNumber = 1;
            this.startLinePos = 0;
            this.startPos = 0;
            this.endLineNumber = 1;
            this.endLinePos = 0;
            this.endPos = (source_string == null) ? -1 : source_string.Length;
            this.token = JSToken.None;
            this.errorReported = 0xf4240;
        }

        internal Context(DocumentContext document, string source_string, int lineNumber, int startLinePos, int startPos, int endLineNumber, int endLinePos, int endPos, JSToken token)
        {
            this.document = document;
            this.source_string = source_string;
            this.lineNumber = lineNumber;
            this.startLinePos = startLinePos;
            this.startPos = startPos;
            this.endLineNumber = endLineNumber;
            this.endLinePos = endLinePos;
            this.endPos = endPos;
            this.token = token;
            this.errorReported = 0xf4240;
        }

        internal Context Clone()
        {
            return new Context(this.document, this.source_string, this.lineNumber, this.startLinePos, this.startPos, this.endLineNumber, this.endLinePos, this.endPos, this.token) { errorReported = this.errorReported };
        }

        internal Context CombineWith(Context other)
        {
            return new Context(this.document, this.source_string, this.lineNumber, this.startLinePos, this.startPos, other.endLineNumber, other.endLinePos, other.endPos, this.token);
        }

        internal void EmitFirstLineInfo(ILGenerator ilgen)
        {
            this.document.EmitFirstLineInfo(ilgen, this.StartLine, this.StartColumn, this.EndLine, this.EndColumn);
        }

        internal void EmitLineInfo(ILGenerator ilgen)
        {
            this.document.EmitLineInfo(ilgen, this.StartLine, this.StartColumn, this.EndLine, this.EndColumn);
        }

        internal bool Equals(Context ctx)
        {
            return (((((this.source_string == ctx.source_string) && (this.lineNumber == ctx.lineNumber)) && ((this.startLinePos == ctx.startLinePos) && (this.startPos == ctx.startPos))) && (((this.endLineNumber == ctx.endLineNumber) && (this.endLinePos == ctx.endLinePos)) && (this.endPos == ctx.endPos))) && (this.token == ctx.token));
        }

        internal bool Equals(string str)
        {
            int length = this.endPos - this.startPos;
            return ((length == str.Length) && (string.CompareOrdinal(this.source_string, this.startPos, str, 0, length) == 0));
        }

        public string GetCode()
        {
            if ((this.endPos > this.startPos) && (this.endPos <= this.source_string.Length))
            {
                return this.source_string.Substring(this.startPos, this.endPos - this.startPos);
            }
            return null;
        }

        public JSToken GetToken()
        {
            return this.token;
        }

        internal void HandleError(JSError errorId)
        {
            this.HandleError(errorId, null, false);
        }

        internal void HandleError(JSError errorId, bool treatAsError)
        {
            this.HandleError(errorId, null, treatAsError);
        }

        internal void HandleError(JSError errorId, string message)
        {
            this.HandleError(errorId, message, false);
        }

        internal void HandleError(JSError errorId, string message, bool treatAsError)
        {
            if ((errorId != JSError.UndeclaredVariable) || !this.document.HasAlreadySeenErrorFor(this.GetCode()))
            {
                JScriptException error = new JScriptException(errorId, this);
                if (message != null)
                {
                    error.value = message;
                }
                if (treatAsError)
                {
                    error.isError = treatAsError;
                }
                int severity = error.Severity;
                if (severity < this.errorReported)
                {
                    this.document.HandleError(error);
                    this.errorReported = severity;
                }
            }
        }

        internal void SetSourceContext(DocumentContext document, string source)
        {
            this.source_string = source;
            this.endPos = source.Length;
            this.document = document;
        }

        internal void UpdateWith(Context other)
        {
            this.endPos = other.endPos;
            this.endLineNumber = other.endLineNumber;
            this.endLinePos = other.endLinePos;
        }

        public int EndColumn
        {
            get
            {
                return (this.endPos - this.endLinePos);
            }
        }

        public int EndLine
        {
            get
            {
                return this.endLineNumber;
            }
        }

        public int EndPosition
        {
            get
            {
                return this.endPos;
            }
        }

        public int StartColumn
        {
            get
            {
                return (this.startPos - this.startLinePos);
            }
        }

        public int StartLine
        {
            get
            {
                return this.lineNumber;
            }
        }

        public int StartPosition
        {
            get
            {
                return this.startPos;
            }
        }
    }
}

