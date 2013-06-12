namespace System.Diagnostics
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), AttributeUsage(AttributeTargets.Module | AttributeTargets.Assembly, AllowMultiple=false)]
    public sealed class DebuggableAttribute : Attribute
    {
        private DebuggingModes m_debuggingModes;

        public DebuggableAttribute(DebuggingModes modes)
        {
            this.m_debuggingModes = modes;
        }

        public DebuggableAttribute(bool isJITTrackingEnabled, bool isJITOptimizerDisabled)
        {
            this.m_debuggingModes = DebuggingModes.None;
            if (isJITTrackingEnabled)
            {
                this.m_debuggingModes |= DebuggingModes.Default;
            }
            if (isJITOptimizerDisabled)
            {
                this.m_debuggingModes |= DebuggingModes.DisableOptimizations;
            }
        }

        public DebuggingModes DebuggingFlags
        {
            get
            {
                return this.m_debuggingModes;
            }
        }

        public bool IsJITOptimizerDisabled
        {
            get
            {
                return ((this.m_debuggingModes & DebuggingModes.DisableOptimizations) != DebuggingModes.None);
            }
        }

        public bool IsJITTrackingEnabled
        {
            get
            {
                return ((this.m_debuggingModes & DebuggingModes.Default) != DebuggingModes.None);
            }
        }

        [Flags, ComVisible(true)]
        public enum DebuggingModes
        {
            Default = 1,
            DisableOptimizations = 0x100,
            EnableEditAndContinue = 4,
            IgnoreSymbolStoreSequencePoints = 2,
            None = 0
        }
    }
}

