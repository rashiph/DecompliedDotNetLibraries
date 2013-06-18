namespace Microsoft.VisualBasic.Activities
{
    using System;
    using System.Activities;
    using System.Activities.ExpressionParser;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;

    public static class VisualBasicDesignerHelper
    {
        private static Type VisualBasicExpressionFactoryType = typeof(VisualBasicExpressionFactory);

        public static Activity CreatePrecompiledVisualBasicReference(Type targetType, string expressionText, IEnumerable<string> namespaces, IEnumerable<string> referencedAssemblies, LocationReferenceEnvironment environment, out Type returnType, out SourceExpressionException compileError, out VisualBasicSettings vbSettings)
        {
            LambdaExpression expression = null;
            HashSet<string> namespaceImportsNames = new HashSet<string>();
            HashSet<AssemblyName> refAssemNames = new HashSet<AssemblyName>();
            compileError = null;
            returnType = null;
            if (namespaces != null)
            {
                foreach (string str in namespaces)
                {
                    if (str != null)
                    {
                        namespaceImportsNames.Add(str);
                    }
                }
            }
            if (referencedAssemblies != null)
            {
                foreach (string str2 in referencedAssemblies)
                {
                    if (str2 != null)
                    {
                        refAssemNames.Add(new AssemblyName(str2));
                    }
                }
            }
            VisualBasicHelper helper = new VisualBasicHelper(expressionText, refAssemNames, namespaceImportsNames);
            if (targetType == null)
            {
                try
                {
                    expression = helper.CompileNonGeneric(environment);
                    if (expression != null)
                    {
                        string str3;
                        if (!ExpressionUtilities.IsLocation(expression, targetType, out str3))
                        {
                            string invalidLValueExpression = System.Activities.SR.InvalidLValueExpression;
                            if (str3 != null)
                            {
                                invalidLValueExpression = invalidLValueExpression + ":" + str3;
                            }
                            throw FxTrace.Exception.AsError(new SourceExpressionException(System.Activities.SR.CompilerErrorSpecificExpression(expressionText, invalidLValueExpression)));
                        }
                        returnType = expression.ReturnType;
                    }
                }
                catch (SourceExpressionException exception)
                {
                    compileError = exception;
                    returnType = typeof(object);
                }
                targetType = returnType;
            }
            else
            {
                MethodInfo info = typeof(VisualBasicHelper).GetMethod("Compile", new Type[] { typeof(LocationReferenceEnvironment) }).MakeGenericMethod(new Type[] { targetType });
                try
                {
                    expression = (LambdaExpression) info.Invoke(helper, new object[] { environment });
                    string extraErrorMessage = null;
                    if (!ExpressionUtilities.IsLocation(expression, targetType, out extraErrorMessage))
                    {
                        string str6 = System.Activities.SR.InvalidLValueExpression;
                        if (extraErrorMessage != null)
                        {
                            str6 = str6 + ":" + extraErrorMessage;
                        }
                        throw FxTrace.Exception.AsError(new SourceExpressionException(System.Activities.SR.CompilerErrorSpecificExpression(expressionText, str6)));
                    }
                    returnType = targetType;
                }
                catch (SourceExpressionException exception2)
                {
                    compileError = exception2;
                    returnType = typeof(object);
                }
                catch (TargetInvocationException exception3)
                {
                    SourceExpressionException innerException = exception3.InnerException as SourceExpressionException;
                    if (innerException == null)
                    {
                        throw FxTrace.Exception.AsError(exception3.InnerException);
                    }
                    compileError = innerException;
                    returnType = typeof(object);
                }
            }
            vbSettings = new VisualBasicSettings();
            if (expression != null)
            {
                HashSet<Type> typeReferences = new HashSet<Type>();
                FindTypeReferences(expression.Body, typeReferences);
                foreach (Type type in typeReferences)
                {
                    Assembly assembly = type.Assembly;
                    if (!assembly.IsDynamic)
                    {
                        string name = VisualBasicHelper.GetFastAssemblyName(assembly).Name;
                        VisualBasicImportReference item = new VisualBasicImportReference {
                            Assembly = name,
                            Import = type.Namespace
                        };
                        vbSettings.ImportReferences.Add(item);
                    }
                }
            }
            VisualBasicExpressionFactory factory = (VisualBasicExpressionFactory) Activator.CreateInstance(VisualBasicExpressionFactoryType.MakeGenericType(new Type[] { targetType }));
            return factory.CreateVisualBasicReference(expressionText);
        }

        public static Activity CreatePrecompiledVisualBasicValue(Type targetType, string expressionText, IEnumerable<string> namespaces, IEnumerable<string> referencedAssemblies, LocationReferenceEnvironment environment, out Type returnType, out SourceExpressionException compileError, out VisualBasicSettings vbSettings)
        {
            LambdaExpression expression = null;
            HashSet<string> namespaceImportsNames = new HashSet<string>();
            HashSet<AssemblyName> refAssemNames = new HashSet<AssemblyName>();
            compileError = null;
            returnType = null;
            if (namespaces != null)
            {
                foreach (string str in namespaces)
                {
                    if (str != null)
                    {
                        namespaceImportsNames.Add(str);
                    }
                }
            }
            if (referencedAssemblies != null)
            {
                foreach (string str2 in referencedAssemblies)
                {
                    if (str2 != null)
                    {
                        refAssemNames.Add(new AssemblyName(str2));
                    }
                }
            }
            VisualBasicHelper helper = new VisualBasicHelper(expressionText, refAssemNames, namespaceImportsNames);
            if (targetType == null)
            {
                try
                {
                    expression = helper.CompileNonGeneric(environment);
                    if (expression != null)
                    {
                        returnType = expression.ReturnType;
                    }
                }
                catch (SourceExpressionException exception)
                {
                    compileError = exception;
                    returnType = typeof(object);
                }
                targetType = returnType;
            }
            else
            {
                MethodInfo info = typeof(VisualBasicHelper).GetMethod("Compile", new Type[] { typeof(LocationReferenceEnvironment) }).MakeGenericMethod(new Type[] { targetType });
                try
                {
                    expression = (LambdaExpression) info.Invoke(helper, new object[] { environment });
                    returnType = targetType;
                }
                catch (TargetInvocationException exception2)
                {
                    SourceExpressionException innerException = exception2.InnerException as SourceExpressionException;
                    if (innerException == null)
                    {
                        throw FxTrace.Exception.AsError(exception2.InnerException);
                    }
                    compileError = innerException;
                    returnType = typeof(object);
                }
            }
            vbSettings = new VisualBasicSettings();
            if (expression != null)
            {
                HashSet<Type> typeReferences = new HashSet<Type>();
                FindTypeReferences(expression.Body, typeReferences);
                foreach (Type type in typeReferences)
                {
                    Assembly assembly = type.Assembly;
                    if (!assembly.IsDynamic)
                    {
                        string name = VisualBasicHelper.GetFastAssemblyName(assembly).Name;
                        VisualBasicImportReference item = new VisualBasicImportReference {
                            Assembly = name,
                            Import = type.Namespace
                        };
                        vbSettings.ImportReferences.Add(item);
                    }
                }
            }
            VisualBasicExpressionFactory factory = (VisualBasicExpressionFactory) Activator.CreateInstance(VisualBasicExpressionFactoryType.MakeGenericType(new Type[] { targetType }));
            return factory.CreateVisualBasicValue(expressionText);
        }

        private static void EnsureTypeReferenced(Type type, bool isDirectReference, HashSet<Type> typeReferences)
        {
            if (type != null)
            {
                if (type.HasElementType)
                {
                    EnsureTypeReferenced(type.GetElementType(), isDirectReference, typeReferences);
                }
                else
                {
                    EnsureTypeReferencedRecurse(type, isDirectReference, typeReferences);
                    if (type.IsGenericType)
                    {
                        Type[] genericArguments = type.GetGenericArguments();
                        for (int i = 1; i < genericArguments.Length; i++)
                        {
                            EnsureTypeReferencedRecurse(genericArguments[i], isDirectReference, typeReferences);
                        }
                    }
                }
            }
        }

        private static void EnsureTypeReferencedRecurse(Type type, bool isDirectReference, HashSet<Type> typeReferences)
        {
            if (!typeReferences.Contains(type))
            {
                if (isDirectReference || !VisualBasicHelper.DefaultReferencedAssemblies.Contains(type.Assembly))
                {
                    typeReferences.Add(type);
                }
                Type[] interfaces = type.GetInterfaces();
                for (int i = 0; i < interfaces.Length; i++)
                {
                    EnsureTypeReferencedRecurse(interfaces[i], false, typeReferences);
                }
                for (Type type2 = type.BaseType; (type2 != null) && (type2 != TypeHelper.ObjectType); type2 = type2.BaseType)
                {
                    EnsureTypeReferencedRecurse(type2, false, typeReferences);
                }
            }
        }

        private static void FindTypeReferences(Expression expression, HashSet<Type> typeReferences)
        {
            if (expression != null)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Coalesce:
                    case ExpressionType.Divide:
                    case ExpressionType.Equal:
                    case ExpressionType.ExclusiveOr:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LeftShift:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.Modulo:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.Power:
                    case ExpressionType.RightShift:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                    {
                        BinaryExpression expression2 = (BinaryExpression) expression;
                        FindTypeReferences(expression2.Left, typeReferences);
                        FindTypeReferences(expression2.Right, typeReferences);
                        return;
                    }
                    case ExpressionType.ArrayLength:
                    case ExpressionType.Negate:
                    case ExpressionType.UnaryPlus:
                    case ExpressionType.NegateChecked:
                    case ExpressionType.Not:
                    case ExpressionType.Quote:
                    {
                        UnaryExpression expression19 = (UnaryExpression) expression;
                        FindTypeReferences(expression19.Operand, typeReferences);
                        return;
                    }
                    case ExpressionType.ArrayIndex:
                    {
                        MethodCallExpression expression11 = expression as MethodCallExpression;
                        if (expression11 == null)
                        {
                            BinaryExpression expression12 = (BinaryExpression) expression;
                            FindTypeReferences(expression12.Left, typeReferences);
                            FindTypeReferences(expression12.Right, typeReferences);
                            return;
                        }
                        FindTypeReferences(expression11.Object, typeReferences);
                        ReadOnlyCollection<Expression> arguments = expression11.Arguments;
                        for (int i = 0; i < arguments.Count; i++)
                        {
                            FindTypeReferences(arguments[i], typeReferences);
                        }
                        return;
                    }
                    case ExpressionType.Call:
                    {
                        MethodCallExpression expression13 = (MethodCallExpression) expression;
                        MethodInfo method = expression13.Method;
                        EnsureTypeReferenced(expression13.Type, false, typeReferences);
                        if (expression13.Object == null)
                        {
                            EnsureTypeReferenced(method.DeclaringType, true, typeReferences);
                        }
                        else
                        {
                            FindTypeReferences(expression13.Object, typeReferences);
                        }
                        if ((method.IsGenericMethod && !method.IsGenericMethodDefinition) && !method.ContainsGenericParameters)
                        {
                            Type[] genericArguments = method.GetGenericArguments();
                            for (int k = 1; k < genericArguments.Length; k++)
                            {
                                EnsureTypeReferenced(genericArguments[k], true, typeReferences);
                            }
                        }
                        ParameterInfo[] parameters = method.GetParameters();
                        if (parameters != null)
                        {
                            foreach (ParameterInfo info2 in parameters)
                            {
                                EnsureTypeReferenced(info2.ParameterType, false, typeReferences);
                            }
                        }
                        ReadOnlyCollection<Expression> onlys4 = expression13.Arguments;
                        for (int j = 0; j < onlys4.Count; j++)
                        {
                            FindTypeReferences(onlys4[j], typeReferences);
                        }
                        return;
                    }
                    case ExpressionType.Conditional:
                    {
                        ConditionalExpression expression3 = (ConditionalExpression) expression;
                        FindTypeReferences(expression3.Test, typeReferences);
                        FindTypeReferences(expression3.IfTrue, typeReferences);
                        FindTypeReferences(expression3.IfFalse, typeReferences);
                        return;
                    }
                    case ExpressionType.Constant:
                    {
                        ConstantExpression expression4 = (ConstantExpression) expression;
                        if (!(expression4.Value is Type))
                        {
                            if (expression4.Value != null)
                            {
                                EnsureTypeReferenced(expression4.Value.GetType(), true, typeReferences);
                            }
                            return;
                        }
                        EnsureTypeReferenced((Type) expression4.Value, true, typeReferences);
                        return;
                    }
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.TypeAs:
                    {
                        UnaryExpression expression18 = (UnaryExpression) expression;
                        FindTypeReferences(expression18.Operand, typeReferences);
                        EnsureTypeReferenced(expression18.Type, true, typeReferences);
                        return;
                    }
                    case ExpressionType.Invoke:
                    {
                        InvocationExpression expression5 = (InvocationExpression) expression;
                        FindTypeReferences(expression5.Expression, typeReferences);
                        for (int m = 0; m < expression5.Arguments.Count; m++)
                        {
                            FindTypeReferences(expression5.Arguments[m], typeReferences);
                        }
                        return;
                    }
                    case ExpressionType.Lambda:
                    {
                        LambdaExpression expression6 = (LambdaExpression) expression;
                        FindTypeReferences(expression6.Body, typeReferences);
                        for (int n = 0; n < expression6.Parameters.Count; n++)
                        {
                            FindTypeReferences(expression6.Parameters[n], typeReferences);
                        }
                        return;
                    }
                    case ExpressionType.ListInit:
                    {
                        ListInitExpression expression7 = (ListInitExpression) expression;
                        FindTypeReferences(expression7.NewExpression, typeReferences);
                        for (int num3 = 0; num3 < expression7.Initializers.Count; num3++)
                        {
                            ReadOnlyCollection<Expression> onlys = expression7.Initializers[num3].Arguments;
                            for (int num4 = 0; num4 < onlys.Count; num4++)
                            {
                                FindTypeReferences(onlys[num4], typeReferences);
                            }
                        }
                        return;
                    }
                    case ExpressionType.MemberAccess:
                    {
                        MemberExpression expression9 = (MemberExpression) expression;
                        if (expression9.Expression != null)
                        {
                            FindTypeReferences(expression9.Expression, typeReferences);
                        }
                        else
                        {
                            EnsureTypeReferenced(expression9.Member.DeclaringType, true, typeReferences);
                        }
                        EnsureTypeReferenced(expression9.Type, false, typeReferences);
                        return;
                    }
                    case ExpressionType.MemberInit:
                    {
                        MemberInitExpression expression10 = (MemberInitExpression) expression;
                        FindTypeReferences(expression10.NewExpression, typeReferences);
                        ReadOnlyCollection<MemberBinding> bindings = expression10.Bindings;
                        for (int num5 = 0; num5 < bindings.Count; num5++)
                        {
                            FindTypeReferences(bindings[num5], typeReferences);
                        }
                        return;
                    }
                    case ExpressionType.New:
                    {
                        NewExpression expression16 = (NewExpression) expression;
                        if (expression16.Constructor == null)
                        {
                            EnsureTypeReferenced(expression16.Type, true, typeReferences);
                        }
                        else
                        {
                            EnsureTypeReferenced(expression16.Constructor.DeclaringType, true, typeReferences);
                        }
                        ReadOnlyCollection<Expression> onlys7 = expression16.Arguments;
                        for (int num11 = 0; num11 < onlys7.Count; num11++)
                        {
                            FindTypeReferences(onlys7[num11], typeReferences);
                        }
                        return;
                    }
                    case ExpressionType.NewArrayInit:
                    {
                        NewArrayExpression expression14 = (NewArrayExpression) expression;
                        EnsureTypeReferenced(expression14.Type.GetElementType(), true, typeReferences);
                        ReadOnlyCollection<Expression> expressions = expression14.Expressions;
                        for (int num9 = 0; num9 < expressions.Count; num9++)
                        {
                            FindTypeReferences(expressions[num9], typeReferences);
                        }
                        return;
                    }
                    case ExpressionType.NewArrayBounds:
                    {
                        NewArrayExpression expression15 = (NewArrayExpression) expression;
                        EnsureTypeReferenced(expression15.Type.GetElementType(), true, typeReferences);
                        ReadOnlyCollection<Expression> onlys6 = expression15.Expressions;
                        for (int num10 = 0; num10 < onlys6.Count; num10++)
                        {
                            FindTypeReferences(onlys6[num10], typeReferences);
                        }
                        return;
                    }
                    case ExpressionType.Parameter:
                    {
                        ParameterExpression expression8 = (ParameterExpression) expression;
                        EnsureTypeReferenced(expression8.Type, false, typeReferences);
                        return;
                    }
                    case ExpressionType.TypeIs:
                    {
                        TypeBinaryExpression expression17 = (TypeBinaryExpression) expression;
                        FindTypeReferences(expression17.Expression, typeReferences);
                        EnsureTypeReferenced(expression17.TypeOperand, true, typeReferences);
                        return;
                    }
                    case ExpressionType.Assign:
                    {
                        BinaryExpression expression21 = (BinaryExpression) expression;
                        FindTypeReferences(expression21.Left, typeReferences);
                        FindTypeReferences(expression21.Right, typeReferences);
                        return;
                    }
                    case ExpressionType.Block:
                    {
                        BlockExpression expression20 = (BlockExpression) expression;
                        ReadOnlyCollection<ParameterExpression> variables = expression20.Variables;
                        for (int num12 = 0; num12 < variables.Count; num12++)
                        {
                            FindTypeReferences(variables[num12], typeReferences);
                        }
                        ReadOnlyCollection<Expression> onlys9 = expression20.Expressions;
                        for (int num13 = 0; num13 < onlys9.Count; num13++)
                        {
                            FindTypeReferences(onlys9[num13], typeReferences);
                        }
                        return;
                    }
                }
            }
        }

        private static void FindTypeReferences(MemberBinding binding, HashSet<Type> typeReferences)
        {
            ReadOnlyCollection<MemberBinding> bindings;
            int num3;
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                {
                    MemberAssignment assignment = (MemberAssignment) binding;
                    FindTypeReferences(assignment.Expression, typeReferences);
                    return;
                }
                case MemberBindingType.MemberBinding:
                {
                    MemberMemberBinding binding3 = (MemberMemberBinding) binding;
                    bindings = binding3.Bindings;
                    num3 = 0;
                    break;
                }
                case MemberBindingType.ListBinding:
                {
                    MemberListBinding binding2 = (MemberListBinding) binding;
                    ReadOnlyCollection<ElementInit> initializers = binding2.Initializers;
                    for (int i = 0; i < initializers.Count; i++)
                    {
                        ReadOnlyCollection<Expression> arguments = initializers[i].Arguments;
                        for (int j = 0; j < arguments.Count; j++)
                        {
                            FindTypeReferences(arguments[j], typeReferences);
                        }
                    }
                    return;
                }
                default:
                    return;
            }
            while (num3 < bindings.Count)
            {
                FindTypeReferences(bindings[num3], typeReferences);
                num3++;
            }
        }

        public static Activity RecompileVisualBasicReference(ActivityWithResult visualBasicReference, out Type returnType, out SourceExpressionException compileError, out VisualBasicSettings vbSettings)
        {
            IVisualBasicExpression expression = visualBasicReference as IVisualBasicExpression;
            if (expression == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentException());
            }
            string expressionText = expression.ExpressionText;
            LocationReferenceEnvironment parentEnvironment = visualBasicReference.GetParentEnvironment();
            HashSet<VisualBasicImportReference> allImportReferences = VisualBasicHelper.GetAllImportReferences((parentEnvironment != null) ? parentEnvironment.Root : null);
            HashSet<string> namespaces = new HashSet<string>();
            HashSet<string> referencedAssemblies = new HashSet<string>();
            foreach (VisualBasicImportReference reference in allImportReferences)
            {
                namespaces.Add(reference.Import);
                referencedAssemblies.Add(reference.Assembly);
            }
            return CreatePrecompiledVisualBasicReference(null, expressionText, namespaces, referencedAssemblies, parentEnvironment, out returnType, out compileError, out vbSettings);
        }

        public static Activity RecompileVisualBasicValue(ActivityWithResult visualBasicValue, out Type returnType, out SourceExpressionException compileError, out VisualBasicSettings vbSettings)
        {
            IVisualBasicExpression expression = visualBasicValue as IVisualBasicExpression;
            if (expression == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentException());
            }
            string expressionText = expression.ExpressionText;
            LocationReferenceEnvironment parentEnvironment = visualBasicValue.GetParentEnvironment();
            HashSet<VisualBasicImportReference> allImportReferences = VisualBasicHelper.GetAllImportReferences((parentEnvironment != null) ? parentEnvironment.Root : null);
            HashSet<string> namespaces = new HashSet<string>();
            HashSet<string> referencedAssemblies = new HashSet<string>();
            foreach (VisualBasicImportReference reference in allImportReferences)
            {
                namespaces.Add(reference.Import);
                referencedAssemblies.Add(reference.Assembly);
            }
            return CreatePrecompiledVisualBasicValue(null, expressionText, namespaces, referencedAssemblies, parentEnvironment, out returnType, out compileError, out vbSettings);
        }

        private abstract class VisualBasicExpressionFactory
        {
            protected VisualBasicExpressionFactory()
            {
            }

            public abstract Activity CreateVisualBasicReference(string expressionText);
            public abstract Activity CreateVisualBasicValue(string expressionText);
        }

        private class VisualBasicExpressionFactory<T> : VisualBasicDesignerHelper.VisualBasicExpressionFactory
        {
            public override Activity CreateVisualBasicReference(string expressionText)
            {
                return new VisualBasicReference<T> { ExpressionText = expressionText };
            }

            public override Activity CreateVisualBasicValue(string expressionText)
            {
                return new VisualBasicValue<T> { ExpressionText = expressionText };
            }
        }
    }
}

