namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel.Compiler;

    internal abstract class ArithmeticLiteral
    {
        protected internal Type m_type;
        private static Dictionary<Type, TypeFlags> supportedTypes = CreateSupportedTypesDictionary();
        private static Dictionary<Type, LiteralMaker> types = CreateTypesDictionary();

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ArithmeticLiteral()
        {
        }

        internal virtual object Add()
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { Messages.NullValue, CodeBinaryOperatorType.Add, this.TypeName }), typeof(void), CodeBinaryOperatorType.Add, this.m_type);
        }

        internal virtual object Add(bool v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Add, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }

        internal virtual object Add(char v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Add, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }

        internal virtual object Add(decimal v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Add, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }

        internal virtual object Add(double v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Add, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }

        internal virtual object Add(int v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Add, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }

        internal virtual object Add(long v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Add, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }

        internal virtual object Add(float v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Add, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }

        internal virtual object Add(string v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Add, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }

        internal virtual object Add(ushort v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Add, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }

        internal virtual object Add(uint v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Add, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }

        internal virtual object Add(ulong v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Add, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }

        internal virtual object Add(ArithmeticLiteral v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.TypeName, CodeBinaryOperatorType.Add, this.TypeName }), v.m_type, CodeBinaryOperatorType.Add, this.m_type);
        }

        internal virtual object BitAnd()
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { Messages.NullValue, CodeBinaryOperatorType.BitwiseAnd, this.TypeName }), typeof(void), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }

        internal virtual object BitAnd(bool v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.TypeName }), v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }

        internal virtual object BitAnd(decimal v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.TypeName }), v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }

        internal virtual object BitAnd(double v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.TypeName }), v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }

        internal virtual object BitAnd(int v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.TypeName }), v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }

        internal virtual object BitAnd(long v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.TypeName }), v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }

        internal virtual object BitAnd(float v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.TypeName }), v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }

        internal virtual object BitAnd(ushort v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.TypeName }), v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }

        internal virtual object BitAnd(uint v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.TypeName }), v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }

        internal virtual object BitAnd(ulong v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.TypeName }), v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }

        internal virtual object BitAnd(ArithmeticLiteral v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.m_type, CodeBinaryOperatorType.BitwiseAnd, this.TypeName }), v.m_type, CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }

        internal virtual object BitOr()
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { Messages.NullValue, CodeBinaryOperatorType.BitwiseOr, this.TypeName }), typeof(void), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }

        internal virtual object BitOr(bool v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.TypeName }), v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }

        internal virtual object BitOr(decimal v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.TypeName }), v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }

        internal virtual object BitOr(double v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.TypeName }), v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }

        internal virtual object BitOr(int v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.TypeName }), v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }

        internal virtual object BitOr(long v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.TypeName }), v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }

        internal virtual object BitOr(float v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.TypeName }), v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }

        internal virtual object BitOr(ushort v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.TypeName }), v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }

        internal virtual object BitOr(uint v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.TypeName }), v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }

        internal virtual object BitOr(ulong v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.TypeName }), v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }

        internal virtual object BitOr(ArithmeticLiteral v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.TypeName, CodeBinaryOperatorType.BitwiseOr, this.TypeName }), v.m_type, CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }

        private static Dictionary<Type, TypeFlags> CreateSupportedTypesDictionary()
        {
            Dictionary<Type, TypeFlags> dictionary = new Dictionary<Type, TypeFlags>(0x1a);
            dictionary.Add(typeof(byte), TypeFlags.UInt16);
            dictionary.Add(typeof(byte?), TypeFlags.Nullable | TypeFlags.UInt16);
            dictionary.Add(typeof(sbyte), TypeFlags.Int32);
            dictionary.Add(typeof(sbyte?), TypeFlags.Nullable | TypeFlags.Int32);
            dictionary.Add(typeof(char), TypeFlags.UInt16);
            dictionary.Add(typeof(char?), TypeFlags.Nullable | TypeFlags.UInt16);
            dictionary.Add(typeof(short), TypeFlags.Int32);
            dictionary.Add(typeof(short?), TypeFlags.Nullable | TypeFlags.Int32);
            dictionary.Add(typeof(int), TypeFlags.Int32);
            dictionary.Add(typeof(int?), TypeFlags.Nullable | TypeFlags.Int32);
            dictionary.Add(typeof(long), TypeFlags.Int64);
            dictionary.Add(typeof(long?), TypeFlags.Nullable | TypeFlags.Int64);
            dictionary.Add(typeof(ushort), TypeFlags.UInt16);
            dictionary.Add(typeof(ushort?), TypeFlags.Nullable | TypeFlags.UInt16);
            dictionary.Add(typeof(uint), TypeFlags.UInt32);
            dictionary.Add(typeof(uint?), TypeFlags.Nullable | TypeFlags.UInt32);
            dictionary.Add(typeof(ulong), TypeFlags.UInt64);
            dictionary.Add(typeof(ulong?), TypeFlags.Nullable | TypeFlags.UInt64);
            dictionary.Add(typeof(float), TypeFlags.Single);
            dictionary.Add(typeof(float?), TypeFlags.Nullable | TypeFlags.Single);
            dictionary.Add(typeof(double), TypeFlags.Double);
            dictionary.Add(typeof(double?), TypeFlags.Nullable | TypeFlags.Double);
            dictionary.Add(typeof(decimal), TypeFlags.Decimal);
            dictionary.Add(typeof(decimal?), TypeFlags.Nullable | TypeFlags.Decimal);
            dictionary.Add(typeof(bool), TypeFlags.Boolean);
            dictionary.Add(typeof(bool?), TypeFlags.Nullable | TypeFlags.Boolean);
            dictionary.Add(typeof(string), TypeFlags.String);
            return dictionary;
        }

        private static Dictionary<Type, LiteralMaker> CreateTypesDictionary()
        {
            Dictionary<Type, LiteralMaker> dictionary = new Dictionary<Type, LiteralMaker>(0x10);
            dictionary.Add(typeof(byte), new LiteralMaker(ArithmeticLiteral.MakeByte));
            dictionary.Add(typeof(sbyte), new LiteralMaker(ArithmeticLiteral.MakeSByte));
            dictionary.Add(typeof(char), new LiteralMaker(ArithmeticLiteral.MakeChar));
            dictionary.Add(typeof(short), new LiteralMaker(ArithmeticLiteral.MakeShort));
            dictionary.Add(typeof(int), new LiteralMaker(ArithmeticLiteral.MakeInt));
            dictionary.Add(typeof(long), new LiteralMaker(ArithmeticLiteral.MakeLong));
            dictionary.Add(typeof(ushort), new LiteralMaker(ArithmeticLiteral.MakeUShort));
            dictionary.Add(typeof(uint), new LiteralMaker(ArithmeticLiteral.MakeUInt));
            dictionary.Add(typeof(ulong), new LiteralMaker(ArithmeticLiteral.MakeULong));
            dictionary.Add(typeof(float), new LiteralMaker(ArithmeticLiteral.MakeFloat));
            dictionary.Add(typeof(double), new LiteralMaker(ArithmeticLiteral.MakeDouble));
            dictionary.Add(typeof(decimal), new LiteralMaker(ArithmeticLiteral.MakeDecimal));
            dictionary.Add(typeof(bool), new LiteralMaker(ArithmeticLiteral.MakeBoolean));
            dictionary.Add(typeof(string), new LiteralMaker(ArithmeticLiteral.MakeString));
            dictionary.Add(typeof(byte?), new LiteralMaker(ArithmeticLiteral.MakeByte));
            dictionary.Add(typeof(sbyte?), new LiteralMaker(ArithmeticLiteral.MakeSByte));
            dictionary.Add(typeof(char?), new LiteralMaker(ArithmeticLiteral.MakeChar));
            dictionary.Add(typeof(short?), new LiteralMaker(ArithmeticLiteral.MakeShort));
            dictionary.Add(typeof(int?), new LiteralMaker(ArithmeticLiteral.MakeInt));
            dictionary.Add(typeof(long?), new LiteralMaker(ArithmeticLiteral.MakeLong));
            dictionary.Add(typeof(ushort?), new LiteralMaker(ArithmeticLiteral.MakeUShort));
            dictionary.Add(typeof(uint?), new LiteralMaker(ArithmeticLiteral.MakeUInt));
            dictionary.Add(typeof(ulong?), new LiteralMaker(ArithmeticLiteral.MakeULong));
            dictionary.Add(typeof(float?), new LiteralMaker(ArithmeticLiteral.MakeFloat));
            dictionary.Add(typeof(double?), new LiteralMaker(ArithmeticLiteral.MakeDouble));
            dictionary.Add(typeof(decimal?), new LiteralMaker(ArithmeticLiteral.MakeDecimal));
            dictionary.Add(typeof(bool?), new LiteralMaker(ArithmeticLiteral.MakeBoolean));
            return dictionary;
        }

        internal virtual object Divide()
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { Messages.NullValue, CodeBinaryOperatorType.Divide, this.TypeName }), typeof(void), CodeBinaryOperatorType.Divide, this.m_type);
        }

        internal virtual object Divide(decimal v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Divide, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Divide, this.m_type);
        }

        internal virtual object Divide(double v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Divide, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Divide, this.m_type);
        }

        internal virtual object Divide(int v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Divide, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Divide, this.m_type);
        }

        internal virtual object Divide(long v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Divide, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Divide, this.m_type);
        }

        internal virtual object Divide(float v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Divide, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Divide, this.m_type);
        }

        internal virtual object Divide(ushort v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Divide, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Divide, this.m_type);
        }

        internal virtual object Divide(uint v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Divide, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Divide, this.m_type);
        }

        internal virtual object Divide(ulong v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Divide, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Divide, this.m_type);
        }

        internal virtual object Divide(ArithmeticLiteral v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.TypeName, CodeBinaryOperatorType.Divide, this.TypeName }), v.m_type, CodeBinaryOperatorType.Divide, this.m_type);
        }

        private static ArithmeticLiteral MakeBoolean(object literalValue)
        {
            return new BooleanArithmeticLiteral((bool) literalValue);
        }

        private static ArithmeticLiteral MakeByte(object literalValue)
        {
            return new UShortArithmeticLiteral((byte) literalValue);
        }

        private static ArithmeticLiteral MakeChar(object literalValue)
        {
            return new CharArithmeticLiteral((char) literalValue);
        }

        private static ArithmeticLiteral MakeDecimal(object literalValue)
        {
            return new DecimalArithmeticLiteral((decimal) literalValue);
        }

        private static ArithmeticLiteral MakeDouble(object literalValue)
        {
            return new DoubleArithmeticLiteral((double) literalValue);
        }

        private static ArithmeticLiteral MakeFloat(object literalValue)
        {
            return new FloatArithmeticLiteral((float) literalValue);
        }

        private static ArithmeticLiteral MakeInt(object literalValue)
        {
            return new IntArithmeticLiteral((int) literalValue);
        }

        internal static ArithmeticLiteral MakeLiteral(Type literalType, object literalValue)
        {
            LiteralMaker maker;
            if (literalValue == null)
            {
                return new NullArithmeticLiteral(literalType);
            }
            if (!types.TryGetValue(literalType, out maker))
            {
                return null;
            }
            return maker(literalValue);
        }

        private static ArithmeticLiteral MakeLong(object literalValue)
        {
            return new LongArithmeticLiteral((long) literalValue);
        }

        private static ArithmeticLiteral MakeSByte(object literalValue)
        {
            return new IntArithmeticLiteral((sbyte) literalValue);
        }

        private static ArithmeticLiteral MakeShort(object literalValue)
        {
            return new IntArithmeticLiteral((short) literalValue);
        }

        private static ArithmeticLiteral MakeString(object literalValue)
        {
            return new StringArithmeticLiteral(literalValue.ToString());
        }

        private static ArithmeticLiteral MakeUInt(object literalValue)
        {
            return new UIntArithmeticLiteral((uint) literalValue);
        }

        private static ArithmeticLiteral MakeULong(object literalValue)
        {
            return new ULongArithmeticLiteral((ulong) literalValue);
        }

        private static ArithmeticLiteral MakeUShort(object literalValue)
        {
            return new UShortArithmeticLiteral((ushort) literalValue);
        }

        internal virtual object Modulus()
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { Messages.NullValue, CodeBinaryOperatorType.Modulus, this.TypeName }), typeof(void), CodeBinaryOperatorType.Modulus, this.m_type);
        }

        internal virtual object Modulus(decimal v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Modulus, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Modulus, this.m_type);
        }

        internal virtual object Modulus(double v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Modulus, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Modulus, this.m_type);
        }

        internal virtual object Modulus(int v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Modulus, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Modulus, this.m_type);
        }

        internal virtual object Modulus(long v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Modulus, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Modulus, this.m_type);
        }

        internal virtual object Modulus(float v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Modulus, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Modulus, this.m_type);
        }

        internal virtual object Modulus(ushort v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Modulus, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Modulus, this.m_type);
        }

        internal virtual object Modulus(uint v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Modulus, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Modulus, this.m_type);
        }

        internal virtual object Modulus(ulong v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Modulus, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Modulus, this.m_type);
        }

        internal virtual object Modulus(ArithmeticLiteral v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.m_type, CodeBinaryOperatorType.Modulus, this.TypeName }), v.m_type, CodeBinaryOperatorType.Modulus, this.m_type);
        }

        internal virtual object Multiply()
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { Messages.NullValue, CodeBinaryOperatorType.Multiply, this.TypeName }), typeof(void), CodeBinaryOperatorType.Multiply, this.m_type);
        }

        internal virtual object Multiply(decimal v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Multiply, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Multiply, this.m_type);
        }

        internal virtual object Multiply(double v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Multiply, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Multiply, this.m_type);
        }

        internal virtual object Multiply(int v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Multiply, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Multiply, this.m_type);
        }

        internal virtual object Multiply(long v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Multiply, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Multiply, this.m_type);
        }

        internal virtual object Multiply(float v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Multiply, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Multiply, this.m_type);
        }

        internal virtual object Multiply(ushort v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Multiply, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Multiply, this.m_type);
        }

        internal virtual object Multiply(uint v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Multiply, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Multiply, this.m_type);
        }

        internal virtual object Multiply(ulong v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Multiply, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Multiply, this.m_type);
        }

        internal virtual object Multiply(ArithmeticLiteral v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.TypeName, CodeBinaryOperatorType.Multiply, this.TypeName }), v.m_type, CodeBinaryOperatorType.Multiply, this.m_type);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static Type ResultType(CodeBinaryOperatorType operation, TypeFlags lhsType, TypeFlags rhsType)
        {
            TypeFlags flags = lhsType | rhsType;
            bool flag = (flags & TypeFlags.Nullable) == TypeFlags.Nullable;
            if (flag)
            {
                flags ^= TypeFlags.Nullable;
            }
            switch (operation)
            {
                case CodeBinaryOperatorType.Add:
                    if ((lhsType != TypeFlags.String) && (rhsType != TypeFlags.String))
                    {
                        break;
                    }
                    return typeof(string);

                case CodeBinaryOperatorType.Subtract:
                case CodeBinaryOperatorType.Multiply:
                case CodeBinaryOperatorType.Divide:
                case CodeBinaryOperatorType.Modulus:
                    break;

                case CodeBinaryOperatorType.BitwiseOr:
                case CodeBinaryOperatorType.BitwiseAnd:
                    switch (flags)
                    {
                        case TypeFlags.UInt16:
                        case TypeFlags.Int32:
                        case (TypeFlags.Int32 | TypeFlags.UInt16):
                            if (!flag)
                            {
                                return typeof(int);
                            }
                            return typeof(int?);

                        case TypeFlags.UInt32:
                        case (TypeFlags.UInt32 | TypeFlags.UInt16):
                            if (!flag)
                            {
                                return typeof(uint);
                            }
                            return typeof(uint?);

                        case (TypeFlags.UInt32 | TypeFlags.Int32):
                        case TypeFlags.Int64:
                        case (TypeFlags.Int64 | TypeFlags.UInt16):
                        case (TypeFlags.Int64 | TypeFlags.Int32):
                        case (TypeFlags.Int64 | TypeFlags.UInt32):
                            if (!flag)
                            {
                                return typeof(long);
                            }
                            return typeof(long?);

                        case TypeFlags.UInt64:
                        case (TypeFlags.UInt64 | TypeFlags.UInt16):
                        case (TypeFlags.UInt64 | TypeFlags.UInt32):
                            if (!flag)
                            {
                                return typeof(ulong);
                            }
                            return typeof(ulong?);

                        case TypeFlags.Boolean:
                            if (!flag)
                            {
                                return typeof(bool);
                            }
                            return typeof(bool?);
                    }
                    goto Label_032A;

                default:
                    goto Label_032A;
            }
            switch (flags)
            {
                case TypeFlags.Double:
                case (TypeFlags.Double | TypeFlags.UInt16):
                case (TypeFlags.Double | TypeFlags.Int32):
                case (TypeFlags.Double | TypeFlags.UInt32):
                case (TypeFlags.Double | TypeFlags.Int64):
                case (TypeFlags.Double | TypeFlags.UInt64):
                case (TypeFlags.Double | TypeFlags.Single):
                    if (!flag)
                    {
                        return typeof(double);
                    }
                    return typeof(double?);

                case (TypeFlags.Single | TypeFlags.UInt64):
                case TypeFlags.Single:
                case (TypeFlags.Single | TypeFlags.UInt16):
                case (TypeFlags.Single | TypeFlags.Int32):
                case (TypeFlags.Single | TypeFlags.UInt32):
                case (TypeFlags.Single | TypeFlags.Int64):
                    if (!flag)
                    {
                        return typeof(float);
                    }
                    return typeof(float?);

                case TypeFlags.UInt16:
                case TypeFlags.Int32:
                case (TypeFlags.Int32 | TypeFlags.UInt16):
                    if (flag)
                    {
                        return typeof(int?);
                    }
                    return typeof(int);

                case TypeFlags.UInt32:
                case (TypeFlags.UInt32 | TypeFlags.UInt16):
                    if (flag)
                    {
                        return typeof(uint?);
                    }
                    return typeof(uint);

                case (TypeFlags.UInt32 | TypeFlags.Int32):
                case TypeFlags.Int64:
                case (TypeFlags.Int64 | TypeFlags.UInt16):
                case (TypeFlags.Int64 | TypeFlags.Int32):
                case (TypeFlags.Int64 | TypeFlags.UInt32):
                    if (flag)
                    {
                        return typeof(long?);
                    }
                    return typeof(long);

                case TypeFlags.UInt64:
                case (TypeFlags.UInt64 | TypeFlags.UInt16):
                case (TypeFlags.UInt64 | TypeFlags.UInt32):
                    if (flag)
                    {
                        return typeof(ulong?);
                    }
                    return typeof(ulong);

                case TypeFlags.Decimal:
                case (TypeFlags.Decimal | TypeFlags.UInt16):
                case (TypeFlags.Decimal | TypeFlags.Int32):
                case (TypeFlags.Decimal | TypeFlags.UInt32):
                case (TypeFlags.Decimal | TypeFlags.Int64):
                case (TypeFlags.Decimal | TypeFlags.UInt64):
                    if (!flag)
                    {
                        return typeof(decimal);
                    }
                    return typeof(decimal?);
            }
        Label_032A:
            return null;
        }

        internal static RuleBinaryExpressionInfo ResultType(CodeBinaryOperatorType operation, Type lhs, CodeExpression lhsExpression, Type rhs, CodeExpression rhsExpression, RuleValidation validator, out ValidationError error)
        {
            TypeFlags flags;
            TypeFlags flags2;
            if (supportedTypes.TryGetValue(lhs, out flags) && supportedTypes.TryGetValue(rhs, out flags2))
            {
                Type resultType = ResultType(operation, flags, flags2);
                if (resultType != null)
                {
                    error = null;
                    return new RuleBinaryExpressionInfo(lhs, rhs, resultType);
                }
                string errorText = string.Format(CultureInfo.CurrentCulture, Messages.ArithOpBadTypes, new object[] { operation.ToString(), (lhs == typeof(NullLiteral)) ? Messages.NullValue : RuleDecompiler.DecompileType(lhs), (rhs == typeof(NullLiteral)) ? Messages.NullValue : RuleDecompiler.DecompileType(rhs) });
                error = new ValidationError(errorText, 0x545);
                return null;
            }
            MethodInfo mi = Literal.MapOperatorToMethod(operation, lhs, lhsExpression, rhs, rhsExpression, validator, out error);
            if (mi != null)
            {
                return new RuleBinaryExpressionInfo(lhs, rhs, mi);
            }
            return null;
        }

        internal virtual object Subtract()
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { Messages.NullValue, CodeBinaryOperatorType.Subtract, this.TypeName }), typeof(void), CodeBinaryOperatorType.Subtract, this.m_type);
        }

        internal virtual object Subtract(decimal v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Subtract, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Subtract, this.m_type);
        }

        internal virtual object Subtract(double v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Subtract, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Subtract, this.m_type);
        }

        internal virtual object Subtract(int v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Subtract, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Subtract, this.m_type);
        }

        internal virtual object Subtract(long v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Subtract, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Subtract, this.m_type);
        }

        internal virtual object Subtract(float v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Subtract, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Subtract, this.m_type);
        }

        internal virtual object Subtract(ushort v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Subtract, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Subtract, this.m_type);
        }

        internal virtual object Subtract(uint v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Subtract, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Subtract, this.m_type);
        }

        internal virtual object Subtract(ulong v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.GetType(), CodeBinaryOperatorType.Subtract, this.TypeName }), v.GetType(), CodeBinaryOperatorType.Subtract, this.m_type);
        }

        internal virtual object Subtract(ArithmeticLiteral v)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, new object[] { v.TypeName, CodeBinaryOperatorType.Subtract, this.TypeName }), v.m_type, CodeBinaryOperatorType.Subtract, this.m_type);
        }

        protected virtual string TypeName
        {
            get
            {
                return this.m_type.FullName;
            }
        }

        internal abstract object Value { get; }

        private delegate ArithmeticLiteral LiteralMaker(object literalValue);

        [Flags]
        private enum TypeFlags
        {
            Boolean = 0x100,
            Decimal = 0x80,
            Double = 0x40,
            Int32 = 2,
            Int64 = 8,
            Nullable = 0x10000,
            Single = 0x20,
            String = 0x800,
            UInt16 = 1,
            UInt32 = 4,
            UInt64 = 0x10
        }
    }
}

