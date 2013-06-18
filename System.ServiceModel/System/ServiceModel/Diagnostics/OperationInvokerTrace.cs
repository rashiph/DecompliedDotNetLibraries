namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    internal static class OperationInvokerTrace
    {
        private static TraceSource codeGenSource;
        private static MethodInfo traceInstructionMethod;

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
            get
            {
                if (codeGenSource == null)
                {
                    codeGenSource = new TraceSource("System.ServiceModel.OperationInvoker.CodeGeneration");
                }
                return codeGenSource;
            }
        }

        internal static MethodInfo TraceInstructionMethod
        {
            get
            {
                if (traceInstructionMethod == null)
                {
                    traceInstructionMethod = typeof(OperationInvokerTrace).GetMethod("TraceInstruction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return traceInstructionMethod;
            }
        }
    }
}

