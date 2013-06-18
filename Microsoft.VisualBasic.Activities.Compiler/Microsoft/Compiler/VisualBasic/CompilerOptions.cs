namespace Microsoft.Compiler.VisualBasic
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    internal sealed class CompilerOptions
    {
        private int[] m_empty;
        private int[] m_ignoreWarnings;
        private OptionCompareSetting m_optionCompare = OptionCompareSetting.Binary;
        private bool m_optionInfer = false;
        private OptionStrictSetting m_optionStrict = OptionStrictSetting.On;
        private bool m_removeIntChecks = false;
        private int[] m_treatWarningsAsErrors;
        private OptionWarningLevelSetting m_warningLevel = OptionWarningLevelSetting.Regular;

        public CompilerOptions()
        {
            int[] numArray = new int[0];
            this.m_ignoreWarnings = numArray;
            this.m_treatWarningsAsErrors = numArray;
            this.m_empty = numArray;
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification="For Internal Partners Only")]
        public int[] IgnoreWarnings
        {
            get
            {
                return this.m_ignoreWarnings;
            }
            set
            {
                if (value != null)
                {
                    int length = value.Length;
                    if (length != 0)
                    {
                        int[] array = new int[length];
                        this.m_ignoreWarnings = array;
                        value.CopyTo(array, 0);
                        return;
                    }
                }
                this.m_ignoreWarnings = this.m_empty;
            }
        }

        public OptionCompareSetting OptionCompare
        {
            get
            {
                return this.m_optionCompare;
            }
            set
            {
                this.m_optionCompare = value;
            }
        }

        public bool OptionInfer
        {
            [return: MarshalAs(UnmanagedType.U1)]
            get
            {
                return this.m_optionInfer;
            }
            [param: MarshalAs(UnmanagedType.U1)]
            set
            {
                this.m_optionInfer = value;
            }
        }

        public OptionStrictSetting OptionStrict
        {
            get
            {
                return this.m_optionStrict;
            }
            set
            {
                this.m_optionStrict = value;
            }
        }

        public bool RemoveIntChecks
        {
            [return: MarshalAs(UnmanagedType.U1)]
            get
            {
                return this.m_removeIntChecks;
            }
            [param: MarshalAs(UnmanagedType.U1)]
            set
            {
                this.m_removeIntChecks = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification="For Internal Partners Only")]
        public int[] TreatWarningsAsErrors
        {
            get
            {
                return this.m_treatWarningsAsErrors;
            }
            set
            {
                if (value != null)
                {
                    int length = value.Length;
                    if (length != 0)
                    {
                        int[] array = new int[length];
                        this.m_treatWarningsAsErrors = array;
                        value.CopyTo(array, 0);
                        return;
                    }
                }
                this.m_treatWarningsAsErrors = this.m_empty;
            }
        }

        public OptionWarningLevelSetting WarningLevel
        {
            get
            {
                return this.m_warningLevel;
            }
            set
            {
                this.m_warningLevel = value;
            }
        }
    }
}

