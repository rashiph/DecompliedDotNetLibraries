namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable]
    internal static class MetadataArgs
    {
        public static SkipAddresses Skip = new SkipAddresses();

        [Serializable, ComVisible(true)]
        public struct SkipAddresses
        {
            public byte[] ByteArray;
            public System.Reflection.ConstArray ConstArray;
            public System.Reflection.CorElementType CorElementType;
            public System.Reflection.DeclSecurityAttributes DeclSecurityAttributes;
            public System.Reflection.EventAttributes EventAttributes;
            public System.Reflection.FieldAttributes FieldAttributes;
            public System.Guid Guid;
            public int Int32;
            public int[] Int32Array;
            public System.Reflection.MetadataColumnType MetadataColumnType;
            public MetadataFieldOffset[] MetadataFieldOffsetArray;
            public System.Reflection.MethodAttributes MethodAttributes;
            public System.Reflection.MethodImplAttributes MethodImplAttributes;
            public System.Reflection.MethodSemanticsAttributes MethodSemanticsAttributes;
            public System.Reflection.ParameterAttributes ParameterAttributes;
            public System.Reflection.PInvokeAttributes PInvokeAttributes;
            public System.Reflection.PropertyAttributes PropertyAttributes;
            public string String;
            public System.Reflection.TypeAttributes TypeAttributes;
        }
    }
}

