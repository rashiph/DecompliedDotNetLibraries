namespace System.CodeDom.Compiler
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.IO;

    internal class CodeValidator
    {
        private CodeTypeDeclaration currentClass;
        private static readonly char[] newLineChars = new char[] { '\r', '\n', '\u2028', '\u2029', '\x0085' };

        private static void ValidateArgumentReferenceExpression(CodeArgumentReferenceExpression e)
        {
            ValidateIdentifier(e, "ParameterName", e.ParameterName);
        }

        private static void ValidateArity(CodeTypeReference e)
        {
            string baseType = e.BaseType;
            int num = 0;
            for (int i = 0; i < baseType.Length; i++)
            {
                if (baseType[i] == '`')
                {
                    i++;
                    int num3 = 0;
                    while (((i < baseType.Length) && (baseType[i] >= '0')) && (baseType[i] <= '9'))
                    {
                        num3 = (num3 * 10) + (baseType[i] - '0');
                        i++;
                    }
                    num += num3;
                }
            }
            if ((num != e.TypeArguments.Count) && (e.TypeArguments.Count != 0))
            {
                throw new ArgumentException(SR.GetString("ArityDoesntMatch", new object[] { baseType, e.TypeArguments.Count }));
            }
        }

        private void ValidateArrayCreateExpression(CodeArrayCreateExpression e)
        {
            ValidateTypeReference(e.CreateType);
            CodeExpressionCollection initializers = e.Initializers;
            if (initializers.Count > 0)
            {
                this.ValidateExpressionList(initializers);
            }
            else if (e.SizeExpression != null)
            {
                this.ValidateExpression(e.SizeExpression);
            }
        }

        private void ValidateArrayIndexerExpression(CodeArrayIndexerExpression e)
        {
            this.ValidateExpression(e.TargetObject);
            foreach (CodeExpression expression in e.Indices)
            {
                this.ValidateExpression(expression);
            }
        }

        private void ValidateAssignStatement(CodeAssignStatement e)
        {
            this.ValidateExpression(e.Left);
            this.ValidateExpression(e.Right);
        }

        private void ValidateAttachEventStatement(CodeAttachEventStatement e)
        {
            this.ValidateEventReferenceExpression(e.Event);
            this.ValidateExpression(e.Listener);
        }

        private void ValidateAttributeArgument(CodeAttributeArgument arg)
        {
            if ((arg.Name != null) && (arg.Name.Length > 0))
            {
                ValidateIdentifier(arg, "Name", arg.Name);
            }
            this.ValidateExpression(arg.Value);
        }

        private void ValidateAttributes(CodeAttributeDeclarationCollection attributes)
        {
            if (attributes.Count != 0)
            {
                IEnumerator enumerator = attributes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    CodeAttributeDeclaration current = (CodeAttributeDeclaration) enumerator.Current;
                    ValidateTypeName(current, "Name", current.Name);
                    ValidateTypeReference(current.AttributeType);
                    foreach (CodeAttributeArgument argument in current.Arguments)
                    {
                        this.ValidateAttributeArgument(argument);
                    }
                }
            }
        }

        private void ValidateBaseReferenceExpression(CodeBaseReferenceExpression e)
        {
        }

        private void ValidateBinaryOperatorExpression(CodeBinaryOperatorExpression e)
        {
            this.ValidateExpression(e.Left);
            this.ValidateExpression(e.Right);
        }

        private void ValidateCastExpression(CodeCastExpression e)
        {
            ValidateTypeReference(e.TargetType);
            this.ValidateExpression(e.Expression);
        }

        private static void ValidateChecksumPragma(CodeChecksumPragma e)
        {
            if (e.FileName.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                throw new ArgumentException(SR.GetString("InvalidPathCharsInChecksum", new object[] { e.FileName }));
            }
        }

        private void ValidateCodeCompileUnit(CodeCompileUnit e)
        {
            ValidateCodeDirectives(e.StartDirectives);
            ValidateCodeDirectives(e.EndDirectives);
            if (e is CodeSnippetCompileUnit)
            {
                this.ValidateSnippetCompileUnit((CodeSnippetCompileUnit) e);
            }
            else
            {
                this.ValidateCompileUnitStart(e);
                this.ValidateNamespaces(e);
                this.ValidateCompileUnitEnd(e);
            }
        }

        private static void ValidateCodeDirective(CodeDirective e)
        {
            if (e is CodeChecksumPragma)
            {
                ValidateChecksumPragma((CodeChecksumPragma) e);
            }
            else
            {
                if (!(e is CodeRegionDirective))
                {
                    throw new ArgumentException(SR.GetString("InvalidElementType", new object[] { e.GetType().FullName }), "e");
                }
                ValidateRegionDirective((CodeRegionDirective) e);
            }
        }

        private static void ValidateCodeDirectives(CodeDirectiveCollection e)
        {
            for (int i = 0; i < e.Count; i++)
            {
                ValidateCodeDirective(e[i]);
            }
        }

        private void ValidateComment(CodeComment e)
        {
        }

        private void ValidateCommentStatement(CodeCommentStatement e)
        {
            this.ValidateComment(e.Comment);
        }

        private void ValidateCommentStatements(CodeCommentStatementCollection e)
        {
            foreach (CodeCommentStatement statement in e)
            {
                this.ValidateCommentStatement(statement);
            }
        }

        private void ValidateCompileUnitEnd(CodeCompileUnit e)
        {
        }

        private void ValidateCompileUnitStart(CodeCompileUnit e)
        {
            if (e.AssemblyCustomAttributes.Count > 0)
            {
                this.ValidateAttributes(e.AssemblyCustomAttributes);
            }
        }

        private void ValidateConditionStatement(CodeConditionStatement e)
        {
            this.ValidateExpression(e.Condition);
            this.ValidateStatements(e.TrueStatements);
            if (e.FalseStatements.Count > 0)
            {
                this.ValidateStatements(e.FalseStatements);
            }
        }

        private void ValidateConstructor(CodeConstructor e)
        {
            if (e.CustomAttributes.Count > 0)
            {
                this.ValidateAttributes(e.CustomAttributes);
            }
            this.ValidateParameters(e.Parameters);
            CodeExpressionCollection baseConstructorArgs = e.BaseConstructorArgs;
            CodeExpressionCollection chainedConstructorArgs = e.ChainedConstructorArgs;
            if (baseConstructorArgs.Count > 0)
            {
                this.ValidateExpressionList(baseConstructorArgs);
            }
            if (chainedConstructorArgs.Count > 0)
            {
                this.ValidateExpressionList(chainedConstructorArgs);
            }
            this.ValidateStatements(e.Statements);
        }

        private static void ValidateDefaultValueExpression(CodeDefaultValueExpression e)
        {
            ValidateTypeReference(e.Type);
        }

        private void ValidateDelegateCreateExpression(CodeDelegateCreateExpression e)
        {
            ValidateTypeReference(e.DelegateType);
            this.ValidateExpression(e.TargetObject);
            ValidateIdentifier(e, "MethodName", e.MethodName);
        }

        private void ValidateDelegateInvokeExpression(CodeDelegateInvokeExpression e)
        {
            if (e.TargetObject != null)
            {
                this.ValidateExpression(e.TargetObject);
            }
            this.ValidateExpressionList(e.Parameters);
        }

        private void ValidateDirectionExpression(CodeDirectionExpression e)
        {
            this.ValidateExpression(e.Expression);
        }

        private void ValidateEvent(CodeMemberEvent e)
        {
            if (e.CustomAttributes.Count > 0)
            {
                this.ValidateAttributes(e.CustomAttributes);
            }
            if (e.PrivateImplementationType != null)
            {
                ValidateTypeReference(e.Type);
                ValidateIdentifier(e, "Name", e.Name);
            }
            ValidateTypeReferences(e.ImplementationTypes);
        }

        private void ValidateEventReferenceExpression(CodeEventReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                this.ValidateExpression(e.TargetObject);
            }
            ValidateIdentifier(e, "EventName", e.EventName);
        }

        private void ValidateExpression(CodeExpression e)
        {
            if (e is CodeArrayCreateExpression)
            {
                this.ValidateArrayCreateExpression((CodeArrayCreateExpression) e);
            }
            else if (e is CodeBaseReferenceExpression)
            {
                this.ValidateBaseReferenceExpression((CodeBaseReferenceExpression) e);
            }
            else if (e is CodeBinaryOperatorExpression)
            {
                this.ValidateBinaryOperatorExpression((CodeBinaryOperatorExpression) e);
            }
            else if (e is CodeCastExpression)
            {
                this.ValidateCastExpression((CodeCastExpression) e);
            }
            else if (e is CodeDefaultValueExpression)
            {
                ValidateDefaultValueExpression((CodeDefaultValueExpression) e);
            }
            else if (e is CodeDelegateCreateExpression)
            {
                this.ValidateDelegateCreateExpression((CodeDelegateCreateExpression) e);
            }
            else if (e is CodeFieldReferenceExpression)
            {
                this.ValidateFieldReferenceExpression((CodeFieldReferenceExpression) e);
            }
            else if (e is CodeArgumentReferenceExpression)
            {
                ValidateArgumentReferenceExpression((CodeArgumentReferenceExpression) e);
            }
            else if (e is CodeVariableReferenceExpression)
            {
                ValidateVariableReferenceExpression((CodeVariableReferenceExpression) e);
            }
            else if (e is CodeIndexerExpression)
            {
                this.ValidateIndexerExpression((CodeIndexerExpression) e);
            }
            else if (e is CodeArrayIndexerExpression)
            {
                this.ValidateArrayIndexerExpression((CodeArrayIndexerExpression) e);
            }
            else if (e is CodeSnippetExpression)
            {
                this.ValidateSnippetExpression((CodeSnippetExpression) e);
            }
            else if (e is CodeMethodInvokeExpression)
            {
                this.ValidateMethodInvokeExpression((CodeMethodInvokeExpression) e);
            }
            else if (e is CodeMethodReferenceExpression)
            {
                this.ValidateMethodReferenceExpression((CodeMethodReferenceExpression) e);
            }
            else if (e is CodeEventReferenceExpression)
            {
                this.ValidateEventReferenceExpression((CodeEventReferenceExpression) e);
            }
            else if (e is CodeDelegateInvokeExpression)
            {
                this.ValidateDelegateInvokeExpression((CodeDelegateInvokeExpression) e);
            }
            else if (e is CodeObjectCreateExpression)
            {
                this.ValidateObjectCreateExpression((CodeObjectCreateExpression) e);
            }
            else if (e is CodeParameterDeclarationExpression)
            {
                this.ValidateParameterDeclarationExpression((CodeParameterDeclarationExpression) e);
            }
            else if (e is CodeDirectionExpression)
            {
                this.ValidateDirectionExpression((CodeDirectionExpression) e);
            }
            else if (e is CodePrimitiveExpression)
            {
                this.ValidatePrimitiveExpression((CodePrimitiveExpression) e);
            }
            else if (e is CodePropertyReferenceExpression)
            {
                this.ValidatePropertyReferenceExpression((CodePropertyReferenceExpression) e);
            }
            else if (e is CodePropertySetValueReferenceExpression)
            {
                this.ValidatePropertySetValueReferenceExpression((CodePropertySetValueReferenceExpression) e);
            }
            else if (e is CodeThisReferenceExpression)
            {
                this.ValidateThisReferenceExpression((CodeThisReferenceExpression) e);
            }
            else if (e is CodeTypeReferenceExpression)
            {
                ValidateTypeReference(((CodeTypeReferenceExpression) e).Type);
            }
            else if (e is CodeTypeOfExpression)
            {
                ValidateTypeOfExpression((CodeTypeOfExpression) e);
            }
            else
            {
                if (e == null)
                {
                    throw new ArgumentNullException("e");
                }
                throw new ArgumentException(SR.GetString("InvalidElementType", new object[] { e.GetType().FullName }), "e");
            }
        }

        private void ValidateExpressionList(CodeExpressionCollection expressions)
        {
            IEnumerator enumerator = expressions.GetEnumerator();
            while (enumerator.MoveNext())
            {
                this.ValidateExpression((CodeExpression) enumerator.Current);
            }
        }

        private void ValidateExpressionStatement(CodeExpressionStatement e)
        {
            this.ValidateExpression(e.Expression);
        }

        private void ValidateField(CodeMemberField e)
        {
            if (e.CustomAttributes.Count > 0)
            {
                this.ValidateAttributes(e.CustomAttributes);
            }
            ValidateIdentifier(e, "Name", e.Name);
            if (!this.IsCurrentEnum)
            {
                ValidateTypeReference(e.Type);
            }
            if (e.InitExpression != null)
            {
                this.ValidateExpression(e.InitExpression);
            }
        }

        private void ValidateFieldReferenceExpression(CodeFieldReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                this.ValidateExpression(e.TargetObject);
            }
            ValidateIdentifier(e, "FieldName", e.FieldName);
        }

        private static void ValidateGotoStatement(CodeGotoStatement e)
        {
            ValidateIdentifier(e, "Label", e.Label);
        }

        private static void ValidateIdentifier(object e, string propertyName, string identifier)
        {
            if (!CodeGenerator.IsValidLanguageIndependentIdentifier(identifier))
            {
                throw new ArgumentException(SR.GetString("InvalidLanguageIdentifier", new object[] { identifier, propertyName, e.GetType().FullName }), "identifier");
            }
        }

        internal void ValidateIdentifiers(CodeObject e)
        {
            if (e is CodeCompileUnit)
            {
                this.ValidateCodeCompileUnit((CodeCompileUnit) e);
            }
            else if (e is CodeComment)
            {
                this.ValidateComment((CodeComment) e);
            }
            else if (e is CodeExpression)
            {
                this.ValidateExpression((CodeExpression) e);
            }
            else if (e is CodeNamespace)
            {
                this.ValidateNamespace((CodeNamespace) e);
            }
            else if (e is CodeNamespaceImport)
            {
                ValidateNamespaceImport((CodeNamespaceImport) e);
            }
            else if (e is CodeStatement)
            {
                this.ValidateStatement((CodeStatement) e);
            }
            else if (e is CodeTypeMember)
            {
                this.ValidateTypeMember((CodeTypeMember) e);
            }
            else if (e is CodeTypeReference)
            {
                ValidateTypeReference((CodeTypeReference) e);
            }
            else
            {
                if (!(e is CodeDirective))
                {
                    throw new ArgumentException(SR.GetString("InvalidElementType", new object[] { e.GetType().FullName }), "e");
                }
                ValidateCodeDirective((CodeDirective) e);
            }
        }

        private void ValidateIndexerExpression(CodeIndexerExpression e)
        {
            this.ValidateExpression(e.TargetObject);
            foreach (CodeExpression expression in e.Indices)
            {
                this.ValidateExpression(expression);
            }
        }

        private void ValidateIterationStatement(CodeIterationStatement e)
        {
            this.ValidateStatement(e.InitStatement);
            this.ValidateExpression(e.TestExpression);
            this.ValidateStatement(e.IncrementStatement);
            this.ValidateStatements(e.Statements);
        }

        private void ValidateLabeledStatement(CodeLabeledStatement e)
        {
            ValidateIdentifier(e, "Label", e.Label);
            if (e.Statement != null)
            {
                this.ValidateStatement(e.Statement);
            }
        }

        private void ValidateLinePragmaStart(CodeLinePragma e)
        {
        }

        private void ValidateMemberMethod(CodeMemberMethod e)
        {
            this.ValidateCommentStatements(e.Comments);
            if (e.LinePragma != null)
            {
                this.ValidateLinePragmaStart(e.LinePragma);
            }
            this.ValidateTypeParameters(e.TypeParameters);
            ValidateTypeReferences(e.ImplementationTypes);
            if (e is CodeEntryPointMethod)
            {
                this.ValidateStatements(((CodeEntryPointMethod) e).Statements);
            }
            else if (e is CodeConstructor)
            {
                this.ValidateConstructor((CodeConstructor) e);
            }
            else if (e is CodeTypeConstructor)
            {
                this.ValidateTypeConstructor((CodeTypeConstructor) e);
            }
            else
            {
                this.ValidateMethod(e);
            }
        }

        private void ValidateMethod(CodeMemberMethod e)
        {
            if (e.CustomAttributes.Count > 0)
            {
                this.ValidateAttributes(e.CustomAttributes);
            }
            if (e.ReturnTypeCustomAttributes.Count > 0)
            {
                this.ValidateAttributes(e.ReturnTypeCustomAttributes);
            }
            ValidateTypeReference(e.ReturnType);
            if (e.PrivateImplementationType != null)
            {
                ValidateTypeReference(e.PrivateImplementationType);
            }
            ValidateIdentifier(e, "Name", e.Name);
            this.ValidateParameters(e.Parameters);
            if (!this.IsCurrentInterface && ((e.Attributes & MemberAttributes.ScopeMask) != MemberAttributes.Abstract))
            {
                this.ValidateStatements(e.Statements);
            }
        }

        private void ValidateMethodInvokeExpression(CodeMethodInvokeExpression e)
        {
            this.ValidateMethodReferenceExpression(e.Method);
            this.ValidateExpressionList(e.Parameters);
        }

        private void ValidateMethodReferenceExpression(CodeMethodReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                this.ValidateExpression(e.TargetObject);
            }
            ValidateIdentifier(e, "MethodName", e.MethodName);
            ValidateTypeReferences(e.TypeArguments);
        }

        private void ValidateMethodReturnStatement(CodeMethodReturnStatement e)
        {
            if (e.Expression != null)
            {
                this.ValidateExpression(e.Expression);
            }
        }

        private void ValidateNamespace(CodeNamespace e)
        {
            this.ValidateCommentStatements(e.Comments);
            ValidateNamespaceStart(e);
            this.ValidateNamespaceImports(e);
            this.ValidateTypes(e);
        }

        private static void ValidateNamespaceImport(CodeNamespaceImport e)
        {
            ValidateTypeName(e, "Namespace", e.Namespace);
        }

        private void ValidateNamespaceImports(CodeNamespace e)
        {
            IEnumerator enumerator = e.Imports.GetEnumerator();
            while (enumerator.MoveNext())
            {
                CodeNamespaceImport current = (CodeNamespaceImport) enumerator.Current;
                if (current.LinePragma != null)
                {
                    this.ValidateLinePragmaStart(current.LinePragma);
                }
                ValidateNamespaceImport(current);
            }
        }

        private void ValidateNamespaces(CodeCompileUnit e)
        {
            foreach (CodeNamespace namespace2 in e.Namespaces)
            {
                this.ValidateNamespace(namespace2);
            }
        }

        private static void ValidateNamespaceStart(CodeNamespace e)
        {
            if ((e.Name != null) && (e.Name.Length > 0))
            {
                ValidateTypeName(e, "Name", e.Name);
            }
        }

        private void ValidateObjectCreateExpression(CodeObjectCreateExpression e)
        {
            ValidateTypeReference(e.CreateType);
            this.ValidateExpressionList(e.Parameters);
        }

        private void ValidateParameterDeclarationExpression(CodeParameterDeclarationExpression e)
        {
            if (e.CustomAttributes.Count > 0)
            {
                this.ValidateAttributes(e.CustomAttributes);
            }
            ValidateTypeReference(e.Type);
            ValidateIdentifier(e, "Name", e.Name);
        }

        private void ValidateParameters(CodeParameterDeclarationExpressionCollection parameters)
        {
            IEnumerator enumerator = parameters.GetEnumerator();
            while (enumerator.MoveNext())
            {
                CodeParameterDeclarationExpression current = (CodeParameterDeclarationExpression) enumerator.Current;
                this.ValidateParameterDeclarationExpression(current);
            }
        }

        private void ValidatePrimitiveExpression(CodePrimitiveExpression e)
        {
        }

        private void ValidateProperty(CodeMemberProperty e)
        {
            if (e.CustomAttributes.Count > 0)
            {
                this.ValidateAttributes(e.CustomAttributes);
            }
            ValidateTypeReference(e.Type);
            ValidateTypeReferences(e.ImplementationTypes);
            if ((e.PrivateImplementationType != null) && !this.IsCurrentInterface)
            {
                ValidateTypeReference(e.PrivateImplementationType);
            }
            if ((e.Parameters.Count > 0) && (string.Compare(e.Name, "Item", StringComparison.OrdinalIgnoreCase) == 0))
            {
                this.ValidateParameters(e.Parameters);
            }
            else
            {
                ValidateIdentifier(e, "Name", e.Name);
            }
            if ((e.HasGet && !this.IsCurrentInterface) && ((e.Attributes & MemberAttributes.ScopeMask) != MemberAttributes.Abstract))
            {
                this.ValidateStatements(e.GetStatements);
            }
            if ((e.HasSet && !this.IsCurrentInterface) && ((e.Attributes & MemberAttributes.ScopeMask) != MemberAttributes.Abstract))
            {
                this.ValidateStatements(e.SetStatements);
            }
        }

        private void ValidatePropertyReferenceExpression(CodePropertyReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                this.ValidateExpression(e.TargetObject);
            }
            ValidateIdentifier(e, "PropertyName", e.PropertyName);
        }

        private void ValidatePropertySetValueReferenceExpression(CodePropertySetValueReferenceExpression e)
        {
        }

        private static void ValidateRegionDirective(CodeRegionDirective e)
        {
            if (e.RegionText.IndexOfAny(newLineChars) != -1)
            {
                throw new ArgumentException(SR.GetString("InvalidRegion", new object[] { e.RegionText }));
            }
        }

        private void ValidateRemoveEventStatement(CodeRemoveEventStatement e)
        {
            this.ValidateEventReferenceExpression(e.Event);
            this.ValidateExpression(e.Listener);
        }

        private void ValidateSnippetCompileUnit(CodeSnippetCompileUnit e)
        {
            if (e.LinePragma != null)
            {
                this.ValidateLinePragmaStart(e.LinePragma);
            }
        }

        private void ValidateSnippetExpression(CodeSnippetExpression e)
        {
        }

        private void ValidateSnippetMember(CodeSnippetTypeMember e)
        {
        }

        private void ValidateSnippetStatement(CodeSnippetStatement e)
        {
        }

        private void ValidateStatement(CodeStatement e)
        {
            ValidateCodeDirectives(e.StartDirectives);
            ValidateCodeDirectives(e.EndDirectives);
            if (e is CodeCommentStatement)
            {
                this.ValidateCommentStatement((CodeCommentStatement) e);
            }
            else if (e is CodeMethodReturnStatement)
            {
                this.ValidateMethodReturnStatement((CodeMethodReturnStatement) e);
            }
            else if (e is CodeConditionStatement)
            {
                this.ValidateConditionStatement((CodeConditionStatement) e);
            }
            else if (e is CodeTryCatchFinallyStatement)
            {
                this.ValidateTryCatchFinallyStatement((CodeTryCatchFinallyStatement) e);
            }
            else if (e is CodeAssignStatement)
            {
                this.ValidateAssignStatement((CodeAssignStatement) e);
            }
            else if (e is CodeExpressionStatement)
            {
                this.ValidateExpressionStatement((CodeExpressionStatement) e);
            }
            else if (e is CodeIterationStatement)
            {
                this.ValidateIterationStatement((CodeIterationStatement) e);
            }
            else if (e is CodeThrowExceptionStatement)
            {
                this.ValidateThrowExceptionStatement((CodeThrowExceptionStatement) e);
            }
            else if (e is CodeSnippetStatement)
            {
                this.ValidateSnippetStatement((CodeSnippetStatement) e);
            }
            else if (e is CodeVariableDeclarationStatement)
            {
                this.ValidateVariableDeclarationStatement((CodeVariableDeclarationStatement) e);
            }
            else if (e is CodeAttachEventStatement)
            {
                this.ValidateAttachEventStatement((CodeAttachEventStatement) e);
            }
            else if (e is CodeRemoveEventStatement)
            {
                this.ValidateRemoveEventStatement((CodeRemoveEventStatement) e);
            }
            else if (e is CodeGotoStatement)
            {
                ValidateGotoStatement((CodeGotoStatement) e);
            }
            else
            {
                if (!(e is CodeLabeledStatement))
                {
                    throw new ArgumentException(SR.GetString("InvalidElementType", new object[] { e.GetType().FullName }), "e");
                }
                this.ValidateLabeledStatement((CodeLabeledStatement) e);
            }
        }

        private void ValidateStatements(CodeStatementCollection stms)
        {
            IEnumerator enumerator = stms.GetEnumerator();
            while (enumerator.MoveNext())
            {
                this.ValidateStatement((CodeStatement) enumerator.Current);
            }
        }

        private void ValidateThisReferenceExpression(CodeThisReferenceExpression e)
        {
        }

        private void ValidateThrowExceptionStatement(CodeThrowExceptionStatement e)
        {
            if (e.ToThrow != null)
            {
                this.ValidateExpression(e.ToThrow);
            }
        }

        private void ValidateTryCatchFinallyStatement(CodeTryCatchFinallyStatement e)
        {
            this.ValidateStatements(e.TryStatements);
            CodeCatchClauseCollection catchClauses = e.CatchClauses;
            if (catchClauses.Count > 0)
            {
                IEnumerator enumerator = catchClauses.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    CodeCatchClause current = (CodeCatchClause) enumerator.Current;
                    ValidateTypeReference(current.CatchExceptionType);
                    ValidateIdentifier(current, "LocalName", current.LocalName);
                    this.ValidateStatements(current.Statements);
                }
            }
            CodeStatementCollection finallyStatements = e.FinallyStatements;
            if (finallyStatements.Count > 0)
            {
                this.ValidateStatements(finallyStatements);
            }
        }

        private void ValidateTypeConstructor(CodeTypeConstructor e)
        {
            this.ValidateStatements(e.Statements);
        }

        private void ValidateTypeDeclaration(CodeTypeDeclaration e)
        {
            CodeTypeDeclaration currentClass = this.currentClass;
            this.currentClass = e;
            this.ValidateTypeStart(e);
            this.ValidateTypeParameters(e.TypeParameters);
            this.ValidateTypeMembers(e);
            ValidateTypeReferences(e.BaseTypes);
            this.currentClass = currentClass;
        }

        private void ValidateTypeMember(CodeTypeMember e)
        {
            this.ValidateCommentStatements(e.Comments);
            ValidateCodeDirectives(e.StartDirectives);
            ValidateCodeDirectives(e.EndDirectives);
            if (e.LinePragma != null)
            {
                this.ValidateLinePragmaStart(e.LinePragma);
            }
            if (e is CodeMemberEvent)
            {
                this.ValidateEvent((CodeMemberEvent) e);
            }
            else if (e is CodeMemberField)
            {
                this.ValidateField((CodeMemberField) e);
            }
            else if (e is CodeMemberMethod)
            {
                this.ValidateMemberMethod((CodeMemberMethod) e);
            }
            else if (e is CodeMemberProperty)
            {
                this.ValidateProperty((CodeMemberProperty) e);
            }
            else if (e is CodeSnippetTypeMember)
            {
                this.ValidateSnippetMember((CodeSnippetTypeMember) e);
            }
            else
            {
                if (!(e is CodeTypeDeclaration))
                {
                    throw new ArgumentException(SR.GetString("InvalidElementType", new object[] { e.GetType().FullName }), "e");
                }
                this.ValidateTypeDeclaration((CodeTypeDeclaration) e);
            }
        }

        private void ValidateTypeMembers(CodeTypeDeclaration e)
        {
            foreach (CodeTypeMember member in e.Members)
            {
                this.ValidateTypeMember(member);
            }
        }

        private static void ValidateTypeName(object e, string propertyName, string typeName)
        {
            if (!CodeGenerator.IsValidLanguageIndependentTypeName(typeName))
            {
                throw new ArgumentException(SR.GetString("InvalidTypeName", new object[] { typeName, propertyName, e.GetType().FullName }), "typeName");
            }
        }

        private static void ValidateTypeOfExpression(CodeTypeOfExpression e)
        {
            ValidateTypeReference(e.Type);
        }

        private void ValidateTypeParameter(CodeTypeParameter e)
        {
            ValidateIdentifier(e, "Name", e.Name);
            ValidateTypeReferences(e.Constraints);
            this.ValidateAttributes(e.CustomAttributes);
        }

        private void ValidateTypeParameters(CodeTypeParameterCollection parameters)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                this.ValidateTypeParameter(parameters[i]);
            }
        }

        private static void ValidateTypeReference(CodeTypeReference e)
        {
            string baseType = e.BaseType;
            ValidateTypeName(e, "BaseType", baseType);
            ValidateArity(e);
            ValidateTypeReferences(e.TypeArguments);
        }

        private static void ValidateTypeReferences(CodeTypeReferenceCollection refs)
        {
            for (int i = 0; i < refs.Count; i++)
            {
                ValidateTypeReference(refs[i]);
            }
        }

        private void ValidateTypes(CodeNamespace e)
        {
            foreach (CodeTypeDeclaration declaration in e.Types)
            {
                this.ValidateTypeDeclaration(declaration);
            }
        }

        private void ValidateTypeStart(CodeTypeDeclaration e)
        {
            this.ValidateCommentStatements(e.Comments);
            if (e.CustomAttributes.Count > 0)
            {
                this.ValidateAttributes(e.CustomAttributes);
            }
            ValidateIdentifier(e, "Name", e.Name);
            if (this.IsCurrentDelegate)
            {
                CodeTypeDelegate delegate2 = (CodeTypeDelegate) e;
                ValidateTypeReference(delegate2.ReturnType);
                this.ValidateParameters(delegate2.Parameters);
            }
            else
            {
                foreach (CodeTypeReference reference in e.BaseTypes)
                {
                    ValidateTypeReference(reference);
                }
            }
        }

        private void ValidateVariableDeclarationStatement(CodeVariableDeclarationStatement e)
        {
            ValidateTypeReference(e.Type);
            ValidateIdentifier(e, "Name", e.Name);
            if (e.InitExpression != null)
            {
                this.ValidateExpression(e.InitExpression);
            }
        }

        private static void ValidateVariableReferenceExpression(CodeVariableReferenceExpression e)
        {
            ValidateIdentifier(e, "VariableName", e.VariableName);
        }

        private bool IsCurrentDelegate
        {
            get
            {
                return ((this.currentClass != null) && (this.currentClass is CodeTypeDelegate));
            }
        }

        private bool IsCurrentEnum
        {
            get
            {
                return (((this.currentClass != null) && !(this.currentClass is CodeTypeDelegate)) && this.currentClass.IsEnum);
            }
        }

        private bool IsCurrentInterface
        {
            get
            {
                return (((this.currentClass != null) && !(this.currentClass is CodeTypeDelegate)) && this.currentClass.IsInterface);
            }
        }
    }
}

