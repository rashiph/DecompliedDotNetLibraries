namespace System.Xml.Linq
{
    using System;

    internal class XObjectChangeAnnotation
    {
        internal EventHandler<XObjectChangeEventArgs> changed;
        internal EventHandler<XObjectChangeEventArgs> changing;
    }
}

