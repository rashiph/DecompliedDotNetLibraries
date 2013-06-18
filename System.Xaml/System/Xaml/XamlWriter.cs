namespace System.Xaml
{
    using System;
    using System.Runtime.CompilerServices;

    public abstract class XamlWriter : IDisposable
    {
        protected XamlWriter()
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

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract void WriteEndMember();
        public abstract void WriteEndObject();
        public abstract void WriteGetObject();
        public abstract void WriteNamespace(NamespaceDeclaration namespaceDeclaration);
        public void WriteNode(XamlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            switch (reader.NodeType)
            {
                case XamlNodeType.None:
                    return;

                case XamlNodeType.StartObject:
                    this.WriteStartObject(reader.Type);
                    return;

                case XamlNodeType.GetObject:
                    this.WriteGetObject();
                    return;

                case XamlNodeType.EndObject:
                    this.WriteEndObject();
                    return;

                case XamlNodeType.StartMember:
                    this.WriteStartMember(reader.Member);
                    return;

                case XamlNodeType.EndMember:
                    this.WriteEndMember();
                    return;

                case XamlNodeType.Value:
                    this.WriteValue(reader.Value);
                    return;

                case XamlNodeType.NamespaceDeclaration:
                    this.WriteNamespace(reader.Namespace);
                    return;
            }
            throw new NotImplementedException(System.Xaml.SR.Get("MissingCaseXamlNodes"));
        }

        public abstract void WriteStartMember(XamlMember xamlMember);
        public abstract void WriteStartObject(XamlType type);
        public abstract void WriteValue(object value);

        protected bool IsDisposed { get; private set; }

        public abstract XamlSchemaContext SchemaContext { get; }
    }
}

