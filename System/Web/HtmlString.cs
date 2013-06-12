namespace System.Web
{
    using System;

    public class HtmlString : IHtmlString
    {
        private string _htmlString;

        public HtmlString(string value)
        {
            this._htmlString = value;
        }

        public string ToHtmlString()
        {
            return this._htmlString;
        }

        public override string ToString()
        {
            return this._htmlString;
        }
    }
}

