namespace System.Data.Design
{
    using Microsoft.Win32;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.Odbc;
    using System.Data.OleDb;
    using System.Data.OracleClient;
    using System.Data.SqlClient;
    using System.Design;
    using System.IO;
    using System.Reflection;

    internal abstract class QueryGeneratorBase
    {
        protected DbSourceCommand activeCommand;
        protected TypedDataSourceCodeGenerator codeGenerator;
        protected CodeDomProvider codeProvider;
        protected int commandIndex;
        protected static string commandVariableName = "command";
        protected string containerParamName = "dataSet";
        protected Type containerParamType = typeof(DataSet);
        protected string containerParamTypeName;
        protected bool declarationOnly;
        protected System.Data.Design.DesignTable designTable;
        protected bool getMethod;
        protected bool isFunctionsDataComponent;
        protected static string maxRecordsParameterName = "maxRecords";
        protected MemberAttributes methodAttributes;
        protected string methodName;
        protected DbSource methodSource;
        protected MethodTypeEnum methodType;
        protected GenericNameHandler nameHandler;
        protected bool pagingMethod;
        protected ParameterGenerationOption parameterOption;
        private static string persistScaleAndPrecisionRegistryKey = @"SOFTWARE\Microsoft\MSDataSetGenerator\PersistScaleAndPrecision";
        protected DbProviderFactory providerFactory;
        protected Type returnType = typeof(void);
        protected static string returnVariableName = "returnValue";
        private static PropertyDescriptor sqlCeParaDbTypeDescriptor;
        private static object sqlCeParameterInstance;
        private static Type sqlCeParameterType;
        private const string SqlCeParameterTypeName = "System.Data.SqlServerCe.SqlCeParameter, System.Data.SqlServerCe, Version=3.5.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";
        protected static string startRecordParameterName = "startRecord";
        protected string updateCommandName;
        protected string updateParameterName;
        protected string updateParameterTypeName;
        protected CodeTypeReference updateParameterTypeReference = CodeGenHelper.GlobalType(typeof(DataSet));

        internal QueryGeneratorBase(TypedDataSourceCodeGenerator codeGenerator)
        {
            this.codeGenerator = codeGenerator;
        }

        internal static CodeExpression AddNewParameterStatements(DesignParameter parameter, Type parameterType, DbProviderFactory factory, IList statements, CodeExpression parameterVariable)
        {
            if (parameterType == typeof(SqlParameter))
            {
                return BuildNewSqlParameterStatement(parameter);
            }
            if (parameterType == typeof(OleDbParameter))
            {
                return BuildNewOleDbParameterStatement(parameter);
            }
            if (parameterType == typeof(OdbcParameter))
            {
                return BuildNewOdbcParameterStatement(parameter);
            }
            if (parameterType == typeof(OracleParameter))
            {
                return BuildNewOracleParameterStatement(parameter);
            }
            if ((parameterType == SqlCeParameterType) && StringUtil.NotEmptyAfterTrim(parameter.ProviderType))
            {
                return BuildNewSqlCeParameterStatement(parameter);
            }
            return BuildNewUnknownParameterStatements(parameter, parameterType, factory, statements, parameterVariable);
        }

        protected void AddSetParameterStatements(DesignParameter parameter, string parameterName, CodeExpression cmdExpression, int parameterIndex, IList statements)
        {
            this.AddSetParameterStatements(parameter, parameterName, null, cmdExpression, parameterIndex, 0, statements);
        }

