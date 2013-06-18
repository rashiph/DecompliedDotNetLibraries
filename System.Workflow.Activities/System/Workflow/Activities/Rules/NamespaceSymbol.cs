namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Text;

    internal class NamespaceSymbol : Symbol
    {
        internal readonly int Level;
        private string name;
        internal Dictionary<string, Symbol> NestedSymbols;
        internal readonly NamespaceSymbol Parent;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal NamespaceSymbol()
        {
        }

        internal NamespaceSymbol(string name, NamespaceSymbol parent)
        {
            this.name = name;
            this.Parent = parent;
            this.Level = (parent == null) ? 0 : (parent.Level + 1);
        }

        internal NamespaceSymbol AddNamespace(string nsName)
        {
            if (this.NestedSymbols == null)
            {
                this.NestedSymbols = new Dictionary<string, Symbol>();
            }
            Symbol symbol = null;
            if (!this.NestedSymbols.TryGetValue(nsName, out symbol))
            {
                symbol = new NamespaceSymbol(nsName, this);
                this.NestedSymbols.Add(nsName, symbol);
            }
            return (symbol as NamespaceSymbol);
        }

        internal void AddType(Type type)
        {
            TypeSymbol symbol = new TypeSymbol(type);
            string name = symbol.Name;
            if (this.NestedSymbols == null)
            {
                this.NestedSymbols = new Dictionary<string, Symbol>();
            }
            Symbol symbol2 = null;
            if (this.NestedSymbols.TryGetValue(name, out symbol2))
            {
                OverloadedTypeSymbol symbol3 = symbol2 as OverloadedTypeSymbol;
                if (symbol3 == null)
                {
                    TypeSymbol symbol4 = symbol2 as TypeSymbol;
                    symbol3 = new OverloadedTypeSymbol(name, symbol, symbol4);
                    this.NestedSymbols[name] = symbol3;
                }
                else
                {
                    symbol3.AddLocalType(symbol);
                }
            }
            else
            {
                this.NestedSymbols.Add(name, symbol);
            }
        }

        internal Symbol FindMember(string memberName)
        {
            Symbol symbol = null;
            this.NestedSymbols.TryGetValue(memberName, out symbol);
            return symbol;
        }

        internal ArrayList GetMembers()
        {
            ArrayList list = new ArrayList(this.NestedSymbols.Count);
            foreach (Symbol symbol in this.NestedSymbols.Values)
            {
                symbol.RecordSymbol(list);
            }
            return list;
        }

        internal string GetQualifiedName()
        {
            StringBuilder builder = new StringBuilder();
            Stack<string> stack = new Stack<string>();
            stack.Push(this.Name);
            for (NamespaceSymbol symbol = this.Parent; symbol != null; symbol = symbol.Parent)
            {
                stack.Push(symbol.Name);
            }
            builder.Append(stack.Pop());
            while (stack.Count > 0)
            {
                builder.Append('.');
                builder.Append(stack.Pop());
            }
            return builder.ToString();
        }

        internal override CodeExpression ParseRootIdentifier(Parser parser, ParserContext parserContext, bool assignIsEquality)
        {
            return parser.ParseRootNamespaceIdentifier(parserContext, this, assignIsEquality);
        }

        internal override void RecordSymbol(ArrayList list)
        {
            list.Add(this.Name);
        }

        internal override string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
        }
    }
}

