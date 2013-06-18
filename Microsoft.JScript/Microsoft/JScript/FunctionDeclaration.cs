namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Text;

    public sealed class FunctionDeclaration : AST
    {
        private Completion completion;
        private Member declaringObject;
        internal JSProperty enclosingProperty;
        private JSVariableField field;
        internal FunctionObject func;
        private TypeExpression ifaceId;
        private bool inFastScope;
        internal bool isMethod;
        private string name;

        internal FunctionDeclaration(Context context, AST ifaceId, IdentifierLiteral id, ParameterDeclaration[] formal_parameters, TypeExpression return_type, Block body, FunctionScope own_scope, FieldAttributes attributes, bool isMethod, bool isGetter, bool isSetter, bool isAbstract, bool isFinal, CustomAttributeList customAttributes) : base(context)
        {
            this.completion = new Completion();
            MethodAttributes privateScope = MethodAttributes.PrivateScope;
            if ((attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public)
            {
                privateScope = MethodAttributes.Public;
            }
            else if ((attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Private)
            {
                privateScope = MethodAttributes.Private;
            }
            else if ((attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Assembly)
            {
                privateScope = MethodAttributes.Assembly;
            }
            else if ((attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Family)
            {
                privateScope = MethodAttributes.Family;
            }
            else if ((attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamORAssem)
            {
                privateScope = MethodAttributes.FamORAssem;
            }
            else
            {
                privateScope = MethodAttributes.Public;
            }
            if (((attributes & FieldAttributes.Static) != FieldAttributes.PrivateScope) || !isMethod)
            {
                privateScope |= MethodAttributes.Static;
            }
            else
            {
                privateScope |= MethodAttributes.NewSlot | MethodAttributes.Virtual;
            }
            if (isAbstract)
            {
                privateScope |= MethodAttributes.Abstract;
            }
            if (isFinal)
            {
                privateScope |= MethodAttributes.Final;
            }
            this.name = id.ToString();
            this.isMethod = isMethod;
            if (ifaceId != null)
            {
                if (isMethod)
                {
                    this.ifaceId = new TypeExpression(ifaceId);
                    privateScope &= ~MethodAttributes.MemberAccessMask;
                    privateScope |= MethodAttributes.Final | MethodAttributes.Private;
                }
                else
                {
                    this.declaringObject = new Member(ifaceId.context, ifaceId, id);
                    this.name = this.declaringObject.ToString();
                }
            }
            ScriptObject obj2 = base.Globals.ScopeStack.Peek();
            if (((attributes == FieldAttributes.PrivateScope) && !isAbstract) && !isFinal)
            {
                if (obj2 is ClassScope)
                {
                    attributes |= FieldAttributes.Public;
                }
            }
            else if (!(obj2 is ClassScope))
            {
                base.context.HandleError(JSError.NotInsideClass);
                attributes = FieldAttributes.PrivateScope;
                privateScope = MethodAttributes.Public;
            }
            if (obj2 is ActivationObject)
            {
                this.inFastScope = ((ActivationObject) obj2).fast;
                string name = this.name;
                if (isGetter)
                {
                    privateScope |= MethodAttributes.SpecialName;
                    this.name = "get_" + this.name;
                    if (return_type == null)
                    {
                        return_type = new TypeExpression(new ConstantWrapper(Typeob.Object, context));
                    }
                }
                else if (isSetter)
                {
                    privateScope |= MethodAttributes.SpecialName;
                    this.name = "set_" + this.name;
                    return_type = new TypeExpression(new ConstantWrapper(Typeob.Void, context));
                }
                attributes &= FieldAttributes.FieldAccessMask;
                MethodAttributes attributes3 = privateScope & MethodAttributes.MemberAccessMask;
                if ((((privateScope & MethodAttributes.Virtual) != MethodAttributes.PrivateScope) && ((privateScope & MethodAttributes.Final) == MethodAttributes.PrivateScope)) && (((attributes3 == MethodAttributes.Private) || (attributes3 == MethodAttributes.Assembly)) || (attributes3 == MethodAttributes.FamANDAssem)))
                {
                    privateScope |= MethodAttributes.CheckAccessOnOverride;
                }
                this.func = new FunctionObject(this.name, formal_parameters, return_type, body, own_scope, obj2, base.context, privateScope, customAttributes, this.isMethod);
                if (this.declaringObject == null)
                {
                    string str2 = this.name;
                    if (this.ifaceId != null)
                    {
                        str2 = ifaceId.ToString() + "." + str2;
                    }
                    JSVariableField field = (JSVariableField) ((ActivationObject) obj2).name_table[str2];
                    if ((field != null) && ((!(field is JSMemberField) || !(((JSMemberField) field).value is FunctionObject)) || this.func.isExpandoMethod))
                    {
                        if (name != this.name)
                        {
                            field.originalContext.HandleError(JSError.ClashWithProperty);
                        }
                        else
                        {
                            id.context.HandleError(JSError.DuplicateName, this.func.isExpandoMethod);
                            if (field.value is FunctionObject)
                            {
                                ((FunctionObject) field.value).suppressIL = true;
                            }
                        }
                    }
                    if (this.isMethod)
                    {
                        if ((!(field is JSMemberField) || !(((JSMemberField) field).value is FunctionObject)) || (name != this.name))
                        {
                            this.field = ((ActivationObject) obj2).AddNewField(str2, this.func, attributes | FieldAttributes.Literal);
                            if (name == this.name)
                            {
                                this.field.type = new TypeExpression(new ConstantWrapper(Typeob.FunctionWrapper, base.context));
                            }
                        }
                        else
                        {
                            this.field = ((JSMemberField) field).AddOverload(this.func, attributes | FieldAttributes.Literal);
                        }
                    }
                    else if (obj2 is FunctionScope)
                    {
                        if (this.inFastScope)
                        {
                            attributes |= FieldAttributes.Literal;
                        }
                        this.field = ((FunctionScope) obj2).AddNewField(this.name, attributes, this.func);
                        if (this.field is JSLocalField)
                        {
                            JSLocalField field2 = (JSLocalField) this.field;
                            if (this.inFastScope)
                            {
                                field2.type = new TypeExpression(new ConstantWrapper(Typeob.ScriptFunction, base.context));
                                field2.attributeFlags |= FieldAttributes.Literal;
                            }
                            field2.debugOn = base.context.document.debugOn;
                            field2.isDefined = true;
                        }
                    }
                    else if (this.inFastScope)
                    {
                        this.field = ((ActivationObject) obj2).AddNewField(this.name, this.func, attributes | FieldAttributes.Literal);
                        this.field.type = new TypeExpression(new ConstantWrapper(Typeob.ScriptFunction, base.context));
                    }
                    else
                    {
                        this.field = ((ActivationObject) obj2).AddNewField(this.name, this.func, attributes | FieldAttributes.Static);
                    }
                    this.field.originalContext = context;
                    if (name != this.name)
                    {
                        string str3 = name;
                        if (this.ifaceId != null)
                        {
                            str3 = ifaceId.ToString() + "." + name;
                        }
                        FieldInfo info = (FieldInfo) ((ClassScope) obj2).name_table[str3];
                        if (info != null)
                        {
                            if (info.IsLiteral)
                            {
                                object obj3 = ((JSVariableField) info).value;
                                if (obj3 is JSProperty)
                                {
                                    this.enclosingProperty = (JSProperty) obj3;
                                }
                            }
                            if (this.enclosingProperty == null)
                            {
                                id.context.HandleError(JSError.DuplicateName, true);
                            }
                        }
                        if (this.enclosingProperty == null)
                        {
                            this.enclosingProperty = new JSProperty(name);
                            ((JSMemberField) ((ActivationObject) obj2).AddNewField(str3, this.enclosingProperty, attributes | FieldAttributes.Literal)).originalContext = base.context;
                        }
                        else if ((isGetter && (this.enclosingProperty.getter != null)) || (isSetter && (this.enclosingProperty.setter != null)))
                        {
                            id.context.HandleError(JSError.DuplicateName, true);
                        }
                        if (isGetter)
                        {
                            this.enclosingProperty.getter = new JSFieldMethod(this.field, obj2);
                        }
                        else
                        {
                            this.enclosingProperty.setter = new JSFieldMethod(this.field, obj2);
                        }
                    }
                }
            }
            else
            {
                this.inFastScope = false;
                this.func = new FunctionObject(this.name, formal_parameters, return_type, body, own_scope, obj2, base.context, MethodAttributes.Public, null, false);
                this.field = ((StackFrame) obj2).AddNewField(this.name, new Closure(this.func), attributes | FieldAttributes.Static);
            }
        }

        internal override object Evaluate()
        {
            if (this.declaringObject != null)
            {
                this.declaringObject.SetValue(this.func);
            }
            return this.completion;
        }

        internal override Context GetFirstExecutableContext()
        {
            return null;
        }

        public static Closure JScriptFunctionDeclaration(RuntimeTypeHandle handle, string name, string method_name, string[] formal_parameters, JSLocalField[] fields, bool must_save_stack_locals, bool hasArgumentsObject, string text, object declaringObject, VsaEngine engine)
        {
            return new Closure(new FunctionObject(Type.GetTypeFromHandle(handle), name, method_name, formal_parameters, fields, must_save_stack_locals, hasArgumentsObject, text, engine), declaringObject);
        }

        internal override AST PartiallyEvaluate()
        {
            if (this.ifaceId != null)
            {
                this.ifaceId.PartiallyEvaluate();
                this.func.implementedIface = this.ifaceId.ToIReflect();
                Type implementedIface = this.func.implementedIface as Type;
                ClassScope scope = this.func.implementedIface as ClassScope;
                if (((implementedIface != null) && !implementedIface.IsInterface) || ((scope != null) && !scope.owner.isInterface))
                {
                    this.ifaceId.context.HandleError(JSError.NeedInterface);
                    this.func.implementedIface = null;
                }
                if ((this.func.attributes & MethodAttributes.Abstract) != MethodAttributes.PrivateScope)
                {
                    this.func.funcContext.HandleError(JSError.AbstractCannotBePrivate);
                }
            }
            else if (this.declaringObject != null)
            {
                this.declaringObject.PartiallyEvaluateAsCallable();
            }
            this.func.PartiallyEvaluate();
            if ((this.inFastScope && this.func.isExpandoMethod) && ((this.field != null) && (this.field.type != null)))
            {
                this.field.type.expression = new ConstantWrapper(Typeob.ScriptFunction, null);
            }
            if (((this.func.attributes & MethodAttributes.Abstract) != MethodAttributes.PrivateScope) && !((ClassScope) this.func.enclosing_scope).owner.isAbstract)
            {
                ((ClassScope) this.func.enclosing_scope).owner.attributes |= TypeAttributes.Abstract;
                ((ClassScope) this.func.enclosing_scope).owner.context.HandleError(JSError.CannotBeAbstract, this.name);
            }
            if ((this.enclosingProperty != null) && !this.enclosingProperty.GetterAndSetterAreConsistent())
            {
                base.context.HandleError(JSError.GetAndSetAreInconsistent);
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
        }

        private void TranslateToILClosure(ILGenerator il)
        {
            if (!this.func.isStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
            }
            il.Emit(OpCodes.Ldtoken, (this.func.classwriter != null) ? this.func.classwriter : base.compilerGlobals.classwriter);
            il.Emit(OpCodes.Ldstr, this.name);
            il.Emit(OpCodes.Ldstr, this.func.GetName());
            int length = this.func.formal_parameters.Length;
            ConstantWrapper.TranslateToILInt(il, length);
            il.Emit(OpCodes.Newarr, Typeob.String);
            for (int i = 0; i < length; i++)
            {
                il.Emit(OpCodes.Dup);
                ConstantWrapper.TranslateToILInt(il, i);
                il.Emit(OpCodes.Ldstr, this.func.formal_parameters[i]);
                il.Emit(OpCodes.Stelem_Ref);
            }
            length = this.func.fields.Length;
            ConstantWrapper.TranslateToILInt(il, length);
            il.Emit(OpCodes.Newarr, Typeob.JSLocalField);
            for (int j = 0; j < length; j++)
            {
                JSLocalField field = this.func.fields[j];
                il.Emit(OpCodes.Dup);
                ConstantWrapper.TranslateToILInt(il, j);
                il.Emit(OpCodes.Ldstr, field.Name);
                il.Emit(OpCodes.Ldtoken, field.FieldType);
                ConstantWrapper.TranslateToILInt(il, field.slotNumber);
                il.Emit(OpCodes.Newobj, CompilerGlobals.jsLocalFieldConstructor);
                il.Emit(OpCodes.Stelem_Ref);
            }
            if (this.func.must_save_stack_locals)
            {
                il.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4_0);
            }
            if (this.func.hasArgumentsObject)
            {
                il.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4_0);
            }
            il.Emit(OpCodes.Ldstr, this.func.ToString());
            if (!this.func.isStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
            }
            else
            {
                il.Emit(OpCodes.Ldnull);
            }
            base.EmitILToLoadEngine(il);
            il.Emit(OpCodes.Call, CompilerGlobals.jScriptFunctionDeclarationMethod);
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            if (!this.func.suppressIL)
            {
                this.func.TranslateToIL(base.compilerGlobals);
                if (this.declaringObject != null)
                {
                    this.declaringObject.TranslateToILInitializer(il);
                    this.declaringObject.TranslateToILPreSet(il);
                    this.TranslateToILClosure(il);
                    this.declaringObject.TranslateToILSet(il);
                }
                else
                {
                    object metaData = this.field.metaData;
                    if (this.func.isMethod)
                    {
                        if (metaData is FunctionDeclaration)
                        {
                            this.field.metaData = null;
                        }
                        else
                        {
                            this.TranslateToILSourceTextProvider();
                        }
                    }
                    else if (metaData != null)
                    {
                        this.TranslateToILClosure(il);
                        if (metaData is LocalBuilder)
                        {
                            il.Emit(OpCodes.Stloc, (LocalBuilder) metaData);
                        }
                        else if (this.func.isStatic)
                        {
                            il.Emit(OpCodes.Stsfld, (FieldInfo) metaData);
                        }
                        else
                        {
                            il.Emit(OpCodes.Stfld, (FieldInfo) metaData);
                        }
                    }
                }
            }
        }

        private void TranslateToILSourceTextProvider()
        {
            if (!base.Engine.doFast && (string.Compare(this.name, this.field.Name, StringComparison.Ordinal) == 0))
            {
                StringBuilder builder = new StringBuilder(this.func.ToString());
                for (JSMemberField field = ((JSMemberField) this.field).nextOverload; field != null; field = field.nextOverload)
                {
                    field.metaData = this;
                    builder.Append('\n');
                    builder.Append(field.value.ToString());
                }
                MethodAttributes attributes = MethodAttributes.Static | MethodAttributes.Public;
                ILGenerator iLGenerator = ((ClassScope) this.func.enclosing_scope).GetTypeBuilder().DefineMethod(this.name + " source", attributes, Typeob.String, new Type[0]).GetILGenerator();
                iLGenerator.Emit(OpCodes.Ldstr, builder.ToString());
                iLGenerator.Emit(OpCodes.Ret);
            }
        }
    }
}

