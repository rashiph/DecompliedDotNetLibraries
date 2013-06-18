namespace System.ServiceModel.Dispatcher
{
    using System;

    internal enum ValueDataType : byte
    {
        Boolean = 1,
        Double = 2,
        None = 0,
        Sequence = 4,
        StackFrame = 3,
        String = 5
    }
}

