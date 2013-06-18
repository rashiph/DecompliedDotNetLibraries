namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;

    internal abstract class DbSqlParser
    {
        private DbSqlParserColumnCollection _columns;
        private static int _identifierGroup;
        private static int _keywordGroup;
        private static int _otherGroup;
        private static int _quotedidentifierGroup;
        private string _quotePrefixCharacter;
        private string _quoteSuffixCharacter;
        private static Regex _sqlTokenParser;
        private static string _sqlTokenPattern;
        private static int _stringGroup;
        private DbSqlParserTableCollection _tables;
        private const string SqlTokenPattern_Part1 = @"[\s;]*((?<keyword>all|as|compute|cross|distinct|for|from|full|group|having|intersect|inner|join|left|minus|natural|order|outer|on|right|select|top|union|using|where)\b|(?<identifier>";
        private const string SqlTokenPattern_Part2 = "*)|";
        private const string SqlTokenPattern_Part3 = "(?<quotedidentifier>";
        private const string SqlTokenPattern_Part4 = ")";
        private const string SqlTokenPattern_Part5 = "|(?<string>";
        private const string SqlTokenPattern_Part6 = @")|(?<other>.))[\s;]*";

        public DbSqlParser(string quotePrefixCharacter, string quoteSuffixCharacter, string regexPattern)
        {
            this._quotePrefixCharacter = quotePrefixCharacter;
            this._quoteSuffixCharacter = quoteSuffixCharacter;
            _sqlTokenPattern = regexPattern;
        }

        private void AddColumn(int maxPart, Token[] namePart, Token aliasName)
        {
            this.Columns.Add(this.GetPart(0, namePart, maxPart), this.GetPart(1, namePart, maxPart), this.GetPart(2, namePart, maxPart), this.GetPart(3, namePart, maxPart), this.GetTokenAsString(aliasName));
        }

        private void AddTable(int maxPart, Token[] namePart, Token correlationName)
        {
            this.Tables.Add(this.GetPart(1, namePart, maxPart), this.GetPart(2, namePart, maxPart), this.GetPart(3, namePart, maxPart), this.GetTokenAsString(correlationName));
        }

        protected abstract bool CatalogMatch(string valueA, string valueB);
        private void CompleteSchemaInformation()
        {
            DbSqlParserColumnCollection columns = this.Columns;
            DbSqlParserTableCollection tables = this.Tables;
            int count = columns.Count;
            int num10 = tables.Count;
            for (int i = 0; i < num10; i++)
            {
                DbSqlParserTable table2 = tables[i];
                DbSqlParserColumnCollection columns4 = this.GatherTableColumns(table2);
                table2.Columns = columns4;
            }
            for (int j = 0; j < count; j++)
            {
                DbSqlParserColumn column = columns[j];
                DbSqlParserTable table = this.FindTableForColumn(column);
                if (!column.IsExpression)
                {
                    if ("*" == column.ColumnName)
                    {
                        columns.RemoveAt(j);
                        if (column.TableName.Length != 0)
                        {
                            DbSqlParserColumnCollection columns3 = table.Columns;
                            int num9 = columns3.Count;
                            for (int m = 0; m < num9; m++)
                            {
                                columns.Insert(j + m, columns3[m]);
                            }
                            count += num9 - 1;
                            j += num9 - 1;
                        }
                        else
                        {
                            for (int n = 0; n < num10; n++)
                            {
                                table = tables[n];
                                DbSqlParserColumnCollection columns2 = table.Columns;
                                int num8 = columns2.Count;
                                for (int num2 = 0; num2 < num8; num2++)
                                {
                                    columns.Insert(j + num2, columns2[num2]);
                                }
                                count += num8 - 1;
                                j += num8;
                            }
                        }
                    }
                    else
                    {
                        DbSqlParserColumn completedColumn = this.FindCompletedColumn(table, column);
                        if (completedColumn != null)
                        {
                            column.CopySchemaInfoFrom(completedColumn);
                        }
                        else
                        {
                            column.CopySchemaInfoFrom(table);
                        }
                    }
                }
            }
            for (int k = 0; k < num10; k++)
            {
                DbSqlParserTable table3 = tables[k];
                this.GatherKeyColumns(table3);
            }
        }

        internal static string CreateRegexPattern(string validIdentifierFirstCharacters, string validIdendifierCharacters, string quotePrefixCharacter, string quotedIdentifierCharacters, string quoteSuffixCharacter, string stringPattern)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(@"[\s;]*((?<keyword>all|as|compute|cross|distinct|for|from|full|group|having|intersect|inner|join|left|minus|natural|order|outer|on|right|select|top|union|using|where)\b|(?<identifier>");
            builder.Append(validIdentifierFirstCharacters);
            builder.Append(validIdendifierCharacters);
            builder.Append("*)|");
            builder.Append(quotePrefixCharacter);
            builder.Append("(?<quotedidentifier>");
            builder.Append(quotedIdentifierCharacters);
            builder.Append(")");
            builder.Append(quoteSuffixCharacter);
            builder.Append("|(?<string>");
            builder.Append(stringPattern);
            builder.Append(@")|(?<other>.))[\s;]*");
            return builder.ToString();
        }

        protected DbSqlParserColumn FindCompletedColumn(DbSqlParserTable table, DbSqlParserColumn searchColumn)
        {
            DbSqlParserColumnCollection columns = table.Columns;
            int count = columns.Count;
            for (int i = 0; i < count; i++)
            {
                DbSqlParserColumn column = columns[i];
                if (this.CatalogMatch(column.ColumnName, searchColumn.ColumnName))
                {
                    return column;
                }
            }
            return null;
        }

        internal DbSqlParserTable FindTableForColumn(DbSqlParserColumn column)
        {
            DbSqlParserTableCollection tables = this.Tables;
            int count = tables.Count;
            for (int i = 0; i < count; i++)
            {
                DbSqlParserTable table = tables[i];
                if ((System.Data.Common.ADP.IsEmpty(column.DatabaseName) && System.Data.Common.ADP.IsEmpty(column.SchemaName)) && this.CatalogMatch(column.TableName, table.CorrelationName))
                {
                    return table;
                }
                if (((System.Data.Common.ADP.IsEmpty(column.DatabaseName) || this.CatalogMatch(column.DatabaseName, table.DatabaseName)) && (System.Data.Common.ADP.IsEmpty(column.SchemaName) || this.CatalogMatch(column.SchemaName, table.SchemaName))) && (System.Data.Common.ADP.IsEmpty(column.TableName) || this.CatalogMatch(column.TableName, table.TableName)))
                {
                    return table;
                }
            }
            return null;
        }

        protected abstract void GatherKeyColumns(DbSqlParserTable table);
        protected abstract DbSqlParserColumnCollection GatherTableColumns(DbSqlParserTable table);
        private string GetPart(int part, Token[] namePart, int maxPart)
        {
            int index = ((maxPart - namePart.Length) + part) + 1;
            if (0 > index)
            {
                return null;
            }
            return this.GetTokenAsString(namePart[index]);
        }

        private static Regex GetSqlTokenParser()
        {
            Regex regex = _sqlTokenParser;
            if (regex == null)
            {
                regex = new Regex(_sqlTokenPattern, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
                _identifierGroup = regex.GroupNumberFromName("identifier");
                _quotedidentifierGroup = regex.GroupNumberFromName("quotedidentifier");
                _keywordGroup = regex.GroupNumberFromName("keyword");
                _stringGroup = regex.GroupNumberFromName("string");
                _otherGroup = regex.GroupNumberFromName("other");
                _sqlTokenParser = regex;
            }
            return regex;
        }

        private string GetTokenAsString(Token token)
        {
            if (TokenType.QuotedIdentifier == token.Type)
            {
                return (this._quotePrefixCharacter + token.Value + this._quoteSuffixCharacter);
            }
            return token.Value;
        }

        public void Parse(string statementText)
        {
            this.Parse2(statementText);
            this.CompleteSchemaInformation();
        }

        private void Parse2(string statementText)
        {
            bool flag;
            PARSERSTATE nOTHINGYET = PARSERSTATE.NOTHINGYET;
            Token[] namePart = new Token[4];
            int maxPart = 0;
            Token @null = Token.Null;
            TokenType type7 = TokenType.Null;
            int num2 = 0;
            this._columns = null;
            this._tables = null;
            Match match = SqlTokenParser.Match(statementText);
            Token token2 = TokenFromMatch(match);
        Label_003B:
            flag = false;
            switch (nOTHINGYET)
            {
                case PARSERSTATE.NOTHINGYET:
                    if (token2.Type != TokenType.Keyword_SELECT)
                    {
                        throw System.Data.Common.ADP.InvalidOperation(Res.GetString("ADP_SQLParserInternalError"));
                    }
                    nOTHINGYET = PARSERSTATE.SELECT;
                    goto Label_061A;

                case PARSERSTATE.SELECT:
                {
                    TokenType type2 = token2.Type;
                    if (type2 > TokenType.Other_Star)
                    {
                        switch (type2)
                        {
                            case TokenType.Keyword_DISTINCT:
                            case TokenType.Keyword_ALL:
                                goto Label_061A;

                            case TokenType.Keyword_FROM:
                                nOTHINGYET = PARSERSTATE.FROM;
                                goto Label_061A;
                        }
                        break;
                    }
                    switch (type2)
                    {
                        case TokenType.Identifier:
                        case TokenType.QuotedIdentifier:
                            nOTHINGYET = PARSERSTATE.COLUMN;
                            maxPart = 0;
                            namePart[0] = token2;
                            goto Label_061A;

                        case TokenType.Other_LeftParen:
                            nOTHINGYET = PARSERSTATE.EXPRESSION;
                            num2++;
                            goto Label_061A;

                        case TokenType.Other_RightParen:
                            throw System.Data.Common.ADP.SyntaxErrorMissingParenthesis();

                        case TokenType.Other_Star:
                            nOTHINGYET = PARSERSTATE.COLUMNALIAS;
                            maxPart = 0;
                            namePart[0] = token2;
                            goto Label_061A;
                    }
                    break;
                }
                case PARSERSTATE.COLUMN:
                {
                    TokenType type = token2.Type;
                    if (type > TokenType.Other_Star)
                    {
                        switch (type)
                        {
                            case TokenType.Keyword_AS:
                                goto Label_061A;

                            case TokenType.Keyword_FROM:
                                goto Label_01FF;
                        }
                    }
                    else
                    {
                        switch (type)
                        {
                            case TokenType.Identifier:
                            case TokenType.QuotedIdentifier:
                                if (TokenType.Other_Period != type7)
                                {
                                    nOTHINGYET = PARSERSTATE.COLUMNALIAS;
                                    @null = token2;
                                }
                                else
                                {
                                    namePart[++maxPart] = token2;
                                }
                                goto Label_061A;

                            case TokenType.Other_Comma:
                                goto Label_01FF;

                            case TokenType.Other_Period:
                                if (maxPart > 3)
                                {
                                    throw System.Data.Common.ADP.SyntaxErrorTooManyNameParts();
                                }
                                goto Label_061A;

                            case TokenType.Other_LeftParen:
                                nOTHINGYET = PARSERSTATE.EXPRESSION;
                                num2++;
                                maxPart = -1;
                                goto Label_061A;

                            case TokenType.Other_RightParen:
                                throw System.Data.Common.ADP.SyntaxErrorMissingParenthesis();

                            case TokenType.Other_Star:
                                nOTHINGYET = PARSERSTATE.COLUMNALIAS;
                                namePart[++maxPart] = token2;
                                goto Label_061A;
                        }
                    }
                    nOTHINGYET = PARSERSTATE.EXPRESSION;
                    maxPart = -1;
                    goto Label_061A;
                }
                case PARSERSTATE.COLUMNALIAS:
                {
                    TokenType type9 = token2.Type;
                    if ((type9 != TokenType.Other_Comma) && (type9 != TokenType.Keyword_FROM))
                    {
                        throw System.Data.Common.ADP.SyntaxErrorExpectedCommaAfterColumn();
                    }
                    nOTHINGYET = (token2.Type == TokenType.Keyword_FROM) ? PARSERSTATE.FROM : PARSERSTATE.SELECT;
                    this.AddColumn(maxPart, namePart, @null);
                    maxPart = -1;
                    @null = Token.Null;
                    goto Label_061A;
                }
                case PARSERSTATE.TABLE:
                    switch (token2.Type)
                    {
                        case TokenType.Null:
                        case TokenType.Keyword_COMPUTE:
                        case TokenType.Keyword_FOR:
                        case TokenType.Keyword_GROUP:
                        case TokenType.Keyword_HAVING:
                        case TokenType.Keyword_INTERSECT:
                        case TokenType.Keyword_MINUS:
                        case TokenType.Keyword_ORDER:
                        case TokenType.Keyword_UNION:
                        case TokenType.Keyword_WHERE:
                            nOTHINGYET = PARSERSTATE.DONE;
                            flag = true;
                            goto Label_061A;

                        case TokenType.Identifier:
                        case TokenType.QuotedIdentifier:
                            if (TokenType.Other_Period != type7)
                            {
                                nOTHINGYET = PARSERSTATE.TABLEALIAS;
                                @null = token2;
                            }
                            else
                            {
                                namePart[++maxPart] = token2;
                            }
                            goto Label_061A;

                        case TokenType.Other_Comma:
                        case TokenType.Keyword_JOIN:
                            nOTHINGYET = PARSERSTATE.FROM;
                            flag = true;
                            goto Label_061A;

                        case TokenType.Other_Period:
                            if (maxPart > 2)
                            {
                                throw System.Data.Common.ADP.SyntaxErrorTooManyNameParts();
                            }
                            goto Label_061A;

                        case TokenType.Keyword_AS:
                            goto Label_061A;

                        case TokenType.Keyword_CROSS:
                        case TokenType.Keyword_LEFT:
                        case TokenType.Keyword_NATURAL:
                        case TokenType.Keyword_RIGHT:
                            nOTHINGYET = PARSERSTATE.JOIN;
                            flag = true;
                            goto Label_061A;

                        case TokenType.Keyword_ON:
                        case TokenType.Keyword_USING:
                            nOTHINGYET = PARSERSTATE.JOINCONDITION;
                            flag = true;
                            goto Label_061A;
                    }
                    throw System.Data.Common.ADP.SyntaxErrorExpectedNextPart();

                case PARSERSTATE.TABLEALIAS:
                    flag = true;
                    switch (token2.Type)
                    {
                        case TokenType.Keyword_COMPUTE:
                        case TokenType.Keyword_FOR:
                        case TokenType.Keyword_GROUP:
                        case TokenType.Keyword_HAVING:
                        case TokenType.Keyword_INTERSECT:
                        case TokenType.Keyword_MINUS:
                        case TokenType.Keyword_ORDER:
                        case TokenType.Keyword_UNION:
                        case TokenType.Keyword_WHERE:
                        case TokenType.Null:
                            nOTHINGYET = PARSERSTATE.DONE;
                            goto Label_061A;

                        case TokenType.Keyword_CROSS:
                        case TokenType.Keyword_LEFT:
                        case TokenType.Keyword_NATURAL:
                        case TokenType.Keyword_RIGHT:
                            nOTHINGYET = PARSERSTATE.JOIN;
                            goto Label_061A;

                        case TokenType.Keyword_JOIN:
                        case TokenType.Other_Comma:
                            nOTHINGYET = PARSERSTATE.FROM;
                            goto Label_061A;

                        case TokenType.Keyword_ON:
                        case TokenType.Keyword_USING:
                            nOTHINGYET = PARSERSTATE.JOINCONDITION;
                            goto Label_061A;
                    }
                    throw System.Data.Common.ADP.SyntaxErrorExpectedCommaAfterTable();

                case PARSERSTATE.FROM:
                    switch (token2.Type)
                    {
                        case TokenType.Identifier:
                        case TokenType.QuotedIdentifier:
                            nOTHINGYET = PARSERSTATE.TABLE;
                            maxPart = 0;
                            namePart[0] = token2;
                            goto Label_061A;
                    }
                    throw System.Data.Common.ADP.SyntaxErrorExpectedIdentifier();

                case PARSERSTATE.EXPRESSION:
                    switch (token2.Type)
                    {
                        case TokenType.Identifier:
                        case TokenType.QuotedIdentifier:
                            if (num2 == 0)
                            {
                                @null = token2;
                            }
                            break;

                        case TokenType.Other_Comma:
                        case TokenType.Keyword_FROM:
                            if (num2 == 0)
                            {
                                nOTHINGYET = (token2.Type == TokenType.Keyword_FROM) ? PARSERSTATE.FROM : PARSERSTATE.SELECT;
                                this.AddColumn(maxPart, namePart, @null);
                                maxPart = -1;
                                @null = Token.Null;
                            }
                            break;

                        case TokenType.Other_LeftParen:
                            num2++;
                            break;

                        case TokenType.Other_RightParen:
                            num2--;
                            break;
                    }
                    goto Label_061A;

                case PARSERSTATE.JOIN:
                    switch (token2.Type)
                    {
                        case TokenType.Keyword_INNER:
                        case TokenType.Keyword_OUTER:
                            goto Label_061A;

                        case TokenType.Keyword_JOIN:
                            nOTHINGYET = PARSERSTATE.FROM;
                            goto Label_061A;
                    }
                    throw System.Data.Common.ADP.SyntaxErrorExpectedNextPart();

                case PARSERSTATE.JOINCONDITION:
                    switch (token2.Type)
                    {
                        case TokenType.Other_LeftParen:
                            num2++;
                            goto Label_061A;

                        case TokenType.Other_RightParen:
                            num2--;
                            goto Label_061A;
                    }
                    if (num2 == 0)
                    {
                        switch (token2.Type)
                        {
                            case TokenType.Keyword_COMPUTE:
                            case TokenType.Keyword_FOR:
                            case TokenType.Keyword_GROUP:
                            case TokenType.Keyword_HAVING:
                            case TokenType.Keyword_INTERSECT:
                            case TokenType.Keyword_MINUS:
                            case TokenType.Keyword_ORDER:
                            case TokenType.Keyword_UNION:
                            case TokenType.Keyword_WHERE:
                            case TokenType.Null:
                                nOTHINGYET = PARSERSTATE.DONE;
                                break;

                            case TokenType.Keyword_CROSS:
                            case TokenType.Keyword_LEFT:
                            case TokenType.Keyword_NATURAL:
                            case TokenType.Keyword_RIGHT:
                                nOTHINGYET = PARSERSTATE.JOIN;
                                break;

                            case TokenType.Keyword_JOIN:
                                nOTHINGYET = PARSERSTATE.FROM;
                                break;
                        }
                    }
                    goto Label_061A;

                case PARSERSTATE.DONE:
                    return;

                default:
                    throw System.Data.Common.ADP.InvalidOperation(Res.GetString("ADP_SQLParserInternalError"));
            }
            nOTHINGYET = PARSERSTATE.EXPRESSION;
            goto Label_061A;
        Label_01FF:
            nOTHINGYET = (token2.Type == TokenType.Keyword_FROM) ? PARSERSTATE.FROM : PARSERSTATE.SELECT;
            this.AddColumn(maxPart, namePart, @null);
            maxPart = -1;
            @null = Token.Null;
        Label_061A:
            if (flag)
            {
                this.AddTable(maxPart, namePart, @null);
                maxPart = -1;
                @null = Token.Null;
                flag = false;
            }
            type7 = token2.Type;
            match = match.NextMatch();
            token2 = TokenFromMatch(match);
            goto Label_003B;
        }

        internal static Token TokenFromMatch(Match match)
        {
            TokenType keyword;
            string str;
            if (((match == null) || (Match.Empty == match)) || !match.Success)
            {
                return Token.Null;
            }
            if (match.Groups[_identifierGroup].Success)
            {
                return new Token(TokenType.Identifier, match.Groups[_identifierGroup].Value);
            }
            if (match.Groups[_quotedidentifierGroup].Success)
            {
                return new Token(TokenType.QuotedIdentifier, match.Groups[_quotedidentifierGroup].Value);
            }
            if (match.Groups[_stringGroup].Success)
            {
                return new Token(TokenType.String, match.Groups[_stringGroup].Value);
            }
            if (!match.Groups[_otherGroup].Success)
            {
                if (!match.Groups[_keywordGroup].Success)
                {
                    goto Label_042D;
                }
                str = match.Groups[_keywordGroup].Value.ToLower(CultureInfo.InvariantCulture);
                int length = str.Length;
                keyword = TokenType.Keyword;
                switch (length)
                {
                    case 2:
                        if (!("as" == str))
                        {
                            if ("on" == str)
                            {
                                keyword = TokenType.Keyword_ON;
                            }
                            break;
                        }
                        keyword = TokenType.Keyword_AS;
                        break;

                    case 3:
                        if (!("for" == str))
                        {
                            if ("all" == str)
                            {
                                keyword = TokenType.Keyword_ALL;
                            }
                            else if ("top" == str)
                            {
                                keyword = TokenType.Keyword_TOP;
                            }
                            break;
                        }
                        keyword = TokenType.Keyword_FOR;
                        break;

                    case 4:
                        if (!("from" == str))
                        {
                            if ("into" == str)
                            {
                                keyword = TokenType.Keyword_INTO;
                            }
                            else if ("join" == str)
                            {
                                keyword = TokenType.Keyword_JOIN;
                            }
                            else if ("left" == str)
                            {
                                keyword = TokenType.Keyword_LEFT;
                            }
                            break;
                        }
                        keyword = TokenType.Keyword_FROM;
                        break;

                    case 5:
                        if (!("where" == str))
                        {
                            if ("group" == str)
                            {
                                keyword = TokenType.Keyword_GROUP;
                            }
                            else if ("order" == str)
                            {
                                keyword = TokenType.Keyword_ORDER;
                            }
                            else if ("right" == str)
                            {
                                keyword = TokenType.Keyword_RIGHT;
                            }
                            else if ("outer" == str)
                            {
                                keyword = TokenType.Keyword_OUTER;
                            }
                            else if ("using" == str)
                            {
                                keyword = TokenType.Keyword_USING;
                            }
                            else if ("cross" == str)
                            {
                                keyword = TokenType.Keyword_CROSS;
                            }
                            else if ("union" == str)
                            {
                                keyword = TokenType.Keyword_UNION;
                            }
                            else if ("minus" == str)
                            {
                                keyword = TokenType.Keyword_MINUS;
                            }
                            else if ("inner" == str)
                            {
                                keyword = TokenType.Keyword_INNER;
                            }
                            break;
                        }
                        keyword = TokenType.Keyword_WHERE;
                        break;

                    case 6:
                        if (!("select" == str))
                        {
                            if ("having" == str)
                            {
                                keyword = TokenType.Keyword_HAVING;
                            }
                            break;
                        }
                        keyword = TokenType.Keyword_SELECT;
                        break;

                    case 7:
                        if (!("compute" == str))
                        {
                            if ("natural" == str)
                            {
                                keyword = TokenType.Keyword_NATURAL;
                            }
                            break;
                        }
                        keyword = TokenType.Keyword_COMPUTE;
                        break;

                    case 8:
                        if ("distinct" == str)
                        {
                            keyword = TokenType.Keyword_DISTINCT;
                        }
                        break;

                    case 9:
                        if ("intersect" == str)
                        {
                            keyword = TokenType.Keyword_INTERSECT;
                        }
                        break;
                }
            }
            else
            {
                string str2 = match.Groups[_otherGroup].Value.ToLower(CultureInfo.InvariantCulture);
                TokenType other = TokenType.Other;
                switch (str2[0])
                {
                    case '(':
                        other = TokenType.Other_LeftParen;
                        break;

                    case ')':
                        other = TokenType.Other_RightParen;
                        break;

                    case '*':
                        other = TokenType.Other_Star;
                        break;

                    case ',':
                        other = TokenType.Other_Comma;
                        break;

                    case '.':
                        other = TokenType.Other_Period;
                        break;
                }
                return new Token(other, match.Groups[_otherGroup].Value);
            }
            if (TokenType.Keyword != keyword)
            {
                return new Token(keyword, str);
            }
        Label_042D:
            return Token.Null;
        }

        internal DbSqlParserColumnCollection Columns
        {
            get
            {
                if (this._columns == null)
                {
                    this._columns = new DbSqlParserColumnCollection();
                }
                return this._columns;
            }
        }

        protected virtual string QuotePrefixCharacter
        {
            get
            {
                return this._quotePrefixCharacter;
            }
        }

        protected virtual string QuoteSuffixCharacter
        {
            get
            {
                return this._quoteSuffixCharacter;
            }
        }

        private static Regex SqlTokenParser
        {
            get
            {
                Regex sqlTokenParser = _sqlTokenParser;
                if (sqlTokenParser == null)
                {
                    sqlTokenParser = GetSqlTokenParser();
                }
                return sqlTokenParser;
            }
        }

        internal DbSqlParserTableCollection Tables
        {
            get
            {
                if (this._tables == null)
                {
                    this._tables = new DbSqlParserTableCollection();
                }
                return this._tables;
            }
        }

        private enum PARSERSTATE
        {
            COLUMN = 3,
            COLUMNALIAS = 4,
            DONE = 11,
            EXPRESSION = 8,
            FROM = 7,
            JOIN = 9,
            JOINCONDITION = 10,
            NOTHINGYET = 1,
            SELECT = 2,
            TABLE = 5,
            TABLEALIAS = 6
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Token
        {
            private DbSqlParser.TokenType _type;
            private string _value;
            internal static readonly DbSqlParser.Token Null;
            internal string Value
            {
                get
                {
                    return this._value;
                }
            }
            internal DbSqlParser.TokenType Type
            {
                get
                {
                    return this._type;
                }
            }
            internal Token(DbSqlParser.TokenType type, string value)
            {
                this._type = type;
                this._value = value;
            }

            static Token()
            {
                Null = new DbSqlParser.Token(DbSqlParser.TokenType.Null, null);
            }
        }

        public enum TokenType
        {
            Identifier = 1,
            Keyword = 200,
            Keyword_ALL = 0xc9,
            Keyword_AS = 0xca,
            Keyword_COMPUTE = 0xcb,
            Keyword_CROSS = 0xcc,
            Keyword_DISTINCT = 0xcd,
            Keyword_FOR = 0xce,
            Keyword_FROM = 0xcf,
            Keyword_FULL = 0xd0,
            Keyword_GROUP = 0xd1,
            Keyword_HAVING = 210,
            Keyword_INNER = 0xd3,
            Keyword_INTERSECT = 0xd4,
            Keyword_INTO = 0xd5,
            Keyword_JOIN = 0xd6,
            Keyword_LEFT = 0xd7,
            Keyword_MINUS = 0xd8,
            Keyword_NATURAL = 0xd9,
            Keyword_ON = 0xda,
            Keyword_ORDER = 0xdb,
            Keyword_OUTER = 220,
            Keyword_RIGHT = 0xdd,
            Keyword_SELECT = 0xde,
            Keyword_TOP = 0xdf,
            Keyword_UNION = 0xe0,
            Keyword_USING = 0xe1,
            Keyword_WHERE = 0xe2,
            Null = 0,
            Other = 100,
            Other_Comma = 0x65,
            Other_LeftParen = 0x67,
            Other_Period = 0x66,
            Other_RightParen = 0x68,
            Other_Star = 0x69,
            QuotedIdentifier = 2,
            String = 3
        }
    }
}

