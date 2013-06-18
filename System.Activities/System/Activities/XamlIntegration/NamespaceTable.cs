namespace System.Activities.XamlIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Xaml;

    internal class NamespaceTable : IXamlNamespaceResolver
    {
        private Dictionary<string, NamespaceDeclaration> namespacesCache;
        private Stack<List<NamespaceDeclaration>> namespaceStack = new Stack<List<NamespaceDeclaration>>();
        private List<NamespaceDeclaration> tempNamespaceList = new List<NamespaceDeclaration>();

        public void AddNamespace(NamespaceDeclaration xamlNamespace)
        {
            this.tempNamespaceList.Add(xamlNamespace);
            this.namespacesCache = null;
        }

        private void ConstructNamespaceCache()
        {
            Dictionary<string, NamespaceDeclaration> dictionary = new Dictionary<string, NamespaceDeclaration>();
            if ((this.tempNamespaceList != null) && (this.tempNamespaceList.Count > 0))
            {
                foreach (NamespaceDeclaration declaration in this.tempNamespaceList)
                {
                    if (!dictionary.ContainsKey(declaration.Prefix))
                    {
                        dictionary.Add(declaration.Prefix, declaration);
                    }
                }
            }
            foreach (List<NamespaceDeclaration> list in this.namespaceStack)
            {
                foreach (NamespaceDeclaration declaration2 in list)
                {
                    if (!dictionary.ContainsKey(declaration2.Prefix))
                    {
                        dictionary.Add(declaration2.Prefix, declaration2);
                    }
                }
            }
            this.namespacesCache = dictionary;
        }

        public void EnterScope()
        {
            if (this.tempNamespaceList != null)
            {
                this.namespaceStack.Push(this.tempNamespaceList);
                this.tempNamespaceList = new List<NamespaceDeclaration>();
            }
        }

        public void ExitScope()
        {
            if (this.namespaceStack.Pop().Count != 0)
            {
                this.namespacesCache = null;
            }
        }

        public string GetNamespace(string prefix)
        {
            NamespaceDeclaration declaration;
            if (this.namespacesCache == null)
            {
                this.ConstructNamespaceCache();
            }
            if (this.namespacesCache.TryGetValue(prefix, out declaration))
            {
                return declaration.Namespace;
            }
            return null;
        }

        public IEnumerable<NamespaceDeclaration> GetNamespacePrefixes()
        {
            if (this.namespacesCache == null)
            {
                this.ConstructNamespaceCache();
            }
            return this.namespacesCache.Values;
        }

        public void ManageNamespace(XamlReader reader)
        {
            switch (reader.NodeType)
            {
                case XamlNodeType.StartObject:
                case XamlNodeType.GetObject:
                case XamlNodeType.StartMember:
                    this.EnterScope();
                    return;

                case XamlNodeType.EndObject:
                case XamlNodeType.EndMember:
                    this.ExitScope();
                    break;

                case XamlNodeType.Value:
                    break;

                case XamlNodeType.NamespaceDeclaration:
                    this.AddNamespace(reader.Namespace);
                    return;

                default:
                    return;
            }
        }
    }
}

