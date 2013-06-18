namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class Constant : AST
    {
        internal FieldAttributes attributes;
        private Completion completion;
        internal CustomAttributeList customAttributes;
        internal JSVariableField field;
        private Lookup identifier;
        internal string name;
        internal AST value;
        private FieldBuilder valueField;

        internal Constant(Context context, Lookup identifier, TypeExpression type, AST value, FieldAttributes attributes, CustomAttributeList customAttributes) : base(context)
        {
            this.attributes = attributes | FieldAttributes.InitOnly;
            this.customAttributes = customAttributes;
            this.completion = new Completion();
            this.identifier = identifier;
            this.name = identifier.ToString();
            this.value = value;
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            while (parent is WithObject)
            {
                parent = parent.GetParent();
            }
            if (parent is ClassScope)
            {
                if (this.name == ((ClassScope) parent).name)
                {
                    identifier.context.HandleError(JSError.CannotUseNameOfClass);
                    this.name = this.name + " const";
                }
                if (attributes == FieldAttributes.PrivateScope)
                {
                    attributes = FieldAttributes.Public;
                }
            }
            else
            {
                if (attributes != FieldAttributes.PrivateScope)
                {
                    base.context.HandleError(JSError.NotInsideClass);
                }
                attributes = FieldAttributes.Public;
            }
            if (((IActivationObject) parent).GetLocalField(this.name) != null)
            {
                identifier.context.HandleError(JSError.DuplicateName, true);
                this.name = this.name + " const";
            }
            if (parent is ActivationObject)
            {
                this.field = ((ActivationObject) parent).AddNewField(this.identifier.ToString(), value, attributes);
            }
            else
            {
                this.field = ((StackFrame) parent).AddNewField(this.identifier.ToString(), value, attributes | FieldAttributes.Static);
            }
            this.field.type = type;
            this.field.customAttributes = customAttributes;
            this.field.originalContext = context;
            if (this.field is JSLocalField)
            {
                ((JSLocalField) this.field).debugOn = this.identifier.context.document.debugOn;
            }
        }

        internal override object Evaluate()
        {
            if (this.value == null)
            {
                this.completion.value = this.field.value;
            }
            else
            {
                this.completion.value = this.value.Evaluate();
            }
            return this.completion;
        }

        internal override AST PartiallyEvaluate()
        {
            this.field.attributeFlags &= ~FieldAttributes.InitOnly;
            this.identifier.PartiallyEvaluateAsReference();
            if (this.field.type != null)
            {
                this.field.type.PartiallyEvaluate();
            }
            base.Globals.ScopeStack.Peek();
            if (this.value == null)
            {
                this.value = new ConstantWrapper(null, base.context);
                this.field.attributeFlags |= FieldAttributes.InitOnly;
                goto Label_017F;
            }
            this.value = this.value.PartiallyEvaluate();
            this.identifier.SetPartialValue(this.value);
            if (this.value is ConstantWrapper)
            {
                object obj2 = this.field.value = this.value.Evaluate();
                if (this.field.type != null)
                {
                    this.field.value = Microsoft.JScript.Convert.Coerce(obj2, this.field.type, true);
                }
                if (this.field.IsStatic && (((obj2 is Type) || (obj2 is ClassScope)) || ((obj2 is TypedArray) || (Microsoft.JScript.Convert.GetTypeCode(obj2) != TypeCode.Object))))
                {
                    this.field.attributeFlags |= FieldAttributes.Literal;
                    goto Label_0128;
                }
            }
            this.field.attributeFlags |= FieldAttributes.InitOnly;
        Label_0128:
            if (this.field.type == null)
            {
                this.field.type = new TypeExpression(new ConstantWrapper(this.value.InferType(null), null));
            }
        Label_017F:
            if ((this.field != null) && (this.field.customAttributes != null))
            {
                this.field.customAttributes.PartiallyEvaluate();
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            if ((this.field.attributeFlags & FieldAttributes.Literal) != FieldAttributes.PrivateScope)
            {
                object obj2 = this.field.value;
                if (((obj2 is Type) || (obj2 is ClassScope)) || (obj2 is TypedArray))
                {
                    this.field.attributeFlags &= ~FieldAttributes.Literal;
                    this.identifier.TranslateToILPreSet(il);
                    this.identifier.TranslateToILSet(il, new ConstantWrapper(obj2, null));
                    this.field.attributeFlags |= FieldAttributes.Literal;
                }
            }
            else
            {
                if (!this.field.IsStatic)
                {
                    FieldBuilder builder = this.valueField = this.field.metaData as FieldBuilder;
                    if (builder != null)
                    {
                        this.field.metaData = ((TypeBuilder) builder.DeclaringType).DefineField(this.name + " value", this.field.type.ToType(), FieldAttributes.Private);
                    }
                }
                this.field.attributeFlags &= ~FieldAttributes.InitOnly;
                this.identifier.TranslateToILPreSet(il);
                this.identifier.TranslateToILSet(il, this.value);
                this.field.attributeFlags |= FieldAttributes.InitOnly;
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            if (this.value != null)
            {
                this.value.TranslateToILInitializer(il);
            }
        }

        internal void TranslateToILInitOnlyInitializers(ILGenerator il)
        {
            FieldBuilder valueField = this.valueField;
            if (valueField != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldfld, (FieldBuilder) this.field.metaData);
                il.Emit(OpCodes.Stfld, valueField);
                this.valueField = (FieldBuilder) this.field.metaData;
                this.field.metaData = valueField;
            }
        }
    }
}

