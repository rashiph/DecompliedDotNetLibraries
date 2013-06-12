namespace System.Diagnostics
{
    using System;

    internal class FilterElement : TypedElement
    {
        public FilterElement() : base(typeof(TraceFilter))
        {
        }

        public TraceFilter GetRuntimeObject()
        {
            TraceFilter filter = (TraceFilter) base.BaseGetRuntimeObject();
            filter.initializeData = base.InitData;
            return filter;
        }

        internal TraceFilter RefreshRuntimeObject(TraceFilter filter)
        {
            if (!(Type.GetType(this.TypeName) != filter.GetType()) && !(base.InitData != filter.initializeData))
            {
                return filter;
            }
            base._runtimeObject = null;
            return this.GetRuntimeObject();
        }
    }
}

