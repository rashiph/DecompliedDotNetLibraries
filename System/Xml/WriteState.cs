namespace System.Xml
{
    using System;

    public enum WriteState
    {
        Start,
        Prolog,
        Element,
        Attribute,
        Content,
        Closed,
        Error
    }
}

