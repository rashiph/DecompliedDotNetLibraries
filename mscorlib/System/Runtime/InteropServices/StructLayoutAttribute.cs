namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection;
    using System.Security;

    [ComVisible(true), AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited=false)]
    public sealed class StructLayoutAttribute : Attribute
    {
        internal LayoutKind _val;
        public System.Runtime.InteropServices.CharSet CharSet;
        private const int DEFAULT_PACKING_SIZE = 8;
        public int Pack;
        public int Size;

        public StructLayoutAttribute(short layoutKind)
        {
            this._val = (LayoutKind) layoutKind;
        }

        public StructLayoutAttribute(LayoutKind layoutKind)
        {
            this._val = layoutKind;
        }

        internal StructLayoutAttribute(LayoutKind layoutKind, int pack, int size, System.Runtime.InteropServices.CharSet charSet)
        {
            this._val = layoutKind;
            this.Pack = pack;
            this.Size = size;
            this.CharSet = charSet;
        }

        [SecurityCritical]
        internal static Attribute GetCustomAttribute(RuntimeType type)
        {
            if (!IsDefined(type))
            {
                return null;
            }
            int packSize = 0;
            int classSize = 0;
            LayoutKind auto = LayoutKind.Auto;
            switch ((type.Attributes & TypeAttributes.LayoutMask))
            {
                case TypeAttributes.AnsiClass:
                    auto = LayoutKind.Auto;
                    break;

                case TypeAttributes.SequentialLayout:
                    auto = LayoutKind.Sequential;
                    break;

                case TypeAttributes.ExplicitLayout:
                    auto = LayoutKind.Explicit;
                    break;
            }
            System.Runtime.InteropServices.CharSet none = System.Runtime.InteropServices.CharSet.None;
            TypeAttributes attributes2 = type.Attributes & TypeAttributes.CustomFormatClass;
            if (attributes2 == TypeAttributes.AnsiClass)
            {
                none = System.Runtime.InteropServices.CharSet.Ansi;
            }
            else if (attributes2 == TypeAttributes.UnicodeClass)
            {
                none = System.Runtime.InteropServices.CharSet.Unicode;
            }
            else if (attributes2 == TypeAttributes.AutoClass)
            {
                none = System.Runtime.InteropServices.CharSet.Auto;
            }
            type.GetRuntimeModule().MetadataImport.GetClassLayout(type.MetadataToken, out packSize, out classSize);
            if (packSize == 0)
            {
                packSize = 8;
            }
            return new StructLayoutAttribute(auto, packSize, classSize, none);
        }

        internal static bool IsDefined(RuntimeType type)
        {
            return ((!type.IsInterface && !type.HasElementType) && !type.IsGenericParameter);
        }

        public LayoutKind Value
        {
            get
            {
                return this._val;
            }
        }
    }
}

