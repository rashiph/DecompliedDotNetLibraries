namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    public abstract class JSVariableField : JSField
    {
        internal FieldAttributes attributeFlags;
        internal CLSComplianceSpec clsCompliance;
        internal CustomAttributeList customAttributes;
        internal string debuggerName;
        internal object metaData;
        private MethodInfo method;
        private string name;
        internal ScriptObject obj;
        internal Context originalContext;
        internal TypeExpression type;
        internal object value;

        internal JSVariableField(string name, ScriptObject obj, FieldAttributes attributeFlags)
        {
            this.obj = obj;
            this.name = name;
            this.debuggerName = name;
            this.metaData = null;
            if ((attributeFlags & FieldAttributes.FieldAccessMask) == FieldAttributes.PrivateScope)
            {
                attributeFlags |= FieldAttributes.Public;
            }
            this.attributeFlags = attributeFlags;
            this.type = null;
            this.method = null;
            this.value = null;
            this.originalContext = null;
            this.clsCompliance = CLSComplianceSpec.NotAttributed;
        }

        internal void CheckCLSCompliance(bool classIsCLSCompliant)
        {
            if (this.customAttributes != null)
            {
                Microsoft.JScript.CustomAttribute elem = this.customAttributes.GetAttribute(Typeob.CLSCompliantAttribute);
                if (elem != null)
                {
                    this.clsCompliance = elem.GetCLSComplianceValue();
                    this.customAttributes.Remove(elem);
                }
            }
            if (classIsCLSCompliant)
            {
                if (((this.clsCompliance != CLSComplianceSpec.NonCLSCompliant) && (this.type != null)) && !this.type.IsCLSCompliant())
                {
                    this.clsCompliance = CLSComplianceSpec.NonCLSCompliant;
                    if (this.originalContext != null)
                    {
                        this.originalContext.HandleError(JSError.NonCLSCompliantMember);
                    }
                }
            }
            else if (this.clsCompliance == CLSComplianceSpec.CLSCompliant)
            {
                this.originalContext.HandleError(JSError.MemberTypeCLSCompliantMismatch);
            }
        }

        internal MethodInfo GetAsMethod(object obj)
        {
            if (this.method == null)
            {
                this.method = new JSFieldMethod(this, obj);
            }
            return this.method;
        }

        internal override string GetClassFullName()
        {
            if (!(this.obj is ClassScope))
            {
                throw new JScriptException(JSError.InternalError);
            }
            return ((ClassScope) this.obj).GetFullName();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            if (this.customAttributes != null)
            {
                return (object[]) this.customAttributes.Evaluate();
            }
            return new object[0];
        }

        internal virtual IReflect GetInferredType(JSField inference_target)
        {
            if (this.type != null)
            {
                return this.type.ToIReflect();
            }
            return Typeob.Object;
        }

        internal override object GetMetaData()
        {
            return this.metaData;
        }

        internal override PackageScope GetPackage()
        {
            if (!(this.obj is ClassScope))
            {
                throw new JScriptException(JSError.InternalError);
            }
            return ((ClassScope) this.obj).GetPackage();
        }

        internal void WriteCustomAttribute(bool doCRS)
        {
            if (this.metaData is FieldBuilder)
            {
                FieldBuilder metaData = (FieldBuilder) this.metaData;
                if (this.customAttributes != null)
                {
                    CustomAttributeBuilder[] customAttributeBuilders = this.customAttributes.GetCustomAttributeBuilders(false);
                    int index = 0;
                    int length = customAttributeBuilders.Length;
                    while (index < length)
                    {
                        metaData.SetCustomAttribute(customAttributeBuilders[index]);
                        index++;
                    }
                }
                if (this.clsCompliance == CLSComplianceSpec.CLSCompliant)
                {
                    metaData.SetCustomAttribute(new CustomAttributeBuilder(CompilerGlobals.clsCompliantAttributeCtor, new object[] { true }));
                }
                else if (this.clsCompliance == CLSComplianceSpec.NonCLSCompliant)
                {
                    metaData.SetCustomAttribute(new CustomAttributeBuilder(CompilerGlobals.clsCompliantAttributeCtor, new object[] { false }));
                }
                if (doCRS && base.IsStatic)
                {
                    metaData.SetCustomAttribute(new CustomAttributeBuilder(CompilerGlobals.contextStaticAttributeCtor, new object[0]));
                }
            }
        }

        public override FieldAttributes Attributes
        {
            get
            {
                return this.attributeFlags;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                if (this.obj is ClassScope)
                {
                    return ((ClassScope) this.obj).GetTypeBuilderOrEnumBuilder();
                }
                return null;
            }
        }

        public override Type FieldType
        {
            get
            {
                Type type = Typeob.Object;
                if (this.type != null)
                {
                    type = this.type.ToType();
                    if (type == Typeob.Void)
                    {
                        type = Typeob.Object;
                    }
                }
                return type;
            }
        }

        public override string Name
        {
            get
            {
                return this.name;
            }
        }
    }
}

