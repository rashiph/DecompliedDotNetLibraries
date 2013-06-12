namespace System.Web
{
    using System;

    internal class HttpServerVarsCollectionEntry
    {
        internal readonly bool IsDynamic;
        internal readonly string Name;
        internal readonly string Value;
        internal readonly DynamicServerVariable Var;

        internal HttpServerVarsCollectionEntry(string name, string value)
        {
            this.Name = name;
            this.Value = value;
            this.IsDynamic = false;
        }

        internal HttpServerVarsCollectionEntry(string name, DynamicServerVariable var)
        {
            this.Name = name;
            this.Var = var;
            this.IsDynamic = true;
        }

        internal string GetValue(HttpRequest request)
        {
            string str = null;
            if (this.IsDynamic)
            {
                if (request != null)
                {
                    str = request.CalcDynamicServerVariable(this.Var);
                }
                return str;
            }
            return this.Value;
        }
    }
}

