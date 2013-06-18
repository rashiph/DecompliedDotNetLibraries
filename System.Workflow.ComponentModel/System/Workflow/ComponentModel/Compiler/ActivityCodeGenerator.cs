namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    public class ActivityCodeGenerator
    {
        public virtual void GenerateCode(CodeGenerationManager manager, object obj)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            Activity context = obj as Activity;
            if (context == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(Activity).FullName }), "obj");
            }
            manager.Context.Push(context);
            Walker walker = new Walker();
            walker.FoundProperty += delegate (Walker w, WalkerEventArgs args) {
                ActivityBind currentValue = args.CurrentValue as ActivityBind;
                if (currentValue != null)
                {
                    if (args.CurrentProperty != null)
                    {
                        manager.Context.Push(args.CurrentProperty);
                    }
                    manager.Context.Push(args.CurrentPropertyOwner);
                    foreach (ActivityCodeGenerator generator in manager.GetCodeGenerators(currentValue.GetType()))
                    {
                        generator.GenerateCode(manager, args.CurrentValue);
                    }
                    manager.Context.Pop();
                    if (args.CurrentProperty != null)
                    {
                        manager.Context.Pop();
                    }
                }
            };
            walker.WalkProperties(context, obj);
            manager.Context.Pop();
        }

        protected CodeTypeDeclaration GetCodeTypeDeclaration(CodeGenerationManager manager, string fullClassName)
        {
            string str;
            string str2;
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (fullClassName == null)
            {
                throw new ArgumentNullException("fullClassName");
            }
            Helpers.GetNamespaceAndClassName(fullClassName, out str, out str2);
            CodeNamespaceCollection namespaces = manager.Context[typeof(CodeNamespaceCollection)] as CodeNamespaceCollection;
            if (namespaces == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_ContextStackItemMissing", new object[] { typeof(CodeNamespaceCollection).Name }));
            }
            CodeNamespace codeNamespace = null;
            return Helpers.GetCodeNamespaceAndClass(namespaces, str, str2, out codeNamespace);
        }
    }
}

