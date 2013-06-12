namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;

    internal static class IOUtil
    {
        internal static bool FlagTest(MessageEnum flag, MessageEnum target)
        {
            return ((flag & target) == target);
        }

        internal static object[] ReadArgs(__BinaryParser input)
        {
            int num = input.ReadInt32();
            object[] objArray = new object[num];
            for (int i = 0; i < num; i++)
            {
                objArray[i] = ReadWithCode(input);
            }
            return objArray;
        }

        internal static object ReadWithCode(__BinaryParser input)
        {
            InternalPrimitiveTypeE code = (InternalPrimitiveTypeE) input.ReadByte();
            switch (code)
            {
                case InternalPrimitiveTypeE.Null:
                    return null;

                case InternalPrimitiveTypeE.String:
                    return input.ReadString();
            }
            return input.ReadValue(code);
        }

        internal static void WriteStringWithCode(string value, __BinaryWriter sout)
        {
            if (value == null)
            {
                sout.WriteByte(0x11);
            }
            else
            {
                sout.WriteByte(0x12);
                sout.WriteString(value);
            }
        }

        internal static void WriteWithCode(Type type, object value, __BinaryWriter sout)
        {
            if (type == null)
            {
                sout.WriteByte(0x11);
            }
            else if (object.ReferenceEquals(type, Converter.typeofString))
            {
                WriteStringWithCode((string) value, sout);
            }
            else
            {
                InternalPrimitiveTypeE code = Converter.ToCode(type);
                sout.WriteByte((byte) code);
                sout.WriteValue(code, value);
            }
        }
    }
}

