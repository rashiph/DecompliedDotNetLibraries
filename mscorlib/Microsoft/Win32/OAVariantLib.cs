namespace Microsoft.Win32
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security;

    internal sealed class OAVariantLib
    {
        public const int AlphaBool = 2;
        public const int CalendarHijri = 8;
        internal static readonly Type[] ClassTypes;
        private const int CV_OBJECT = 0x12;
        public const int LocalBool = 0x10;
        public const int NoUserOverride = 4;
        public const int NoValueProp = 1;

        static OAVariantLib()
        {
            Type[] typeArray = new Type[0x17];
            typeArray[0] = typeof(Empty);
            typeArray[1] = typeof(void);
            typeArray[2] = typeof(bool);
            typeArray[3] = typeof(char);
            typeArray[4] = typeof(sbyte);
            typeArray[5] = typeof(byte);
            typeArray[6] = typeof(short);
            typeArray[7] = typeof(ushort);
            typeArray[8] = typeof(int);
            typeArray[9] = typeof(uint);
            typeArray[10] = typeof(long);
            typeArray[11] = typeof(ulong);
            typeArray[12] = typeof(float);
            typeArray[13] = typeof(double);
            typeArray[14] = typeof(string);
            typeArray[15] = typeof(void);
            typeArray[0x10] = typeof(DateTime);
            typeArray[0x11] = typeof(TimeSpan);
            typeArray[0x12] = typeof(object);
            typeArray[0x13] = typeof(decimal);
            typeArray[0x15] = typeof(Missing);
            typeArray[0x16] = typeof(DBNull);
            ClassTypes = typeArray;
        }

        private OAVariantLib()
        {
        }

        [SecurityCritical]
        internal static Variant ChangeType(Variant source, Type targetClass, short options, CultureInfo culture)
        {
            if (targetClass == null)
            {
                throw new ArgumentNullException("targetClass");
            }
            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }
            Variant result = new Variant();
            ChangeTypeEx(ref result, ref source, culture.LCID, targetClass.TypeHandle.Value, GetCVTypeFromClass(targetClass), options);
            return result;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void ChangeTypeEx(ref Variant result, ref Variant source, int lcid, IntPtr typeHandle, int cvType, short flags);
        private static int GetCVTypeFromClass(Type ctype)
        {
            int num = -1;
            for (int i = 0; i < ClassTypes.Length; i++)
            {
                if (ctype.Equals(ClassTypes[i]))
                {
                    num = i;
                    break;
                }
            }
            if (num == -1)
            {
                num = 0x12;
            }
            return num;
        }
    }
}

