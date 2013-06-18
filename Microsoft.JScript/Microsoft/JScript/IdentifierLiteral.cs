namespace Microsoft.JScript
{
    using System;
    using System.Reflection.Emit;

    internal sealed class IdentifierLiteral : AST
    {
        private string identifier;

        internal IdentifierLiteral(string identifier, Context context) : base(context)
        {
            this.identifier = identifier;
        }

        internal override object Evaluate()
        {
            throw new JScriptException(JSError.InternalError, base.context);
        }

        internal override AST PartiallyEvaluate()
        {
            throw new JScriptException(JSError.InternalError, base.context);
        }

        public override string ToString()
        {
            return this.identifier;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            throw new JScriptException(JSError.InternalError, base.context);
        }
    }
}

