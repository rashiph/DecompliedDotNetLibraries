namespace System.Web.Routing
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class ParameterSubsegment : PathSubsegment
    {
        public ParameterSubsegment(string parameterName)
        {
            if (parameterName.StartsWith("*", StringComparison.Ordinal))
            {
                this.ParameterName = parameterName.Substring(1);
                this.IsCatchAll = true;
            }
            else
            {
                this.ParameterName = parameterName;
            }
        }

        public bool IsCatchAll { get; private set; }

        public string ParameterName { get; private set; }
    }
}

