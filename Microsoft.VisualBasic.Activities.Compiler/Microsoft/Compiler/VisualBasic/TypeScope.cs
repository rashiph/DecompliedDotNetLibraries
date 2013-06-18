namespace Microsoft.Compiler.VisualBasic
{
    using System;
    using System.Runtime.InteropServices;

    internal class TypeScope : ITypeScope
    {
        private static TypeScope m_empty;

        private TypeScope()
        {
        }

        public virtual Type[] FindTypes(string typeName, string nsPrefix)
        {
            return null;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public virtual bool NamespaceExists(string ns)
        {
            return false;
        }

        internal static TypeScope Empty
        {
            get
            {
                if (m_empty == null)
                {
                    m_empty = new TypeScope();
                }
                return m_empty;
            }
        }
    }
}

