namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    internal class CodeDomLoader : IDisposable
    {
        private CodeCompileUnit codeCompileUnit;
        private TypeProvider typeProvider;
        private List<Type> types = new List<Type>();

        internal CodeDomLoader(TypeProvider typeProvider, CodeCompileUnit codeCompileUnit)
        {
            this.typeProvider = typeProvider;
            this.codeCompileUnit = codeCompileUnit;
            this.AddTypes();
        }

        private void AddTypes()
        {
            if ((this.typeProvider != null) && (this.types != null))
            {
                this.types.Clear();
                foreach (CodeNamespace namespace2 in this.codeCompileUnit.Namespaces)
                {
                    foreach (CodeTypeDeclaration declaration in namespace2.Types)
                    {
                        string name = Helper.EnsureTypeName(declaration.Name);
                        if (namespace2.Name.Length > 0)
                        {
                            name = Helper.EnsureTypeName(namespace2.Name) + "." + name;
                        }
                        DesignTimeType item = this.typeProvider.GetType(name, false) as DesignTimeType;
                        if (item == null)
                        {
                            item = new DesignTimeType(null, declaration.Name, namespace2.Imports, namespace2.Name, this.typeProvider);
                            this.types.Add(item);
                            this.typeProvider.AddType(item);
                        }
                        item.AddCodeTypeDeclaration(declaration);
                    }
                }
                Queue queue = new Queue(this.types);
                while (queue.Count != 0)
                {
                    Type type2 = queue.Dequeue() as Type;
                    if (type2.DeclaringType != null)
                    {
                        this.types.Add(type2);
                    }
                    foreach (Type type3 in type2.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
                    {
                        queue.Enqueue(type3);
                    }
                }
            }
        }

        public void Dispose()
        {
            this.RemoveTypes();
            this.typeProvider = null;
            this.types = null;
        }

        internal void Refresh(EventHandler refresher)
        {
            this.RemoveTypes();
            refresher(this.typeProvider, EventArgs.Empty);
            this.AddTypes();
        }

        private void RemoveTypes()
        {
            if ((this.typeProvider != null) && (this.types != null))
            {
                this.typeProvider.RemoveTypes(this.types.ToArray());
                this.types.Clear();
            }
        }
    }
}

