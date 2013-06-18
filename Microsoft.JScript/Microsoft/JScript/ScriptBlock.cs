namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;

    public class ScriptBlock : AST
    {
        private JSField[] fields;
        private GlobalScope own_scope;
        private Block statement_block;

        internal ScriptBlock(Context context, Block statement_block) : base(context)
        {
            this.statement_block = statement_block;
            this.own_scope = (GlobalScope) base.Engine.ScriptObjectStackTop();
            this.fields = null;
        }

        internal override object Evaluate()
        {
            if (this.fields == null)
            {
                this.fields = this.own_scope.GetFields();
            }
            int index = 0;
            int length = this.fields.Length;
            while (index < length)
            {
                FieldInfo info = this.fields[index];
                if (!(info is JSExpandoField))
                {
                    object obj2 = info.GetValue(this.own_scope);
                    if (obj2 is FunctionObject)
                    {
                        ((FunctionObject) obj2).engine = base.Engine;
                        this.own_scope.AddFieldOrUseExistingField(info.Name, new Closure((FunctionObject) obj2), info.Attributes);
                    }
                    else if (obj2 is ClassScope)
                    {
                        this.own_scope.AddFieldOrUseExistingField(info.Name, obj2, info.Attributes);
                    }
                    else
                    {
                        this.own_scope.AddFieldOrUseExistingField(info.Name, Microsoft.JScript.Missing.Value, info.Attributes);
                    }
                }
                index++;
            }
            object obj3 = this.statement_block.Evaluate();
            if (obj3 is Completion)
            {
                obj3 = ((Completion) obj3).value;
            }
            return obj3;
        }

        internal override Context GetFirstExecutableContext()
        {
            return this.statement_block.GetFirstExecutableContext();
        }

        internal override AST PartiallyEvaluate()
        {
            this.statement_block.PartiallyEvaluate();
            if ((base.Engine.PEFileKind == PEFileKinds.Dll) && base.Engine.doSaveAfterCompile)
            {
                this.statement_block.ComplainAboutAnythingOtherThanClassOrPackage();
            }
            this.fields = this.own_scope.GetFields();
            return this;
        }

        internal void ProcessAssemblyAttributeLists()
        {
            this.statement_block.ProcessAssemblyAttributeLists();
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            Expression expression = this.statement_block.ToExpression();
            if (expression != null)
            {
                expression.TranslateToIL(il, rtype);
            }
            else
            {
                this.statement_block.TranslateToIL(il, Typeob.Void);
                new ConstantWrapper(null, base.context).TranslateToIL(il, rtype);
            }
        }

        internal TypeBuilder TranslateToILClass(CompilerGlobals compilerGlobals)
        {
            return this.TranslateToILClass(compilerGlobals, true);
        }

        internal TypeBuilder TranslateToILClass(CompilerGlobals compilerGlobals, bool pushScope)
        {
            VsaEngine engine = base.Engine;
            int num2 = engine.classCounter++;
            TypeBuilder builder = compilerGlobals.classwriter = compilerGlobals.module.DefineType("JScript " + num2.ToString(CultureInfo.InvariantCulture), TypeAttributes.Public, Typeob.GlobalScope, (Type[]) null);
            compilerGlobals.classwriter.SetCustomAttribute(new CustomAttributeBuilder(CompilerGlobals.compilerGlobalScopeAttributeCtor, new object[0]));
            if (null == compilerGlobals.globalScopeClassWriter)
            {
                compilerGlobals.globalScopeClassWriter = builder;
            }
            ILGenerator iLGenerator = compilerGlobals.classwriter.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { Typeob.GlobalScope }).GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Dup);
            iLGenerator.Emit(OpCodes.Ldfld, CompilerGlobals.engineField);
            iLGenerator.Emit(OpCodes.Call, CompilerGlobals.globalScopeConstructor);
            iLGenerator.Emit(OpCodes.Ret);
            iLGenerator = builder.DefineMethod("Global Code", MethodAttributes.Public, Typeob.Object, null).GetILGenerator();
            if (base.Engine.GenerateDebugInfo)
            {
                for (ScriptObject obj2 = this.own_scope.GetParent(); obj2 != null; obj2 = obj2.GetParent())
                {
                    if ((obj2 is WrappedNamespace) && !((WrappedNamespace) obj2).name.Equals(""))
                    {
                        iLGenerator.UsingNamespace(((WrappedNamespace) obj2).name);
                    }
                }
            }
            int startLine = base.context.StartLine;
            int startColumn = base.context.StartColumn;
            Context firstExecutableContext = this.GetFirstExecutableContext();
            if (firstExecutableContext != null)
            {
                firstExecutableContext.EmitFirstLineInfo(iLGenerator);
            }
            if (pushScope)
            {
                base.EmitILToLoadEngine(iLGenerator);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Call, CompilerGlobals.pushScriptObjectMethod);
            }
            this.TranslateToILInitializer(iLGenerator);
            this.TranslateToIL(iLGenerator, Typeob.Object);
            if (pushScope)
            {
                base.EmitILToLoadEngine(iLGenerator);
                iLGenerator.Emit(OpCodes.Call, CompilerGlobals.popScriptObjectMethod);
                iLGenerator.Emit(OpCodes.Pop);
            }
            iLGenerator.Emit(OpCodes.Ret);
            return builder;
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            int length = this.fields.Length;
            if (length > 0)
            {
                for (int i = 0; i < length; i++)
                {
                    JSGlobalField field = this.fields[i] as JSGlobalField;
                    if (field != null)
                    {
                        Type fieldType = field.FieldType;
                        if (((field.IsLiteral && (fieldType != Typeob.ScriptFunction)) && (fieldType != Typeob.Type)) || (field.metaData != null))
                        {
                            if (((fieldType.IsPrimitive || (fieldType == Typeob.String)) || fieldType.IsEnum) && (field.metaData == null))
                            {
                                base.compilerGlobals.classwriter.DefineField(field.Name, fieldType, field.Attributes).SetConstant(field.value);
                            }
                        }
                        else if (!(field.value is FunctionObject) || !((FunctionObject) field.value).suppressIL)
                        {
                            FieldBuilder builder2 = base.compilerGlobals.classwriter.DefineField(field.Name, fieldType, (field.Attributes & ~(FieldAttributes.Literal | FieldAttributes.InitOnly)) | FieldAttributes.Static);
                            field.metaData = builder2;
                            field.WriteCustomAttribute(base.Engine.doCRS);
                        }
                    }
                }
            }
            this.statement_block.TranslateToILInitializer(il);
        }
    }
}

