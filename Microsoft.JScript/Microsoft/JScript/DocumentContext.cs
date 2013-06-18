namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Diagnostics.SymbolStore;
    using System.Reflection.Emit;

    public class DocumentContext
    {
        private CompilerGlobals _compilerGlobals;
        private bool checkForFirst;
        internal bool debugOn;
        internal string documentName;
        internal ISymbolDocumentWriter documentWriter;
        internal VsaEngine engine;
        private int firstEndCol;
        private int firstEndLine;
        private int firstStartCol;
        private int firstStartLine;
        internal static readonly Guid language = new Guid("3a12d0b6-c26c-11d0-b442-00a0244a1dd2");
        internal int lastLineInSource;
        private SimpleHashtable reportedVariables;
        internal VsaItem sourceItem;
        internal int startCol;
        internal int startLine;
        internal static readonly Guid vendor = new Guid("994b45c4-e6e9-11d2-903f-00c04fa302a1");

        internal DocumentContext(VsaItem sourceItem)
        {
            if (sourceItem.codebase != null)
            {
                this.documentName = sourceItem.codebase;
            }
            else
            {
                string rootMoniker = sourceItem.engine.RootMoniker;
                this.documentName = rootMoniker + (rootMoniker.EndsWith("/", StringComparison.Ordinal) ? "" : "/") + sourceItem.Name;
            }
            this.documentWriter = null;
            this.startLine = 0;
            this.startCol = 0;
            this.lastLineInSource = 0;
            this.sourceItem = sourceItem;
            this.engine = sourceItem.engine;
            this.debugOn = (this.engine != null) && this.engine.GenerateDebugInfo;
            this._compilerGlobals = null;
            this.checkForFirst = false;
        }

        internal DocumentContext(string name, VsaEngine engine)
        {
            this.documentName = name;
            this.documentWriter = null;
            this.startLine = 0;
            this.startCol = 0;
            this.lastLineInSource = 0;
            this.sourceItem = null;
            this.engine = engine;
            this.debugOn = (engine != null) && engine.GenerateDebugInfo;
            this._compilerGlobals = null;
            this.reportedVariables = null;
            this.checkForFirst = false;
        }

        internal DocumentContext(string documentName, int startLine, int startCol, int lastLineInSource, VsaItem sourceItem)
        {
            this.documentName = documentName;
            this.documentWriter = null;
            this.startLine = startLine;
            this.startCol = startCol;
            this.lastLineInSource = lastLineInSource;
            this.sourceItem = sourceItem;
            this.engine = sourceItem.engine;
            this.debugOn = (this.engine != null) && this.engine.GenerateDebugInfo;
            this._compilerGlobals = null;
            this.checkForFirst = false;
        }

        internal void EmitFirstLineInfo(ILGenerator ilgen, int line, int column, int endLine, int endColumn)
        {
            this.EmitLineInfo(ilgen, line, column, endLine, endColumn);
            this.checkForFirst = true;
            this.firstStartLine = line;
            this.firstStartCol = column;
            this.firstEndLine = endLine;
            this.firstEndCol = endColumn;
        }

        internal void EmitLineInfo(ILGenerator ilgen, int line, int column, int endLine, int endColumn)
        {
            if (this.debugOn)
            {
                if (((this.checkForFirst && (line == this.firstStartLine)) && ((column == this.firstStartCol) && (endLine == this.firstEndLine))) && (endColumn == this.firstEndCol))
                {
                    this.checkForFirst = false;
                }
                else
                {
                    if (this.documentWriter == null)
                    {
                        this.documentWriter = this.GetSymDocument(this.documentName);
                    }
                    ilgen.MarkSequencePoint(this.documentWriter, (this.startLine + line) - this.lastLineInSource, (this.startCol + column) + 1, (this.startLine - this.lastLineInSource) + endLine, (this.startCol + endColumn) + 1);
                }
            }
        }

        private ISymbolDocumentWriter GetSymDocument(string documentName)
        {
            SimpleHashtable documents = this.compilerGlobals.documents;
            object obj2 = documents[documentName];
            if (obj2 == null)
            {
                obj2 = this._compilerGlobals.module.DefineDocument(this.documentName, language, vendor, Guid.Empty);
                documents[documentName] = obj2;
            }
            return (ISymbolDocumentWriter) obj2;
        }

        internal void HandleError(JScriptException error)
        {
            if (this.sourceItem == null)
            {
                if (error.Severity == 0)
                {
                    throw error;
                }
            }
            else if (!this.sourceItem.engine.OnCompilerError(error))
            {
                throw new EndOfFile();
            }
        }

        internal bool HasAlreadySeenErrorFor(string varName)
        {
            if (this.reportedVariables == null)
            {
                this.reportedVariables = new SimpleHashtable(8);
            }
            else if (this.reportedVariables[varName] != null)
            {
                return true;
            }
            this.reportedVariables[varName] = varName;
            return false;
        }

        internal CompilerGlobals compilerGlobals
        {
            get
            {
                if (this._compilerGlobals == null)
                {
                    this._compilerGlobals = this.engine.CompilerGlobals;
                }
                return this._compilerGlobals;
            }
        }
    }
}

