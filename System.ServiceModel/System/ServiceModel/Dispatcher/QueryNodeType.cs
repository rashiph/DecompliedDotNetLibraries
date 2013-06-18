namespace System.ServiceModel.Dispatcher
{
    using System;

    internal enum QueryNodeType : byte
    {
        All = 0xff,
        Ancestor = 0x85,
        Any = 0,
        Attribute = 2,
        ChildNodes = 0xbc,
        Comment = 0x10,
        Element = 4,
        Multiple = 0x80,
        Namespace = 0x40,
        Processing = 0x20,
        Root = 1,
        Text = 8
    }
}

