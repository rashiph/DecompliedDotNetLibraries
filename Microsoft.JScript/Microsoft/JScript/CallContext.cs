namespace Microsoft.JScript
{
    using System;

    internal class CallContext
    {
        private readonly object[] actual_parameters;
        private readonly LateBinding callee;
        internal readonly Context sourceContext;

        internal CallContext(Context sourceContext, LateBinding callee, object[] actual_parameters)
        {
            this.sourceContext = sourceContext;
            this.callee = callee;
            this.actual_parameters = actual_parameters;
        }

        internal string FunctionName()
        {
            if (this.callee == null)
            {
                return "eval";
            }
            return this.callee.ToString();
        }
    }
}

