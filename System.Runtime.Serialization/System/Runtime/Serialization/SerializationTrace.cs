namespace System.Runtime.Serialization
{
    using System;
    using System.Diagnostics;
    using System.Security;

    internal static class SerializationTrace
    {
        [SecurityCritical]
        private static TraceSource codeGen;

        internal static void TraceInstruction(string instruction)
        {
            CodeGenerationTraceSource.TraceEvent(TraceEventType.Verbose, 0, instruction);
        }

        internal static void WriteInstruction(int lineNumber, string instruction)
        {
            CodeGenerationTraceSource.TraceInformation("{0:00000}: {1}", new object[] { lineNumber, instruction });
        }

        internal static SourceSwitch CodeGenerationSwitch
        {
            get
            {
                return CodeGenerationTraceSource.Switch;
            }
        }

        private static TraceSource CodeGenerationTraceSource
        {
            [SecuritySafeCritical]
            get
            {
                if (codeGen == null)
                {
                    codeGen = new TraceSource("System.Runtime.Serialization.CodeGeneration");
                }
                return codeGen;
            }
        }
    }
}

