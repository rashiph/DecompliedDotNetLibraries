namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Reflection.Emit;

    public sealed class Block : AST
    {
        private Completion completion;
        private ArrayList list;

        internal Block(Context context) : base(context)
        {
            this.completion = new Completion();
            this.list = new ArrayList();
        }

        internal void Append(AST elem)
        {
            this.list.Add(elem);
        }

        internal void ComplainAboutAnythingOtherThanClassOrPackage()
        {
            int num = 0;
            int count = this.list.Count;
            while (num < count)
            {
                object obj2 = this.list[num];
                if ((!(obj2 is Class) && !(obj2 is Package)) && !(obj2 is Import))
                {
                    Block block = obj2 as Block;
                    if ((block == null) || (block.list.Count != 0))
                    {
                        Expression expression = obj2 as Expression;
                        if ((expression == null) || !(expression.operand is AssemblyCustomAttributeList))
                        {
                            ((AST) obj2).context.HandleError(JSError.OnlyClassesAndPackagesAllowed);
                            return;
                        }
                    }
                }
                num++;
            }
        }

        internal override object Evaluate()
        {
            this.completion.Continue = 0;
            this.completion.Exit = 0;
            this.completion.value = null;
            int num = 0;
            int count = this.list.Count;
            while (num < count)
            {
                object obj2;
                AST ast = (AST) this.list[num];
                try
                {
                    obj2 = ast.Evaluate();
                }
                catch (JScriptException exception)
                {
                    if (exception.context == null)
                    {
                        exception.context = ast.context;
                    }
                    throw exception;
                }
                Completion completion = (Completion) obj2;
                if (completion.value != null)
                {
                    this.completion.value = completion.value;
                }
                if (completion.Continue > 1)
                {
                    this.completion.Continue = completion.Continue - 1;
                    break;
                }
                if (completion.Exit > 0)
                {
                    this.completion.Exit = completion.Exit - 1;
                    break;
                }
                if (completion.Return)
                {
                    return completion;
                }
                num++;
            }
            return this.completion;
        }

        internal void EvaluateInstanceVariableInitializers()
        {
            int num = 0;
            int count = this.list.Count;
            while (num < count)
            {
                object obj2 = this.list[num];
                VariableDeclaration declaration = obj2 as VariableDeclaration;
                if (((declaration != null) && !declaration.field.IsStatic) && !declaration.field.IsLiteral)
                {
                    declaration.Evaluate();
                }
                else
                {
                    Block block = obj2 as Block;
                    if (block != null)
                    {
                        block.EvaluateInstanceVariableInitializers();
                    }
                }
                num++;
            }
        }

        internal void EvaluateStaticVariableInitializers()
        {
            int num = 0;
            int count = this.list.Count;
            while (num < count)
            {
                object obj2 = this.list[num];
                VariableDeclaration declaration = obj2 as VariableDeclaration;
                if (((declaration != null) && declaration.field.IsStatic) && !declaration.field.IsLiteral)
                {
                    declaration.Evaluate();
                }
                else
                {
                    StaticInitializer initializer = obj2 as StaticInitializer;
                    if (initializer != null)
                    {
                        initializer.Evaluate();
                    }
                    else
                    {
                        Class class2 = obj2 as Class;
                        if (class2 != null)
                        {
                            class2.Evaluate();
                        }
                        else
                        {
                            Constant constant = obj2 as Constant;
                            if ((constant != null) && constant.field.IsStatic)
                            {
                                constant.Evaluate();
                            }
                            else
                            {
                                Block block = obj2 as Block;
                                if (block != null)
                                {
                                    block.EvaluateStaticVariableInitializers();
                                }
                            }
                        }
                    }
                }
                num++;
            }
        }

        internal override Context GetFirstExecutableContext()
        {
            int num = 0;
            int count = this.list.Count;
            while (num < count)
            {
                AST ast = (AST) this.list[num];
                Context firstExecutableContext = ast.GetFirstExecutableContext();
                if (firstExecutableContext != null)
                {
                    return firstExecutableContext;
                }
                num++;
            }
            return null;
        }

        internal override bool HasReturn()
        {
            int num = 0;
            int count = this.list.Count;
            while (num < count)
            {
                AST ast = (AST) this.list[num];
                if (ast.HasReturn())
                {
                    return true;
                }
                num++;
            }
            return false;
        }

        internal void MarkSuperOKIfIsFirstStatement()
        {
            if ((this.list.Count > 0) && (this.list[0] is ConstructorCall))
            {
                ((ConstructorCall) this.list[0]).isOK = true;
            }
        }

        internal override AST PartiallyEvaluate()
        {
            int num = 0;
            int count = this.list.Count;
            while (num < count)
            {
                this.list[num] = ((AST) this.list[num]).PartiallyEvaluate();
                num++;
            }
            return this;
        }

        internal void ProcessAssemblyAttributeLists()
        {
            int num = 0;
            int count = this.list.Count;
            while (num < count)
            {
                Expression expression = this.list[num] as Expression;
                if (expression != null)
                {
                    AssemblyCustomAttributeList operand = expression.operand as AssemblyCustomAttributeList;
                    if (operand != null)
                    {
                        operand.Process();
                    }
                }
                num++;
            }
        }

        internal Expression ToExpression()
        {
            if ((this.list.Count == 1) && (this.list[0] is Expression))
            {
                return (Expression) this.list[0];
            }
            return null;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            Label item = il.DefineLabel();
            base.compilerGlobals.BreakLabelStack.Push(item);
            base.compilerGlobals.ContinueLabelStack.Push(item);
            int num = 0;
            int count = this.list.Count;
            while (num < count)
            {
                ((AST) this.list[num]).TranslateToIL(il, Typeob.Void);
                num++;
            }
            il.MarkLabel(item);
            base.compilerGlobals.BreakLabelStack.Pop();
            base.compilerGlobals.ContinueLabelStack.Pop();
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            int num = 0;
            int count = this.list.Count;
            while (num < count)
            {
                ((AST) this.list[num]).TranslateToILInitializer(il);
                num++;
            }
        }

        internal void TranslateToILInitOnlyInitializers(ILGenerator il)
        {
            int num = 0;
            int count = this.list.Count;
            while (num < count)
            {
                Constant constant = this.list[num] as Constant;
                if (constant != null)
                {
                    constant.TranslateToILInitOnlyInitializers(il);
                }
                num++;
            }
        }

        internal void TranslateToILInstanceInitializers(ILGenerator il)
        {
            int num = 0;
            int count = this.list.Count;
            while (num < count)
            {
                AST ast = (AST) this.list[num];
                if (((ast is VariableDeclaration) && !((VariableDeclaration) ast).field.IsStatic) && !((VariableDeclaration) ast).field.IsLiteral)
                {
                    ast.TranslateToILInitializer(il);
                    ast.TranslateToIL(il, Typeob.Void);
                }
                else if ((ast is FunctionDeclaration) && !((FunctionDeclaration) ast).func.isStatic)
                {
                    ast.TranslateToILInitializer(il);
                }
                else if ((ast is Constant) && !((Constant) ast).field.IsStatic)
                {
                    ast.TranslateToIL(il, Typeob.Void);
                }
                else if (ast is Block)
                {
                    ((Block) ast).TranslateToILInstanceInitializers(il);
                }
                num++;
            }
        }

        internal void TranslateToILStaticInitializers(ILGenerator il)
        {
            int num = 0;
            int count = this.list.Count;
            while (num < count)
            {
                AST ast = (AST) this.list[num];
                if (((ast is VariableDeclaration) && ((VariableDeclaration) ast).field.IsStatic) || ((ast is Constant) && ((Constant) ast).field.IsStatic))
                {
                    ast.TranslateToILInitializer(il);
                    ast.TranslateToIL(il, Typeob.Void);
                }
                else if (ast is StaticInitializer)
                {
                    ast.TranslateToIL(il, Typeob.Void);
                }
                else if ((ast is FunctionDeclaration) && ((FunctionDeclaration) ast).func.isStatic)
                {
                    ast.TranslateToILInitializer(il);
                }
                else if (ast is Class)
                {
                    ast.TranslateToIL(il, Typeob.Void);
                }
                else if (ast is Block)
                {
                    ((Block) ast).TranslateToILStaticInitializers(il);
                }
                num++;
            }
        }
    }
}

