namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class VBConversionBinder : ConvertBinder
    {
        private static readonly int _hash = typeof(VBConversionBinder).GetHashCode();

        public VBConversionBinder(Type T) : base(T, true)
        {
        }

        public override bool Equals(object _other)
        {
            VBConversionBinder binder = _other as VBConversionBinder;
            return ((binder != null) && (this.Type == binder.Type));
        }

        public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            if (IDOUtils.NeedsDeferral(target, null, null))
            {
                return this.Defer(target, new DynamicMetaObject[0]);
            }
            if ((errorSuggestion != null) && !Conversions.CanUserDefinedConvert(target.Value, this.Type))
            {
                return errorSuggestion;
            }
            return new DynamicMetaObject(Expression.Convert(Expression.Call(typeof(Conversions).GetMethod("FallbackUserDefinedConversion"), target.Expression, Expression.Constant(this.Type, typeof(Type))), this.ReturnType), IDOUtils.CreateRestrictions(target, null, null));
        }

        public override int GetHashCode()
        {
            return (_hash ^ this.Type.GetHashCode());
        }
    }
}

