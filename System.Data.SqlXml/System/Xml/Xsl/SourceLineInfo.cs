namespace System.Xml.Xsl
{
    using System;
    using System.Diagnostics;
    using System.Runtime;

    [DebuggerDisplay("{Uri} [{StartLine},{StartPos} -- {EndLine},{EndPos}]")]
    internal class SourceLineInfo : ISourceLineInfo
    {
        protected Location end;
        public static SourceLineInfo NoSource = new SourceLineInfo(string.Empty, 0xfeefee, 0, 0xfeefee, 0);
        protected const int NoSourceMagicNumber = 0xfeefee;
        protected Location start;
        protected string uriString;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SourceLineInfo(string uriString, Location start, Location end)
        {
            this.uriString = uriString;
            this.start = start;
            this.end = end;
        }

        public SourceLineInfo(string uriString, int startLine, int startPos, int endLine, int endPos) : this(uriString, new Location(startLine, startPos), new Location(endLine, endPos))
        {
        }

        public static string GetFileName(string uriString)
        {
            System.Uri uri;
            if (((uriString.Length != 0) && System.Uri.TryCreate(uriString, UriKind.Absolute, out uri)) && uri.IsFile)
            {
                return uri.LocalPath;
            }
            return uriString;
        }

        [Conditional("DEBUG")]
        public static void Validate(ISourceLineInfo lineInfo)
        {
            if (lineInfo.Start.Line != 0)
            {
                int line = lineInfo.Start.Line;
            }
        }

        public Location End
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.end;
            }
        }

        public int EndLine
        {
            get
            {
                return this.end.Line;
            }
        }

        public int EndPos
        {
            get
            {
                return this.end.Pos;
            }
        }

        public bool IsNoSource
        {
            get
            {
                return (this.StartLine == 0xfeefee);
            }
        }

        public Location Start
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.start;
            }
        }

        public int StartLine
        {
            get
            {
                return this.start.Line;
            }
        }

        public int StartPos
        {
            get
            {
                return this.start.Pos;
            }
        }

        public string Uri
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.uriString;
            }
        }
    }
}

