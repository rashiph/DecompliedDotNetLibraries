namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Workflow.ComponentModel.Compiler;

    internal class ParserContext
    {
        internal ICollection completions;
        private int currentToken;
        internal Dictionary<object, int> exprPositions;
        internal bool provideIntellisense;
        private List<Token> tokens;

        internal ParserContext(string expressionString)
        {
            this.exprPositions = new Dictionary<object, int>();
            Scanner scanner = new Scanner(expressionString);
            this.tokens = new List<Token>();
            scanner.Tokenize(this.tokens);
        }

        internal ParserContext(List<Token> tokens)
        {
            this.exprPositions = new Dictionary<object, int>();
            this.provideIntellisense = true;
            this.tokens = tokens;
        }

        private static void AddCandidates(List<MemberInfo> candidateMethods, MemberInfo[] methods)
        {
            if (methods != null)
            {
                for (int i = 0; i < methods.Length; i++)
                {
                    MethodInfo item = (MethodInfo) methods[i];
                    if (!item.IsGenericMethod)
                    {
                        candidateMethods.Add(item);
                    }
                }
            }
        }

        internal static bool IsNonPrivate(FieldInfo fieldInfo, Type thisType)
        {
            if ((fieldInfo.IsPublic || fieldInfo.IsFamily) || fieldInfo.IsFamilyOrAssembly)
            {
                return true;
            }
            if (!fieldInfo.IsAssembly && !fieldInfo.IsFamilyAndAssembly)
            {
                return false;
            }
            return (fieldInfo.DeclaringType.Assembly == thisType.Assembly);
        }

        internal static bool IsNonPrivate(MethodInfo methodInfo, Type thisType)
        {
            if ((methodInfo.IsPublic || methodInfo.IsFamily) || methodInfo.IsFamilyOrAssembly)
            {
                return true;
            }
            if (!methodInfo.IsAssembly && !methodInfo.IsFamilyAndAssembly)
            {
                return false;
            }
            return (methodInfo.DeclaringType.Assembly == thisType.Assembly);
        }

        internal static bool IsNonPrivate(Type type, Type thisType)
        {
            if (type.IsPublic || type.IsNestedPublic)
            {
                return true;
            }
            if ((!type.IsNestedAssembly && !type.IsNestedFamANDAssem) && !type.IsNestedFamORAssem)
            {
                return false;
            }
            return (type.Assembly == thisType.Assembly);
        }

        internal Token NextToken()
        {
            if (this.currentToken == (this.tokens.Count - 1))
            {
                this.currentToken++;
                return null;
            }
            this.currentToken++;
            return this.tokens[this.currentToken];
        }

        internal void RestoreCurrentToken(int tokenValue)
        {
            this.currentToken = tokenValue;
        }

        internal int SaveCurrentToken()
        {
            return this.currentToken;
        }

        internal void SetConstructorCompletions(Type computedType, Type thisType)
        {
            BindingFlags constructorBindingFlags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance;
            if (computedType.Assembly == thisType.Assembly)
            {
                constructorBindingFlags |= BindingFlags.NonPublic;
            }
            List<Type> targetTypes = new List<Type>(1) {
                computedType
            };
            this.completions = RuleValidation.GetConstructors(targetTypes, constructorBindingFlags);
        }

        internal void SetMethodCompletions(Type computedType, Type thisType, string methodName, bool includeStatic, bool includeInstance, RuleValidation validation)
        {
            BindingFlags @public = BindingFlags.Public;
            if (computedType.Assembly == thisType.Assembly)
            {
                @public |= BindingFlags.NonPublic;
            }
            if (includeInstance)
            {
                @public |= BindingFlags.Instance;
            }
            if (includeStatic)
            {
                @public |= BindingFlags.FlattenHierarchy | BindingFlags.Static;
            }
            List<MemberInfo> candidateMethods = new List<MemberInfo>();
            MemberInfo[] methods = computedType.GetMember(methodName, MemberTypes.Method, @public);
            AddCandidates(candidateMethods, methods);
            if (computedType.IsInterface)
            {
                List<Type> list2 = new List<Type>();
                list2.AddRange(computedType.GetInterfaces());
                for (int i = 0; i < list2.Count; i++)
                {
                    methods = list2[i].GetMember(methodName, MemberTypes.Method, @public);
                    AddCandidates(candidateMethods, methods);
                    Type[] interfaces = list2[i].GetInterfaces();
                    if (interfaces.Length > 0)
                    {
                        list2.AddRange(interfaces);
                    }
                }
                methods = typeof(object).GetMember(methodName, MemberTypes.Method, @public);
                AddCandidates(candidateMethods, methods);
            }
            foreach (ExtensionMethodInfo info in validation.ExtensionMethods)
            {
                ValidationError error;
                if ((info.Name == methodName) && RuleValidation.TypesAreAssignable(computedType, info.AssumedDeclaringType, null, out error))
                {
                    candidateMethods.Add(info);
                }
            }
            this.completions = candidateMethods;
        }

        internal void SetNamespaceCompletions(NamespaceSymbol nsSym)
        {
            this.completions = nsSym.GetMembers();
        }

        internal void SetNestedClassCompletions(Type computedType, Type thisType)
        {
            BindingFlags bindingAttr = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance;
            if (computedType.Assembly == thisType.Assembly)
            {
                bindingAttr |= BindingFlags.NonPublic;
            }
            List<MemberInfo> list = new List<MemberInfo>(computedType.GetMembers(bindingAttr));
            Dictionary<string, MemberInfo> dictionary = new Dictionary<string, MemberInfo>();
            foreach (MemberInfo info in list)
            {
                if (info != null)
                {
                    MemberTypes memberType = info.MemberType;
                    if (((memberType == MemberTypes.TypeInfo) || (memberType == MemberTypes.NestedType)) && ((info.DeclaringType == thisType) || IsNonPrivate((Type) info, thisType)))
                    {
                        dictionary[info.Name] = info;
                    }
                }
            }
            this.completions = dictionary.Values;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal void SetTypeMemberCompletions(Type computedType, Type thisType, bool isStatic, RuleValidation validation)
        {
            BindingFlags @public = BindingFlags.Public;
            if (isStatic)
            {
                @public |= BindingFlags.FlattenHierarchy | BindingFlags.Static;
            }
            else
            {
                @public |= BindingFlags.Instance;
            }
            if (computedType.Assembly == thisType.Assembly)
            {
                @public |= BindingFlags.NonPublic;
            }
            List<MemberInfo> list = new List<MemberInfo>(computedType.GetMembers(@public));
            if (computedType.IsInterface)
            {
                List<Type> list2 = new List<Type>(computedType.GetInterfaces());
                for (int i = 0; i < list2.Count; i++)
                {
                    Type type = list2[i];
                    list2.AddRange(type.GetInterfaces());
                    list.AddRange(type.GetMembers(@public));
                }
                list.AddRange(typeof(object).GetMembers(@public));
            }
            foreach (ExtensionMethodInfo info in validation.ExtensionMethods)
            {
                ValidationError error;
                if (RuleValidation.TypesAreAssignable(computedType, info.AssumedDeclaringType, null, out error))
                {
                    list.Add(info);
                }
            }
            Dictionary<string, MemberInfo> dictionary = new Dictionary<string, MemberInfo>();
            foreach (MemberInfo info2 in list)
            {
                MethodInfo info3;
                PropertyInfo info4;
                if (info2 != null)
                {
                    switch (info2.MemberType)
                    {
                        case MemberTypes.Property:
                        {
                            info4 = (PropertyInfo) info2;
                            ParameterInfo[] indexParameters = info4.GetIndexParameters();
                            if ((indexParameters == null) || (indexParameters.Length <= 0))
                            {
                                goto Label_0292;
                            }
                            foreach (MethodInfo info5 in info4.GetAccessors((@public & BindingFlags.NonPublic) != BindingFlags.Default))
                            {
                                if ((info5.DeclaringType == thisType) || IsNonPrivate(info5, thisType))
                                {
                                    dictionary[info5.Name] = info5;
                                }
                            }
                            break;
                        }
                        case MemberTypes.TypeInfo:
                        case MemberTypes.NestedType:
                            if (isStatic && ((info2.DeclaringType == thisType) || IsNonPrivate((Type) info2, thisType)))
                            {
                                dictionary[info2.Name] = info2;
                            }
                            break;

                        case MemberTypes.Field:
                            goto Label_01E3;

                        case MemberTypes.Method:
                            goto Label_014C;
                    }
                }
                continue;
            Label_014C:
                info3 = (MethodInfo) info2;
                if ((!info3.IsSpecialName && !info3.IsGenericMethod) && (((info3.DeclaringType == thisType) || IsNonPrivate(info3, thisType)) || (info3 is ExtensionMethodInfo)))
                {
                    dictionary[info2.Name] = info2;
                }
                continue;
            Label_01E3:
                if ((info2.DeclaringType == thisType) || IsNonPrivate((FieldInfo) info2, thisType))
                {
                    dictionary[info2.Name] = info2;
                }
                continue;
            Label_0292:
                if (info2.DeclaringType == thisType)
                {
                    dictionary[info2.Name] = info2;
                }
                else
                {
                    foreach (MethodInfo info6 in info4.GetAccessors((@public & BindingFlags.NonPublic) != BindingFlags.Default))
                    {
                        if (IsNonPrivate(info6, thisType))
                        {
                            dictionary[info2.Name] = info2;
                            break;
                        }
                    }
                }
            }
            this.completions = dictionary.Values;
        }

        internal Token CurrentToken
        {
            get
            {
                if (this.currentToken >= this.tokens.Count)
                {
                    return null;
                }
                return this.tokens[this.currentToken];
            }
        }

        internal int NumTokens
        {
            get
            {
                return this.tokens.Count;
            }
        }
    }
}

