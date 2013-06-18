namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;

    internal class CodeMethodMap
    {
        private CodeStatementCollection _begin;
        private CodeStatementCollection _container;
        private CodeStatementCollection _end;
        private CodeStatementCollection _fields;
        private CodeStatementCollection _locals;
        private CodeMemberMethod _method;
        private CodeStatementCollection _statements;
        private CodeStatementCollection _targetStatements;
        private CodeStatementCollection _variables;

        internal CodeMethodMap(CodeMemberMethod method) : this(null, method)
        {
        }

        internal CodeMethodMap(CodeStatementCollection targetStatements, CodeMemberMethod method)
        {
            this._method = method;
            if (targetStatements != null)
            {
                this._targetStatements = targetStatements;
            }
            else
            {
                this._targetStatements = this._method.Statements;
            }
        }

        internal void Add(CodeStatementCollection statements)
        {
            foreach (CodeStatement statement in statements)
            {
                string str = statement.UserData["IContainer"] as string;
                if ((str != null) && (str == "IContainer"))
                {
                    this.ContainerStatements.Add(statement);
                }
                else if ((statement is CodeAssignStatement) && (((CodeAssignStatement) statement).Left is CodeFieldReferenceExpression))
                {
                    this.FieldAssignments.Add(statement);
                }
                else if ((statement is CodeAssignStatement) && (((CodeAssignStatement) statement).Left is CodeVariableReferenceExpression))
                {
                    this.VariableAssignments.Add(statement);
                }
                else if (statement is CodeVariableDeclarationStatement)
                {
                    this.LocalVariables.Add(statement);
                }
                else
                {
                    string str2 = statement.UserData["statement-ordering"] as string;
                    if (str2 == null)
                    {
                        goto Label_013A;
                    }
                    string str3 = str2;
                    if (str3 == null)
                    {
                        goto Label_012B;
                    }
                    if (!(str3 == "begin"))
                    {
                        if (str3 == "end")
                        {
                            goto Label_011C;
                        }
                        if (str3 == "default")
                        {
                        }
                        goto Label_012B;
                    }
                    this.BeginStatements.Add(statement);
                }
                continue;
            Label_011C:
                this.EndStatements.Add(statement);
                continue;
            Label_012B:
                this.Statements.Add(statement);
                continue;
            Label_013A:
                this.Statements.Add(statement);
            }
        }

        internal void Combine()
        {
            if (this._container != null)
            {
                this._targetStatements.AddRange(this._container);
            }
            if (this._locals != null)
            {
                this._targetStatements.AddRange(this._locals);
            }
            if (this._fields != null)
            {
                this._targetStatements.AddRange(this._fields);
            }
            if (this._variables != null)
            {
                this._targetStatements.AddRange(this._variables);
            }
            if (this._begin != null)
            {
                this._targetStatements.AddRange(this._begin);
            }
            if (this._statements != null)
            {
                this._targetStatements.AddRange(this._statements);
            }
            if (this._end != null)
            {
                this._targetStatements.AddRange(this._end);
            }
        }

        internal CodeStatementCollection BeginStatements
        {
            get
            {
                if (this._begin == null)
                {
                    this._begin = new CodeStatementCollection();
                }
                return this._begin;
            }
        }

        internal CodeStatementCollection ContainerStatements
        {
            get
            {
                if (this._container == null)
                {
                    this._container = new CodeStatementCollection();
                }
                return this._container;
            }
        }

        internal CodeStatementCollection EndStatements
        {
            get
            {
                if (this._end == null)
                {
                    this._end = new CodeStatementCollection();
                }
                return this._end;
            }
        }

        internal CodeStatementCollection FieldAssignments
        {
            get
            {
                if (this._fields == null)
                {
                    this._fields = new CodeStatementCollection();
                }
                return this._fields;
            }
        }

        internal CodeStatementCollection LocalVariables
        {
            get
            {
                if (this._locals == null)
                {
                    this._locals = new CodeStatementCollection();
                }
                return this._locals;
            }
        }

        internal CodeMemberMethod Method
        {
            get
            {
                return this._method;
            }
        }

        internal CodeStatementCollection Statements
        {
            get
            {
                if (this._statements == null)
                {
                    this._statements = new CodeStatementCollection();
                }
                return this._statements;
            }
        }

        internal CodeStatementCollection VariableAssignments
        {
            get
            {
                if (this._variables == null)
                {
                    this._variables = new CodeStatementCollection();
                }
                return this._variables;
            }
        }
    }
}

