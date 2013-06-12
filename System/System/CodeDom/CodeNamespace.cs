namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Threading;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeNamespace : CodeObject
    {
        private CodeTypeDeclarationCollection classes;
        private CodeCommentStatementCollection comments;
        private const int CommentsCollection = 2;
        private CodeNamespaceImportCollection imports;
        private const int ImportsCollection = 1;
        private string name;
        private CodeNamespaceCollection namespaces;
        private int populated;
        private const int TypesCollection = 4;

        public event EventHandler PopulateComments;

        public event EventHandler PopulateImports;

        public event EventHandler PopulateTypes;

        public CodeNamespace()
        {
            this.imports = new CodeNamespaceImportCollection();
            this.comments = new CodeCommentStatementCollection();
            this.classes = new CodeTypeDeclarationCollection();
            this.namespaces = new CodeNamespaceCollection();
        }

        public CodeNamespace(string name)
        {
            this.imports = new CodeNamespaceImportCollection();
            this.comments = new CodeCommentStatementCollection();
            this.classes = new CodeTypeDeclarationCollection();
            this.namespaces = new CodeNamespaceCollection();
            this.Name = name;
        }

        private CodeNamespace(SerializationInfo info, StreamingContext context)
        {
            this.imports = new CodeNamespaceImportCollection();
            this.comments = new CodeCommentStatementCollection();
            this.classes = new CodeTypeDeclarationCollection();
            this.namespaces = new CodeNamespaceCollection();
        }

        public CodeCommentStatementCollection Comments
        {
            get
            {
                if ((this.populated & 2) == 0)
                {
                    this.populated |= 2;
                    if (this.PopulateComments != null)
                    {
                        this.PopulateComments(this, EventArgs.Empty);
                    }
                }
                return this.comments;
            }
        }

        public CodeNamespaceImportCollection Imports
        {
            get
            {
                if ((this.populated & 1) == 0)
                {
                    this.populated |= 1;
                    if (this.PopulateImports != null)
                    {
                        this.PopulateImports(this, EventArgs.Empty);
                    }
                }
                return this.imports;
            }
        }

        public string Name
        {
            get
            {
                if (this.name != null)
                {
                    return this.name;
                }
                return string.Empty;
            }
            set
            {
                this.name = value;
            }
        }

        public CodeTypeDeclarationCollection Types
        {
            get
            {
                if ((this.populated & 4) == 0)
                {
                    this.populated |= 4;
                    if (this.PopulateTypes != null)
                    {
                        this.PopulateTypes(this, EventArgs.Empty);
                    }
                }
                return this.classes;
            }
        }
    }
}

