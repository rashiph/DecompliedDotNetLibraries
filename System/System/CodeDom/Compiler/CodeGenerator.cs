namespace System.CodeDom.Compiler
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public abstract class CodeGenerator : ICodeGenerator
    {
        private CodeTypeDeclaration currentClass;
        private CodeTypeMember currentMember;
        private bool inNestedBinary;
        private CodeGeneratorOptions options;
        private IndentedTextWriter output;
        private const int ParameterMultilineThreshold = 15;

        protected CodeGenerator()
        {
        }

        protected virtual void ContinueOnNewLine(string st)
        {
            this.Output.WriteLine(st);
        }

        protected abstract string CreateEscapedIdentifier(string value);
        protected abstract string CreateValidIdentifier(string value);
        protected abstract void GenerateArgumentReferenceExpression(CodeArgumentReferenceExpression e);
        protected abstract void GenerateArrayCreateExpression(CodeArrayCreateExpression e);
        protected abstract void GenerateArrayIndexerExpression(CodeArrayIndexerExpression e);
        protected abstract void GenerateAssignStatement(CodeAssignStatement e);
        protected abstract void GenerateAttachEventStatement(CodeAttachEventStatement e);
        protected abstract void GenerateAttributeDeclarationsEnd(CodeAttributeDeclarationCollection attributes);
        protected abstract void GenerateAttributeDeclarationsStart(CodeAttributeDeclarationCollection attributes);
        protected abstract void GenerateBaseReferenceExpression(CodeBaseReferenceExpression e);
        protected virtual void GenerateBinaryOperatorExpression(CodeBinaryOperatorExpression e)
        {
            bool flag = false;
            this.Output.Write("(");
            this.GenerateExpression(e.Left);
            this.Output.Write(" ");
            if ((e.Left is CodeBinaryOperatorExpression) || (e.Right is CodeBinaryOperatorExpression))
            {
                if (!this.inNestedBinary)
                {
                    flag = true;
                    this.inNestedBinary = true;
                    this.Indent += 3;
                }
                this.ContinueOnNewLine("");
            }
            this.OutputOperator(e.Operator);
            this.Output.Write(" ");
            this.GenerateExpression(e.Right);
            this.Output.Write(")");
            if (flag)
            {
                this.Indent -= 3;
                this.inNestedBinary = false;
            }
        }

        protected abstract void GenerateCastExpression(CodeCastExpression e);
        public virtual void GenerateCodeFromMember(CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
        {
            if (this.output != null)
            {
                throw new InvalidOperationException(SR.GetString("CodeGenReentrance"));
            }
            this.options = (options == null) ? new CodeGeneratorOptions() : options;
            this.output = new IndentedTextWriter(writer, this.options.IndentString);
            try
            {
                CodeTypeDeclaration declaredType = new CodeTypeDeclaration();
                this.currentClass = declaredType;
                this.GenerateTypeMember(member, declaredType);
            }
            finally
            {
                this.currentClass = null;
                this.output = null;
                this.options = null;
            }
        }

        protected abstract void GenerateComment(CodeComment e);
        protected virtual void GenerateCommentStatement(CodeCommentStatement e)
        {
            if (e.Comment == null)
            {
                throw new ArgumentException(SR.GetString("Argument_NullComment", new object[] { "e" }), "e");
            }
            this.GenerateComment(e.Comment);
        }

        protected virtual void GenerateCommentStatements(CodeCommentStatementCollection e)
        {
            foreach (CodeCommentStatement statement in e)
            {
                this.GenerateCommentStatement(statement);
            }
        }

        protected virtual void GenerateCompileUnit(CodeCompileUnit e)
        {
            this.GenerateCompileUnitStart(e);
            this.GenerateNamespaces(e);
            this.GenerateCompileUnitEnd(e);
        }

        protected virtual void GenerateCompileUnitEnd(CodeCompileUnit e)
        {
            if (e.EndDirectives.Count > 0)
            {
                this.GenerateDirectives(e.EndDirectives);
            }
        }

        protected virtual void GenerateCompileUnitStart(CodeCompileUnit e)
        {
            if (e.StartDirectives.Count > 0)
            {
                this.GenerateDirectives(e.StartDirectives);
            }
        }

        protected abstract void GenerateConditionStatement(CodeConditionStatement e);
        protected abstract void GenerateConstructor(CodeConstructor e, CodeTypeDeclaration c);
        private void GenerateConstructors(CodeTypeDeclaration e)
        {
            IEnumerator enumerator = e.Members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is CodeConstructor)
                {
                    this.currentMember = (CodeTypeMember) enumerator.Current;
                    if (this.options.BlankLinesBetweenMembers)
                    {
                        this.Output.WriteLine();
                    }
                    if (this.currentMember.StartDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.StartDirectives);
                    }
                    this.GenerateCommentStatements(this.currentMember.Comments);
                    CodeConstructor current = (CodeConstructor) enumerator.Current;
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaStart(current.LinePragma);
                    }
                    this.GenerateConstructor(current, e);
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaEnd(current.LinePragma);
                    }
                    if (this.currentMember.EndDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.EndDirectives);
                    }
                }
            }
        }

        protected virtual void GenerateDecimalValue(decimal d)
        {
            this.Output.Write(d.ToString(CultureInfo.InvariantCulture));
        }

        protected virtual void GenerateDefaultValueExpression(CodeDefaultValueExpression e)
        {
        }

        protected abstract void GenerateDelegateCreateExpression(CodeDelegateCreateExpression e);
        protected abstract void GenerateDelegateInvokeExpression(CodeDelegateInvokeExpression e);
        protected virtual void GenerateDirectionExpression(CodeDirectionExpression e)
        {
            this.OutputDirection(e.Direction);
            this.GenerateExpression(e.Expression);
        }

        protected virtual void GenerateDirectives(CodeDirectiveCollection directives)
        {
        }

        protected virtual void GenerateDoubleValue(double d)
        {
            this.Output.Write(d.ToString("R", CultureInfo.InvariantCulture));
        }

        protected abstract void GenerateEntryPointMethod(CodeEntryPointMethod e, CodeTypeDeclaration c);
        protected abstract void GenerateEvent(CodeMemberEvent e, CodeTypeDeclaration c);
        protected abstract void GenerateEventReferenceExpression(CodeEventReferenceExpression e);
        private void GenerateEvents(CodeTypeDeclaration e)
        {
            IEnumerator enumerator = e.Members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is CodeMemberEvent)
                {
                    this.currentMember = (CodeTypeMember) enumerator.Current;
                    if (this.options.BlankLinesBetweenMembers)
                    {
                        this.Output.WriteLine();
                    }
                    if (this.currentMember.StartDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.StartDirectives);
                    }
                    this.GenerateCommentStatements(this.currentMember.Comments);
                    CodeMemberEvent current = (CodeMemberEvent) enumerator.Current;
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaStart(current.LinePragma);
                    }
                    this.GenerateEvent(current, e);
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaEnd(current.LinePragma);
                    }
                    if (this.currentMember.EndDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.EndDirectives);
                    }
                }
            }
        }

        protected void GenerateExpression(CodeExpression e)
        {
            if (e is CodeArrayCreateExpression)
            {
                this.GenerateArrayCreateExpression((CodeArrayCreateExpression) e);
            }
            else if (e is CodeBaseReferenceExpression)
            {
                this.GenerateBaseReferenceExpression((CodeBaseReferenceExpression) e);
            }
            else if (e is CodeBinaryOperatorExpression)
            {
                this.GenerateBinaryOperatorExpression((CodeBinaryOperatorExpression) e);
            }
            else if (e is CodeCastExpression)
            {
                this.GenerateCastExpression((CodeCastExpression) e);
            }
            else if (e is CodeDelegateCreateExpression)
            {
                this.GenerateDelegateCreateExpression((CodeDelegateCreateExpression) e);
            }
            else if (e is CodeFieldReferenceExpression)
            {
                this.GenerateFieldReferenceExpression((CodeFieldReferenceExpression) e);
            }
            else if (e is CodeArgumentReferenceExpression)
            {
                this.GenerateArgumentReferenceExpression((CodeArgumentReferenceExpression) e);
            }
            else if (e is CodeVariableReferenceExpression)
            {
                this.GenerateVariableReferenceExpression((CodeVariableReferenceExpression) e);
            }
            else if (e is CodeIndexerExpression)
            {
                this.GenerateIndexerExpression((CodeIndexerExpression) e);
            }
            else if (e is CodeArrayIndexerExpression)
            {
                this.GenerateArrayIndexerExpression((CodeArrayIndexerExpression) e);
            }
            else if (e is CodeSnippetExpression)
            {
                this.GenerateSnippetExpression((CodeSnippetExpression) e);
            }
            else if (e is CodeMethodInvokeExpression)
            {
                this.GenerateMethodInvokeExpression((CodeMethodInvokeExpression) e);
            }
            else if (e is CodeMethodReferenceExpression)
            {
                this.GenerateMethodReferenceExpression((CodeMethodReferenceExpression) e);
            }
            else if (e is CodeEventReferenceExpression)
            {
                this.GenerateEventReferenceExpression((CodeEventReferenceExpression) e);
            }
            else if (e is CodeDelegateInvokeExpression)
            {
                this.GenerateDelegateInvokeExpression((CodeDelegateInvokeExpression) e);
            }
            else if (e is CodeObjectCreateExpression)
            {
                this.GenerateObjectCreateExpression((CodeObjectCreateExpression) e);
            }
            else if (e is CodeParameterDeclarationExpression)
            {
                this.GenerateParameterDeclarationExpression((CodeParameterDeclarationExpression) e);
            }
            else if (e is CodeDirectionExpression)
            {
                this.GenerateDirectionExpression((CodeDirectionExpression) e);
            }
            else if (e is CodePrimitiveExpression)
            {
                this.GeneratePrimitiveExpression((CodePrimitiveExpression) e);
            }
            else if (e is CodePropertyReferenceExpression)
            {
                this.GeneratePropertyReferenceExpression((CodePropertyReferenceExpression) e);
            }
            else if (e is CodePropertySetValueReferenceExpression)
            {
                this.GeneratePropertySetValueReferenceExpression((CodePropertySetValueReferenceExpression) e);
            }
            else if (e is CodeThisReferenceExpression)
            {
                this.GenerateThisReferenceExpression((CodeThisReferenceExpression) e);
            }
            else if (e is CodeTypeReferenceExpression)
            {
                this.GenerateTypeReferenceExpression((CodeTypeReferenceExpression) e);
            }
            else if (e is CodeTypeOfExpression)
            {
                this.GenerateTypeOfExpression((CodeTypeOfExpression) e);
            }
            else if (e is CodeDefaultValueExpression)
            {
                this.GenerateDefaultValueExpression((CodeDefaultValueExpression) e);
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

        protected abstract void GenerateExpressionStatement(CodeExpressionStatement e);
        protected abstract void GenerateField(CodeMemberField e);
        protected abstract void GenerateFieldReferenceExpression(CodeFieldReferenceExpression e);
        private void GenerateFields(CodeTypeDeclaration e)
        {
            IEnumerator enumerator = e.Members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is CodeMemberField)
                {
                    this.currentMember = (CodeTypeMember) enumerator.Current;
                    if (this.options.BlankLinesBetweenMembers)
                    {
                        this.Output.WriteLine();
                    }
                    if (this.currentMember.StartDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.StartDirectives);
                    }
                    this.GenerateCommentStatements(this.currentMember.Comments);
                    CodeMemberField current = (CodeMemberField) enumerator.Current;
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaStart(current.LinePragma);
                    }
                    this.GenerateField(current);
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaEnd(current.LinePragma);
                    }
                    if (this.currentMember.EndDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.EndDirectives);
                    }
                }
            }
        }

        protected abstract void GenerateGotoStatement(CodeGotoStatement e);
        protected abstract void GenerateIndexerExpression(CodeIndexerExpression e);
        protected abstract void GenerateIterationStatement(CodeIterationStatement e);
        protected abstract void GenerateLabeledStatement(CodeLabeledStatement e);
        protected abstract void GenerateLinePragmaEnd(CodeLinePragma e);
        protected abstract void GenerateLinePragmaStart(CodeLinePragma e);
        protected abstract void GenerateMethod(CodeMemberMethod e, CodeTypeDeclaration c);
        protected abstract void GenerateMethodInvokeExpression(CodeMethodInvokeExpression e);
        protected abstract void GenerateMethodReferenceExpression(CodeMethodReferenceExpression e);
        protected abstract void GenerateMethodReturnStatement(CodeMethodReturnStatement e);
        private void GenerateMethods(CodeTypeDeclaration e)
        {
            IEnumerator enumerator = e.Members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (((enumerator.Current is CodeMemberMethod) && !(enumerator.Current is CodeTypeConstructor)) && !(enumerator.Current is CodeConstructor))
                {
                    this.currentMember = (CodeTypeMember) enumerator.Current;
                    if (this.options.BlankLinesBetweenMembers)
                    {
                        this.Output.WriteLine();
                    }
                    if (this.currentMember.StartDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.StartDirectives);
                    }
                    this.GenerateCommentStatements(this.currentMember.Comments);
                    CodeMemberMethod current = (CodeMemberMethod) enumerator.Current;
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaStart(current.LinePragma);
                    }
                    if (enumerator.Current is CodeEntryPointMethod)
                    {
                        this.GenerateEntryPointMethod((CodeEntryPointMethod) enumerator.Current, e);
                    }
                    else
                    {
                        this.GenerateMethod(current, e);
                    }
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaEnd(current.LinePragma);
                    }
                    if (this.currentMember.EndDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.EndDirectives);
                    }
                }
            }
        }

        protected virtual void GenerateNamespace(CodeNamespace e)
        {
            this.GenerateCommentStatements(e.Comments);
            this.GenerateNamespaceStart(e);
            this.GenerateNamespaceImports(e);
            this.Output.WriteLine("");
            this.GenerateTypes(e);
            this.GenerateNamespaceEnd(e);
        }

        protected abstract void GenerateNamespaceEnd(CodeNamespace e);
        protected abstract void GenerateNamespaceImport(CodeNamespaceImport e);
        protected void GenerateNamespaceImports(CodeNamespace e)
        {
            IEnumerator enumerator = e.Imports.GetEnumerator();
            while (enumerator.MoveNext())
            {
                CodeNamespaceImport current = (CodeNamespaceImport) enumerator.Current;
                if (current.LinePragma != null)
                {
                    this.GenerateLinePragmaStart(current.LinePragma);
                }
                this.GenerateNamespaceImport(current);
                if (current.LinePragma != null)
                {
                    this.GenerateLinePragmaEnd(current.LinePragma);
                }
            }
        }

        protected void GenerateNamespaces(CodeCompileUnit e)
        {
            foreach (CodeNamespace namespace2 in e.Namespaces)
            {
                ((ICodeGenerator) this).GenerateCodeFromNamespace(namespace2, this.output.InnerWriter, this.options);
            }
        }

        protected abstract void GenerateNamespaceStart(CodeNamespace e);
        private void GenerateNestedTypes(CodeTypeDeclaration e)
        {
            IEnumerator enumerator = e.Members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is CodeTypeDeclaration)
                {
                    if (this.options.BlankLinesBetweenMembers)
                    {
                        this.Output.WriteLine();
                    }
                    CodeTypeDeclaration current = (CodeTypeDeclaration) enumerator.Current;
                    ((ICodeGenerator) this).GenerateCodeFromType(current, this.output.InnerWriter, this.options);
                }
            }
        }

        protected abstract void GenerateObjectCreateExpression(CodeObjectCreateExpression e);
        protected virtual void GenerateParameterDeclarationExpression(CodeParameterDeclarationExpression e)
        {
            if (e.CustomAttributes.Count > 0)
            {
                this.OutputAttributeDeclarations(e.CustomAttributes);
                this.Output.Write(" ");
            }
            this.OutputDirection(e.Direction);
            this.OutputTypeNamePair(e.Type, e.Name);
        }

        protected virtual void GeneratePrimitiveExpression(CodePrimitiveExpression e)
        {
            if (e.Value == null)
            {
                this.Output.Write(this.NullToken);
            }
            else if (e.Value is string)
            {
                this.Output.Write(this.QuoteSnippetString((string) e.Value));
            }
            else if (e.Value is char)
            {
                this.Output.Write("'" + e.Value.ToString() + "'");
            }
            else if (e.Value is byte)
            {
                byte num = (byte) e.Value;
                this.Output.Write(num.ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is short)
            {
                short num2 = (short) e.Value;
                this.Output.Write(num2.ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is int)
            {
                int num3 = (int) e.Value;
                this.Output.Write(num3.ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is long)
            {
                long num4 = (long) e.Value;
                this.Output.Write(num4.ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is float)
            {
                this.GenerateSingleFloatValue((float) e.Value);
            }
            else if (e.Value is double)
            {
                this.GenerateDoubleValue((double) e.Value);
            }
            else if (e.Value is decimal)
            {
                this.GenerateDecimalValue((decimal) e.Value);
            }
            else
            {
                if (!(e.Value is bool))
                {
                    throw new ArgumentException(SR.GetString("InvalidPrimitiveType", new object[] { e.Value.GetType().ToString() }));
                }
                if ((bool) e.Value)
                {
                    this.Output.Write("true");
                }
                else
                {
                    this.Output.Write("false");
                }
            }
        }

        private void GenerateProperties(CodeTypeDeclaration e)
        {
            IEnumerator enumerator = e.Members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is CodeMemberProperty)
                {
                    this.currentMember = (CodeTypeMember) enumerator.Current;
                    if (this.options.BlankLinesBetweenMembers)
                    {
                        this.Output.WriteLine();
                    }
                    if (this.currentMember.StartDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.StartDirectives);
                    }
                    this.GenerateCommentStatements(this.currentMember.Comments);
                    CodeMemberProperty current = (CodeMemberProperty) enumerator.Current;
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaStart(current.LinePragma);
                    }
                    this.GenerateProperty(current, e);
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaEnd(current.LinePragma);
                    }
                    if (this.currentMember.EndDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.EndDirectives);
                    }
                }
            }
        }

        protected abstract void GenerateProperty(CodeMemberProperty e, CodeTypeDeclaration c);
        protected abstract void GeneratePropertyReferenceExpression(CodePropertyReferenceExpression e);
        protected abstract void GeneratePropertySetValueReferenceExpression(CodePropertySetValueReferenceExpression e);
        protected abstract void GenerateRemoveEventStatement(CodeRemoveEventStatement e);
        protected virtual void GenerateSingleFloatValue(float s)
        {
            this.Output.Write(s.ToString("R", CultureInfo.InvariantCulture));
        }

        protected virtual void GenerateSnippetCompileUnit(CodeSnippetCompileUnit e)
        {
            this.GenerateDirectives(e.StartDirectives);
            if (e.LinePragma != null)
            {
                this.GenerateLinePragmaStart(e.LinePragma);
            }
            this.Output.WriteLine(e.Value);
            if (e.LinePragma != null)
            {
                this.GenerateLinePragmaEnd(e.LinePragma);
            }
            if (e.EndDirectives.Count > 0)
            {
                this.GenerateDirectives(e.EndDirectives);
            }
        }

        protected abstract void GenerateSnippetExpression(CodeSnippetExpression e);
        protected abstract void GenerateSnippetMember(CodeSnippetTypeMember e);
        private void GenerateSnippetMembers(CodeTypeDeclaration e)
        {
            IEnumerator enumerator = e.Members.GetEnumerator();
            bool flag = false;
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is CodeSnippetTypeMember)
                {
                    flag = true;
                    this.currentMember = (CodeTypeMember) enumerator.Current;
                    if (this.options.BlankLinesBetweenMembers)
                    {
                        this.Output.WriteLine();
                    }
                    if (this.currentMember.StartDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.StartDirectives);
                    }
                    this.GenerateCommentStatements(this.currentMember.Comments);
                    CodeSnippetTypeMember current = (CodeSnippetTypeMember) enumerator.Current;
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaStart(current.LinePragma);
                    }
                    int indent = this.Indent;
                    this.Indent = 0;
                    this.GenerateSnippetMember(current);
                    this.Indent = indent;
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaEnd(current.LinePragma);
                    }
                    if (this.currentMember.EndDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.EndDirectives);
                    }
                }
            }
            if (flag)
            {
                this.Output.WriteLine();
            }
        }

        protected virtual void GenerateSnippetStatement(CodeSnippetStatement e)
        {
            this.Output.WriteLine(e.Value);
        }

        protected void GenerateStatement(CodeStatement e)
        {
            if (e.StartDirectives.Count > 0)
            {
                this.GenerateDirectives(e.StartDirectives);
            }
            if (e.LinePragma != null)
            {
                this.GenerateLinePragmaStart(e.LinePragma);
            }
            if (e is CodeCommentStatement)
            {
                this.GenerateCommentStatement((CodeCommentStatement) e);
            }
            else if (e is CodeMethodReturnStatement)
            {
                this.GenerateMethodReturnStatement((CodeMethodReturnStatement) e);
            }
            else if (e is CodeConditionStatement)
            {
                this.GenerateConditionStatement((CodeConditionStatement) e);
            }
            else if (e is CodeTryCatchFinallyStatement)
            {
                this.GenerateTryCatchFinallyStatement((CodeTryCatchFinallyStatement) e);
            }
            else if (e is CodeAssignStatement)
            {
                this.GenerateAssignStatement((CodeAssignStatement) e);
            }
            else if (e is CodeExpressionStatement)
            {
                this.GenerateExpressionStatement((CodeExpressionStatement) e);
            }
            else if (e is CodeIterationStatement)
            {
                this.GenerateIterationStatement((CodeIterationStatement) e);
            }
            else if (e is CodeThrowExceptionStatement)
            {
                this.GenerateThrowExceptionStatement((CodeThrowExceptionStatement) e);
            }
            else if (e is CodeSnippetStatement)
            {
                int indent = this.Indent;
                this.Indent = 0;
                this.GenerateSnippetStatement((CodeSnippetStatement) e);
                this.Indent = indent;
            }
            else if (e is CodeVariableDeclarationStatement)
            {
                this.GenerateVariableDeclarationStatement((CodeVariableDeclarationStatement) e);
            }
            else if (e is CodeAttachEventStatement)
            {
                this.GenerateAttachEventStatement((CodeAttachEventStatement) e);
            }
            else if (e is CodeRemoveEventStatement)
            {
                this.GenerateRemoveEventStatement((CodeRemoveEventStatement) e);
            }
            else if (e is CodeGotoStatement)
            {
                this.GenerateGotoStatement((CodeGotoStatement) e);
            }
            else
            {
                if (!(e is CodeLabeledStatement))
                {
                    throw new ArgumentException(SR.GetString("InvalidElementType", new object[] { e.GetType().FullName }), "e");
                }
                this.GenerateLabeledStatement((CodeLabeledStatement) e);
            }
            if (e.LinePragma != null)
            {
                this.GenerateLinePragmaEnd(e.LinePragma);
            }
            if (e.EndDirectives.Count > 0)
            {
                this.GenerateDirectives(e.EndDirectives);
            }
        }

        protected void GenerateStatements(CodeStatementCollection stms)
        {
            IEnumerator enumerator = stms.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ((ICodeGenerator) this).GenerateCodeFromStatement((CodeStatement) enumerator.Current, this.output.InnerWriter, this.options);
            }
        }

        protected abstract void GenerateThisReferenceExpression(CodeThisReferenceExpression e);
        protected abstract void GenerateThrowExceptionStatement(CodeThrowExceptionStatement e);
        protected abstract void GenerateTryCatchFinallyStatement(CodeTryCatchFinallyStatement e);
        private void GenerateType(CodeTypeDeclaration e)
        {
            this.currentClass = e;
            if (e.StartDirectives.Count > 0)
            {
                this.GenerateDirectives(e.StartDirectives);
            }
            this.GenerateCommentStatements(e.Comments);
            if (e.LinePragma != null)
            {
                this.GenerateLinePragmaStart(e.LinePragma);
            }
            this.GenerateTypeStart(e);
            if (this.Options.VerbatimOrder)
            {
                foreach (CodeTypeMember member in e.Members)
                {
                    this.GenerateTypeMember(member, e);
                }
            }
            else
            {
                this.GenerateFields(e);
                this.GenerateSnippetMembers(e);
                this.GenerateTypeConstructors(e);
                this.GenerateConstructors(e);
                this.GenerateProperties(e);
                this.GenerateEvents(e);
                this.GenerateMethods(e);
                this.GenerateNestedTypes(e);
            }
            this.currentClass = e;
            this.GenerateTypeEnd(e);
            if (e.LinePragma != null)
            {
                this.GenerateLinePragmaEnd(e.LinePragma);
            }
            if (e.EndDirectives.Count > 0)
            {
                this.GenerateDirectives(e.EndDirectives);
            }
        }

        protected abstract void GenerateTypeConstructor(CodeTypeConstructor e);
        private void GenerateTypeConstructors(CodeTypeDeclaration e)
        {
            IEnumerator enumerator = e.Members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is CodeTypeConstructor)
                {
                    this.currentMember = (CodeTypeMember) enumerator.Current;
                    if (this.options.BlankLinesBetweenMembers)
                    {
                        this.Output.WriteLine();
                    }
                    if (this.currentMember.StartDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.StartDirectives);
                    }
                    this.GenerateCommentStatements(this.currentMember.Comments);
                    CodeTypeConstructor current = (CodeTypeConstructor) enumerator.Current;
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaStart(current.LinePragma);
                    }
                    this.GenerateTypeConstructor(current);
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaEnd(current.LinePragma);
                    }
                    if (this.currentMember.EndDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.EndDirectives);
                    }
                }
            }
        }

        protected abstract void GenerateTypeEnd(CodeTypeDeclaration e);
        private void GenerateTypeMember(CodeTypeMember member, CodeTypeDeclaration declaredType)
        {
            if (this.options.BlankLinesBetweenMembers)
            {
                this.Output.WriteLine();
            }
            if (member is CodeTypeDeclaration)
            {
                ((ICodeGenerator) this).GenerateCodeFromType((CodeTypeDeclaration) member, this.output.InnerWriter, this.options);
                this.currentClass = declaredType;
            }
            else
            {
                if (member.StartDirectives.Count > 0)
                {
                    this.GenerateDirectives(member.StartDirectives);
                }
                this.GenerateCommentStatements(member.Comments);
                if (member.LinePragma != null)
                {
                    this.GenerateLinePragmaStart(member.LinePragma);
                }
                if (member is CodeMemberField)
                {
                    this.GenerateField((CodeMemberField) member);
                }
                else if (member is CodeMemberProperty)
                {
                    this.GenerateProperty((CodeMemberProperty) member, declaredType);
                }
                else if (member is CodeMemberMethod)
                {
                    if (member is CodeConstructor)
                    {
                        this.GenerateConstructor((CodeConstructor) member, declaredType);
                    }
                    else if (member is CodeTypeConstructor)
                    {
                        this.GenerateTypeConstructor((CodeTypeConstructor) member);
                    }
                    else if (member is CodeEntryPointMethod)
                    {
                        this.GenerateEntryPointMethod((CodeEntryPointMethod) member, declaredType);
                    }
                    else
                    {
                        this.GenerateMethod((CodeMemberMethod) member, declaredType);
                    }
                }
                else if (member is CodeMemberEvent)
                {
                    this.GenerateEvent((CodeMemberEvent) member, declaredType);
                }
                else if (member is CodeSnippetTypeMember)
                {
                    int indent = this.Indent;
                    this.Indent = 0;
                    this.GenerateSnippetMember((CodeSnippetTypeMember) member);
                    this.Indent = indent;
                    this.Output.WriteLine();
                }
                if (member.LinePragma != null)
                {
                    this.GenerateLinePragmaEnd(member.LinePragma);
                }
                if (member.EndDirectives.Count > 0)
                {
                    this.GenerateDirectives(member.EndDirectives);
                }
            }
        }

        protected virtual void GenerateTypeOfExpression(CodeTypeOfExpression e)
        {
            this.Output.Write("typeof(");
            this.OutputType(e.Type);
            this.Output.Write(")");
        }

        protected virtual void GenerateTypeReferenceExpression(CodeTypeReferenceExpression e)
        {
            this.OutputType(e.Type);
        }

        protected void GenerateTypes(CodeNamespace e)
        {
            foreach (CodeTypeDeclaration declaration in e.Types)
            {
                if (this.options.BlankLinesBetweenMembers)
                {
                    this.Output.WriteLine();
                }
                ((ICodeGenerator) this).GenerateCodeFromType(declaration, this.output.InnerWriter, this.options);
            }
        }

        protected abstract void GenerateTypeStart(CodeTypeDeclaration e);
        protected abstract void GenerateVariableDeclarationStatement(CodeVariableDeclarationStatement e);
        protected abstract void GenerateVariableReferenceExpression(CodeVariableReferenceExpression e);
        protected abstract string GetTypeOutput(CodeTypeReference value);
        private static bool IsSpecialTypeChar(char ch, ref bool nextMustBeStartChar)
        {
            switch (ch)
            {
                case '[':
                case ']':
                case '$':
                case '&':
                case '*':
                case '+':
                case ',':
                case '-':
                case '.':
                case ':':
                case '<':
                case '>':
                    nextMustBeStartChar = true;
                    return true;

                case '`':
                    return true;
            }
            return false;
        }

        protected abstract bool IsValidIdentifier(string value);
        public static bool IsValidLanguageIndependentIdentifier(string value)
        {
            return IsValidTypeNameOrIdentifier(value, false);
        }

        internal static bool IsValidLanguageIndependentTypeName(string value)
        {
            return IsValidTypeNameOrIdentifier(value, true);
        }

        private static bool IsValidTypeNameOrIdentifier(string value, bool isTypeName)
        {
            bool nextMustBeStartChar = true;
            if (value.Length == 0)
            {
                return false;
            }
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                switch (char.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.ModifierLetter:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.LetterNumber:
                    {
                        nextMustBeStartChar = false;
                        continue;
                    }
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.ConnectorPunctuation:
                        if (!nextMustBeStartChar || (c == '_'))
                        {
                            break;
                        }
                        return false;

                    default:
                        goto Label_008C;
                }
                nextMustBeStartChar = false;
                continue;
            Label_008C:
                if (!isTypeName || !IsSpecialTypeChar(c, ref nextMustBeStartChar))
                {
                    return false;
                }
            }
            return true;
        }

        protected virtual void OutputAttributeArgument(CodeAttributeArgument arg)
        {
            if ((arg.Name != null) && (arg.Name.Length > 0))
            {
                this.OutputIdentifier(arg.Name);
                this.Output.Write("=");
            }
            ((ICodeGenerator) this).GenerateCodeFromExpression(arg.Value, this.output.InnerWriter, this.options);
        }

        protected virtual void OutputAttributeDeclarations(CodeAttributeDeclarationCollection attributes)
        {
            if (attributes.Count != 0)
            {
                this.GenerateAttributeDeclarationsStart(attributes);
                bool flag = true;
                IEnumerator enumerator = attributes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        this.ContinueOnNewLine(", ");
                    }
                    CodeAttributeDeclaration current = (CodeAttributeDeclaration) enumerator.Current;
                    this.Output.Write(current.Name);
                    this.Output.Write("(");
                    bool flag2 = true;
                    foreach (CodeAttributeArgument argument in current.Arguments)
                    {
                        if (flag2)
                        {
                            flag2 = false;
                        }
                        else
                        {
                            this.Output.Write(", ");
                        }
                        this.OutputAttributeArgument(argument);
                    }
                    this.Output.Write(")");
                }
                this.GenerateAttributeDeclarationsEnd(attributes);
            }
        }

        protected virtual void OutputDirection(FieldDirection dir)
        {
            switch (dir)
            {
                case FieldDirection.In:
                    break;

                case FieldDirection.Out:
                    this.Output.Write("out ");
                    return;

                case FieldDirection.Ref:
                    this.Output.Write("ref ");
                    break;

                default:
                    return;
            }
        }

        protected virtual void OutputExpressionList(CodeExpressionCollection expressions)
        {
            this.OutputExpressionList(expressions, false);
        }

        protected virtual void OutputExpressionList(CodeExpressionCollection expressions, bool newlineBetweenItems)
        {
            bool flag = true;
            IEnumerator enumerator = expressions.GetEnumerator();
            this.Indent++;
            while (enumerator.MoveNext())
            {
                if (flag)
                {
                    flag = false;
                }
                else if (newlineBetweenItems)
                {
                    this.ContinueOnNewLine(",");
                }
                else
                {
                    this.Output.Write(", ");
                }
                ((ICodeGenerator) this).GenerateCodeFromExpression((CodeExpression) enumerator.Current, this.output.InnerWriter, this.options);
            }
            this.Indent--;
        }

        protected virtual void OutputFieldScopeModifier(MemberAttributes attributes)
        {
            MemberAttributes attributes2 = attributes & MemberAttributes.VTableMask;
            if (attributes2 == MemberAttributes.New)
            {
                this.Output.Write("new ");
            }
            switch ((attributes & MemberAttributes.ScopeMask))
            {
                case MemberAttributes.Final:
                case MemberAttributes.Override:
                    break;

                case MemberAttributes.Static:
                    this.Output.Write("static ");
                    return;

                case MemberAttributes.Const:
                    this.Output.Write("const ");
                    break;

                default:
                    return;
            }
        }

        protected virtual void OutputIdentifier(string ident)
        {
            this.Output.Write(ident);
        }

        protected virtual void OutputMemberAccessModifier(MemberAttributes attributes)
        {
            MemberAttributes attributes2 = attributes & MemberAttributes.AccessMask;
            if (attributes2 <= MemberAttributes.Family)
            {
                if (attributes2 != MemberAttributes.Assembly)
                {
                    if (attributes2 != MemberAttributes.FamilyAndAssembly)
                    {
                        if (attributes2 == MemberAttributes.Family)
                        {
                            this.Output.Write("protected ");
                        }
                        return;
                    }
                    this.Output.Write("internal ");
                    return;
                }
            }
            else
            {
                switch (attributes2)
                {
                    case MemberAttributes.FamilyOrAssembly:
                        this.Output.Write("protected internal ");
                        return;

                    case MemberAttributes.Private:
                        this.Output.Write("private ");
                        return;

                    case MemberAttributes.Public:
                        this.Output.Write("public ");
                        return;
                }
                return;
            }
            this.Output.Write("internal ");
        }

        protected virtual void OutputMemberScopeModifier(MemberAttributes attributes)
        {
            MemberAttributes attributes2 = attributes & MemberAttributes.VTableMask;
            if (attributes2 == MemberAttributes.New)
            {
                this.Output.Write("new ");
            }
            switch ((attributes & MemberAttributes.ScopeMask))
            {
                case MemberAttributes.Abstract:
                    this.Output.Write("abstract ");
                    return;

                case MemberAttributes.Final:
                    this.Output.Write("");
                    return;

                case MemberAttributes.Static:
                    this.Output.Write("static ");
                    return;

                case MemberAttributes.Override:
                    this.Output.Write("override ");
                    return;
            }
            switch ((attributes & MemberAttributes.AccessMask))
            {
                case MemberAttributes.Family:
                case MemberAttributes.Public:
                    this.Output.Write("virtual ");
                    break;
            }
        }

        protected virtual void OutputOperator(CodeBinaryOperatorType op)
        {
            switch (op)
            {
                case CodeBinaryOperatorType.Add:
                    this.Output.Write("+");
                    return;

                case CodeBinaryOperatorType.Subtract:
                    this.Output.Write("-");
                    return;

                case CodeBinaryOperatorType.Multiply:
                    this.Output.Write("*");
                    return;

                case CodeBinaryOperatorType.Divide:
                    this.Output.Write("/");
                    return;

                case CodeBinaryOperatorType.Modulus:
                    this.Output.Write("%");
                    return;

                case CodeBinaryOperatorType.Assign:
                    this.Output.Write("=");
                    return;

                case CodeBinaryOperatorType.IdentityInequality:
                    this.Output.Write("!=");
                    return;

                case CodeBinaryOperatorType.IdentityEquality:
                    this.Output.Write("==");
                    return;

                case CodeBinaryOperatorType.ValueEquality:
                    this.Output.Write("==");
                    return;

                case CodeBinaryOperatorType.BitwiseOr:
                    this.Output.Write("|");
                    return;

                case CodeBinaryOperatorType.BitwiseAnd:
                    this.Output.Write("&");
                    return;

                case CodeBinaryOperatorType.BooleanOr:
                    this.Output.Write("||");
                    return;

                case CodeBinaryOperatorType.BooleanAnd:
                    this.Output.Write("&&");
                    return;

                case CodeBinaryOperatorType.LessThan:
                    this.Output.Write("<");
                    return;

                case CodeBinaryOperatorType.LessThanOrEqual:
                    this.Output.Write("<=");
                    return;

                case CodeBinaryOperatorType.GreaterThan:
                    this.Output.Write(">");
                    return;

                case CodeBinaryOperatorType.GreaterThanOrEqual:
                    this.Output.Write(">=");
                    return;
            }
        }

        protected virtual void OutputParameters(CodeParameterDeclarationExpressionCollection parameters)
        {
            bool flag = true;
            bool flag2 = parameters.Count > 15;
            if (flag2)
            {
                this.Indent += 3;
            }
            IEnumerator enumerator = parameters.GetEnumerator();
            while (enumerator.MoveNext())
            {
                CodeParameterDeclarationExpression current = (CodeParameterDeclarationExpression) enumerator.Current;
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    this.Output.Write(", ");
                }
                if (flag2)
                {
                    this.ContinueOnNewLine("");
                }
                this.GenerateExpression(current);
            }
            if (flag2)
            {
                this.Indent -= 3;
            }
        }

        protected abstract void OutputType(CodeTypeReference typeRef);
        protected virtual void OutputTypeAttributes(TypeAttributes attributes, bool isStruct, bool isEnum)
        {
            switch ((attributes & TypeAttributes.NestedFamORAssem))
            {
                case TypeAttributes.Public:
                case TypeAttributes.NestedPublic:
                    this.Output.Write("public ");
                    break;

                case TypeAttributes.NestedPrivate:
                    this.Output.Write("private ");
                    break;
            }
            if (isStruct)
            {
                this.Output.Write("struct ");
            }
            else if (isEnum)
            {
                this.Output.Write("enum ");
            }
            else
            {
                TypeAttributes attributes3 = attributes & TypeAttributes.ClassSemanticsMask;
                if (attributes3 != TypeAttributes.AnsiClass)
                {
                    if (attributes3 != TypeAttributes.ClassSemanticsMask)
                    {
                        return;
                    }
                }
                else
                {
                    if ((attributes & TypeAttributes.Sealed) == TypeAttributes.Sealed)
                    {
                        this.Output.Write("sealed ");
                    }
                    if ((attributes & TypeAttributes.Abstract) == TypeAttributes.Abstract)
                    {
                        this.Output.Write("abstract ");
                    }
                    this.Output.Write("class ");
                    return;
                }
                this.Output.Write("interface ");
            }
        }

        protected virtual void OutputTypeNamePair(CodeTypeReference typeRef, string name)
        {
            this.OutputType(typeRef);
            this.Output.Write(" ");
            this.OutputIdentifier(name);
        }

        protected abstract string QuoteSnippetString(string value);
        protected abstract bool Supports(GeneratorSupport support);
        string ICodeGenerator.CreateEscapedIdentifier(string value)
        {
            return this.CreateEscapedIdentifier(value);
        }

        string ICodeGenerator.CreateValidIdentifier(string value)
        {
            return this.CreateValidIdentifier(value);
        }

        void ICodeGenerator.GenerateCodeFromCompileUnit(CodeCompileUnit e, TextWriter w, CodeGeneratorOptions o)
        {
            bool flag = false;
            if ((this.output != null) && (w != this.output.InnerWriter))
            {
                throw new InvalidOperationException(SR.GetString("CodeGenOutputWriter"));
            }
            if (this.output == null)
            {
                flag = true;
                this.options = (o == null) ? new CodeGeneratorOptions() : o;
                this.output = new IndentedTextWriter(w, this.options.IndentString);
            }
            try
            {
                if (e is CodeSnippetCompileUnit)
                {
                    this.GenerateSnippetCompileUnit((CodeSnippetCompileUnit) e);
                }
                else
                {
                    this.GenerateCompileUnit(e);
                }
            }
            finally
            {
                if (flag)
                {
                    this.output = null;
                    this.options = null;
                }
            }
        }

        void ICodeGenerator.GenerateCodeFromExpression(CodeExpression e, TextWriter w, CodeGeneratorOptions o)
        {
            bool flag = false;
            if ((this.output != null) && (w != this.output.InnerWriter))
            {
                throw new InvalidOperationException(SR.GetString("CodeGenOutputWriter"));
            }
            if (this.output == null)
            {
                flag = true;
                this.options = (o == null) ? new CodeGeneratorOptions() : o;
                this.output = new IndentedTextWriter(w, this.options.IndentString);
            }
            try
            {
                this.GenerateExpression(e);
            }
            finally
            {
                if (flag)
                {
                    this.output = null;
                    this.options = null;
                }
            }
        }

        void ICodeGenerator.GenerateCodeFromNamespace(CodeNamespace e, TextWriter w, CodeGeneratorOptions o)
        {
            bool flag = false;
            if ((this.output != null) && (w != this.output.InnerWriter))
            {
                throw new InvalidOperationException(SR.GetString("CodeGenOutputWriter"));
            }
            if (this.output == null)
            {
                flag = true;
                this.options = (o == null) ? new CodeGeneratorOptions() : o;
                this.output = new IndentedTextWriter(w, this.options.IndentString);
            }
            try
            {
                this.GenerateNamespace(e);
            }
            finally
            {
                if (flag)
                {
                    this.output = null;
                    this.options = null;
                }
            }
        }

        void ICodeGenerator.GenerateCodeFromStatement(CodeStatement e, TextWriter w, CodeGeneratorOptions o)
        {
            bool flag = false;
            if ((this.output != null) && (w != this.output.InnerWriter))
            {
                throw new InvalidOperationException(SR.GetString("CodeGenOutputWriter"));
            }
            if (this.output == null)
            {
                flag = true;
                this.options = (o == null) ? new CodeGeneratorOptions() : o;
                this.output = new IndentedTextWriter(w, this.options.IndentString);
            }
            try
            {
                this.GenerateStatement(e);
            }
            finally
            {
                if (flag)
                {
                    this.output = null;
                    this.options = null;
                }
            }
        }

        void ICodeGenerator.GenerateCodeFromType(CodeTypeDeclaration e, TextWriter w, CodeGeneratorOptions o)
        {
            bool flag = false;
            if ((this.output != null) && (w != this.output.InnerWriter))
            {
                throw new InvalidOperationException(SR.GetString("CodeGenOutputWriter"));
            }
            if (this.output == null)
            {
                flag = true;
                this.options = (o == null) ? new CodeGeneratorOptions() : o;
                this.output = new IndentedTextWriter(w, this.options.IndentString);
            }
            try
            {
                this.GenerateType(e);
            }
            finally
            {
                if (flag)
                {
                    this.output = null;
                    this.options = null;
                }
            }
        }

        string ICodeGenerator.GetTypeOutput(CodeTypeReference type)
        {
            return this.GetTypeOutput(type);
        }

        bool ICodeGenerator.IsValidIdentifier(string value)
        {
            return this.IsValidIdentifier(value);
        }

        bool ICodeGenerator.Supports(GeneratorSupport support)
        {
            return this.Supports(support);
        }

        void ICodeGenerator.ValidateIdentifier(string value)
        {
            this.ValidateIdentifier(value);
        }

        protected virtual void ValidateIdentifier(string value)
        {
            if (!this.IsValidIdentifier(value))
            {
                throw new ArgumentException(SR.GetString("InvalidIdentifier", new object[] { value }));
            }
        }

        public static void ValidateIdentifiers(CodeObject e)
        {
            new CodeValidator().ValidateIdentifiers(e);
        }

        protected CodeTypeDeclaration CurrentClass
        {
            get
            {
                return this.currentClass;
            }
        }

        protected CodeTypeMember CurrentMember
        {
            get
            {
                return this.currentMember;
            }
        }

        protected string CurrentMemberName
        {
            get
            {
                if (this.currentMember != null)
                {
                    return this.currentMember.Name;
                }
                return "<% unknown %>";
            }
        }

        protected string CurrentTypeName
        {
            get
            {
                if (this.currentClass != null)
                {
                    return this.currentClass.Name;
                }
                return "<% unknown %>";
            }
        }

        protected int Indent
        {
            get
            {
                return this.output.Indent;
            }
            set
            {
                this.output.Indent = value;
            }
        }

        protected bool IsCurrentClass
        {
            get
            {
                return (((this.currentClass != null) && !(this.currentClass is CodeTypeDelegate)) && this.currentClass.IsClass);
            }
        }

        protected bool IsCurrentDelegate
        {
            get
            {
                return ((this.currentClass != null) && (this.currentClass is CodeTypeDelegate));
            }
        }

        protected bool IsCurrentEnum
        {
            get
            {
                return (((this.currentClass != null) && !(this.currentClass is CodeTypeDelegate)) && this.currentClass.IsEnum);
            }
        }

        protected bool IsCurrentInterface
        {
            get
            {
                return (((this.currentClass != null) && !(this.currentClass is CodeTypeDelegate)) && this.currentClass.IsInterface);
            }
        }

        protected bool IsCurrentStruct
        {
            get
            {
                return (((this.currentClass != null) && !(this.currentClass is CodeTypeDelegate)) && this.currentClass.IsStruct);
            }
        }

        protected abstract string NullToken { get; }

        protected CodeGeneratorOptions Options
        {
            get
            {
                return this.options;
            }
        }

        protected TextWriter Output
        {
            get
            {
                return this.output;
            }
        }
    }
}

