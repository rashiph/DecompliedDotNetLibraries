namespace System.Runtime.CompilerServices
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.SymbolStore;
    using System.Linq.Expressions;
    using System.Linq.Expressions.Compiler;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class SymbolDocumentGenerator : DebugInfoGenerator
    {
        private Dictionary<SymbolDocumentInfo, ISymbolDocumentWriter> _symbolWriters;

        private ISymbolDocumentWriter GetSymbolWriter(MethodBuilder method, SymbolDocumentInfo document)
        {
            ISymbolDocumentWriter writer;
            if (this._symbolWriters == null)
            {
                this._symbolWriters = new Dictionary<SymbolDocumentInfo, ISymbolDocumentWriter>();
            }
            if (!this._symbolWriters.TryGetValue(document, out writer))
            {
                writer = ((ModuleBuilder) method.Module).DefineDocument(document.FileName, document.Language, document.LanguageVendor, SymbolGuids.DocumentType_Text);
                this._symbolWriters.Add(document, writer);
            }
            return writer;
        }

        public override void MarkSequencePoint(LambdaExpression method, int ilOffset, DebugInfoExpression sequencePoint)
        {
            throw Error.PdbGeneratorNeedsExpressionCompiler();
        }

        internal override void MarkSequencePoint(LambdaExpression method, MethodBase methodBase, ILGenerator ilg, DebugInfoExpression sequencePoint)
        {
            MethodBuilder builder = methodBase as MethodBuilder;
            if (builder != null)
            {
                ilg.MarkSequencePoint(this.GetSymbolWriter(builder, sequencePoint.Document), sequencePoint.StartLine, sequencePoint.StartColumn, sequencePoint.EndLine, sequencePoint.EndColumn);
            }
        }

        internal override void SetLocalName(LocalBuilder localBuilder, string name)
        {
            localBuilder.SetLocalSymInfo(name);
        }
    }
}

