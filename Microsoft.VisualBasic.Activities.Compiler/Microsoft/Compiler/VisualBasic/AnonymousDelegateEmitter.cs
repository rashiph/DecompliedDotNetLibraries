namespace Microsoft.Compiler.VisualBasic
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class AnonymousDelegateEmitter : TypeEmitter
    {
        private Dictionary<string, Type> m_types;

        public AnonymousDelegateEmitter(SymbolMap symbolMap, ModuleBuilder moduleBuilder) : base(symbolMap, moduleBuilder)
        {
            this.m_types = new Dictionary<string, Type>();
        }

        public unsafe Type EmitType(BCSYM_NamedRoot* pSymbol)
        {
            BCITER_CHILD bciter_child;
            TypeBuilder builder = this.DefineType(BCSYM.PNamedRoot((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pSymbol));
            this.m_types.Add(builder.Name, builder);
            ushort* numPtr2 = BCSYM_NamedRoot.GetCompiler(pSymbol)[12][900];
            BCITER_CHILD.Init(&bciter_child, pSymbol, false, false, false);
            for (BCSYM_NamedRoot* rootPtr = BCITER_CHILD.GetNext(&bciter_child); rootPtr != null; rootPtr = BCITER_CHILD.GetNext(&bciter_child))
            {
                if (((byte) ((*(((byte*) rootPtr)) * 3)[&?s_rgBilkindInfo@BCSYM@@1QBUBilkindInfo@@B] & 0x40)) != 0)
                {
                    ushort* numPtr = *((ushort**) (rootPtr + 12));
                    if ((numPtr == numPtr2) || (((numPtr != null) && (numPtr2 != null)) && (((byte) (*(((int*) (numPtr - 6))) == *(((int*) (numPtr2 - 6))))) != 0)))
                    {
                        this.DefineConstructor(builder, (BCSYM_Proc*) rootPtr).SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
                    }
                    else
                    {
                        this.DefineMethod(builder, (BCSYM_Proc*) rootPtr).SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
                    }
                }
            }
            Type type = builder.CreateType();
            this.m_types[builder.Name] = type;
            return type;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public override unsafe bool TryGetType(BCSYM_NamedRoot* pSymbol, ref Type type)
        {
            string key = new string(*((char**) (pSymbol + 12)));
            return this.m_types.TryGetValue(key, out type);
        }
    }
}

