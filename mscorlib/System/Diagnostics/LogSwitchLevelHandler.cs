namespace System.Diagnostics
{
    using System;
    using System.Runtime.CompilerServices;

    [Serializable]
    internal delegate void LogSwitchLevelHandler(LogSwitch ls, LoggingLevels newLevel);
}

