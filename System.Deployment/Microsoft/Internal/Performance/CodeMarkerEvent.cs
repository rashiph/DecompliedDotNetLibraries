namespace Microsoft.Internal.Performance
{
    using System;

    internal enum CodeMarkerEvent
    {
        perfCopyBegin = 0x234,
        perfCopyEnd = 0x235,
        perfNewApptBegin = 0x20d,
        perfNewApptEnd = 0x20e,
        perfNewTaskBegin = 0x20b,
        perfNewTaskEnd = 0x20c,
        perfParseBegin = 0x1c86,
        perfParseEnd = 0x1c87,
        perfPersisterWriteEnd = 0x1f91,
        perfPersisterWriteStart = 0x1f90
    }
}

