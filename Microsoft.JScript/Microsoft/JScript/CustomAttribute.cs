namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration.Assemblies;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class CustomAttribute : AST
    {
        private ASTList args;
        private AST ctor;
        private ArrayList namedArgFields;
        private ArrayList namedArgFieldValues;
        private ArrayList namedArgProperties;
        private ArrayList namedArgPropertyValues;
        private ArrayList positionalArgValues;
        internal bool raiseToPropertyLevel;
        private AST target;
        internal object type;

        internal CustomAttribute(Context context, AST func, ASTList args) : base(context)
        {
            this.ctor = func;
            this.args = args;
            this.target = null;
            this.type = null;
            this.positionalArgValues = new ArrayList();
            this.namedArgFields = new ArrayList();
            this.namedArgFieldValues = new ArrayList();
            this.namedArgProperties = new ArrayList();
            this.namedArgPropertyValues = new ArrayList();
            this.raiseToPropertyLevel = false;
        }

        private static bool CheckForCustomAttribute(IList<CustomAttributeData> attributes, Type caType)
        {
            Type type = Globals.TypeRefs.ToReferenceContext(caType);
            foreach (CustomAttributeData data in attributes)
            {
                if (data.Constructor.DeclaringType == type)
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckIfTargetOK(object caType)
        {
            if (caType != null)
            {
                AttributeTargets validOn = 0;
                Type target = caType as Type;
                if (target != null)
                {
                    validOn = ((AttributeUsageAttribute) GetCustomAttributes(target, typeof(AttributeUsageAttribute), true)[0]).ValidOn;
                }
                else
                {
                    validOn = ((ClassScope) caType).owner.validOn;
                }
                object obj2 = this.target;
                Class class2 = obj2 as Class;
                if (class2 != null)
                {
                    if (class2.isInterface)
                    {
                        if ((validOn & AttributeTargets.Interface) != 0)
                        {
                            return true;
                        }
                    }
                    else if (class2 is EnumDeclaration)
                    {
                        if ((validOn & AttributeTargets.Enum) != 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if ((validOn & AttributeTargets.Class) != 0)
                        {
                            if (target == typeof(AttributeUsageAttribute))
                            {
                                if (this.positionalArgValues.Count > 0)
                                {
                                    object obj3 = this.positionalArgValues[0];
                                    if (obj3 is AttributeTargets)
                                    {
                                        class2.validOn = (AttributeTargets) obj3;
                                    }
                                }
                                int num = 0;
                                int count = this.namedArgProperties.Count;
                                while (num < count)
                                {
                                    PropertyInfo info = this.namedArgProperties[num] as PropertyInfo;
                                    if (info.Name == "AllowMultiple")
                                    {
                                        class2.allowMultiple = (bool) this.namedArgPropertyValues[num];
                                    }
                                    num++;
                                }
                            }
                            return true;
                        }
                        if (target.FullName == "System.NonSerializedAttribute")
                        {
                            class2.attributes &= ~TypeAttributes.Serializable;
                            return false;
                        }
                    }
                    base.context.HandleError(JSError.InvalidCustomAttributeTarget, GetTypeName(caType));
                    return false;
                }
                FunctionDeclaration declaration = obj2 as FunctionDeclaration;
                if (declaration != null)
                {
                    if (((validOn & AttributeTargets.Property) != 0) && (declaration.enclosingProperty != null))
                    {
                        if ((declaration.enclosingProperty.getter == null) || (((JSFieldMethod) declaration.enclosingProperty.getter).func == declaration.func))
                        {
                            this.raiseToPropertyLevel = true;
                            return true;
                        }
                        base.context.HandleError(JSError.PropertyLevelAttributesMustBeOnGetter);
                        return false;
                    }
                    if (((validOn & AttributeTargets.Method) != 0) && declaration.isMethod)
                    {
                        return true;
                    }
                    if (((validOn & AttributeTargets.Constructor) != 0) && declaration.func.isConstructor)
                    {
                        return true;
                    }
                    base.context.HandleError(JSError.InvalidCustomAttributeTarget, GetTypeName(caType));
                    return false;
                }
                if ((obj2 is VariableDeclaration) || (obj2 is Constant))
                {
                    if ((validOn & AttributeTargets.Field) != 0)
                    {
                        return true;
                    }
                    base.context.HandleError(JSError.InvalidCustomAttributeTarget, GetTypeName(caType));
                    return false;
                }
                if ((obj2 is AssemblyCustomAttributeList) && ((validOn & AttributeTargets.Assembly) != 0))
                {
                    return true;
                }
                if ((obj2 == null) && ((validOn & AttributeTargets.Parameter) != 0))
                {
                    return true;
                }
                base.context.HandleError(JSError.InvalidCustomAttributeTarget, GetTypeName(caType));
            }
            return false;
        }

        private void ConvertClassScopesAndEnumWrappers(ArrayList vals)
        {
            int num = 0;
            int count = vals.Count;
            while (num < count)
            {
                ClassScope scope = vals[num] as ClassScope;
                if (scope != null)
                {
                    vals[num] = scope.GetTypeBuilder();
                }
                else
                {
                    EnumWrapper wrapper = vals[num] as EnumWrapper;
                    if (wrapper != null)
                    {
                        vals[num] = wrapper.ToNumericValue();
                    }
                }
                num++;
            }
        }

        private void ConvertFieldAndPropertyInfos(ArrayList vals)
        {
            int num = 0;
            int count = vals.Count;
            while (num < count)
            {
                JSField field = vals[num] as JSField;
                if (field != null)
                {
                    vals[num] = field.GetMetaData();
                }
                else
                {
                    JSProperty property = vals[num] as JSProperty;
                    if (property != null)
                    {
                        vals[num] = property.metaData;
                    }
                }
                num++;
            }
        }

        private static ushort DaysSince2000()
        {
            TimeSpan span = (TimeSpan) (DateTime.Now - new DateTime(0x7d0, 1, 1));
            return (ushort) span.Days;
        }

        internal override object Evaluate()
        {
            ConstructorInfo member = (ConstructorInfo) ((Binding) this.ctor).member;
            ParameterInfo[] parameters = member.GetParameters();
            int length = parameters.Length;
            for (int i = this.positionalArgValues.Count; i < length; i++)
            {
                this.positionalArgValues.Add(Microsoft.JScript.Convert.CoerceT(null, parameters[i].ParameterType));
            }
            object[] array = new object[length];
            this.positionalArgValues.CopyTo(0, array, 0, length);
            object obj2 = member.Invoke(BindingFlags.ExactBinding, null, array, null);
            int num3 = 0;
            int count = this.namedArgProperties.Count;
            while (num3 < count)
            {
                JSProperty property = this.namedArgProperties[num3] as JSProperty;
                if (property != null)
                {
                    property.SetValue(obj2, Microsoft.JScript.Convert.Coerce(this.namedArgPropertyValues[num3], property.PropertyIR()), null);
                }
                else
                {
                    ((PropertyInfo) this.namedArgProperties[num3]).SetValue(obj2, this.namedArgPropertyValues[num3], null);
                }
                num3++;
            }
            int num5 = 0;
            int num6 = this.namedArgFields.Count;
            while (num5 < num6)
            {
                JSVariableField field = this.namedArgFields[num5] as JSVariableField;
                if (field != null)
                {
                    field.SetValue(obj2, Microsoft.JScript.Convert.Coerce(this.namedArgFieldValues[num5], field.GetInferredType(null)));
                }
                else
                {
                    ((FieldInfo) this.namedArgFields[num5]).SetValue(obj2, this.namedArgFieldValues[num5]);
                }
                num5++;
            }
            return obj2;
        }

        private static object[] ExtractCustomAttribute(IList<CustomAttributeData> attributes, Type caType)
        {
            Type type = Globals.TypeRefs.ToReferenceContext(caType);
            foreach (CustomAttributeData data in attributes)
            {
                if (data.Constructor.DeclaringType == type)
                {
                    ArrayList list = new ArrayList();
                    foreach (CustomAttributeTypedArgument argument in data.ConstructorArguments)
                    {
                        list.Add(GetCustomAttributeValue(argument));
                    }
                    object target = Activator.CreateInstance(caType, list.ToArray());
                    foreach (CustomAttributeNamedArgument argument2 in data.NamedArguments)
                    {
                        caType.InvokeMember(argument2.MemberInfo.Name, BindingFlags.SetProperty | BindingFlags.SetField | BindingFlags.Public | BindingFlags.Instance, null, target, new object[] { GetCustomAttributeValue(argument2.TypedValue) }, null, CultureInfo.InvariantCulture, null);
                    }
                    return new object[] { target };
                }
            }
            return new object[0];
        }

        internal CLSComplianceSpec GetCLSComplianceValue()
        {
            if (!((bool) this.positionalArgValues[0]))
            {
                return CLSComplianceSpec.NonCLSCompliant;
            }
            return CLSComplianceSpec.CLSCompliant;
        }

        internal CustomAttributeBuilder GetCustomAttribute()
        {
            ConstructorInfo member = (ConstructorInfo) ((Binding) this.ctor).member;
            ParameterInfo[] parameters = member.GetParameters();
            int length = parameters.Length;
            if (member is JSConstructor)
            {
                member = ((JSConstructor) member).GetConstructorInfo(base.compilerGlobals);
            }
            this.ConvertClassScopesAndEnumWrappers(this.positionalArgValues);
            this.ConvertClassScopesAndEnumWrappers(this.namedArgPropertyValues);
            this.ConvertClassScopesAndEnumWrappers(this.namedArgFieldValues);
            this.ConvertFieldAndPropertyInfos(this.namedArgProperties);
            this.ConvertFieldAndPropertyInfos(this.namedArgFields);
            for (int i = this.positionalArgValues.Count; i < length; i++)
            {
                this.positionalArgValues.Add(Microsoft.JScript.Convert.CoerceT(null, parameters[i].ParameterType));
            }
            object[] array = new object[length];
            this.positionalArgValues.CopyTo(0, array, 0, length);
            PropertyInfo[] infoArray2 = new PropertyInfo[this.namedArgProperties.Count];
            this.namedArgProperties.CopyTo(infoArray2);
            object[] objArray2 = new object[this.namedArgPropertyValues.Count];
            this.namedArgPropertyValues.CopyTo(objArray2);
            FieldInfo[] infoArray3 = new FieldInfo[this.namedArgFields.Count];
            this.namedArgFields.CopyTo(infoArray3);
            object[] objArray3 = new object[this.namedArgFieldValues.Count];
            this.namedArgFieldValues.CopyTo(objArray3);
            return new CustomAttributeBuilder(member, array, infoArray2, objArray2, infoArray3, objArray3);
        }

        internal static object[] GetCustomAttributes(Assembly target, Type caType, bool inherit)
        {
            if (!target.ReflectionOnly)
            {
                return target.GetCustomAttributes(caType, inherit);
            }
            return ExtractCustomAttribute(CustomAttributeData.GetCustomAttributes(target), caType);
        }

        internal static object[] GetCustomAttributes(MemberInfo target, Type caType, bool inherit)
        {
            if (!(target.GetType().Assembly == typeof(Microsoft.JScript.CustomAttribute).Assembly) && target.Module.Assembly.ReflectionOnly)
            {
                return ExtractCustomAttribute(CustomAttributeData.GetCustomAttributes(target), caType);
            }
            return target.GetCustomAttributes(caType, inherit);
        }

        internal static object[] GetCustomAttributes(Module target, Type caType, bool inherit)
        {
            if (!target.Assembly.ReflectionOnly)
            {
                return target.GetCustomAttributes(caType, inherit);
            }
            return ExtractCustomAttribute(CustomAttributeData.GetCustomAttributes(target), caType);
        }

        internal static object[] GetCustomAttributes(ParameterInfo target, Type caType, bool inherit)
        {
            if (!(target.GetType().Assembly == typeof(Microsoft.JScript.CustomAttribute).Assembly) && target.Member.Module.Assembly.ReflectionOnly)
            {
                return ExtractCustomAttribute(CustomAttributeData.GetCustomAttributes(target), caType);
            }
            return target.GetCustomAttributes(caType, inherit);
        }

        private static object GetCustomAttributeValue(CustomAttributeTypedArgument arg)
        {
            Type argumentType = arg.ArgumentType;
            if (argumentType.IsEnum)
            {
                return Enum.ToObject(Type.GetType(argumentType.FullName), arg.Value);
            }
            return arg.Value;
        }

        internal object GetTypeIfAttributeHasToBeUnique()
        {
            Type target = this.type as Type;
            if (target != null)
            {
                object[] objArray = GetCustomAttributes(target, typeof(AttributeUsageAttribute), false);
                if ((objArray.Length > 0) && !((AttributeUsageAttribute) objArray[0]).AllowMultiple)
                {
                    return target;
                }
                return null;
            }
            if (!((ClassScope) this.type).owner.allowMultiple)
            {
                return this.type;
            }
            return null;
        }

        private static string GetTypeName(object t)
        {
            Type type = t as Type;
            if (type != null)
            {
                return type.FullName;
            }
            return ((ClassScope) t).GetFullName();
        }

        internal static bool IsDefined(MemberInfo target, Type caType, bool inherit)
        {
            if (!(target.GetType().Assembly == typeof(Microsoft.JScript.CustomAttribute).Assembly) && target.Module.Assembly.ReflectionOnly)
            {
                return CheckForCustomAttribute(CustomAttributeData.GetCustomAttributes(target), caType);
            }
            return target.IsDefined(caType, inherit);
        }

        internal static bool IsDefined(ParameterInfo target, Type caType, bool inherit)
        {
            if (!(target.GetType().Assembly == typeof(Microsoft.JScript.CustomAttribute).Assembly) && target.Member.Module.Assembly.ReflectionOnly)
            {
                return CheckForCustomAttribute(CustomAttributeData.GetCustomAttributes(target), caType);
            }
            return target.IsDefined(caType, inherit);
        }

        internal bool IsExpandoAttribute()
        {
            Lookup ctor = this.ctor as Lookup;
            return ((ctor != null) && (ctor.Name == "expando"));
        }

        private Version ParseVersion(string vString)
        {
            ushort major = 1;
            ushort minor = 0;
            ushort build = 0;
            ushort revision = 0;
            try
            {
                int length = vString.Length;
                int index = vString.IndexOf('.', 0);
                if (index < 0)
                {
                    throw new Exception();
                }
                major = ushort.Parse(vString.Substring(0, index), CultureInfo.InvariantCulture);
                int num7 = vString.IndexOf('.', index + 1);
                if (num7 < (index + 1))
                {
                    minor = ushort.Parse(vString.Substring(index + 1, (length - index) - 1), CultureInfo.InvariantCulture);
                }
                else
                {
                    minor = ushort.Parse(vString.Substring(index + 1, (num7 - index) - 1), CultureInfo.InvariantCulture);
                    if (vString[num7 + 1] == '*')
                    {
                        build = DaysSince2000();
                        revision = SecondsSinceMidnight();
                    }
                    else
                    {
                        int num8 = vString.IndexOf('.', num7 + 1);
                        if (num8 < (num7 + 1))
                        {
                            build = ushort.Parse(vString.Substring(num7 + 1, (length - num7) - 1), CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            build = ushort.Parse(vString.Substring(num7 + 1, (num8 - num7) - 1), CultureInfo.InvariantCulture);
                            if (vString[num8 + 1] == '*')
                            {
                                revision = SecondsSinceMidnight();
                            }
                            else
                            {
                                revision = ushort.Parse(vString.Substring(num8 + 1, (length - num8) - 1), CultureInfo.InvariantCulture);
                            }
                        }
                    }
                }
            }
            catch
            {
                this.args[0].context.HandleError(JSError.NotValidVersionString);
            }
            return new Version(major, minor, build, revision);
        }

        internal override AST PartiallyEvaluate()
        {
            this.ctor = this.ctor.PartiallyEvaluateAsCallable();
            ASTList args = new ASTList(this.args.context);
            ASTList list2 = new ASTList(this.args.context);
            int num = 0;
            int count = this.args.count;
            while (num < count)
            {
                AST ast = this.args[num];
                Assign elem = ast as Assign;
                if (elem != null)
                {
                    elem.rhside = elem.rhside.PartiallyEvaluate();
                    list2.Append(elem);
                }
                else
                {
                    args.Append(ast.PartiallyEvaluate());
                }
                num++;
            }
            int num3 = args.count;
            IReflect[] argIRs = new IReflect[num3];
            for (int i = 0; i < num3; i++)
            {
                AST ast2 = args[i];
                if (ast2 is ConstantWrapper)
                {
                    object argument = ast2.Evaluate();
                    if ((argIRs[i] = TypeOfArgument(argument)) == null)
                    {
                        goto Label_0120;
                    }
                    this.positionalArgValues.Add(argument);
                    continue;
                }
                if ((ast2 is ArrayLiteral) && ((ArrayLiteral) ast2).IsOkToUseInCustomAttribute())
                {
                    argIRs[i] = Typeob.ArrayObject;
                    this.positionalArgValues.Add(ast2.Evaluate());
                    continue;
                }
            Label_0120:
                ast2.context.HandleError(JSError.InvalidCustomAttributeArgument);
                return null;
            }
            this.type = this.ctor.ResolveCustomAttribute(args, argIRs, this.target);
            if (this.type == null)
            {
                return null;
            }
            if (Microsoft.JScript.Convert.IsPromotableTo((IReflect) this.type, Typeob.CodeAccessSecurityAttribute))
            {
                base.context.HandleError(JSError.CannotUseStaticSecurityAttribute);
                return null;
            }
            ParameterInfo[] parameters = ((ConstructorInfo) ((Binding) this.ctor).member).GetParameters();
            int num5 = 0;
            int num6 = this.positionalArgValues.Count;
            foreach (ParameterInfo info2 in parameters)
            {
                IReflect reflect = (info2 is ParameterDeclaration) ? ((ParameterDeclaration) info2).ParameterIReflect : info2.ParameterType;
                if (num5 < num6)
                {
                    object obj3 = this.positionalArgValues[num5];
                    this.positionalArgValues[num5] = Microsoft.JScript.Convert.Coerce(obj3, reflect, obj3 is ArrayObject);
                    num5++;
                }
                else
                {
                    object defaultParameterValue;
                    if (TypeReferences.GetDefaultParameterValue(info2) == System.Convert.DBNull)
                    {
                        defaultParameterValue = Microsoft.JScript.Convert.Coerce(null, reflect);
                    }
                    else
                    {
                        defaultParameterValue = TypeReferences.GetDefaultParameterValue(info2);
                    }
                    this.positionalArgValues.Add(defaultParameterValue);
                }
            }
            int num7 = 0;
            int num8 = list2.count;
            while (num7 < num8)
            {
                Assign assign2 = (Assign) list2[num7];
                if ((assign2.lhside is Lookup) && ((assign2.rhside is ConstantWrapper) || ((assign2.rhside is ArrayLiteral) && ((ArrayLiteral) assign2.rhside).IsOkToUseInCustomAttribute())))
                {
                    object obj5 = assign2.rhside.Evaluate();
                    IReflect reflect2 = null;
                    if ((obj5 is ArrayObject) || (((reflect2 = TypeOfArgument(obj5)) != null) && (reflect2 != Typeob.Object)))
                    {
                        string name = ((Lookup) assign2.lhside).Name;
                        MemberInfo[] member = ((IReflect) this.type).GetMember(name, BindingFlags.Public | BindingFlags.Instance);
                        if ((member == null) || (member.Length == 0))
                        {
                            assign2.context.HandleError(JSError.NoSuchMember);
                            return null;
                        }
                        if (member.Length == 1)
                        {
                            MemberInfo info3 = member[0];
                            if (info3 is FieldInfo)
                            {
                                FieldInfo info4 = (FieldInfo) info3;
                                if (info4.IsLiteral || info4.IsInitOnly)
                                {
                                    goto Label_04B6;
                                }
                                try
                                {
                                    IReflect reflect3 = (info4 is JSVariableField) ? ((JSVariableField) info4).GetInferredType(null) : info4.FieldType;
                                    obj5 = Microsoft.JScript.Convert.Coerce(obj5, reflect3, obj5 is ArrayObject);
                                    this.namedArgFields.Add(info3);
                                    this.namedArgFieldValues.Add(obj5);
                                    goto Label_04C9;
                                }
                                catch (JScriptException)
                                {
                                    assign2.rhside.context.HandleError(JSError.TypeMismatch);
                                    return null;
                                }
                            }
                            if (info3 is PropertyInfo)
                            {
                                PropertyInfo prop = (PropertyInfo) info3;
                                MethodInfo setMethod = JSProperty.GetSetMethod(prop, false);
                                if (setMethod != null)
                                {
                                    ParameterInfo[] infoArray3 = setMethod.GetParameters();
                                    if ((infoArray3 != null) && (infoArray3.Length == 1))
                                    {
                                        try
                                        {
                                            IReflect reflect4 = (infoArray3[0] is ParameterDeclaration) ? ((ParameterDeclaration) infoArray3[0]).ParameterIReflect : infoArray3[0].ParameterType;
                                            obj5 = Microsoft.JScript.Convert.Coerce(obj5, reflect4, obj5 is ArrayObject);
                                            this.namedArgProperties.Add(info3);
                                            this.namedArgPropertyValues.Add(obj5);
                                            goto Label_04C9;
                                        }
                                        catch (JScriptException)
                                        {
                                            assign2.rhside.context.HandleError(JSError.TypeMismatch);
                                            return null;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            Label_04B6:
                assign2.context.HandleError(JSError.InvalidCustomAttributeArgument);
                return null;
            Label_04C9:
                num7++;
            }
            if (!this.CheckIfTargetOK(this.type))
            {
                return null;
            }
            try
            {
                Type type = this.type as Type;
                if ((type != null) && (this.target is AssemblyCustomAttributeList))
                {
                    if (type.FullName == "System.Reflection.AssemblyAlgorithmIdAttribute")
                    {
                        if (this.positionalArgValues.Count > 0)
                        {
                            base.Engine.Globals.assemblyHashAlgorithm = (AssemblyHashAlgorithm) Microsoft.JScript.Convert.CoerceT(this.positionalArgValues[0], typeof(AssemblyHashAlgorithm));
                        }
                        return null;
                    }
                    if (type.FullName == "System.Reflection.AssemblyCultureAttribute")
                    {
                        if (this.positionalArgValues.Count > 0)
                        {
                            string str2 = Microsoft.JScript.Convert.ToString(this.positionalArgValues[0]);
                            if ((base.Engine.PEFileKind != PEFileKinds.Dll) && (str2.Length > 0))
                            {
                                base.context.HandleError(JSError.ExecutablesCannotBeLocalized);
                                return null;
                            }
                            base.Engine.Globals.assemblyCulture = new CultureInfo(str2);
                        }
                        return null;
                    }
                    if (type.FullName == "System.Reflection.AssemblyDelaySignAttribute")
                    {
                        if (this.positionalArgValues.Count > 0)
                        {
                            base.Engine.Globals.assemblyDelaySign = Microsoft.JScript.Convert.ToBoolean(this.positionalArgValues[0], false);
                        }
                        return null;
                    }
                    if (type.FullName == "System.Reflection.AssemblyFlagsAttribute")
                    {
                        if (this.positionalArgValues.Count > 0)
                        {
                            base.Engine.Globals.assemblyFlags = (AssemblyFlags) ((uint) Microsoft.JScript.Convert.CoerceT(this.positionalArgValues[0], typeof(uint)));
                        }
                        return null;
                    }
                    if (type.FullName == "System.Reflection.AssemblyKeyFileAttribute")
                    {
                        if (this.positionalArgValues.Count > 0)
                        {
                            base.Engine.Globals.assemblyKeyFileName = Microsoft.JScript.Convert.ToString(this.positionalArgValues[0]);
                            base.Engine.Globals.assemblyKeyFileNameContext = base.context;
                            if ((base.Engine.Globals.assemblyKeyFileName != null) && (base.Engine.Globals.assemblyKeyFileName.Length == 0))
                            {
                                base.Engine.Globals.assemblyKeyFileName = null;
                                base.Engine.Globals.assemblyKeyFileNameContext = null;
                            }
                        }
                        return null;
                    }
                    if (type.FullName == "System.Reflection.AssemblyKeyNameAttribute")
                    {
                        if (this.positionalArgValues.Count > 0)
                        {
                            base.Engine.Globals.assemblyKeyName = Microsoft.JScript.Convert.ToString(this.positionalArgValues[0]);
                            base.Engine.Globals.assemblyKeyNameContext = base.context;
                            if ((base.Engine.Globals.assemblyKeyName != null) && (base.Engine.Globals.assemblyKeyName.Length == 0))
                            {
                                base.Engine.Globals.assemblyKeyName = null;
                                base.Engine.Globals.assemblyKeyNameContext = null;
                            }
                        }
                        return null;
                    }
                    if (type.FullName == "System.Reflection.AssemblyVersionAttribute")
                    {
                        if (this.positionalArgValues.Count > 0)
                        {
                            base.Engine.Globals.assemblyVersion = this.ParseVersion(Microsoft.JScript.Convert.ToString(this.positionalArgValues[0]));
                        }
                        return null;
                    }
                    if (type.FullName == "System.CLSCompliantAttribute")
                    {
                        base.Engine.isCLSCompliant = ((this.args == null) || (this.args.count == 0)) || Microsoft.JScript.Convert.ToBoolean(this.positionalArgValues[0], false);
                        return this;
                    }
                }
            }
            catch (ArgumentException)
            {
                base.context.HandleError(JSError.InvalidCall);
            }
            return this;
        }

        private static ushort SecondsSinceMidnight()
        {
            TimeSpan span = (TimeSpan) (DateTime.Now - DateTime.Today);
            return (ushort) (((((span.Hours * 60) * 60) + (span.Minutes * 60)) + span.Seconds) / 2);
        }

        internal void SetTarget(AST target)
        {
            this.target = target;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
        }

        internal static IReflect TypeOfArgument(object argument)
        {
            if (argument is Enum)
            {
                return argument.GetType();
            }
            if (argument is EnumWrapper)
            {
                return ((EnumWrapper) argument).classScopeOrType;
            }
            switch (Microsoft.JScript.Convert.GetTypeCode(argument))
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    return Typeob.Object;

                case TypeCode.Object:
                    if (argument is Type)
                    {
                        return Typeob.Type;
                    }
                    if (argument is ClassScope)
                    {
                        return Typeob.Type;
                    }
                    break;

                case TypeCode.Boolean:
                    return Typeob.Boolean;

                case TypeCode.Char:
                    return Typeob.Char;

                case TypeCode.SByte:
                    return Typeob.SByte;

                case TypeCode.Byte:
                    return Typeob.Byte;

                case TypeCode.Int16:
                    return Typeob.Int16;

                case TypeCode.UInt16:
                    return Typeob.UInt16;

                case TypeCode.Int32:
                    return Typeob.Int32;

                case TypeCode.UInt32:
                    return Typeob.UInt32;

                case TypeCode.Int64:
                    return Typeob.Int64;

                case TypeCode.UInt64:
                    return Typeob.UInt64;

                case TypeCode.Single:
                    return Typeob.Single;

                case TypeCode.Double:
                    return Typeob.Double;

                case TypeCode.String:
                    return Typeob.String;
            }
            return null;
        }
    }
}

