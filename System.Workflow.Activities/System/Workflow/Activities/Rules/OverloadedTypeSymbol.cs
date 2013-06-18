namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;

    internal class OverloadedTypeSymbol : TypeSymbolBase
    {
        private string name;
        internal List<TypeSymbol> TypeSymbols;

        private OverloadedTypeSymbol(string name, List<TypeSymbol> typeSymbols)
        {
            this.TypeSymbols = new List<TypeSymbol>();
            this.name = name;
            this.TypeSymbols = typeSymbols;
        }

        internal OverloadedTypeSymbol(string name, TypeSymbol typeSym1, TypeSymbol typeSym2)
        {
            this.TypeSymbols = new List<TypeSymbol>();
            this.name = name;
            this.AddLocalType(typeSym1);
            this.AddLocalType(typeSym2);
        }

        internal void AddLocalType(TypeSymbol typeSym)
        {
            this.TypeSymbols.Add(typeSym);
        }

        internal override OverloadedTypeSymbol OverloadType(TypeSymbolBase newTypeSymBase)
        {
            List<TypeSymbol> typeSymbols = new List<TypeSymbol>();
            TypeSymbol item = null;
            OverloadedTypeSymbol symbol2 = newTypeSymBase as OverloadedTypeSymbol;
            if (symbol2 != null)
            {
                typeSymbols.AddRange(symbol2.TypeSymbols);
            }
            else
            {
                item = newTypeSymBase as TypeSymbol;
                if (item != null)
                {
                    typeSymbols.Add(item);
                }
            }
            foreach (TypeSymbol symbol3 in this.TypeSymbols)
            {
                foreach (TypeSymbol symbol4 in typeSymbols)
                {
                    if (!symbol4.CanOverload(symbol3))
                    {
                        return null;
                    }
                }
                typeSymbols.Add(symbol3);
            }
            return new OverloadedTypeSymbol(this.name, typeSymbols);
        }

        internal override CodeExpression ParseRootIdentifier(Parser parser, ParserContext parserContext, bool assignIsEquality)
        {
            return parser.ParseRootOverloadedTypeIdentifier(parserContext, this.TypeSymbols, assignIsEquality);
        }

        internal override void RecordSymbol(ArrayList list)
        {
            foreach (TypeSymbol symbol in this.TypeSymbols)
            {
                list.Add(symbol.Type);
            }
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

