namespace System.Web.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal sealed class ContentPathSegment : PathSegment
    {
        public ContentPathSegment(IList<PathSubsegment> subsegments)
        {
            this.Subsegments = subsegments;
        }

        public bool IsCatchAll
        {
            get
            {
                return this.Subsegments.Any<PathSubsegment>(seg => ((seg is ParameterSubsegment) && ((ParameterSubsegment) seg).IsCatchAll));
            }
        }

        public IList<PathSubsegment> Subsegments { get; private set; }
    }
}

