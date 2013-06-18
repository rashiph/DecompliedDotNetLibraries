namespace System.Management
{
    using Microsoft.CSharp;
    using Microsoft.JScript;
    using Microsoft.VisualBasic;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class ManagementClassGenerator
    {
        private string arrConvFuncName;
        private ArrayList arrKeys;
        private ArrayList arrKeyType;
        private bool bDateConversionFunctionsAdded;
        private bool bHasEmbeddedProperties;
        private ArrayList BitMap;
        private ArrayList BitValues;
        private bool bSingletonClass;
        private bool bTimeSpanConversionFunctionsAdded;
        private bool bUnsignedSupported;
        private CodeAttributeArgument caa;
        private CodeAttributeDeclaration cad;
        private CodeBinaryOperatorExpression cboe;
        private CodeTypeDeclaration cc;
        private CodeTypeDeclaration ccc;
        private CodeConstructor cctor;
        private CodeMemberField cf;
        private CodeIterationStatement cfls;
        private CodeIndexerExpression cie;
        private CodeConditionStatement cis;
        private ManagementClass classobj;
        private CodeMemberField cmf;
        private CodeMethodInvokeExpression cmie;
        private CodeExpressionStatement cmis;
        private CodeMemberMethod cmm;
        private CodeMemberProperty cmp;
        private CodeNamespace cn;
        private CodeObjectCreateExpression coce;
        private ArrayList CommentsString;
        private CodeDomProvider cp;
        private CodeParameterDeclarationExpression cpde;
        private CodePropertyReferenceExpression cpre;
        private const int DMTF_DATETIME_STR_LENGTH = 0x19;
        private CodeTypeDeclaration ecc;
        private CodeTypeDeclaration EnumObj;
        private string enumType;
        private string genFileName;
        private const int IDS_COMMENT_ATTRIBPROP = 3;
        private const int IDS_COMMENT_AUTOCOMMITPROP = 14;
        private const int IDS_COMMENT_CLASSBEGIN = 6;
        private const int IDS_COMMENT_CLASSNAME = 10;
        private const int IDS_COMMENT_CONSTRUCTORS = 8;
        private const int IDS_COMMENT_CURRENTOBJ = 0x16;
        private const int IDS_COMMENT_DATECONVFUNC = 4;
        private const int IDS_COMMENT_EMBEDDEDOBJ = 0x15;
        private const int IDS_COMMENT_ENUMIMPL = 0x12;
        private const int IDS_COMMENT_FLAGFOREMBEDDED = 0x17;
        private const int IDS_COMMENT_GETINSTANCES = 5;
        private const int IDS_COMMENT_ISPROPNULL = 1;
        private const int IDS_COMMENT_LATEBOUNDOBJ = 12;
        private const int IDS_COMMENT_LATEBOUNDPROP = 0x13;
        private const int IDS_COMMENT_MGMTPATH = 15;
        private const int IDS_COMMENT_MGMTSCOPE = 13;
        private const int IDS_COMMENT_ORIG_NAMESPACE = 9;
        private const int IDS_COMMENT_PRIV_AUTOCOMMIT = 7;
        private const int IDS_COMMENT_PROP_TYPECONVERTER = 0x10;
        private const int IDS_COMMENT_RESETPROP = 2;
        private const int IDS_COMMENT_SHOULDSERIALIZE = 0;
        private const int IDS_COMMENT_SYSOBJECT = 11;
        private const int IDS_COMMENT_SYSPROPCLASS = 0x11;
        private const int IDS_COMMENTS_CREATEDCLASS = 20;
        private string NETNamespace;
        private string OriginalClassName;
        private string OriginalNamespace;
        private string OriginalPath;
        private string OriginalServer;
        private SortedList PrivateNamesUsed;
        private SortedList PublicMethods;
        private SortedList PublicNamesUsed;
        private SortedList PublicProperties;
        private TextWriter tw;
        private ArrayList ValueMap;
        private ArrayList Values;
        private string VSVERSION;

        public ManagementClassGenerator()
        {
            this.VSVERSION = "8.0.0.0";
            this.OriginalServer = string.Empty;
            this.OriginalNamespace = string.Empty;
            this.OriginalClassName = string.Empty;
            this.OriginalPath = string.Empty;
            this.bUnsignedSupported = true;
            this.NETNamespace = string.Empty;
            this.arrConvFuncName = string.Empty;
            this.enumType = string.Empty;
            this.genFileName = string.Empty;
            this.arrKeyType = new ArrayList(5);
            this.arrKeys = new ArrayList(5);
            this.BitMap = new ArrayList(5);
            this.BitValues = new ArrayList(5);
            this.ValueMap = new ArrayList(5);
            this.Values = new ArrayList(5);
            this.PublicProperties = new SortedList(StringComparer.OrdinalIgnoreCase);
            this.PublicMethods = new SortedList(StringComparer.OrdinalIgnoreCase);
            this.PublicNamesUsed = new SortedList(StringComparer.OrdinalIgnoreCase);
            this.PrivateNamesUsed = new SortedList(StringComparer.OrdinalIgnoreCase);
            this.CommentsString = new ArrayList(5);
        }

        public ManagementClassGenerator(ManagementClass cls)
        {
            this.VSVERSION = "8.0.0.0";
            this.OriginalServer = string.Empty;
            this.OriginalNamespace = string.Empty;
            this.OriginalClassName = string.Empty;
            this.OriginalPath = string.Empty;
            this.bUnsignedSupported = true;
            this.NETNamespace = string.Empty;
            this.arrConvFuncName = string.Empty;
            this.enumType = string.Empty;
            this.genFileName = string.Empty;
            this.arrKeyType = new ArrayList(5);
            this.arrKeys = new ArrayList(5);
            this.BitMap = new ArrayList(5);
            this.BitValues = new ArrayList(5);
            this.ValueMap = new ArrayList(5);
            this.Values = new ArrayList(5);
            this.PublicProperties = new SortedList(StringComparer.OrdinalIgnoreCase);
            this.PublicMethods = new SortedList(StringComparer.OrdinalIgnoreCase);
            this.PublicNamesUsed = new SortedList(StringComparer.OrdinalIgnoreCase);
            this.PrivateNamesUsed = new SortedList(StringComparer.OrdinalIgnoreCase);
            this.CommentsString = new ArrayList(5);
            this.classobj = cls;
        }

        private void AddClassComments(CodeTypeDeclaration cc)
        {
            cc.Comments.Add(new CodeCommentStatement(GetString("COMMENT_SHOULDSERIALIZE")));
            cc.Comments.Add(new CodeCommentStatement(GetString("COMMENT_ISPROPNULL")));
            cc.Comments.Add(new CodeCommentStatement(GetString("COMMENT_RESETPROP")));
            cc.Comments.Add(new CodeCommentStatement(GetString("COMMENT_ATTRIBPROP")));
        }

        private void AddCommentsForEmbeddedProperties()
        {
            this.cc.Comments.Add(new CodeCommentStatement(GetString("")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDDED_COMMENT1")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDDED_COMMENT2")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDDED_COMMENT3")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDDED_COMMENT4")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDDED_COMMENT5")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDDED_COMMENT6")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDDED_COMMENT7")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_VB_CODESAMP1")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_VB_CODESAMP2")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_VB_CODESAMP3")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_VB_CODESAMP4")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_VB_CODESAMP5")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_VB_CODESAMP6")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_VB_CODESAMP7")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_VB_CODESAMP8")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_VB_CODESAMP9")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_VB_CODESAMP10")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDDED_COMMENT8")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_CS_CODESAMP1")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_CS_CODESAMP2")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_CS_CODESAMP3")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_CS_CODESAMP4")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_CS_CODESAMP5")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_CS_CODESAMP6")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_CS_CODESAMP7")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_CS_CODESAMP8")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_CS_CODESAMP9")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_CS_CODESAMP10")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_CS_CODESAMP11")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_CS_CODESAMP12")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_CS_CODESAMP13")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_CS_CODESAMP14")));
            this.cc.Comments.Add(new CodeCommentStatement(GetString("EMBEDED_CS_CODESAMP15")));
        }

        private void AddGetStatementsForEnumArray(CodeIndexerExpression ciProp, CodeMemberProperty cmProp)
        {
            string name = "arrEnumVals";
            string str2 = "enumToRet";
            string str3 = "counter";
            string baseType = cmProp.Type.BaseType;
            cmProp.GetStatements.Add(new CodeVariableDeclarationStatement("System.Array", name, new CodeCastExpression(new CodeTypeReference("System.Array"), ciProp)));
            cmProp.GetStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(baseType, 1), str2, new CodeArrayCreateExpression(new CodeTypeReference(baseType), new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(name), "Length"))));
            this.cfls = new CodeIterationStatement();
            cmProp.GetStatements.Add(new CodeVariableDeclarationStatement("System.Int32", str3, new CodePrimitiveExpression(0)));
            this.cfls.InitStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(str3), new CodePrimitiveExpression(0));
            CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(str3),
                Operator = CodeBinaryOperatorType.LessThan,
                Right = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(name), "Length")
            };
            this.cfls.TestExpression = expression;
            this.cfls.IncrementStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(str3), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(str3), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1)));
            CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression {
                Method = { MethodName = "GetValue", TargetObject = new CodeVariableReferenceExpression(name) }
            };
            expression2.Parameters.Add(new CodeVariableReferenceExpression(str3));
            CodeMethodInvokeExpression expression3 = new CodeMethodInvokeExpression {
                Method = { TargetObject = new CodeTypeReferenceExpression("System.Convert") }
            };
            expression3.Parameters.Add(expression2);
            expression3.Method.MethodName = this.arrConvFuncName;
            this.cfls.Statements.Add(new CodeAssignStatement(new CodeIndexerExpression(new CodeVariableReferenceExpression(str2), new CodeExpression[] { new CodeVariableReferenceExpression(str3) }), new CodeCastExpression(new CodeTypeReference(baseType), expression3)));
            cmProp.GetStatements.Add(this.cfls);
            cmProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(str2)));
        }

        private void AddPropertySet(CodeIndexerExpression prop, bool bArray, CodeStatementCollection statColl, string strType, CodeVariableReferenceExpression varValue)
        {
            if (varValue == null)
            {
                varValue = new CodeVariableReferenceExpression("value");
            }
            if (!bArray)
            {
                statColl.Add(new CodeAssignStatement(prop, this.ConvertPropertyToString(strType, varValue)));
            }
            else
            {
                string name = "len";
                string str2 = "iCounter";
                string str3 = "arrProp";
                CodeConditionStatement statement = new CodeConditionStatement();
                CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression {
                    Left = varValue,
                    Operator = CodeBinaryOperatorType.IdentityInequality,
                    Right = new CodePrimitiveExpression(null)
                };
                statement.Condition = expression;
                CodePropertyReferenceExpression initExpression = new CodePropertyReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Array"), varValue), "Length");
                statement.TrueStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), name, initExpression));
                CodeTypeReference type = new CodeTypeReference(new CodeTypeReference("System.String"), 1);
                statement.TrueStatements.Add(new CodeVariableDeclarationStatement(type, str3, new CodeArrayCreateExpression(new CodeTypeReference("System.String"), new CodeVariableReferenceExpression(name))));
                this.cfls = new CodeIterationStatement();
                this.cfls.InitStatement = new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str2, new CodePrimitiveExpression(0));
                expression = new CodeBinaryOperatorExpression {
                    Left = new CodeVariableReferenceExpression(str2),
                    Operator = CodeBinaryOperatorType.LessThan,
                    Right = new CodeVariableReferenceExpression(name)
                };
                this.cfls.TestExpression = expression;
                this.cfls.IncrementStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(str2), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(str2), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1)));
                CodeMethodInvokeExpression beginingExpression = new CodeMethodInvokeExpression {
                    Method = { MethodName = "GetValue", TargetObject = new CodeCastExpression(new CodeTypeReference("System.Array"), varValue) }
                };
                beginingExpression.Parameters.Add(new CodeVariableReferenceExpression(str2));
                this.cfls.Statements.Add(new CodeAssignStatement(new CodeIndexerExpression(new CodeVariableReferenceExpression(str3), new CodeExpression[] { new CodeVariableReferenceExpression(str2) }), this.ConvertPropertyToString(strType, beginingExpression)));
                statement.TrueStatements.Add(this.cfls);
                statement.TrueStatements.Add(new CodeAssignStatement(prop, new CodeVariableReferenceExpression(str3)));
                statement.FalseStatements.Add(new CodeAssignStatement(prop, new CodePrimitiveExpression(null)));
                statColl.Add(statement);
            }
        }

        private void AddToDateTimeFunction()
        {
            string name = "dmtfDate";
            string str2 = "year";
            string str3 = "month";
            string str4 = "day";
            string str5 = "hour";
            string str6 = "minute";
            string str7 = "second";
            string str8 = "ticks";
            string str9 = "dmtf";
            string str10 = "tempString";
            string str11 = "datetime";
            CodeCastExpression initExpression = null;
            CodeMemberMethod method = new CodeMemberMethod {
                Name = this.PrivateNamesUsed["ToDateTimeMethod"].ToString(),
                Attributes = MemberAttributes.Static,
                ReturnType = new CodeTypeReference("System.DateTime")
            };
            method.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.String"), name));
            method.Comments.Add(new CodeCommentStatement(GetString("COMMENT_TODATETIME")));
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.DateTime"), "initializer", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.DateTime"), "MinValue")));
            CodeVariableReferenceExpression targetObject = new CodeVariableReferenceExpression("initializer");
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str2, new CodePropertyReferenceExpression(targetObject, "Year")));
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str3, new CodePropertyReferenceExpression(targetObject, "Month")));
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str4, new CodePropertyReferenceExpression(targetObject, "Day")));
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str5, new CodePropertyReferenceExpression(targetObject, "Hour")));
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str6, new CodePropertyReferenceExpression(targetObject, "Minute")));
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str7, new CodePropertyReferenceExpression(targetObject, "Second")));
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int64"), str8, new CodePrimitiveExpression(0)));
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), str9, new CodeVariableReferenceExpression(name)));
            CodeFieldReferenceExpression expression3 = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.DateTime"), "MinValue");
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.DateTime"), str11, expression3));
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), str10, new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.String"), "Empty")));
            CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(str9),
                Right = new CodePrimitiveExpression(null),
                Operator = CodeBinaryOperatorType.IdentityEquality
            };
            CodeConditionStatement statement = new CodeConditionStatement {
                Condition = expression
            };
            CodeObjectCreateExpression toThrow = new CodeObjectCreateExpression {
                CreateType = new CodeTypeReference(this.PublicNamesUsed["ArgumentOutOfRangeException"].ToString())
            };
            statement.TrueStatements.Add(new CodeThrowExceptionStatement(toThrow));
            method.Statements.Add(statement);
            expression = new CodeBinaryOperatorExpression {
                Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str9), "Length"),
                Right = new CodePrimitiveExpression(0),
                Operator = CodeBinaryOperatorType.ValueEquality
            };
            statement = new CodeConditionStatement {
                Condition = expression
            };
            statement.TrueStatements.Add(new CodeThrowExceptionStatement(toThrow));
            method.Statements.Add(statement);
            expression = new CodeBinaryOperatorExpression {
                Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str9), "Length"),
                Right = new CodePrimitiveExpression(0x19),
                Operator = CodeBinaryOperatorType.IdentityInequality
            };
            statement = new CodeConditionStatement {
                Condition = expression
            };
            statement.TrueStatements.Add(new CodeThrowExceptionStatement(toThrow));
            method.Statements.Add(statement);
            CodeTryCatchFinallyStatement statement2 = new CodeTryCatchFinallyStatement();
            DateTimeConversionFunctionHelper(statement2.TryStatements, "****", str10, str9, str2, 0, 4);
            DateTimeConversionFunctionHelper(statement2.TryStatements, "**", str10, str9, str3, 4, 2);
            DateTimeConversionFunctionHelper(statement2.TryStatements, "**", str10, str9, str4, 6, 2);
            DateTimeConversionFunctionHelper(statement2.TryStatements, "**", str10, str9, str5, 8, 2);
            DateTimeConversionFunctionHelper(statement2.TryStatements, "**", str10, str9, str6, 10, 2);
            DateTimeConversionFunctionHelper(statement2.TryStatements, "**", str10, str9, str7, 12, 2);
            CodeMethodReferenceExpression expression6 = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str9), "Substring");
            CodeMethodInvokeExpression right = new CodeMethodInvokeExpression {
                Method = expression6
            };
            right.Parameters.Add(new CodePrimitiveExpression(15));
            right.Parameters.Add(new CodePrimitiveExpression(6));
            statement2.TryStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str10), right));
            expression = new CodeBinaryOperatorExpression {
                Left = new CodePrimitiveExpression("******"),
                Right = new CodeVariableReferenceExpression(str10),
                Operator = CodeBinaryOperatorType.IdentityInequality
            };
            statement = new CodeConditionStatement {
                Condition = expression
            };
            CodeMethodReferenceExpression expression8 = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("System.Int64"), "Parse");
            CodeMethodInvokeExpression expression9 = new CodeMethodInvokeExpression {
                Method = expression8
            };
            expression9.Parameters.Add(new CodeVariableReferenceExpression(str10));
            expression = new CodeBinaryOperatorExpression {
                Left = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "TicksPerMillisecond"),
                Right = new CodePrimitiveExpression(0x3e8),
                Operator = CodeBinaryOperatorType.Divide
            };
            initExpression = new CodeCastExpression("System.Int64", expression);
            CodeBinaryOperatorExpression expression10 = new CodeBinaryOperatorExpression {
                Left = expression9,
                Right = initExpression,
                Operator = CodeBinaryOperatorType.Multiply
            };
            statement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str8), expression10));
            statement2.TryStatements.Add(statement);
            CodeBinaryOperatorExpression expression11 = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(str2),
                Right = new CodePrimitiveExpression(0),
                Operator = CodeBinaryOperatorType.LessThan
            };
            CodeBinaryOperatorExpression expression12 = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(str3),
                Right = new CodePrimitiveExpression(0),
                Operator = CodeBinaryOperatorType.LessThan
            };
            CodeBinaryOperatorExpression expression13 = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(str4),
                Right = new CodePrimitiveExpression(0),
                Operator = CodeBinaryOperatorType.LessThan
            };
            CodeBinaryOperatorExpression expression14 = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(str5),
                Right = new CodePrimitiveExpression(0),
                Operator = CodeBinaryOperatorType.LessThan
            };
            CodeBinaryOperatorExpression expression15 = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(str6),
                Right = new CodePrimitiveExpression(0),
                Operator = CodeBinaryOperatorType.LessThan
            };
            CodeBinaryOperatorExpression expression16 = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(str7),
                Right = new CodePrimitiveExpression(0),
                Operator = CodeBinaryOperatorType.LessThan
            };
            CodeBinaryOperatorExpression expression17 = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(str8),
                Right = new CodePrimitiveExpression(0),
                Operator = CodeBinaryOperatorType.LessThan
            };
            CodeBinaryOperatorExpression expression18 = new CodeBinaryOperatorExpression {
                Left = expression11,
                Right = expression12,
                Operator = CodeBinaryOperatorType.BooleanOr
            };
            CodeBinaryOperatorExpression expression19 = new CodeBinaryOperatorExpression {
                Left = expression18,
                Right = expression13,
                Operator = CodeBinaryOperatorType.BooleanOr
            };
            CodeBinaryOperatorExpression expression20 = new CodeBinaryOperatorExpression {
                Left = expression19,
                Right = expression14,
                Operator = CodeBinaryOperatorType.BooleanOr
            };
            CodeBinaryOperatorExpression expression21 = new CodeBinaryOperatorExpression {
                Left = expression20,
                Right = expression15,
                Operator = CodeBinaryOperatorType.BooleanOr
            };
            CodeBinaryOperatorExpression expression22 = new CodeBinaryOperatorExpression {
                Left = expression21,
                Right = expression15,
                Operator = CodeBinaryOperatorType.BooleanOr
            };
            CodeBinaryOperatorExpression expression23 = new CodeBinaryOperatorExpression {
                Left = expression22,
                Right = expression16,
                Operator = CodeBinaryOperatorType.BooleanOr
            };
            CodeBinaryOperatorExpression expression24 = new CodeBinaryOperatorExpression {
                Left = expression23,
                Right = expression17,
                Operator = CodeBinaryOperatorType.BooleanOr
            };
            statement = new CodeConditionStatement {
                Condition = expression24
            };
            statement.TrueStatements.Add(new CodeThrowExceptionStatement(toThrow));
            statement2.TryStatements.Add(statement);
            string localName = "e";
            CodeCatchClause clause = new CodeCatchClause(localName);
            CodeObjectCreateExpression expression25 = new CodeObjectCreateExpression {
                CreateType = new CodeTypeReference(this.PublicNamesUsed["ArgumentOutOfRangeException"].ToString())
            };
            expression25.Parameters.Add(new CodePrimitiveExpression(null));
            expression25.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(localName), "Message"));
            clause.Statements.Add(new CodeThrowExceptionStatement(expression25));
            statement2.CatchClauses.Add(clause);
            method.Statements.Add(statement2);
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference("System.DateTime");
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(str2));
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(str3));
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(str4));
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(str5));
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(str6));
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(str7));
            this.coce.Parameters.Add(new CodePrimitiveExpression(0));
            method.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str11), this.coce));
            CodeMethodReferenceExpression expression26 = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str11), "AddTicks");
            CodeMethodInvokeExpression expression27 = new CodeMethodInvokeExpression {
                Method = expression26
            };
            expression27.Parameters.Add(new CodeVariableReferenceExpression(str8));
            method.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str11), expression27));
            expression8 = new CodeMethodReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("System.TimeZone"), "CurrentTimeZone"), "GetUtcOffset");
            expression9 = new CodeMethodInvokeExpression {
                Method = expression8
            };
            expression9.Parameters.Add(new CodeVariableReferenceExpression(str11));
            string str13 = "tickOffset";
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.TimeSpan"), str13, expression9));
            string str14 = "UTCOffset";
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str14, new CodePrimitiveExpression(0)));
            string str15 = "OffsetToBeAdjusted";
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str15, new CodePrimitiveExpression(0)));
            string str16 = "OffsetMins";
            expression = new CodeBinaryOperatorExpression {
                Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str13), "Ticks"),
                Right = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "TicksPerMinute"),
                Operator = CodeBinaryOperatorType.Divide
            };
            initExpression = new CodeCastExpression("System.Int64", expression);
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int64"), str16, initExpression));
            expression6 = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str9), "Substring");
            right = new CodeMethodInvokeExpression {
                Method = expression6
            };
            right.Parameters.Add(new CodePrimitiveExpression(0x16));
            right.Parameters.Add(new CodePrimitiveExpression(3));
            method.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str10), right));
            expression = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(str10),
                Right = new CodePrimitiveExpression("******"),
                Operator = CodeBinaryOperatorType.IdentityInequality
            };
            statement = new CodeConditionStatement {
                Condition = expression
            };
            expression6 = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str9), "Substring");
            right = new CodeMethodInvokeExpression {
                Method = expression6
            };
            right.Parameters.Add(new CodePrimitiveExpression(0x15));
            right.Parameters.Add(new CodePrimitiveExpression(4));
            statement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str10), right));
            CodeTryCatchFinallyStatement statement3 = new CodeTryCatchFinallyStatement();
            expression6 = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("System.Int32"), "Parse");
            right = new CodeMethodInvokeExpression {
                Method = expression6
            };
            right.Parameters.Add(new CodeVariableReferenceExpression(str10));
            statement3.TryStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str14), right));
            statement3.CatchClauses.Add(clause);
            statement.TrueStatements.Add(statement3);
            expression = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(str16),
                Right = new CodeVariableReferenceExpression(str14),
                Operator = CodeBinaryOperatorType.Subtract
            };
            statement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str15), new CodeCastExpression(new CodeTypeReference("System.Int32"), expression)));
            expression6 = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str11), "AddMinutes");
            right = new CodeMethodInvokeExpression {
                Method = expression6
            };
            right.Parameters.Add(new CodeCastExpression("System.Double", new CodeVariableReferenceExpression(str15)));
            statement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str11), right));
            method.Statements.Add(statement);
            method.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(str11)));
            this.cc.Members.Add(method);
        }

        private void AddToDMTFDateTimeFunction()
        {
            string name = "utcString";
            string str2 = "date";
            CodeCastExpression initExpression = null;
            CodeMemberMethod cmmdt = new CodeMemberMethod {
                Name = this.PrivateNamesUsed["ToDMTFDateTimeMethod"].ToString(),
                Attributes = MemberAttributes.Static,
                ReturnType = new CodeTypeReference("System.String")
            };
            cmmdt.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.DateTime"), str2));
            cmmdt.Comments.Add(new CodeCommentStatement(GetString("COMMENT_TODMTFDATETIME")));
            cmmdt.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), name, new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.String"), "Empty")));
            CodeMethodReferenceExpression expression2 = new CodeMethodReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("System.TimeZone"), "CurrentTimeZone"), "GetUtcOffset");
            CodeMethodInvokeExpression expression3 = new CodeMethodInvokeExpression {
                Method = expression2
            };
            expression3.Parameters.Add(new CodeVariableReferenceExpression(str2));
            string str3 = "tickOffset";
            cmmdt.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.TimeSpan"), str3, expression3));
            string str4 = "OffsetMins";
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str3), "Ticks");
            this.cboe.Right = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "TicksPerMinute");
            this.cboe.Operator = CodeBinaryOperatorType.Divide;
            initExpression = new CodeCastExpression("System.Int64", this.cboe);
            cmmdt.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int64"), str4, initExpression));
            expression2 = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("System.Math"), "Abs");
            expression3 = new CodeMethodInvokeExpression {
                Method = expression2
            };
            expression3.Parameters.Add(new CodeVariableReferenceExpression(str4));
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = expression3;
            this.cboe.Right = new CodePrimitiveExpression(0x3e7);
            this.cboe.Operator = CodeBinaryOperatorType.GreaterThan;
            CodeConditionStatement statement = new CodeConditionStatement {
                Condition = this.cboe
            };
            expression2 = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str2), "ToUniversalTime");
            expression3 = new CodeMethodInvokeExpression {
                Method = expression2
            };
            statement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str2), expression3));
            statement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(name), new CodePrimitiveExpression("+000")));
            CodeBinaryOperatorExpression expression4 = new CodeBinaryOperatorExpression {
                Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str3), "Ticks"),
                Right = new CodePrimitiveExpression(0),
                Operator = CodeBinaryOperatorType.GreaterThanOrEqual
            };
            CodeConditionStatement statement2 = new CodeConditionStatement {
                Condition = expression4
            };
            CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression {
                Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str3), "Ticks"),
                Right = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "TicksPerMinute"),
                Operator = CodeBinaryOperatorType.Divide
            };
            expression3 = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Int64 "), expression), "ToString")
            };
            CodeMethodInvokeExpression expression6 = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(expression3, "PadLeft")
            };
            expression6.Parameters.Add(new CodePrimitiveExpression(3));
            expression6.Parameters.Add(new CodePrimitiveExpression('0'));
            statement2.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(name), GenerateConcatStrings(new CodePrimitiveExpression("+"), expression6)));
            expression3 = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Int64 "), new CodeVariableReferenceExpression(str4)), "ToString")
            };
            statement2.FalseStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), "strTemp", expression3));
            expression6 = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("strTemp"), "Substring")
            };
            expression6.Parameters.Add(new CodePrimitiveExpression(1));
            expression6.Parameters.Add(new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("strTemp"), "Length"), CodeBinaryOperatorType.Subtract, new CodePrimitiveExpression(1)));
            CodeMethodInvokeExpression expression7 = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(expression6, "PadLeft")
            };
            expression7.Parameters.Add(new CodePrimitiveExpression(3));
            expression7.Parameters.Add(new CodePrimitiveExpression('0'));
            statement2.FalseStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(name), GenerateConcatStrings(new CodePrimitiveExpression("-"), expression7)));
            statement.FalseStatements.Add(statement2);
            cmmdt.Statements.Add(statement);
            string str5 = "dmtfDateTime";
            expression3 = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Int32 "), new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Year")), "ToString")
            };
            expression6 = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(expression3, "PadLeft")
            };
            expression6.Parameters.Add(new CodePrimitiveExpression(4));
            expression6.Parameters.Add(new CodePrimitiveExpression('0'));
            cmmdt.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), str5, expression6));
            this.ToDMTFDateHelper("Month", cmmdt, "System.Int32 ");
            this.ToDMTFDateHelper("Day", cmmdt, "System.Int32 ");
            this.ToDMTFDateHelper("Hour", cmmdt, "System.Int32 ");
            this.ToDMTFDateHelper("Minute", cmmdt, "System.Int32 ");
            this.ToDMTFDateHelper("Second", cmmdt, "System.Int32 ");
            cmmdt.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str5), GenerateConcatStrings(new CodeVariableReferenceExpression(str5), new CodePrimitiveExpression("."))));
            string str6 = "dtTemp";
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference("System.DateTime");
            this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Year"));
            this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Month"));
            this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Day"));
            this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Hour"));
            this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Minute"));
            this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Second"));
            this.coce.Parameters.Add(new CodePrimitiveExpression(0));
            cmmdt.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.DateTime"), str6, this.coce));
            string str7 = "microsec";
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Ticks");
            this.cboe.Right = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str6), "Ticks");
            this.cboe.Operator = CodeBinaryOperatorType.Subtract;
            expression4 = new CodeBinaryOperatorExpression {
                Left = this.cboe,
                Right = new CodePrimitiveExpression(0x3e8),
                Operator = CodeBinaryOperatorType.Multiply
            };
            expression = new CodeBinaryOperatorExpression {
                Left = expression4,
                Right = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "TicksPerMillisecond"),
                Operator = CodeBinaryOperatorType.Divide
            };
            initExpression = new CodeCastExpression("System.Int64", expression);
            cmmdt.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int64"), str7, initExpression));
            string str8 = "strMicrosec";
            expression3 = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Int64 "), new CodeVariableReferenceExpression(str7)), "ToString")
            };
            cmmdt.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), str8, expression3));
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str8), "Length");
            this.cboe.Right = new CodePrimitiveExpression(6);
            this.cboe.Operator = CodeBinaryOperatorType.GreaterThan;
            statement = new CodeConditionStatement {
                Condition = this.cboe
            };
            expression3 = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str8), "Substring")
            };
            expression3.Parameters.Add(new CodePrimitiveExpression(0));
            expression3.Parameters.Add(new CodePrimitiveExpression(6));
            statement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str8), expression3));
            cmmdt.Statements.Add(statement);
            expression3 = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str8), "PadLeft")
            };
            expression3.Parameters.Add(new CodePrimitiveExpression(6));
            expression3.Parameters.Add(new CodePrimitiveExpression('0'));
            expression6 = GenerateConcatStrings(new CodeVariableReferenceExpression(str5), expression3);
            cmmdt.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str5), expression6));
            cmmdt.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str5), GenerateConcatStrings(new CodeVariableReferenceExpression(str5), new CodeVariableReferenceExpression(name))));
            cmmdt.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(str5)));
            this.cc.Members.Add(cmmdt);
        }

        private void AddToDMTFTimeIntervalFunction()
        {
            string name = "dmtftimespan";
            string str2 = "timespan";
            string str3 = "tsTemp";
            string str4 = "microsec";
            string str5 = "strMicroSec";
            CodeMemberMethod method = new CodeMemberMethod {
                Name = this.PrivateNamesUsed["ToDMTFTimeIntervalMethod"].ToString(),
                Attributes = MemberAttributes.Static,
                ReturnType = new CodeTypeReference("System.String")
            };
            method.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.TimeSpan"), str2));
            method.Comments.Add(new CodeCommentStatement(GetString("COMMENT_TODMTFTIMEINTERVAL")));
            CodePropertyReferenceExpression expression = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Days");
            this.cmie = new CodeMethodInvokeExpression();
            this.cmie.Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Int32 "), expression), "ToString");
            CodeMethodInvokeExpression initExpression = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(this.cmie, "PadLeft")
            };
            initExpression.Parameters.Add(new CodePrimitiveExpression(8));
            initExpression.Parameters.Add(new CodePrimitiveExpression('0'));
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), name, initExpression));
            CodeObjectCreateExpression toThrow = new CodeObjectCreateExpression {
                CreateType = new CodeTypeReference(this.PublicNamesUsed["ArgumentOutOfRangeException"].ToString())
            };
            CodeFieldReferenceExpression expression4 = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "MaxValue");
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.TimeSpan"), "maxTimeSpan", expression4));
            CodeBinaryOperatorExpression expression5 = new CodeBinaryOperatorExpression {
                Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Days"),
                Operator = CodeBinaryOperatorType.GreaterThan,
                Right = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("maxTimeSpan"), "Days")
            };
            CodeConditionStatement statement = new CodeConditionStatement {
                Condition = expression5
            };
            statement.TrueStatements.Add(new CodeThrowExceptionStatement(toThrow));
            method.Statements.Add(statement);
            CodeFieldReferenceExpression expression6 = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "MinValue");
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.TimeSpan"), "minTimeSpan", expression6));
            CodeBinaryOperatorExpression expression7 = new CodeBinaryOperatorExpression {
                Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Days"),
                Operator = CodeBinaryOperatorType.LessThan,
                Right = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("minTimeSpan"), "Days")
            };
            CodeConditionStatement statement2 = new CodeConditionStatement {
                Condition = expression7
            };
            statement2.TrueStatements.Add(new CodeThrowExceptionStatement(toThrow));
            method.Statements.Add(statement2);
            expression = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Hours");
            this.cmie = new CodeMethodInvokeExpression();
            this.cmie.Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Int32 "), expression), "ToString");
            initExpression = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(this.cmie, "PadLeft")
            };
            initExpression.Parameters.Add(new CodePrimitiveExpression(2));
            initExpression.Parameters.Add(new CodePrimitiveExpression('0'));
            CodeMethodInvokeExpression right = GenerateConcatStrings(new CodeVariableReferenceExpression(name), initExpression);
            method.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(name), right));
            expression = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Minutes");
            this.cmie = new CodeMethodInvokeExpression();
            this.cmie.Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Int32 "), expression), "ToString");
            initExpression = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(this.cmie, "PadLeft")
            };
            initExpression.Parameters.Add(new CodePrimitiveExpression(2));
            initExpression.Parameters.Add(new CodePrimitiveExpression('0'));
            right = GenerateConcatStrings(new CodeVariableReferenceExpression(name), initExpression);
            method.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(name), right));
            expression = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Seconds");
            this.cmie = new CodeMethodInvokeExpression();
            this.cmie.Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Int32 "), expression), "ToString");
            initExpression = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(this.cmie, "PadLeft")
            };
            initExpression.Parameters.Add(new CodePrimitiveExpression(2));
            initExpression.Parameters.Add(new CodePrimitiveExpression('0'));
            right = GenerateConcatStrings(new CodeVariableReferenceExpression(name), initExpression);
            method.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(name), right));
            right = GenerateConcatStrings(new CodeVariableReferenceExpression(name), new CodePrimitiveExpression("."));
            method.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(name), right));
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference("System.TimeSpan");
            this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Days"));
            this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Hours"));
            this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Minutes"));
            this.coce.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Seconds"));
            this.coce.Parameters.Add(new CodePrimitiveExpression(0));
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.TimeSpan"), str3, this.coce));
            expression5 = new CodeBinaryOperatorExpression {
                Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Ticks"),
                Right = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str3), "Ticks"),
                Operator = CodeBinaryOperatorType.Subtract
            };
            CodeBinaryOperatorExpression expression9 = new CodeBinaryOperatorExpression {
                Left = expression5,
                Right = new CodePrimitiveExpression(0x3e8),
                Operator = CodeBinaryOperatorType.Multiply
            };
            CodeBinaryOperatorExpression expression10 = new CodeBinaryOperatorExpression {
                Left = expression9,
                Right = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "TicksPerMillisecond"),
                Operator = CodeBinaryOperatorType.Divide
            };
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int64"), str4, new CodeCastExpression("System.Int64", expression10)));
            this.cmie = new CodeMethodInvokeExpression();
            this.cmie.Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Int64 "), new CodeVariableReferenceExpression(str4)), "ToString");
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), str5, this.cmie));
            expression5 = new CodeBinaryOperatorExpression {
                Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str5), "Length"),
                Right = new CodePrimitiveExpression(6),
                Operator = CodeBinaryOperatorType.GreaterThan
            };
            statement = new CodeConditionStatement {
                Condition = expression5
            };
            this.cmie = new CodeMethodInvokeExpression();
            this.cmie.Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str5), "Substring");
            this.cmie.Parameters.Add(new CodePrimitiveExpression(0));
            this.cmie.Parameters.Add(new CodePrimitiveExpression(6));
            statement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str5), this.cmie));
            method.Statements.Add(statement);
            this.cmie = new CodeMethodInvokeExpression();
            this.cmie.Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str5), "PadLeft");
            this.cmie.Parameters.Add(new CodePrimitiveExpression(6));
            this.cmie.Parameters.Add(new CodePrimitiveExpression('0'));
            right = GenerateConcatStrings(new CodeVariableReferenceExpression(name), this.cmie);
            method.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(name), right));
            right = GenerateConcatStrings(new CodeVariableReferenceExpression(name), new CodePrimitiveExpression(":000"));
            method.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(name), right));
            method.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(name)));
            this.cc.Members.Add(method);
        }

        private void AddToTimeSpanFunction()
        {
            string name = "dmtfTimespan";
            string str2 = "days";
            string str3 = "hours";
            string str4 = "minutes";
            string str5 = "seconds";
            string str6 = "ticks";
            CodeMemberMethod method = new CodeMemberMethod {
                Name = this.PrivateNamesUsed["ToTimeSpanMethod"].ToString(),
                Attributes = MemberAttributes.Static,
                ReturnType = new CodeTypeReference("System.TimeSpan")
            };
            method.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.String"), name));
            method.Comments.Add(new CodeCommentStatement(GetString("COMMENT_TOTIMESPAN")));
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str2, new CodePrimitiveExpression(0)));
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str3, new CodePrimitiveExpression(0)));
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str4, new CodePrimitiveExpression(0)));
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str5, new CodePrimitiveExpression(0)));
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int64"), str6, new CodePrimitiveExpression(0)));
            CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(name),
                Right = new CodePrimitiveExpression(null),
                Operator = CodeBinaryOperatorType.IdentityEquality
            };
            CodeConditionStatement statement = new CodeConditionStatement {
                Condition = expression
            };
            CodeObjectCreateExpression toThrow = new CodeObjectCreateExpression {
                CreateType = new CodeTypeReference(this.PublicNamesUsed["ArgumentOutOfRangeException"].ToString())
            };
            statement.TrueStatements.Add(new CodeThrowExceptionStatement(toThrow));
            method.Statements.Add(statement);
            expression = new CodeBinaryOperatorExpression {
                Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(name), "Length"),
                Right = new CodePrimitiveExpression(0),
                Operator = CodeBinaryOperatorType.ValueEquality
            };
            statement = new CodeConditionStatement {
                Condition = expression
            };
            statement.TrueStatements.Add(new CodeThrowExceptionStatement(toThrow));
            method.Statements.Add(statement);
            expression = new CodeBinaryOperatorExpression {
                Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(name), "Length"),
                Right = new CodePrimitiveExpression(0x19),
                Operator = CodeBinaryOperatorType.IdentityInequality
            };
            statement = new CodeConditionStatement {
                Condition = expression
            };
            statement.TrueStatements.Add(new CodeThrowExceptionStatement(toThrow));
            method.Statements.Add(statement);
            CodeMethodInvokeExpression right = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(name), "Substring")
            };
            right.Parameters.Add(new CodePrimitiveExpression(0x15));
            right.Parameters.Add(new CodePrimitiveExpression(4));
            expression = new CodeBinaryOperatorExpression {
                Left = right,
                Right = new CodePrimitiveExpression(":000"),
                Operator = CodeBinaryOperatorType.IdentityInequality
            };
            statement = new CodeConditionStatement {
                Condition = expression
            };
            statement.TrueStatements.Add(new CodeThrowExceptionStatement(toThrow));
            method.Statements.Add(statement);
            CodeTryCatchFinallyStatement statement2 = new CodeTryCatchFinallyStatement();
            string str7 = "tempString";
            statement2.TryStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), str7, new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.String"), "Empty")));
            ToTimeSpanHelper(0, 8, str2, statement2.TryStatements);
            ToTimeSpanHelper(8, 2, str3, statement2.TryStatements);
            ToTimeSpanHelper(10, 2, str4, statement2.TryStatements);
            ToTimeSpanHelper(12, 2, str5, statement2.TryStatements);
            right = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(name), "Substring")
            };
            right.Parameters.Add(new CodePrimitiveExpression(15));
            right.Parameters.Add(new CodePrimitiveExpression(6));
            statement2.TryStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str7), right));
            right = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("System.Int64"), "Parse")
            };
            right.Parameters.Add(new CodeVariableReferenceExpression(str7));
            statement2.TryStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str6), new CodeBinaryOperatorExpression(right, CodeBinaryOperatorType.Multiply, new CodeCastExpression("System.Int64", new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "TicksPerMillisecond"), CodeBinaryOperatorType.Divide, new CodePrimitiveExpression(0x3e8))))));
            CodeBinaryOperatorExpression expression4 = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(str2),
                Right = new CodePrimitiveExpression(0),
                Operator = CodeBinaryOperatorType.LessThan
            };
            CodeBinaryOperatorExpression expression5 = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(str3),
                Right = new CodePrimitiveExpression(0),
                Operator = CodeBinaryOperatorType.LessThan
            };
            CodeBinaryOperatorExpression expression6 = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(str4),
                Right = new CodePrimitiveExpression(0),
                Operator = CodeBinaryOperatorType.LessThan
            };
            CodeBinaryOperatorExpression expression7 = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(str5),
                Right = new CodePrimitiveExpression(0),
                Operator = CodeBinaryOperatorType.LessThan
            };
            CodeBinaryOperatorExpression expression8 = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(str6),
                Right = new CodePrimitiveExpression(0),
                Operator = CodeBinaryOperatorType.LessThan
            };
            CodeBinaryOperatorExpression expression9 = new CodeBinaryOperatorExpression {
                Left = expression4,
                Right = expression5,
                Operator = CodeBinaryOperatorType.BooleanOr
            };
            CodeBinaryOperatorExpression expression10 = new CodeBinaryOperatorExpression {
                Left = expression9,
                Right = expression6,
                Operator = CodeBinaryOperatorType.BooleanOr
            };
            CodeBinaryOperatorExpression expression11 = new CodeBinaryOperatorExpression {
                Left = expression10,
                Right = expression7,
                Operator = CodeBinaryOperatorType.BooleanOr
            };
            CodeBinaryOperatorExpression expression12 = new CodeBinaryOperatorExpression {
                Left = expression11,
                Right = expression8,
                Operator = CodeBinaryOperatorType.BooleanOr
            };
            statement = new CodeConditionStatement {
                Condition = expression12
            };
            statement.TrueStatements.Add(new CodeThrowExceptionStatement(toThrow));
            string localName = "e";
            CodeCatchClause clause = new CodeCatchClause(localName);
            CodeObjectCreateExpression expression13 = new CodeObjectCreateExpression {
                CreateType = new CodeTypeReference(this.PublicNamesUsed["ArgumentOutOfRangeException"].ToString())
            };
            expression13.Parameters.Add(new CodePrimitiveExpression(null));
            expression13.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(localName), "Message"));
            clause.Statements.Add(new CodeThrowExceptionStatement(expression13));
            statement2.CatchClauses.Add(clause);
            method.Statements.Add(statement2);
            string str9 = "timespan";
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference("System.TimeSpan");
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(str2));
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(str3));
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(str4));
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(str5));
            this.coce.Parameters.Add(new CodePrimitiveExpression(0));
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.TimeSpan"), str9, this.coce));
            string str10 = "tsTemp";
            right = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("System.TimeSpan"), "FromTicks")
            };
            right.Parameters.Add(new CodeVariableReferenceExpression(str6));
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.TimeSpan"), str10, right));
            right = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str9), "Add")
            };
            right.Parameters.Add(new CodeVariableReferenceExpression(str10));
            method.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str9), right));
            method.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(str9)));
            this.cc.Members.Add(method);
        }

        private void CheckIfClassIsProperlyInitialized()
        {
            if (this.classobj == null)
            {
                if ((this.OriginalNamespace == null) || ((this.OriginalNamespace != null) && (this.OriginalNamespace.Length == 0)))
                {
                    throw new ArgumentOutOfRangeException(GetString("NAMESPACE_NOTINIT_EXCEPT"));
                }
                if ((this.OriginalClassName == null) || ((this.OriginalClassName != null) && (this.OriginalClassName.Length == 0)))
                {
                    throw new ArgumentOutOfRangeException(GetString("CLASSNAME_NOTINIT_EXCEPT"));
                }
            }
        }

        private static int ConvertBitMapValueToInt32(string bitMap)
        {
            string str = "0x";
            if (bitMap.StartsWith(str, StringComparison.Ordinal) || bitMap.StartsWith(str.ToUpper(CultureInfo.InvariantCulture), StringComparison.Ordinal))
            {
                str = string.Empty;
                char[] chArray = bitMap.ToCharArray();
                int length = bitMap.Length;
                for (int i = 2; i < length; i++)
                {
                    str = str + chArray[i];
                }
                return System.Convert.ToInt32(str, (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(int)));
            }
            return System.Convert.ToInt32(bitMap, (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(int)));
        }

        private CodeTypeReference ConvertCIMType(CimType cType, bool isArray)
        {
            string str;
            switch (cType)
            {
                case CimType.SInt16:
                    str = "System.Int16";
                    break;

                case CimType.SInt32:
                    str = "System.Int32";
                    break;

                case CimType.Real32:
                    str = "System.Single";
                    break;

                case CimType.Real64:
                    str = "System.Double";
                    break;

                case CimType.String:
                    str = "System.String";
                    break;

                case CimType.Boolean:
                    str = "System.Boolean";
                    break;

                case CimType.SInt8:
                    str = "System.SByte";
                    break;

                case CimType.UInt8:
                    str = "System.Byte";
                    break;

                case CimType.UInt16:
                    if (this.bUnsignedSupported)
                    {
                        str = "System.UInt16";
                    }
                    else
                    {
                        str = "System.Int16";
                    }
                    break;

                case CimType.UInt32:
                    if (this.bUnsignedSupported)
                    {
                        str = "System.UInt32";
                    }
                    else
                    {
                        str = "System.Int32";
                    }
                    break;

                case CimType.SInt64:
                    str = "System.Int64";
                    break;

                case CimType.UInt64:
                    if (this.bUnsignedSupported)
                    {
                        str = "System.UInt64";
                    }
                    else
                    {
                        str = "System.Int64";
                    }
                    break;

                case CimType.DateTime:
                    str = "System.DateTime";
                    break;

                case CimType.Reference:
                    str = this.PublicNamesUsed["PathClass"].ToString();
                    break;

                case CimType.Char16:
                    str = "System.Char";
                    break;

                default:
                    str = this.PublicNamesUsed["BaseObjClass"].ToString();
                    break;
            }
            if (isArray)
            {
                return new CodeTypeReference(str, 1);
            }
            return new CodeTypeReference(str);
        }

        private CodeExpression ConvertPropertyToString(string strType, CodeExpression beginingExpression)
        {
            switch (strType)
            {
                case "System.DateTime":
                {
                    CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression();
                    expression.Parameters.Add(new CodeCastExpression(new CodeTypeReference("System.DateTime"), beginingExpression));
                    expression.Method.MethodName = this.PrivateNamesUsed["ToDMTFDateTimeMethod"].ToString();
                    return expression;
                }
                case "System.TimeSpan":
                {
                    CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression();
                    expression2.Parameters.Add(new CodeCastExpression(new CodeTypeReference("System.TimeSpan"), beginingExpression));
                    expression2.Method.MethodName = this.PrivateNamesUsed["ToDMTFTimeIntervalMethod"].ToString();
                    return expression2;
                }
                case "System.Management.ManagementPath":
                    return new CodePropertyReferenceExpression(new CodeCastExpression(new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()), beginingExpression), this.PublicNamesUsed["PathProperty"].ToString());
            }
            return null;
        }

        private static string ConvertToNumericValueAndAddToArray(CimType cimType, string numericValue, ArrayList arrayToAdd, out string enumType)
        {
            string str = string.Empty;
            enumType = string.Empty;
            switch (cimType)
            {
                case CimType.SInt16:
                case CimType.SInt32:
                case CimType.SInt8:
                case CimType.UInt8:
                case CimType.UInt16:
                    arrayToAdd.Add(System.Convert.ToInt32(numericValue, (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(int))));
                    str = "ToInt32";
                    enumType = "System.Int32";
                    return str;

                case CimType.UInt32:
                    arrayToAdd.Add(System.Convert.ToInt32(numericValue, (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(int))));
                    str = "ToInt32";
                    enumType = "System.Int32";
                    return str;
            }
            return str;
        }

        private static string ConvertValuesToName(string str)
        {
            string str2 = string.Empty;
            string str3 = "_";
            string str4 = string.Empty;
            bool flag = true;
            if (str.Length == 0)
            {
                return string.Copy("");
            }
            char[] chArray = str.ToCharArray();
            if (!char.IsLetter(chArray[0]))
            {
                str2 = "Val_";
                str4 = "l";
            }
            for (int i = 0; i < str.Length; i++)
            {
                flag = true;
                if (!char.IsLetterOrDigit(chArray[i]))
                {
                    if (str4 == str3)
                    {
                        flag = false;
                    }
                    else
                    {
                        str4 = str3;
                    }
                }
                else
                {
                    str4 = new string(chArray[i], 1);
                }
                if (flag)
                {
                    str2 = str2 + str4;
                }
            }
            return str2;
        }

        private CodeExpression CreateObjectForProperty(string strType, CodeExpression param)
        {
            switch (strType)
            {
                case "System.DateTime":
                    if (param == null)
                    {
                        return new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.DateTime"), "MinValue");
                    }
                    this.cmie = new CodeMethodInvokeExpression();
                    this.cmie.Parameters.Add(param);
                    this.cmie.Method.MethodName = this.PrivateNamesUsed["ToDateTimeMethod"].ToString();
                    return this.cmie;

                case "System.TimeSpan":
                    if (param == null)
                    {
                        this.coce = new CodeObjectCreateExpression();
                        this.coce.CreateType = new CodeTypeReference("System.TimeSpan");
                        this.coce.Parameters.Add(new CodePrimitiveExpression(0));
                        this.coce.Parameters.Add(new CodePrimitiveExpression(0));
                        this.coce.Parameters.Add(new CodePrimitiveExpression(0));
                        this.coce.Parameters.Add(new CodePrimitiveExpression(0));
                        this.coce.Parameters.Add(new CodePrimitiveExpression(0));
                        return this.coce;
                    }
                    this.cmie = new CodeMethodInvokeExpression();
                    this.cmie.Parameters.Add(param);
                    this.cmie.Method.MethodName = this.PrivateNamesUsed["ToTimeSpanMethod"].ToString();
                    return this.cmie;

                case "System.Management.ManagementPath":
                    this.coce = new CodeObjectCreateExpression();
                    this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
                    this.coce.Parameters.Add(param);
                    return this.coce;
            }
            return null;
        }

        private static void DateTimeConversionFunctionHelper(CodeStatementCollection cmmdt, string toCompare, string tempVarName, string dmtfVarName, string toAssign, int SubStringParam1, int SubStringParam2)
        {
            CodeMethodReferenceExpression expression = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(dmtfVarName), "Substring");
            CodeMethodInvokeExpression right = new CodeMethodInvokeExpression {
                Method = expression
            };
            right.Parameters.Add(new CodePrimitiveExpression(SubStringParam1));
            right.Parameters.Add(new CodePrimitiveExpression(SubStringParam2));
            cmmdt.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(tempVarName), right));
            CodeBinaryOperatorExpression expression3 = new CodeBinaryOperatorExpression {
                Left = new CodePrimitiveExpression(toCompare),
                Right = new CodeVariableReferenceExpression(tempVarName),
                Operator = CodeBinaryOperatorType.IdentityInequality
            };
            CodeConditionStatement statement = new CodeConditionStatement {
                Condition = expression3
            };
            expression = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("System.Int32"), "Parse");
            right = new CodeMethodInvokeExpression {
                Method = expression
            };
            right.Parameters.Add(new CodeVariableReferenceExpression(tempVarName));
            statement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(toAssign), right));
            cmmdt.Add(statement);
        }

        private void GenarateConstructorWithLateBound()
        {
            string variableName = "theObject";
            string propertyName = "SystemProperties";
            this.cctor = new CodeConstructor();
            this.cctor.Attributes = MemberAttributes.Public;
            this.cpde = new CodeParameterDeclarationExpression();
            this.cpde.Type = new CodeTypeReference(this.PublicNamesUsed["LateBoundClass"].ToString());
            this.cpde.Name = variableName;
            this.cctor.Parameters.Add(this.cpde);
            this.InitPrivateMemberVariables(this.cctor);
            this.cis = new CodeConditionStatement();
            this.cpre = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(variableName), propertyName);
            this.cie = new CodeIndexerExpression(this.cpre, new CodeExpression[] { new CodePrimitiveExpression("__CLASS") });
            this.cpre = new CodePropertyReferenceExpression(this.cie, "Value");
            this.cmie = new CodeMethodInvokeExpression();
            this.cmie.Method.MethodName = this.PrivateNamesUsed["ClassNameCheckFunc"].ToString();
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(variableName));
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = this.cmie;
            this.cboe.Right = new CodePrimitiveExpression(true);
            this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
            this.cis.Condition = this.cboe;
            this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()), new CodeVariableReferenceExpression(variableName)));
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["SystemPropertiesClass"].ToString());
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()));
            this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["SystemPropertiesObject"].ToString()), this.coce));
            this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["CurrentObject"].ToString()), new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString())));
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ArgumentExceptionClass"].ToString());
            this.coce.Parameters.Add(new CodePrimitiveExpression(GetString("CLASSNOT_FOUND_EXCEPT")));
            this.cis.FalseStatements.Add(new CodeThrowExceptionStatement(this.coce));
            this.cctor.Statements.Add(this.cis);
            this.cc.Members.Add(this.cctor);
        }

        private void GenarateConstructorWithLateBoundForEmbedded()
        {
            string variableName = "theObject";
            this.cctor = new CodeConstructor();
            this.cctor.Attributes = MemberAttributes.Public;
            this.cpde = new CodeParameterDeclarationExpression();
            this.cpde.Type = new CodeTypeReference(this.PublicNamesUsed["BaseObjClass"].ToString());
            this.cpde.Name = variableName;
            this.cctor.Parameters.Add(this.cpde);
            this.InitPrivateMemberVariables(this.cctor);
            this.cmie = new CodeMethodInvokeExpression();
            this.cmie.Method.MethodName = this.PrivateNamesUsed["ClassNameCheckFunc"].ToString();
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(variableName));
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = this.cmie;
            this.cboe.Right = new CodePrimitiveExpression(true);
            this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
            this.cis = new CodeConditionStatement();
            this.cis.Condition = this.cboe;
            this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["EmbeddedObject"].ToString()), new CodeVariableReferenceExpression(variableName)));
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["SystemPropertiesClass"].ToString());
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(variableName));
            this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["SystemPropertiesObject"].ToString()), this.coce));
            this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["CurrentObject"].ToString()), new CodeVariableReferenceExpression(this.PrivateNamesUsed["EmbeddedObject"].ToString())));
            this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString()), new CodePrimitiveExpression(true)));
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ArgumentExceptionClass"].ToString());
            this.coce.Parameters.Add(new CodePrimitiveExpression(GetString("CLASSNOT_FOUND_EXCEPT")));
            this.cis.FalseStatements.Add(new CodeThrowExceptionStatement(this.coce));
            this.cctor.Statements.Add(this.cis);
            this.cc.Members.Add(this.cctor);
        }

        private bool GenerateAndWriteCode(CodeLanguage lang)
        {
            if (!this.InitializeCodeGenerator(lang))
            {
                return false;
            }
            this.InitializeCodeTypeDeclaration(lang);
            this.GetCodeTypeDeclarationForClass(true);
            this.cc.Name = this.cp.CreateValidIdentifier(this.cc.Name);
            this.cn.Types.Add(this.cc);
            try
            {
                this.cp.GenerateCodeFromNamespace(this.cn, this.tw, new CodeGeneratorOptions());
            }
            finally
            {
                this.tw.Close();
            }
            return true;
        }

        private void GenerateClassNameProperty()
        {
            string name = "strRet";
            this.cmp = new CodeMemberProperty();
            this.cmp.Name = this.PublicNamesUsed["ClassNameProperty"].ToString();
            this.cmp.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            this.cmp.Type = new CodeTypeReference("System.String");
            this.caa = new CodeAttributeArgument();
            this.caa.Value = new CodePrimitiveExpression(true);
            this.cad = new CodeAttributeDeclaration();
            this.cad.Name = "Browsable";
            this.cad.Arguments.Add(this.caa);
            this.cmp.CustomAttributes = new CodeAttributeDeclarationCollection();
            this.cmp.CustomAttributes.Add(this.cad);
            this.caa = new CodeAttributeArgument();
            this.caa.Value = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("DesignerSerializationVisibility"), "Hidden");
            this.cad = new CodeAttributeDeclaration();
            this.cad.Name = "DesignerSerializationVisibility";
            this.cad.Arguments.Add(this.caa);
            this.cmp.CustomAttributes.Add(this.cad);
            this.cmp.GetStatements.Add(new CodeVariableDeclarationStatement("System.String", name, new CodeVariableReferenceExpression(this.PrivateNamesUsed["CreationClassName"].ToString())));
            this.cis = new CodeConditionStatement();
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["CurrentObject"].ToString());
            this.cboe.Right = new CodePrimitiveExpression(null);
            this.cboe.Operator = CodeBinaryOperatorType.IdentityInequality;
            this.cis.Condition = this.cboe;
            CodeConditionStatement statement = new CodeConditionStatement();
            CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression {
                Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["CurrentObject"].ToString()), this.PublicNamesUsed["ClassPathProperty"].ToString()),
                Right = new CodePrimitiveExpression(null),
                Operator = CodeBinaryOperatorType.IdentityInequality
            };
            statement.Condition = expression;
            this.cis.TrueStatements.Add(statement);
            statement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(name), new CodeCastExpression(new CodeTypeReference("System.String"), new CodeIndexerExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["CurrentObject"].ToString()), new CodeExpression[] { new CodePrimitiveExpression("__CLASS") }))));
            CodeConditionStatement statement2 = new CodeConditionStatement();
            CodeBinaryOperatorExpression expression2 = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(name),
                Right = new CodePrimitiveExpression(null),
                Operator = CodeBinaryOperatorType.IdentityEquality
            };
            CodeBinaryOperatorExpression expression3 = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(name),
                Right = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.String"), "Empty"),
                Operator = CodeBinaryOperatorType.IdentityEquality
            };
            CodeBinaryOperatorExpression expression4 = new CodeBinaryOperatorExpression {
                Left = expression2,
                Right = expression3,
                Operator = CodeBinaryOperatorType.BooleanOr
            };
            statement2.Condition = expression4;
            statement2.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(name), new CodeVariableReferenceExpression(this.PrivateNamesUsed["CreationClassName"].ToString())));
            statement.TrueStatements.Add(statement2);
            this.cmp.GetStatements.Add(this.cis);
            this.cmp.GetStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(name)));
            this.cc.Members.Add(this.cmp);
        }

        public CodeTypeDeclaration GenerateCode(bool includeSystemProperties, bool systemPropertyClass)
        {
            if (systemPropertyClass)
            {
                this.InitilializePublicPrivateMembers();
                return this.GenerateSystemPropertiesClass();
            }
            this.CheckIfClassIsProperlyInitialized();
            this.InitializeCodeGeneration();
            return this.GetCodeTypeDeclarationForClass(includeSystemProperties);
        }

        public bool GenerateCode(CodeLanguage lang, string filePath, string netNamespace)
        {
            if (filePath == null)
            {
                throw new ArgumentOutOfRangeException(GetString("NULLFILEPATH_EXCEPT"));
            }
            if (filePath.Length == 0)
            {
                throw new ArgumentOutOfRangeException(GetString("EMPTY_FILEPATH_EXCEPT"));
            }
            this.NETNamespace = netNamespace;
            this.CheckIfClassIsProperlyInitialized();
            this.InitializeCodeGeneration();
            this.tw = new StreamWriter(new FileStream(filePath, FileMode.Create), Encoding.UTF8);
            return this.GenerateAndWriteCode(lang);
        }

        private void GenerateCodeForRefAndDateTimeTypes(CodeIndexerExpression prop, bool bArray, CodeStatementCollection statColl, string strType, CodeVariableReferenceExpression varToAssign, bool bIsValueProprequired)
        {
            if (!bArray)
            {
                CodeConditionStatement statement = new CodeConditionStatement();
                CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression {
                    Left = prop,
                    Operator = CodeBinaryOperatorType.IdentityInequality,
                    Right = new CodePrimitiveExpression(null)
                };
                statement.Condition = expression;
                if (string.Compare(strType, this.PublicNamesUsed["PathClass"].ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    CodeMethodReferenceExpression expression2 = new CodeMethodReferenceExpression {
                        MethodName = "ToString",
                        TargetObject = prop
                    };
                    this.cmie = new CodeMethodInvokeExpression();
                    this.cmie.Method = expression2;
                    if (varToAssign == null)
                    {
                        statement.TrueStatements.Add(new CodeMethodReturnStatement(this.CreateObjectForProperty(strType, this.cmie)));
                        statColl.Add(statement);
                        statColl.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
                    }
                    else
                    {
                        statColl.Add(new CodeAssignStatement(varToAssign, new CodePrimitiveExpression(null)));
                        statement.TrueStatements.Add(new CodeAssignStatement(varToAssign, this.CreateObjectForProperty(strType, this.cmie)));
                        statColl.Add(statement);
                    }
                }
                else
                {
                    statColl.Add(statement);
                    CodeExpression param = null;
                    if (bIsValueProprequired)
                    {
                        param = new CodeCastExpression(new CodeTypeReference("System.String"), new CodePropertyReferenceExpression(prop, "Value"));
                    }
                    else
                    {
                        param = new CodeCastExpression(new CodeTypeReference("System.String"), prop);
                    }
                    if (varToAssign == null)
                    {
                        statement.TrueStatements.Add(new CodeMethodReturnStatement(this.CreateObjectForProperty(strType, param)));
                        statement.FalseStatements.Add(new CodeMethodReturnStatement(this.CreateObjectForProperty(strType, null)));
                    }
                    else
                    {
                        statement.TrueStatements.Add(new CodeAssignStatement(varToAssign, this.CreateObjectForProperty(strType, param)));
                        statement.FalseStatements.Add(new CodeAssignStatement(varToAssign, this.CreateObjectForProperty(strType, null)));
                    }
                }
            }
            else
            {
                string name = "len";
                string str2 = "iCounter";
                string str3 = "arrToRet";
                CodeConditionStatement statement2 = new CodeConditionStatement();
                CodeBinaryOperatorExpression expression4 = new CodeBinaryOperatorExpression {
                    Left = prop,
                    Operator = CodeBinaryOperatorType.IdentityInequality,
                    Right = new CodePrimitiveExpression(null)
                };
                statement2.Condition = expression4;
                CodePropertyReferenceExpression initExpression = null;
                if (bIsValueProprequired)
                {
                    initExpression = new CodePropertyReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Array"), new CodePropertyReferenceExpression(prop, "Value")), "Length");
                }
                else
                {
                    initExpression = new CodePropertyReferenceExpression(new CodeCastExpression(new CodeTypeReference("System.Array"), prop), "Length");
                }
                statement2.TrueStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), name, initExpression));
                CodeTypeReference type = new CodeTypeReference(new CodeTypeReference(strType), 1);
                statement2.TrueStatements.Add(new CodeVariableDeclarationStatement(type, str3, new CodeArrayCreateExpression(new CodeTypeReference(strType), new CodeVariableReferenceExpression(name))));
                this.cfls = new CodeIterationStatement();
                this.cfls.InitStatement = new CodeVariableDeclarationStatement(new CodeTypeReference("System.Int32"), str2, new CodePrimitiveExpression(0));
                expression4 = new CodeBinaryOperatorExpression {
                    Left = new CodeVariableReferenceExpression(str2),
                    Operator = CodeBinaryOperatorType.LessThan,
                    Right = new CodeVariableReferenceExpression(name)
                };
                this.cfls.TestExpression = expression4;
                this.cfls.IncrementStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(str2), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(str2), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1)));
                CodeMethodInvokeExpression expression6 = new CodeMethodInvokeExpression {
                    Method = { MethodName = "GetValue" }
                };
                if (bIsValueProprequired)
                {
                    expression6.Method.TargetObject = new CodeCastExpression(new CodeTypeReference("System.Array"), new CodePropertyReferenceExpression(prop, "Value"));
                }
                else
                {
                    expression6.Method.TargetObject = new CodeCastExpression(new CodeTypeReference("System.Array"), prop);
                }
                expression6.Parameters.Add(new CodeVariableReferenceExpression(str2));
                CodeMethodInvokeExpression expression7 = new CodeMethodInvokeExpression {
                    Method = { MethodName = "ToString", TargetObject = expression6 }
                };
                this.cfls.Statements.Add(new CodeAssignStatement(new CodeIndexerExpression(new CodeVariableReferenceExpression(str3), new CodeExpression[] { new CodeVariableReferenceExpression(str2) }), this.CreateObjectForProperty(strType, expression7)));
                statement2.TrueStatements.Add(this.cfls);
                if (varToAssign == null)
                {
                    statement2.TrueStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(str3)));
                    statColl.Add(statement2);
                    statColl.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
                }
                else
                {
                    statColl.Add(new CodeAssignStatement(varToAssign, new CodePrimitiveExpression(null)));
                    statement2.TrueStatements.Add(new CodeAssignStatement(varToAssign, new CodeVariableReferenceExpression(str3)));
                    statColl.Add(statement2);
                }
            }
        }

        private void GenerateCollectionClass()
        {
            string typeName = "ManagementObjectCollection";
            string variableName = "privColObj";
            string str3 = "objCollection";
            this.ccc = new CodeTypeDeclaration(this.PrivateNamesUsed["CollectionClass"].ToString());
            this.ccc.BaseTypes.Add("System.Object");
            this.ccc.BaseTypes.Add("ICollection");
            this.ccc.TypeAttributes = TypeAttributes.NestedPublic;
            this.cf = new CodeMemberField();
            this.cf.Name = variableName;
            this.cf.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            this.cf.Type = new CodeTypeReference(typeName);
            this.ccc.Members.Add(this.cf);
            this.cctor = new CodeConstructor();
            this.cctor.Attributes = MemberAttributes.Public;
            this.cpde = new CodeParameterDeclarationExpression();
            this.cpde.Name = str3;
            this.cpde.Type = new CodeTypeReference(typeName);
            this.cctor.Parameters.Add(this.cpde);
            this.cctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(variableName), new CodeVariableReferenceExpression(str3)));
            this.ccc.Members.Add(this.cctor);
            this.cmp = new CodeMemberProperty();
            this.cmp.Type = new CodeTypeReference("System.Int32");
            this.cmp.Attributes = MemberAttributes.Public | MemberAttributes.Override | MemberAttributes.Final;
            this.cmp.Name = "Count";
            this.cmp.ImplementationTypes.Add("System.Collections.ICollection");
            this.cmp.GetStatements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(variableName), "Count")));
            this.ccc.Members.Add(this.cmp);
            this.cmp = new CodeMemberProperty();
            this.cmp.Type = new CodeTypeReference("System.Boolean");
            this.cmp.Attributes = MemberAttributes.Public | MemberAttributes.Override | MemberAttributes.Final;
            this.cmp.Name = "IsSynchronized";
            this.cmp.ImplementationTypes.Add("System.Collections.ICollection");
            this.cmp.GetStatements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(variableName), "IsSynchronized")));
            this.ccc.Members.Add(this.cmp);
            this.cmp = new CodeMemberProperty();
            this.cmp.Type = new CodeTypeReference("System.Object");
            this.cmp.Attributes = MemberAttributes.Public | MemberAttributes.Override | MemberAttributes.Final;
            this.cmp.Name = "SyncRoot";
            this.cmp.ImplementationTypes.Add("System.Collections.ICollection");
            this.cmp.GetStatements.Add(new CodeMethodReturnStatement(new CodeThisReferenceExpression()));
            this.ccc.Members.Add(this.cmp);
            string str4 = "array";
            string str5 = "index";
            string name = "nCtr";
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Override | MemberAttributes.Final;
            this.cmm.Name = "CopyTo";
            this.cmm.ImplementationTypes.Add("System.Collections.ICollection");
            this.cpde = new CodeParameterDeclarationExpression();
            this.cpde.Name = str4;
            this.cpde.Type = new CodeTypeReference("System.Array");
            this.cmm.Parameters.Add(this.cpde);
            this.cpde = new CodeParameterDeclarationExpression();
            this.cpde.Name = str5;
            this.cpde.Type = new CodeTypeReference("System.Int32");
            this.cmm.Parameters.Add(this.cpde);
            this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(variableName), "CopyTo", new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str4));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str5));
            this.cmm.Statements.Add(new CodeExpressionStatement(this.cmie));
            this.cmm.Statements.Add(new CodeVariableDeclarationStatement("System.Int32", name));
            this.cfls = new CodeIterationStatement();
            this.cfls.InitStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(name), new CodePrimitiveExpression(0));
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = new CodeVariableReferenceExpression(name);
            this.cboe.Operator = CodeBinaryOperatorType.LessThan;
            this.cboe.Right = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str4), "Length");
            this.cfls.TestExpression = this.cboe;
            this.cfls.IncrementStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(name), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(name), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1)));
            this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str4), "SetValue", new CodeExpression[0]);
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str4), "GetValue", new CodeExpression[] { new CodeVariableReferenceExpression(name) });
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PrivateNamesUsed["GeneratedClassName"].ToString());
            this.coce.Parameters.Add(new CodeCastExpression(new CodeTypeReference(this.PublicNamesUsed["LateBoundClass"].ToString()), expression));
            this.cmie.Parameters.Add(this.coce);
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cfls.Statements.Add(new CodeExpressionStatement(this.cmie));
            this.cmm.Statements.Add(this.cfls);
            this.ccc.Members.Add(this.cmm);
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Override | MemberAttributes.Final;
            this.cmm.Name = "GetEnumerator";
            this.cmm.ImplementationTypes.Add("System.Collections.IEnumerable");
            this.cmm.ReturnType = new CodeTypeReference("System.Collections.IEnumerator");
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PrivateNamesUsed["EnumeratorClass"].ToString());
            this.coce.Parameters.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(variableName), "GetEnumerator", new CodeExpression[0]));
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.coce));
            this.ccc.Members.Add(this.cmm);
            this.GenerateEnumeratorClass();
            this.ccc.Comments.Add(new CodeCommentStatement(GetString("COMMENT_ENUMIMPL")));
            this.cc.Members.Add(this.ccc);
        }

        private void GenerateCommitMethod()
        {
            this.cmm = new CodeMemberMethod();
            this.cmm.Name = this.PublicNamesUsed["CommitMethod"].ToString();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            this.caa = new CodeAttributeArgument();
            this.caa.Value = new CodePrimitiveExpression(true);
            this.cad = new CodeAttributeDeclaration();
            this.cad.Name = "Browsable";
            this.cad.Arguments.Add(this.caa);
            this.cmm.CustomAttributes = new CodeAttributeDeclarationCollection();
            this.cmm.CustomAttributes.Add(this.cad);
            this.cis = new CodeConditionStatement();
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString());
            this.cboe.Right = new CodePrimitiveExpression(false);
            this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
            this.cis.Condition = this.cboe;
            this.cmie = new CodeMethodInvokeExpression();
            this.cmie.Method.TargetObject = new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString());
            this.cmie.Method.MethodName = "Put";
            this.cis.TrueStatements.Add(new CodeExpressionStatement(this.cmie));
            this.cmm.Statements.Add(this.cis);
            this.cc.Members.Add(this.cmm);
            this.cmm = new CodeMemberMethod();
            this.cmm.Name = this.PublicNamesUsed["CommitMethod"].ToString();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            CodeParameterDeclarationExpression expression = new CodeParameterDeclarationExpression {
                Type = new CodeTypeReference(this.PublicNamesUsed["PutOptions"].ToString()),
                Name = this.PrivateNamesUsed["putOptions"].ToString()
            };
            this.cmm.Parameters.Add(expression);
            this.caa = new CodeAttributeArgument();
            this.caa.Value = new CodePrimitiveExpression(true);
            this.cad = new CodeAttributeDeclaration();
            this.cad.Name = "Browsable";
            this.cad.Arguments.Add(this.caa);
            this.cmm.CustomAttributes = new CodeAttributeDeclarationCollection();
            this.cmm.CustomAttributes.Add(this.cad);
            this.cis = new CodeConditionStatement();
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString());
            this.cboe.Right = new CodePrimitiveExpression(false);
            this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
            this.cis.Condition = this.cboe;
            this.cmie = new CodeMethodInvokeExpression();
            this.cmie.Method.TargetObject = new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString());
            this.cmie.Method.MethodName = "Put";
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["putOptions"].ToString()));
            this.cis.TrueStatements.Add(new CodeExpressionStatement(this.cmie));
            this.cmm.Statements.Add(this.cis);
            this.cc.Members.Add(this.cmm);
        }

        private static CodeMethodInvokeExpression GenerateConcatStrings(CodeExpression ce1, CodeExpression ce2)
        {
            return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("System.String"), "Concat", new CodeExpression[] { ce1, ce2 });
        }

        private void GenerateConstructorWithKeys()
        {
            if (this.arrKeyType.Count > 0)
            {
                this.cctor = new CodeConstructor();
                this.cctor.Attributes = MemberAttributes.Public;
                CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                    Method = { MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString(), TargetObject = new CodeThisReferenceExpression() }
                };
                for (int i = 0; i < this.arrKeys.Count; i++)
                {
                    this.cpde = new CodeParameterDeclarationExpression();
                    this.cpde.Type = new CodeTypeReference(((CodeTypeReference) this.arrKeyType[i]).BaseType);
                    this.cpde.Name = "key" + this.arrKeys[i].ToString();
                    this.cctor.Parameters.Add(this.cpde);
                }
                if ((this.cctor.Parameters.Count == 1) && (this.cctor.Parameters[0].Type.BaseType == new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()).BaseType))
                {
                    this.cpde = new CodeParameterDeclarationExpression();
                    this.cpde.Type = new CodeTypeReference("System.Object");
                    this.cpde.Name = "dummyParam";
                    this.cctor.Parameters.Add(this.cpde);
                    this.cctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("dummyParam"), new CodePrimitiveExpression(null)));
                }
                expression.Parameters.Add(new CodePrimitiveExpression(null));
                this.cmie = new CodeMethodInvokeExpression();
                this.cmie.Method.TargetObject = new CodeTypeReferenceExpression(this.PrivateNamesUsed["GeneratedClassName"].ToString());
                this.cmie.Method.MethodName = this.PublicNamesUsed["ConstructPathFunction"].ToString();
                for (int j = 0; j < this.arrKeys.Count; j++)
                {
                    this.cmie.Parameters.Add(new CodeVariableReferenceExpression("key" + this.arrKeys[j]));
                }
                this.coce = new CodeObjectCreateExpression();
                this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
                this.coce.Parameters.Add(this.cmie);
                expression.Parameters.Add(this.coce);
                expression.Parameters.Add(new CodePrimitiveExpression(null));
                this.cctor.Statements.Add(new CodeExpressionStatement(expression));
                this.cc.Members.Add(this.cctor);
            }
        }

        private void GenerateConstructorWithOptions()
        {
            string name = "getOptions";
            this.cctor = new CodeConstructor();
            this.cctor.Attributes = MemberAttributes.Public;
            this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["GetOptionsClass"].ToString()), name));
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString(), TargetObject = new CodeThisReferenceExpression() }
            };
            expression.Parameters.Add(new CodePrimitiveExpression(null));
            this.cmie = new CodeMethodInvokeExpression();
            this.cmie.Method.TargetObject = new CodeTypeReferenceExpression(this.PrivateNamesUsed["GeneratedClassName"].ToString());
            this.cmie.Method.MethodName = this.PublicNamesUsed["ConstructPathFunction"].ToString();
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
            this.coce.Parameters.Add(this.cmie);
            expression.Parameters.Add(this.coce);
            expression.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cctor.Statements.Add(new CodeExpressionStatement(expression));
            this.cc.Members.Add(this.cctor);
        }

        private void GenerateConstructorWithPath()
        {
            string variableName = "path";
            this.cctor = new CodeConstructor();
            this.cctor.Attributes = MemberAttributes.Public;
            this.cpde = new CodeParameterDeclarationExpression();
            this.cpde.Type = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
            this.cpde.Name = variableName;
            this.cctor.Parameters.Add(this.cpde);
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString(), TargetObject = new CodeThisReferenceExpression() }
            };
            expression.Parameters.Add(new CodePrimitiveExpression(null));
            expression.Parameters.Add(new CodeVariableReferenceExpression(variableName));
            expression.Parameters.Add(new CodePrimitiveExpression(null));
            this.cctor.Statements.Add(new CodeExpressionStatement(expression));
            this.cc.Members.Add(this.cctor);
        }

        private void GenerateConstructorWithPathOptions()
        {
            string name = "path";
            string str2 = "getOptions";
            this.cctor = new CodeConstructor();
            this.cctor.Attributes = MemberAttributes.Public;
            this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()), name));
            this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["GetOptionsClass"].ToString()), str2));
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString(), TargetObject = new CodeThisReferenceExpression() }
            };
            expression.Parameters.Add(new CodePrimitiveExpression(null));
            expression.Parameters.Add(new CodeVariableReferenceExpression(name));
            expression.Parameters.Add(new CodeVariableReferenceExpression(str2));
            this.cctor.Statements.Add(new CodeExpressionStatement(expression));
            this.cc.Members.Add(this.cctor);
        }

        private void GenerateConstructorWithScope()
        {
            this.cctor = new CodeConstructor();
            this.cctor.Attributes = MemberAttributes.Public;
            this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString(), TargetObject = new CodeThisReferenceExpression() }
            };
            expression.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
            this.cmie = new CodeMethodInvokeExpression();
            this.cmie.Method.TargetObject = new CodeTypeReferenceExpression(this.PrivateNamesUsed["GeneratedClassName"].ToString());
            this.cmie.Method.MethodName = this.PublicNamesUsed["ConstructPathFunction"].ToString();
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
            this.coce.Parameters.Add(this.cmie);
            expression.Parameters.Add(this.coce);
            expression.Parameters.Add(new CodePrimitiveExpression(null));
            this.cctor.Statements.Add(new CodeExpressionStatement(expression));
            this.cc.Members.Add(this.cctor);
        }

        private void GenerateConstructorWithScopeKeys()
        {
            this.cctor = new CodeConstructor();
            this.cctor.Attributes = MemberAttributes.Public;
            this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString(), TargetObject = new CodeThisReferenceExpression() }
            };
            if (this.arrKeyType.Count > 0)
            {
                for (int i = 0; i < this.arrKeys.Count; i++)
                {
                    this.cpde = new CodeParameterDeclarationExpression();
                    this.cpde.Type = new CodeTypeReference(((CodeTypeReference) this.arrKeyType[i]).BaseType);
                    this.cpde.Name = "key" + this.arrKeys[i].ToString();
                    this.cctor.Parameters.Add(this.cpde);
                }
                if ((this.cctor.Parameters.Count == 2) && (this.cctor.Parameters[1].Type.BaseType == new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()).BaseType))
                {
                    this.cpde = new CodeParameterDeclarationExpression();
                    this.cpde.Type = new CodeTypeReference("System.Object");
                    this.cpde.Name = "dummyParam";
                    this.cctor.Parameters.Add(this.cpde);
                    this.cctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("dummyParam"), new CodePrimitiveExpression(null)));
                }
                expression.Parameters.Add(new CodeCastExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString())));
                this.cmie = new CodeMethodInvokeExpression();
                this.cmie.Method.TargetObject = new CodeTypeReferenceExpression(this.PrivateNamesUsed["GeneratedClassName"].ToString());
                this.cmie.Method.MethodName = this.PublicNamesUsed["ConstructPathFunction"].ToString();
                for (int j = 0; j < this.arrKeys.Count; j++)
                {
                    this.cmie.Parameters.Add(new CodeVariableReferenceExpression("key" + this.arrKeys[j]));
                }
                this.coce = new CodeObjectCreateExpression();
                this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
                this.coce.Parameters.Add(this.cmie);
                expression.Parameters.Add(this.coce);
                expression.Parameters.Add(new CodePrimitiveExpression(null));
                this.cctor.Statements.Add(new CodeExpressionStatement(expression));
                this.cc.Members.Add(this.cctor);
            }
        }

        private void GenerateConstructorWithScopeOptions()
        {
            string name = "getOptions";
            this.cctor = new CodeConstructor();
            this.cctor.Attributes = MemberAttributes.Public;
            this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
            this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["GetOptionsClass"].ToString()), name));
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString(), TargetObject = new CodeThisReferenceExpression() }
            };
            expression.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
            this.cmie = new CodeMethodInvokeExpression();
            this.cmie.Method.TargetObject = new CodeTypeReferenceExpression(this.PrivateNamesUsed["GeneratedClassName"].ToString());
            this.cmie.Method.MethodName = this.PublicNamesUsed["ConstructPathFunction"].ToString();
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
            this.coce.Parameters.Add(this.cmie);
            expression.Parameters.Add(this.coce);
            expression.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cctor.Statements.Add(new CodeExpressionStatement(expression));
            this.cc.Members.Add(this.cctor);
        }

        private void GenerateConstructorWithScopePath()
        {
            string name = "path";
            this.cctor = new CodeConstructor();
            this.cctor.Attributes = MemberAttributes.Public;
            this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
            this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()), name));
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString(), TargetObject = new CodeThisReferenceExpression() }
            };
            expression.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
            expression.Parameters.Add(new CodeVariableReferenceExpression(name));
            expression.Parameters.Add(new CodePrimitiveExpression(null));
            this.cctor.Statements.Add(new CodeExpressionStatement(expression));
            this.cc.Members.Add(this.cctor);
        }

        private void GenerateConstructorWithScopePathOptions()
        {
            string name = "path";
            string str2 = "getOptions";
            this.cctor = new CodeConstructor();
            this.cctor.Attributes = MemberAttributes.Public;
            this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
            this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()), name));
            this.cctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["GetOptionsClass"].ToString()), str2));
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString(), TargetObject = new CodeThisReferenceExpression() }
            };
            expression.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
            expression.Parameters.Add(new CodeVariableReferenceExpression(name));
            expression.Parameters.Add(new CodeVariableReferenceExpression(str2));
            this.cctor.Statements.Add(new CodeExpressionStatement(expression));
            this.cc.Members.Add(this.cctor);
        }

        private void GenerateConstructPath()
        {
            this.cmm = new CodeMemberMethod();
            this.cmm.Name = this.PublicNamesUsed["ConstructPathFunction"].ToString();
            this.cmm.Attributes = MemberAttributes.Private | MemberAttributes.Static;
            this.cmm.ReturnType = new CodeTypeReference("System.String");
            for (int i = 0; i < this.arrKeys.Count; i++)
            {
                string baseType = ((CodeTypeReference) this.arrKeyType[i]).BaseType;
                this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(baseType, "key" + this.arrKeys[i].ToString()));
            }
            string str2 = this.OriginalNamespace + ":" + this.OriginalClassName;
            if (this.bSingletonClass)
            {
                str2 = str2 + "=@";
                this.cmm.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(str2)));
            }
            else
            {
                string name = "strPath";
                this.cmm.Statements.Add(new CodeVariableDeclarationStatement("System.String", name, new CodePrimitiveExpression(str2)));
                for (int j = 0; j < this.arrKeys.Count; j++)
                {
                    CodeMethodInvokeExpression expression;
                    if (((CodeTypeReference) this.arrKeyType[j]).BaseType == "System.String")
                    {
                        CodeMethodInvokeExpression expression2 = GenerateConcatStrings(new CodeVariableReferenceExpression("key" + this.arrKeys[j]), new CodePrimitiveExpression("\""));
                        CodeMethodInvokeExpression expression3 = GenerateConcatStrings(new CodePrimitiveExpression("\""), expression2);
                        CodeMethodInvokeExpression expression4 = GenerateConcatStrings(new CodePrimitiveExpression((j == 0) ? ("." + this.arrKeys[j] + "=") : ("," + this.arrKeys[j] + "=")), expression3);
                        expression = GenerateConcatStrings(new CodeVariableReferenceExpression(name), expression4);
                    }
                    else
                    {
                        this.cmie = new CodeMethodInvokeExpression();
                        this.cmie.Method.TargetObject = new CodeCastExpression(new CodeTypeReference(((CodeTypeReference) this.arrKeyType[j]).BaseType + " "), new CodeVariableReferenceExpression("key" + this.arrKeys[j]));
                        this.cmie.Method.MethodName = "ToString";
                        CodeMethodInvokeExpression expression5 = GenerateConcatStrings(new CodePrimitiveExpression((j == 0) ? ("." + this.arrKeys[j] + "=") : ("," + this.arrKeys[j] + "=")), this.cmie);
                        expression = GenerateConcatStrings(new CodeVariableReferenceExpression(name), expression5);
                    }
                    this.cmm.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(name), expression));
                }
                this.cmm.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(name)));
            }
            this.cc.Members.Add(this.cmm);
        }

        private void GenerateCreateInstance()
        {
            string name = "tmpMgmtClass";
            this.cmm = new CodeMemberMethod();
            string str2 = "mgmtScope";
            string str3 = "mgmtPath";
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            this.cmm.Name = this.PublicNamesUsed["CreateInst"].ToString();
            this.cmm.ReturnType = new CodeTypeReference(this.PrivateNamesUsed["GeneratedClassName"].ToString());
            this.caa = new CodeAttributeArgument();
            this.caa.Value = new CodePrimitiveExpression(true);
            this.cad = new CodeAttributeDeclaration();
            this.cad.Name = "Browsable";
            this.cad.Arguments.Add(this.caa);
            this.cmm.CustomAttributes = new CodeAttributeDeclarationCollection();
            this.cmm.CustomAttributes.Add(this.cad);
            this.cmm.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), str2, new CodePrimitiveExpression(null)));
            CodeConditionStatement statement = new CodeConditionStatement();
            CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["statMgmtScope"].ToString()),
                Right = new CodePrimitiveExpression(null),
                Operator = CodeBinaryOperatorType.IdentityEquality
            };
            statement.Condition = expression;
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString());
            statement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str2), this.coce));
            statement.TrueStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Path"), "NamespacePath"), new CodeVariableReferenceExpression(this.PrivateNamesUsed["CreationWmiNamespace"].ToString())));
            statement.FalseStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str2), new CodeVariableReferenceExpression(this.PrivateNamesUsed["statMgmtScope"].ToString())));
            this.cmm.Statements.Add(statement);
            CodeObjectCreateExpression initExpression = new CodeObjectCreateExpression {
                CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString())
            };
            initExpression.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["CreationClassName"].ToString()));
            this.cmm.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()), str3, initExpression));
            CodeObjectCreateExpression expression3 = new CodeObjectCreateExpression {
                CreateType = new CodeTypeReference(this.PublicNamesUsed["ManagementClass"].ToString())
            };
            expression3.Parameters.Add(new CodeVariableReferenceExpression(str2));
            expression3.Parameters.Add(new CodeVariableReferenceExpression(str3));
            expression3.Parameters.Add(new CodePrimitiveExpression(null));
            this.cmm.Statements.Add(new CodeVariableDeclarationStatement(this.PublicNamesUsed["ManagementClass"].ToString(), name, expression3));
            CodeMethodInvokeExpression expression4 = new CodeMethodInvokeExpression {
                Method = { MethodName = "CreateInstance", TargetObject = new CodeVariableReferenceExpression(name) }
            };
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PrivateNamesUsed["GeneratedClassName"].ToString());
            this.coce.Parameters.Add(expression4);
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.coce));
            this.cc.Members.Add(this.cmm);
        }

        private void GenerateDateTimeConversionFunction()
        {
            this.AddToDateTimeFunction();
            this.AddToDMTFDateTimeFunction();
        }

        private void GenerateDefaultConstructor()
        {
            this.cctor = new CodeConstructor();
            this.cctor.Attributes = MemberAttributes.Public;
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { MethodName = this.PrivateNamesUsed["InitialObjectFunc"].ToString(), TargetObject = new CodeThisReferenceExpression() }
            };
            expression.Parameters.Add(new CodePrimitiveExpression(null));
            if (this.bSingletonClass)
            {
                this.cmie = new CodeMethodInvokeExpression();
                this.cmie.Method.TargetObject = new CodeTypeReferenceExpression(this.PrivateNamesUsed["GeneratedClassName"].ToString());
                this.cmie.Method.MethodName = this.PublicNamesUsed["ConstructPathFunction"].ToString();
                this.coce = new CodeObjectCreateExpression();
                this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
                this.coce.Parameters.Add(this.cmie);
                expression.Parameters.Add(this.coce);
            }
            else
            {
                expression.Parameters.Add(new CodePrimitiveExpression(null));
            }
            expression.Parameters.Add(new CodePrimitiveExpression(null));
            this.cctor.Statements.Add(new CodeExpressionStatement(expression));
            this.cc.Members.Add(this.cctor);
            this.cctor.Comments.Add(new CodeCommentStatement(GetString("COMMENT_CONSTRUCTORS")));
        }

        private void GenerateDeleteInstance()
        {
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            this.cmm.Name = this.PublicNamesUsed["DeleteInst"].ToString();
            this.caa = new CodeAttributeArgument();
            this.caa.Value = new CodePrimitiveExpression(true);
            this.cad = new CodeAttributeDeclaration();
            this.cad.Name = "Browsable";
            this.cad.Arguments.Add(this.caa);
            this.cmm.CustomAttributes = new CodeAttributeDeclarationCollection();
            this.cmm.CustomAttributes.Add(this.cad);
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { MethodName = "Delete", TargetObject = new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()) }
            };
            this.cmm.Statements.Add(expression);
            this.cc.Members.Add(this.cmm);
        }

        private void GenerateEnumeratorClass()
        {
            string variableName = "privObjEnum";
            string str2 = "ManagementObjectEnumerator";
            string str3 = "ManagementObjectCollection";
            string str4 = "objEnum";
            this.ecc = new CodeTypeDeclaration(this.PrivateNamesUsed["EnumeratorClass"].ToString());
            this.ecc.TypeAttributes = TypeAttributes.NestedPublic;
            this.ecc.BaseTypes.Add("System.Object");
            this.ecc.BaseTypes.Add("System.Collections.IEnumerator");
            this.cf = new CodeMemberField();
            this.cf.Name = variableName;
            this.cf.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            this.cf.Type = new CodeTypeReference(str3 + "." + str2);
            this.ecc.Members.Add(this.cf);
            this.cctor = new CodeConstructor();
            this.cctor.Attributes = MemberAttributes.Public;
            this.cpde = new CodeParameterDeclarationExpression();
            this.cpde.Name = str4;
            this.cpde.Type = new CodeTypeReference(str3 + "." + str2);
            this.cctor.Parameters.Add(this.cpde);
            this.cctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(variableName), new CodeVariableReferenceExpression(str4)));
            this.ecc.Members.Add(this.cctor);
            this.cmp = new CodeMemberProperty();
            this.cmp.Type = new CodeTypeReference("System.Object");
            this.cmp.Attributes = MemberAttributes.Public | MemberAttributes.Override | MemberAttributes.Final;
            this.cmp.Name = "Current";
            this.cmp.ImplementationTypes.Add("System.Collections.IEnumerator");
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PrivateNamesUsed["GeneratedClassName"].ToString());
            this.coce.Parameters.Add(new CodeCastExpression(new CodeTypeReference(this.PublicNamesUsed["LateBoundClass"].ToString()), new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(variableName), "Current")));
            this.cmp.GetStatements.Add(new CodeMethodReturnStatement(this.coce));
            this.ecc.Members.Add(this.cmp);
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Override | MemberAttributes.Final;
            this.cmm.Name = "MoveNext";
            this.cmm.ImplementationTypes.Add("System.Collections.IEnumerator");
            this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
            this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(variableName), "MoveNext", new CodeExpression[0]);
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
            this.ecc.Members.Add(this.cmm);
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Override | MemberAttributes.Final;
            this.cmm.Name = "Reset";
            this.cmm.ImplementationTypes.Add("System.Collections.IEnumerator");
            this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(variableName), "Reset", new CodeExpression[0]);
            this.cmm.Statements.Add(new CodeExpressionStatement(this.cmie));
            this.ecc.Members.Add(this.cmm);
            this.ccc.Members.Add(this.ecc);
        }

        private void GenerateGetInstancesWithCondition()
        {
            string name = "condition";
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            this.cmm.Name = this.PublicNamesUsed["FilterFunction"].ToString();
            this.cmm.ReturnType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.String", name));
            this.cmie = new CodeMethodInvokeExpression(null, this.PublicNamesUsed["FilterFunction"].ToString(), new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
            this.cc.Members.Add(this.cmm);
        }

        private void GenerateGetInstancesWithNoParameters()
        {
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            this.cmm.Name = this.PublicNamesUsed["FilterFunction"].ToString();
            this.cmm.ReturnType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
            this.cmie = new CodeMethodInvokeExpression();
            this.cmie.Method.MethodName = this.PublicNamesUsed["FilterFunction"].ToString();
            this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
            this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
            this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
            this.cc.Members.Add(this.cmm);
            this.cmm.Comments.Add(new CodeCommentStatement(GetString("COMMENT_GETINSTANCES")));
        }

        private void GenerateGetInstancesWithProperties()
        {
            string name = "selectedProperties";
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            this.cmm.Name = this.PublicNamesUsed["FilterFunction"].ToString();
            this.cmm.ReturnType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.String []", name));
            this.cmie = new CodeMethodInvokeExpression(null, this.PublicNamesUsed["FilterFunction"].ToString(), new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
            this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
            this.cc.Members.Add(this.cmm);
        }

        private void GenerateGetInstancesWithScope()
        {
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            this.cmm.Name = this.PublicNamesUsed["FilterFunction"].ToString();
            this.cmm.ReturnType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["QueryOptionsClass"].ToString()), this.PrivateNamesUsed["EnumParam"].ToString()));
            string name = "clsObject";
            string str2 = "pathObj";
            this.cis = new CodeConditionStatement();
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString());
            this.cboe.Right = new CodePrimitiveExpression(null);
            this.cboe.Operator = CodeBinaryOperatorType.IdentityEquality;
            this.cis.Condition = this.cboe;
            CodeConditionStatement statement = new CodeConditionStatement();
            CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["statMgmtScope"].ToString()),
                Right = new CodePrimitiveExpression(null),
                Operator = CodeBinaryOperatorType.IdentityEquality
            };
            statement.Condition = expression;
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString());
            statement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()), this.coce));
            statement.TrueStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()), "Path"), "NamespacePath"), new CodePrimitiveExpression(this.classobj.Scope.Path.NamespacePath)));
            statement.FalseStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()), new CodeVariableReferenceExpression(this.PrivateNamesUsed["statMgmtScope"].ToString())));
            this.cis.TrueStatements.Add(statement);
            this.cmm.Statements.Add(this.cis);
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
            this.cmm.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()), str2, this.coce));
            this.cmm.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "ClassName"), new CodePrimitiveExpression(this.OriginalClassName)));
            this.cmm.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "NamespacePath"), new CodePrimitiveExpression(this.classobj.Scope.Path.NamespacePath)));
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ManagementClass"].ToString());
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(str2));
            this.coce.Parameters.Add(new CodePrimitiveExpression(null));
            this.cmm.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(this.PublicNamesUsed["ManagementClass"].ToString()), name, this.coce));
            this.cis = new CodeConditionStatement();
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["EnumParam"].ToString());
            this.cboe.Right = new CodePrimitiveExpression(null);
            this.cboe.Operator = CodeBinaryOperatorType.IdentityEquality;
            this.cis.Condition = this.cboe;
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["QueryOptionsClass"].ToString());
            this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["EnumParam"].ToString()), this.coce));
            this.cis.TrueStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["EnumParam"].ToString()), "EnsureLocatable"), new CodePrimitiveExpression(true)));
            this.cmm.Statements.Add(this.cis);
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
            this.cmie = new CodeMethodInvokeExpression();
            this.cmie.Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(name), "GetInstances");
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["EnumParam"].ToString()));
            this.coce.Parameters.Add(this.cmie);
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.coce));
            this.cc.Members.Add(this.cmm);
        }

        private void GenerateGetInstancesWithScopeCondition()
        {
            string name = "condition";
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            this.cmm.Name = this.PublicNamesUsed["FilterFunction"].ToString();
            this.cmm.ReturnType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.String"), name));
            this.cmie = new CodeMethodInvokeExpression(null, this.PublicNamesUsed["FilterFunction"].ToString(), new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
            this.cc.Members.Add(this.cmm);
        }

        private void GenerateGetInstancesWithScopeProperties()
        {
            string name = "selectedProperties";
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            this.cmm.Name = this.PublicNamesUsed["FilterFunction"].ToString();
            this.cmm.ReturnType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(this.PublicNamesUsed["ScopeClass"].ToString(), this.PrivateNamesUsed["ScopeParam"].ToString()));
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.String []", name));
            this.cmie = new CodeMethodInvokeExpression(null, this.PublicNamesUsed["FilterFunction"].ToString(), new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
            this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
            this.cc.Members.Add(this.cmm);
        }

        private void GenerateGetInstancesWithScopeWhereProperties()
        {
            string name = "condition";
            string str2 = "selectedProperties";
            string str3 = "ObjectSearcher";
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            this.cmm.Name = this.PublicNamesUsed["FilterFunction"].ToString();
            this.cmm.ReturnType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.String", name));
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.String []", str2));
            this.cis = new CodeConditionStatement();
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString());
            this.cboe.Right = new CodePrimitiveExpression(null);
            this.cboe.Operator = CodeBinaryOperatorType.IdentityEquality;
            this.cis.Condition = this.cboe;
            CodeConditionStatement statement = new CodeConditionStatement();
            CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["statMgmtScope"].ToString()),
                Right = new CodePrimitiveExpression(null),
                Operator = CodeBinaryOperatorType.IdentityEquality
            };
            statement.Condition = expression;
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString());
            statement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()), this.coce));
            statement.TrueStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()), "Path"), "NamespacePath"), new CodePrimitiveExpression(this.classobj.Scope.Path.NamespacePath)));
            statement.FalseStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()), new CodeVariableReferenceExpression(this.PrivateNamesUsed["statMgmtScope"].ToString())));
            this.cis.TrueStatements.Add(statement);
            this.cmm.Statements.Add(this.cis);
            CodeObjectCreateExpression expression2 = new CodeObjectCreateExpression {
                CreateType = new CodeTypeReference(this.PublicNamesUsed["QueryClass"].ToString())
            };
            expression2.Parameters.Add(new CodePrimitiveExpression(this.OriginalClassName));
            expression2.Parameters.Add(new CodeVariableReferenceExpression(name));
            expression2.Parameters.Add(new CodeVariableReferenceExpression(str2));
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ObjectSearcherClass"].ToString());
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
            this.coce.Parameters.Add(expression2);
            this.cmm.Statements.Add(new CodeVariableDeclarationStatement(this.PublicNamesUsed["ObjectSearcherClass"].ToString(), str3, this.coce));
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["QueryOptionsClass"].ToString());
            this.cmm.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(this.PublicNamesUsed["QueryOptionsClass"].ToString()), this.PrivateNamesUsed["EnumParam"].ToString(), this.coce));
            this.cmm.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["EnumParam"].ToString()), "EnsureLocatable"), new CodePrimitiveExpression(true)));
            this.cmm.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str3), "Options"), new CodeVariableReferenceExpression(this.PrivateNamesUsed["EnumParam"].ToString())));
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
            this.coce.Parameters.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str3), "Get", new CodeExpression[0]));
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.coce));
            this.cc.Members.Add(this.cmm);
        }

        private void GenerateGetInstancesWithWhereProperties()
        {
            string name = "selectedProperties";
            string str2 = "condition";
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            this.cmm.Name = this.PublicNamesUsed["FilterFunction"].ToString();
            this.cmm.ReturnType = new CodeTypeReference(this.PrivateNamesUsed["CollectionClass"].ToString());
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.String", str2));
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.String []", name));
            this.cmie = new CodeMethodInvokeExpression(null, this.PublicNamesUsed["FilterFunction"].ToString(), new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str2));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
            this.cc.Members.Add(this.cmm);
        }

        private void GenerateIfClassvalidFunction()
        {
            this.GenerateIfClassvalidFuncWithAllParams();
            string name = "theObj";
            string str2 = "count";
            string str3 = "parentClasses";
            this.cmm = new CodeMemberMethod();
            this.cmm.Name = this.PrivateNamesUsed["ClassNameCheckFunc"].ToString();
            this.cmm.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["BaseObjClass"].ToString()), name));
            CodeExpression[] parameters = new CodeExpression[] { new CodeCastExpression(new CodeTypeReference("System.String"), new CodeIndexerExpression(new CodeVariableReferenceExpression(name), new CodeExpression[] { new CodePrimitiveExpression("__CLASS") })), new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), this.PublicNamesUsed["ClassNameProperty"].ToString()), new CodePrimitiveExpression(true), new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("System.Globalization.CultureInfo"), "InvariantCulture") };
            this.cmie = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("System.String"), "Compare", parameters);
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = this.cmie;
            this.cboe.Right = new CodePrimitiveExpression(0);
            this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
            CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(name),
                Right = new CodePrimitiveExpression(null),
                Operator = CodeBinaryOperatorType.IdentityInequality
            };
            CodeBinaryOperatorExpression expression2 = new CodeBinaryOperatorExpression {
                Left = expression,
                Right = this.cboe,
                Operator = CodeBinaryOperatorType.BooleanAnd
            };
            this.cis = new CodeConditionStatement();
            this.cis.Condition = expression2;
            this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));
            CodeExpression initExpression = new CodeCastExpression(new CodeTypeReference("System.Array"), new CodeIndexerExpression(new CodeVariableReferenceExpression(name), new CodeExpression[] { new CodePrimitiveExpression("__DERIVATION") }));
            this.cis.FalseStatements.Add(new CodeVariableDeclarationStatement("System.Array", str3, initExpression));
            CodeConditionStatement statement = new CodeConditionStatement();
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = new CodeVariableReferenceExpression(str3);
            this.cboe.Right = new CodePrimitiveExpression(null);
            this.cboe.Operator = CodeBinaryOperatorType.IdentityInequality;
            statement.Condition = this.cboe;
            this.cfls = new CodeIterationStatement();
            statement.TrueStatements.Add(new CodeVariableDeclarationStatement("System.Int32", str2, new CodePrimitiveExpression(0)));
            this.cfls.InitStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(str2), new CodePrimitiveExpression(0));
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = new CodeVariableReferenceExpression(str2);
            this.cboe.Operator = CodeBinaryOperatorType.LessThan;
            this.cboe.Right = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str3), "Length");
            this.cfls.TestExpression = this.cboe;
            this.cfls.IncrementStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(str2), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(str2), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1)));
            CodeMethodInvokeExpression expression4 = new CodeMethodInvokeExpression {
                Method = { MethodName = "GetValue", TargetObject = new CodeVariableReferenceExpression(str3) }
            };
            expression4.Parameters.Add(new CodeVariableReferenceExpression(str2));
            CodeExpression[] expressionArray2 = new CodeExpression[] { new CodeCastExpression(new CodeTypeReference("System.String"), expression4), new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), this.PublicNamesUsed["ClassNameProperty"].ToString()), new CodePrimitiveExpression(true), new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("System.Globalization.CultureInfo"), "InvariantCulture") };
            CodeMethodInvokeExpression expression5 = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("System.String"), "Compare", expressionArray2);
            CodeConditionStatement statement2 = new CodeConditionStatement();
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = expression5;
            this.cboe.Right = new CodePrimitiveExpression(0);
            this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
            statement2.Condition = this.cboe;
            statement2.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));
            statement.TrueStatements.Add(this.cfls);
            this.cfls.Statements.Add(statement2);
            this.cis.FalseStatements.Add(statement);
            this.cmm.Statements.Add(this.cis);
            this.cmm.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));
            this.cc.Members.Add(this.cmm);
        }

        private void GenerateIfClassvalidFuncWithAllParams()
        {
            string name = "path";
            string str2 = "OptionsParam";
            this.cmm = new CodeMemberMethod();
            this.cmm.Name = this.PrivateNamesUsed["ClassNameCheckFunc"].ToString();
            this.cmm.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()), name));
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["GetOptionsClass"].ToString()), str2));
            CodeExpression[] parameters = new CodeExpression[] { new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(name), "ClassName"), new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), this.PublicNamesUsed["ClassNameProperty"].ToString()), new CodePrimitiveExpression(true), new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("System.Globalization.CultureInfo"), "InvariantCulture") };
            this.cmie = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("System.String"), "Compare", parameters);
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = this.cmie;
            this.cboe.Right = new CodePrimitiveExpression(0);
            this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
            CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(name),
                Right = new CodePrimitiveExpression(null),
                Operator = CodeBinaryOperatorType.IdentityInequality
            };
            CodeBinaryOperatorExpression expression2 = new CodeBinaryOperatorExpression {
                Left = expression,
                Right = this.cboe,
                Operator = CodeBinaryOperatorType.BooleanAnd
            };
            this.cis = new CodeConditionStatement();
            this.cis.Condition = expression2;
            this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["LateBoundClass"].ToString());
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(str2));
            CodeMethodReferenceExpression method = new CodeMethodReferenceExpression {
                MethodName = this.PrivateNamesUsed["ClassNameCheckFunc"].ToString()
            };
            this.cis.FalseStatements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(method, new CodeExpression[] { this.coce })));
            this.cmm.Statements.Add(this.cis);
            this.cc.Members.Add(this.cmm);
        }

        private void GenerateInitializeObject()
        {
            string name = "path";
            string str2 = "getOptions";
            bool flag = true;
            try
            {
                this.classobj.Qualifiers["priveleges"].ToString();
            }
            catch (ManagementException exception)
            {
                if (exception.ErrorCode != ManagementStatus.NotFound)
                {
                    throw;
                }
                flag = false;
            }
            CodeMemberMethod cmMethod = new CodeMemberMethod {
                Name = this.PrivateNamesUsed["InitialObjectFunc"].ToString(),
                Attributes = MemberAttributes.Private | MemberAttributes.Final
            };
            cmMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString()), this.PrivateNamesUsed["ScopeParam"].ToString()));
            cmMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()), name));
            cmMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.PublicNamesUsed["GetOptionsClass"].ToString()), str2));
            this.InitPrivateMemberVariables(cmMethod);
            this.cis = new CodeConditionStatement();
            this.cis.Condition = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(name), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
            CodeConditionStatement statement = new CodeConditionStatement();
            this.cmie = new CodeMethodInvokeExpression();
            this.cmie.Method.MethodName = this.PrivateNamesUsed["ClassNameCheckFunc"].ToString();
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str2));
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = this.cmie;
            this.cboe.Right = new CodePrimitiveExpression(true);
            this.cboe.Operator = CodeBinaryOperatorType.IdentityInequality;
            statement.Condition = this.cboe;
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ArgumentExceptionClass"].ToString());
            this.coce.Parameters.Add(new CodePrimitiveExpression(GetString("CLASSNOT_FOUND_EXCEPT")));
            statement.TrueStatements.Add(new CodeThrowExceptionStatement(this.coce));
            this.cis.TrueStatements.Add(statement);
            cmMethod.Statements.Add(this.cis);
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["LateBoundClass"].ToString());
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["ScopeParam"].ToString()));
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(str2));
            cmMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()), this.coce));
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["SystemPropertiesClass"].ToString());
            this.coce.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()));
            cmMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["SystemPropertiesObject"].ToString()), this.coce));
            cmMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["CurrentObject"].ToString()), new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString())));
            this.cc.Members.Add(cmMethod);
            if (flag)
            {
                this.cpre = new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()), this.PublicNamesUsed["ScopeProperty"].ToString()), "Options"), "EnablePrivileges");
                this.cctor.Statements.Add(new CodeAssignStatement(this.cpre, new CodePrimitiveExpression(true)));
            }
        }

        private void GenerateMethods()
        {
            string name = "inParams";
            string str2 = "outParams";
            string str3 = "classObj";
            bool flag = false;
            bool flag2 = false;
            CodePropertyReferenceExpression initExpression = null;
            CimType cimType = CimType.SInt8;
            CodeTypeReference type = null;
            bool bArray = false;
            bool dateTimeType = false;
            ArrayList list = new ArrayList(5);
            ArrayList list2 = new ArrayList(5);
            ArrayList list3 = new ArrayList(5);
            for (int i = 0; i < this.PublicMethods.Count; i++)
            {
                flag = false;
                MethodData data = this.classobj.Methods[this.PublicMethods.GetKey(i).ToString()];
                string variableName = this.PrivateNamesUsed["LateBoundObject"].ToString();
                if ((data.OutParameters != null) && (data.OutParameters.Properties != null))
                {
                    foreach (PropertyData data2 in data.OutParameters.Properties)
                    {
                        list.Add(data2.Name);
                    }
                }
                this.cmm = new CodeMemberMethod();
                this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                this.cmm.Name = this.PublicMethods[data.Name].ToString();
                foreach (QualifierData data3 in data.Qualifiers)
                {
                    if (string.Compare(data3.Name, "static", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.cmm.Attributes |= MemberAttributes.Static;
                        flag = true;
                        break;
                    }
                    if (string.Compare(data3.Name, "privileges", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        flag2 = true;
                    }
                }
                this.cis = new CodeConditionStatement();
                this.cboe = new CodeBinaryOperatorExpression();
                if (flag)
                {
                    this.cmm.Statements.Add(new CodeVariableDeclarationStatement("System.Boolean", "IsMethodStatic", new CodePrimitiveExpression(flag)));
                    this.cboe.Left = new CodeVariableReferenceExpression("IsMethodStatic");
                    this.cboe.Right = new CodePrimitiveExpression(true);
                }
                else
                {
                    this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString());
                    this.cboe.Right = new CodePrimitiveExpression(false);
                }
                this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
                this.cis.Condition = this.cboe;
                bool flag5 = true;
                this.cis.TrueStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(this.PublicNamesUsed["BaseObjClass"].ToString()), name, new CodePrimitiveExpression(null)));
                if (flag)
                {
                    string str5 = "mgmtPath";
                    CodeObjectCreateExpression expression2 = new CodeObjectCreateExpression {
                        CreateType = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString())
                    };
                    expression2.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["CreationClassName"].ToString()));
                    this.cis.TrueStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()), str5, expression2));
                    CodeObjectCreateExpression expression3 = new CodeObjectCreateExpression {
                        CreateType = new CodeTypeReference(this.PublicNamesUsed["ManagementClass"].ToString())
                    };
                    expression3.Parameters.Add(new CodeVariableReferenceExpression(this.PrivateNamesUsed["statMgmtScope"].ToString()));
                    expression3.Parameters.Add(new CodeVariableReferenceExpression(str5));
                    expression3.Parameters.Add(new CodePrimitiveExpression(null));
                    this.coce = new CodeObjectCreateExpression();
                    this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ManagementClass"].ToString());
                    this.coce.Parameters.Add(expression3);
                    this.cis.TrueStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(this.PublicNamesUsed["ManagementClass"].ToString()), str3, expression3));
                    variableName = str3;
                }
                if (flag2)
                {
                    initExpression = new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(flag ? str3 : this.PrivateNamesUsed["LateBoundObject"].ToString()), this.PublicNamesUsed["ScopeProperty"].ToString()), "Options"), "EnablePrivileges");
                    this.cis.TrueStatements.Add(new CodeVariableDeclarationStatement("System.Boolean", this.PrivateNamesUsed["Privileges"].ToString(), initExpression));
                    this.cis.TrueStatements.Add(new CodeAssignStatement(initExpression, new CodePrimitiveExpression(true)));
                }
                if ((data.InParameters != null) && (data.InParameters.Properties != null))
                {
                    foreach (PropertyData data4 in data.InParameters.Properties)
                    {
                        dateTimeType = false;
                        if (flag5)
                        {
                            this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(variableName), "GetMethodParameters", new CodeExpression[] { new CodePrimitiveExpression(data.Name) });
                            this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(name), this.cmie));
                            flag5 = false;
                        }
                        this.cpde = new CodeParameterDeclarationExpression();
                        this.cpde.Name = data4.Name;
                        this.cpde.Type = this.ConvertCIMType(data4.Type, data4.IsArray);
                        this.cpde.Direction = FieldDirection.In;
                        if (data4.Type == CimType.DateTime)
                        {
                            CodeTypeReference codeType = this.cpde.Type;
                            dateTimeType = this.GetDateTimeType(data4, ref codeType);
                            this.cpde.Type = codeType;
                        }
                        for (int k = 0; k < list.Count; k++)
                        {
                            if (string.Compare(data4.Name, list[k].ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                this.cpde.Direction = FieldDirection.Ref;
                                list2.Add(data4.Name);
                                list3.Add(this.cpde.Type);
                            }
                        }
                        this.cmm.Parameters.Add(this.cpde);
                        this.cie = new CodeIndexerExpression(new CodeVariableReferenceExpression(name), new CodeExpression[] { new CodePrimitiveExpression(data4.Name) });
                        if (data4.Type == CimType.Reference)
                        {
                            this.AddPropertySet(this.cie, data4.IsArray, this.cis.TrueStatements, this.PublicNamesUsed["PathClass"].ToString(), new CodeVariableReferenceExpression(this.cpde.Name));
                        }
                        else if (data4.Type == CimType.DateTime)
                        {
                            if (dateTimeType)
                            {
                                this.AddPropertySet(this.cie, data4.IsArray, this.cis.TrueStatements, "System.TimeSpan", new CodeVariableReferenceExpression(this.cpde.Name));
                            }
                            else
                            {
                                this.AddPropertySet(this.cie, data4.IsArray, this.cis.TrueStatements, "System.DateTime", new CodeVariableReferenceExpression(this.cpde.Name));
                            }
                        }
                        else if (this.cpde.Type.ArrayRank == 0)
                        {
                            this.cis.TrueStatements.Add(new CodeAssignStatement(this.cie, new CodeCastExpression(new CodeTypeReference(this.cpde.Type.BaseType + " "), new CodeVariableReferenceExpression(this.cpde.Name))));
                        }
                        else
                        {
                            this.cis.TrueStatements.Add(new CodeAssignStatement(this.cie, new CodeCastExpression(this.cpde.Type, new CodeVariableReferenceExpression(this.cpde.Name))));
                        }
                    }
                }
                list.Clear();
                bool flag7 = false;
                flag5 = true;
                bool flag8 = false;
                CodeMethodInvokeExpression right = null;
                if ((data.OutParameters != null) && (data.OutParameters.Properties != null))
                {
                    foreach (PropertyData data5 in data.OutParameters.Properties)
                    {
                        dateTimeType = false;
                        if (flag5)
                        {
                            this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(variableName), "InvokeMethod", new CodeExpression[0]);
                            this.cmie.Parameters.Add(new CodePrimitiveExpression(data.Name));
                            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
                            this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
                            this.cis.TrueStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(this.PublicNamesUsed["BaseObjClass"].ToString()), str2, this.cmie));
                            flag5 = false;
                            flag8 = true;
                        }
                        bool flag6 = false;
                        for (int m = 0; m < list2.Count; m++)
                        {
                            if (string.Compare(data5.Name, list2[m].ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                flag6 = true;
                            }
                        }
                        if (!flag6)
                        {
                            if (string.Compare(data5.Name, "ReturnValue", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                this.cmm.ReturnType = this.ConvertCIMType(data5.Type, data5.IsArray);
                                flag7 = true;
                                cimType = data5.Type;
                                if (data5.Type == CimType.DateTime)
                                {
                                    CodeTypeReference returnType = this.cmm.ReturnType;
                                    this.GetDateTimeType(data5, ref returnType);
                                    this.cmm.ReturnType = returnType;
                                }
                                type = this.cmm.ReturnType;
                                bArray = data5.IsArray;
                            }
                            else
                            {
                                this.cpde = new CodeParameterDeclarationExpression();
                                this.cpde.Name = data5.Name;
                                this.cpde.Type = this.ConvertCIMType(data5.Type, data5.IsArray);
                                this.cpde.Direction = FieldDirection.Out;
                                this.cmm.Parameters.Add(this.cpde);
                                if (data5.Type == CimType.DateTime)
                                {
                                    CodeTypeReference reference4 = this.cpde.Type;
                                    dateTimeType = this.GetDateTimeType(data5, ref reference4);
                                    this.cpde.Type = reference4;
                                }
                                this.cpre = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Properties");
                                this.cie = new CodeIndexerExpression(this.cpre, new CodeExpression[] { new CodePrimitiveExpression(data5.Name) });
                                if (data5.Type == CimType.Reference)
                                {
                                    this.GenerateCodeForRefAndDateTimeTypes(this.cie, data5.IsArray, this.cis.TrueStatements, this.PublicNamesUsed["PathClass"].ToString(), new CodeVariableReferenceExpression(data5.Name), true);
                                }
                                else if (data5.Type == CimType.DateTime)
                                {
                                    if (dateTimeType)
                                    {
                                        this.GenerateCodeForRefAndDateTimeTypes(this.cie, data5.IsArray, this.cis.TrueStatements, "System.TimeSpan", new CodeVariableReferenceExpression(data5.Name), true);
                                    }
                                    else
                                    {
                                        this.GenerateCodeForRefAndDateTimeTypes(this.cie, data5.IsArray, this.cis.TrueStatements, "System.DateTime", new CodeVariableReferenceExpression(data5.Name), true);
                                    }
                                }
                                else if (data5.IsArray || (data5.Type == CimType.Object))
                                {
                                    this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(data5.Name), new CodeCastExpression(this.ConvertCIMType(data5.Type, data5.IsArray), new CodePropertyReferenceExpression(this.cie, "Value"))));
                                }
                                else
                                {
                                    right = new CodeMethodInvokeExpression();
                                    right.Parameters.Add(new CodePropertyReferenceExpression(this.cie, "Value"));
                                    right.Method.MethodName = this.GetConversionFunction(data5.Type);
                                    right.Method.TargetObject = new CodeTypeReferenceExpression("System.Convert");
                                    this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(data5.Name), right));
                                }
                                if ((data5.Type == CimType.DateTime) && !data5.IsArray)
                                {
                                    if (dateTimeType)
                                    {
                                        this.coce = new CodeObjectCreateExpression();
                                        this.coce.CreateType = new CodeTypeReference("System.TimeSpan");
                                        this.coce.Parameters.Add(new CodePrimitiveExpression(0));
                                        this.coce.Parameters.Add(new CodePrimitiveExpression(0));
                                        this.coce.Parameters.Add(new CodePrimitiveExpression(0));
                                        this.coce.Parameters.Add(new CodePrimitiveExpression(0));
                                        this.coce.Parameters.Add(new CodePrimitiveExpression(0));
                                        this.cis.FalseStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(data5.Name), this.coce));
                                    }
                                    else
                                    {
                                        this.cis.FalseStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(data5.Name), new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.DateTime"), "MinValue")));
                                    }
                                }
                                else if (IsPropertyValueType(data5.Type) && !data5.IsArray)
                                {
                                    right = new CodeMethodInvokeExpression();
                                    right.Parameters.Add(new CodePrimitiveExpression(0));
                                    right.Method.MethodName = this.GetConversionFunction(data5.Type);
                                    right.Method.TargetObject = new CodeTypeReferenceExpression("System.Convert");
                                    this.cis.FalseStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(data5.Name), right));
                                }
                                else
                                {
                                    this.cis.FalseStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(data5.Name), new CodePrimitiveExpression(null)));
                                }
                            }
                        }
                    }
                }
                if (!flag8)
                {
                    this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(variableName), "InvokeMethod", new CodeExpression[0]);
                    this.cmie.Parameters.Add(new CodePrimitiveExpression(data.Name));
                    this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
                    this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
                    this.cmis = new CodeExpressionStatement(this.cmie);
                    this.cis.TrueStatements.Add(this.cmis);
                }
                for (int j = 0; j < list2.Count; j++)
                {
                    this.cpre = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Properties");
                    this.cie = new CodeIndexerExpression(this.cpre, new CodeExpression[] { new CodePrimitiveExpression(list2[j].ToString()) });
                    this.cis.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(list2[j].ToString()), new CodeCastExpression((CodeTypeReference) list3[j], new CodePropertyReferenceExpression(this.cie, "Value"))));
                }
                list2.Clear();
                if (flag2)
                {
                    this.cis.TrueStatements.Add(new CodeAssignStatement(initExpression, new CodeVariableReferenceExpression(this.PrivateNamesUsed["Privileges"].ToString())));
                }
                if (flag7)
                {
                    CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement(type, "retVar");
                    this.cpre = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), "Properties");
                    this.cie = new CodeIndexerExpression(this.cpre, new CodeExpression[] { new CodePrimitiveExpression("ReturnValue") });
                    if (type.BaseType == new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString()).BaseType)
                    {
                        this.cmm.Statements.Add(statement);
                        this.cmm.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("retVar"), new CodePrimitiveExpression(null)));
                        this.GenerateCodeForRefAndDateTimeTypes(this.cie, bArray, this.cis.TrueStatements, this.PublicNamesUsed["PathClass"].ToString(), new CodeVariableReferenceExpression("retVar"), true);
                        this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("retVar")));
                        this.cis.FalseStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
                    }
                    else if (type.BaseType == "System.DateTime")
                    {
                        this.cmm.Statements.Add(statement);
                        this.cmm.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("retVar"), new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.DateTime"), "MinValue")));
                        this.GenerateCodeForRefAndDateTimeTypes(this.cie, bArray, this.cis.TrueStatements, "System.DateTime", new CodeVariableReferenceExpression("retVar"), true);
                        this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("retVar")));
                        this.cis.FalseStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("retVar")));
                    }
                    else if (type.BaseType == "System.TimeSpan")
                    {
                        this.cmm.Statements.Add(statement);
                        this.coce = new CodeObjectCreateExpression();
                        this.coce.CreateType = new CodeTypeReference("System.TimeSpan");
                        this.coce.Parameters.Add(new CodePrimitiveExpression(0));
                        this.coce.Parameters.Add(new CodePrimitiveExpression(0));
                        this.coce.Parameters.Add(new CodePrimitiveExpression(0));
                        this.coce.Parameters.Add(new CodePrimitiveExpression(0));
                        this.coce.Parameters.Add(new CodePrimitiveExpression(0));
                        this.cmm.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("retVar"), this.coce));
                        this.GenerateCodeForRefAndDateTimeTypes(this.cie, bArray, this.cis.TrueStatements, "System.TimeSpan", new CodeVariableReferenceExpression("retVar"), true);
                        this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("retVar")));
                        this.cis.FalseStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("retVar")));
                    }
                    else if ((type.ArrayRank == 0) && (type.BaseType != new CodeTypeReference(this.PublicNamesUsed["BaseObjClass"].ToString()).BaseType))
                    {
                        this.cmie = new CodeMethodInvokeExpression();
                        this.cmie.Parameters.Add(new CodePropertyReferenceExpression(this.cie, "Value"));
                        this.cmie.Method.MethodName = this.GetConversionFunction(cimType);
                        this.cmie.Method.TargetObject = new CodeTypeReferenceExpression("System.Convert");
                        this.cis.TrueStatements.Add(new CodeMethodReturnStatement(this.cmie));
                        this.cmie = new CodeMethodInvokeExpression();
                        this.cmie.Parameters.Add(new CodePrimitiveExpression(0));
                        this.cmie.Method.MethodName = this.GetConversionFunction(cimType);
                        this.cmie.Method.TargetObject = new CodeTypeReferenceExpression("System.Convert");
                        this.cis.FalseStatements.Add(new CodeMethodReturnStatement(this.cmie));
                    }
                    else
                    {
                        this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(type, new CodePropertyReferenceExpression(this.cie, "Value"))));
                        this.cis.FalseStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
                    }
                }
                this.cmm.Statements.Add(this.cis);
                this.cc.Members.Add(this.cmm);
            }
        }

        private void GenerateMethodToInitializeVariables()
        {
            CodeMemberMethod method = new CodeMemberMethod {
                Name = this.PrivateNamesUsed["initVariable"].ToString(),
                Attributes = MemberAttributes.Private | MemberAttributes.Final
            };
            method.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["AutoCommitProperty"].ToString()), new CodePrimitiveExpression(true)));
            method.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString()), new CodePrimitiveExpression(false)));
            this.cc.Members.Add(method);
        }

        private void GeneratePathProperty()
        {
            this.cmp = new CodeMemberProperty();
            this.cmp.Name = this.PublicNamesUsed["PathProperty"].ToString();
            this.cmp.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            this.cmp.Type = new CodeTypeReference(this.PublicNamesUsed["PathClass"].ToString());
            this.caa = new CodeAttributeArgument();
            this.caa.Value = new CodePrimitiveExpression(true);
            this.cad = new CodeAttributeDeclaration();
            this.cad.Name = "Browsable";
            this.cad.Arguments.Add(this.caa);
            this.cmp.CustomAttributes = new CodeAttributeDeclarationCollection();
            this.cmp.CustomAttributes.Add(this.cad);
            this.cpre = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()), "Path");
            this.cis = new CodeConditionStatement();
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString());
            this.cboe.Right = new CodePrimitiveExpression(false);
            this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
            this.cis.Condition = this.cboe;
            this.cis.TrueStatements.Add(new CodeMethodReturnStatement(this.cpre));
            this.cis.FalseStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
            this.cmp.GetStatements.Add(this.cis);
            this.cis = new CodeConditionStatement();
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString());
            this.cboe.Right = new CodePrimitiveExpression(false);
            this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
            this.cis.Condition = this.cboe;
            CodeConditionStatement statement = new CodeConditionStatement();
            this.cmie = new CodeMethodInvokeExpression();
            this.cmie.Method.MethodName = this.PrivateNamesUsed["ClassNameCheckFunc"].ToString();
            this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression("value"));
            this.cmie.Parameters.Add(new CodePrimitiveExpression(null));
            CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression {
                Left = this.cmie,
                Right = new CodePrimitiveExpression(true),
                Operator = CodeBinaryOperatorType.IdentityInequality
            };
            statement.Condition = expression;
            this.coce = new CodeObjectCreateExpression();
            this.coce.CreateType = new CodeTypeReference(this.PublicNamesUsed["ArgumentExceptionClass"].ToString());
            this.coce.Parameters.Add(new CodePrimitiveExpression(GetString("CLASSNOT_FOUND_EXCEPT")));
            statement.TrueStatements.Add(new CodeThrowExceptionStatement(this.coce));
            this.cis.TrueStatements.Add(statement);
            this.cis.TrueStatements.Add(new CodeAssignStatement(this.cpre, new CodeSnippetExpression("value")));
            this.cmp.SetStatements.Add(this.cis);
            this.cc.Members.Add(this.cmp);
            this.cmp.Comments.Add(new CodeCommentStatement(GetString("COMMENT_MGMTPATH")));
        }

        private void GeneratePrivateMember(string memberName, string MemberType, string Comment)
        {
            this.GeneratePrivateMember(memberName, MemberType, null, false, Comment);
        }

        private void GeneratePrivateMember(string memberName, string MemberType, CodeExpression initExpression, bool isStatic, string Comment)
        {
            this.cf = new CodeMemberField();
            this.cf.Name = memberName;
            this.cf.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            if (isStatic)
            {
                this.cf.Attributes |= MemberAttributes.Static;
            }
            this.cf.Type = new CodeTypeReference(MemberType);
            if ((initExpression != null) && isStatic)
            {
                this.cf.InitExpression = initExpression;
            }
            this.cc.Members.Add(this.cf);
            if ((Comment != null) && (Comment.Length != 0))
            {
                this.cf.Comments.Add(new CodeCommentStatement(Comment));
            }
        }

        private void GenerateProperties()
        {
            bool bDynamicClass = this.IsDynamicClass();
            CodeMemberMethod method = null;
            CodeMemberProperty property = null;
            string propertyName = string.Empty;
            bool dateTimeType = false;
            for (int i = 0; i < this.PublicProperties.Count; i++)
            {
                dateTimeType = false;
                PropertyData prop = this.classobj.Properties[this.PublicProperties.GetKey(i).ToString()];
                bool bRead = true;
                bool bWrite = true;
                bool bStatic = false;
                this.cmp = new CodeMemberProperty();
                this.cmp.Name = this.PublicProperties[prop.Name].ToString();
                this.cmp.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                this.cmp.Type = this.ConvertCIMType(prop.Type, prop.IsArray);
                if (prop.Type == CimType.DateTime)
                {
                    CodeTypeReference type = this.cmp.Type;
                    dateTimeType = this.GetDateTimeType(prop, ref type);
                    this.cmp.Type = type;
                }
                if (((this.cmp.Type.ArrayRank == 0) && (this.cmp.Type.BaseType == new CodeTypeReference(this.PublicNamesUsed["BaseObjClass"].ToString()).BaseType)) || ((this.cmp.Type.ArrayRank > 0) && (this.cmp.Type.ArrayElementType.BaseType == new CodeTypeReference(this.PublicNamesUsed["BaseObjClass"].ToString()).BaseType)))
                {
                    this.bHasEmbeddedProperties = true;
                }
                propertyName = "Is" + this.PublicProperties[prop.Name].ToString() + "Null";
                property = new CodeMemberProperty {
                    Name = propertyName,
                    Attributes = MemberAttributes.Public | MemberAttributes.Final,
                    Type = new CodeTypeReference("System.Boolean")
                };
                this.caa = new CodeAttributeArgument();
                this.caa.Value = new CodePrimitiveExpression(true);
                this.cad = new CodeAttributeDeclaration();
                this.cad.Name = "Browsable";
                this.cad.Arguments.Add(this.caa);
                this.cmp.CustomAttributes = new CodeAttributeDeclarationCollection();
                this.cmp.CustomAttributes.Add(this.cad);
                this.caa = new CodeAttributeArgument();
                this.caa.Value = new CodePrimitiveExpression(false);
                this.cad = new CodeAttributeDeclaration();
                this.cad.Name = "Browsable";
                this.cad.Arguments.Add(this.caa);
                property.CustomAttributes = new CodeAttributeDeclarationCollection();
                property.CustomAttributes.Add(this.cad);
                this.caa = new CodeAttributeArgument();
                this.caa.Value = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("DesignerSerializationVisibility"), "Hidden");
                this.cad = new CodeAttributeDeclaration();
                this.cad.Name = "DesignerSerializationVisibility";
                this.cad.Arguments.Add(this.caa);
                this.cmp.CustomAttributes.Add(this.cad);
                property.CustomAttributes.Add(this.cad);
                this.cie = new CodeIndexerExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["CurrentObject"].ToString()), new CodeExpression[] { new CodePrimitiveExpression(prop.Name) });
                bool nullable = false;
                string str2 = this.ProcessPropertyQualifiers(prop, ref bRead, ref bWrite, ref bStatic, bDynamicClass, out nullable);
                if (bRead || bWrite)
                {
                    if (str2.Length != 0)
                    {
                        this.caa = new CodeAttributeArgument();
                        this.caa.Value = new CodePrimitiveExpression(str2);
                        this.cad = new CodeAttributeDeclaration();
                        this.cad.Name = "Description";
                        this.cad.Arguments.Add(this.caa);
                        this.cmp.CustomAttributes.Add(this.cad);
                    }
                    bool flag7 = this.GeneratePropertyHelperEnums(prop, this.PublicProperties[prop.Name].ToString(), nullable);
                    if (bRead)
                    {
                        if (IsPropertyValueType(prop.Type) && !prop.IsArray)
                        {
                            this.cis = new CodeConditionStatement();
                            this.cis.Condition = new CodeBinaryOperatorExpression(this.cie, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
                            this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));
                            this.cis.FalseStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));
                            property.GetStatements.Add(this.cis);
                            this.cc.Members.Add(property);
                            this.caa = new CodeAttributeArgument();
                            this.caa.Value = new CodeTypeOfExpression(this.PrivateNamesUsed["ConverterClass"].ToString());
                            this.cad = new CodeAttributeDeclaration();
                            this.cad.Name = this.PublicNamesUsed["TypeConverter"].ToString();
                            this.cad.Arguments.Add(this.caa);
                            this.cmp.CustomAttributes.Add(this.cad);
                            if (prop.Type != CimType.DateTime)
                            {
                                this.cis = new CodeConditionStatement();
                                this.cis.Condition = new CodeBinaryOperatorExpression(this.cie, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
                                if (flag7)
                                {
                                    if (prop.IsArray)
                                    {
                                        this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
                                    }
                                    else
                                    {
                                        this.cmie = new CodeMethodInvokeExpression();
                                        this.cmie.Method.TargetObject = new CodeTypeReferenceExpression("System.Convert");
                                        this.cmie.Parameters.Add(new CodePrimitiveExpression(prop.NullEnumValue));
                                        this.cmie.Method.MethodName = this.arrConvFuncName;
                                        this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(this.cmp.Type, this.cmie)));
                                    }
                                }
                                else
                                {
                                    this.cmie = new CodeMethodInvokeExpression();
                                    this.cmie.Parameters.Add(new CodePrimitiveExpression(prop.NullEnumValue));
                                    this.cmie.Method.MethodName = this.GetConversionFunction(prop.Type);
                                    this.cmie.Method.TargetObject = new CodeTypeReferenceExpression("System.Convert");
                                    if (prop.IsArray)
                                    {
                                        CodeExpression[] initializers = new CodeExpression[] { this.cmie };
                                        this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodeArrayCreateExpression(this.cmp.Type, initializers)));
                                    }
                                    else
                                    {
                                        this.cis.TrueStatements.Add(new CodeMethodReturnStatement(this.cmie));
                                    }
                                }
                                this.cmp.GetStatements.Add(this.cis);
                            }
                            this.cmm = new CodeMemberMethod();
                            this.cmm.Name = "ShouldSerialize" + this.PublicProperties[prop.Name].ToString();
                            this.cmm.Attributes = MemberAttributes.Private | MemberAttributes.Final;
                            this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
                            CodeConditionStatement statement = new CodeConditionStatement {
                                Condition = new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), propertyName), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(false))
                            };
                            statement.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));
                            this.cmm.Statements.Add(statement);
                            this.cmm.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));
                            this.cc.Members.Add(this.cmm);
                        }
                        if (prop.Type == CimType.Reference)
                        {
                            this.GenerateCodeForRefAndDateTimeTypes(this.cie, prop.IsArray, this.cmp.GetStatements, this.PublicNamesUsed["PathClass"].ToString(), null, false);
                        }
                        else if (prop.Type == CimType.DateTime)
                        {
                            if (dateTimeType)
                            {
                                this.GenerateCodeForRefAndDateTimeTypes(this.cie, prop.IsArray, this.cmp.GetStatements, "System.TimeSpan", null, false);
                            }
                            else
                            {
                                this.GenerateCodeForRefAndDateTimeTypes(this.cie, prop.IsArray, this.cmp.GetStatements, "System.DateTime", null, false);
                            }
                        }
                        else if (flag7)
                        {
                            if (prop.IsArray)
                            {
                                this.AddGetStatementsForEnumArray(this.cie, this.cmp);
                            }
                            else
                            {
                                this.cmie = new CodeMethodInvokeExpression();
                                this.cmie.Method.TargetObject = new CodeTypeReferenceExpression("System.Convert");
                                this.cmie.Parameters.Add(this.cie);
                                this.cmie.Method.MethodName = this.arrConvFuncName;
                                this.cmp.GetStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(this.cmp.Type, this.cmie)));
                            }
                        }
                        else
                        {
                            this.cmp.GetStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(this.cmp.Type, this.cie)));
                        }
                    }
                    if (bWrite)
                    {
                        if (nullable)
                        {
                            method = new CodeMemberMethod {
                                Name = "Reset" + this.PublicProperties[prop.Name].ToString(),
                                Attributes = MemberAttributes.Private | MemberAttributes.Final
                            };
                            method.Statements.Add(new CodeAssignStatement(this.cie, new CodePrimitiveExpression(null)));
                        }
                        if (prop.Type == CimType.Reference)
                        {
                            this.AddPropertySet(this.cie, prop.IsArray, this.cmp.SetStatements, this.PublicNamesUsed["PathClass"].ToString(), null);
                        }
                        else if (prop.Type == CimType.DateTime)
                        {
                            if (dateTimeType)
                            {
                                this.AddPropertySet(this.cie, prop.IsArray, this.cmp.SetStatements, "System.TimeSpan", null);
                            }
                            else
                            {
                                this.AddPropertySet(this.cie, prop.IsArray, this.cmp.SetStatements, "System.DateTime", null);
                            }
                        }
                        else if (flag7 && nullable)
                        {
                            CodeConditionStatement statement2 = new CodeConditionStatement();
                            if (prop.IsArray)
                            {
                                statement2.Condition = new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(new CodeTypeReference(this.PublicProperties[prop.Name].ToString() + "Values")), "NULL_ENUM_VALUE"), CodeBinaryOperatorType.ValueEquality, new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("value"), new CodeExpression[] { new CodePrimitiveExpression(0) }));
                            }
                            else
                            {
                                statement2.Condition = new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(new CodeTypeReference(this.PublicProperties[prop.Name].ToString() + "Values")), "NULL_ENUM_VALUE"), CodeBinaryOperatorType.ValueEquality, new CodeSnippetExpression("value"));
                            }
                            statement2.TrueStatements.Add(new CodeAssignStatement(this.cie, new CodePrimitiveExpression(null)));
                            statement2.FalseStatements.Add(new CodeAssignStatement(this.cie, new CodeSnippetExpression("value")));
                            this.cmp.SetStatements.Add(statement2);
                        }
                        else
                        {
                            this.cmp.SetStatements.Add(new CodeAssignStatement(this.cie, new CodeSnippetExpression("value")));
                        }
                        this.cmie = new CodeMethodInvokeExpression();
                        this.cmie.Method.TargetObject = new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString());
                        this.cmie.Method.MethodName = "Put";
                        this.cboe = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["AutoCommitProperty"].ToString()), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(true));
                        CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString()), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(false));
                        CodeBinaryOperatorExpression expression2 = new CodeBinaryOperatorExpression {
                            Right = this.cboe,
                            Left = expression,
                            Operator = CodeBinaryOperatorType.BooleanAnd
                        };
                        this.cis = new CodeConditionStatement();
                        this.cis.Condition = expression2;
                        this.cis.TrueStatements.Add(new CodeExpressionStatement(this.cmie));
                        this.cmp.SetStatements.Add(this.cis);
                        if (nullable)
                        {
                            method.Statements.Add(this.cis);
                        }
                    }
                    this.cc.Members.Add(this.cmp);
                    if (nullable & bWrite)
                    {
                        this.cc.Members.Add(method);
                    }
                }
            }
            this.GenerateCommitMethod();
        }

        private bool GeneratePropertyHelperEnums(PropertyData prop, string strPropertyName, bool bNullable)
        {
            bool flag = false;
            bool flag2 = false;
            string name = this.ResolveCollision(strPropertyName + "Values", true);
            if ((this.Values.Count > 0) && ((this.ValueMap.Count == 0) || (this.ValueMap.Count == this.Values.Count)))
            {
                if (this.ValueMap.Count == 0)
                {
                    flag2 = true;
                }
                this.EnumObj = new CodeTypeDeclaration(name);
                if (prop.IsArray)
                {
                    this.cmp.Type = new CodeTypeReference(name, 1);
                }
                else
                {
                    this.cmp.Type = new CodeTypeReference(name);
                }
                this.EnumObj.IsEnum = true;
                this.EnumObj.TypeAttributes = TypeAttributes.Public;
                long num = 0L;
                for (int i = 0; i < this.Values.Count; i++)
                {
                    this.cmf = new CodeMemberField();
                    this.cmf.Name = this.Values[i].ToString();
                    if (this.ValueMap.Count > 0)
                    {
                        this.cmf.InitExpression = new CodePrimitiveExpression(this.ValueMap[i]);
                        long num3 = System.Convert.ToInt64(this.ValueMap[i], (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(ulong)));
                        if (num3 > num)
                        {
                            num = num3;
                        }
                        if (!flag2 && (System.Convert.ToInt64(this.ValueMap[i], (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(ulong))) == 0L))
                        {
                            flag2 = true;
                        }
                    }
                    else
                    {
                        this.cmf.InitExpression = new CodePrimitiveExpression(i);
                        if (i > num)
                        {
                            num = i;
                        }
                    }
                    this.EnumObj.Members.Add(this.cmf);
                }
                if (bNullable && !flag2)
                {
                    this.cmf = new CodeMemberField();
                    this.cmf.Name = "NULL_ENUM_VALUE";
                    this.cmf.InitExpression = new CodePrimitiveExpression(0);
                    this.EnumObj.Members.Add(this.cmf);
                    prop.NullEnumValue = 0L;
                }
                else if (bNullable && flag2)
                {
                    this.cmf = new CodeMemberField();
                    this.cmf.Name = "NULL_ENUM_VALUE";
                    this.cmf.InitExpression = new CodePrimitiveExpression((int) (num + 1L));
                    this.EnumObj.Members.Add(this.cmf);
                    prop.NullEnumValue = (int) (num + 1L);
                }
                else if (!bNullable && !flag2)
                {
                    this.cmf = new CodeMemberField();
                    this.cmf.Name = "INVALID_ENUM_VALUE";
                    this.cmf.InitExpression = new CodePrimitiveExpression(0);
                    this.EnumObj.Members.Add(this.cmf);
                    prop.NullEnumValue = 0L;
                }
                this.cc.Members.Add(this.EnumObj);
                flag = true;
            }
            this.Values.Clear();
            this.ValueMap.Clear();
            flag2 = false;
            if ((this.BitValues.Count > 0) && ((this.BitMap.Count == 0) || (this.BitMap.Count == this.BitValues.Count)))
            {
                if (this.BitMap.Count == 0)
                {
                    flag2 = true;
                }
                this.EnumObj = new CodeTypeDeclaration(name);
                if (prop.IsArray)
                {
                    this.cmp.Type = new CodeTypeReference(name, 1);
                }
                else
                {
                    this.cmp.Type = new CodeTypeReference(name);
                }
                this.EnumObj.IsEnum = true;
                this.EnumObj.TypeAttributes = TypeAttributes.Public;
                int num4 = 1;
                long num5 = 0L;
                for (int j = 0; j < this.BitValues.Count; j++)
                {
                    this.cmf = new CodeMemberField();
                    this.cmf.Name = this.BitValues[j].ToString();
                    if (this.BitMap.Count > 0)
                    {
                        this.cmf.InitExpression = new CodePrimitiveExpression(this.BitMap[j]);
                        long num7 = System.Convert.ToInt64(this.BitMap[j], (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(ulong)));
                        if (num7 > num5)
                        {
                            num5 = num7;
                        }
                    }
                    else
                    {
                        this.cmf.InitExpression = new CodePrimitiveExpression(num4);
                        if (num4 > num5)
                        {
                            num5 = num4;
                        }
                        num4 = num4 << 1;
                    }
                    if (!flag2 && (System.Convert.ToInt64(this.BitMap[j], (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(ulong))) == 0L))
                    {
                        flag2 = true;
                    }
                    this.EnumObj.Members.Add(this.cmf);
                }
                if (bNullable && !flag2)
                {
                    this.cmf = new CodeMemberField();
                    this.cmf.Name = "NULL_ENUM_VALUE";
                    this.cmf.InitExpression = new CodePrimitiveExpression(0);
                    this.EnumObj.Members.Add(this.cmf);
                    prop.NullEnumValue = 0L;
                }
                else if (bNullable && flag2)
                {
                    this.cmf = new CodeMemberField();
                    this.cmf.Name = "NULL_ENUM_VALUE";
                    if (this.BitValues.Count > 30)
                    {
                        num5 += 1L;
                    }
                    else
                    {
                        num5 = num5 << 1;
                    }
                    this.cmf.InitExpression = new CodePrimitiveExpression((int) num5);
                    this.EnumObj.Members.Add(this.cmf);
                    prop.NullEnumValue = (int) num5;
                }
                else if (!bNullable && !flag2)
                {
                    this.cmf = new CodeMemberField();
                    this.cmf.Name = "INVALID_ENUM_VALUE";
                    this.cmf.InitExpression = new CodePrimitiveExpression(0);
                    this.EnumObj.Members.Add(this.cmf);
                    prop.NullEnumValue = 0L;
                }
                this.cc.Members.Add(this.EnumObj);
                flag = true;
            }
            this.BitValues.Clear();
            this.BitMap.Clear();
            return flag;
        }

        private void GeneratePublicProperty(string propName, string propType, CodeExpression Value, bool isBrowsable, string Comment, bool isStatic)
        {
            this.cmp = new CodeMemberProperty();
            this.cmp.Name = propName;
            this.cmp.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            this.cmp.Type = new CodeTypeReference(propType);
            if (isStatic)
            {
                this.cmp.Attributes |= MemberAttributes.Static;
            }
            this.caa = new CodeAttributeArgument();
            this.caa.Value = new CodePrimitiveExpression(isBrowsable);
            this.cad = new CodeAttributeDeclaration();
            this.cad.Name = "Browsable";
            this.cad.Arguments.Add(this.caa);
            this.cmp.CustomAttributes = new CodeAttributeDeclarationCollection();
            this.cmp.CustomAttributes.Add(this.cad);
            if (IsDesignerSerializationVisibilityToBeSet(propName))
            {
                this.caa = new CodeAttributeArgument();
                this.caa.Value = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("DesignerSerializationVisibility"), "Hidden");
                this.cad = new CodeAttributeDeclaration();
                this.cad.Name = "DesignerSerializationVisibility";
                this.cad.Arguments.Add(this.caa);
                this.cmp.CustomAttributes.Add(this.cad);
            }
            this.cmp.GetStatements.Add(new CodeMethodReturnStatement(Value));
            this.cmp.SetStatements.Add(new CodeAssignStatement(Value, new CodeSnippetExpression("value")));
            this.cc.Members.Add(this.cmp);
            if ((Comment != null) && (Comment.Length != 0))
            {
                this.cmp.Comments.Add(new CodeCommentStatement(Comment));
            }
        }

        private void GeneratePublicReadOnlyProperty(string propName, string propType, object propValue, bool isLiteral, bool isBrowsable, string Comment)
        {
            this.cmp = new CodeMemberProperty();
            this.cmp.Name = propName;
            this.cmp.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            this.cmp.Type = new CodeTypeReference(propType);
            this.caa = new CodeAttributeArgument();
            this.caa.Value = new CodePrimitiveExpression(isBrowsable);
            this.cad = new CodeAttributeDeclaration();
            this.cad.Name = "Browsable";
            this.cad.Arguments.Add(this.caa);
            this.cmp.CustomAttributes = new CodeAttributeDeclarationCollection();
            this.cmp.CustomAttributes.Add(this.cad);
            this.caa = new CodeAttributeArgument();
            this.caa.Value = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("DesignerSerializationVisibility"), "Hidden");
            this.cad = new CodeAttributeDeclaration();
            this.cad.Name = "DesignerSerializationVisibility";
            this.cad.Arguments.Add(this.caa);
            this.cmp.CustomAttributes.Add(this.cad);
            if (isLiteral)
            {
                this.cmp.GetStatements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression(propValue.ToString())));
            }
            else
            {
                this.cmp.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(propValue)));
            }
            this.cc.Members.Add(this.cmp);
            if ((Comment != null) && (Comment.Length != 0))
            {
                this.cmp.Comments.Add(new CodeCommentStatement(Comment));
            }
        }

        private void GenerateScopeProperty()
        {
            this.cmp = new CodeMemberProperty();
            this.cmp.Name = this.PublicNamesUsed["ScopeProperty"].ToString();
            this.cmp.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            this.cmp.Type = new CodeTypeReference(this.PublicNamesUsed["ScopeClass"].ToString());
            this.caa = new CodeAttributeArgument();
            this.caa.Value = new CodePrimitiveExpression(true);
            this.cad = new CodeAttributeDeclaration();
            this.cad.Name = "Browsable";
            this.cad.Arguments.Add(this.caa);
            this.cmp.CustomAttributes = new CodeAttributeDeclarationCollection();
            this.cmp.CustomAttributes.Add(this.cad);
            if (IsDesignerSerializationVisibilityToBeSet(this.PublicNamesUsed["ScopeProperty"].ToString()))
            {
                this.caa = new CodeAttributeArgument();
                this.caa.Value = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("DesignerSerializationVisibility"), "Hidden");
                this.cad = new CodeAttributeDeclaration();
                this.cad.Name = "DesignerSerializationVisibility";
                this.cad.Arguments.Add(this.caa);
                this.cmp.CustomAttributes.Add(this.cad);
            }
            this.cis = new CodeConditionStatement();
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString());
            this.cboe.Right = new CodePrimitiveExpression(false);
            this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
            this.cis.Condition = this.cboe;
            CodeExpression expression = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()), "Scope");
            this.cis.TrueStatements.Add(new CodeMethodReturnStatement(expression));
            this.cis.FalseStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
            this.cmp.GetStatements.Add(this.cis);
            this.cis = new CodeConditionStatement();
            this.cboe = new CodeBinaryOperatorExpression();
            this.cboe.Left = new CodeVariableReferenceExpression(this.PrivateNamesUsed["IsEmbedded"].ToString());
            this.cboe.Right = new CodePrimitiveExpression(false);
            this.cboe.Operator = CodeBinaryOperatorType.ValueEquality;
            this.cis.Condition = this.cboe;
            this.cis.TrueStatements.Add(new CodeAssignStatement(expression, new CodeSnippetExpression("value")));
            this.cmp.SetStatements.Add(this.cis);
            this.cc.Members.Add(this.cmp);
            this.cmp.Comments.Add(new CodeCommentStatement(GetString("COMMENT_MGMTSCOPE")));
        }

        private CodeTypeDeclaration GenerateSystemPropertiesClass()
        {
            CodeTypeDeclaration declaration = new CodeTypeDeclaration(this.PublicNamesUsed["SystemPropertiesClass"].ToString()) {
                TypeAttributes = TypeAttributes.NestedPublic
            };
            this.cctor = new CodeConstructor();
            this.cctor.Attributes = MemberAttributes.Public;
            this.cpde = new CodeParameterDeclarationExpression();
            this.cpde.Type = new CodeTypeReference(this.PublicNamesUsed["BaseObjClass"].ToString());
            this.cpde.Name = "ManagedObject";
            this.cctor.Parameters.Add(this.cpde);
            this.cctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()), new CodeVariableReferenceExpression("ManagedObject")));
            declaration.Members.Add(this.cctor);
            this.caa = new CodeAttributeArgument();
            this.caa.Value = new CodeTypeOfExpression(typeof(ExpandableObjectConverter));
            this.cad = new CodeAttributeDeclaration();
            this.cad.Name = this.PublicNamesUsed["TypeConverter"].ToString();
            this.cad.Arguments.Add(this.caa);
            declaration.CustomAttributes.Add(this.cad);
            int index = 0;
            foreach (PropertyData data in this.classobj.SystemProperties)
            {
                this.cmp = new CodeMemberProperty();
                this.caa = new CodeAttributeArgument();
                this.caa.Value = new CodePrimitiveExpression(true);
                this.cad = new CodeAttributeDeclaration();
                this.cad.Name = "Browsable";
                this.cad.Arguments.Add(this.caa);
                this.cmp.CustomAttributes = new CodeAttributeDeclarationCollection();
                this.cmp.CustomAttributes.Add(this.cad);
                char[] chArray = data.Name.ToCharArray();
                index = 0;
                while (index < chArray.Length)
                {
                    if (char.IsLetterOrDigit(chArray[index]))
                    {
                        break;
                    }
                    index++;
                }
                if (index == chArray.Length)
                {
                    index = 0;
                }
                char[] chArray2 = new char[chArray.Length - index];
                for (int i = index; i < chArray.Length; i++)
                {
                    chArray2[i - index] = chArray[i];
                }
                this.cmp.Name = new string(chArray2).ToUpper(CultureInfo.InvariantCulture);
                this.cmp.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                this.cmp.Type = this.ConvertCIMType(data.Type, data.IsArray);
                this.cie = new CodeIndexerExpression(new CodeVariableReferenceExpression(this.PrivateNamesUsed["LateBoundObject"].ToString()), new CodeExpression[] { new CodePrimitiveExpression(data.Name) });
                this.cmp.GetStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(this.cmp.Type, this.cie)));
                declaration.Members.Add(this.cmp);
            }
            this.cf = new CodeMemberField();
            this.cf.Name = this.PrivateNamesUsed["LateBoundObject"].ToString();
            this.cf.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            this.cf.Type = new CodeTypeReference(this.PublicNamesUsed["BaseObjClass"].ToString());
            declaration.Members.Add(this.cf);
            declaration.Comments.Add(new CodeCommentStatement(GetString("COMMENT_SYSPROPCLASS")));
            return declaration;
        }

        private void GenerateTimeSpanConversionFunction()
        {
            this.AddToTimeSpanFunction();
            this.AddToDMTFTimeIntervalFunction();
        }

        private CodeTypeDeclaration GenerateTypeConverterClass()
        {
            string type = "System.ComponentModel.ITypeDescriptorContext";
            string name = "context";
            string str3 = "destinationType";
            string str4 = "value";
            string str5 = "System.Globalization.CultureInfo";
            string str6 = "culture";
            string str7 = "System.Collections.IDictionary";
            string str8 = "dictionary";
            string typeName = "PropertyDescriptorCollection";
            string str10 = "attributeVar";
            string variableName = "inBaseType";
            string str12 = "baseConverter";
            string str13 = "baseType";
            string str14 = "TypeDescriptor";
            string str15 = "srcType";
            CodeTypeDeclaration declaration = new CodeTypeDeclaration(this.PrivateNamesUsed["ConverterClass"].ToString());
            declaration.BaseTypes.Add(this.PublicNamesUsed["TypeConverter"].ToString());
            this.cf = new CodeMemberField();
            this.cf.Name = str12;
            this.cf.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            this.cf.Type = new CodeTypeReference(this.PublicNamesUsed["TypeConverter"].ToString());
            declaration.Members.Add(this.cf);
            this.cf = new CodeMemberField();
            this.cf.Name = str13;
            this.cf.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            this.cf.Type = new CodeTypeReference(this.PublicNamesUsed["Type"].ToString());
            declaration.Members.Add(this.cf);
            this.cctor = new CodeConstructor();
            this.cctor.Attributes = MemberAttributes.Public;
            this.cpde = new CodeParameterDeclarationExpression();
            this.cpde.Name = variableName;
            this.cpde.Type = new CodeTypeReference("System.Type");
            this.cctor.Parameters.Add(this.cpde);
            this.cmie = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(str14), "GetConverter", new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(variableName));
            this.cctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str12), this.cmie));
            this.cctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(str13), new CodeVariableReferenceExpression(variableName)));
            declaration.Members.Add(this.cctor);
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Overloaded | MemberAttributes.Override;
            this.cmm.Name = "CanConvertFrom";
            this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(type, name));
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.Type", str15));
            this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str12), "CanConvertFrom", new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str15));
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
            declaration.Members.Add(this.cmm);
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Overloaded | MemberAttributes.Override;
            this.cmm.Name = "CanConvertTo";
            this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(type, name));
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.Type", str3));
            this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str12), "CanConvertTo", new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str3));
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
            declaration.Members.Add(this.cmm);
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Overloaded | MemberAttributes.Override;
            this.cmm.Name = "ConvertFrom";
            this.cmm.ReturnType = new CodeTypeReference("System.Object");
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(type, name));
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(str5, str6));
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.Object"), str4));
            this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str12), "ConvertFrom", new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str6));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str4));
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
            declaration.Members.Add(this.cmm);
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Overloaded | MemberAttributes.Override;
            this.cmm.ReturnType = new CodeTypeReference("System.Object");
            this.cmm.Name = "CreateInstance";
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(type, name));
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(str7, str8));
            this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str12), "CreateInstance", new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str8));
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
            declaration.Members.Add(this.cmm);
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Overloaded | MemberAttributes.Override;
            this.cmm.Name = "GetCreateInstanceSupported";
            this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(type, name));
            this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str12), "GetCreateInstanceSupported", new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
            declaration.Members.Add(this.cmm);
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Overloaded | MemberAttributes.Override;
            this.cmm.Name = "GetProperties";
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(type, name));
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.Object"), str4));
            CodeTypeReference reference = new CodeTypeReference(new CodeTypeReference("System.Attribute"), 1);
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(reference, str10));
            this.cmm.ReturnType = new CodeTypeReference(typeName);
            this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str12), "GetProperties", new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str4));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str10));
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
            declaration.Members.Add(this.cmm);
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Overloaded | MemberAttributes.Override;
            this.cmm.Name = "GetPropertiesSupported";
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(type, name));
            this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
            this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str12), "GetPropertiesSupported", new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
            declaration.Members.Add(this.cmm);
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Overloaded | MemberAttributes.Override;
            this.cmm.Name = "GetStandardValues";
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(type, name));
            this.cmm.ReturnType = new CodeTypeReference("System.ComponentModel.TypeConverter.StandardValuesCollection");
            this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str12), "GetStandardValues", new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
            declaration.Members.Add(this.cmm);
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Overloaded | MemberAttributes.Override;
            this.cmm.Name = "GetStandardValuesExclusive";
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(type, name));
            this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
            this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str12), "GetStandardValuesExclusive", new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
            declaration.Members.Add(this.cmm);
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Overloaded | MemberAttributes.Override;
            this.cmm.Name = "GetStandardValuesSupported";
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(type, name));
            this.cmm.ReturnType = new CodeTypeReference("System.Boolean");
            this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str12), "GetStandardValuesSupported", new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cmm.Statements.Add(new CodeMethodReturnStatement(this.cmie));
            declaration.Members.Add(this.cmm);
            this.cmm = new CodeMemberMethod();
            this.cmm.Attributes = MemberAttributes.Public | MemberAttributes.Overloaded | MemberAttributes.Override;
            this.cmm.Name = "ConvertTo";
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(type, name));
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(str5, str6));
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.Object"), str4));
            this.cmm.Parameters.Add(new CodeParameterDeclarationExpression("System.Type", str3));
            this.cmm.ReturnType = new CodeTypeReference("System.Object");
            this.cmie = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(str12), "ConvertTo", new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(name));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str6));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str4));
            this.cmie.Parameters.Add(new CodeVariableReferenceExpression(str3));
            CodeMethodReturnStatement statement = new CodeMethodReturnStatement(this.cmie);
            this.cis = new CodeConditionStatement();
            CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression {
                Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str13), "BaseType"),
                Right = new CodeTypeOfExpression(typeof(Enum)),
                Operator = CodeBinaryOperatorType.IdentityEquality
            };
            this.cis.Condition = expression;
            CodeBinaryOperatorExpression condition = new CodeBinaryOperatorExpression {
                Left = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("value"), "GetType", new CodeExpression[0]),
                Right = new CodeVariableReferenceExpression("destinationType"),
                Operator = CodeBinaryOperatorType.IdentityEquality
            };
            this.cis.TrueStatements.Add(new CodeConditionStatement(condition, new CodeStatement[] { new CodeMethodReturnStatement(new CodeVariableReferenceExpression("value")) }));
            CodeBinaryOperatorExpression expression3 = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("value"), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
            CodeBinaryOperatorExpression expression4 = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(name), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
            CodeBinaryOperatorExpression expression5 = new CodeBinaryOperatorExpression {
                Left = expression3,
                Right = expression4,
                Operator = CodeBinaryOperatorType.BooleanAnd
            };
            this.cmie = new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(name), "PropertyDescriptor"), "ShouldSerializeValue", new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(name), "Instance"));
            CodeBinaryOperatorExpression expression6 = new CodeBinaryOperatorExpression(this.cmie, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(false));
            CodeBinaryOperatorExpression expression7 = new CodeBinaryOperatorExpression {
                Left = expression5,
                Right = expression6,
                Operator = CodeBinaryOperatorType.BooleanAnd
            };
            this.cis.TrueStatements.Add(new CodeConditionStatement(expression7, new CodeStatement[] { new CodeMethodReturnStatement(new CodeSnippetExpression(" \"NULL_ENUM_VALUE\" ")) }));
            this.cis.TrueStatements.Add(statement);
            this.cmm.Statements.Add(this.cis);
            this.cis = new CodeConditionStatement();
            expression = new CodeBinaryOperatorExpression {
                Left = new CodeVariableReferenceExpression(str13),
                Right = new CodeTypeOfExpression(this.PublicNamesUsed["Boolean"].ToString()),
                Operator = CodeBinaryOperatorType.IdentityEquality
            };
            condition = new CodeBinaryOperatorExpression {
                Left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str13), "BaseType"),
                Right = new CodeTypeOfExpression(this.PublicNamesUsed["ValueType"].ToString()),
                Operator = CodeBinaryOperatorType.IdentityEquality
            };
            expression3 = new CodeBinaryOperatorExpression {
                Left = expression,
                Right = condition,
                Operator = CodeBinaryOperatorType.BooleanAnd
            };
            this.cis.Condition = expression3;
            expression3 = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("value"), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
            expression4 = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(name), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
            expression5 = new CodeBinaryOperatorExpression {
                Left = expression3,
                Right = expression4,
                Operator = CodeBinaryOperatorType.BooleanAnd
            };
            this.cmie = new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(name), "PropertyDescriptor"), "ShouldSerializeValue", new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(name), "Instance"));
            expression6 = new CodeBinaryOperatorExpression(this.cmie, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(false));
            expression7 = new CodeBinaryOperatorExpression {
                Left = expression5,
                Right = expression6,
                Operator = CodeBinaryOperatorType.BooleanAnd
            };
            this.cis.TrueStatements.Add(new CodeConditionStatement(expression7, new CodeStatement[] { new CodeMethodReturnStatement(new CodePrimitiveExpression("")) }));
            this.cis.TrueStatements.Add(statement);
            this.cmm.Statements.Add(this.cis);
            this.cis = new CodeConditionStatement();
            expression = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(name), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
            this.cmie = new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(name), "PropertyDescriptor"), "ShouldSerializeValue", new CodeExpression[0]);
            this.cmie.Parameters.Add(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(name), "Instance"));
            condition = new CodeBinaryOperatorExpression(this.cmie, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(false));
            expression3 = new CodeBinaryOperatorExpression {
                Left = expression,
                Right = condition,
                Operator = CodeBinaryOperatorType.BooleanAnd
            };
            this.cis.Condition = expression3;
            this.cis.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression("")));
            this.cmm.Statements.Add(this.cis);
            this.cmm.Statements.Add(statement);
            declaration.Members.Add(this.cmm);
            declaration.Comments.Add(new CodeCommentStatement(GetString("COMMENT_PROPTYPECONVERTER")));
            return declaration;
        }

        private CodeTypeDeclaration GetCodeTypeDeclarationForClass(bool bIncludeSystemClassinClassDef)
        {
            this.cc = new CodeTypeDeclaration(this.PrivateNamesUsed["GeneratedClassName"].ToString());
            this.cc.BaseTypes.Add(new CodeTypeReference(this.PrivateNamesUsed["ComponentClass"].ToString()));
            this.AddClassComments(this.cc);
            this.GeneratePublicReadOnlyProperty(this.PublicNamesUsed["NamespaceProperty"].ToString(), "System.String", this.OriginalNamespace, false, true, GetString("COMMENT_ORIGNAMESPACE"));
            this.GeneratePrivateMember(this.PrivateNamesUsed["CreationWmiNamespace"].ToString(), "System.String", new CodePrimitiveExpression(this.OriginalNamespace), true, GetString("COMMENT_CREATEDWMINAMESPACE"));
            this.GenerateClassNameProperty();
            this.GeneratePrivateMember(this.PrivateNamesUsed["CreationClassName"].ToString(), "System.String", new CodePrimitiveExpression(this.OriginalClassName), true, GetString("COMMENT_CREATEDCLASS"));
            this.GeneratePublicReadOnlyProperty(this.PublicNamesUsed["SystemPropertiesProperty"].ToString(), this.PublicNamesUsed["SystemPropertiesClass"].ToString(), this.PrivateNamesUsed["SystemPropertiesObject"].ToString(), true, true, GetString("COMMENT_SYSOBJECT"));
            this.GeneratePublicReadOnlyProperty(this.PublicNamesUsed["LateBoundObjectProperty"].ToString(), this.PublicNamesUsed["BaseObjClass"].ToString(), this.PrivateNamesUsed["CurrentObject"].ToString(), true, false, GetString("COMMENT_LATEBOUNDPROP"));
            this.GenerateScopeProperty();
            this.GeneratePublicProperty(this.PublicNamesUsed["AutoCommitProperty"].ToString(), "System.Boolean", new CodeSnippetExpression(this.PrivateNamesUsed["AutoCommitProperty"].ToString()), false, GetString("COMMENT_AUTOCOMMITPROP"), false);
            this.GeneratePathProperty();
            this.GeneratePrivateMember(this.PrivateNamesUsed["statMgmtScope"].ToString(), this.PublicNamesUsed["ScopeClass"].ToString(), new CodePrimitiveExpression(null), true, GetString("COMMENT_STATICMANAGEMENTSCOPE"));
            this.GeneratePublicProperty(this.PrivateNamesUsed["staticScope"].ToString(), this.PublicNamesUsed["ScopeClass"].ToString(), new CodeVariableReferenceExpression(this.PrivateNamesUsed["statMgmtScope"].ToString()), true, GetString("COMMENT_STATICSCOPEPROPERTY"), true);
            this.GenerateIfClassvalidFunction();
            this.GenerateProperties();
            this.GenerateMethodToInitializeVariables();
            this.GenerateConstructPath();
            this.GenerateDefaultConstructor();
            this.GenerateInitializeObject();
            if (this.bSingletonClass)
            {
                this.GenerateConstructorWithScope();
                this.GenerateConstructorWithOptions();
                this.GenerateConstructorWithScopeOptions();
            }
            else
            {
                this.GenerateConstructorWithKeys();
                this.GenerateConstructorWithScopeKeys();
                this.GenerateConstructorWithPathOptions();
                this.GenerateConstructorWithScopePath();
                this.GenerateGetInstancesWithNoParameters();
                this.GenerateGetInstancesWithCondition();
                this.GenerateGetInstancesWithProperties();
                this.GenerateGetInstancesWithWhereProperties();
                this.GenerateGetInstancesWithScope();
                this.GenerateGetInstancesWithScopeCondition();
                this.GenerateGetInstancesWithScopeProperties();
                this.GenerateGetInstancesWithScopeWhereProperties();
                this.GenerateCollectionClass();
            }
            this.GenerateConstructorWithPath();
            this.GenerateConstructorWithScopePathOptions();
            this.GenarateConstructorWithLateBound();
            this.GenarateConstructorWithLateBoundForEmbedded();
            this.GenerateCreateInstance();
            this.GenerateDeleteInstance();
            this.GenerateMethods();
            this.GeneratePrivateMember(this.PrivateNamesUsed["SystemPropertiesObject"].ToString(), this.PublicNamesUsed["SystemPropertiesClass"].ToString(), null);
            this.GeneratePrivateMember(this.PrivateNamesUsed["LateBoundObject"].ToString(), this.PublicNamesUsed["LateBoundClass"].ToString(), GetString("COMMENT_LATEBOUNDOBJ"));
            this.GeneratePrivateMember(this.PrivateNamesUsed["AutoCommitProperty"].ToString(), "System.Boolean", new CodePrimitiveExpression(true), false, GetString("COMMENT_PRIVAUTOCOMMIT"));
            this.GeneratePrivateMember(this.PrivateNamesUsed["EmbeddedObject"].ToString(), this.PublicNamesUsed["BaseObjClass"].ToString(), GetString("COMMENT_EMBEDDEDOBJ"));
            this.GeneratePrivateMember(this.PrivateNamesUsed["CurrentObject"].ToString(), this.PublicNamesUsed["BaseObjClass"].ToString(), GetString("COMMENT_CURRENTOBJ"));
            this.GeneratePrivateMember(this.PrivateNamesUsed["IsEmbedded"].ToString(), "System.Boolean", new CodePrimitiveExpression(false), false, GetString("COMMENT_FLAGFOREMBEDDED"));
            this.cc.Members.Add(this.GenerateTypeConverterClass());
            if (bIncludeSystemClassinClassDef)
            {
                this.cc.Members.Add(this.GenerateSystemPropertiesClass());
            }
            if (this.bHasEmbeddedProperties)
            {
                this.AddCommentsForEmbeddedProperties();
            }
            this.cc.Comments.Add(new CodeCommentStatement(GetString("COMMENT_CLASSBEGIN") + this.OriginalClassName));
            return this.cc;
        }

        private string GetConversionFunction(CimType cimType)
        {
            string str = string.Empty;
            switch (cimType)
            {
                case CimType.SInt16:
                    return "ToInt16";

                case CimType.SInt32:
                    return "ToInt32";

                case CimType.Real32:
                    return "ToSingle";

                case CimType.Real64:
                    return "ToDouble";

                case (CimType.Real32 | CimType.SInt16):
                case (CimType.Real64 | CimType.SInt16):
                case ((CimType) 9):
                case (CimType.String | CimType.SInt16):
                case (CimType.String | CimType.Real32):
                case CimType.Object:
                case (CimType.String | CimType.Real32 | CimType.SInt16):
                case (CimType.Object | CimType.SInt16):
                    return str;

                case CimType.String:
                    return "ToString";

                case CimType.Boolean:
                    return "ToBoolean";

                case CimType.SInt8:
                    return "ToSByte";

                case CimType.UInt8:
                    return "ToByte";

                case CimType.UInt16:
                    if (this.bUnsignedSupported)
                    {
                        return "ToUInt16";
                    }
                    return "ToInt16";

                case CimType.UInt32:
                    if (this.bUnsignedSupported)
                    {
                        return "ToUInt32";
                    }
                    return "ToInt32";

                case CimType.SInt64:
                    return "ToInt64";

                case CimType.UInt64:
                    if (this.bUnsignedSupported)
                    {
                        return "ToUInt64";
                    }
                    return "ToInt64";

                case CimType.Char16:
                    return "ToChar";
            }
            return str;
        }

        private bool GetDateTimeType(PropertyData prop, ref CodeTypeReference codeType)
        {
            bool flag = false;
            codeType = null;
            if (prop.IsArray)
            {
                codeType = new CodeTypeReference("System.DateTime", 1);
            }
            else
            {
                codeType = new CodeTypeReference("System.DateTime");
            }
            try
            {
                if (string.Compare(prop.Qualifiers["SubType"].Value.ToString(), "interval", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    flag = true;
                    if (prop.IsArray)
                    {
                        codeType = new CodeTypeReference("System.TimeSpan", 1);
                    }
                    else
                    {
                        codeType = new CodeTypeReference("System.TimeSpan");
                    }
                }
            }
            catch (ManagementException)
            {
            }
            if (flag)
            {
                if (!this.bTimeSpanConversionFunctionsAdded)
                {
                    this.cc.Comments.Add(new CodeCommentStatement(GetString("COMMENT_TIMESPANCONVFUNC")));
                    this.bTimeSpanConversionFunctionsAdded = true;
                    this.GenerateTimeSpanConversionFunction();
                }
                return flag;
            }
            if (!this.bDateConversionFunctionsAdded)
            {
                this.cc.Comments.Add(new CodeCommentStatement(GetString("COMMENT_DATECONVFUNC")));
                this.bDateConversionFunctionsAdded = true;
                this.GenerateDateTimeConversionFunction();
            }
            return flag;
        }

        private static string GetString(string strToGet)
        {
            return RC.GetString(strToGet);
        }

        private void GetUnsignedSupport(CodeLanguage Language)
        {
            switch (Language)
            {
                case CodeLanguage.CSharp:
                    this.bUnsignedSupported = true;
                    break;

                case CodeLanguage.JScript:
                case CodeLanguage.VB:
                    break;

                default:
                    return;
            }
        }

        private void InitializeClassObject()
        {
            if (this.classobj == null)
            {
                ManagementPath path;
                if (this.OriginalPath.Length != 0)
                {
                    path = new ManagementPath(this.OriginalPath);
                }
                else
                {
                    path = new ManagementPath();
                    if (this.OriginalServer.Length != 0)
                    {
                        path.Server = this.OriginalServer;
                    }
                    path.ClassName = this.OriginalClassName;
                    path.NamespacePath = this.OriginalNamespace;
                }
                this.classobj = new ManagementClass(path);
            }
            else
            {
                ManagementPath path2 = this.classobj.Path;
                this.OriginalServer = path2.Server;
                this.OriginalClassName = path2.ClassName;
                this.OriginalNamespace = path2.NamespacePath;
                char[] chArray = this.OriginalNamespace.ToCharArray();
                if (((chArray.Length >= 2) && (chArray[0] == '\\')) && (chArray[1] == '\\'))
                {
                    bool flag = false;
                    int length = this.OriginalNamespace.Length;
                    this.OriginalNamespace = string.Empty;
                    for (int i = 2; i < length; i++)
                    {
                        if (flag)
                        {
                            this.OriginalNamespace = this.OriginalNamespace + chArray[i];
                        }
                        else if (chArray[i] == '\\')
                        {
                            flag = true;
                        }
                    }
                }
            }
            try
            {
                this.classobj.Get();
            }
            catch (ManagementException)
            {
                throw;
            }
            this.bSingletonClass = false;
            using (QualifierDataCollection.QualifierDataEnumerator enumerator = this.classobj.Qualifiers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (string.Compare(enumerator.Current.Name, "singleton", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.bSingletonClass = true;
                        return;
                    }
                }
            }
        }

        private void InitializeCodeGeneration()
        {
            this.InitializeClassObject();
            this.InitilializePublicPrivateMembers();
            this.ProcessNamespaceAndClassName();
            this.ProcessNamingCollisions();
        }

        private bool InitializeCodeGenerator(CodeLanguage lang)
        {
            string str = "";
            Assembly assembly = null;
            Type type = null;
            bool flag = true;
            AssemblyName name = null;
            AssemblyName assemblyRef = null;
            try
            {
                switch (lang)
                {
                    case CodeLanguage.CSharp:
                        str = "C#.";
                        this.cp = new CSharpCodeProvider();
                        goto Label_01B9;

                    case CodeLanguage.JScript:
                        str = "JScript.NET.";
                        this.cp = new JScriptCodeProvider();
                        goto Label_01B9;

                    case CodeLanguage.VB:
                        str = "Visual Basic.";
                        this.cp = new VBCodeProvider();
                        goto Label_01B9;

                    case CodeLanguage.VJSharp:
                        str = "Visual J#.";
                        flag = false;
                        name = Assembly.GetExecutingAssembly().GetName();
                        assemblyRef = new AssemblyName {
                            CultureInfo = new CultureInfo(""),
                            Name = "VJSharpCodeProvider"
                        };
                        assemblyRef.SetPublicKey(name.GetPublicKey());
                        assemblyRef.Version = name.Version;
                        assembly = Assembly.Load(assemblyRef);
                        if (assembly != null)
                        {
                            type = assembly.GetType("Microsoft.VJSharp.VJSharpCodeProvider");
                            if (type != null)
                            {
                                this.cp = (CodeDomProvider) Activator.CreateInstance(type);
                                flag = true;
                            }
                        }
                        goto Label_01B9;

                    case CodeLanguage.Mcpp:
                        str = "Managed C++.";
                        flag = false;
                        name = Assembly.GetExecutingAssembly().GetName();
                        assemblyRef = new AssemblyName {
                            CultureInfo = new CultureInfo("")
                        };
                        assemblyRef.SetPublicKey(name.GetPublicKey());
                        assemblyRef.Name = "CppCodeProvider";
                        assemblyRef.Version = new Version(this.VSVERSION);
                        assembly = Assembly.Load(assemblyRef);
                        if (assembly != null)
                        {
                            type = assembly.GetType("Microsoft.VisualC.CppCodeProvider");
                            if (type != null)
                            {
                                this.cp = (CodeDomProvider) Activator.CreateInstance(type);
                                flag = true;
                            }
                        }
                        goto Label_01B9;
                }
            }
            catch
            {
                throw new ArgumentOutOfRangeException(string.Format(GetString("UNABLE_TOCREATE_GEN_EXCEPT"), str));
            }
        Label_01B9:
            if (!flag)
            {
                throw new ArgumentOutOfRangeException(string.Format(GetString("UNABLE_TOCREATE_GEN_EXCEPT"), str));
            }
            this.GetUnsignedSupport(lang);
            return true;
        }

        private void InitializeCodeTypeDeclaration(CodeLanguage lang)
        {
            this.cn = new CodeNamespace(this.PrivateNamesUsed["GeneratedNamespace"].ToString());
            this.cn.Imports.Add(new CodeNamespaceImport("System"));
            this.cn.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));
            this.cn.Imports.Add(new CodeNamespaceImport("System.Management"));
            this.cn.Imports.Add(new CodeNamespaceImport("System.Collections"));
            this.cn.Imports.Add(new CodeNamespaceImport("System.Globalization"));
            if (lang == CodeLanguage.VB)
            {
                this.cn.Imports.Add(new CodeNamespaceImport("Microsoft.VisualBasic"));
            }
        }

        private void InitilializePublicPrivateMembers()
        {
            this.PublicNamesUsed.Add("SystemPropertiesProperty", "SystemProperties");
            this.PublicNamesUsed.Add("LateBoundObjectProperty", "LateBoundObject");
            this.PublicNamesUsed.Add("NamespaceProperty", "OriginatingNamespace");
            this.PublicNamesUsed.Add("ClassNameProperty", "ManagementClassName");
            this.PublicNamesUsed.Add("ScopeProperty", "Scope");
            this.PublicNamesUsed.Add("PathProperty", "Path");
            this.PublicNamesUsed.Add("SystemPropertiesClass", "ManagementSystemProperties");
            this.PublicNamesUsed.Add("LateBoundClass", "System.Management.ManagementObject");
            this.PublicNamesUsed.Add("PathClass", "System.Management.ManagementPath");
            this.PublicNamesUsed.Add("ScopeClass", "System.Management.ManagementScope");
            this.PublicNamesUsed.Add("QueryOptionsClass", "System.Management.EnumerationOptions");
            this.PublicNamesUsed.Add("GetOptionsClass", "System.Management.ObjectGetOptions");
            this.PublicNamesUsed.Add("ArgumentExceptionClass", "System.ArgumentException");
            this.PublicNamesUsed.Add("QueryClass", "SelectQuery");
            this.PublicNamesUsed.Add("ObjectSearcherClass", "System.Management.ManagementObjectSearcher");
            this.PublicNamesUsed.Add("FilterFunction", "GetInstances");
            this.PublicNamesUsed.Add("ConstructPathFunction", "ConstructPath");
            this.PublicNamesUsed.Add("TypeConverter", "TypeConverter");
            this.PublicNamesUsed.Add("AutoCommitProperty", "AutoCommit");
            this.PublicNamesUsed.Add("CommitMethod", "CommitObject");
            this.PublicNamesUsed.Add("ManagementClass", "System.Management.ManagementClass");
            this.PublicNamesUsed.Add("NotSupportedExceptClass", "System.NotSupportedException");
            this.PublicNamesUsed.Add("BaseObjClass", "System.Management.ManagementBaseObject");
            this.PublicNamesUsed.Add("OptionsProp", "Options");
            this.PublicNamesUsed.Add("ClassPathProperty", "ClassPath");
            this.PublicNamesUsed.Add("CreateInst", "CreateInstance");
            this.PublicNamesUsed.Add("DeleteInst", "Delete");
            this.PublicNamesUsed.Add("SystemNameSpace", "System");
            this.PublicNamesUsed.Add("ArgumentOutOfRangeException", "System.ArgumentOutOfRangeException");
            this.PublicNamesUsed.Add("System", "System");
            this.PublicNamesUsed.Add("Other", "Other");
            this.PublicNamesUsed.Add("Unknown", "Unknown");
            this.PublicNamesUsed.Add("PutOptions", "System.Management.PutOptions");
            this.PublicNamesUsed.Add("Type", "System.Type");
            this.PublicNamesUsed.Add("Boolean", "System.Boolean");
            this.PublicNamesUsed.Add("ValueType", "System.ValueType");
            this.PublicNamesUsed.Add("Events1", "Events");
            this.PublicNamesUsed.Add("Component1", "Component");
            this.PrivateNamesUsed.Add("SystemPropertiesObject", "PrivateSystemProperties");
            this.PrivateNamesUsed.Add("LateBoundObject", "PrivateLateBoundObject");
            this.PrivateNamesUsed.Add("AutoCommitProperty", "AutoCommitProp");
            this.PrivateNamesUsed.Add("Privileges", "EnablePrivileges");
            this.PrivateNamesUsed.Add("ComponentClass", "System.ComponentModel.Component");
            this.PrivateNamesUsed.Add("ScopeParam", "mgmtScope");
            this.PrivateNamesUsed.Add("NullRefExcep", "System.NullReferenceException");
            this.PrivateNamesUsed.Add("ConverterClass", "WMIValueTypeConverter");
            this.PrivateNamesUsed.Add("EnumParam", "enumOptions");
            this.PrivateNamesUsed.Add("CreationClassName", "CreatedClassName");
            this.PrivateNamesUsed.Add("CreationWmiNamespace", "CreatedWmiNamespace");
            this.PrivateNamesUsed.Add("ClassNameCheckFunc", "CheckIfProperClass");
            this.PrivateNamesUsed.Add("EmbeddedObject", "embeddedObj");
            this.PrivateNamesUsed.Add("CurrentObject", "curObj");
            this.PrivateNamesUsed.Add("IsEmbedded", "isEmbedded");
            this.PrivateNamesUsed.Add("ToDateTimeMethod", "ToDateTime");
            this.PrivateNamesUsed.Add("ToDMTFDateTimeMethod", "ToDmtfDateTime");
            this.PrivateNamesUsed.Add("ToDMTFTimeIntervalMethod", "ToDmtfTimeInterval");
            this.PrivateNamesUsed.Add("ToTimeSpanMethod", "ToTimeSpan");
            this.PrivateNamesUsed.Add("SetMgmtScope", "SetStaticManagementScope");
            this.PrivateNamesUsed.Add("statMgmtScope", "statMgmtScope");
            this.PrivateNamesUsed.Add("staticScope", "StaticScope");
            this.PrivateNamesUsed.Add("initVariable", "Initialize");
            this.PrivateNamesUsed.Add("putOptions", "putOptions");
            this.PrivateNamesUsed.Add("InitialObjectFunc", "InitializeObject");
        }

        private void InitPrivateMemberVariables(CodeMemberMethod cmMethod)
        {
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { MethodName = this.PrivateNamesUsed["initVariable"].ToString() }
            };
            cmMethod.Statements.Add(expression);
        }

        private int IsContainedIn(string strToFind, ref SortedList sortedList)
        {
            for (int i = 0; i < sortedList.Count; i++)
            {
                if (string.Compare(sortedList.GetByIndex(i).ToString(), strToFind, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        private static bool IsContainedInArray(string strToFind, ArrayList arrToSearch)
        {
            for (int i = 0; i < arrToSearch.Count; i++)
            {
                if (string.Compare(arrToSearch[i].ToString(), strToFind, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsDesignerSerializationVisibilityToBeSet(string propName)
        {
            return (string.Compare(propName, "Path", StringComparison.OrdinalIgnoreCase) != 0);
        }

        private bool IsDynamicClass()
        {
            bool flag = false;
            try
            {
                flag = System.Convert.ToBoolean(this.classobj.Qualifiers["dynamic"].Value, (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(bool)));
            }
            catch (ManagementException)
            {
            }
            return flag;
        }

        private static bool IsPropertyValueType(CimType cType)
        {
            bool flag = true;
            CimType type = cType;
            return ((((type != CimType.String) && (type != CimType.Object)) && (type != CimType.Reference)) && flag);
        }

        private static bool isTypeInt(CimType cType)
        {
            switch (cType)
            {
                case CimType.SInt16:
                case CimType.SInt32:
                case CimType.SInt8:
                case CimType.UInt8:
                case CimType.UInt16:
                case CimType.UInt32:
                    return true;
            }
            return false;
        }

        private void ProcessNamespaceAndClassName()
        {
            string inString = string.Empty;
            string nETNamespace = string.Empty;
            if (this.NETNamespace.Length == 0)
            {
                nETNamespace = this.OriginalNamespace.Replace('\\', '.').ToUpper(CultureInfo.InvariantCulture);
            }
            else
            {
                nETNamespace = this.NETNamespace;
            }
            if (this.OriginalClassName.IndexOf('_') > 0)
            {
                inString = this.OriginalClassName.Substring(0, this.OriginalClassName.IndexOf('_'));
                if (this.NETNamespace.Length == 0)
                {
                    nETNamespace = nETNamespace + "." + inString;
                }
                inString = this.OriginalClassName.Substring(this.OriginalClassName.IndexOf('_') + 1);
            }
            else
            {
                inString = this.OriginalClassName;
            }
            if (!char.IsLetter(inString[0]))
            {
                inString = "C" + inString;
            }
            inString = this.ResolveCollision(inString, true);
            if (((Type.GetType("System." + inString) != null) || (Type.GetType("System.ComponentModel." + inString) != null)) || (((Type.GetType("System.Management." + inString) != null) || (Type.GetType("System.Collections." + inString) != null)) || (Type.GetType("System.Globalization." + inString) != null)))
            {
                this.PublicNamesUsed.Add(inString, inString);
                inString = this.ResolveCollision(inString, true);
            }
            this.PrivateNamesUsed.Add("GeneratedClassName", inString);
            this.PrivateNamesUsed.Add("GeneratedNamespace", nETNamespace);
        }

        private void ProcessNamingCollisions()
        {
            int num;
            if (this.classobj.Properties != null)
            {
                foreach (PropertyData data in this.classobj.Properties)
                {
                    this.PublicProperties.Add(data.Name, data.Name);
                }
            }
            if (this.classobj.Methods != null)
            {
                foreach (MethodData data2 in this.classobj.Methods)
                {
                    this.PublicMethods.Add(data2.Name, data2.Name);
                }
            }
            foreach (string str in this.PublicNamesUsed.Values)
            {
                num = this.IsContainedIn(str, ref this.PublicProperties);
                if (num != -1)
                {
                    this.PublicProperties.SetByIndex(num, this.ResolveCollision(str, false));
                }
                else
                {
                    num = this.IsContainedIn(str, ref this.PublicMethods);
                    if (num != -1)
                    {
                        this.PublicMethods.SetByIndex(num, this.ResolveCollision(str, false));
                    }
                }
            }
            foreach (string str2 in this.PublicProperties.Values)
            {
                num = this.IsContainedIn(str2, ref this.PrivateNamesUsed);
                if (num != -1)
                {
                    this.PrivateNamesUsed.SetByIndex(num, this.ResolveCollision(str2, false));
                }
            }
            foreach (string str3 in this.PublicMethods.Values)
            {
                num = this.IsContainedIn(str3, ref this.PrivateNamesUsed);
                if (num != -1)
                {
                    this.PrivateNamesUsed.SetByIndex(num, this.ResolveCollision(str3, false));
                }
            }
            foreach (string str4 in this.PublicProperties.Values)
            {
                num = this.IsContainedIn(str4, ref this.PublicMethods);
                if (num != -1)
                {
                    this.PublicMethods.SetByIndex(num, this.ResolveCollision(str4, false));
                }
            }
            string inString = this.PrivateNamesUsed["GeneratedClassName"].ToString() + "Collection";
            this.PrivateNamesUsed.Add("CollectionClass", this.ResolveCollision(inString, true));
            inString = this.PrivateNamesUsed["GeneratedClassName"].ToString() + "Enumerator";
            this.PrivateNamesUsed.Add("EnumeratorClass", this.ResolveCollision(inString, true));
        }

        private string ProcessPropertyQualifiers(PropertyData prop, ref bool bRead, ref bool bWrite, ref bool bStatic, bool bDynamicClass, out bool nullable)
        {
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            nullable = true;
            bRead = true;
            bWrite = false;
            this.arrConvFuncName = "ToInt32";
            this.enumType = "System.Int32";
            string str = string.Empty;
            foreach (QualifierData data in prop.Qualifiers)
            {
                if (string.Compare(data.Name, "description", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    str = data.Value.ToString();
                }
                else if (string.Compare(data.Name, "Not_Null", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    nullable = false;
                }
                else
                {
                    if (string.Compare(data.Name, "key", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.arrKeyType.Add(this.cmp.Type);
                        this.arrKeys.Add(prop.Name);
                        nullable = false;
                        break;
                    }
                    if (string.Compare(data.Name, "static", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        bStatic = true;
                        this.cmp.Attributes |= MemberAttributes.Static;
                    }
                    else if (string.Compare(data.Name, "read", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (!((bool) data.Value))
                        {
                            bRead = false;
                        }
                        else
                        {
                            bRead = true;
                        }
                    }
                    else if (string.Compare(data.Name, "write", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        flag = true;
                        if ((bool) data.Value)
                        {
                            flag2 = true;
                        }
                        else
                        {
                            flag2 = false;
                        }
                    }
                    else if ((string.Compare(data.Name, "ValueMap", StringComparison.OrdinalIgnoreCase) == 0) && !flag3)
                    {
                        try
                        {
                            this.ValueMap.Clear();
                            if (isTypeInt(prop.Type) && (data.Value != null))
                            {
                                string[] strArray = (string[]) data.Value;
                                for (int i = 0; i < strArray.Length; i++)
                                {
                                    try
                                    {
                                        this.arrConvFuncName = ConvertToNumericValueAndAddToArray(prop.Type, strArray[i], this.ValueMap, out this.enumType);
                                    }
                                    catch (OverflowException)
                                    {
                                    }
                                }
                            }
                        }
                        catch (FormatException)
                        {
                            flag3 = true;
                            this.ValueMap.Clear();
                        }
                        catch (InvalidCastException)
                        {
                            this.ValueMap.Clear();
                        }
                    }
                    else if ((string.Compare(data.Name, "Values", StringComparison.OrdinalIgnoreCase) == 0) && !flag3)
                    {
                        try
                        {
                            this.Values.Clear();
                            if (isTypeInt(prop.Type) && (data.Value != null))
                            {
                                ArrayList arrIn = new ArrayList(5);
                                string[] strArray2 = (string[]) data.Value;
                                for (int j = 0; j < strArray2.Length; j++)
                                {
                                    if (strArray2[j].Length == 0)
                                    {
                                        this.Values.Clear();
                                        flag3 = true;
                                        break;
                                    }
                                    string str2 = ConvertValuesToName(strArray2[j]);
                                    arrIn.Add(str2);
                                }
                                this.ResolveEnumNameValues(arrIn, ref this.Values);
                            }
                        }
                        catch (InvalidCastException)
                        {
                            this.Values.Clear();
                        }
                    }
                    else if ((string.Compare(data.Name, "BitMap", StringComparison.OrdinalIgnoreCase) == 0) && !flag3)
                    {
                        try
                        {
                            this.BitMap.Clear();
                            if (isTypeInt(prop.Type) && (data.Value != null))
                            {
                                string[] strArray3 = (string[]) data.Value;
                                for (int k = 0; k < strArray3.Length; k++)
                                {
                                    this.BitMap.Add(ConvertBitMapValueToInt32(strArray3[k]));
                                }
                            }
                        }
                        catch (FormatException)
                        {
                            this.BitMap.Clear();
                            flag3 = true;
                        }
                        catch (InvalidCastException)
                        {
                            this.BitMap.Clear();
                        }
                    }
                    else if ((string.Compare(data.Name, "BitValues", StringComparison.OrdinalIgnoreCase) == 0) && !flag3)
                    {
                        try
                        {
                            this.BitValues.Clear();
                            if (isTypeInt(prop.Type) && (data.Value != null))
                            {
                                ArrayList list2 = new ArrayList(5);
                                string[] strArray4 = (string[]) data.Value;
                                for (int m = 0; m < strArray4.Length; m++)
                                {
                                    if (strArray4[m].Length == 0)
                                    {
                                        this.BitValues.Clear();
                                        flag3 = true;
                                        break;
                                    }
                                    string str3 = ConvertValuesToName(strArray4[m]);
                                    list2.Add(str3);
                                }
                                this.ResolveEnumNameValues(list2, ref this.BitValues);
                            }
                        }
                        catch (InvalidCastException)
                        {
                            this.BitValues.Clear();
                        }
                    }
                }
            }
            if (((!bDynamicClass && !flag) || ((!bDynamicClass && flag) && flag2)) || ((bDynamicClass && flag) && flag2))
            {
                bWrite = true;
            }
            return str;
        }

        private string ResolveCollision(string inString, bool bCheckthisFirst)
        {
            string strToFind = inString;
            bool flag = true;
            int num = -1;
            string str2 = "";
            if (!bCheckthisFirst)
            {
                num++;
                strToFind = strToFind + str2 + num.ToString((IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(int)));
            }
            while (flag)
            {
                if (((this.IsContainedIn(strToFind, ref this.PublicProperties) == -1) && (this.IsContainedIn(strToFind, ref this.PublicMethods) == -1)) && ((this.IsContainedIn(strToFind, ref this.PublicNamesUsed) == -1) && (this.IsContainedIn(strToFind, ref this.PrivateNamesUsed) == -1)))
                {
                    flag = false;
                    break;
                }
                try
                {
                    num++;
                }
                catch (OverflowException)
                {
                    str2 = str2 + "_";
                    num = 0;
                }
                strToFind = inString + str2 + num.ToString((IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(int)));
            }
            if (strToFind.Length > 0)
            {
                strToFind = strToFind.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture) + strToFind.Substring(1, strToFind.Length - 1);
            }
            return strToFind;
        }

        private void ResolveEnumNameValues(ArrayList arrIn, ref ArrayList arrayOut)
        {
            arrayOut.Clear();
            int num = 0;
            string inString = string.Empty;
            IFormatProvider format = (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(int));
            for (int i = 0; i < arrIn.Count; i++)
            {
                inString = arrIn[i].ToString();
                inString = this.ResolveCollision(inString, true);
                if (IsContainedInArray(inString, arrayOut))
                {
                    num = 0;
                    inString = arrIn[i].ToString() + num.ToString(format);
                    while (IsContainedInArray(inString, arrayOut))
                    {
                        num++;
                        inString = arrIn[i].ToString() + num.ToString(format);
                    }
                }
                arrayOut.Add(inString);
            }
        }

        private void ToDMTFDateHelper(string dateTimeMember, CodeMemberMethod cmmdt, string strType)
        {
            string variableName = "dmtfDateTime";
            string str2 = "date";
            CodeMethodInvokeExpression targetObject = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(new CodeCastExpression(new CodeTypeReference(strType), new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(str2), dateTimeMember)), "ToString")
            };
            CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(targetObject, "PadLeft")
            };
            expression2.Parameters.Add(new CodePrimitiveExpression(2));
            expression2.Parameters.Add(new CodePrimitiveExpression('0'));
            GenerateConcatStrings(targetObject, expression2);
            cmmdt.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(variableName), GenerateConcatStrings(new CodeVariableReferenceExpression(variableName), expression2)));
        }

        private static void ToTimeSpanHelper(int start, int numOfCharacters, string strVarToAssign, CodeStatementCollection statCol)
        {
            string variableName = "tempString";
            string str2 = "dmtfTimespan";
            CodeMethodInvokeExpression right = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(str2), "Substring")
            };
            right.Parameters.Add(new CodePrimitiveExpression(start));
            right.Parameters.Add(new CodePrimitiveExpression(numOfCharacters));
            statCol.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(variableName), right));
            right = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("System.Int32"), "Parse")
            };
            right.Parameters.Add(new CodeVariableReferenceExpression(variableName));
            statCol.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(strVarToAssign), right));
        }

        public string GeneratedFileName
        {
            get
            {
                return this.genFileName;
            }
        }

        public string GeneratedTypeName
        {
            get
            {
                return (this.PrivateNamesUsed["GeneratedNamespace"].ToString() + "." + this.PrivateNamesUsed["GeneratedClassName"].ToString());
            }
        }
    }
}

