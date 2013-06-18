namespace System.Runtime
{
    using System;
    using System.Diagnostics;

    internal class TraceLevelHelper
    {
        private static TraceEventType[] EtwLevelToTraceEventType = new TraceEventType[] { TraceEventType.Critical, TraceEventType.Critical, TraceEventType.Error, TraceEventType.Warning, TraceEventType.Information, TraceEventType.Verbose };

        private static TraceEventType EtwOpcodeToTraceEventType(TraceEventOpcode opcode)
        {
            if (opcode == TraceEventOpcode.Start)
            {
                return TraceEventType.Start;
            }
            if (opcode == TraceEventOpcode.Stop)
            {
                return TraceEventType.Stop;
            }
            if (opcode == TraceEventOpcode.Suspend)
            {
                return TraceEventType.Suspend;
            }
            if (opcode == TraceEventOpcode.Resume)
            {
                return TraceEventType.Resume;
            }
            return TraceEventType.Information;
        }

        internal static TraceEventType GetTraceEventType(byte level)
        {
            return EtwLevelToTraceEventType[level];
        }

        internal static TraceEventType GetTraceEventType(TraceEventLevel level)
        {
            return EtwLevelToTraceEventType[(int) level];
        }

        internal static TraceEventType GetTraceEventType(byte level, byte opcode)
        {
            if (opcode == 0)
            {
                return EtwLevelToTraceEventType[level];
            }
            return EtwOpcodeToTraceEventType((TraceEventOpcode) opcode);
        }

        internal static string LookupSeverity(TraceEventLevel level, TraceEventOpcode opcode)
        {
            switch (opcode)
            {
                case TraceEventOpcode.Start:
                    return "Start";

                case TraceEventOpcode.Stop:
                    return "Stop";

                case TraceEventOpcode.Resume:
                    return "Resume";

                case TraceEventOpcode.Suspend:
                    return "Suspend";

                case TraceEventOpcode.Info:
                    switch (level)
                    {
                        case TraceEventLevel.Critical:
                            return "Critical";

                        case TraceEventLevel.Error:
                            return "Error";

                        case TraceEventLevel.Warning:
                            return "Warning";

                        case TraceEventLevel.Informational:
                            return "Information";

                        case TraceEventLevel.Verbose:
                            return "Verbose";
                    }
                    return level.ToString();
            }
            return opcode.ToString();
        }
    }
}

