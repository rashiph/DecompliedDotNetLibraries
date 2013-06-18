namespace System.Xaml
{
    using System;
    using System.Runtime.CompilerServices;

    public abstract class XamlReader : IDisposable
    {
        protected XamlReader()
        {
        }

        public void Close()
        {
            ((IDisposable) this).Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            this.IsDisposed = true;
        }

        public abstract bool Read();
        public virtual XamlReader ReadSubtree()
        {
            return new XamlSubreader(this);
        }

        public virtual void Skip()
        {
            switch (this.NodeType)
            {
                case XamlNodeType.StartObject:
                    this.SkipFromTo(XamlNodeType.StartObject, XamlNodeType.EndObject);
                    break;

                case XamlNodeType.StartMember:
                    this.SkipFromTo(XamlNodeType.StartMember, XamlNodeType.EndMember);
                    break;
            }
            this.Read();
        }

        private void SkipFromTo(XamlNodeType startNodeType, XamlNodeType endNodeType)
        {
            int num = 1;
            while (num > 0)
            {
                this.Read();
                XamlNodeType nodeType = this.NodeType;
                if (nodeType == startNodeType)
                {
                    num++;
                }
                else if (nodeType == endNodeType)
                {
                    num--;
                }
            }
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected bool IsDisposed { get; private set; }

        public abstract bool IsEof { get; }

        public abstract XamlMember Member { get; }

        public abstract NamespaceDeclaration Namespace { get; }

        public abstract XamlNodeType NodeType { get; }

        public abstract XamlSchemaContext SchemaContext { get; }

        public abstract XamlType Type { get; }

        public abstract object Value { get; }
    }
}

