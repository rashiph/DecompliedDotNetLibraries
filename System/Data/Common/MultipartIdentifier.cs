namespace System.Data.Common
{
    using System;
    using System.Text;

    internal class MultipartIdentifier
    {
        internal const int CatalogIndex = 1;
        private const int MaxParts = 4;
        internal const int SchemaIndex = 2;
        internal const int ServerIndex = 0;
        internal const int TableIndex = 3;

        private static void IncrementStringCount(string name, string[] ary, ref int position, string property)
        {
            position++;
            int length = ary.Length;
            if (position >= length)
            {
                throw ADP.InvalidMultipartNameToManyParts(property, name, length);
            }
            ary[position] = string.Empty;
        }

        private static bool IsWhitespace(char ch)
        {
            return char.IsWhiteSpace(ch);
        }

        internal static string[] ParseMultipartIdentifier(string name, string leftQuote, string rightQuote, string property, bool ThrowOnEmptyMultipartName)
        {
            return ParseMultipartIdentifier(name, leftQuote, rightQuote, '.', 4, true, property, ThrowOnEmptyMultipartName);
        }

        internal static string[] ParseMultipartIdentifier(string name, string leftQuote, string rightQuote, char separator, int limit, bool removequotes, string property, bool ThrowOnEmptyMultipartName)
        {
            if (limit <= 0)
            {
                throw ADP.InvalidMultipartNameToManyParts(property, name, limit);
            }
            if (((-1 != leftQuote.IndexOf(separator)) || (-1 != rightQuote.IndexOf(separator))) || (leftQuote.Length != rightQuote.Length))
            {
                throw ADP.InvalidMultipartNameIncorrectUsageOfQuotes(property, name);
            }
            string[] ary = new string[limit];
            int index = 0;
            MPIState state = MPIState.MPI_Value;
            StringBuilder builder = new StringBuilder(name.Length);
            StringBuilder builder2 = null;
            char ch2 = ' ';
            for (int i = 0; i < name.Length; i++)
            {
                int num5;
                char ch = name[i];
                switch (state)
                {
                    case MPIState.MPI_Value:
                    {
                        if (!IsWhitespace(ch))
                        {
                            if (ch != separator)
                            {
                                break;
                            }
                            ary[index] = string.Empty;
                            IncrementStringCount(name, ary, ref index, property);
                        }
                        continue;
                    }
                    case MPIState.MPI_ParseNonQuote:
                    {
                        if (ch != separator)
                        {
                            goto Label_0135;
                        }
                        ary[index] = builder.ToString();
                        IncrementStringCount(name, ary, ref index, property);
                        state = MPIState.MPI_Value;
                        continue;
                    }
                    case MPIState.MPI_LookForSeparator:
                    {
                        if (!IsWhitespace(ch))
                        {
                            if (ch != separator)
                            {
                                throw ADP.InvalidMultipartNameIncorrectUsageOfQuotes(property, name);
                            }
                            IncrementStringCount(name, ary, ref index, property);
                            state = MPIState.MPI_Value;
                        }
                        continue;
                    }
                    case MPIState.MPI_LookForNextCharOrSeparator:
                    {
                        if (IsWhitespace(ch))
                        {
                            goto Label_01DD;
                        }
                        if (ch != separator)
                        {
                            goto Label_01BB;
                        }
                        IncrementStringCount(name, ary, ref index, property);
                        state = MPIState.MPI_Value;
                        continue;
                    }
                    case MPIState.MPI_ParseQuote:
                    {
                        if (ch != ch2)
                        {
                            goto Label_0203;
                        }
                        if (!removequotes)
                        {
                            builder.Append(ch);
                        }
                        state = MPIState.MPI_RightQuote;
                        continue;
                    }
                    case MPIState.MPI_RightQuote:
                    {
                        if (ch != ch2)
                        {
                            goto Label_021E;
                        }
                        builder.Append(ch);
                        state = MPIState.MPI_ParseQuote;
                        continue;
                    }
                    default:
                    {
                        continue;
                    }
                }
                if (-1 != (num5 = leftQuote.IndexOf(ch)))
                {
                    ch2 = rightQuote[num5];
                    builder.Length = 0;
                    if (!removequotes)
                    {
                        builder.Append(ch);
                    }
                    state = MPIState.MPI_ParseQuote;
                }
                else
                {
                    if (-1 != rightQuote.IndexOf(ch))
                    {
                        throw ADP.InvalidMultipartNameIncorrectUsageOfQuotes(property, name);
                    }
                    builder.Length = 0;
                    builder.Append(ch);
                    state = MPIState.MPI_ParseNonQuote;
                }
                continue;
            Label_0135:
                if (-1 != rightQuote.IndexOf(ch))
                {
                    throw ADP.InvalidMultipartNameIncorrectUsageOfQuotes(property, name);
                }
                if (-1 != leftQuote.IndexOf(ch))
                {
                    throw ADP.InvalidMultipartNameIncorrectUsageOfQuotes(property, name);
                }
                if (IsWhitespace(ch))
                {
                    ary[index] = builder.ToString();
                    if (builder2 == null)
                    {
                        builder2 = new StringBuilder();
                    }
                    builder2.Length = 0;
                    builder2.Append(ch);
                    state = MPIState.MPI_LookForNextCharOrSeparator;
                }
                else
                {
                    builder.Append(ch);
                }
                continue;
            Label_01BB:
                builder.Append(builder2);
                builder.Append(ch);
                ary[index] = builder.ToString();
                state = MPIState.MPI_ParseNonQuote;
                continue;
            Label_01DD:
                builder2.Append(ch);
                continue;
            Label_0203:
                builder.Append(ch);
                continue;
            Label_021E:
                if (ch == separator)
                {
                    ary[index] = builder.ToString();
                    IncrementStringCount(name, ary, ref index, property);
                    state = MPIState.MPI_Value;
                }
                else
                {
                    if (!IsWhitespace(ch))
                    {
                        throw ADP.InvalidMultipartNameIncorrectUsageOfQuotes(property, name);
                    }
                    ary[index] = builder.ToString();
                    state = MPIState.MPI_LookForSeparator;
                }
            }
            switch (state)
            {
                case MPIState.MPI_Value:
                case MPIState.MPI_LookForSeparator:
                case MPIState.MPI_LookForNextCharOrSeparator:
                    break;

                case MPIState.MPI_ParseNonQuote:
                case MPIState.MPI_RightQuote:
                    ary[index] = builder.ToString();
                    break;

                default:
                    throw ADP.InvalidMultipartNameIncorrectUsageOfQuotes(property, name);
            }
            if (ary[0] == null)
            {
                if (ThrowOnEmptyMultipartName)
                {
                    throw ADP.InvalidMultipartName(property, name);
                }
                return ary;
            }
            int num3 = (limit - index) - 1;
            if (num3 > 0)
            {
                for (int j = limit - 1; j >= num3; j--)
                {
                    ary[j] = ary[j - num3];
                    ary[j - num3] = null;
                }
            }
            return ary;
        }

        private enum MPIState
        {
            MPI_Value,
            MPI_ParseNonQuote,
            MPI_LookForSeparator,
            MPI_LookForNextCharOrSeparator,
            MPI_ParseQuote,
            MPI_RightQuote
        }
    }
}

