namespace System.Data.SqlClient
{
    using System;
    using System.Data.Common;
    using System.Text;

    internal static class SqlServerEscapeHelper
    {
        internal static string EscapeIdentifier(string name)
        {
            return ("[" + name.Replace("]", "]]") + "]");
        }

        internal static void EscapeIdentifier(StringBuilder builder, string name)
        {
            builder.Append("[");
            builder.Append(name.Replace("]", "]]"));
            builder.Append("]");
        }

        internal static string EscapeStringAsLiteral(string input)
        {
            return input.Replace("'", "''");
        }

        internal static string MakeStringLiteral(string input)
        {
            if (ADP.IsEmpty(input))
            {
                return "''";
            }
            return ("'" + EscapeStringAsLiteral(input) + "'");
        }
    }
}

