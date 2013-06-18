namespace Microsoft.Build.Tasks
{
    using System;
    using System.Collections;
    using System.Text;

    internal sealed class ParseState
    {
        private string namespaceName;
        private Stack namespaceStack = new Stack();
        private int openConditionalDirectives;
        private bool resolvingClass;
        private bool resolvingNamespace;

        internal ParseState()
        {
            this.Reset();
        }

        internal void CloseConditionalDirective()
        {
            this.openConditionalDirectives--;
        }

        internal string ComposeQualifiedClassName(string className)
        {
            StringBuilder builder = new StringBuilder(0x400);
            foreach (string str in this.namespaceStack)
            {
                if ((str != null) && (str.Length > 0))
                {
                    builder.Insert(0, '.');
                    builder.Insert(0, str);
                }
            }
            builder.Append(className);
            return builder.ToString();
        }

        internal void OpenConditionalDirective()
        {
            this.openConditionalDirectives++;
        }

        internal string PopNamespacePart()
        {
            if (this.namespaceStack.Count == 0)
            {
                return null;
            }
            return (string) this.namespaceStack.Pop();
        }

        internal void PushNamespacePart(string namespacePart)
        {
            this.namespaceStack.Push(namespacePart);
        }

        internal void Reset()
        {
            this.resolvingNamespace = false;
            this.resolvingClass = false;
            this.namespaceName = string.Empty;
        }

        internal bool InsideConditionalDirective
        {
            get
            {
                return (this.openConditionalDirectives > 0);
            }
        }

        internal string Namespace
        {
            get
            {
                return this.namespaceName;
            }
            set
            {
                this.namespaceName = value;
            }
        }

        internal bool ResolvingClass
        {
            get
            {
                return this.resolvingClass;
            }
            set
            {
                this.resolvingClass = value;
            }
        }

        internal bool ResolvingNamespace
        {
            get
            {
                return this.resolvingNamespace;
            }
            set
            {
                this.resolvingNamespace = value;
            }
        }
    }
}

