namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal sealed class JSClosureField : JSVariableField
    {
        internal FieldInfo field;

        internal JSClosureField(FieldInfo field) : base(field.Name, null, field.Attributes | FieldAttributes.Static)
        {
            if (field is JSFieldInfo)
            {
                field = ((JSFieldInfo) field).field;
            }
            this.field = field;
        }

        internal override IReflect GetInferredType(JSField inference_target)
        {
            if (this.field is JSMemberField)
            {
                return ((JSMemberField) this.field).GetInferredType(inference_target);
            }
            return this.field.FieldType;
        }

        internal override object GetMetaData()
        {
            if (this.field is JSField)
            {
                return ((JSField) this.field).GetMetaData();
            }
            return this.field;
        }

        public override object GetValue(object obj)
        {
            if (!(obj is StackFrame))
            {
                throw new JScriptException(JSError.InternalError);
            }
            return this.field.GetValue(((StackFrame) ((StackFrame) obj).engine.ScriptObjectStackTop()).closureInstance);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo locale)
        {
            if (!(obj is StackFrame))
            {
                throw new JScriptException(JSError.InternalError);
            }
            this.field.SetValue(((StackFrame) ((StackFrame) obj).engine.ScriptObjectStackTop()).closureInstance, value, invokeAttr, binder, locale);
        }

        public override Type DeclaringType
        {
            get
            {
                return this.field.DeclaringType;
            }
        }

        public override Type FieldType
        {
            get
            {
                return this.field.FieldType;
            }
        }
    }
}

