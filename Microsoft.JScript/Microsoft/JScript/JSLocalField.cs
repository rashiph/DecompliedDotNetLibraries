namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;

    public sealed class JSLocalField : JSVariableField
    {
        internal bool debugOn;
        private ArrayList dependents;
        internal IReflect inferred_type;
        internal bool isDefined;
        internal bool isUsedBeforeDefinition;
        internal JSLocalField outerField;
        internal int slotNumber;

        public JSLocalField(string name, RuntimeTypeHandle handle, int slotNumber) : this(name, null, slotNumber, Microsoft.JScript.Missing.Value)
        {
            base.type = new TypeExpression(new ConstantWrapper(Type.GetTypeFromHandle(handle), null));
            this.isDefined = true;
        }

        internal JSLocalField(string name, FunctionScope scope, int slotNumber, object value) : base(name, scope, FieldAttributes.Static | FieldAttributes.Public)
        {
            this.slotNumber = slotNumber;
            this.inferred_type = null;
            this.dependents = null;
            base.value = value;
            this.debugOn = false;
            this.outerField = null;
            this.isDefined = false;
            this.isUsedBeforeDefinition = false;
        }

        internal override IReflect GetInferredType(JSField inference_target)
        {
            if (this.outerField != null)
            {
                return this.outerField.GetInferredType(inference_target);
            }
            if (base.type != null)
            {
                return base.GetInferredType(inference_target);
            }
            if ((this.inferred_type == null) || (this.inferred_type == Typeob.Object))
            {
                return Typeob.Object;
            }
            if ((inference_target != null) && (inference_target != this))
            {
                if (this.dependents == null)
                {
                    this.dependents = new ArrayList();
                }
                this.dependents.Add(inference_target);
            }
            return this.inferred_type;
        }

        public override object GetValue(object obj)
        {
            if (((base.attributeFlags & FieldAttributes.Literal) == FieldAttributes.PrivateScope) || (base.value is FunctionObject))
            {
                while (obj is BlockScope)
                {
                    obj = ((BlockScope) obj).GetParent();
                }
                StackFrame parent = (StackFrame) obj;
                JSLocalField outerField = this.outerField;
                int slotNumber = this.slotNumber;
                while (outerField != null)
                {
                    slotNumber = outerField.slotNumber;
                    parent = (StackFrame) parent.GetParent();
                    outerField = outerField.outerField;
                }
                return parent.localVars[slotNumber];
            }
            return base.value;
        }

        internal void SetInferredType(IReflect ir, AST expr)
        {
            this.isDefined = true;
            if (base.type == null)
            {
                if (this.outerField != null)
                {
                    this.outerField.SetInferredType(ir, expr);
                }
                else
                {
                    if (Microsoft.JScript.Convert.IsPrimitiveNumericTypeFitForDouble(ir))
                    {
                        ir = Typeob.Double;
                    }
                    else if (ir == Typeob.Void)
                    {
                        ir = Typeob.Object;
                    }
                    if (this.inferred_type == null)
                    {
                        this.inferred_type = ir;
                    }
                    else if ((ir != this.inferred_type) && ((!Microsoft.JScript.Convert.IsPrimitiveNumericType(this.inferred_type) || !Microsoft.JScript.Convert.IsPrimitiveNumericType(ir)) || !Microsoft.JScript.Convert.IsPromotableTo(ir, this.inferred_type)))
                    {
                        this.inferred_type = Typeob.Object;
                        if (this.dependents != null)
                        {
                            int num = 0;
                            int count = this.dependents.Count;
                            while (num < count)
                            {
                                ((JSLocalField) this.dependents[num]).SetInferredType(Typeob.Object, null);
                                num++;
                            }
                        }
                    }
                }
            }
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo locale)
        {
            if (base.type != null)
            {
                value = Microsoft.JScript.Convert.Coerce(value, base.type);
            }
            while (obj is BlockScope)
            {
                obj = ((BlockScope) obj).GetParent();
            }
            StackFrame parent = (StackFrame) obj;
            JSLocalField outerField = this.outerField;
            int slotNumber = this.slotNumber;
            while (outerField != null)
            {
                slotNumber = outerField.slotNumber;
                parent = (StackFrame) parent.GetParent();
                outerField = outerField.outerField;
            }
            if (parent.localVars != null)
            {
                parent.localVars[slotNumber] = value;
            }
        }

        public override Type FieldType
        {
            get
            {
                if (base.type != null)
                {
                    return base.FieldType;
                }
                return Microsoft.JScript.Convert.ToType(this.GetInferredType(null));
            }
        }
    }
}

