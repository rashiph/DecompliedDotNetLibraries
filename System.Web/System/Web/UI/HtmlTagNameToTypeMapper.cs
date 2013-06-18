namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.UI.HtmlControls;
    using System.Web.Util;

    internal class HtmlTagNameToTypeMapper : ITagNameToTypeMapper
    {
        private static Hashtable _inputTypes;
        private static Hashtable _tagMap;

        internal HtmlTagNameToTypeMapper()
        {
        }

        Type ITagNameToTypeMapper.GetControlType(string tagName, IDictionary attributeBag)
        {
            Type type;
            if (_tagMap == null)
            {
                Hashtable hashtable = new Hashtable(10, StringComparer.OrdinalIgnoreCase);
                hashtable.Add("a", typeof(HtmlAnchor));
                hashtable.Add("button", typeof(HtmlButton));
                hashtable.Add("form", typeof(HtmlForm));
                hashtable.Add("head", typeof(HtmlHead));
                hashtable.Add("img", typeof(HtmlImage));
                hashtable.Add("textarea", typeof(HtmlTextArea));
                hashtable.Add("select", typeof(HtmlSelect));
                hashtable.Add("table", typeof(HtmlTable));
                hashtable.Add("tr", typeof(HtmlTableRow));
                hashtable.Add("td", typeof(HtmlTableCell));
                hashtable.Add("th", typeof(HtmlTableCell));
                _tagMap = hashtable;
            }
            if (_inputTypes == null)
            {
                Hashtable hashtable2 = new Hashtable(10, StringComparer.OrdinalIgnoreCase);
                hashtable2.Add("text", typeof(HtmlInputText));
                hashtable2.Add("password", typeof(HtmlInputPassword));
                hashtable2.Add("button", typeof(HtmlInputButton));
                hashtable2.Add("submit", typeof(HtmlInputSubmit));
                hashtable2.Add("reset", typeof(HtmlInputReset));
                hashtable2.Add("image", typeof(HtmlInputImage));
                hashtable2.Add("checkbox", typeof(HtmlInputCheckBox));
                hashtable2.Add("radio", typeof(HtmlInputRadioButton));
                hashtable2.Add("hidden", typeof(HtmlInputHidden));
                hashtable2.Add("file", typeof(HtmlInputFile));
                _inputTypes = hashtable2;
            }
            if (StringUtil.EqualsIgnoreCase("input", tagName))
            {
                string str = (string) attributeBag["type"];
                if (str == null)
                {
                    str = "text";
                }
                type = (Type) _inputTypes[str];
                if (type == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Invalid_type_for_input_tag", new object[] { str }));
                }
                return type;
            }
            type = (Type) _tagMap[tagName];
            if (type == null)
            {
                type = typeof(HtmlGenericControl);
            }
            return type;
        }
    }
}

