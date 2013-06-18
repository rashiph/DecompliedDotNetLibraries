namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel.Compiler;

    internal class Parser
    {
        private static CodeExpression defaultSize = new CodePrimitiveExpression(0);
        private Dictionary<string, Symbol> globalUniqueSymbols = new Dictionary<string, Symbol>();
        private Dictionary<string, Symbol> localUniqueSymbols = new Dictionary<string, Symbol>();
        private static readonly BinaryPrecedenceDescriptor[] precedences = new BinaryPrecedenceDescriptor[] { new BinaryPrecedenceDescriptor(new BinaryOperationDescriptor[] { new BinaryOperationDescriptor(TokenID.Or, CodeBinaryOperatorType.BooleanOr) }), new BinaryPrecedenceDescriptor(new BinaryOperationDescriptor[] { new BinaryOperationDescriptor(TokenID.And, CodeBinaryOperatorType.BooleanAnd) }), new BinaryPrecedenceDescriptor(new BinaryOperationDescriptor[] { new BinaryOperationDescriptor(TokenID.BitOr, CodeBinaryOperatorType.BitwiseOr) }), new BinaryPrecedenceDescriptor(new BinaryOperationDescriptor[] { new BinaryOperationDescriptor(TokenID.BitAnd, CodeBinaryOperatorType.BitwiseAnd) }), new BinaryPrecedenceDescriptor(new BinaryOperationDescriptor[] { new BinaryOperationDescriptor(TokenID.Equal, CodeBinaryOperatorType.ValueEquality), new BinaryOperationDescriptor(TokenID.Assign, CodeBinaryOperatorType.ValueEquality), new NotEqualOperationDescriptor(TokenID.NotEqual) }), new BinaryPrecedenceDescriptor(new BinaryOperationDescriptor[] { new BinaryOperationDescriptor(TokenID.Less, CodeBinaryOperatorType.LessThan), new BinaryOperationDescriptor(TokenID.LessEqual, CodeBinaryOperatorType.LessThanOrEqual), new BinaryOperationDescriptor(TokenID.Greater, CodeBinaryOperatorType.GreaterThan), new BinaryOperationDescriptor(TokenID.GreaterEqual, CodeBinaryOperatorType.GreaterThanOrEqual) }), new BinaryPrecedenceDescriptor(new BinaryOperationDescriptor[] { new BinaryOperationDescriptor(TokenID.Plus, CodeBinaryOperatorType.Add), new BinaryOperationDescriptor(TokenID.Minus, CodeBinaryOperatorType.Subtract) }), new BinaryPrecedenceDescriptor(new BinaryOperationDescriptor[] { new BinaryOperationDescriptor(TokenID.Multiply, CodeBinaryOperatorType.Multiply), new BinaryOperationDescriptor(TokenID.Divide, CodeBinaryOperatorType.Divide), new BinaryOperationDescriptor(TokenID.Modulus, CodeBinaryOperatorType.Modulus) }) };
        private RuleValidation validation;

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily"), SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal Parser(RuleValidation validation)
        {
            this.validation = validation;
            Type[] types = null;
            ITypeProvider typeProvider = validation.GetTypeProvider();
            if (typeProvider == null)
            {
                try
                {
                    types = validation.ThisType.Assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types;
                }
            }
            else
            {
                types = typeProvider.GetTypes();
            }
            Dictionary<string, NamespaceSymbol> dictionary = new Dictionary<string, NamespaceSymbol>();
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
            NamespaceSymbol symbol = null;
            Symbol symbol2 = null;
            NamespaceSymbol symbol3 = null;
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (((type != null) && ((!type.IsNotPublic || (type.Assembly == null)) || (type.Assembly == validation.ThisType.Assembly))) && !type.IsNested)
                {
                    string str = type.Namespace;
                    if (string.IsNullOrEmpty(str))
                    {
                        if (symbol3 == null)
                        {
                            symbol3 = new NamespaceSymbol();
                            dictionary.Add("", symbol3);
                        }
                        symbol = symbol3;
                    }
                    else
                    {
                        string[] strArray = str.Split(new char[] { '.' });
                        if (!dictionary.TryGetValue(strArray[0], out symbol))
                        {
                            symbol = new NamespaceSymbol(strArray[0], null);
                            dictionary.Add(strArray[0], symbol);
                            this.globalUniqueSymbols[strArray[0]] = symbol;
                        }
                        if (strArray.Length > 1)
                        {
                            for (int j = 1; j < strArray.Length; j++)
                            {
                                symbol = symbol.AddNamespace(strArray[j]);
                                if (this.globalUniqueSymbols.TryGetValue(strArray[j], out symbol2))
                                {
                                    NamespaceSymbol symbol4 = symbol2 as NamespaceSymbol;
                                    if ((symbol4 != null) && (symbol4.Parent != symbol.Parent))
                                    {
                                        if (symbol4.Level == symbol.Level)
                                        {
                                            dictionary2[strArray[j]] = null;
                                        }
                                        else if (symbol.Level < symbol4.Level)
                                        {
                                            this.globalUniqueSymbols[strArray[j]] = symbol;
                                        }
                                    }
                                }
                                else
                                {
                                    this.globalUniqueSymbols.Add(strArray[j], symbol);
                                }
                            }
                        }
                    }
                    symbol.AddType(type);
                }
            }
            foreach (string str2 in dictionary2.Keys)
            {
                this.globalUniqueSymbols.Remove(str2);
            }
            Queue<NamespaceSymbol> queue = new Queue<NamespaceSymbol>();
            foreach (NamespaceSymbol symbol5 in dictionary.Values)
            {
                queue.Enqueue(symbol5);
            }
            dictionary2.Clear();
            while (queue.Count > 0)
            {
                foreach (Symbol symbol6 in queue.Dequeue().NestedSymbols.Values)
                {
                    NamespaceSymbol item = symbol6 as NamespaceSymbol;
                    if (item != null)
                    {
                        queue.Enqueue(item);
                    }
                    else
                    {
                        string name = symbol6.Name;
                        if (this.globalUniqueSymbols.TryGetValue(name, out symbol2))
                        {
                            if (!(symbol2 is NamespaceSymbol))
                            {
                                TypeSymbolBase base2 = (TypeSymbolBase) symbol2;
                                TypeSymbolBase typeSymBase = (TypeSymbolBase) symbol6;
                                OverloadedTypeSymbol symbol8 = base2.OverloadType(typeSymBase);
                                if (symbol8 == null)
                                {
                                    dictionary2[name] = null;
                                }
                                else
                                {
                                    this.globalUniqueSymbols[name] = symbol8;
                                }
                            }
                        }
                        else
                        {
                            this.globalUniqueSymbols.Add(name, symbol6);
                        }
                    }
                }
            }
            foreach (string str4 in dictionary2.Keys)
            {
                this.globalUniqueSymbols.Remove(str4);
            }
            Type thisType = validation.ThisType;
            foreach (MemberInfo info in thisType.GetMembers(BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                PropertyInfo info2;
                Type type3;
                TypeSymbol symbol9;
                OverloadedTypeSymbol symbol10;
                switch (info.MemberType)
                {
                    case MemberTypes.Property:
                    {
                        info2 = (PropertyInfo) info;
                        ParameterInfo[] indexParameters = info2.GetIndexParameters();
                        if ((indexParameters == null) || (indexParameters.Length <= 0))
                        {
                            break;
                        }
                        foreach (MethodInfo info3 in info2.GetAccessors(true))
                        {
                            if ((info3.DeclaringType == thisType) || ParserContext.IsNonPrivate(info3, thisType))
                            {
                                this.localUniqueSymbols[info.Name] = new MemberSymbol(info3);
                            }
                        }
                        continue;
                    }
                    case MemberTypes.TypeInfo:
                    case MemberTypes.NestedType:
                    {
                        type3 = (Type) info;
                        symbol9 = new TypeSymbol(type3);
                        if (!this.globalUniqueSymbols.TryGetValue(symbol9.Name, out symbol2))
                        {
                            goto Label_06AA;
                        }
                        TypeSymbolBase base4 = symbol2 as TypeSymbolBase;
                        if (base4 == null)
                        {
                            goto Label_0674;
                        }
                        symbol10 = base4.OverloadType(symbol9);
                        if (symbol10 != null)
                        {
                            goto Label_0643;
                        }
                        if ((info.DeclaringType == thisType) || ParserContext.IsNonPrivate(type3, thisType))
                        {
                            this.globalUniqueSymbols[symbol9.Name] = symbol9;
                        }
                        continue;
                    }
                    case MemberTypes.Field:
                    {
                        if ((info.DeclaringType == thisType) || ParserContext.IsNonPrivate((FieldInfo) info, thisType))
                        {
                            this.localUniqueSymbols[info.Name] = new MemberSymbol(info);
                        }
                        continue;
                    }
                    case MemberTypes.Method:
                    {
                        MethodInfo methodInfo = (MethodInfo) info;
                        if ((!methodInfo.IsSpecialName && !methodInfo.IsGenericMethod) && ((info.DeclaringType == thisType) || ParserContext.IsNonPrivate(methodInfo, thisType)))
                        {
                            this.localUniqueSymbols[info.Name] = new MemberSymbol(info);
                        }
                        continue;
                    }
                    default:
                    {
                        continue;
                    }
                }
                if (info.DeclaringType == thisType)
                {
                    this.localUniqueSymbols[info.Name] = new MemberSymbol(info);
                }
                else
                {
                    foreach (MethodInfo info4 in info2.GetAccessors(true))
                    {
                        if (ParserContext.IsNonPrivate(info4, thisType))
                        {
                            this.localUniqueSymbols[info.Name] = new MemberSymbol(info);
                            break;
                        }
                    }
                }
                continue;
            Label_0643:
                if ((info.DeclaringType == thisType) || ParserContext.IsNonPrivate(type3, thisType))
                {
                    this.globalUniqueSymbols[symbol9.Name] = symbol10;
                }
                continue;
            Label_0674:
                if ((info.DeclaringType == thisType) || ParserContext.IsNonPrivate((Type) info, thisType))
                {
                    this.globalUniqueSymbols[symbol9.Name] = symbol9;
                }
                continue;
            Label_06AA:
                if ((info.DeclaringType == thisType) || ParserContext.IsNonPrivate(type3, thisType))
                {
                    this.globalUniqueSymbols[symbol9.Name] = symbol9;
                }
            }
        }

        private object ConstructCustomType(Type type, List<CodeExpression> arguments, int lparenPosition)
        {
            string str;
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            List<CandidateConstructor> candidateConstructors = this.GetCandidateConstructors(constructors, arguments);
            if ((candidateConstructors == null) || (candidateConstructors.Count == 0))
            {
                str = string.Format(CultureInfo.CurrentCulture, Messages.UnknownMethod, new object[] { type.Name, RuleDecompiler.DecompileType(type) });
                throw new RuleSyntaxException(0x137, str, lparenPosition);
            }
            CandidateConstructor constructor = FindBestConstructor(candidateConstructors);
            if (constructor == null)
            {
                str = string.Format(CultureInfo.CurrentCulture, Messages.AmbiguousConstructor, new object[] { type.Name });
                throw new RuleSyntaxException(0x54a, str, lparenPosition);
            }
            object obj2 = null;
            try
            {
                obj2 = constructor.InvokeConstructor();
            }
            catch (TargetInvocationException exception)
            {
                if (exception.InnerException == null)
                {
                    throw;
                }
                throw new RuleSyntaxException(0x137, exception.InnerException.Message, lparenPosition);
            }
            return obj2;
        }

        private static CandidateConstructor FindBestConstructor(List<CandidateConstructor> candidates)
        {
            int count = candidates.Count;
            List<CandidateConstructor> list = new List<CandidateConstructor>(1) {
                candidates[0]
            };
            for (int i = 1; i < count; i++)
            {
                CandidateConstructor item = candidates[i];
                CandidateConstructor other = list[0];
                int num3 = item.CompareConstructor(other);
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

        private List<CandidateConstructor> GetCandidateConstructors(ConstructorInfo[] allCtors, List<CodeExpression> arguments)
        {
            if ((allCtors == null) || (allCtors.Length == 0))
            {
                return null;
            }
            int count = arguments.Count;
            List<CandidateConstructor> list = new List<CandidateConstructor>(allCtors.Length);
            for (int i = 0; i < allCtors.Length; i++)
            {
                ConstructorInfo ctor = allCtors[i];
                ParameterInfo[] parameters = ctor.GetParameters();
                if (parameters.Length == 0)
                {
                    if (count == 0)
                    {
                        list.Add(new CandidateConstructor(ctor, new object[0], false));
                        return list;
                    }
                    continue;
                }
                int length = parameters.Length;
                int index = length;
                ParameterInfo info2 = parameters[length - 1];
                if (info2.ParameterType.IsArray)
                {
                    object[] customAttributes = info2.GetCustomAttributes(typeof(ParamArrayAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                    {
                        index--;
                    }
                }
                if ((count >= index) && ((index != length) || (count == length)))
                {
                    object[] ctorArgs = new object[length];
                    int num5 = 0;
                    while (num5 < index)
                    {
                        object obj2 = this.MatchArgument(parameters[num5].ParameterType, arguments[num5]);
                        if (obj2 == null)
                        {
                            break;
                        }
                        ctorArgs[num5] = obj2;
                        num5++;
                    }
                    if (num5 == index)
                    {
                        if (index == length)
                        {
                            list.Add(new CandidateConstructor(ctor, ctorArgs, false));
                            continue;
                        }
                        if (count == index)
                        {
                            list.Add(new CandidateConstructor(ctor, ctorArgs, true));
                            continue;
                        }
                        if ((count == (index + 1)) && (this.validation.ExpressionInfo(arguments[num5]).ExpressionType == typeof(NullLiteral)))
                        {
                            list.Add(new CandidateConstructor(ctor, ctorArgs, false));
                            continue;
                        }
                        Type parameterType = parameters[num5].ParameterType;
                        Type elementType = parameterType.GetElementType();
                        Array array = (Array) parameterType.InvokeMember(parameterType.Name, BindingFlags.CreateInstance, null, null, new object[] { count - index }, CultureInfo.CurrentCulture);
                        ctorArgs[index] = array;
                        while (num5 < count)
                        {
                            object obj3 = this.MatchArgument(elementType, arguments[num5]);
                            if (obj3 == null)
                            {
                                break;
                            }
                            array.SetValue(obj3, (int) (num5 - index));
                            num5++;
                        }
                        if (num5 == count)
                        {
                            list.Add(new CandidateConstructor(ctor, ctorArgs, index != length));
                        }
                    }
                }
            }
            return list;
        }

        internal ICollection GetExpressionCompletions(string expressionString)
        {
            try
            {
                ParserContext parserContext = new IntellisenseParser(expressionString).BackParse();
                if (parserContext != null)
                {
                    Token currentToken = parserContext.CurrentToken;
                    if ((parserContext.NumTokens == 2) && (currentToken.TokenID == TokenID.Identifier))
                    {
                        string str = (string) currentToken.Value;
                        if (str.Length == 1)
                        {
                            return this.GetRootCompletions(str[0]);
                        }
                    }
                    else
                    {
                        this.validation.Errors.Clear();
                        this.ParsePostfixExpression(parserContext, true, ValueCheck.Read);
                        return parserContext.completions;
                    }
                }
            }
            catch (RuleSyntaxException exception)
            {
                if (exception.ErrorNumber != 0)
                {
                    return null;
                }
            }
            return null;
        }

        private ICollection GetRootCompletions(char firstCharacter)
        {
            ArrayList list = new ArrayList();
            char upperFirstCharacter = char.ToUpper(firstCharacter, CultureInfo.InvariantCulture);
            foreach (KeyValuePair<string, Symbol> pair in this.globalUniqueSymbols)
            {
                string key = pair.Key;
                if (char.ToUpper(key[0], CultureInfo.InvariantCulture) == upperFirstCharacter)
                {
                    Symbol symbol = null;
                    if (!this.localUniqueSymbols.TryGetValue(key, out symbol))
                    {
                        pair.Value.RecordSymbol(list);
                    }
                }
            }
            foreach (KeyValuePair<string, Symbol> pair2 in this.localUniqueSymbols)
            {
                if (char.ToUpper(pair2.Key[0], CultureInfo.InvariantCulture) == upperFirstCharacter)
                {
                    pair2.Value.RecordSymbol(list);
                }
            }
            Scanner.AddKeywordsStartingWith(upperFirstCharacter, list);
            return list;
        }

        private object MatchArgument(Type parameterType, CodeExpression arg)
        {
            Type fromType = arg.GetType();
            if (TypeProvider.IsAssignable(parameterType, fromType))
            {
                return arg;
            }
            CodePrimitiveExpression rhsExpression = arg as CodePrimitiveExpression;
            if (rhsExpression != null)
            {
                ValidationError error = null;
                if (RuleValidation.TypesAreAssignable(this.validation.ExpressionInfo(rhsExpression).ExpressionType, parameterType, rhsExpression, out error))
                {
                    return rhsExpression.Value;
                }
            }
            return null;
        }

        private CodeExpression ParseArgument(ParserContext parserContext, bool assignIsEquality)
        {
            CodeExpression expression = null;
            Token currentToken = parserContext.CurrentToken;
            int startPosition = currentToken.StartPosition;
            FieldDirection? nullable = null;
            ValueCheck read = ValueCheck.Read;
            switch (currentToken.TokenID)
            {
                case TokenID.In:
                    nullable = new FieldDirection?(FieldDirection.In);
                    parserContext.NextToken();
                    break;

                case TokenID.Out:
                    nullable = new FieldDirection?(FieldDirection.Out);
                    parserContext.NextToken();
                    read = ValueCheck.Write;
                    break;

                case TokenID.Ref:
                    nullable = new FieldDirection?(FieldDirection.Ref);
                    parserContext.NextToken();
                    read = ValueCheck.Write | ValueCheck.Read;
                    break;
            }
            expression = this.ParseBinaryExpression(parserContext, 0, true, read);
            if (nullable.HasValue)
            {
                expression = new CodeDirectionExpression(nullable.Value, expression);
                parserContext.exprPositions[expression] = startPosition;
                this.ValidateExpression(parserContext, expression, assignIsEquality, ValueCheck.Read);
            }
            return expression;
        }

        private List<CodeExpression> ParseArgumentList(ParserContext parserContext)
        {
            List<CodeExpression> list = new List<CodeExpression>();
            if (parserContext.CurrentToken.TokenID != TokenID.RParen)
            {
                CodeExpression item = this.ParseArgument(parserContext, true);
                list.Add(item);
                while (parserContext.CurrentToken.TokenID == TokenID.Comma)
                {
                    parserContext.NextToken();
                    item = this.ParseArgument(parserContext, true);
                    list.Add(item);
                }
                if (parserContext.CurrentToken.TokenID != TokenID.RParen)
                {
                    throw new RuleSyntaxException(0x182, Messages.Parser_MissingRParenAfterArgumentList, parserContext.CurrentToken.StartPosition);
                }
            }
            parserContext.NextToken();
            return list;
        }

        private List<CodeExpression> ParseArrayCreationArguments(ParserContext parserContext)
        {
            if (parserContext.CurrentToken.TokenID != TokenID.LCurlyBrace)
            {
                return null;
            }
            List<CodeExpression> list = new List<CodeExpression>();
            parserContext.NextToken();
            if (parserContext.CurrentToken.TokenID != TokenID.RCurlyBrace)
            {
                list.Add(this.ParseInitializer(parserContext, true));
                while (parserContext.CurrentToken.TokenID == TokenID.Comma)
                {
                    parserContext.NextToken();
                    list.Add(this.ParseInitializer(parserContext, true));
                }
                if (parserContext.CurrentToken.TokenID != TokenID.RCurlyBrace)
                {
                    throw new RuleSyntaxException(0x1ab, Messages.Parser_MissingRCurlyAfterInitializers, parserContext.CurrentToken.StartPosition);
                }
            }
            parserContext.NextToken();
            return list;
        }

        private static Type ParseArrayType(ParserContext parserContext, Type baseType)
        {
            Type type = baseType;
            while (parserContext.CurrentToken.TokenID == TokenID.LBracket)
            {
                int rank = 1;
                while (parserContext.NextToken().TokenID == TokenID.Comma)
                {
                    rank++;
                }
                if (parserContext.CurrentToken.TokenID != TokenID.RBracket)
                {
                    throw new RuleSyntaxException(410, Messages.Parser_MissingCloseSquareBracket, parserContext.CurrentToken.StartPosition);
                }
                parserContext.NextToken();
                if (rank == 1)
                {
                    type = type.MakeArrayType();
                }
                else
                {
                    type = type.MakeArrayType(rank);
                }
            }
            return type;
        }

        private CodeStatement ParseAssignmentStatement(ParserContext parserContext)
        {
            CodeStatement statement = null;
            CodeExpression left = this.ParsePostfixExpression(parserContext, false, ValueCheck.Read);
            Token currentToken = parserContext.CurrentToken;
            if (currentToken.TokenID == TokenID.Assign)
            {
                int startPosition = currentToken.StartPosition;
                parserContext.NextToken();
                CodeExpression right = this.ParseBinaryExpression(parserContext, 0, true, ValueCheck.Read);
                statement = new CodeAssignStatement(left, right);
                parserContext.exprPositions[statement] = startPosition;
            }
            else
            {
                statement = new CodeExpressionStatement(left);
                parserContext.exprPositions[statement] = parserContext.exprPositions[left];
            }
            this.ValidateStatement(parserContext, statement);
            return statement;
        }

        private CodeExpression ParseBinaryExpression(ParserContext parserContext, int precedence, bool assignIsEquality, ValueCheck check)
        {
            CodeExpression left = (precedence == (precedences.Length - 1)) ? this.ParseUnaryExpression(parserContext, assignIsEquality, check) : this.ParseBinaryExpression(parserContext, precedence + 1, assignIsEquality, check);
            if (left != null)
            {
                while (true)
                {
                    Token currentToken = parserContext.CurrentToken;
                    BinaryOperationDescriptor descriptor2 = precedences[precedence].FindOperation(currentToken.TokenID);
                    if (descriptor2 == null)
                    {
                        return left;
                    }
                    parserContext.NextToken();
                    CodeExpression right = (precedence == (precedences.Length - 1)) ? this.ParseUnaryExpression(parserContext, true, check) : this.ParseBinaryExpression(parserContext, precedence + 1, true, check);
                    left = descriptor2.CreateBinaryExpression(left, right, currentToken.StartPosition, this, parserContext, assignIsEquality);
                }
            }
            return left;
        }

        internal RuleExpressionCondition ParseCondition(string expressionString)
        {
            this.validation.Errors.Clear();
            ParserContext parserContext = new ParserContext(expressionString);
            if (parserContext.CurrentToken.TokenID == TokenID.EndOfInput)
            {
                throw new RuleSyntaxException(400, Messages.Parser_EmptyExpression, parserContext.CurrentToken.StartPosition);
            }
            CodeExpression expression = this.ParseBinaryExpression(parserContext, 0, true, ValueCheck.Read);
            if (parserContext.CurrentToken.TokenID != TokenID.EndOfInput)
            {
                throw new RuleSyntaxException(0x191, Messages.Parser_ExtraCharactersIgnored, parserContext.CurrentToken.StartPosition);
            }
            if (expression == null)
            {
                return null;
            }
            RuleExpressionInfo info = this.validation.ExpressionInfo(expression);
            if (info == null)
            {
                return null;
            }
            if (!RuleValidation.IsValidBooleanResult(info.ExpressionType))
            {
                throw new RuleSyntaxException(0x547, Messages.ConditionMustBeBoolean, 0);
            }
            return new RuleExpressionCondition(expression);
        }

        private CodeExpression ParseConstructorArguments(ParserContext parserContext, Type type, bool assignIsEquality)
        {
            int startPosition = parserContext.CurrentToken.StartPosition;
            parserContext.NextToken();
            if ((parserContext.CurrentToken.TokenID == TokenID.EndOfInput) && parserContext.provideIntellisense)
            {
                parserContext.SetConstructorCompletions(type, this.Validator.ThisType);
                return null;
            }
            List<CodeExpression> argumentExprs = this.ParseArgumentList(parserContext);
            if (!type.IsValueType || (argumentExprs.Count != 0))
            {
                if (type.IsAbstract)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.UnknownConstructor, new object[] { RuleDecompiler.DecompileType(type) });
                    throw new RuleSyntaxException(0x137, message, startPosition);
                }
                BindingFlags constructorBindingFlags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance;
                if (type.Assembly == this.validation.ThisType.Assembly)
                {
                    constructorBindingFlags |= BindingFlags.NonPublic;
                }
                ValidationError error = null;
                if (this.validation.ResolveConstructor(type, constructorBindingFlags, argumentExprs, out error) == null)
                {
                    throw new RuleSyntaxException(error.ErrorNumber, error.ErrorText, startPosition);
                }
            }
            CodeExpression expression = new CodeObjectCreateExpression(type, argumentExprs.ToArray());
            parserContext.exprPositions[expression] = startPosition;
            this.ValidateExpression(parserContext, expression, assignIsEquality, ValueCheck.Read);
            return expression;
        }

        private CodeExpression ParseElementOperator(ParserContext parserContext, CodeExpression primaryExpr, bool assignIsEquality)
        {
            int startPosition = parserContext.CurrentToken.StartPosition;
            parserContext.NextToken();
            CodeExpression[] indices = this.ParseIndexList(parserContext).ToArray();
            CodeExpression expression = null;
            if (this.validation.ExpressionInfo(primaryExpr).ExpressionType.IsArray)
            {
                expression = new CodeArrayIndexerExpression(primaryExpr, indices);
            }
            else
            {
                expression = new CodeIndexerExpression(primaryExpr, indices);
            }
            parserContext.exprPositions[expression] = startPosition;
            this.ValidateExpression(parserContext, expression, assignIsEquality, ValueCheck.Read);
            return expression;
        }

        private CodeExpression ParseFieldOrProperty(ParserContext parserContext, CodeExpression postfixExpr, string name, int namePosition, bool assignIsEquality, ValueCheck check)
        {
            CodeExpression expression = null;
            Type expressionType = this.Validator.ExpressionInfo(postfixExpr).ExpressionType;
            MemberInfo info = this.Validator.ResolveFieldOrProperty(expressionType, name);
            if (info == null)
            {
                Type type = this.Validator.ExpressionInfo(postfixExpr).ExpressionType;
                string message = string.Format(CultureInfo.CurrentCulture, Messages.UnknownFieldOrProperty, new object[] { name, RuleDecompiler.DecompileType(type) });
                throw new RuleSyntaxException(390, message, namePosition);
            }
            if (info.MemberType == MemberTypes.Field)
            {
                expression = new CodeFieldReferenceExpression(postfixExpr, name);
            }
            else
            {
                expression = new CodePropertyReferenceExpression(postfixExpr, name);
            }
            parserContext.exprPositions[expression] = namePosition;
            this.ValidateExpression(parserContext, expression, assignIsEquality, check);
            return expression;
        }

        private Type ParseGenericType(ParserContext parserContext, List<Type> candidateGenericTypes, string typeName)
        {
            Type[] typeArguments = this.ParseGenericTypeArgList(parserContext);
            foreach (Type type in candidateGenericTypes)
            {
                if (type.GetGenericArguments().Length == typeArguments.Length)
                {
                    return type.MakeGenericType(typeArguments);
                }
            }
            string message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_BadTypeArgCount, new object[] { typeName });
            throw new RuleSyntaxException(0x18d, message, parserContext.CurrentToken.StartPosition);
        }

        private Type[] ParseGenericTypeArgList(ParserContext parserContext)
        {
            List<Type> list = new List<Type>();
            do
            {
                Token token = parserContext.NextToken();
                Type item = this.TryParseTypeSpecifier(parserContext, true);
                if (item == null)
                {
                    throw new RuleSyntaxException(0x18e, Messages.Parser_InvalidTypeArgument, token.StartPosition);
                }
                list.Add(item);
            }
            while (parserContext.CurrentToken.TokenID == TokenID.Comma);
            if (parserContext.CurrentToken.TokenID != TokenID.Greater)
            {
                throw new RuleSyntaxException(0x18f, Messages.Parser_MissingCloseAngleBracket, parserContext.CurrentToken.StartPosition);
            }
            parserContext.NextToken();
            return list.ToArray();
        }

        private List<CodeExpression> ParseIndexList(ParserContext parserContext)
        {
            List<CodeExpression> list = new List<CodeExpression>();
            CodeExpression item = this.ParseBinaryExpression(parserContext, 0, true, ValueCheck.Read);
            list.Add(item);
            while (parserContext.CurrentToken.TokenID == TokenID.Comma)
            {
                parserContext.NextToken();
                item = this.ParseBinaryExpression(parserContext, 0, true, ValueCheck.Read);
                list.Add(item);
            }
            if (parserContext.CurrentToken.TokenID != TokenID.RBracket)
            {
                throw new RuleSyntaxException(410, Messages.Parser_MissingCloseSquareBracket, parserContext.CurrentToken.StartPosition);
            }
            parserContext.NextToken();
            return list;
        }

        private CodeExpression ParseInitializer(ParserContext parserContext, bool assignIsEquality)
        {
            return this.ParseBinaryExpression(parserContext, 0, assignIsEquality, ValueCheck.Read);
        }

        private CodeExpression ParseMemberOperator(ParserContext parserContext, CodeExpression primaryExpr, bool assignIsEquality, ValueCheck check)
        {
            Token token = parserContext.NextToken();
            if (token.TokenID != TokenID.Identifier)
            {
                if (!parserContext.provideIntellisense || (token.TokenID != TokenID.EndOfInput))
                {
                    throw new RuleSyntaxException(0x185, Messages.Parser_MissingIdentifierAfterDot, parserContext.CurrentToken.StartPosition);
                }
                parserContext.SetTypeMemberCompletions(this.validation.ExpressionInfo(primaryExpr).ExpressionType, this.validation.ThisType, primaryExpr is CodeTypeReferenceExpression, this.validation);
                return null;
            }
            string methodName = (string) token.Value;
            int startPosition = token.StartPosition;
            if (parserContext.NextToken().TokenID == TokenID.LParen)
            {
                return this.ParseMethodInvoke(parserContext, primaryExpr, methodName, true);
            }
            return this.ParseFieldOrProperty(parserContext, primaryExpr, methodName, startPosition, assignIsEquality, check);
        }

        private CodeExpression ParseMethodInvoke(ParserContext parserContext, CodeExpression postfixExpr, string methodName, bool assignIsEquality)
        {
            int startPosition = parserContext.CurrentToken.StartPosition;
            parserContext.NextToken();
            if ((parserContext.CurrentToken.TokenID == TokenID.EndOfInput) && parserContext.provideIntellisense)
            {
                bool includeStatic = postfixExpr is CodeTypeReferenceExpression;
                parserContext.SetMethodCompletions(this.validation.ExpressionInfo(postfixExpr).ExpressionType, this.validation.ThisType, methodName, includeStatic, !includeStatic, this.validation);
                return null;
            }
            List<CodeExpression> list = this.ParseArgumentList(parserContext);
            postfixExpr = new CodeMethodInvokeExpression(postfixExpr, methodName, list.ToArray());
            parserContext.exprPositions[postfixExpr] = startPosition;
            this.ValidateExpression(parserContext, postfixExpr, assignIsEquality, ValueCheck.Read);
            return postfixExpr;
        }

        private Type ParseNestedType(ParserContext parserContext, Type currentType)
        {
            Type type = null;
            while (parserContext.CurrentToken.TokenID == TokenID.Dot)
            {
                int tokenValue = parserContext.SaveCurrentToken();
                Token token = parserContext.NextToken();
                if (token.TokenID != TokenID.Identifier)
                {
                    if (!parserContext.provideIntellisense || (token.TokenID != TokenID.EndOfInput))
                    {
                        throw new RuleSyntaxException(0x185, Messages.Parser_MissingIdentifierAfterDot, parserContext.CurrentToken.StartPosition);
                    }
                    parserContext.SetTypeMemberCompletions(currentType, this.validation.ThisType, true, this.validation);
                    return null;
                }
                string typeName = (string) token.Value;
                BindingFlags @public = BindingFlags.Public;
                if (currentType.Assembly == this.validation.ThisType.Assembly)
                {
                    @public |= BindingFlags.NonPublic;
                }
                if (parserContext.NextToken().TokenID == TokenID.Less)
                {
                    List<Type> candidateGenericTypes = new List<Type>();
                    Type[] nestedTypes = currentType.GetNestedTypes(@public);
                    string str2 = typeName + "`";
                    for (int i = 0; i < nestedTypes.Length; i++)
                    {
                        Type item = nestedTypes[i];
                        if (item.Name.StartsWith(str2, StringComparison.Ordinal))
                        {
                            candidateGenericTypes.Add(item);
                        }
                    }
                    if (candidateGenericTypes.Count == 0)
                    {
                        parserContext.RestoreCurrentToken(tokenValue);
                        return currentType;
                    }
                    type = this.ParseGenericType(parserContext, candidateGenericTypes, typeName);
                    currentType = type;
                }
                else
                {
                    MemberInfo[] member = currentType.GetMember(typeName, @public);
                    if (((member == null) || (member.Length != 1)) || ((member[0].MemberType != MemberTypes.NestedType) && (member[0].MemberType != MemberTypes.TypeInfo)))
                    {
                        parserContext.RestoreCurrentToken(tokenValue);
                        return currentType;
                    }
                    type = (Type) member[0];
                    if (currentType.IsGenericType && type.IsGenericTypeDefinition)
                    {
                        type = type.MakeGenericType(currentType.GetGenericArguments());
                    }
                    currentType = type;
                }
            }
            return type;
        }

        private CodeExpression ParseObjectCreation(ParserContext parserContext, bool assignIsEquality)
        {
            CodeExpression expression = null;
            CodeExpression expression2;
            Token currentToken = parserContext.CurrentToken;
            Type computedType = this.TryParseTypeSpecifierWithOptionalSize(parserContext, assignIsEquality, out expression2);
            if (parserContext.provideIntellisense && (parserContext.CurrentToken.TokenID == TokenID.EndOfInput))
            {
                if (computedType != null)
                {
                    parserContext.SetNestedClassCompletions(computedType, this.validation.ThisType);
                }
                return null;
            }
            if (computedType == null)
            {
                throw new RuleSyntaxException(0x18e, Messages.Parser_InvalidTypeArgument, currentToken.StartPosition);
            }
            if (expression2 == null)
            {
                if (parserContext.CurrentToken.TokenID != TokenID.LParen)
                {
                    throw new RuleSyntaxException(0x18e, Messages.Parser_InvalidNew, currentToken.StartPosition);
                }
                return this.ParseConstructorArguments(parserContext, computedType, assignIsEquality);
            }
            List<CodeExpression> list = this.ParseArrayCreationArguments(parserContext);
            if (list != null)
            {
                if (expression2 == defaultSize)
                {
                    expression = new CodeArrayCreateExpression(computedType, list.ToArray());
                }
                else
                {
                    expression = new CodeArrayCreateExpression(computedType, expression2);
                    ((CodeArrayCreateExpression) expression).Initializers.AddRange(list.ToArray());
                }
            }
            else
            {
                if (expression2 == defaultSize)
                {
                    throw new RuleSyntaxException(0x1aa, Messages.Parser_NoArrayCreationSize, parserContext.CurrentToken.StartPosition);
                }
                expression = new CodeArrayCreateExpression(computedType, expression2);
            }
            this.ValidateExpression(parserContext, expression, assignIsEquality, ValueCheck.Read);
            return expression;
        }

        private CodeExpression ParsePostfixExpression(ParserContext parserContext, bool assignIsEquality, ValueCheck check)
        {
            CodeExpression primaryExpr = this.ParsePrimaryExpression(parserContext, assignIsEquality);
            for (CodeExpression expression2 = this.TryParsePostfixOperator(parserContext, primaryExpr, assignIsEquality, check); expression2 != null; expression2 = this.TryParsePostfixOperator(parserContext, primaryExpr, assignIsEquality, check))
            {
                primaryExpr = expression2;
            }
            return primaryExpr;
        }

        private CodeExpression ParsePrimaryExpression(ParserContext parserContext, bool assignIsEquality)
        {
            CodeExpression expression = null;
            Token currentToken = parserContext.CurrentToken;
            switch (currentToken.TokenID)
            {
                case TokenID.Identifier:
                    return this.ParseRootIdentifier(parserContext, assignIsEquality);

                case TokenID.LParen:
                    parserContext.NextToken();
                    expression = this.ParseBinaryExpression(parserContext, 0, assignIsEquality, ValueCheck.Read);
                    parserContext.exprPositions[expression] = currentToken.StartPosition;
                    if (parserContext.CurrentToken.TokenID != TokenID.RParen)
                    {
                        throw new RuleSyntaxException(0x184, Messages.Parser_MissingRParenInSubexpression, parserContext.CurrentToken.StartPosition);
                    }
                    parserContext.NextToken();
                    return expression;

                case TokenID.StringLiteral:
                case TokenID.CharacterLiteral:
                case TokenID.IntegerLiteral:
                case TokenID.DecimalLiteral:
                case TokenID.FloatLiteral:
                case TokenID.True:
                case TokenID.False:
                case TokenID.Null:
                    parserContext.NextToken();
                    expression = new CodePrimitiveExpression(currentToken.Value);
                    parserContext.exprPositions[expression] = currentToken.StartPosition;
                    this.ValidateExpression(parserContext, expression, assignIsEquality, ValueCheck.Read);
                    return expression;

                case TokenID.This:
                    parserContext.NextToken();
                    expression = new CodeThisReferenceExpression();
                    parserContext.exprPositions[expression] = currentToken.StartPosition;
                    this.ValidateExpression(parserContext, expression, assignIsEquality, ValueCheck.Read);
                    return expression;

                case TokenID.TypeName:
                {
                    parserContext.NextToken();
                    Type type = (Type) currentToken.Value;
                    CodeTypeReference typeRef = new CodeTypeReference(type);
                    this.validation.AddTypeReference(typeRef, type);
                    expression = new CodeTypeReferenceExpression(typeRef);
                    parserContext.exprPositions[expression] = currentToken.StartPosition;
                    this.ValidateExpression(parserContext, expression, assignIsEquality, ValueCheck.Read);
                    return expression;
                }
                case TokenID.New:
                    parserContext.NextToken();
                    return this.ParseObjectCreation(parserContext, assignIsEquality);

                case TokenID.EndOfInput:
                    throw new RuleSyntaxException(0x183, Messages.Parser_MissingOperand, currentToken.StartPosition);
            }
            throw new RuleSyntaxException(0x187, Messages.Parser_UnknownLiteral, currentToken.StartPosition);
        }

        private CodeExpression ParseRootIdentifier(ParserContext parserContext, bool assignIsEquality)
        {
            Token currentToken = parserContext.CurrentToken;
            string key = (string) currentToken.Value;
            Symbol symbol = null;
            if (!this.localUniqueSymbols.TryGetValue(key, out symbol))
            {
                this.globalUniqueSymbols.TryGetValue(key, out symbol);
            }
            if (symbol == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_UnknownIdentifier, new object[] { key });
                throw new RuleSyntaxException(0x188, message, currentToken.StartPosition);
            }
            return symbol.ParseRootIdentifier(this, parserContext, assignIsEquality);
        }

        internal CodeExpression ParseRootNamespaceIdentifier(ParserContext parserContext, NamespaceSymbol nsSym, bool assignIsEquality)
        {
            Symbol symbol = null;
            while (nsSym != null)
            {
                Token token = parserContext.NextToken();
                if (token.TokenID != TokenID.Dot)
                {
                    throw new RuleSyntaxException(0x189, Messages.Parser_MissingDotAfterNamespace, token.StartPosition);
                }
                token = parserContext.NextToken();
                if (token.TokenID != TokenID.Identifier)
                {
                    if (!parserContext.provideIntellisense || (token.TokenID != TokenID.EndOfInput))
                    {
                        throw new RuleSyntaxException(0x185, Messages.Parser_MissingIdentifierAfterDot, token.StartPosition);
                    }
                    parserContext.SetNamespaceCompletions(nsSym);
                    return null;
                }
                string memberName = (string) token.Value;
                symbol = nsSym.FindMember(memberName);
                if (symbol == null)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_UnknownNamespaceMember, new object[] { memberName, nsSym.GetQualifiedName() });
                    throw new RuleSyntaxException(0x18a, message, token.StartPosition);
                }
                nsSym = symbol as NamespaceSymbol;
            }
            return symbol.ParseRootIdentifier(this, parserContext, assignIsEquality);
        }

        internal CodeExpression ParseRootOverloadedTypeIdentifier(ParserContext parserContext, List<TypeSymbol> candidateTypeSymbols, bool assignIsEquality)
        {
            Token currentToken = parserContext.CurrentToken;
            string typeName = (string) currentToken.Value;
            int startPosition = currentToken.StartPosition;
            currentToken = parserContext.NextToken();
            Type currentType = null;
            if (currentToken.TokenID == TokenID.Less)
            {
                List<Type> candidateGenericTypes = new List<Type>(candidateTypeSymbols.Count);
                foreach (TypeSymbol symbol in candidateTypeSymbols)
                {
                    if (symbol.GenericArgCount > 0)
                    {
                        candidateGenericTypes.Add(symbol.Type);
                    }
                }
                currentType = this.ParseGenericType(parserContext, candidateGenericTypes, typeName);
            }
            else
            {
                TypeSymbol symbol2 = candidateTypeSymbols.Find(s => s.GenericArgCount == 0);
                if (symbol2 == null)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_MissingTypeArguments, new object[] { typeName });
                    throw new RuleSyntaxException(0x18b, message, startPosition);
                }
                currentType = symbol2.Type;
            }
            if (parserContext.CurrentToken.TokenID == TokenID.Dot)
            {
                Type type2 = this.ParseNestedType(parserContext, currentType);
                if (type2 != null)
                {
                    currentType = type2;
                }
            }
            return this.ParseTypeRef(parserContext, currentType, startPosition, assignIsEquality);
        }

        internal CodeExpression ParseRootTypeIdentifier(ParserContext parserContext, TypeSymbol typeSym, bool assignIsEquality)
        {
            string message = null;
            int startPosition = parserContext.CurrentToken.StartPosition;
            Token token = parserContext.NextToken();
            if ((typeSym.GenericArgCount > 0) && (token.TokenID != TokenID.Less))
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_MissingTypeArguments, new object[] { typeSym.Name });
                throw new RuleSyntaxException(0x18b, message, token.StartPosition);
            }
            Type type = typeSym.Type;
            if (token.TokenID == TokenID.Less)
            {
                if (typeSym.GenericArgCount == 0)
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_NotAGenericType, new object[] { RuleDecompiler.DecompileType(type) });
                    throw new RuleSyntaxException(0x18c, message, token.StartPosition);
                }
                Type[] typeArguments = this.ParseGenericTypeArgList(parserContext);
                if (typeArguments.Length != typeSym.GenericArgCount)
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_BadTypeArgCount, new object[] { RuleDecompiler.DecompileType(type) });
                    throw new RuleSyntaxException(0x18d, message, parserContext.CurrentToken.StartPosition);
                }
                type = this.Validator.ResolveType(type.AssemblyQualifiedName).MakeGenericType(typeArguments);
            }
            if (parserContext.CurrentToken.TokenID == TokenID.Dot)
            {
                Type type2 = this.ParseNestedType(parserContext, type);
                if (type2 != null)
                {
                    type = type2;
                }
            }
            return this.ParseTypeRef(parserContext, type, startPosition, assignIsEquality);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal RuleAction ParseSingleStatement(string statementString)
        {
            this.validation.Errors.Clear();
            ParserContext parserContext = new ParserContext(statementString);
            RuleAction action = this.ParseStatement(parserContext);
            if (parserContext.CurrentToken.TokenID != TokenID.EndOfInput)
            {
                throw new RuleSyntaxException(0x191, Messages.Parser_ExtraCharactersIgnored, parserContext.CurrentToken.StartPosition);
            }
            return action;
        }

        private RuleAction ParseStatement(ParserContext parserContext)
        {
            RuleAction action = null;
            Token currentToken = parserContext.CurrentToken;
            if (currentToken.TokenID == TokenID.Halt)
            {
                parserContext.NextToken();
                action = new RuleHaltAction();
                parserContext.exprPositions[action] = currentToken.StartPosition;
                this.ValidateAction(parserContext, action);
                return action;
            }
            if (currentToken.TokenID == TokenID.Update)
            {
                parserContext.NextToken();
                if (parserContext.CurrentToken.TokenID != TokenID.LParen)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_MissingLparenAfterCommand, new object[] { "UPDATE" });
                    throw new RuleSyntaxException(0x180, message, parserContext.CurrentToken.StartPosition);
                }
                parserContext.NextToken();
                string path = null;
                Token token2 = parserContext.CurrentToken;
                if (token2.TokenID == TokenID.StringLiteral)
                {
                    path = (string) token2.Value;
                    parserContext.NextToken();
                }
                else
                {
                    CodeExpression expression = this.ParsePostfixExpression(parserContext, true, ValueCheck.Read);
                    RuleAnalysis analysis = new RuleAnalysis(this.validation, true);
                    RuleExpressionWalker.AnalyzeUsage(analysis, expression, false, true, null);
                    ICollection<string> symbols = analysis.GetSymbols();
                    if ((symbols.Count == 0) || (symbols.Count > 1))
                    {
                        throw new RuleSyntaxException(0x181, Messages.Parser_InvalidUpdateExpression, token2.StartPosition);
                    }
                    IEnumerator<string> enumerator = symbols.GetEnumerator();
                    enumerator.MoveNext();
                    path = enumerator.Current;
                }
                if (parserContext.CurrentToken.TokenID != TokenID.RParen)
                {
                    throw new RuleSyntaxException(0x182, Messages.Parser_MissingRParenAfterArgumentList, parserContext.CurrentToken.StartPosition);
                }
                parserContext.NextToken();
                action = new RuleUpdateAction(path);
                parserContext.exprPositions[action] = currentToken.StartPosition;
                this.ValidateAction(parserContext, action);
                return action;
            }
            int tokenValue = parserContext.SaveCurrentToken();
            Type fromType = this.TryParseTypeSpecifier(parserContext, false);
            if (((fromType != null) && (parserContext.CurrentToken.TokenID == TokenID.LParen)) && TypeProvider.IsAssignable(typeof(RuleAction), fromType))
            {
                int startPosition = parserContext.CurrentToken.StartPosition;
                parserContext.NextToken();
                List<CodeExpression> arguments = this.ParseArgumentList(parserContext);
                action = (RuleAction) this.ConstructCustomType(fromType, arguments, startPosition);
                parserContext.exprPositions[action] = currentToken.StartPosition;
                this.ValidateAction(parserContext, action);
                return action;
            }
            parserContext.RestoreCurrentToken(tokenValue);
            CodeStatement codeDomStatement = this.ParseAssignmentStatement(parserContext);
            if (codeDomStatement != null)
            {
                action = new RuleStatementAction(codeDomStatement);
            }
            return action;
        }

        internal List<RuleAction> ParseStatementList(string statementString)
        {
            this.validation.Errors.Clear();
            ParserContext parserContext = new ParserContext(statementString);
            return this.ParseStatements(parserContext);
        }

        private List<RuleAction> ParseStatements(ParserContext parserContext)
        {
            List<RuleAction> list = new List<RuleAction>();
            while (parserContext.CurrentToken.TokenID != TokenID.EndOfInput)
            {
                RuleAction item = this.ParseStatement(parserContext);
                if (item == null)
                {
                    return list;
                }
                list.Add(item);
                while (parserContext.CurrentToken.TokenID == TokenID.Semicolon)
                {
                    parserContext.NextToken();
                }
            }
            return list;
        }

        private CodeExpression ParseTypeRef(ParserContext parserContext, Type type, int typePosition, bool assignIsEquality)
        {
            CodeExpression expression = null;
            if ((parserContext.CurrentToken.TokenID == TokenID.LParen) && TypeProvider.IsAssignable(typeof(IRuleExpression), type))
            {
                int startPosition = parserContext.CurrentToken.StartPosition;
                parserContext.NextToken();
                List<CodeExpression> arguments = this.ParseArgumentList(parserContext);
                expression = (CodeExpression) this.ConstructCustomType(type, arguments, startPosition);
                parserContext.exprPositions[expression] = startPosition;
                this.ValidateExpression(parserContext, expression, assignIsEquality, ValueCheck.Read);
                return expression;
            }
            CodeTypeReference typeRef = new CodeTypeReference(type);
            this.validation.AddTypeReference(typeRef, type);
            expression = new CodeTypeReferenceExpression(typeRef);
            parserContext.exprPositions[expression] = typePosition;
            this.ValidateExpression(parserContext, expression, assignIsEquality, ValueCheck.Read);
            return expression;
        }

        private CodeExpression ParseUnadornedFieldOrProperty(ParserContext parserContext, string name, int namePosition, bool assignIsEquality)
        {
            Type thisType = this.Validator.ThisType;
            MemberInfo info = this.Validator.ResolveFieldOrProperty(thisType, name);
            if (info == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.UnknownFieldOrProperty, new object[] { name, RuleDecompiler.DecompileType(thisType) });
                throw new RuleSyntaxException(390, message, namePosition);
            }
            bool isStatic = false;
            FieldInfo info2 = info as FieldInfo;
            if (info2 != null)
            {
                isStatic = info2.IsStatic;
            }
            else
            {
                PropertyInfo info3 = info as PropertyInfo;
                if (info3 != null)
                {
                    MethodInfo[] accessors = info3.GetAccessors(true);
                    for (int i = 0; i < accessors.Length; i++)
                    {
                        if (accessors[i].IsStatic)
                        {
                            isStatic = true;
                            break;
                        }
                    }
                }
            }
            CodeExpression targetObject = null;
            if (isStatic)
            {
                targetObject = new CodeTypeReferenceExpression(thisType);
            }
            else
            {
                targetObject = new CodeThisReferenceExpression();
            }
            CodeExpression expression = null;
            if (info2 != null)
            {
                expression = new CodeFieldReferenceExpression(targetObject, name);
            }
            else
            {
                expression = new CodePropertyReferenceExpression(targetObject, name);
            }
            parserContext.exprPositions[expression] = namePosition;
            this.ValidateExpression(parserContext, expression, assignIsEquality, ValueCheck.Read);
            return expression;
        }

        internal CodeExpression ParseUnadornedMemberIdentifier(ParserContext parserContext, MemberSymbol symbol, bool assignIsEquality)
        {
            int startPosition = parserContext.CurrentToken.StartPosition;
            parserContext.NextToken();
            if (parserContext.CurrentToken.TokenID == TokenID.LParen)
            {
                return this.ParseUnadornedMethodInvoke(parserContext, symbol.Name, true);
            }
            return this.ParseUnadornedFieldOrProperty(parserContext, symbol.Name, startPosition, assignIsEquality);
        }

        private CodeExpression ParseUnadornedMethodInvoke(ParserContext parserContext, string methodName, bool assignIsEquality)
        {
            Type thisType = this.Validator.ThisType;
            int startPosition = parserContext.CurrentToken.StartPosition;
            parserContext.NextToken();
            if ((parserContext.CurrentToken.TokenID == TokenID.EndOfInput) && parserContext.provideIntellisense)
            {
                parserContext.SetMethodCompletions(thisType, thisType, methodName, true, true, this.validation);
                return null;
            }
            List<CodeExpression> argumentExprs = this.ParseArgumentList(parserContext);
            BindingFlags methodBindingFlags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
            ValidationError error = null;
            RuleMethodInvokeExpressionInfo info = this.validation.ResolveMethod(thisType, methodName, methodBindingFlags, argumentExprs, out error);
            if (info == null)
            {
                throw new RuleSyntaxException(error.ErrorNumber, error.ErrorText, startPosition);
            }
            MethodInfo methodInfo = info.MethodInfo;
            CodeExpression targetObject = null;
            if (methodInfo.IsStatic)
            {
                targetObject = new CodeTypeReferenceExpression(thisType);
            }
            else
            {
                targetObject = new CodeThisReferenceExpression();
            }
            CodeExpression expression = new CodeMethodInvokeExpression(targetObject, methodName, argumentExprs.ToArray());
            parserContext.exprPositions[expression] = startPosition;
            this.ValidateExpression(parserContext, expression, assignIsEquality, ValueCheck.Read);
            return expression;
        }

        private CodeExpression ParseUnaryExpression(ParserContext parserContext, bool assignIsEquality, ValueCheck check)
        {
            Token currentToken = parserContext.CurrentToken;
            CodeExpression expression = null;
            if (currentToken.TokenID == TokenID.Not)
            {
                int startPosition = currentToken.StartPosition;
                parserContext.NextToken();
                expression = new CodeBinaryOperatorExpression(this.ParseUnaryExpression(parserContext, true, check), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(false));
                parserContext.exprPositions[expression] = startPosition;
                this.ValidateExpression(parserContext, expression, assignIsEquality, check);
                return expression;
            }
            if (currentToken.TokenID == TokenID.Minus)
            {
                int num2 = currentToken.StartPosition;
                parserContext.NextToken();
                expression = this.ParseUnaryExpression(parserContext, true, check);
                expression = new CodeBinaryOperatorExpression(new CodePrimitiveExpression(0), CodeBinaryOperatorType.Subtract, expression);
                parserContext.exprPositions[expression] = num2;
                this.ValidateExpression(parserContext, expression, assignIsEquality, check);
                return expression;
            }
            if (currentToken.TokenID == TokenID.LParen)
            {
                int num3 = currentToken.StartPosition;
                int tokenValue = parserContext.SaveCurrentToken();
                currentToken = parserContext.NextToken();
                Type type = this.TryParseTypeSpecifier(parserContext, assignIsEquality);
                if ((type == null) || (parserContext.CurrentToken.TokenID != TokenID.RParen))
                {
                    parserContext.RestoreCurrentToken(tokenValue);
                    return this.ParsePostfixExpression(parserContext, assignIsEquality, check);
                }
                if (parserContext.CurrentToken.TokenID != TokenID.RParen)
                {
                    throw new RuleSyntaxException(0x184, Messages.Parser_MissingRParenInSubexpression, parserContext.CurrentToken.StartPosition);
                }
                parserContext.NextToken();
                expression = this.ParseUnaryExpression(parserContext, true, check);
                CodeTypeReference typeRef = new CodeTypeReference(type);
                this.validation.AddTypeReference(typeRef, type);
                expression = new CodeCastExpression(typeRef, expression);
                parserContext.exprPositions[expression] = num3;
                this.ValidateExpression(parserContext, expression, assignIsEquality, check);
                return expression;
            }
            return this.ParsePostfixExpression(parserContext, assignIsEquality, check);
        }

        private CodeExpression TryParsePostfixOperator(ParserContext parserContext, CodeExpression primaryExpr, bool assignIsEquality, ValueCheck check)
        {
            CodeExpression expression = null;
            if (parserContext.CurrentToken.TokenID == TokenID.Dot)
            {
                return this.ParseMemberOperator(parserContext, primaryExpr, assignIsEquality, check);
            }
            if (parserContext.CurrentToken.TokenID == TokenID.LBracket)
            {
                expression = this.ParseElementOperator(parserContext, primaryExpr, assignIsEquality);
            }
            return expression;
        }

        private Type TryParseTypeName(ParserContext parserContext, bool assignIsEquality)
        {
            Type expressionType = null;
            Token currentToken = parserContext.CurrentToken;
            if (currentToken.TokenID == TokenID.TypeName)
            {
                expressionType = (Type) currentToken.Value;
                parserContext.NextToken();
                return expressionType;
            }
            if (currentToken.TokenID == TokenID.Identifier)
            {
                Symbol symbol = null;
                if (this.globalUniqueSymbols.TryGetValue((string) currentToken.Value, out symbol))
                {
                    CodeExpression expression = symbol.ParseRootIdentifier(this, parserContext, assignIsEquality);
                    if (expression is CodeTypeReferenceExpression)
                    {
                        expressionType = this.validation.ExpressionInfo(expression).ExpressionType;
                    }
                }
            }
            return expressionType;
        }

        private Type TryParseTypeSpecifier(ParserContext parserContext, bool assignIsEquality)
        {
            Type baseType = this.TryParseTypeName(parserContext, assignIsEquality);
            if (baseType != null)
            {
                baseType = ParseArrayType(parserContext, baseType);
            }
            return baseType;
        }

        private Type TryParseTypeSpecifierWithOptionalSize(ParserContext parserContext, bool assignIsEquality, out CodeExpression size)
        {
            Type type = null;
            size = null;
            Token currentToken = parserContext.CurrentToken;
            type = this.TryParseTypeName(parserContext, assignIsEquality);
            if ((type != null) && (parserContext.CurrentToken.TokenID == TokenID.LBracket))
            {
                if (parserContext.NextToken().TokenID != TokenID.RBracket)
                {
                    size = this.ParseBinaryExpression(parserContext, 0, false, ValueCheck.Read);
                }
                else
                {
                    size = defaultSize;
                }
                if (parserContext.CurrentToken.TokenID != TokenID.RBracket)
                {
                    throw new RuleSyntaxException(410, Messages.Parser_MissingCloseSquareBracket1, parserContext.CurrentToken.StartPosition);
                }
                parserContext.NextToken();
            }
            return type;
        }

        private void ValidateAction(ParserContext parserContext, RuleAction action)
        {
            if (!action.Validate(this.validation))
            {
                ValidationError error = this.Validator.Errors[0];
                object key = error.UserData["ErrorObject"];
                int num = 0;
                parserContext.exprPositions.TryGetValue(key, out num);
                throw new RuleSyntaxException(error.ErrorNumber, error.ErrorText, num);
            }
        }

        private void ValidateExpression(ParserContext parserContext, CodeExpression expression, bool assignIsEquality, ValueCheck check)
        {
            if ((parserContext.CurrentToken.TokenID == TokenID.Assign) && !assignIsEquality)
            {
                check = ValueCheck.Write;
            }
            RuleExpressionInfo info = null;
            if ((check & ValueCheck.Read) != ValueCheck.Unknown)
            {
                info = RuleExpressionWalker.Validate(this.Validator, expression, false);
                if ((info != null) && ((check & ValueCheck.Write) != ValueCheck.Unknown))
                {
                    info = RuleExpressionWalker.Validate(this.Validator, expression, true);
                }
            }
            else if ((check & ValueCheck.Write) != ValueCheck.Unknown)
            {
                info = RuleExpressionWalker.Validate(this.Validator, expression, true);
            }
            if ((info == null) && (this.Validator.Errors.Count > 0))
            {
                ValidationError error = this.Validator.Errors[0];
                object key = error.UserData["ErrorObject"];
                int num = 0;
                parserContext.exprPositions.TryGetValue(key, out num);
                throw new RuleSyntaxException(error.ErrorNumber, error.ErrorText, num);
            }
        }

        private void ValidateStatement(ParserContext parserContext, CodeStatement statement)
        {
            if (!CodeDomStatementWalker.Validate(this.Validator, statement) && (this.Validator.Errors.Count > 0))
            {
                ValidationError error = this.Validator.Errors[0];
                object key = error.UserData["ErrorObject"];
                int num = 0;
                parserContext.exprPositions.TryGetValue(key, out num);
                throw new RuleSyntaxException(error.ErrorNumber, error.ErrorText, num);
            }
        }

        private RuleValidation Validator
        {
            get
            {
                return this.validation;
            }
        }

        private class BinaryOperationDescriptor
        {
            private CodeBinaryOperatorType codeDomOperator;
            private TokenID token;

            internal BinaryOperationDescriptor(TokenID token, CodeBinaryOperatorType codeDomOperator)
            {
                this.token = token;
                this.codeDomOperator = codeDomOperator;
            }

            internal virtual CodeBinaryOperatorExpression CreateBinaryExpression(CodeExpression left, CodeExpression right, int operatorPosition, Parser parser, ParserContext parserContext, bool assignIsEquality)
            {
                CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression(left, this.codeDomOperator, right);
                parserContext.exprPositions[expression] = operatorPosition;
                parser.ValidateExpression(parserContext, expression, assignIsEquality, Parser.ValueCheck.Read);
                return expression;
            }

            internal TokenID Token
            {
                get
                {
                    return this.token;
                }
            }
        }

        private class BinaryPrecedenceDescriptor
        {
            private Parser.BinaryOperationDescriptor[] operations;

            internal BinaryPrecedenceDescriptor(params Parser.BinaryOperationDescriptor[] operations)
            {
                this.operations = operations;
            }

            internal Parser.BinaryOperationDescriptor FindOperation(TokenID token)
            {
                foreach (Parser.BinaryOperationDescriptor descriptor in this.operations)
                {
                    if (descriptor.Token == token)
                    {
                        return descriptor;
                    }
                }
                return null;
            }
        }

        private class CandidateConstructor
        {
            private ConstructorInfo ctor;
            private object[] ctorArgs;
            private bool isExpandedMatch;

            internal CandidateConstructor(ConstructorInfo ctor, object[] ctorArgs, bool isExpandedMatch)
            {
                this.ctor = ctor;
                this.ctorArgs = ctorArgs;
                this.isExpandedMatch = isExpandedMatch;
            }

            internal int CompareConstructor(Parser.CandidateConstructor other)
            {
                int num = 1;
                int num2 = -1;
                if (!this.isExpandedMatch && other.isExpandedMatch)
                {
                    return num;
                }
                if (this.isExpandedMatch && !other.isExpandedMatch)
                {
                    return num2;
                }
                if (this.isExpandedMatch && other.isExpandedMatch)
                {
                    int length = this.ctor.GetParameters().Length;
                    int num5 = other.ctor.GetParameters().Length;
                    if (length > num5)
                    {
                        return num;
                    }
                    if (num5 > length)
                    {
                        return num2;
                    }
                }
                return 0;
            }

            internal object InvokeConstructor()
            {
                return this.ctor.Invoke(this.ctorArgs);
            }
        }

        private class NotEqualOperationDescriptor : Parser.BinaryOperationDescriptor
        {
            internal NotEqualOperationDescriptor(TokenID token) : base(token, CodeBinaryOperatorType.IdentityInequality)
            {
            }

            internal override CodeBinaryOperatorExpression CreateBinaryExpression(CodeExpression left, CodeExpression right, int operatorPosition, Parser parser, ParserContext parserContext, bool assignIsEquality)
            {
                CodePrimitiveExpression expression = new CodePrimitiveExpression(false);
                parserContext.exprPositions[expression] = operatorPosition;
                CodeBinaryOperatorExpression expression2 = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.ValueEquality, right);
                parserContext.exprPositions[expression2] = operatorPosition;
                expression2 = new CodeBinaryOperatorExpression(expression2, CodeBinaryOperatorType.ValueEquality, expression);
                parserContext.exprPositions[expression2] = operatorPosition;
                parser.ValidateExpression(parserContext, expression2, assignIsEquality, Parser.ValueCheck.Read);
                return expression2;
            }
        }

        [Flags]
        private enum ValueCheck
        {
            Unknown,
            Read,
            Write
        }
    }
}

