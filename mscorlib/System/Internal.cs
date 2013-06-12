namespace System
{
    using System.Collections.Generic;
    using System.Reflection;

    internal static class Internal
    {
        private static void CommonlyUsedGenericInstantiations_HACK()
        {
            Array.Sort<double>(null);
            Array.Sort<int>(null);
            Array.Sort<IntPtr>(null);
            new ArraySegment<byte>(new byte[1], 0, 0);
            new Dictionary<char, object>();
            new Dictionary<Guid, byte>();
            new Dictionary<Guid, object>();
            new Dictionary<Guid, Guid>();
            new Dictionary<short, IntPtr>();
            new Dictionary<int, byte>();
            new Dictionary<int, int>();
            new Dictionary<int, object>();
            new Dictionary<IntPtr, bool>();
            new Dictionary<IntPtr, short>();
            new Dictionary<object, bool>();
            new Dictionary<object, char>();
            new Dictionary<object, Guid>();
            new Dictionary<object, int>();
            new Dictionary<object, long>();
            new Dictionary<uint, WeakReference>();
            new Dictionary<object, uint>();
            new Dictionary<uint, object>();
            new Dictionary<long, object>();
            new Dictionary<MemberTypes, object>();
            new EnumEqualityComparer<MemberTypes>();
            new Dictionary<object, KeyValuePair<object, object>>();
            new Dictionary<KeyValuePair<object, object>, object>();
            NullableHelper_HACK<bool>();
            NullableHelper_HACK<byte>();
            NullableHelper_HACK<char>();
            NullableHelper_HACK<DateTime>();
            NullableHelper_HACK<decimal>();
            NullableHelper_HACK<double>();
            NullableHelper_HACK<Guid>();
            NullableHelper_HACK<short>();
            NullableHelper_HACK<int>();
            NullableHelper_HACK<long>();
            NullableHelper_HACK<float>();
            NullableHelper_HACK<TimeSpan>();
            NullableHelper_HACK<DateTimeOffset>();
            new List<bool>();
            new List<byte>();
            new List<char>();
            new List<DateTime>();
            new List<decimal>();
            new List<double>();
            new List<Guid>();
            new List<short>();
            new List<int>();
            new List<long>();
            new List<TimeSpan>();
            new List<sbyte>();
            new List<float>();
            new List<ushort>();
            new List<uint>();
            new List<ulong>();
            new List<IntPtr>();
            new List<KeyValuePair<object, object>>();
            new List<GCHandle>();
            new List<DateTimeOffset>();
            RuntimeType.RuntimeTypeCache.Prejitinit_HACK();
            new CerArrayList<RuntimeMethodInfo>(0);
            new CerArrayList<RuntimeConstructorInfo>(0);
            new CerArrayList<RuntimePropertyInfo>(0);
            new CerArrayList<RuntimeEventInfo>(0);
            new CerArrayList<RuntimeFieldInfo>(0);
            new CerArrayList<RuntimeType>(0);
            new KeyValuePair<char, ushort>('\0', 0);
            new KeyValuePair<ushort, double>(0, double.MinValue);
            new KeyValuePair<object, int>(string.Empty, -2147483648);
            new KeyValuePair<int, int>(-2147483648, -2147483648);
            SZArrayHelper_HACK<bool>(null);
            SZArrayHelper_HACK<byte>(null);
            SZArrayHelper_HACK<DateTime>(null);
            SZArrayHelper_HACK<decimal>(null);
            SZArrayHelper_HACK<double>(null);
            SZArrayHelper_HACK<Guid>(null);
            SZArrayHelper_HACK<short>(null);
            SZArrayHelper_HACK<int>(null);
            SZArrayHelper_HACK<long>(null);
            SZArrayHelper_HACK<TimeSpan>(null);
            SZArrayHelper_HACK<sbyte>(null);
            SZArrayHelper_HACK<float>(null);
            SZArrayHelper_HACK<ushort>(null);
            SZArrayHelper_HACK<uint>(null);
            SZArrayHelper_HACK<ulong>(null);
            SZArrayHelper_HACK<DateTimeOffset>(null);
            SZArrayHelper_HACK<CustomAttributeTypedArgument>(null);
            SZArrayHelper_HACK<CustomAttributeNamedArgument>(null);
        }

        private static T NullableHelper_HACK<T>() where T: struct
        {
            Nullable.Compare<T>(null, null);
            Nullable.Equals<T>(null, null);
            T? nullable = null;
            return nullable.GetValueOrDefault();
        }

        private static void SZArrayHelper_HACK<T>(SZArrayHelper oSZArrayHelper)
        {
            oSZArrayHelper.get_Count<T>();
            oSZArrayHelper.get_Item<T>(0);
            oSZArrayHelper.GetEnumerator<T>();
        }
    }
}

