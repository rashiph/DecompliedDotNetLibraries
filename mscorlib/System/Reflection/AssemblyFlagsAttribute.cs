namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Assembly, Inherited=false), ComVisible(true)]
    public sealed class AssemblyFlagsAttribute : Attribute
    {
        private AssemblyNameFlags m_flags;

        [Obsolete("This constructor has been deprecated. Please use AssemblyFlagsAttribute(AssemblyNameFlags) instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public AssemblyFlagsAttribute(int assemblyFlags)
        {
            this.m_flags = (AssemblyNameFlags) assemblyFlags;
        }

        public AssemblyFlagsAttribute(AssemblyNameFlags assemblyFlags)
        {
            this.m_flags = assemblyFlags;
        }

        [Obsolete("This constructor has been deprecated. Please use AssemblyFlagsAttribute(AssemblyNameFlags) instead. http://go.microsoft.com/fwlink/?linkid=14202"), CLSCompliant(false)]
        public AssemblyFlagsAttribute(uint flags)
        {
            this.m_flags = (AssemblyNameFlags) flags;
        }

        public int AssemblyFlags
        {
            get
            {
                return (int) this.m_flags;
            }
        }

        [CLSCompliant(false), Obsolete("This property has been deprecated. Please use AssemblyFlags instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public uint Flags
        {
            get
            {
                return (uint) this.m_flags;
            }
        }
    }
}

