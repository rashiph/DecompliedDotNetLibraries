namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;

    public interface IDesignerFilter
    {
        void PostFilterAttributes(IDictionary attributes);
        void PostFilterEvents(IDictionary events);
        void PostFilterProperties(IDictionary properties);
        void PreFilterAttributes(IDictionary attributes);
        void PreFilterEvents(IDictionary events);
        void PreFilterProperties(IDictionary properties);
    }
}

