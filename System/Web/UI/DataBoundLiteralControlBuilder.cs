namespace System.Web.UI
{
    using System;

    internal class DataBoundLiteralControlBuilder : ControlBuilder
    {
        internal DataBoundLiteralControlBuilder()
        {
        }

        internal void AddDataBindingExpression(CodeBlockBuilder codeBlockBuilder)
        {
            object lastBuilder = base.GetLastBuilder();
            if ((lastBuilder == null) || (lastBuilder is CodeBlockBuilder))
            {
                base.AddSubBuilder(null);
            }
            base.AddSubBuilder(codeBlockBuilder);
        }

        internal void AddLiteralString(string s)
        {
            object lastBuilder = base.GetLastBuilder();
            if ((lastBuilder != null) && (lastBuilder is string))
            {
                base.AddSubBuilder(null);
            }
            base.AddSubBuilder(s);
        }

        internal int GetDataBoundLiteralCount()
        {
            return (base.SubBuilders.Count / 2);
        }

        internal int GetStaticLiteralsCount()
        {
            return ((base.SubBuilders.Count + 1) / 2);
        }
    }
}

