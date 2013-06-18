namespace Microsoft.VisualBasic.Activities
{
    using Microsoft.Compiler.VisualBasic;
    using System;
    using System.Activities;
    using System.Activities.ExpressionParser;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.Text;

    internal class VisualBasicHelper
    {
        private static Hashtable assemblyCache;
        private const int AssemblyCacheInitialSize = 100;
        private static object assemblyCacheLock = new object();
        private static Hashtable assemblyToAssemblyNameCache;
        private const int assemblyToAssemblyNameCacheInitSize = 100;
        private static object assemblyToAssemblyNameCacheLock = new object();
        public static readonly HashSet<Assembly> DefaultReferencedAssemblies = new HashSet<Assembly> { typeof(int).Assembly, typeof(CodeTypeDeclaration).Assembly, typeof(Expression).Assembly, typeof(Microsoft.VisualBasic.Strings).Assembly, typeof(Activity).Assembly };
        private LocationReferenceEnvironment environment;
        private static Dictionary<HashSet<Assembly>, HostedCompiler> HostedCompilerCache;
        private const int HostedCompilerCacheSize = 10;
        private CodeActivityMetadata? metadata;
        private HashSet<string> namespaceImports;
        private HashSet<Assembly> referencedAssemblies;
        private string textToCompile;
        private static HopperCache typeReferenceCache = new HopperCache(100, false);
        private static object typeReferenceCacheLock = new object();
        private const int typeReferenceCacheMaxSize = 100;

        private VisualBasicHelper(string expressionText)
        {
            this.textToCompile = expressionText;
        }

        public VisualBasicHelper(string expressionText, HashSet<AssemblyName> refAssemNames, HashSet<string> namespaceImportsNames) : this(expressionText)
        {
            this.Initialize(refAssemNames, namespaceImportsNames);
        }

        public Expression<Func<ActivityContext, T>> Compile<T>(CodeActivityMetadata metadata)
        {
            this.metadata = new CodeActivityMetadata?(metadata);
            return this.Compile<T>(metadata.Environment);
        }

        public Expression<Func<ActivityContext, T>> Compile<T>(LocationReferenceEnvironment environment)
        {
            CompilerResults results;
            Type type = typeof(T);
            this.environment = environment;
            if (this.referencedAssemblies == null)
            {
                this.referencedAssemblies = new HashSet<Assembly>();
            }
            this.referencedAssemblies.UnionWith(DefaultReferencedAssemblies);
            List<Import> importList = new List<Import>();
            foreach (string str in this.namespaceImports)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    importList.Add(new Import(str));
                }
            }
            HashSet<Type> typeReferences = null;
            EnsureTypeReferenced(type, ref typeReferences);
            foreach (Type type2 in typeReferences)
            {
                this.referencedAssemblies.Add(type2.Assembly);
            }
            VisualBasicScriptAndTypeScope scriptScope = new VisualBasicScriptAndTypeScope(this.environment, this.referencedAssemblies.ToList<Assembly>());
            IImportScope importScope = new VisualBasicImportScope(importList);
            CompilerOptions options = new CompilerOptions {
                OptionStrict = OptionStrictSetting.On
            };
            CompilerContext context = new CompilerContext(scriptScope, scriptScope, importScope, options);
            HostedCompiler cachedHostedCompiler = GetCachedHostedCompiler(this.referencedAssemblies);
            lock (cachedHostedCompiler)
            {
                try
                {
                    results = cachedHostedCompiler.CompileExpression(this.textToCompile, context, type);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    FxTrace.Exception.TraceUnhandledException(exception);
                    throw;
                }
            }
            if (scriptScope.ErrorMessage != null)
            {
                throw FxTrace.Exception.AsError(new SourceExpressionException(System.Activities.SR.CompilerErrorSpecificExpression(this.textToCompile, scriptScope.ErrorMessage)));
            }
            if ((results.Errors != null) && (results.Errors.Count > 0))
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine();
                foreach (Microsoft.Compiler.VisualBasic.Error error in results.Errors)
                {
                    builder.AppendLine(error.Description);
                }
                throw FxTrace.Exception.AsError(new SourceExpressionException(System.Activities.SR.CompilerErrorSpecificExpression(this.textToCompile, builder.ToString())));
            }
            LambdaExpression codeBlock = results.CodeBlock;
            if (codeBlock == null)
            {
                return null;
            }
            Expression expression = this.Rewrite(codeBlock.Body, null);
            ParameterExpression[] parameters = new ParameterExpression[] { FindParameter(expression) ?? ExpressionUtilities.RuntimeContextParameter };
            return Expression.Lambda<Func<ActivityContext, T>>(expression, parameters);
        }

        public static Expression<Func<ActivityContext, T>> Compile<T>(string expressionText, CodeActivityMetadata metadata)
        {
            HashSet<VisualBasicImportReference> allImportReferences = GetAllImportReferences(metadata.Environment.Root);
            VisualBasicHelper helper = new VisualBasicHelper(expressionText);
            HashSet<AssemblyName> refAssemNames = new HashSet<AssemblyName>();
            HashSet<string> namespaceImportsNames = new HashSet<string>();
            foreach (VisualBasicImportReference reference in allImportReferences)
            {
                if (reference.EarlyBoundAssembly != null)
                {
                    namespaceImportsNames.Add(reference.Import);
                    if (helper.referencedAssemblies == null)
                    {
                        helper.referencedAssemblies = new HashSet<Assembly>();
                    }
                    helper.referencedAssemblies.Add(reference.EarlyBoundAssembly);
                }
                else
                {
                    if (reference.AssemblyName != null)
                    {
                        refAssemNames.Add(reference.AssemblyName);
                    }
                    namespaceImportsNames.Add(reference.Import);
                }
            }
            helper.Initialize(refAssemNames, namespaceImportsNames);
            return helper.Compile<T>(metadata);
        }

        public LambdaExpression CompileNonGeneric(LocationReferenceEnvironment environment)
        {
            CompilerResults results;
            this.environment = environment;
            if (this.referencedAssemblies == null)
            {
                this.referencedAssemblies = new HashSet<Assembly>();
            }
            this.referencedAssemblies.UnionWith(DefaultReferencedAssemblies);
            List<Import> importList = new List<Import>();
            foreach (string str in this.namespaceImports)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    importList.Add(new Import(str));
                }
            }
            VisualBasicScriptAndTypeScope scriptScope = new VisualBasicScriptAndTypeScope(this.environment, this.referencedAssemblies.ToList<Assembly>());
            IImportScope importScope = new VisualBasicImportScope(importList);
            CompilerOptions options = new CompilerOptions {
                OptionStrict = OptionStrictSetting.On
            };
            CompilerContext context = new CompilerContext(scriptScope, scriptScope, importScope, options);
            HostedCompiler cachedHostedCompiler = GetCachedHostedCompiler(this.referencedAssemblies);
            lock (cachedHostedCompiler)
            {
                try
                {
                    results = cachedHostedCompiler.CompileExpression(this.textToCompile, context);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    FxTrace.Exception.TraceUnhandledException(exception);
                    throw;
                }
            }
            if (scriptScope.ErrorMessage != null)
            {
                throw FxTrace.Exception.AsError(new SourceExpressionException(System.Activities.SR.CompilerErrorSpecificExpression(this.textToCompile, scriptScope.ErrorMessage)));
            }
            if ((results.Errors != null) && (results.Errors.Count > 0))
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine();
                foreach (Microsoft.Compiler.VisualBasic.Error error in results.Errors)
                {
                    builder.AppendLine(error.Description);
                }
                throw FxTrace.Exception.AsError(new SourceExpressionException(System.Activities.SR.CompilerErrorSpecificExpression(this.textToCompile, builder.ToString())));
            }
            LambdaExpression codeBlock = results.CodeBlock;
            if (codeBlock == null)
            {
                return null;
            }
            return Expression.Lambda(codeBlock.Type, this.Rewrite(codeBlock.Body, null), codeBlock.Parameters);
        }

        private static void EnsureTypeReferenced(Type type, ref HashSet<Type> typeReferences)
        {
            HashSet<Type> other = (HashSet<Type>) typeReferenceCache.GetValue(typeReferenceCacheLock, type);
            if (other != null)
            {
                if (typeReferences == null)
                {
                    typeReferences = other;
                }
                else
                {
                    typeReferences.UnionWith(other);
                }
            }
            else
            {
                other = new HashSet<Type>();
                EnsureTypeReferencedRecurse(type, other);
                lock (typeReferenceCacheLock)
                {
                    typeReferenceCache.Add(type, other);
                }
                if (typeReferences == null)
                {
                    typeReferences = other;
                }
                else
                {
                    typeReferences.UnionWith(other);
                }
            }
        }

        private static void EnsureTypeReferencedRecurse(Type type, HashSet<Type> alreadyVisited)
        {
            if (!alreadyVisited.Contains(type))
            {
                alreadyVisited.Add(type);
                Type[] interfaces = type.GetInterfaces();
                for (int i = 0; i < interfaces.Length; i++)
                {
                    EnsureTypeReferencedRecurse(interfaces[i], alreadyVisited);
                }
                for (Type type2 = type.BaseType; (type2 != null) && (type2 != System.Runtime.TypeHelper.ObjectType); type2 = type2.BaseType)
                {
                    EnsureTypeReferencedRecurse(type2, alreadyVisited);
                }
                if (type.IsGenericType)
                {
                    Type[] genericArguments = type.GetGenericArguments();
                    for (int j = 1; j < genericArguments.Length; j++)
                    {
                        EnsureTypeReferencedRecurse(genericArguments[j], alreadyVisited);
                    }
                }
                if (type.HasElementType)
                {
                    EnsureTypeReferencedRecurse(type.GetElementType(), alreadyVisited);
                }
            }
        }

        private static ParameterExpression FindParameter(ICollection<ElementInit> collection)
        {
            foreach (ElementInit init in collection)
            {
                ParameterExpression expression = FindParameter(init.Arguments);
                if (expression != null)
                {
                    return expression;
                }
            }
            return null;
        }

        private static ParameterExpression FindParameter(ICollection<Expression> collection)
        {
            foreach (Expression expression in collection)
            {
                ParameterExpression expression2 = FindParameter(expression);
                if (expression2 != null)
                {
                    return expression2;
                }
            }
            return null;
        }

        private static ParameterExpression FindParameter(ICollection<MemberBinding> bindings)
        {
            foreach (MemberBinding binding in bindings)
            {
                ParameterExpression expression;
                switch (binding.BindingType)
                {
                    case MemberBindingType.Assignment:
                    {
                        MemberAssignment assignment = (MemberAssignment) binding;
                        expression = FindParameter(assignment.Expression);
                        break;
                    }
                    case MemberBindingType.MemberBinding:
                    {
                        MemberMemberBinding binding3 = (MemberMemberBinding) binding;
                        expression = FindParameter(binding3.Bindings);
                        break;
                    }
                    case MemberBindingType.ListBinding:
                    {
                        MemberListBinding binding2 = (MemberListBinding) binding;
                        expression = FindParameter(binding2.Initializers);
                        break;
                    }
                    default:
                        expression = null;
                        break;
                }
                if (expression != null)
                {
                    return expression;
                }
            }
            return null;
        }

        private static ParameterExpression FindParameter(Expression expression)
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
                        return (FindParameter(expression2.Left) ?? FindParameter(expression2.Right));
                    }
                    case ExpressionType.ArrayLength:
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.Negate:
                    case ExpressionType.UnaryPlus:
                    case ExpressionType.NegateChecked:
                    case ExpressionType.Not:
                    case ExpressionType.Quote:
                    case ExpressionType.TypeAs:
                    {
                        UnaryExpression expression16 = (UnaryExpression) expression;
                        return FindParameter(expression16.Operand);
                    }
                    case ExpressionType.ArrayIndex:
                    {
                        MethodCallExpression expression9 = expression as MethodCallExpression;
                        if (expression9 == null)
                        {
                            BinaryExpression expression10 = (BinaryExpression) expression;
                            return (FindParameter(expression10.Left) ?? FindParameter(expression10.Right));
                        }
                        return (FindParameter(expression9.Object) ?? FindParameter(expression9.Arguments));
                    }
                    case ExpressionType.Call:
                    {
                        MethodCallExpression expression11 = (MethodCallExpression) expression;
                        return (FindParameter(expression11.Object) ?? FindParameter(expression11.Arguments));
                    }
                    case ExpressionType.Conditional:
                    {
                        ConditionalExpression expression3 = (ConditionalExpression) expression;
                        return (FindParameter(expression3.Test) ?? (FindParameter(expression3.IfTrue) ?? FindParameter(expression3.IfFalse)));
                    }
                    case ExpressionType.Constant:
                        return null;

                    case ExpressionType.Invoke:
                    {
                        InvocationExpression expression4 = (InvocationExpression) expression;
                        return (FindParameter(expression4.Expression) ?? FindParameter(expression4.Arguments));
                    }
                    case ExpressionType.Lambda:
                    {
                        LambdaExpression expression5 = (LambdaExpression) expression;
                        return FindParameter(expression5.Body);
                    }
                    case ExpressionType.ListInit:
                    {
                        ListInitExpression expression6 = (ListInitExpression) expression;
                        return (FindParameter(expression6.NewExpression) ?? FindParameter(expression6.Initializers));
                    }
                    case ExpressionType.MemberAccess:
                    {
                        MemberExpression expression7 = (MemberExpression) expression;
                        return FindParameter(expression7.Expression);
                    }
                    case ExpressionType.MemberInit:
                    {
                        MemberInitExpression expression8 = (MemberInitExpression) expression;
                        return (FindParameter(expression8.NewExpression) ?? FindParameter(expression8.Bindings));
                    }
                    case ExpressionType.New:
                    {
                        NewExpression expression13 = (NewExpression) expression;
                        return FindParameter(expression13.Arguments);
                    }
                    case ExpressionType.NewArrayInit:
                    case ExpressionType.NewArrayBounds:
                    {
                        NewArrayExpression expression12 = (NewArrayExpression) expression;
                        return FindParameter(expression12.Expressions);
                    }
                    case ExpressionType.Parameter:
                    {
                        ParameterExpression expression14 = (ParameterExpression) expression;
                        if (!(expression14.Type == typeof(ActivityContext)) || !(expression14.Name == "context"))
                        {
                            return null;
                        }
                        return expression14;
                    }
                    case ExpressionType.TypeIs:
                    {
                        TypeBinaryExpression expression15 = (TypeBinaryExpression) expression;
                        return FindParameter(expression15.Expression);
                    }
                    case ExpressionType.Assign:
                    {
                        BinaryExpression expression20 = (BinaryExpression) expression;
                        return (FindParameter(expression20.Left) ?? FindParameter(expression20.Right));
                    }
                    case ExpressionType.Block:
                    {
                        BlockExpression expression17 = (BlockExpression) expression;
                        ParameterExpression expression18 = FindParameter(expression17.Expressions);
                        if (expression18 == null)
                        {
                            List<Expression> collection = new List<Expression>();
                            foreach (ParameterExpression expression19 in expression17.Variables)
                            {
                                collection.Add(expression19);
                            }
                            return FindParameter(collection);
                        }
                        return expression18;
                    }
                }
            }
            return null;
        }

        public static HashSet<VisualBasicImportReference> GetAllImportReferences(Activity root)
        {
            VisualBasicSettings settings = null;
            if (root != null)
            {
                settings = VisualBasic.GetSettings(root);
            }
            HashSet<VisualBasicImportReference> set = new HashSet<VisualBasicImportReference>(VisualBasicSettings.Default.ImportReferences);
            if (settings != null)
            {
                set.UnionWith(settings.ImportReferences);
            }
            return set;
        }

        private static Assembly GetAssembly(AssemblyName assemblyName)
        {
            if (assemblyCache == null)
            {
                lock (assemblyCacheLock)
                {
                    if (assemblyCache == null)
                    {
                        assemblyCache = new Hashtable(100, new AssemblyNameEqualityComparer());
                    }
                }
            }
            Assembly assembly = assemblyCache[assemblyName] as Assembly;
            if (assembly == null)
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = assemblies.Length - 1; i >= 0; i--)
                {
                    Assembly assembly2 = assemblies[i];
                    if (!assembly2.IsDynamic)
                    {
                        AssemblyName fastAssemblyName = GetFastAssemblyName(assembly2);
                        Version version = fastAssemblyName.Version;
                        CultureInfo cultureInfo = fastAssemblyName.CultureInfo;
                        byte[] publicKeyToken = fastAssemblyName.GetPublicKeyToken();
                        Version version2 = assemblyName.Version;
                        CultureInfo info2 = assemblyName.CultureInfo;
                        byte[] reqKeyToken = assemblyName.GetPublicKeyToken();
                        if ((((string.Compare(fastAssemblyName.Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase) == 0) && ((version2 == null) || version2.Equals(version))) && ((info2 == null) || info2.Equals(cultureInfo))) && ((reqKeyToken == null) || AssemblyNameEqualityComparer.IsSameKeyToken(reqKeyToken, publicKeyToken)))
                        {
                            lock (assemblyCacheLock)
                            {
                                assemblyCache[assemblyName] = assembly2;
                                return assembly2;
                            }
                        }
                    }
                }
                assembly = LoadAssembly(assemblyName);
                if (assembly == null)
                {
                    return assembly;
                }
                lock (assemblyCacheLock)
                {
                    assemblyCache[assemblyName] = assembly;
                }
            }
            return assembly;
        }

        private static HostedCompiler GetCachedHostedCompiler(HashSet<Assembly> assemblySet)
        {
            if (HostedCompilerCache == null)
            {
                IEqualityComparer<HashSet<Assembly>> comparer = HashSet<Assembly>.CreateSetComparer();
                HostedCompilerCache = new Dictionary<HashSet<Assembly>, HostedCompiler>(10, comparer);
            }
            lock (HostedCompilerCache)
            {
                HostedCompiler compiler;
                if (!HostedCompilerCache.TryGetValue(assemblySet, out compiler))
                {
                    if (HostedCompilerCache.Count >= 10)
                    {
                        HashSet<Assembly> key = HostedCompilerCache.Keys.ElementAtOrDefault<HashSet<Assembly>>(1);
                        HostedCompilerCache.Remove(key);
                    }
                    compiler = new HostedCompiler(assemblySet.ToList<Assembly>());
                    HostedCompilerCache[assemblySet] = compiler;
                }
                return compiler;
            }
        }

        public static AssemblyName GetFastAssemblyName(Assembly assembly)
        {
            if (assemblyToAssemblyNameCache == null)
            {
                lock (assemblyToAssemblyNameCacheLock)
                {
                    if (assemblyToAssemblyNameCache == null)
                    {
                        assemblyToAssemblyNameCache = new Hashtable(100);
                    }
                }
            }
            AssemblyName name = assemblyToAssemblyNameCache[assembly] as AssemblyName;
            if (name == null)
            {
                name = new AssemblyName(assembly.FullName);
                lock (assemblyToAssemblyNameCacheLock)
                {
                    assemblyToAssemblyNameCache[assembly] = name;
                }
            }
            return name;
        }

        private void Initialize(HashSet<AssemblyName> refAssemNames, HashSet<string> namespaceImportsNames)
        {
            this.namespaceImports = namespaceImportsNames;
            foreach (AssemblyName name in refAssemNames)
            {
                if (this.referencedAssemblies == null)
                {
                    this.referencedAssemblies = new HashSet<Assembly>();
                }
                Assembly item = GetAssembly(name);
                if (item != null)
                {
                    this.referencedAssemblies.Add(item);
                }
            }
        }

        private static Assembly LoadAssembly(AssemblyName assemblyName)
        {
            Assembly assembly = null;
            byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
            if (((assemblyName.Version != null) || (assemblyName.CultureInfo != null)) || (publicKeyToken != null))
            {
                try
                {
                    assembly = Assembly.Load(assemblyName.FullName);
                }
                catch (Exception exception)
                {
                    if ((!(exception is FileNotFoundException) && !(exception is FileLoadException)) && (!(exception is TargetInvocationException) || (!(((TargetInvocationException) exception).InnerException is FileNotFoundException) && !(((TargetInvocationException) exception).InnerException is FileNotFoundException))))
                    {
                        throw;
                    }
                    assembly = null;
                    FxTrace.Exception.AsWarning(exception);
                    return assembly;
                }
                return assembly;
            }
            return Assembly.LoadWithPartialName(assemblyName.FullName);
        }

        private Expression Rewrite(Expression expression, ReadOnlyCollection<ParameterExpression> lambdaParameters)
        {
            Func<Expression, Expression> selector = null;
            Func<ElementInit, ElementInit> func2 = null;
            Func<MemberBinding, MemberBinding> func3 = null;
            Func<Expression, Expression> func4 = null;
            Func<Expression, Expression> func5 = null;
            Func<Expression, Expression> func6 = null;
            Func<Expression, Expression> func7 = null;
            Func<Expression, Expression> func8 = null;
            if (expression == null)
            {
                return null;
            }
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
                    return Expression.MakeBinary(expression2.NodeType, this.Rewrite(expression2.Left, lambdaParameters), this.Rewrite(expression2.Right, lambdaParameters), expression2.IsLiftedToNull, expression2.Method, (LambdaExpression) this.Rewrite(expression2.Conversion, lambdaParameters));
                }
                case ExpressionType.ArrayLength:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                {
                    UnaryExpression expression17 = (UnaryExpression) expression;
                    return Expression.MakeUnary(expression17.NodeType, this.Rewrite(expression17.Operand, lambdaParameters), expression17.Type, expression17.Method);
                }
                case ExpressionType.ArrayIndex:
                {
                    MethodCallExpression expression10 = expression as MethodCallExpression;
                    if (expression10 == null)
                    {
                        BinaryExpression expression11 = (BinaryExpression) expression;
                        return Expression.ArrayIndex(this.Rewrite(expression11.Left, lambdaParameters), this.Rewrite(expression11.Right, lambdaParameters));
                    }
                    if (func4 == null)
                    {
                        func4 = a => this.Rewrite(a, lambdaParameters);
                    }
                    return Expression.ArrayIndex(this.Rewrite(expression10.Object, lambdaParameters), expression10.Arguments.Select<Expression, Expression>(func4));
                }
                case ExpressionType.Call:
                {
                    MethodCallExpression expression12 = (MethodCallExpression) expression;
                    if (func5 == null)
                    {
                        func5 = a => this.Rewrite(a, lambdaParameters);
                    }
                    return Expression.Call(this.Rewrite(expression12.Object, lambdaParameters), expression12.Method, expression12.Arguments.Select<Expression, Expression>(func5));
                }
                case ExpressionType.Conditional:
                {
                    ConditionalExpression expression3 = (ConditionalExpression) expression;
                    return Expression.Condition(this.Rewrite(expression3.Test, lambdaParameters), this.Rewrite(expression3.IfTrue, lambdaParameters), this.Rewrite(expression3.IfFalse, lambdaParameters));
                }
                case ExpressionType.Constant:
                    return expression;

                case ExpressionType.Invoke:
                {
                    InvocationExpression expression4 = (InvocationExpression) expression;
                    if (selector == null)
                    {
                        selector = a => this.Rewrite(a, lambdaParameters);
                    }
                    return Expression.Invoke(this.Rewrite(expression4.Expression, lambdaParameters), expression4.Arguments.Select<Expression, Expression>(selector));
                }
                case ExpressionType.Lambda:
                {
                    LambdaExpression expression5 = (LambdaExpression) expression;
                    return Expression.Lambda(expression5.Type, this.Rewrite(expression5.Body, expression5.Parameters), expression5.Parameters);
                }
                case ExpressionType.ListInit:
                {
                    ListInitExpression expression6 = (ListInitExpression) expression;
                    if (func2 == null)
                    {
                        func2 = ei => Expression.ElementInit(ei.AddMethod, (IEnumerable<Expression>) (from arg in ei.Arguments select this.Rewrite(arg, lambdaParameters)));
                    }
                    return Expression.ListInit((NewExpression) this.Rewrite(expression6.NewExpression, lambdaParameters), expression6.Initializers.Select<ElementInit, ElementInit>(func2));
                }
                case ExpressionType.MemberAccess:
                {
                    MemberExpression expression8 = (MemberExpression) expression;
                    return Expression.MakeMemberAccess(this.Rewrite(expression8.Expression, lambdaParameters), expression8.Member);
                }
                case ExpressionType.MemberInit:
                {
                    MemberInitExpression expression9 = (MemberInitExpression) expression;
                    if (func3 == null)
                    {
                        func3 = b => this.Rewrite(b, lambdaParameters);
                    }
                    return Expression.MemberInit((NewExpression) this.Rewrite(expression9.NewExpression, lambdaParameters), expression9.Bindings.Select<MemberBinding, MemberBinding>(func3));
                }
                case ExpressionType.UnaryPlus:
                {
                    UnaryExpression expression18 = (UnaryExpression) expression;
                    return Expression.UnaryPlus(this.Rewrite(expression18.Operand, lambdaParameters), expression18.Method);
                }
                case ExpressionType.New:
                {
                    NewExpression expression15 = (NewExpression) expression;
                    if (expression15.Constructor != null)
                    {
                        if (func8 == null)
                        {
                            func8 = a => this.Rewrite(a, lambdaParameters);
                        }
                        return Expression.New(expression15.Constructor, expression15.Arguments.Select<Expression, Expression>(func8));
                    }
                    return expression;
                }
                case ExpressionType.NewArrayInit:
                {
                    NewArrayExpression expression13 = (NewArrayExpression) expression;
                    if (func6 == null)
                    {
                        func6 = e => this.Rewrite(e, lambdaParameters);
                    }
                    return Expression.NewArrayInit(expression13.Type.GetElementType(), expression13.Expressions.Select<Expression, Expression>(func6));
                }
                case ExpressionType.NewArrayBounds:
                {
                    NewArrayExpression expression14 = (NewArrayExpression) expression;
                    if (func7 == null)
                    {
                        func7 = e => this.Rewrite(e, lambdaParameters);
                    }
                    return Expression.NewArrayBounds(expression14.Type.GetElementType(), expression14.Expressions.Select<Expression, Expression>(func7));
                }
                case ExpressionType.Parameter:
                {
                    ParameterExpression expression7 = (ParameterExpression) expression;
                    if ((lambdaParameters == null) || !lambdaParameters.Contains(expression7))
                    {
                        string name = expression7.Name;
                        for (LocationReferenceEnvironment environment = this.environment; environment != null; environment = environment.Parent)
                        {
                            foreach (LocationReference reference in environment.GetLocationReferences())
                            {
                                if (string.Equals(reference.Name, name, StringComparison.OrdinalIgnoreCase))
                                {
                                    LocationReference reference3;
                                    LocationReference locationReference = reference;
                                    if (this.metadata.HasValue && this.metadata.Value.TryGetInlinedLocationReference(reference, out reference3))
                                    {
                                        locationReference = reference3;
                                    }
                                    return ExpressionUtilities.CreateIdentifierExpression(locationReference);
                                }
                            }
                        }
                        return expression7;
                    }
                    return expression7;
                }
                case ExpressionType.TypeIs:
                {
                    TypeBinaryExpression expression16 = (TypeBinaryExpression) expression;
                    return Expression.TypeIs(this.Rewrite(expression16.Expression, lambdaParameters), expression16.TypeOperand);
                }
                case ExpressionType.Assign:
                {
                    BinaryExpression expression22 = (BinaryExpression) expression;
                    return Expression.Assign(this.Rewrite(expression22.Left, lambdaParameters), this.Rewrite(expression22.Right, lambdaParameters));
                }
                case ExpressionType.Block:
                {
                    BlockExpression expression19 = (BlockExpression) expression;
                    List<ParameterExpression> list = new List<ParameterExpression>();
                    foreach (ParameterExpression expression20 in expression19.Variables)
                    {
                        list.Add((ParameterExpression) this.Rewrite(expression20, lambdaParameters));
                    }
                    List<Expression> list2 = new List<Expression>();
                    foreach (Expression expression21 in expression19.Expressions)
                    {
                        list2.Add(this.Rewrite(expression21, lambdaParameters));
                    }
                    return Expression.Block((IEnumerable<ParameterExpression>) list, (IEnumerable<Expression>) list2);
                }
            }
            return expression;
        }

        private MemberBinding Rewrite(MemberBinding binding, ReadOnlyCollection<ParameterExpression> lambdaParameters)
        {
            Func<ElementInit, ElementInit> selector = null;
            Func<MemberBinding, MemberBinding> func2 = null;
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                {
                    MemberAssignment assignment = (MemberAssignment) binding;
                    return Expression.Bind(assignment.Member, this.Rewrite(assignment.Expression, lambdaParameters));
                }
                case MemberBindingType.MemberBinding:
                {
                    MemberMemberBinding binding3 = (MemberMemberBinding) binding;
                    if (func2 == null)
                    {
                        func2 = b => this.Rewrite(b, lambdaParameters);
                    }
                    return Expression.MemberBind(binding3.Member, binding3.Bindings.Select<MemberBinding, MemberBinding>(func2));
                }
                case MemberBindingType.ListBinding:
                {
                    MemberListBinding binding2 = (MemberListBinding) binding;
                    if (selector == null)
                    {
                        selector = li => Expression.ElementInit(li.AddMethod, (IEnumerable<Expression>) (from arg in li.Arguments select this.Rewrite(arg, lambdaParameters)));
                    }
                    return Expression.ListBind(binding2.Member, binding2.Initializers.Select<ElementInit, ElementInit>(selector));
                }
            }
            return binding;
        }

        public string TextToCompile
        {
            get
            {
                return this.textToCompile;
            }
        }

        private class VisualBasicImportScope : IImportScope
        {
            private IList<Import> importList;

            public VisualBasicImportScope(IList<Import> importList)
            {
                this.importList = importList;
            }

            public IList<Import> GetImports()
            {
                return this.importList;
            }
        }

        private class VisualBasicScriptAndTypeScope : IScriptScope, ITypeScope
        {
            private List<Assembly> assemblies;
            private LocationReferenceEnvironment environmentProvider;
            private string errorMessage;

            public VisualBasicScriptAndTypeScope(LocationReferenceEnvironment environmentProvider, List<Assembly> assemblies)
            {
                this.environmentProvider = environmentProvider;
                this.assemblies = assemblies;
            }

            public Type[] FindTypes(string typeName, string nsPrefix)
            {
                return null;
            }

            public Type FindVariable(string name)
            {
                LocationReference reference = null;
                bool flag = false;
                bool flag2 = false;
                for (LocationReferenceEnvironment environment = this.environmentProvider; (environment != null) && !flag2; environment = environment.Parent)
                {
                    foreach (LocationReference reference2 in environment.GetLocationReferences())
                    {
                        if (string.Equals(reference2.Name, name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (flag)
                            {
                                flag2 = true;
                                break;
                            }
                            flag = true;
                            reference = reference2;
                        }
                    }
                }
                if (flag2)
                {
                    this.errorMessage = System.Activities.SR.AmbiguousVBVariableReference(name);
                    return null;
                }
                if (!flag)
                {
                    return null;
                }
                Type type = reference.Type;
                HashSet<Type> typeReferences = null;
                VisualBasicHelper.EnsureTypeReferenced(type, ref typeReferences);
                foreach (Type type2 in typeReferences)
                {
                    if (!this.assemblies.Contains(type2.Assembly))
                    {
                        return null;
                    }
                }
                return type;
            }

            public bool NamespaceExists(string ns)
            {
                return false;
            }

            public string ErrorMessage
            {
                get
                {
                    return this.errorMessage;
                }
            }
        }
    }
}

