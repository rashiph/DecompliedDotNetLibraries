namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    public class RuleValidation
    {
        private Stack<CodeExpression> activeParentNodes;
        private IList<AuthorizedType> authorizedTypes;
        private bool checkStaticType;
        private static Type defaultExtensionAttribute = GetDefaultExtensionAttribute();
        private ValidationErrorCollection errors;
        private Dictionary<CodeExpression, RuleExpressionInfo> expressionInfoMap;
        private Type extensionAttribute;
        private const string ExtensionAttributeFullName = "System.Runtime.CompilerServices.ExtensionAttribute, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
        private List<ExtensionMethodInfo> extensionMethods;
        private List<Assembly> seenAssemblies;
        private Type thisType;
        private ITypeProvider typeProvider;
        private Dictionary<CodeTypeReference, Type> typeRefMap;
        private Dictionary<string, Type> typesUsed;
        private Dictionary<string, Type> typesUsedAuthorized;
        private static readonly Type voidType = typeof(void);
        private static string voidTypeName = voidType.AssemblyQualifiedName;

        internal RuleValidation(object thisObject)
        {
            this.errors = new ValidationErrorCollection();
            this.typesUsed = new Dictionary<string, Type>(0x10);
            this.activeParentNodes = new Stack<CodeExpression>();
            this.expressionInfoMap = new Dictionary<CodeExpression, RuleExpressionInfo>();
            this.typeRefMap = new Dictionary<CodeTypeReference, Type>();
            if (thisObject == null)
            {
                throw new ArgumentNullException("thisObject");
            }
            this.thisType = thisObject.GetType();
            this.typeProvider = new SimpleRunTimeTypeProvider(this.thisType.Assembly);
        }

        public RuleValidation(Type thisType, ITypeProvider typeProvider)
        {
            this.errors = new ValidationErrorCollection();
            this.typesUsed = new Dictionary<string, Type>(0x10);
            this.activeParentNodes = new Stack<CodeExpression>();
            this.expressionInfoMap = new Dictionary<CodeExpression, RuleExpressionInfo>();
            this.typeRefMap = new Dictionary<CodeTypeReference, Type>();
            if (thisType == null)
            {
                throw new ArgumentNullException("thisType");
            }
            this.thisType = thisType;
            this.typeProvider = (typeProvider != null) ? typeProvider : new SimpleRunTimeTypeProvider(this.thisType.Assembly);
        }

        public RuleValidation(Activity activity, ITypeProvider typeProvider, bool checkStaticType)
        {
            this.errors = new ValidationErrorCollection();
            this.typesUsed = new Dictionary<string, Type>(0x10);
            this.activeParentNodes = new Stack<CodeExpression>();
            this.expressionInfoMap = new Dictionary<CodeExpression, RuleExpressionInfo>();
            this.typeRefMap = new Dictionary<CodeTypeReference, Type>();
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (typeProvider == null)
            {
                throw new ArgumentNullException("typeProvider");
            }
            this.thisType = ConditionHelper.GetContextType(typeProvider, activity);
            this.typeProvider = typeProvider;
            this.checkStaticType = checkStaticType;
            if (checkStaticType)
            {
                this.authorizedTypes = WorkflowCompilationContext.Current.GetAuthorizedTypes();
                this.typesUsedAuthorized = new Dictionary<string, Type>();
                this.typesUsedAuthorized.Add(voidTypeName, voidType);
            }
        }

        internal void AddError(ValidationError error)
        {
            this.Errors.Add(error);
        }

        private static void AddExplicitConversions(Type t, Type source, Type target, List<MethodInfo> methods)
        {
            foreach (MethodInfo info in t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (((info.Name == "op_Implicit") || (info.Name == "op_Explicit")) && (info.GetParameters().Length == 1))
                {
                    ValidationError error;
                    Type parameterType = info.GetParameters()[0].ParameterType;
                    Type returnType = info.ReturnType;
                    if (((StandardImplicitConversion(source, parameterType, null, out error) || StandardImplicitConversion(parameterType, source, null, out error)) && (StandardImplicitConversion(target, returnType, null, out error) || StandardImplicitConversion(returnType, target, null, out error))) && !methods.Contains(info))
                    {
                        methods.Add(info);
                    }
                }
            }
        }

        private static void AddImplicitConversions(Type t, Type source, Type target, List<MethodInfo> methods)
        {
            foreach (MethodInfo info in t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if ((info.Name == "op_Implicit") && (info.GetParameters().Length == 1))
                {
                    ValidationError error;
                    Type parameterType = info.GetParameters()[0].ParameterType;
                    Type returnType = info.ReturnType;
                    if ((StandardImplicitConversion(source, parameterType, null, out error) && StandardImplicitConversion(returnType, target, null, out error)) && !methods.Contains(info))
                    {
                        methods.Add(info);
                    }
                }
            }
        }

        internal void AddTypeReference(CodeTypeReference typeRef, Type type)
        {
            this.typeRefMap[typeRef] = type;
        }

        internal bool AllowInternalMembers(Type type)
        {
            return (type.Assembly == this.thisType.Assembly);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static bool CheckValueRange(CodeExpression rhsExpression, Type lhsType, out ValidationError error)
        {
            error = null;
            CodePrimitiveExpression expression = rhsExpression as CodePrimitiveExpression;
            if (expression != null)
            {
                try
                {
                    Convert.ChangeType(expression.Value, lhsType, CultureInfo.CurrentCulture);
                    return true;
                }
                catch (Exception exception)
                {
                    error = new ValidationError(exception.Message, 0x545);
                    return false;
                }
            }
            return false;
        }

        private void DetermineExtensionMethods()
        {
            this.extensionMethods = new List<ExtensionMethodInfo>();
            this.SetExtensionAttribute();
            if (this.extensionAttribute != null)
            {
                this.seenAssemblies = new List<Assembly>();
                Assembly localAssembly = this.typeProvider.LocalAssembly;
                if (localAssembly != null)
                {
                    this.DetermineExtensionMethods(localAssembly);
                    foreach (Assembly assembly2 in this.typeProvider.ReferencedAssemblies)
                    {
                        this.DetermineExtensionMethods(assembly2);
                    }
                }
                else
                {
                    this.DetermineExtensionMethods(this.typeProvider.GetTypes());
                }
            }
        }

        internal void DetermineExtensionMethods(Assembly assembly)
        {
            if (((this.extensionAttribute != null) && (assembly != null)) && !this.seenAssemblies.Contains(assembly))
            {
                this.seenAssemblies.Add(assembly);
                if (this.IsMarkedExtension(assembly))
                {
                    Type[] types;
                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException exception)
                    {
                        types = exception.Types;
                    }
                    this.DetermineExtensionMethods(types);
                }
            }
        }

        private void DetermineExtensionMethods(Type[] types)
        {
            foreach (Type type in types)
            {
                if (((type != null) && (type.IsPublic || type.IsNestedPublic)) && (type.IsSealed && this.IsMarkedExtension(type)))
                {
                    foreach (MethodInfo info in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        if ((info.IsStatic && !info.IsGenericMethod) && this.IsMarkedExtension(info))
                        {
                            ParameterInfo[] parameters = info.GetParameters();
                            if ((parameters.Length > 0) && (parameters[0].ParameterType != null))
                            {
                                this.extensionMethods.Add(new ExtensionMethodInfo(info, parameters));
                            }
                        }
                    }
                }
            }
        }

        private static void EvaluateCandidate(List<CandidateMember> candidates, MemberInfo candidateMember, ParameterInfo[] parameters, List<Argument> arguments, out ValidationError error, BuildArgCountMismatchError buildArgCountMismatchError)
        {
            error = null;
            int count = arguments.Count;
            string name = candidateMember.Name;
            if ((parameters == null) || (parameters.Length == 0))
            {
                if (count == 0)
                {
                    candidates.Add(new CandidateMember(candidateMember));
                }
                else
                {
                    error = buildArgCountMismatchError(name, count);
                }
            }
            else
            {
                List<CandidateParameter> signature = new List<CandidateParameter>();
                int length = parameters.Length;
                int num3 = length;
                ParameterInfo paramInfo = parameters[length - 1];
                if (paramInfo.ParameterType.IsArray)
                {
                    object[] customAttributes = paramInfo.GetCustomAttributes(typeof(ParamArrayAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                    {
                        num3--;
                    }
                }
                if (count < num3)
                {
                    error = buildArgCountMismatchError(name, count);
                }
                else if ((num3 == length) && (count != length))
                {
                    error = buildArgCountMismatchError(name, count);
                }
                else
                {
                    int index = 0;
                    while (index < num3)
                    {
                        CandidateParameter item = new CandidateParameter(parameters[index]);
                        if (!item.Match(arguments[index], name, index + 1, out error))
                        {
                            break;
                        }
                        signature.Add(item);
                        index++;
                    }
                    if (index == num3)
                    {
                        if (num3 < length)
                        {
                            CandidateMember member = null;
                            if (count == num3)
                            {
                                member = new CandidateMember(candidateMember, parameters, signature, CandidateMember.Form.Expanded);
                            }
                            else if (count == length)
                            {
                                CandidateParameter parameter2 = new CandidateParameter(paramInfo);
                                if (parameter2.Match(arguments[index], name, index + 1, out error))
                                {
                                    signature.Add(parameter2);
                                    member = new CandidateMember(candidateMember, parameters, signature, CandidateMember.Form.Normal);
                                }
                            }
                            if (member == null)
                            {
                                CandidateParameter parameter3 = new CandidateParameter(paramInfo.ParameterType.GetElementType());
                                while (index < count)
                                {
                                    if (!parameter3.Match(arguments[index], name, index + 1, out error))
                                    {
                                        return;
                                    }
                                    signature.Add(parameter3);
                                    index++;
                                }
                                member = new CandidateMember(candidateMember, parameters, signature, CandidateMember.Form.Expanded);
                            }
                            candidates.Add(member);
                        }
                        else
                        {
                            candidates.Add(new CandidateMember(candidateMember, parameters, signature, CandidateMember.Form.Normal));
                        }
                    }
                }
            }
        }

        internal static bool ExplicitConversionSpecified(Type fromType, Type toType, out ValidationError error)
        {
            if (!StandardImplicitConversion(fromType, toType, null, out error))
            {
                ValidationError error2;
                if (error != null)
                {
                    return false;
                }
                if ((fromType.IsValueType && toType.IsValueType) && IsExplicitNumericConversion(fromType, toType))
                {
                    return true;
                }
                if (StandardImplicitConversion(toType, fromType, null, out error2))
                {
                    return true;
                }
                if (toType.IsInterface)
                {
                    if (fromType.IsClass && !fromType.IsSealed)
                    {
                        return true;
                    }
                    if (fromType.IsInterface)
                    {
                        return true;
                    }
                }
                if ((fromType.IsInterface && toType.IsClass) && (!toType.IsSealed || InterfaceMatch(toType.GetInterfaces(), fromType)))
                {
                    return true;
                }
                if (FindExplicitConversion(fromType, toType, out error) == null)
                {
                    return false;
                }
            }
            return true;
        }

        public RuleExpressionInfo ExpressionInfo(CodeExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            RuleExpressionInfo info = null;
            this.expressionInfoMap.TryGetValue(expression, out info);
            return info;
        }

        internal MethodInfo FindBestCandidate(Type targetType, List<MethodInfo> methods, params Type[] types)
        {
            List<Argument> arguments = new List<Argument>();
            foreach (Type type in types)
            {
                arguments.Add(new Argument(type));
            }
            List<CandidateMember> candidates = new List<CandidateMember>(methods.Count);
            foreach (MethodInfo info in methods)
            {
                ValidationError error = null;
                EvaluateCandidate(candidates, info, info.GetParameters(), arguments, out error, (name, numArguments) => new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.MethodArgCountMismatch, new object[] { name, numArguments }), 0x196));
            }
            if (candidates.Count == 0)
            {
                return null;
            }
            CandidateMember member = this.FindBestCandidate(targetType, candidates, arguments);
            if (member == null)
            {
                return null;
            }
            return (MethodInfo) member.Member;
        }

        private CandidateMember FindBestCandidate(Type targetType, List<CandidateMember> candidates, List<Argument> arguments)
        {
            int count = candidates.Count;
            List<CandidateMember> list = new List<CandidateMember>(1) {
                candidates[0]
            };
            for (int i = 1; i < count; i++)
            {
                CandidateMember item = candidates[i];
                CandidateMember other = list[0];
                int num3 = item.CompareMember(targetType, other, arguments, this);
                if (num3 > 0)
                {
                    list.Clear();
                    list.Add(item);
                }
                else if (num3 == 0)
                {
                    list.Add(item);
                }
            }
            if (list.Count == 1)
            {
                return list[0];
            }
            return null;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static MethodInfo FindExplicitConversion(Type fromType, Type toType, out ValidationError error)
        {
            ValidationError error2;
            List<MethodInfo> methods = new List<MethodInfo>();
            bool flag = ConditionHelper.IsNullableValueType(fromType);
            bool flag2 = ConditionHelper.IsNullableValueType(toType);
            Type t = flag ? Nullable.GetUnderlyingType(fromType) : fromType;
            Type type2 = flag2 ? Nullable.GetUnderlyingType(toType) : toType;
            if (t.IsClass)
            {
                AddExplicitConversions(t, fromType, toType, methods);
                for (Type type3 = t.BaseType; (type3 != null) && (type3 != typeof(object)); type3 = type3.BaseType)
                {
                    AddExplicitConversions(type3, fromType, toType, methods);
                }
            }
            else if (IsStruct(t))
            {
                AddExplicitConversions(t, fromType, toType, methods);
            }
            if (type2.IsClass)
            {
                AddExplicitConversions(type2, fromType, toType, methods);
                for (Type type4 = type2.BaseType; (type4 != null) && (type4 != typeof(object)); type4 = type4.BaseType)
                {
                    AddExplicitConversions(type4, fromType, toType, methods);
                }
            }
            else if (IsStruct(type2))
            {
                AddExplicitConversions(type2, fromType, toType, methods);
            }
            if (flag && flag2)
            {
                List<MethodInfo> list2 = new List<MethodInfo>();
                if (t.IsClass)
                {
                    AddExplicitConversions(t, t, type2, list2);
                    for (Type type5 = t.BaseType; (type5 != null) && (type5 != typeof(object)); type5 = type5.BaseType)
                    {
                        AddExplicitConversions(type5, t, type2, list2);
                    }
                }
                else if (IsStruct(t))
                {
                    AddExplicitConversions(t, t, type2, list2);
                }
                if (type2.IsClass)
                {
                    AddExplicitConversions(type2, t, type2, list2);
                    for (Type type6 = type2.BaseType; (type6 != null) && (type6 != typeof(object)); type6 = type6.BaseType)
                    {
                        AddExplicitConversions(type6, t, type2, list2);
                    }
                }
                else if (IsStruct(type2))
                {
                    AddExplicitConversions(type2, t, type2, list2);
                }
                foreach (MethodInfo info in list2)
                {
                    methods.Add(new LiftedConversionMethodInfo(info));
                }
            }
            if (methods.Count == 0)
            {
                string str = string.Format(CultureInfo.CurrentCulture, Messages.NoConversion, new object[] { RuleDecompiler.DecompileType(fromType), RuleDecompiler.DecompileType(toType) });
                error = new ValidationError(str, 0x545);
                return null;
            }
            Type lhsType = null;
            for (int i = 0; i < methods.Count; i++)
            {
                if (methods[i].GetParameters()[0].ParameterType == fromType)
                {
                    lhsType = fromType;
                    break;
                }
            }
            if (lhsType == null)
            {
                for (int m = 0; m < methods.Count; m++)
                {
                    Type parameterType = methods[m].GetParameters()[0].ParameterType;
                    if (StandardImplicitConversion(fromType, parameterType, null, out error2))
                    {
                        if (lhsType == null)
                        {
                            lhsType = parameterType;
                        }
                        else if (StandardImplicitConversion(parameterType, lhsType, null, out error2))
                        {
                            lhsType = parameterType;
                        }
                    }
                }
            }
            if (lhsType == null)
            {
                for (int n = 0; n < methods.Count; n++)
                {
                    Type type10 = methods[n].GetParameters()[0].ParameterType;
                    if (StandardImplicitConversion(type10, fromType, null, out error2))
                    {
                        if (lhsType == null)
                        {
                            lhsType = type10;
                        }
                        else if (StandardImplicitConversion(lhsType, type10, null, out error2))
                        {
                            lhsType = type10;
                        }
                    }
                }
            }
            Type rhsType = null;
            for (int j = 0; j < methods.Count; j++)
            {
                if (methods[j].ReturnType == toType)
                {
                    rhsType = toType;
                    break;
                }
            }
            if (rhsType == null)
            {
                for (int num5 = 0; num5 < methods.Count; num5++)
                {
                    Type returnType = methods[num5].ReturnType;
                    if (StandardImplicitConversion(returnType, toType, null, out error2))
                    {
                        if (rhsType == null)
                        {
                            rhsType = returnType;
                        }
                        else if (StandardImplicitConversion(rhsType, returnType, null, out error2))
                        {
                            rhsType = returnType;
                        }
                    }
                }
            }
            if (rhsType == null)
            {
                for (int num6 = 0; num6 < methods.Count; num6++)
                {
                    Type type14 = methods[num6].ReturnType;
                    if (StandardImplicitConversion(toType, type14, null, out error2))
                    {
                        if (rhsType == null)
                        {
                            rhsType = type14;
                        }
                        else if (StandardImplicitConversion(type14, rhsType, null, out error2))
                        {
                            rhsType = type14;
                        }
                    }
                }
            }
            int num7 = 0;
            int num8 = 0;
            for (int k = 0; k < methods.Count; k++)
            {
                if (((methods[k].ReturnType == rhsType) && (methods[k].GetParameters()[0].ParameterType == lhsType)) && !(methods[k] is LiftedConversionMethodInfo))
                {
                    num8 = k;
                    num7++;
                }
            }
            if (num7 == 1)
            {
                error = null;
                return methods[num8];
            }
            if (flag2 && (num7 == 0))
            {
                if (flag)
                {
                    for (int num10 = 0; num10 < methods.Count; num10++)
                    {
                        if (((methods[num10].ReturnType == rhsType) && (methods[num10].GetParameters()[0].ParameterType == lhsType)) && (methods[num10] is LiftedConversionMethodInfo))
                        {
                            num8 = num10;
                            num7++;
                        }
                    }
                    if (num7 == 1)
                    {
                        error = null;
                        return methods[num8];
                    }
                }
                else
                {
                    MethodInfo method = FindExplicitConversion(fromType, type2, out error);
                    if (method != null)
                    {
                        error = null;
                        return new LiftedConversionMethodInfo(method);
                    }
                }
            }
            string errorText = string.Format(CultureInfo.CurrentCulture, Messages.AmbiguousConversion, new object[] { RuleDecompiler.DecompileType(fromType), RuleDecompiler.DecompileType(toType) });
            error = new ValidationError(errorText, 0x545);
            return null;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static MethodInfo FindImplicitConversion(Type fromType, Type toType, out ValidationError error)
        {
            ValidationError error2;
            List<MethodInfo> methods = new List<MethodInfo>();
            bool flag = ConditionHelper.IsNullableValueType(fromType);
            bool flag2 = ConditionHelper.IsNullableValueType(toType);
            Type t = flag ? Nullable.GetUnderlyingType(fromType) : fromType;
            Type type = flag2 ? Nullable.GetUnderlyingType(toType) : toType;
            if (t.IsClass)
            {
                AddImplicitConversions(t, fromType, toType, methods);
                for (Type type3 = t.BaseType; (type3 != null) && (type3 != typeof(object)); type3 = type3.BaseType)
                {
                    AddImplicitConversions(type3, fromType, toType, methods);
                }
            }
            else if (IsStruct(t))
            {
                AddImplicitConversions(t, fromType, toType, methods);
            }
            if (type.IsClass || IsStruct(type))
            {
                AddImplicitConversions(type, fromType, toType, methods);
            }
            if (flag && flag2)
            {
                List<MethodInfo> list2 = new List<MethodInfo>();
                if (t.IsClass)
                {
                    AddImplicitConversions(t, t, type, list2);
                    for (Type type4 = t.BaseType; (type4 != null) && (type4 != typeof(object)); type4 = type4.BaseType)
                    {
                        AddImplicitConversions(type4, t, type, list2);
                    }
                }
                else if (IsStruct(t))
                {
                    AddImplicitConversions(t, t, type, list2);
                }
                if (type.IsClass || IsStruct(type))
                {
                    AddImplicitConversions(type, t, type, list2);
                }
                foreach (MethodInfo info in list2)
                {
                    ParameterInfo[] parameters = info.GetParameters();
                    if (ConditionHelper.IsNonNullableValueType(info.ReturnType) && ConditionHelper.IsNonNullableValueType(parameters[0].ParameterType))
                    {
                        methods.Add(new LiftedConversionMethodInfo(info));
                    }
                }
            }
            if (methods.Count == 0)
            {
                string str = string.Format(CultureInfo.CurrentCulture, Messages.NoConversion, new object[] { RuleDecompiler.DecompileType(fromType), RuleDecompiler.DecompileType(toType) });
                error = new ValidationError(str, 0x545);
                return null;
            }
            Type parameterType = methods[0].GetParameters()[0].ParameterType;
            if (parameterType != fromType)
            {
                for (int j = 1; j < methods.Count; j++)
                {
                    Type rhsType = methods[j].GetParameters()[0].ParameterType;
                    if (rhsType == fromType)
                    {
                        parameterType = fromType;
                        break;
                    }
                    if (StandardImplicitConversion(rhsType, parameterType, null, out error2))
                    {
                        parameterType = rhsType;
                    }
                }
            }
            Type returnType = methods[0].ReturnType;
            if (returnType != toType)
            {
                for (int k = 1; k < methods.Count; k++)
                {
                    Type lhsType = methods[k].ReturnType;
                    if (lhsType == toType)
                    {
                        returnType = toType;
                        break;
                    }
                    if (StandardImplicitConversion(returnType, lhsType, null, out error2))
                    {
                        returnType = lhsType;
                    }
                }
            }
            int num3 = 0;
            int num4 = 0;
            for (int i = 0; i < methods.Count; i++)
            {
                if (((methods[i].ReturnType == returnType) && (methods[i].GetParameters()[0].ParameterType == parameterType)) && !(methods[i] is LiftedConversionMethodInfo))
                {
                    num4 = i;
                    num3++;
                }
            }
            if (num3 == 1)
            {
                error = null;
                return methods[num4];
            }
            if (flag2 && (num3 == 0))
            {
                if (flag)
                {
                    for (int m = 0; m < methods.Count; m++)
                    {
                        if (((methods[m].ReturnType == returnType) && (methods[m].GetParameters()[0].ParameterType == parameterType)) && (methods[m] is LiftedConversionMethodInfo))
                        {
                            num4 = m;
                            num3++;
                        }
                    }
                    if (num3 == 1)
                    {
                        error = null;
                        return methods[num4];
                    }
                }
                else
                {
                    MethodInfo method = FindImplicitConversion(fromType, type, out error);
                    if (method != null)
                    {
                        error = null;
                        return new LiftedConversionMethodInfo(method);
                    }
                }
            }
            string errorText = string.Format(CultureInfo.CurrentCulture, Messages.AmbiguousConversion, new object[] { RuleDecompiler.DecompileType(fromType), RuleDecompiler.DecompileType(toType) });
            error = new ValidationError(errorText, 0x545);
            return null;
        }

        private Type FindType(string typeName)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            Type type = null;
            if (!this.typesUsed.TryGetValue(typeName, out type))
            {
                type = this.typeProvider.GetType(typeName, false);
                if (type != null)
                {
                    this.typesUsed.Add(typeName, type);
                    this.IsAuthorized(type);
                }
            }
            return type;
        }

        private static List<CandidateMember> GetCandidateConstructors(List<ConstructorInfo> constructors, List<Argument> arguments, out ValidationError error)
        {
            List<CandidateMember> candidates = new List<CandidateMember>();
            error = null;
            int num = 0;
            foreach (ConstructorInfo info in constructors)
            {
                ValidationError error2 = null;
                EvaluateCandidate(candidates, info, info.GetParameters(), arguments, out error2, (name, numArguments) => new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.MethodArgCountMismatch, new object[] { name, numArguments }), 0x196));
                error = error2;
                if (error2 != null)
                {
                    num++;
                }
            }
            if (candidates.Count == 0)
            {
                if (num > 1)
                {
                    string errorText = string.Format(CultureInfo.CurrentCulture, Messages.ConstructorOverloadNotFound, new object[0]);
                    error = new ValidationError(errorText, 0x199);
                }
                return null;
            }
            error = null;
            return candidates;
        }

        private static List<CandidateMember> GetCandidateIndexers(List<PropertyInfo> indexerProperties, List<Argument> arguments, out ValidationError error)
        {
            List<CandidateMember> candidates = new List<CandidateMember>();
            error = null;
            int num = 0;
            foreach (PropertyInfo info in indexerProperties)
            {
                ValidationError error2 = null;
                EvaluateCandidate(candidates, info, info.GetIndexParameters(), arguments, out error2, (propName, numArguments) => new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.IndexerCountMismatch, new object[] { numArguments }), 0x19f));
                error = error2;
                if (error2 != null)
                {
                    num++;
                }
            }
            if (candidates.Count == 0)
            {
                if (num > 1)
                {
                    string errorText = string.Format(CultureInfo.CurrentCulture, Messages.IndexerOverloadNotFound, new object[0]);
                    error = new ValidationError(errorText, 0x1a1);
                }
                return null;
            }
            error = null;
            return candidates;
        }

        private static List<CandidateMember> GetCandidateMethods(string methodName, List<MethodInfo> methods, List<Argument> arguments, out ValidationError error)
        {
            List<CandidateMember> candidates = new List<CandidateMember>();
            error = null;
            int num = 0;
            foreach (MethodInfo info in methods)
            {
                ValidationError error2 = null;
                EvaluateCandidate(candidates, info, info.GetParameters(), arguments, out error2, (name, numArguments) => new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.MethodArgCountMismatch, new object[] { name, numArguments }), 0x196));
                error = error2;
                if (error2 != null)
                {
                    num++;
                }
            }
            if (candidates.Count == 0)
            {
                if (num > 1)
                {
                    string errorText = string.Format(CultureInfo.CurrentCulture, Messages.MethodOverloadNotFound, new object[] { methodName });
                    error = new ValidationError(errorText, 0x199);
                }
                return null;
            }
            error = null;
            return candidates;
        }

        private static List<Type> GetCandidateTargetTypes(Type targetType)
        {
            if (targetType.IsInterface)
            {
                List<Type> list = new List<Type> {
                    targetType
                };
                for (int i = 0; i < list.Count; i++)
                {
                    list.AddRange(list[i].GetInterfaces());
                }
                list.Add(typeof(object));
                return list;
            }
            return new List<Type>(1) { targetType };
        }

        internal static List<ConstructorInfo> GetConstructors(List<Type> targetTypes, BindingFlags constructorBindingFlags)
        {
            List<ConstructorInfo> list = new List<ConstructorInfo>();
            for (int i = 0; i < targetTypes.Count; i++)
            {
                Type type = targetTypes[i];
                foreach (ConstructorInfo info in type.GetConstructors(constructorBindingFlags))
                {
                    if ((!info.IsGenericMethod && !info.IsStatic) && (!info.IsPrivate && !info.IsFamily))
                    {
                        list.Add(info);
                    }
                }
            }
            return list;
        }

        private static Type GetDefaultExtensionAttribute()
        {
            return Type.GetType("System.Runtime.CompilerServices.ExtensionAttribute, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false);
        }

        private static List<PropertyInfo> GetIndexerProperties(List<Type> candidateTypes, BindingFlags bindingFlags)
        {
            List<PropertyInfo> list = new List<PropertyInfo>();
            foreach (Type type in candidateTypes)
            {
                object[] customAttributes = type.GetCustomAttributes(typeof(DefaultMemberAttribute), true);
                if ((customAttributes != null) && (customAttributes.Length != 0))
                {
                    DefaultMemberAttribute[] attributeArray = (DefaultMemberAttribute[]) customAttributes;
                    foreach (PropertyInfo info in type.GetProperties(bindingFlags))
                    {
                        bool flag = false;
                        for (int i = 0; i < attributeArray.Length; i++)
                        {
                            if (attributeArray[i].MemberName == info.Name)
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (flag && (info.GetIndexParameters().Length > 0))
                        {
                            list.Add(info);
                        }
                    }
                }
            }
            return list;
        }

        private List<MethodInfo> GetNamedMethods(List<Type> targetTypes, string methodName, BindingFlags methodBindingFlags)
        {
            List<MethodInfo> list = new List<MethodInfo>();
            List<ExtensionMethodInfo> extensionMethods = this.ExtensionMethods;
            for (int i = 0; i < targetTypes.Count; i++)
            {
                Type rhsType = targetTypes[i];
                foreach (MethodInfo info in rhsType.GetMember(methodName, MemberTypes.Method, methodBindingFlags))
                {
                    if (!info.IsGenericMethod)
                    {
                        list.Add(info);
                    }
                }
                foreach (ExtensionMethodInfo info2 in extensionMethods)
                {
                    ValidationError error;
                    if ((info2.Name == methodName) && TypesAreAssignable(rhsType, info2.AssumedDeclaringType, null, out error))
                    {
                        list.Add(info2);
                    }
                }
            }
            return list;
        }

        private static PropertyInfo GetProperty(Type targetType, string propertyName, BindingFlags bindingFlags)
        {
            foreach (PropertyInfo info in targetType.GetMember(propertyName, MemberTypes.Property, bindingFlags))
            {
                ParameterInfo[] indexParameters = info.GetIndexParameters();
                if ((indexParameters == null) || (indexParameters.Length == 0))
                {
                    return info;
                }
            }
            return null;
        }

        internal ITypeProvider GetTypeProvider()
        {
            return this.typeProvider;
        }

        internal static bool ImplicitConversion(Type fromType, Type toType)
        {
            ValidationError error;
            return (StandardImplicitConversion(fromType, toType, null, out error) || (FindImplicitConversion(fromType, toType, out error) != null));
        }

        private static bool InterfaceMatch(Type[] types, Type fromType)
        {
            foreach (Type type in types)
            {
                if (type == fromType)
                {
                    return true;
                }
            }
            return false;
        }

        internal void IsAuthorized(Type type)
        {
            if (this.checkStaticType)
            {
                if (this.authorizedTypes != null)
                {
                    while (type.IsArray)
                    {
                        type = type.GetElementType();
                    }
                    if (type.IsGenericType)
                    {
                        this.IsAuthorizedSimpleType(type.GetGenericTypeDefinition());
                        foreach (Type type2 in type.GetGenericArguments())
                        {
                            this.IsAuthorized(type2);
                        }
                    }
                    else
                    {
                        this.IsAuthorizedSimpleType(type);
                    }
                }
                else
                {
                    ValidationError item = new ValidationError(Messages.Error_ConfigFileMissingOrInvalid, 0x178);
                    this.Errors.Add(item);
                }
            }
        }

        private void IsAuthorizedSimpleType(Type type)
        {
            string assemblyQualifiedName = type.AssemblyQualifiedName;
            if (!this.typesUsedAuthorized.ContainsKey(assemblyQualifiedName))
            {
                bool flag = false;
                foreach (AuthorizedType type2 in this.authorizedTypes)
                {
                    if (type2.RegularExpression.IsMatch(assemblyQualifiedName))
                    {
                        flag = string.Compare(bool.TrueString, type2.Authorized, StringComparison.OrdinalIgnoreCase) == 0;
                        if (!flag)
                        {
                            break;
                        }
                    }
                }
                if (!flag)
                {
                    ValidationError item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.Error_TypeNotAuthorized, new object[] { type.FullName }), 0x16b);
                    item.UserData["ErrorObject"] = type;
                    this.Errors.Add(item);
                }
                else
                {
                    this.typesUsedAuthorized.Add(assemblyQualifiedName, type);
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static bool IsExplicitNumericConversion(Type sourceType, Type testType)
        {
            TypeCode code = ConditionHelper.IsNullableValueType(sourceType) ? Type.GetTypeCode(sourceType.GetGenericArguments()[0]) : Type.GetTypeCode(sourceType);
            TypeCode code2 = ConditionHelper.IsNullableValueType(testType) ? Type.GetTypeCode(testType.GetGenericArguments()[0]) : Type.GetTypeCode(testType);
            switch (code)
            {
                case TypeCode.Char:
                    switch (code2)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.SByte:
                    switch (code2)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Byte:
                    switch (code2)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Int16:
                    switch (code2)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.UInt16:
                    switch (code2)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Int32:
                    switch (code2)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.UInt32:
                    switch (code2)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Int64:
                    switch (code2)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.UInt64:
                    switch (code2)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Single:
                    switch (code2)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Double:
                    switch (code2)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Decimal:
                    switch (code2)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
            }
            return false;
        }

        internal static bool IsInternal(FieldInfo fieldInfo)
        {
            if (!fieldInfo.IsAssembly)
            {
                return fieldInfo.IsFamilyAndAssembly;
            }
            return true;
        }

        internal static bool IsInternal(MethodInfo methodInfo)
        {
            if (!methodInfo.IsAssembly)
            {
                return methodInfo.IsFamilyAndAssembly;
            }
            return true;
        }

        private bool IsMarkedExtension(Assembly assembly)
        {
            if (this.extensionAttribute != null)
            {
                object[] customAttributes = assembly.GetCustomAttributes(this.extensionAttribute, false);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsMarkedExtension(MethodInfo mi)
        {
            if (this.extensionAttribute != null)
            {
                object[] customAttributes = mi.GetCustomAttributes(this.extensionAttribute, false);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsMarkedExtension(Type type)
        {
            if (this.extensionAttribute != null)
            {
                object[] customAttributes = type.GetCustomAttributes(this.extensionAttribute, false);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsPrivate(FieldInfo fieldInfo)
        {
            if ((!fieldInfo.IsPrivate && !fieldInfo.IsFamily) && !fieldInfo.IsFamilyOrAssembly)
            {
                return fieldInfo.IsFamilyAndAssembly;
            }
            return true;
        }

        internal static bool IsPrivate(MethodInfo methodInfo)
        {
            if ((!methodInfo.IsPrivate && !methodInfo.IsFamily) && !methodInfo.IsFamilyOrAssembly)
            {
                return methodInfo.IsFamilyAndAssembly;
            }
            return true;
        }

        private static bool IsStruct(Type type)
        {
            return (type.IsValueType && !type.IsPrimitive);
        }

        internal static bool IsValidBooleanResult(Type type)
        {
            if (!(type == typeof(bool)) && !(type == typeof(bool?)))
            {
                return ImplicitConversion(type, typeof(bool));
            }
            return true;
        }

        public void PopParentExpression()
        {
            this.activeParentNodes.Pop();
        }

        public bool PushParentExpression(CodeExpression newParent)
        {
            if (newParent == null)
            {
                throw new ArgumentNullException("newParent");
            }
            if (this.activeParentNodes.Contains(newParent))
            {
                ValidationError item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CyclicalExpression, new object[0]), 0x179);
                item.UserData["ErrorObject"] = newParent;
                this.Errors.Add(item);
                return false;
            }
            this.activeParentNodes.Push(newParent);
            return true;
        }

        internal RuleConstructorExpressionInfo ResolveConstructor(Type targetType, BindingFlags constructorBindingFlags, List<CodeExpression> argumentExprs, out ValidationError error)
        {
            string str;
            List<Argument> arguments = new List<Argument>(argumentExprs.Count);
            foreach (CodeExpression expression in argumentExprs)
            {
                arguments.Add(new Argument(expression, this));
            }
            List<ConstructorInfo> constructors = GetConstructors(GetCandidateTargetTypes(targetType), constructorBindingFlags);
            if (constructors.Count == 0)
            {
                str = string.Format(CultureInfo.CurrentCulture, Messages.UnknownConstructor, new object[] { RuleDecompiler.DecompileType(targetType) });
                error = new ValidationError(str, 0x137);
                return null;
            }
            List<CandidateMember> candidates = GetCandidateConstructors(constructors, arguments, out error);
            if (candidates == null)
            {
                return null;
            }
            CandidateMember member = this.FindBestCandidate(targetType, candidates, arguments);
            if (member == null)
            {
                str = string.Format(CultureInfo.CurrentCulture, Messages.AmbiguousConstructor, new object[] { RuleDecompiler.DecompileType(targetType) });
                error = new ValidationError(str, 0x54a);
                return null;
            }
            return new RuleConstructorExpressionInfo((ConstructorInfo) member.Member, member.IsExpanded);
        }

        internal MemberInfo ResolveFieldOrProperty(Type targetType, string name)
        {
            BindingFlags bindingAttr = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
            if (this.AllowInternalMembers(targetType))
            {
                bindingAttr |= BindingFlags.NonPublic;
            }
            MemberInfo[] infoArray = targetType.GetMember(name, MemberTypes.Property | MemberTypes.Field, bindingAttr);
            if (infoArray != null)
            {
                int length = infoArray.Length;
                if (length == 1)
                {
                    return infoArray[0];
                }
                if (length > 1)
                {
                    for (int i = 0; i < length; i++)
                    {
                        MemberInfo info = infoArray[i];
                        PropertyInfo info2 = (PropertyInfo) info;
                        ParameterInfo[] indexParameters = info2.GetIndexParameters();
                        if ((indexParameters == null) || (indexParameters.Length == 0))
                        {
                            if (info2 != null)
                            {
                                this.IsAuthorized(info2.PropertyType);
                            }
                            return info2;
                        }
                    }
                }
            }
            if (targetType.IsInterface)
            {
                return this.ResolveProperty(targetType, name, bindingAttr);
            }
            return null;
        }

        internal RulePropertyExpressionInfo ResolveIndexerProperty(Type targetType, BindingFlags bindingFlags, List<CodeExpression> argumentExprs, out ValidationError error)
        {
            string str;
            int count = argumentExprs.Count;
            if (count < 1)
            {
                str = string.Format(CultureInfo.CurrentCulture, Messages.IndexerCountMismatch, new object[] { count });
                error = new ValidationError(str, 0x19f);
                return null;
            }
            List<Argument> arguments = new List<Argument>(count);
            foreach (CodeExpression expression in argumentExprs)
            {
                arguments.Add(new Argument(expression, this));
            }
            List<PropertyInfo> indexerProperties = GetIndexerProperties(GetCandidateTargetTypes(targetType), bindingFlags);
            if (indexerProperties.Count == 0)
            {
                str = string.Format(CultureInfo.CurrentCulture, Messages.IndexerNotFound, new object[] { RuleDecompiler.DecompileType(targetType) });
                error = new ValidationError(str, 0x1a0);
                return null;
            }
            List<CandidateMember> candidates = GetCandidateIndexers(indexerProperties, arguments, out error);
            if (candidates == null)
            {
                return null;
            }
            CandidateMember member = this.FindBestCandidate(targetType, candidates, arguments);
            if (member == null)
            {
                str = string.Format(CultureInfo.CurrentCulture, Messages.AmbiguousIndexerMatch, new object[0]);
                error = new ValidationError(str, 0x54a);
                return null;
            }
            PropertyInfo pi = (PropertyInfo) member.Member;
            if (pi != null)
            {
                this.IsAuthorized(pi.PropertyType);
            }
            return new RulePropertyExpressionInfo(pi, pi.PropertyType, member.IsExpanded);
        }

        internal RuleMethodInvokeExpressionInfo ResolveMethod(Type targetType, string methodName, BindingFlags methodBindingFlags, List<CodeExpression> argumentExprs, out ValidationError error)
        {
            string str;
            List<Argument> arguments = new List<Argument>(argumentExprs.Count);
            foreach (CodeExpression expression in argumentExprs)
            {
                arguments.Add(new Argument(expression, this));
            }
            List<Type> candidateTargetTypes = GetCandidateTargetTypes(targetType);
            List<MethodInfo> methods = this.GetNamedMethods(candidateTargetTypes, methodName, methodBindingFlags);
            if (methods.Count == 0)
            {
                str = string.Format(CultureInfo.CurrentCulture, Messages.UnknownMethod, new object[] { methodName, RuleDecompiler.DecompileType(targetType) });
                error = new ValidationError(str, 0x137);
                return null;
            }
            List<CandidateMember> candidates = GetCandidateMethods(methodName, methods, arguments, out error);
            if (candidates == null)
            {
                return null;
            }
            CandidateMember member = this.FindBestCandidate(targetType, candidates, arguments);
            if (member == null)
            {
                str = string.Format(CultureInfo.CurrentCulture, Messages.AmbiguousMatch, new object[] { methodName });
                error = new ValidationError(str, 0x54a);
                return null;
            }
            MethodInfo mi = (MethodInfo) member.Member;
            if (mi != null)
            {
                this.IsAuthorized(mi.ReturnType);
            }
            return new RuleMethodInvokeExpressionInfo(mi, member.IsExpanded);
        }

        internal PropertyInfo ResolveProperty(Type targetType, string propertyName, BindingFlags bindingFlags)
        {
            PropertyInfo info = GetProperty(targetType, propertyName, bindingFlags);
            if ((info == null) && targetType.IsInterface)
            {
                Type[] interfaces = targetType.GetInterfaces();
                List<Type> list = new List<Type>();
                list.AddRange(interfaces);
                for (int i = 0; i < list.Count; i++)
                {
                    info = GetProperty(list[i], propertyName, bindingFlags);
                    if (info != null)
                    {
                        break;
                    }
                    Type[] collection = list[i].GetInterfaces();
                    if (collection.Length > 0)
                    {
                        list.AddRange(collection);
                    }
                }
            }
            if (info != null)
            {
                this.IsAuthorized(info.PropertyType);
            }
            return info;
        }

        internal Type ResolveType(CodeTypeReference typeRef)
        {
            Type type = null;
            if (!this.typeRefMap.TryGetValue(typeRef, out type))
            {
                type = this.FindType(typeRef.BaseType);
                if (type == null)
                {
                    string qualifiedName = typeRef.UserData["QualifiedName"] as string;
                    type = this.ResolveType(qualifiedName);
                    if (type != null)
                    {
                        this.typeRefMap.Add(typeRef, type);
                        return type;
                    }
                    ValidationError item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.UnknownType, new object[] { typeRef.BaseType }), 0x549);
                    item.UserData["ErrorObject"] = typeRef;
                    this.Errors.Add(item);
                    return null;
                }
                if (typeRef.TypeArguments.Count > 0)
                {
                    Type[] typeArguments = new Type[typeRef.TypeArguments.Count];
                    for (int i = 0; i < typeRef.TypeArguments.Count; i++)
                    {
                        CodeTypeReference reference = typeRef.TypeArguments[i];
                        if (reference.BaseType.StartsWith("[", StringComparison.Ordinal))
                        {
                            reference.BaseType = reference.BaseType.Substring(1, reference.BaseType.Length - 2);
                        }
                        typeArguments[i] = this.ResolveType(reference);
                        if (typeArguments[i] == null)
                        {
                            return null;
                        }
                    }
                    type = type.MakeGenericType(typeArguments);
                    if (type == null)
                    {
                        StringBuilder builder = new StringBuilder(typeRef.BaseType);
                        string str3 = "<";
                        foreach (Type type2 in typeArguments)
                        {
                            builder.Append(str3);
                            str3 = ",";
                            builder.Append(RuleDecompiler.DecompileType(type2));
                        }
                        builder.Append(">");
                        ValidationError error2 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.UnknownGenericType, new object[] { builder.ToString() }), 0x549);
                        error2.UserData["ErrorObject"] = typeRef;
                        this.Errors.Add(error2);
                        return null;
                    }
                }
                if (type != null)
                {
                    CodeTypeReference arrayElementType = typeRef;
                    if (arrayElementType.ArrayRank > 0)
                    {
                        do
                        {
                            type = (arrayElementType.ArrayRank == 1) ? type.MakeArrayType() : type.MakeArrayType(arrayElementType.ArrayRank);
                            arrayElementType = arrayElementType.ArrayElementType;
                        }
                        while (arrayElementType.ArrayRank > 0);
                    }
                }
                if (type != null)
                {
                    this.typeRefMap.Add(typeRef, type);
                    typeRef.UserData["QualifiedName"] = type.AssemblyQualifiedName;
                }
            }
            return type;
        }

        internal Type ResolveType(string qualifiedName)
        {
            Type type = null;
            if (qualifiedName != null)
            {
                type = this.typeProvider.GetType(qualifiedName, false);
                if (type == null)
                {
                    type = Type.GetType(qualifiedName, false);
                }
            }
            return type;
        }

        private void SetExtensionAttribute()
        {
            this.extensionAttribute = this.typeProvider.GetType("System.Runtime.CompilerServices.ExtensionAttribute, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false);
            if (this.extensionAttribute == null)
            {
                this.extensionAttribute = defaultExtensionAttribute;
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static bool StandardImplicitConversion(Type rhsType, Type lhsType, CodeExpression rhsExpression, out ValidationError error)
        {
            error = null;
            if (rhsType == lhsType)
            {
                return true;
            }
            if (rhsType == typeof(NullLiteral))
            {
                if (ConditionHelper.IsNonNullableValueType(lhsType))
                {
                    string errorText = string.Format(CultureInfo.CurrentCulture, Messages.AssignNotAllowed, new object[] { Messages.NullValue, RuleDecompiler.DecompileType(lhsType) });
                    error = new ValidationError(errorText, 0x545);
                    return false;
                }
                return true;
            }
            bool flag = ConditionHelper.IsNullableValueType(lhsType);
            if (ConditionHelper.IsNullableValueType(rhsType))
            {
                if (!flag)
                {
                    return (lhsType == typeof(object));
                }
                rhsType = Nullable.GetUnderlyingType(rhsType);
            }
            if (flag)
            {
                lhsType = Nullable.GetUnderlyingType(lhsType);
            }
            if (lhsType == rhsType)
            {
                return true;
            }
            if (TypeProvider.IsAssignable(lhsType, rhsType))
            {
                return true;
            }
            if (lhsType.IsEnum)
            {
                CodePrimitiveExpression expression = rhsExpression as CodePrimitiveExpression;
                if ((expression != null) && (expression.Value != null))
                {
                    switch (Type.GetTypeCode(expression.Value.GetType()))
                    {
                        case TypeCode.Char:
                            return (((char) expression.Value) == '\0');

                        case TypeCode.SByte:
                            return (((sbyte) expression.Value) == 0);

                        case TypeCode.Byte:
                            return (((byte) expression.Value) == 0);

                        case TypeCode.Int16:
                            return (((short) expression.Value) == 0);

                        case TypeCode.UInt16:
                            return (((ushort) expression.Value) == 0);

                        case TypeCode.Int32:
                            return (((int) expression.Value) == 0);

                        case TypeCode.UInt32:
                            return (((uint) expression.Value) == 0);

                        case TypeCode.Int64:
                            return (((long) expression.Value) == 0L);

                        case TypeCode.UInt64:
                            return (((ulong) expression.Value) == 0L);
                    }
                }
                return false;
            }
            if (!rhsType.IsEnum)
            {
                TypeCode typeCode = Type.GetTypeCode(lhsType);
                TypeCode code2 = Type.GetTypeCode(rhsType);
                switch (typeCode)
                {
                    case TypeCode.Char:
                        switch (code2)
                        {
                            case TypeCode.Char:
                                return true;

                            case TypeCode.SByte:
                            case TypeCode.Byte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                                return CheckValueRange(rhsExpression, lhsType, out error);
                        }
                        return false;

                    case TypeCode.SByte:
                        switch (code2)
                        {
                            case TypeCode.Char:
                            case TypeCode.Byte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                                return CheckValueRange(rhsExpression, lhsType, out error);

                            case TypeCode.SByte:
                                return true;
                        }
                        return false;

                    case TypeCode.Byte:
                        switch (code2)
                        {
                            case TypeCode.Char:
                            case TypeCode.SByte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                                return CheckValueRange(rhsExpression, lhsType, out error);

                            case TypeCode.Byte:
                                return true;
                        }
                        return false;

                    case TypeCode.Int16:
                        switch (code2)
                        {
                            case TypeCode.Char:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                                return CheckValueRange(rhsExpression, lhsType, out error);

                            case TypeCode.SByte:
                            case TypeCode.Byte:
                            case TypeCode.Int16:
                                return true;
                        }
                        return false;

                    case TypeCode.UInt16:
                        switch (code2)
                        {
                            case TypeCode.Char:
                            case TypeCode.Byte:
                            case TypeCode.UInt16:
                                return true;

                            case TypeCode.SByte:
                            case TypeCode.Int16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                                return CheckValueRange(rhsExpression, lhsType, out error);
                        }
                        return false;

                    case TypeCode.Int32:
                        switch (code2)
                        {
                            case TypeCode.Char:
                            case TypeCode.SByte:
                            case TypeCode.Byte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                                return true;

                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                                return CheckValueRange(rhsExpression, lhsType, out error);
                        }
                        return false;

                    case TypeCode.UInt32:
                        switch (code2)
                        {
                            case TypeCode.Char:
                            case TypeCode.Byte:
                            case TypeCode.UInt16:
                            case TypeCode.UInt32:
                                return true;

                            case TypeCode.SByte:
                            case TypeCode.Int16:
                            case TypeCode.Int32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                                return CheckValueRange(rhsExpression, lhsType, out error);
                        }
                        return false;

                    case TypeCode.Int64:
                        switch (code2)
                        {
                            case TypeCode.Char:
                            case TypeCode.SByte:
                            case TypeCode.Byte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                                return true;

                            case TypeCode.UInt64:
                                return CheckValueRange(rhsExpression, lhsType, out error);
                        }
                        return false;

                    case TypeCode.UInt64:
                        switch (code2)
                        {
                            case TypeCode.Char:
                            case TypeCode.Byte:
                            case TypeCode.UInt16:
                            case TypeCode.UInt32:
                            case TypeCode.UInt64:
                                return true;

                            case TypeCode.SByte:
                            case TypeCode.Int16:
                            case TypeCode.Int32:
                            case TypeCode.Int64:
                                return CheckValueRange(rhsExpression, lhsType, out error);
                        }
                        return false;

                    case TypeCode.Single:
                        switch (code2)
                        {
                            case TypeCode.Char:
                            case TypeCode.SByte:
                            case TypeCode.Byte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                            case TypeCode.Single:
                                return true;
                        }
                        return false;

                    case TypeCode.Double:
                        switch (code2)
                        {
                            case TypeCode.Char:
                            case TypeCode.SByte:
                            case TypeCode.Byte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                                return true;
                        }
                        return false;

                    case TypeCode.Decimal:
                        switch (code2)
                        {
                            case TypeCode.Char:
                            case TypeCode.SByte:
                            case TypeCode.Byte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                            case TypeCode.Decimal:
                                return true;
                        }
                        return false;
                }
            }
            return false;
        }

        internal static bool TypesAreAssignable(Type rhsType, Type lhsType, CodeExpression rhsExpression, out ValidationError error)
        {
            if (!StandardImplicitConversion(rhsType, lhsType, rhsExpression, out error))
            {
                if (error != null)
                {
                    return false;
                }
                if (FindImplicitConversion(rhsType, lhsType, out error) == null)
                {
                    return false;
                }
            }
            return true;
        }

        internal bool ValidateConditionExpression(CodeExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            RuleExpressionInfo info = RuleExpressionWalker.Validate(this, expression, false);
            if (info == null)
            {
                return false;
            }
            Type expressionType = info.ExpressionType;
            if (!IsValidBooleanResult(expressionType) && ((expressionType != null) || (this.Errors.Count == 0)))
            {
                ValidationError item = new ValidationError(Messages.ConditionMustBeBoolean, 0x547);
                item.UserData["ErrorObject"] = expression;
                this.Errors.Add(item);
            }
            return (this.Errors.Count == 0);
        }

        internal bool ValidateMemberAccess(CodeExpression targetExpression, Type targetType, FieldInfo accessorMethod, string memberName, CodeExpression parentExpr)
        {
            return this.ValidateMemberAccess(targetExpression, targetType, memberName, parentExpr, accessorMethod.DeclaringType.Assembly, IsPrivate(accessorMethod), IsInternal(accessorMethod), accessorMethod.IsStatic);
        }

        internal bool ValidateMemberAccess(CodeExpression targetExpression, Type targetType, MethodInfo accessorMethod, string memberName, CodeExpression parentExpr)
        {
            return this.ValidateMemberAccess(targetExpression, targetType, memberName, parentExpr, accessorMethod.DeclaringType.Assembly, IsPrivate(accessorMethod), IsInternal(accessorMethod), accessorMethod.IsStatic);
        }

        private bool ValidateMemberAccess(CodeExpression targetExpression, Type targetType, string memberName, CodeExpression parentExpr, Assembly methodAssembly, bool isPrivate, bool isInternal, bool isStatic)
        {
            if (isStatic != (targetExpression is CodeTypeReferenceExpression))
            {
                string str;
                int num;
                if (isStatic)
                {
                    str = string.Format(CultureInfo.CurrentCulture, Messages.StaticMember, new object[] { memberName });
                    num = 0x561;
                }
                else
                {
                    str = string.Format(CultureInfo.CurrentCulture, Messages.NonStaticMember, new object[] { memberName });
                    num = 0x562;
                }
                ValidationError item = new ValidationError(str, num);
                item.UserData["ErrorObject"] = parentExpr;
                this.Errors.Add(item);
                return false;
            }
            if (isPrivate && (targetType != this.ThisType))
            {
                ValidationError error2 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CannotAccessPrivateMember, new object[] { memberName, RuleDecompiler.DecompileType(targetType) }), 0x54a);
                error2.UserData["ErrorObject"] = parentExpr;
                this.Errors.Add(error2);
                return false;
            }
            if (isInternal && (this.ThisType.Assembly != methodAssembly))
            {
                ValidationError error3 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CannotAccessInternalMember, new object[] { memberName, RuleDecompiler.DecompileType(targetType) }), 0x54a);
                error3.UserData["ErrorObject"] = parentExpr;
                this.Errors.Add(error3);
                return false;
            }
            return true;
        }

        internal RuleExpressionInfo ValidateSubexpression(CodeExpression expr, RuleExpressionInternal ruleExpr, bool isWritten)
        {
            RuleExpressionInfo info = ruleExpr.Validate(expr, this, isWritten);
            if (info != null)
            {
                this.expressionInfoMap[expr] = info;
            }
            return info;
        }

        public ValidationErrorCollection Errors
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.errors;
            }
        }

        internal List<ExtensionMethodInfo> ExtensionMethods
        {
            get
            {
                if (this.extensionMethods == null)
                {
                    this.DetermineExtensionMethods();
                }
                return this.extensionMethods;
            }
        }

        public Type ThisType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.thisType;
            }
        }

        private class Argument
        {
            internal FieldDirection direction;
            internal CodeExpression expression;
            internal Type type;

            internal Argument(Type type)
            {
                this.direction = FieldDirection.In;
                this.type = type;
            }

            internal Argument(CodeExpression expr, RuleValidation validation)
            {
                this.expression = expr;
                this.direction = FieldDirection.In;
                CodeDirectionExpression expression = expr as CodeDirectionExpression;
                if (expression != null)
                {
                    this.direction = expression.Direction;
                }
                this.type = validation.ExpressionInfo(expr).ExpressionType;
            }
        }

        private delegate ValidationError BuildArgCountMismatchError(string name, int numArguments);

        private class CandidateMember
        {
            private Form form;
            internal MemberInfo Member;
            private ParameterInfo[] memberParameters;
            private static ParameterInfo[] noParameters = new ParameterInfo[0];
            private static List<RuleValidation.CandidateParameter> noSignature = new List<RuleValidation.CandidateParameter>();
            private List<RuleValidation.CandidateParameter> signature;

            internal CandidateMember(MemberInfo member) : this(member, noParameters, noSignature, Form.Normal)
            {
            }

            internal CandidateMember(MemberInfo member, ParameterInfo[] parameters, List<RuleValidation.CandidateParameter> signature, Form form)
            {
                this.Member = member;
                this.memberParameters = parameters;
                this.signature = signature;
                this.form = form;
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
            internal int CompareMember(Type targetType, RuleValidation.CandidateMember other, List<RuleValidation.Argument> arguments, RuleValidation validator)
            {
                int num = 1;
                int num2 = -1;
                int num3 = 0;
                Type declaringType = this.Member.DeclaringType;
                Type toType = other.Member.DeclaringType;
                if (declaringType != toType)
                {
                    if (TypeProvider.IsAssignable(toType, declaringType))
                    {
                        return num;
                    }
                    if (TypeProvider.IsAssignable(declaringType, toType))
                    {
                        return num2;
                    }
                }
                bool flag = false;
                bool flag2 = false;
                bool flag3 = true;
                ExtensionMethodInfo member = this.Member as ExtensionMethodInfo;
                ExtensionMethodInfo info2 = other.Member as ExtensionMethodInfo;
                if ((member == null) && (info2 != null))
                {
                    return num;
                }
                if ((member != null) && (info2 == null))
                {
                    return num2;
                }
                if ((member != null) && (info2 != null))
                {
                    string[] test = member.DeclaringType.FullName.Split(new char[] { '.' });
                    string[] strArray2 = info2.DeclaringType.FullName.Split(new char[] { '.' });
                    string[] reference = validator.thisType.FullName.Split(new char[] { '.' });
                    int num4 = MatchNameSpace(test, reference);
                    int num5 = MatchNameSpace(strArray2, reference);
                    if (num4 > num5)
                    {
                        return num;
                    }
                    if (num4 < num5)
                    {
                        return num2;
                    }
                    RuleValidation.CandidateParameter parameter = new RuleValidation.CandidateParameter(member.AssumedDeclaringType);
                    RuleValidation.CandidateParameter parameter2 = new RuleValidation.CandidateParameter(info2.AssumedDeclaringType);
                    if (!parameter.Equals(parameter2))
                    {
                        flag3 = false;
                        int num6 = parameter.CompareConversion(parameter2, new RuleValidation.Argument(targetType));
                        if (num6 < 0)
                        {
                            flag2 = true;
                        }
                        else if (num6 > 0)
                        {
                            flag = true;
                        }
                    }
                    for (int i = 0; i < arguments.Count; i++)
                    {
                        RuleValidation.CandidateParameter parameter3 = this.signature[i];
                        RuleValidation.CandidateParameter parameter4 = other.signature[i];
                        if (!parameter3.Equals(parameter4))
                        {
                            flag3 = false;
                        }
                        int num8 = parameter3.CompareConversion(parameter4, arguments[i]);
                        if (num8 < 0)
                        {
                            flag2 = true;
                        }
                        else if (num8 > 0)
                        {
                            flag = true;
                        }
                    }
                    if (flag && !flag2)
                    {
                        return num;
                    }
                    if (!flag && flag2)
                    {
                        return num2;
                    }
                }
                else
                {
                    for (int j = 0; j < arguments.Count; j++)
                    {
                        RuleValidation.CandidateParameter parameter5 = this.signature[j];
                        RuleValidation.CandidateParameter parameter6 = other.signature[j];
                        if (!parameter5.Equals(parameter6))
                        {
                            flag3 = false;
                        }
                        int num10 = parameter5.CompareConversion(parameter6, arguments[j]);
                        if (num10 < 0)
                        {
                            return num2;
                        }
                        if (num10 > 0)
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        return num;
                    }
                }
                if (flag3)
                {
                    if ((this.form == Form.Normal) && (other.form == Form.Expanded))
                    {
                        return num;
                    }
                    if ((this.form == Form.Expanded) && (other.form == Form.Normal))
                    {
                        return num2;
                    }
                    if ((this.form != Form.Expanded) || (other.form != Form.Expanded))
                    {
                        return num3;
                    }
                    int length = this.memberParameters.Length;
                    int num12 = other.memberParameters.Length;
                    if (length > num12)
                    {
                        return num;
                    }
                    if (num12 > length)
                    {
                        return num2;
                    }
                }
                return num3;
            }

            private static int MatchNameSpace(string[] test, string[] reference)
            {
                int num2 = Math.Min(test.Length, reference.Length);
                int index = 0;
                while (index < num2)
                {
                    if (test[index] != reference[index])
                    {
                        return index;
                    }
                    index++;
                }
                return index;
            }

            internal bool IsExpanded
            {
                get
                {
                    return (this.form == Form.Expanded);
                }
            }

            internal enum Form
            {
                Normal,
                Expanded
            }
        }

        private class CandidateParameter
        {
            private FieldDirection direction;
            private Type type;

            internal CandidateParameter(ParameterInfo paramInfo)
            {
                this.direction = FieldDirection.In;
                if (paramInfo.IsOut)
                {
                    this.direction = FieldDirection.Out;
                }
                else if (paramInfo.ParameterType.IsByRef)
                {
                    this.direction = FieldDirection.Ref;
                }
                this.type = paramInfo.ParameterType;
            }

            internal CandidateParameter(Type type)
            {
                this.type = type;
                this.direction = FieldDirection.In;
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
            private static bool BetterSignedConversion(Type t1, Type t2)
            {
                TypeCode typeCode = Type.GetTypeCode(t1);
                TypeCode code2 = Type.GetTypeCode(t2);
                switch (typeCode)
                {
                    case TypeCode.SByte:
                        switch (code2)
                        {
                            case TypeCode.Byte:
                            case TypeCode.UInt16:
                            case TypeCode.UInt32:
                            case TypeCode.UInt64:
                                return true;
                        }
                        break;

                    case TypeCode.Int16:
                        switch (code2)
                        {
                            case TypeCode.UInt16:
                            case TypeCode.UInt32:
                            case TypeCode.UInt64:
                                return true;
                        }
                        break;

                    case TypeCode.Int32:
                        if ((code2 != TypeCode.UInt32) && (code2 != TypeCode.UInt64))
                        {
                            break;
                        }
                        return true;

                    case TypeCode.Int64:
                        if (code2 != TypeCode.UInt64)
                        {
                            break;
                        }
                        return true;

                    case TypeCode.Object:
                        if (!ConditionHelper.IsNullableValueType(t1))
                        {
                            return false;
                        }
                        t1 = t1.GetGenericArguments()[0];
                        if (ConditionHelper.IsNullableValueType(t2))
                        {
                            t2 = t2.GetGenericArguments()[0];
                        }
                        return BetterSignedConversion(t1, t2);
                }
                return false;
            }

            internal int CompareConversion(RuleValidation.CandidateParameter otherParam, RuleValidation.Argument argument)
            {
                int num = 1;
                int num2 = -1;
                if (this.type != otherParam.type)
                {
                    ValidationError error;
                    if (argument.type == this.type)
                    {
                        return num;
                    }
                    if (argument.type == otherParam.type)
                    {
                        return num2;
                    }
                    bool flag = RuleValidation.TypesAreAssignable(this.type, otherParam.type, null, out error);
                    bool flag2 = RuleValidation.TypesAreAssignable(otherParam.type, this.type, null, out error);
                    if (flag && !flag2)
                    {
                        return num;
                    }
                    if (flag2 && !flag)
                    {
                        return num2;
                    }
                    if (BetterSignedConversion(this.type, otherParam.type))
                    {
                        return num;
                    }
                    if (BetterSignedConversion(otherParam.type, this.type))
                    {
                        return num2;
                    }
                }
                return 0;
            }

            public override bool Equals(object obj)
            {
                RuleValidation.CandidateParameter parameter = obj as RuleValidation.CandidateParameter;
                if (parameter == null)
                {
                    return false;
                }
                return ((this.direction == parameter.direction) && (this.type == parameter.type));
            }

            public override int GetHashCode()
            {
                return (this.direction.GetHashCode() ^ this.type.GetHashCode());
            }

            internal bool Match(RuleValidation.Argument argument, string methodName, int argPosition, out ValidationError error)
            {
                string str;
                if (this.direction == argument.direction)
                {
                    if (this.type.IsByRef && (this.type != argument.type))
                    {
                        str = string.Format(CultureInfo.CurrentCulture, Messages.MethodArgumentTypeMismatch, new object[] { argPosition, methodName, RuleDecompiler.DecompileType(argument.type), RuleDecompiler.DecompileType(this.type) });
                        error = new ValidationError(str, 0x198);
                        return false;
                    }
                    if (RuleValidation.TypesAreAssignable(argument.type, this.type, argument.expression, out error))
                    {
                        return true;
                    }
                    if (error == null)
                    {
                        str = string.Format(CultureInfo.CurrentCulture, Messages.MethodArgumentTypeMismatch, new object[] { argPosition, methodName, RuleDecompiler.DecompileType(argument.type), RuleDecompiler.DecompileType(this.type) });
                        error = new ValidationError(str, 0x198);
                    }
                    return false;
                }
                string str2 = "";
                switch (this.direction)
                {
                    case FieldDirection.In:
                        str2 = "in";
                        break;

                    case FieldDirection.Out:
                        str2 = "out";
                        break;

                    case FieldDirection.Ref:
                        str2 = "ref";
                        break;
                }
                str = string.Format(CultureInfo.CurrentCulture, Messages.MethodDirectionMismatch, new object[] { argPosition, methodName, str2 });
                error = new ValidationError(str, 0x197);
                return false;
            }
        }
    }
}

