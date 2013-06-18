namespace Microsoft.Compiler.VisualBasic
{
    using System;

    internal class ScriptScope : IScriptScope
    {
        private static ScriptScope m_empty;

        private ScriptScope()
        {
        }

        public virtual Type FindVariable(string name)
        {
            return null;
        }

        internal static ScriptScope Empty
        {
            get
            {
                if (m_empty == null)
                {
                    m_empty = new ScriptScope();
                }
                return m_empty;
            }
        }
    }
}

