namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class Call : AST
    {
        private bool alreadyPartiallyEvaluated;
        private ASTList args;
        private object[] argValues;
        private FunctionScope enclosingFunctionScope;
        internal AST func;
        internal bool inBrackets;
        private bool isAssignmentToDefaultIndexedProperty;
        internal bool isConstructor;
        private int outParameterCount;

        internal Call(Context context, AST func, ASTList args, bool inBrackets) : base(context)
        {
            this.func = func;
            this.args = (args == null) ? new ASTList(context) : args;
            this.argValues = null;
            this.outParameterCount = 0;
            int num = 0;
            int count = this.args.count;
            while (num < count)
            {
                if (this.args[num] is AddressOf)
                {
                    this.outParameterCount++;
                }
                num++;
            }
            this.isConstructor = false;
            this.inBrackets = inBrackets;
            this.enclosingFunctionScope = null;
            this.alreadyPartiallyEvaluated = false;
            this.isAssignmentToDefaultIndexedProperty = false;
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            while (!(parent is FunctionScope))
            {
                parent = parent.GetParent();
                if (parent == null)
                {
                    return;
                }
            }
            this.enclosingFunctionScope = (FunctionScope) parent;
        }

        private bool AllParamsAreMissing()
        {
            int num = 0;
            int count = this.args.count;
            while (num < count)
            {
                AST ast = this.args[num];
                if (!(ast is ConstantWrapper) || (((ConstantWrapper) ast).value != System.Reflection.Missing.Value))
                {
                    return false;
                }
                num++;
            }
            return true;
        }

        private IReflect[] ArgIRs()
        {
            int count = this.args.count;
            IReflect[] reflectArray = new IReflect[count];
            for (int i = 0; i < count; i++)
            {
                AST ast = this.args[i];
                IReflect ir = reflectArray[i] = ast.InferType(null);
                if (ast is AddressOf)
                {
                    if (ir is ClassScope)
                    {
                        ir = ((ClassScope) ir).GetBakedSuperType();
                    }
                    reflectArray[i] = Microsoft.JScript.Convert.ToType("&", Microsoft.JScript.Convert.ToType(ir));
                }
            }
            return reflectArray;
        }

        internal bool CanBeFunctionDeclaration()
        {
            bool flag = (this.func is Lookup) && (this.outParameterCount == 0);
            if (flag)
            {
                int num = 0;
                int count = this.args.count;
                while (num < count)
                {
                    AST ast = this.args[num];
                    flag = ast is Lookup;
                    if (!flag)
                    {
                        return flag;
                    }
                    num++;
                }
            }
            return flag;
        }

        internal override void CheckIfOKToUseInSuperConstructorCall()
        {
            this.func.CheckIfOKToUseInSuperConstructorCall();
        }

        internal override bool Delete()
        {
            object[] objArray = (this.args == null) ? null : this.args.EvaluateAsArray();
            int length = objArray.Length;
            object obj2 = this.func.Evaluate();
            if (obj2 == null)
            {
                return true;
            }
            if (length == 0)
            {
                return true;
            }
            Type type = obj2.GetType();
            MethodInfo method = type.GetMethod("op_Delete", BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Static, null, new Type[] { type, Typeob.ArrayOfObject }, null);
            if (((method == null) || ((method.Attributes & MethodAttributes.SpecialName) == MethodAttributes.PrivateScope)) || (method.ReturnType != Typeob.Boolean))
            {
                return LateBinding.DeleteMember(obj2, Microsoft.JScript.Convert.ToString(objArray[length - 1]));
            }
            method = new JSMethodInfo(method);
            return (bool) method.Invoke(null, new object[] { obj2, objArray });
        }

        internal override object Evaluate()
        {
            object obj3;
            if ((this.outParameterCount > 0) && VsaEngine.executeForJSEE)
            {
                throw new JScriptException(JSError.RefParamsNonSupportedInDebugger);
            }
            LateBinding callee = this.func.EvaluateAsLateBinding();
            object[] objArray = (this.args == null) ? null : this.args.EvaluateAsArray();
            base.Globals.CallContextStack.Push(new CallContext(base.context, callee, objArray));
            try
            {
                object obj2 = null;
                CallableExpression func = this.func as CallableExpression;
                if ((func == null) || !(func.expression is Call))
                {
                    obj2 = callee.Call(objArray, this.isConstructor, this.inBrackets, base.Engine);
                }
                else
                {
                    obj2 = LateBinding.CallValue(callee.obj, objArray, this.isConstructor, this.inBrackets, base.Engine, func.GetObject2(), JSBinder.ob, null, null);
                }
                if (this.outParameterCount > 0)
                {
                    int index = 0;
                    int count = this.args.count;
                    while (index < count)
                    {
                        if (this.args[index] is AddressOf)
                        {
                            this.args[index].SetValue(objArray[index]);
                        }
                        index++;
                    }
                }
                obj3 = obj2;
            }
            catch (TargetInvocationException exception)
            {
                JScriptException innerException;
                if (exception.InnerException is JScriptException)
                {
                    innerException = (JScriptException) exception.InnerException;
                    if (innerException.context == null)
                    {
                        if (innerException.Number == -2146823281)
                        {
                            innerException.context = this.func.context;
                        }
                        else
                        {
                            innerException.context = base.context;
                        }
                    }
                }
                else
                {
                    innerException = new JScriptException(exception.InnerException, base.context);
                }
                throw innerException;
            }
            catch (JScriptException exception3)
            {
                if (exception3.context == null)
                {
                    if (exception3.Number == -2146823281)
                    {
                        exception3.context = this.func.context;
                    }
                    else
                    {
                        exception3.context = base.context;
                    }
                }
                throw exception3;
            }
            catch (Exception exception4)
            {
                throw new JScriptException(exception4, base.context);
            }
            finally
            {
                base.Globals.CallContextStack.Pop();
            }
            return obj3;
        }

        internal void EvaluateIndices()
        {
            this.argValues = this.args.EvaluateAsArray();
        }

        internal IdentifierLiteral GetName()
        {
            return new IdentifierLiteral(this.func.ToString(), this.func.context);
        }

        internal void GetParameters(ArrayList parameters)
        {
            int num = 0;
            int count = this.args.count;
            while (num < count)
            {
                AST ast = this.args[num];
                parameters.Add(new ParameterDeclaration(ast.context, ast.ToString(), null, null));
                num++;
            }
        }

        internal override IReflect InferType(JSField inference_target)
        {
            if (this.func is Binding)
            {
                return ((Binding) this.func).InferTypeOfCall(inference_target, this.isConstructor);
            }
            if (this.func is ConstantWrapper)
            {
                object obj2 = ((ConstantWrapper) this.func).value;
                if (((obj2 is Type) || (obj2 is ClassScope)) || (obj2 is TypedArray))
                {
                    return (IReflect) obj2;
                }
            }
            return Typeob.Object;
        }

        private JSLocalField[] LocalsThatWereOutParameters()
        {
            int outParameterCount = this.outParameterCount;
            if (outParameterCount == 0)
            {
                return null;
            }
            JSLocalField[] fieldArray = new JSLocalField[outParameterCount];
            int num2 = 0;
            for (int i = 0; i < outParameterCount; i++)
            {
                AST ast = this.args[i];
                if (ast is AddressOf)
                {
                    FieldInfo field = ((AddressOf) ast).GetField();
                    if (field is JSLocalField)
                    {
                        fieldArray[num2++] = (JSLocalField) field;
                    }
                }
            }
            return fieldArray;
        }

        internal void MakeDeletable()
        {
            if (this.func is Binding)
            {
                Binding func = (Binding) this.func;
                func.InvalidateBinding();
                func.PartiallyEvaluateAsCallable();
                func.ResolveLHValue();
            }
        }

        internal override AST PartiallyEvaluate()
        {
            if (this.alreadyPartiallyEvaluated)
            {
                return this;
            }
            this.alreadyPartiallyEvaluated = true;
            if (this.inBrackets && this.AllParamsAreMissing())
            {
                if (this.isConstructor)
                {
                    this.args.context.HandleError(JSError.TypeMismatch);
                }
                return new ConstantWrapper(new TypedArray(((TypeExpression) new TypeExpression(this.func).PartiallyEvaluate()).ToIReflect(), this.args.count + 1), base.context);
            }
            this.func = this.func.PartiallyEvaluateAsCallable();
            this.args = (ASTList) this.args.PartiallyEvaluate();
            IReflect[] argIRs = this.ArgIRs();
            this.func.ResolveCall(this.args, argIRs, this.isConstructor, this.inBrackets);
            if ((!this.isConstructor && !this.inBrackets) && ((this.func is Binding) && (this.args.count == 1)))
            {
                Binding func = (Binding) this.func;
                if (func.member is Type)
                {
                    Type member = (Type) func.member;
                    ConstantWrapper wrapper = this.args[0] as ConstantWrapper;
                    if (wrapper != null)
                    {
                        try
                        {
                            if ((wrapper.value == null) || (wrapper.value is DBNull))
                            {
                                return this;
                            }
                            if (wrapper.isNumericLiteral && (((member == Typeob.Decimal) || (member == Typeob.Int64)) || ((member == Typeob.UInt64) || (member == Typeob.Single))))
                            {
                                return new ConstantWrapper(Microsoft.JScript.Convert.CoerceT(wrapper.context.GetCode(), member, true), base.context);
                            }
                            return new ConstantWrapper(Microsoft.JScript.Convert.CoerceT(wrapper.Evaluate(), member, true), base.context);
                        }
                        catch
                        {
                            wrapper.context.HandleError(JSError.TypeMismatch);
                            goto Label_0354;
                        }
                    }
                    if (!Binding.AssignmentCompatible(member, this.args[0], argIRs[0], false))
                    {
                        this.args[0].context.HandleError(JSError.ImpossibleConversion);
                    }
                }
                else if (func.member is JSVariableField)
                {
                    JSVariableField field = (JSVariableField) func.member;
                    if (field.IsLiteral)
                    {
                        if (field.value is ClassScope)
                        {
                            ClassScope scope = (ClassScope) field.value;
                            IReflect underlyingTypeIfEnum = scope.GetUnderlyingTypeIfEnum();
                            if (underlyingTypeIfEnum != null)
                            {
                                if ((!Microsoft.JScript.Convert.IsPromotableTo(argIRs[0], underlyingTypeIfEnum) && !Microsoft.JScript.Convert.IsPromotableTo(underlyingTypeIfEnum, argIRs[0])) && ((argIRs[0] != Typeob.String) || (underlyingTypeIfEnum == scope)))
                                {
                                    this.args[0].context.HandleError(JSError.ImpossibleConversion);
                                }
                            }
                            else if (!Microsoft.JScript.Convert.IsPromotableTo(argIRs[0], scope) && !Microsoft.JScript.Convert.IsPromotableTo(scope, argIRs[0]))
                            {
                                this.args[0].context.HandleError(JSError.ImpossibleConversion);
                            }
                        }
                        else if (field.value is TypedArray)
                        {
                            TypedArray array = (TypedArray) field.value;
                            if (!Microsoft.JScript.Convert.IsPromotableTo(argIRs[0], array) && !Microsoft.JScript.Convert.IsPromotableTo(array, argIRs[0]))
                            {
                                this.args[0].context.HandleError(JSError.ImpossibleConversion);
                            }
                        }
                    }
                }
            }
        Label_0354:
            return this;
        }

        internal override AST PartiallyEvaluateAsReference()
        {
            this.func = this.func.PartiallyEvaluateAsCallable();
            this.args = (ASTList) this.args.PartiallyEvaluate();
            return this;
        }

        internal override void SetPartialValue(AST partial_value)
        {
            if (this.isConstructor)
            {
                base.context.HandleError(JSError.IllegalAssignment);
            }
            else if (this.func is Binding)
            {
                ((Binding) this.func).SetPartialValue(this.args, this.ArgIRs(), partial_value, this.inBrackets);
            }
            else if (this.func is ThisLiteral)
            {
                ((ThisLiteral) this.func).ResolveAssignmentToDefaultIndexedProperty(this.args, this.ArgIRs(), partial_value);
            }
        }

        internal override void SetValue(object value)
        {
            LateBinding binding = this.func.EvaluateAsLateBinding();
            try
            {
                binding.SetIndexedPropertyValue((this.argValues != null) ? this.argValues : this.args.EvaluateAsArray(), value);
            }
            catch (JScriptException exception)
            {
                if (exception.context == null)
                {
                    exception.context = this.func.context;
                }
                throw exception;
            }
            catch (Exception exception2)
            {
                throw new JScriptException(exception2, this.func.context);
            }
        }

        internal Microsoft.JScript.CustomAttribute ToCustomAttribute()
        {
            return new Microsoft.JScript.CustomAttribute(base.context, this.func, this.args);
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            if (base.context.document.debugOn)
            {
                il.Emit(OpCodes.Nop);
            }
            bool flag = true;
            if ((this.enclosingFunctionScope != null) && (this.enclosingFunctionScope.owner != null))
            {
                Binding func = this.func as Binding;
                if ((func != null) && !this.enclosingFunctionScope.closuresMightEscape)
                {
                    if (func.member is JSLocalField)
                    {
                        this.enclosingFunctionScope.owner.TranslateToILToSaveLocals(il);
                    }
                    else
                    {
                        flag = false;
                    }
                }
                else
                {
                    this.enclosingFunctionScope.owner.TranslateToILToSaveLocals(il);
                }
            }
            this.func.TranslateToILCall(il, rtype, this.args, this.isConstructor, this.inBrackets);
            if ((flag && (this.enclosingFunctionScope != null)) && (this.enclosingFunctionScope.owner != null))
            {
                if (this.outParameterCount == 0)
                {
                    this.enclosingFunctionScope.owner.TranslateToILToRestoreLocals(il);
                }
                else
                {
                    this.enclosingFunctionScope.owner.TranslateToILToRestoreLocals(il, this.LocalsThatWereOutParameters());
                }
            }
            if (base.context.document.debugOn)
            {
                il.Emit(OpCodes.Nop);
            }
        }

        internal override void TranslateToILDelete(ILGenerator il, Type rtype)
        {
            IReflect ir = this.func.InferType(null);
            Type type = Microsoft.JScript.Convert.ToType(ir);
            this.func.TranslateToIL(il, type);
            this.args.TranslateToIL(il, Typeob.ArrayOfObject);
            if (this.func is Binding)
            {
                MethodInfo deleteOpMethod;
                if (ir is ClassScope)
                {
                    deleteOpMethod = ((ClassScope) ir).owner.deleteOpMethod;
                }
                else
                {
                    deleteOpMethod = ir.GetMethod("op_Delete", BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Static, null, new Type[] { type, Typeob.ArrayOfObject }, null);
                }
                if (((deleteOpMethod != null) && ((deleteOpMethod.Attributes & MethodAttributes.SpecialName) != MethodAttributes.PrivateScope)) && (deleteOpMethod.ReturnType == Typeob.Boolean))
                {
                    il.Emit(OpCodes.Call, deleteOpMethod);
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Boolean, rtype);
                    return;
                }
            }
            ConstantWrapper.TranslateToILInt(il, this.args.count - 1);
            il.Emit(OpCodes.Ldelem_Ref);
            Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, Typeob.String);
            il.Emit(OpCodes.Call, CompilerGlobals.deleteMemberMethod);
            Microsoft.JScript.Convert.Emit(this, il, Typeob.Boolean, rtype);
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            this.func.TranslateToILInitializer(il);
            this.args.TranslateToILInitializer(il);
        }

        internal override void TranslateToILPreSet(ILGenerator il)
        {
            this.func.TranslateToILPreSet(il, this.args);
        }

        internal override void TranslateToILPreSet(ILGenerator il, ASTList args)
        {
            this.isAssignmentToDefaultIndexedProperty = true;
            base.TranslateToILPreSet(il, args);
        }

        internal override void TranslateToILPreSetPlusGet(ILGenerator il)
        {
            this.func.TranslateToILPreSetPlusGet(il, this.args, this.inBrackets);
        }

        internal override void TranslateToILSet(ILGenerator il, AST rhvalue)
        {
            if (this.isAssignmentToDefaultIndexedProperty)
            {
                base.TranslateToILSet(il, rhvalue);
            }
            else
            {
                this.func.TranslateToILSet(il, rhvalue);
            }
        }
    }
}

