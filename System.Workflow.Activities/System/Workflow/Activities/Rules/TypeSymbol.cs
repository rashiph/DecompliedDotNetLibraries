namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Globalization;
    using System.Runtime;

    internal class TypeSymbol : TypeSymbolBase
    {
        internal readonly int GenericArgCount;
        private string name;
        internal readonly System.Type Type;

        internal TypeSymbol(System.Type type)
        {
            this.Type = type;
            this.name = type.Name;
            if (type.IsGenericType)
            {
                int length = type.Name.LastIndexOf('`');
                if (length > 0)
                {
                    string s = type.Name.Substring(length + 1);
                    this.GenericArgCount = int.Parse(s, CultureInfo.InvariantCulture);
                    this.name = type.Name.Substring(0, length);
                }
            }
        }

        internal bool CanOverload(TypeSymbol typeSym)
        {
            return (typeSym.GenericArgCount != this.GenericArgCount);
        }

        internal override OverloadedTypeSymbol OverloadType(TypeSymbolBase newTypeSymBase)
        {
            OverloadedTypeSymbol symbol = newTypeSymBase as OverloadedTypeSymbol;
            if (symbol != null)
            {
                return symbol.OverloadType(this);
            }
            TypeSymbol typeSym = newTypeSymBase as TypeSymbol;
            if ((typeSym != null) && this.CanOverload(typeSym))
            {
                return new OverloadedTypeSymbol(this.name, this, typeSym);
            }
            return null;
        }

        internal override CodeExpression ParseRootIdentifier(Parser parser, ParserContext parserContext, bool assignIsEquality)
        {
            return parser.ParseRootTypeIdentifier(parserContext, this, assignIsEquality);
        }

        internal override void RecordSymbol(ArrayList list)
        {
            list.Add(this.Type);
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

