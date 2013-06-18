namespace System.Web.Routing
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class LiteralSubsegment : PathSubsegment
    {
        public LiteralSubsegment(string literal)
        {
            this.Literal = literal;
        }

        public string Literal { get; private set; }
    }
}

