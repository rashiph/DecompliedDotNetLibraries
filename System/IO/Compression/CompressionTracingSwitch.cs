namespace System.IO.Compression
{
    using System;
    using System.Diagnostics;

    internal class CompressionTracingSwitch : Switch
    {
        internal static CompressionTracingSwitch tracingSwitch = new CompressionTracingSwitch("CompressionSwitch", "Compression Library Tracing Switch");

        internal CompressionTracingSwitch(string displayName, string description) : base(displayName, description)
        {
        }

        public static bool Informational
        {
            get
            {
                return (tracingSwitch.SwitchSetting >= 1);
            }
        }

        public static bool Verbose
        {
            get
            {
                return (tracingSwitch.SwitchSetting >= 2);
            }
        }
    }
}