        protected void AddSetParameterStatements(DesignParameter parameter, string parameterName, DesignParameter isNullParameter, CodeExpression cmdExpression, int parameterIndex, int isNullParameterIndex, IList statements)
        {
            Type parameterUrtType = this.GetParameterUrtType(parameter);
            CodeCastExpression right = new CodeCastExpression(parameterUrtType, CodeGenHelper.Argument(parameterName));
            right.UserData.Add("CastIsBoxing", true);
            CodeCastExpression expression2 = null;
            CodeCastExpression expression3 = null;
            if ((this.codeGenerator != null) && CodeGenHelper.IsGeneratingJSharpCode(this.codeGenerator.CodeProvider))
            {
                expression2 = new CodeCastExpression(typeof(int), CodeGenHelper.Primitive(0));
                expression2.UserData.Add("CastIsBoxing", true);
                expression3 = new CodeCastExpression(typeof(int), CodeGenHelper.Primitive(1));
                expression3.UserData.Add("CastIsBoxing", true);
            }
            else
            {
                expression2 = new CodeCastExpression(typeof(object), CodeGenHelper.Primitive(0));
                expression3 = new CodeCastExpression(typeof(object), CodeGenHelper.Primitive(1));
            }
            CodeExpression left = CodeGenHelper.Property(CodeGenHelper.Indexer(CodeGenHelper.Property(cmdExpression, "Parameters"), CodeGenHelper.Primitive(parameterIndex)), "Value");
            CodeExpression expression5 = null;
            if (isNullParameter != null)
            {
                expression5 = CodeGenHelper.Property(CodeGenHelper.Indexer(CodeGenHelper.Property(cmdExpression, "Parameters"), CodeGenHelper.Primitive(isNullParameterIndex)), "Value");
            }
            int num = (isNullParameter == null) ? 1 : 2;
            CodeStatement[] trueStms = new CodeStatement[num];
            CodeStatement[] falseStms = new CodeStatement[num];
            if (parameter.AllowDbNull && parameterUrtType.IsValueType)
            {
                right = new CodeCastExpression(parameterUrtType, CodeGenHelper.Property(CodeGenHelper.Argument(parameterName), "Value"));
                right.UserData.Add("CastIsBoxing", true);
                trueStms[0] = CodeGenHelper.Assign(left, right);
                falseStms[0] = CodeGenHelper.Assign(left, CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DBNull)), "Value"));
                if (isNullParameter != null)
                {
                    trueStms[1] = trueStms[0];
                    falseStms[1] = falseStms[0];
                    trueStms[0] = CodeGenHelper.Assign(expression5, expression2);
                    falseStms[0] = CodeGenHelper.Assign(expression5, expression3);
                }
                statements.Add(CodeGenHelper.If(CodeGenHelper.EQ(CodeGenHelper.Property(CodeGenHelper.Argument(parameterName), "HasValue"), CodeGenHelper.Primitive(true)), trueStms, falseStms));
            }
            else if (parameter.AllowDbNull && !parameterUrtType.IsValueType)
            {
                trueStms[0] = CodeGenHelper.Assign(left, CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DBNull)), "Value"));
                falseStms[0] = CodeGenHelper.Assign(left, right);
                if (isNullParameter != null)
                {
                    trueStms[1] = trueStms[0];
                    falseStms[1] = falseStms[0];
                    trueStms[0] = CodeGenHelper.Assign(expression5, expression3);
                    falseStms[0] = CodeGenHelper.Assign(expression5, expression2);
                }
                statements.Add(CodeGenHelper.If(CodeGenHelper.IdEQ(CodeGenHelper.Argument(parameterName), CodeGenHelper.Primitive(null)), trueStms, falseStms));
            }
            else if (!parameter.AllowDbNull && !parameterUrtType.IsValueType)
            {
                CodeStatement[] statementArray3 = new CodeStatement[] { CodeGenHelper.Throw(CodeGenHelper.GlobalType(typeof(ArgumentNullException)), parameterName) };
                falseStms[0] = CodeGenHelper.Assign(left, right);
                if (isNullParameter != null)
                {
                    falseStms[1] = falseStms[0];
                    falseStms[0] = CodeGenHelper.Assign(expression5, expression2);
                }
                statements.Add(CodeGenHelper.If(CodeGenHelper.IdEQ(CodeGenHelper.Argument(parameterName), CodeGenHelper.Primitive(null)), statementArray3, falseStms));
            }
            else if (!parameter.AllowDbNull && parameterUrtType.IsValueType)
            {
                if (isNullParameter != null)
                {
                    statements.Add(CodeGenHelper.Assign(expression5, expression2));
                }
                statements.Add(CodeGenHelper.Assign(left, right));
            }
        }

        protected bool AddSetReturnParamValuesStatements(IList statements, CodeExpression commandExpression)
        {
            int count = 0;
            if (this.activeCommand.Parameters != null)
            {
                count = this.activeCommand.Parameters.Count;
            }
            for (int i = 0; i < count; i++)
            {
                DesignParameter parameter = this.activeCommand.Parameters[i];
                if (parameter == null)
                {
                    throw new DataSourceGeneratorException("Parameter type is not DesignParameter.");
                }
                if ((parameter.Direction == ParameterDirection.Output) || (parameter.Direction == ParameterDirection.InputOutput))
                {
                    Type parameterUrtType = this.GetParameterUrtType(parameter);
                    string nameFromList = this.nameHandler.GetNameFromList(parameter.ParameterName);
                    CodeExpression returnParam = CodeGenHelper.Property(CodeGenHelper.Indexer(CodeGenHelper.Property(commandExpression, "Parameters"), CodeGenHelper.Primitive(i)), "Value");
                    CodeExpression cond = CodeGenHelper.GenerateDbNullCheck(returnParam);
                    CodeExpression right = CodeGenHelper.GenerateNullExpression(parameterUrtType);
                    CodeStatement trueStm = null;
                    if (right == null)
                    {
                        if (parameter.AllowDbNull && parameterUrtType.IsValueType)
                        {
                            trueStm = CodeGenHelper.Assign(CodeGenHelper.Argument(nameFromList), CodeGenHelper.New(CodeGenHelper.NullableType(parameterUrtType), new CodeExpression[0]));
                        }
                        else if (parameter.AllowDbNull && !parameterUrtType.IsValueType)
                        {
                            trueStm = CodeGenHelper.Assign(CodeGenHelper.Argument(nameFromList), CodeGenHelper.Primitive(null));
                        }
                        else
                        {
                            trueStm = CodeGenHelper.Throw(CodeGenHelper.GlobalType(typeof(StrongTypingException)), System.Design.SR.GetString("CG_ParameterIsDBNull", new object[] { nameFromList }), CodeGenHelper.Primitive(null));
                        }
                    }
                    else
                    {
                        trueStm = CodeGenHelper.Assign(CodeGenHelper.Argument(this.nameHandler.GetNameFromList(parameter.ParameterName)), right);
                    }
                    CodeStatement falseStm = null;
                    if (parameter.AllowDbNull && parameterUrtType.IsValueType)
                    {
                        falseStm = CodeGenHelper.Assign(CodeGenHelper.Argument(nameFromList), CodeGenHelper.New(CodeGenHelper.NullableType(parameterUrtType), new CodeExpression[] { CodeGenHelper.Cast(CodeGenHelper.GlobalType(parameterUrtType), returnParam) }));
                    }
                    else
                    {
                        falseStm = CodeGenHelper.Assign(CodeGenHelper.Argument(nameFromList), CodeGenHelper.Cast(CodeGenHelper.GlobalType(parameterUrtType), returnParam));
                    }
                    statements.Add(CodeGenHelper.If(cond, trueStm, falseStm));
                }
            }
            return true;
        }

        protected void AddThrowsClauseIfNeeded(CodeMemberMethod dbMethod)
        {
            CodeTypeReference[] referenceArray = new CodeTypeReference[1];
            int count = 0;
            bool flag = false;
            if (this.activeCommand.Parameters != null)
            {
                count = this.activeCommand.Parameters.Count;
            }
            for (int i = 0; i < count; i++)
            {
                DesignParameter parameter = this.activeCommand.Parameters[i];
                if (parameter == null)
                {
                    throw new DataSourceGeneratorException("Parameter type is not DesignParameter.");
                }
                if (((parameter.Direction == ParameterDirection.Output) || (parameter.Direction == ParameterDirection.InputOutput)) && (CodeGenHelper.GenerateNullExpression(this.GetParameterUrtType(parameter)) == null))
                {
                    referenceArray[0] = CodeGenHelper.GlobalType(typeof(StrongTypingException));
                    flag = true;
                }
            }
            if (!flag)
            {
                int returnParameterPosition = this.GetReturnParameterPosition(this.activeCommand);
                if (((returnParameterPosition >= 0) && !this.getMethod) && ((this.methodSource.QueryType != QueryType.Scalar) && (CodeGenHelper.GenerateNullExpression(this.GetParameterUrtType(this.activeCommand.Parameters[returnParameterPosition])) == null)))
                {
                    referenceArray[0] = CodeGenHelper.GlobalType(typeof(StrongTypingException));
                    flag = true;
                }
            }
            if (flag)
            {
                dbMethod.UserData.Add("throwsCollection", new CodeTypeReferenceCollection(referenceArray));
            }
        }

        private static CodeExpression BuildNewOdbcParameterStatement(DesignParameter parameter)
        {
            OdbcParameter parameter2 = new OdbcParameter();
            OdbcType odbcType = OdbcType.Char;
            bool flag = false;
            if ((parameter.ProviderType != null) && (parameter.ProviderType.Length > 0))
            {
                try
                {
                    odbcType = (OdbcType) Enum.Parse(typeof(OdbcType), parameter.ProviderType);
                    flag = true;
                }
                catch
                {
                }
            }
            if (!flag)
            {
                parameter2.DbType = parameter.DbType;
                odbcType = parameter2.OdbcType;
            }
            return NewParameter(parameter, typeof(OdbcParameter), typeof(OdbcType), odbcType.ToString());
        }

        private static CodeExpression BuildNewOleDbParameterStatement(DesignParameter parameter)
        {
            OleDbParameter parameter2 = new OleDbParameter();
            OleDbType oleDbType = OleDbType.Char;
            bool flag = false;
            if ((parameter.ProviderType != null) && (parameter.ProviderType.Length > 0))
            {
                try
                {
                    oleDbType = (OleDbType) Enum.Parse(typeof(OleDbType), parameter.ProviderType);
                    flag = true;
                }
                catch
                {
                }
            }
            if (!flag)
            {
                parameter2.DbType = parameter.DbType;
                oleDbType = parameter2.OleDbType;
            }
            return NewParameter(parameter, typeof(OleDbParameter), typeof(OleDbType), oleDbType.ToString());
        }

        private static CodeExpression BuildNewOracleParameterStatement(DesignParameter parameter)
        {
            OracleParameter parameter2 = new OracleParameter();
            OracleType oracleType = OracleType.Char;
            bool flag = false;
            if ((parameter.ProviderType != null) && (parameter.ProviderType.Length > 0))
            {
                try
                {
                    oracleType = (OracleType) Enum.Parse(typeof(OracleType), parameter.ProviderType);
                    flag = true;
                }
                catch
                {
                }
            }
            if (!flag)
            {
                parameter2.DbType = parameter.DbType;
                oracleType = parameter2.OracleType;
            }
            return NewParameter(parameter, typeof(OracleParameter), typeof(OracleType), oracleType.ToString());
        }

        private static CodeExpression BuildNewSqlCeParameterStatement(DesignParameter parameter)
        {
            SqlDbType type = SqlDbType.Char;
            bool flag = false;
            if ((parameter.ProviderType != null) && (parameter.ProviderType.Length > 0))
            {
                try
                {
                    type = (SqlDbType) Enum.Parse(typeof(SqlDbType), parameter.ProviderType);
                    flag = true;
                }
                catch
                {
                }
            }
            if (!flag)
            {
                object sqlCeParameterInstance = SqlCeParameterInstance;
                if (sqlCeParameterInstance != null)
                {
                    PropertyDescriptor sqlCeParaDbTypeDescriptor = SqlCeParaDbTypeDescriptor;
                    if (sqlCeParaDbTypeDescriptor != null)
                    {
                        sqlCeParaDbTypeDescriptor.SetValue(sqlCeParameterInstance, parameter.DbType);
                        type = (SqlDbType) sqlCeParaDbTypeDescriptor.GetValue(sqlCeParameterInstance);
                    }
                }
            }
            return NewParameter(parameter, SqlCeParameterType, typeof(SqlDbType), type.ToString());
        }

        private static CodeExpression BuildNewSqlParameterStatement(DesignParameter parameter)
        {
            SqlParameter parameter2 = new SqlParameter();
            SqlDbType sqlDbType = SqlDbType.Char;
            bool flag = false;
            if ((parameter.ProviderType != null) && (parameter.ProviderType.Length > 0))
            {
                try
                {
                    sqlDbType = (SqlDbType) Enum.Parse(typeof(SqlDbType), parameter.ProviderType);
                    flag = true;
                }
                catch
                {
                }
            }
            if (!flag)
            {
                parameter2.DbType = parameter.DbType;
                sqlDbType = parameter2.SqlDbType;
            }
            return NewParameter(parameter, typeof(SqlParameter), typeof(SqlDbType), sqlDbType.ToString());
        }

        private static CodeExpression BuildNewUnknownParameterStatements(DesignParameter parameter, Type parameterType, DbProviderFactory factory, IList statements, CodeExpression parameterVariable)
        {
            if (!ParamVariableDeclared(statements))
            {
                statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(parameterType), "param", CodeGenHelper.New(CodeGenHelper.GlobalType(parameterType), new CodeExpression[0])));
                parameterVariable = CodeGenHelper.Variable("param");
            }
            else
            {
                if ((parameterVariable == null) || !(parameterVariable is CodeVariableReferenceExpression))
                {
                    parameterVariable = CodeGenHelper.Variable("param");
                }
                statements.Add(CodeGenHelper.Assign(parameterVariable, CodeGenHelper.New(CodeGenHelper.GlobalType(parameterType), new CodeExpression[0])));
            }
            IDbDataParameter parameter2 = (IDbDataParameter) Activator.CreateInstance(parameterType);
            statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(parameterVariable, "ParameterName"), CodeGenHelper.Str(parameter.ParameterName)));
            if (parameter.DbType != parameter2.DbType)
            {
                statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(parameterVariable, "DbType"), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DbType)), parameter.DbType.ToString())));
            }
            PropertyInfo providerTypeProperty = ProviderManager.GetProviderTypeProperty(factory);
            if (((providerTypeProperty != null) && (parameter.ProviderType != null)) && (parameter.ProviderType.Length > 0))
            {
                object obj2 = null;
                try
                {
                    obj2 = Enum.Parse(providerTypeProperty.PropertyType, parameter.ProviderType);
                }
                catch
                {
                }
                if (obj2 != null)
                {
                    statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(parameterVariable, providerTypeProperty.Name), CodeGenHelper.Field(CodeGenHelper.TypeExpr(CodeGenHelper.GlobalType(providerTypeProperty.PropertyType)), obj2.ToString())));
                }
            }
            if (parameter.Size != parameter2.Size)
            {
                statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(parameterVariable, "Size"), CodeGenHelper.Primitive(parameter.Size)));
            }
            if (parameter.Direction != parameter2.Direction)
            {
                statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(parameterVariable, "Direction"), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(ParameterDirection)), parameter.Direction.ToString())));
            }
            if (parameter.IsNullable != parameter2.IsNullable)
            {
                statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(parameterVariable, "IsNullable"), CodeGenHelper.Primitive(parameter.IsNullable)));
            }
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(persistScaleAndPrecisionRegistryKey))
            {
                if (key != null)
                {
                    if (parameter.Precision != parameter2.Precision)
                    {
                        statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(parameterVariable, "Precision"), CodeGenHelper.Primitive(parameter.Precision)));
                    }
                    if (parameter.Scale != parameter2.Scale)
                    {
                        statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(parameterVariable, "Scale"), CodeGenHelper.Primitive(parameter.Scale)));
                    }
                }
            }
            if (parameter.SourceColumn != parameter2.SourceColumn)
            {
                statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(parameterVariable, "SourceColumn"), CodeGenHelper.Str(parameter.SourceColumn)));
            }
            if (parameter.SourceVersion != parameter2.SourceVersion)
            {
                statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(parameterVariable, "SourceVersion"), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DataRowVersion)), parameter.SourceVersion.ToString())));
            }
            if ((parameter2 is DbParameter) && (parameter.SourceColumnNullMapping != ((DbParameter) parameter2).SourceColumnNullMapping))
            {
                statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(parameterVariable, "SourceColumnNullMapping"), CodeGenHelper.Primitive(parameter.SourceColumnNullMapping)));
            }
            return parameterVariable;
        }

        internal abstract CodeMemberMethod Generate();
        private Type GetParameterSqlType(DesignParameter parameter)
        {
            IDesignConnection connection = null;
            if (!StringUtil.EqualValue(connection.Provider, ManagedProviderNames.SqlClient))
            {
                throw new InternalException("We should never attempt to generate SqlType-parameters for non-Sql providers.");
            }
            SqlDbType sqlDbType = SqlDbType.Char;
            bool flag = false;
            if ((parameter.ProviderType != null) && (parameter.ProviderType.Length > 0))
            {
                try
                {
                    sqlDbType = (SqlDbType) Enum.Parse(typeof(SqlDbType), parameter.ProviderType);
                    flag = true;
                }
                catch
                {
                }
            }
            if (!flag)
            {
                SqlParameter parameter2 = new SqlParameter {
                    DbType = parameter.DbType
                };
                sqlDbType = parameter2.SqlDbType;
            }
            Type type2 = TypeConvertions.SqlDbTypeToSqlType(sqlDbType);
            if (type2 != null)
            {
                return type2;
            }
            if (this.codeGenerator != null)
            {
                this.codeGenerator.ProblemList.Add(new DSGeneratorProblem(System.Design.SR.GetString("CG_UnableToConvertSqlDbTypeToSqlType", new object[] { this.MethodName, parameter.Name }), ProblemSeverity.NonFatalError, this.methodSource));
            }
            return typeof(object);
        }

        protected Type GetParameterUrtType(DesignParameter parameter)
        {
            if (this.ParameterOption == ParameterGenerationOption.SqlTypes)
            {
                return this.GetParameterSqlType(parameter);
            }
            if (this.ParameterOption != ParameterGenerationOption.Objects)
            {
                if (this.ParameterOption != ParameterGenerationOption.ClrTypes)
                {
                    throw new InternalException("Unknown parameter generation option.");
                }
                Type type = null;
                if (((parameter.DbType == DbType.Time) && (this.methodSource != null)) && ((this.methodSource.Connection != null) && StringUtil.EqualValue(this.methodSource.Connection.Provider, ManagedProviderNames.SqlClient, true)))
                {
                    type = typeof(TimeSpan);
                }
                else
                {
                    type = TypeConvertions.DbTypeToUrtType(parameter.DbType);
                }
                if (type != null)
                {
                    return type;
                }
                if (this.codeGenerator != null)
                {
                    this.codeGenerator.ProblemList.Add(new DSGeneratorProblem(System.Design.SR.GetString("CG_UnableToConvertDbTypeToUrtType", new object[] { this.MethodName, parameter.Name }), ProblemSeverity.NonFatalError, this.methodSource));
                }
            }
            return typeof(object);
        }

        protected DesignParameter GetReturnParameter(DbSourceCommand command)
        {
            foreach (DesignParameter parameter in command.Parameters)
            {
                if (parameter.Direction == ParameterDirection.ReturnValue)
                {
                    return parameter;
                }
            }
            return null;
        }

        protected int GetReturnParameterPosition(DbSourceCommand command)
        {
            if ((command != null) && (command.Parameters != null))
            {
                for (int i = 0; i < command.Parameters.Count; i++)
                {
                    if (command.Parameters[i].Direction == ParameterDirection.ReturnValue)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private static CodeExpression NewParameter(DesignParameter parameter, Type parameterType, Type typeEnumType, string typeEnumValue)
        {
            if (parameterType == typeof(SqlParameter))
            {
                return CodeGenHelper.New(CodeGenHelper.GlobalType(parameterType), new CodeExpression[] { CodeGenHelper.Str(parameter.ParameterName), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeEnumType), typeEnumValue), CodeGenHelper.Primitive(parameter.Size), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(ParameterDirection)), parameter.Direction.ToString()), CodeGenHelper.Primitive(parameter.Precision), CodeGenHelper.Primitive(parameter.Scale), CodeGenHelper.Str(parameter.SourceColumn), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DataRowVersion)), parameter.SourceVersion.ToString()), CodeGenHelper.Primitive(parameter.SourceColumnNullMapping), CodeGenHelper.Primitive(null), CodeGenHelper.Str(string.Empty), CodeGenHelper.Str(string.Empty), CodeGenHelper.Str(string.Empty) });
            }
            if (parameterType == SqlCeParameterType)
            {
                return CodeGenHelper.New(CodeGenHelper.GlobalType(parameterType), new CodeExpression[] { CodeGenHelper.Str(parameter.ParameterName), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeEnumType), typeEnumValue), CodeGenHelper.Primitive(parameter.Size), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(ParameterDirection)), parameter.Direction.ToString()), CodeGenHelper.Primitive(parameter.IsNullable), CodeGenHelper.Primitive(parameter.Precision), CodeGenHelper.Primitive(parameter.Scale), CodeGenHelper.Str(parameter.SourceColumn), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DataRowVersion)), parameter.SourceVersion.ToString()), CodeGenHelper.Primitive(null) });
            }
            if (parameterType == typeof(OracleParameter))
            {
                return CodeGenHelper.New(CodeGenHelper.GlobalType(parameterType), new CodeExpression[] { CodeGenHelper.Str(parameter.ParameterName), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeEnumType), typeEnumValue), CodeGenHelper.Primitive(parameter.Size), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(ParameterDirection)), parameter.Direction.ToString()), CodeGenHelper.Str(parameter.SourceColumn), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DataRowVersion)), parameter.SourceVersion.ToString()), CodeGenHelper.Primitive(parameter.SourceColumnNullMapping), CodeGenHelper.Primitive(null) });
            }
            return CodeGenHelper.New(CodeGenHelper.GlobalType(parameterType), new CodeExpression[] { CodeGenHelper.Str(parameter.ParameterName), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeEnumType), typeEnumValue), CodeGenHelper.Primitive(parameter.Size), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(ParameterDirection)), parameter.Direction.ToString()), CodeGenHelper.Cast(CodeGenHelper.GlobalType(typeof(byte)), CodeGenHelper.Primitive(parameter.Precision)), CodeGenHelper.Cast(CodeGenHelper.GlobalType(typeof(byte)), CodeGenHelper.Primitive(parameter.Scale)), CodeGenHelper.Str(parameter.SourceColumn), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DataRowVersion)), parameter.SourceVersion.ToString()), CodeGenHelper.Primitive(parameter.SourceColumnNullMapping), CodeGenHelper.Primitive(null) });
        }

        private static bool ParamVariableDeclared(IList statements)
        {
            foreach (object obj2 in statements)
            {
                if (obj2 is CodeVariableDeclarationStatement)
                {
                    CodeVariableDeclarationStatement statement = obj2 as CodeVariableDeclarationStatement;
                    if (statement.Name == "param")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static CodeStatement SetCommandTextStatement(CodeExpression commandExpression, string commandText)
        {
            return CodeGenHelper.Assign(CodeGenHelper.Property(commandExpression, "CommandText"), CodeGenHelper.Str(commandText));
        }

        internal static CodeStatement SetCommandTypeStatement(CodeExpression commandExpression, CommandType commandType)
        {
            return CodeGenHelper.Assign(CodeGenHelper.Property(commandExpression, "CommandType"), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(CommandType)), commandType.ToString()));
        }

        internal DbSourceCommand ActiveCommand
        {
            get
            {
                return this.activeCommand;
            }
            set
            {
                this.activeCommand = value;
            }
        }

        internal CodeDomProvider CodeProvider
        {
            get
            {
                return this.codeProvider;
            }
            set
            {
                this.codeProvider = value;
            }
        }

        internal int CommandIndex
        {
            get
            {
                return this.commandIndex;
            }
            set
            {
                this.commandIndex = value;
            }
        }

        internal string ContainerParameterName
        {
            get
            {
                return this.containerParamName;
            }
            set
            {
                this.containerParamName = value;
            }
        }

        internal Type ContainerParameterType
        {
            get
            {
                return this.containerParamType;
            }
            set
            {
                this.containerParamType = value;
            }
        }

        internal string ContainerParameterTypeName
        {
            get
            {
                return this.containerParamTypeName;
            }
            set
            {
                this.containerParamTypeName = value;
            }
        }

        internal bool DeclarationOnly
        {
            get
            {
                return this.declarationOnly;
            }
            set
            {
                this.declarationOnly = value;
            }
        }

        internal System.Data.Design.DesignTable DesignTable
        {
            get
            {
                return this.designTable;
            }
            set
            {
                this.designTable = value;
            }
        }

        internal bool GenerateGetMethod
        {
            get
            {
                return this.getMethod;
            }
            set
            {
                this.getMethod = value;
            }
        }

        internal bool GeneratePagingMethod
        {
            get
            {
                return this.pagingMethod;
            }
            set
            {
                this.pagingMethod = value;
            }
        }

        internal bool IsFunctionsDataComponent
        {
            get
            {
                return this.isFunctionsDataComponent;
            }
            set
            {
                this.isFunctionsDataComponent = value;
            }
        }

        internal string MethodName
        {
            get
            {
                return this.methodName;
            }
            set
            {
                this.methodName = value;
            }
        }

        internal DbSource MethodSource
        {
            get
            {
                return this.methodSource;
            }
            set
            {
                this.methodSource = value;
            }
        }

        internal MethodTypeEnum MethodType
        {
            get
            {
                return this.methodType;
            }
            set
            {
                this.methodType = value;
            }
        }

        internal ParameterGenerationOption ParameterOption
        {
            get
            {
                return this.parameterOption;
            }
            set
            {
                this.parameterOption = value;
            }
        }

        internal DbProviderFactory ProviderFactory
        {
            get
            {
                return this.providerFactory;
            }
            set
            {
                this.providerFactory = value;
            }
        }

        internal static PropertyDescriptor SqlCeParaDbTypeDescriptor
        {
            get
            {
                if ((sqlCeParaDbTypeDescriptor == null) && (SqlCeParameterType != null))
                {
                    sqlCeParaDbTypeDescriptor = TypeDescriptor.GetProperties(SqlCeParameterType)["DbType"];
                }
                return sqlCeParaDbTypeDescriptor;
            }
        }

        internal static object SqlCeParameterInstance
        {
            get
            {
                if ((sqlCeParameterInstance == null) && (SqlCeParameterType != null))
                {
                    sqlCeParameterInstance = Activator.CreateInstance(SqlCeParameterType);
                }
                return sqlCeParameterInstance;
            }
        }

        internal static Type SqlCeParameterType
        {
            get
            {
                if (sqlCeParameterType == null)
                {
                    try
                    {
                        sqlCeParameterType = Type.GetType("System.Data.SqlServerCe.SqlCeParameter, System.Data.SqlServerCe, Version=3.5.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                    }
                    catch (FileLoadException)
                    {
                    }
                }
                return sqlCeParameterType;
            }
        }

        internal string UpdateCommandName
        {
            get
            {
                return this.updateCommandName;
            }
            set
            {
                this.updateCommandName = value;
            }
        }

        internal string UpdateParameterName
        {
            get
            {
                return this.updateParameterName;
            }
            set
            {
                this.updateParameterName = value;
            }
        }

        internal string UpdateParameterTypeName
        {
            get
            {
                return this.updateParameterTypeName;
            }
            set
            {
                this.updateParameterTypeName = value;
            }
        }

        internal CodeTypeReference UpdateParameterTypeReference
        {
            get
            {
                return this.updateParameterTypeReference;
            }
            set
            {
                this.updateParameterTypeReference = value;
            }
        }
    }
}

