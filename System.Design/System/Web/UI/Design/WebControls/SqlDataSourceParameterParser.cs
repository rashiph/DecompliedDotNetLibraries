namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Web.UI.WebControls;

    internal static class SqlDataSourceParameterParser
    {
        public static Parameter[] ParseCommandText(string providerName, string commandText)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                providerName = "System.Data.SqlClient";
            }
            if (string.IsNullOrEmpty(commandText))
            {
                commandText = string.Empty;
            }
            ParameterParser parser = null;
            string str = providerName.ToLowerInvariant();
            if (str != null)
            {
                if (!(str == "system.data.sqlclient") && !(str == "system.data.sqlserverce.4.0"))
                {
                    if ((str == "system.data.odbc") || (str == "system.data.oledb"))
                    {
                        parser = new MiscParameterParser();
                    }
                    else if (str == "system.data.oracleclient")
                    {
                        parser = new OracleClientParameterParser();
                    }
                }
                else
                {
                    parser = new SqlClientParameterParser();
                }
            }
            if (parser == null)
            {
                return new Parameter[0];
            }
            return parser.ParseCommandText(commandText);
        }

        private sealed class MiscParameterParser : SqlDataSourceParameterParser.ParameterParser
        {
            public override Parameter[] ParseCommandText(string commandText)
            {
                int num = 0;
                int length = commandText.Length;
                State inText = State.InText;
                List<Parameter> list = new List<Parameter>();
                while (num < length)
                {
                    switch (inText)
                    {
                        case State.InText:
                        {
                            if (commandText[num] != '\'')
                            {
                                break;
                            }
                            inText = State.InQuote;
                            continue;
                        }
                        case State.InQuote:
                            num++;
                            goto Label_008E;

                        case State.InDoubleQuote:
                            num++;
                            goto Label_00AF;

                        case State.InBracket:
                            num++;
                            goto Label_00D0;

                        case State.InQuestion:
                        {
                            num++;
                            list.Add(new Parameter("?"));
                            inText = State.InText;
                            continue;
                        }
                        default:
                        {
                            continue;
                        }
                    }
                    if (commandText[num] == '"')
                    {
                        inText = State.InDoubleQuote;
                    }
                    else if (commandText[num] == '[')
                    {
                        inText = State.InBracket;
                    }
                    else if (commandText[num] == '?')
                    {
                        inText = State.InQuestion;
                    }
                    else
                    {
                        num++;
                    }
                    continue;
                Label_008A:
                    num++;
                Label_008E:
                    if ((num < length) && (commandText[num] != '\''))
                    {
                        goto Label_008A;
                    }
                    num++;
                    inText = State.InText;
                    continue;
                Label_00AB:
                    num++;
                Label_00AF:
                    if ((num < length) && (commandText[num] != '"'))
                    {
                        goto Label_00AB;
                    }
                    num++;
                    inText = State.InText;
                    continue;
                Label_00CC:
                    num++;
                Label_00D0:
                    if ((num < length) && (commandText[num] != ']'))
                    {
                        goto Label_00CC;
                    }
                    num++;
                    inText = State.InText;
                }
                return list.ToArray();
            }

            private enum State
            {
                InText,
                InQuote,
                InDoubleQuote,
                InBracket,
                InQuestion
            }
        }

        private sealed class OracleClientParameterParser : SqlDataSourceParameterParser.ParameterParser
        {
            private static bool IsValidParamNameChar(char c)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    return (c == '_');
                }
                return true;
            }

            public override Parameter[] ParseCommandText(string commandText)
            {
                int num = 0;
                int length = commandText.Length;
                State inText = State.InText;
                List<Parameter> list = new List<Parameter>();
                StringCollection strings = new StringCollection();
                while (num < length)
                {
                    string str;
                    switch (inText)
                    {
                        case State.InText:
                        {
                            if (commandText[num] != '\'')
                            {
                                break;
                            }
                            inText = State.InQuote;
                            continue;
                        }
                        case State.InQuote:
                            num++;
                            goto Label_009B;

                        case State.InDoubleQuote:
                            num++;
                            goto Label_00BF;

                        case State.InBracket:
                            num++;
                            goto Label_00E3;

                        case State.InParameter:
                            num++;
                            str = string.Empty;
                            goto Label_0120;

                        default:
                        {
                            continue;
                        }
                    }
                    if (commandText[num] == '"')
                    {
                        inText = State.InDoubleQuote;
                    }
                    else if (commandText[num] == '[')
                    {
                        inText = State.InBracket;
                    }
                    else if (commandText[num] == ':')
                    {
                        inText = State.InParameter;
                    }
                    else
                    {
                        num++;
                    }
                    continue;
                Label_0097:
                    num++;
                Label_009B:
                    if ((num < length) && (commandText[num] != '\''))
                    {
                        goto Label_0097;
                    }
                    num++;
                    inText = State.InText;
                    continue;
                Label_00BB:
                    num++;
                Label_00BF:
                    if ((num < length) && (commandText[num] != '"'))
                    {
                        goto Label_00BB;
                    }
                    num++;
                    inText = State.InText;
                    continue;
                Label_00DF:
                    num++;
                Label_00E3:
                    if ((num < length) && (commandText[num] != ']'))
                    {
                        goto Label_00DF;
                    }
                    num++;
                    inText = State.InText;
                    continue;
                Label_0107:
                    str = str + commandText[num];
                    num++;
                Label_0120:
                    if ((num < length) && IsValidParamNameChar(commandText[num]))
                    {
                        goto Label_0107;
                    }
                    Parameter item = new Parameter(str);
                    if (!strings.Contains(str))
                    {
                        list.Add(item);
                        strings.Add(str);
                    }
                    inText = State.InText;
                }
                return list.ToArray();
            }

            private enum State
            {
                InText,
                InQuote,
                InDoubleQuote,
                InBracket,
                InParameter
            }
        }

        private abstract class ParameterParser
        {
            protected ParameterParser()
            {
            }

            public abstract Parameter[] ParseCommandText(string commandText);
        }

        private sealed class SqlClientParameterParser : SqlDataSourceParameterParser.ParameterParser
        {
            private static bool IsValidParamNameChar(char c)
            {
                if ((!char.IsLetterOrDigit(c) && (c != '@')) && ((c != '$') && (c != '#')))
                {
                    return (c == '_');
                }
                return true;
            }

            public override Parameter[] ParseCommandText(string commandText)
            {
                int num = 0;
                int length = commandText.Length;
                State inText = State.InText;
                List<Parameter> list = new List<Parameter>();
                StringCollection strings = new StringCollection();
                while (num < length)
                {
                    string str;
                    switch (inText)
                    {
                        case State.InText:
                        {
                            if (commandText[num] != '\'')
                            {
                                break;
                            }
                            inText = State.InQuote;
                            continue;
                        }
                        case State.InQuote:
                            num++;
                            goto Label_009B;

                        case State.InDoubleQuote:
                            num++;
                            goto Label_00BF;

                        case State.InBracket:
                            num++;
                            goto Label_00E3;

                        case State.InParameter:
                            num++;
                            str = string.Empty;
                            goto Label_0120;

                        default:
                        {
                            continue;
                        }
                    }
                    if (commandText[num] == '"')
                    {
                        inText = State.InDoubleQuote;
                    }
                    else if (commandText[num] == '[')
                    {
                        inText = State.InBracket;
                    }
                    else if (commandText[num] == '@')
                    {
                        inText = State.InParameter;
                    }
                    else
                    {
                        num++;
                    }
                    continue;
                Label_0097:
                    num++;
                Label_009B:
                    if ((num < length) && (commandText[num] != '\''))
                    {
                        goto Label_0097;
                    }
                    num++;
                    inText = State.InText;
                    continue;
                Label_00BB:
                    num++;
                Label_00BF:
                    if ((num < length) && (commandText[num] != '"'))
                    {
                        goto Label_00BB;
                    }
                    num++;
                    inText = State.InText;
                    continue;
                Label_00DF:
                    num++;
                Label_00E3:
                    if ((num < length) && (commandText[num] != ']'))
                    {
                        goto Label_00DF;
                    }
                    num++;
                    inText = State.InText;
                    continue;
                Label_0107:
                    str = str + commandText[num];
                    num++;
                Label_0120:
                    if ((num < length) && IsValidParamNameChar(commandText[num]))
                    {
                        goto Label_0107;
                    }
                    if (!str.StartsWith("@", StringComparison.Ordinal))
                    {
                        Parameter item = new Parameter(str);
                        if (!strings.Contains(str))
                        {
                            list.Add(item);
                            strings.Add(str);
                        }
                    }
                    inText = State.InText;
                }
                return list.ToArray();
            }

            private enum State
            {
                InText,
                InQuote,
                InDoubleQuote,
                InBracket,
                InParameter
            }
        }
    }
}

