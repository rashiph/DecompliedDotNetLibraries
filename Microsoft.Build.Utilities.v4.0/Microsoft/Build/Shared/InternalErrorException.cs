namespace Microsoft.Build.Shared
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    internal sealed class InternalErrorException : Exception
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal InternalErrorException()
        {
        }

        internal InternalErrorException(string message) : base("MSB0001: Internal MSBuild Error: " + message)
        {
            ConsiderDebuggerLaunch(message, null);
        }

        private InternalErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        internal InternalErrorException(string message, Exception innerException) : base("MSB0001: Internal MSBuild Error: " + message + ((innerException == null) ? string.Empty : ("\n=============\n" + innerException.ToString() + "\n\n")), innerException)
        {
            ConsiderDebuggerLaunch(message, innerException);
        }

        private static void ConsiderDebuggerLaunch(string message, Exception innerException)
        {
            if (innerException != null)
            {
                innerException.ToString();
            }
            if (Environment.GetEnvironmentVariable("MSBUILDLAUNCHDEBUGGER") != null)
            {
                Debugger.Launch();
            }
        }
    }
}

