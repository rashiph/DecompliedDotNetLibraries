namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    public static class Runtime
    {
        private static ModuleBuilder _thunkModuleBuilder;
        private static TypeReferences _typeRefs;
        [DecimalConstant(0, 0, (uint) 1, (uint) 0, (uint) 0)]
        private static readonly decimal DecimalTwoToThe64 = 18446744073709551616M;

        [FileIOPermission(SecurityAction.Assert, Unrestricted=true), ReflectionPermission(SecurityAction.Assert, ReflectionEmit=true)]
        private static ModuleBuilder CreateThunkModuleBuilder()
        {
            AssemblyName name = new AssemblyName {
                Name = "JScript Thunk Assembly"
            };
            ModuleBuilder builder2 = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.Run).DefineDynamicModule("JScript Thunk Module");
            builder2.SetCustomAttribute(new CustomAttributeBuilder(typeof(SecurityTransparentAttribute).GetConstructor(new Type[0]), new object[0]));
            return builder2;
        }

        public static long DoubleToInt64(double val)
        {
            if (double.IsNaN(val))
            {
                return 0L;
            }
            if ((-9.2233720368547758E+18 <= val) && (val <= 9.2233720368547758E+18))
            {
                return (long) val;
            }
            if (double.IsInfinity(val))
            {
                return 0L;
            }
            double num = Math.IEEERemainder(Math.Sign(val) * Math.Floor(Math.Abs(val)), 1.8446744073709552E+19);
            if (num == 9.2233720368547758E+18)
            {
                return -9223372036854775808L;
            }
            return (long) num;
        }

        public static bool Equals(object v1, object v2)
        {
            Equality equality = new Equality(0x35);
            return equality.EvaluateEquality(v1, v2);
        }

        public static long UncheckedDecimalToInt64(decimal val)
        {
            val = decimal.Truncate(val);
            if ((val < -9223372036854775808M) || (9223372036854775807M < val))
            {
                val = decimal.Remainder(val, 18446744073709551616M);
                if (val < -9223372036854775808M)
                {
                    val += 18446744073709551616M;
                }
                else if (val > 9223372036854775807M)
                {
                    val -= 18446744073709551616M;
                }
            }
            return (long) val;
        }

        internal static ModuleBuilder ThunkModuleBuilder
        {
            get
            {
                ModuleBuilder builder = _thunkModuleBuilder;
                if (null == builder)
                {
                    builder = _thunkModuleBuilder = CreateThunkModuleBuilder();
                }
                return builder;
            }
        }

        internal static TypeReferences TypeRefs
        {
            get
            {
                TypeReferences references = _typeRefs;
                if (references == null)
                {
                    references = _typeRefs = new TypeReferences(typeof(Runtime).Module);
                }
                return references;
            }
        }
    }
}

