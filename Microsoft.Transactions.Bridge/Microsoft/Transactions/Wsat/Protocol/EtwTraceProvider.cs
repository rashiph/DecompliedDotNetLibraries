namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Diagnostics;

    internal class EtwTraceProvider
    {
        private Guid controlGuid;
        private EtwTraceCallback etwProc;
        private Guid eventClassGuid;
        private EtwHandle registrationHandle;
        private ulong traceHandle;

        internal EtwTraceProvider(Guid controlGuid, Guid eventClassGuid)
        {
            this.Initialize(controlGuid, eventClassGuid);
        }

        private unsafe uint EtwNotificationCallback(uint requestCode, IntPtr context, IntPtr bufferSize, byte* buffer)
        {
            if (null == buffer)
            {
                return uint.MaxValue;
            }
            if (DebugTrace.Info)
            {
                DebugTrace.Trace(TraceLevel.Info, "EtwNotificationCallback is called!");
            }
            EventTraceHeader* headerPtr = (EventTraceHeader*) buffer;
            switch (requestCode)
            {
                case 4:
                {
                    this.traceHandle = headerPtr->HistoricalContext;
                    uint traceEnableFlags = EtwNativeMethods.GetTraceEnableFlags(this.traceHandle);
                    int traceEnableLevel = EtwNativeMethods.GetTraceEnableLevel(this.traceHandle);
                    if (DebugTrace.Info)
                    {
                        DebugTrace.Trace(TraceLevel.Info, "EtwNotificationCallback: EnableEvents: Current DiagnosticTrace Level {0}", DiagnosticUtility.Level);
                        DebugTrace.Trace(TraceLevel.Info, "EtwNotificationCallback: EnableEvents: flags = {0} , level = {1}", traceEnableFlags, traceEnableLevel);
                    }
                    using (Process process = Process.GetCurrentProcess())
                    {
                        if ((traceEnableFlags == process.Id) && (traceEnableLevel > 0))
                        {
                            DiagnosticUtility.Level = this.LevelFromInt(traceEnableLevel);
                            if (DebugTrace.Info)
                            {
                                DebugTrace.Trace(TraceLevel.Info, "EtwNotificationCallback: New DiagnosticTrace Level {0}", DiagnosticUtility.Level);
                            }
                        }
                        break;
                    }
                }
                case 5:
                    this.traceHandle = 0L;
                    if (DebugTrace.Info)
                    {
                        DebugTrace.Trace(TraceLevel.Info, "EtwNotificationCallback: Disabling Session Handle!!");
                    }
                    break;
            }
            return 0;
        }

        private unsafe void Initialize(Guid ctlGuid, Guid evtClassGuid)
        {
            this.controlGuid = ctlGuid;
            this.eventClassGuid = evtClassGuid;
            TraceGuidRegistration registration = new TraceGuidRegistration();
            this.etwProc = new EtwTraceCallback(this.EtwNotificationCallback);
            registration.Guid = &evtClassGuid;
            registration.RegHandle = null;
            this.registrationHandle = EtwHandle.RegisterTraceGuids(this.etwProc, this.controlGuid, registration);
        }

        private SourceLevels LevelFromInt(int level)
        {
            if (level == 6)
            {
                return SourceLevels.Off;
            }
            if (level == 5)
            {
                return SourceLevels.Verbose;
            }
            if (level == 4)
            {
                return SourceLevels.Information;
            }
            if (level == 3)
            {
                return SourceLevels.Warning;
            }
            if (level == 2)
            {
                return SourceLevels.Error;
            }
            return SourceLevels.Critical;
        }

        internal unsafe uint Trace(MofEvent* evt)
        {
            return EtwNativeMethods.TraceEvent(this.traceHandle, (char*) evt);
        }

        internal bool ShouldTrace
        {
            get
            {
                return (this.traceHandle != 0L);
            }
        }
    }
}

