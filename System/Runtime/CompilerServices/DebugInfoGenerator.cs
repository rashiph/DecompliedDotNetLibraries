namespace System.Runtime.CompilerServices
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;

    public abstract class DebugInfoGenerator
    {
        protected DebugInfoGenerator()
        {
        }

        public static DebugInfoGenerator CreatePdbGenerator()
        {
            return new SymbolDocumentGenerator();
        }

        public abstract void MarkSequencePoint(LambdaExpression method, int ilOffset, DebugInfoExpression sequencePoint);
        internal virtual void MarkSequencePoint(LambdaExpression method, MethodBase methodBase, ILGenerator ilg, DebugInfoExpression sequencePoint)
        {
            this.MarkSequencePoint(method, ilg.ILOffset, sequencePoint);
        }

        internal virtual void SetLocalName(LocalBuilder localBuilder, string name)
        {
        }
    }
}

