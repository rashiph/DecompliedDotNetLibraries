namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), AttributeUsage(AttributeTargets.Assembly, Inherited=false)]
    public sealed class AssemblyCopyrightAttribute : Attribute
    {
        private string m_copyright;

        public AssemblyCopyrightAttribute(string copyright)
        {
            this.m_copyright = copyright;
        }

        public string Copyright
        {
            get
            {
                return this.m_copyright;
            }
        }
    }
}

