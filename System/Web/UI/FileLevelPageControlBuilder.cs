namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.UI.WebControls;

    public class FileLevelPageControlBuilder : RootBuilder
    {
        private bool _containsContentPage;
        private ArrayList _contentBuilderEntries;
        private ControlBuilder _firstControlBuilder;
        private int _firstLiteralLineNumber;
        private string _firstLiteralText;

        internal virtual void AddContentTemplate(object obj, string templateName, ITemplate template)
        {
            ((Page) obj).AddContentTemplate(templateName, template);
        }

        public override void AppendLiteralString(string text)
        {
            if ((this._firstLiteralText == null) && !Util.IsWhiteSpaceString(text))
            {
                int offset = Util.FirstNonWhiteSpaceIndex(text);
                if (offset < 0)
                {
                    offset = 0;
                }
                this._firstLiteralLineNumber = base.Parser._lineNumber - Util.LineCount(text, offset, text.Length);
                this._firstLiteralText = text;
                if (this._containsContentPage)
                {
                    throw new HttpException(System.Web.SR.GetString("Only_Content_supported_on_content_page"));
                }
            }
            base.AppendLiteralString(text);
        }

        public override void AppendSubBuilder(ControlBuilder subBuilder)
        {
            if (subBuilder is ContentBuilderInternal)
            {
                ContentBuilderInternal internal2 = (ContentBuilderInternal) subBuilder;
                this._containsContentPage = true;
                if (this._contentBuilderEntries == null)
                {
                    this._contentBuilderEntries = new ArrayList();
                }
                if (this._firstLiteralText != null)
                {
                    throw new HttpParseException(System.Web.SR.GetString("Only_Content_supported_on_content_page"), null, base.Parser.CurrentVirtualPath, this._firstLiteralText, this._firstLiteralLineNumber);
                }
                if (this._firstControlBuilder != null)
                {
                    base.Parser._lineNumber = this._firstControlBuilder.Line;
                    throw new HttpException(System.Web.SR.GetString("Only_Content_supported_on_content_page"));
                }
                TemplatePropertyEntry entry = new TemplatePropertyEntry {
                    Filter = internal2.ContentPlaceHolderFilter,
                    Name = internal2.ContentPlaceHolder,
                    Builder = internal2
                };
                this._contentBuilderEntries.Add(entry);
            }
            else if (this._firstControlBuilder == null)
            {
                if (this._containsContentPage)
                {
                    throw new HttpException(System.Web.SR.GetString("Only_Content_supported_on_content_page"));
                }
                this._firstControlBuilder = subBuilder;
            }
            base.AppendSubBuilder(subBuilder);
        }

        internal override void InitObject(object obj)
        {
            base.InitObject(obj);
            if (this._contentBuilderEntries != null)
            {
                foreach (TemplatePropertyEntry entry in base.GetFilteredPropertyEntrySet(this._contentBuilderEntries))
                {
                    ContentBuilderInternal builder = (ContentBuilderInternal) entry.Builder;
                    try
                    {
                        builder.SetServiceProvider(base.ServiceProvider);
                        this.AddContentTemplate(obj, builder.ContentPlaceHolder, builder.BuildObject() as ITemplate);
                    }
                    finally
                    {
                        builder.SetServiceProvider(null);
                    }
                }
            }
        }

        internal override void SortEntries()
        {
            base.SortEntries();
            ControlBuilder.FilteredPropertyEntryComparer comparer = null;
            base.ProcessAndSortPropertyEntries(this._contentBuilderEntries, ref comparer);
        }

        internal ICollection ContentBuilderEntries
        {
            get
            {
                return this._contentBuilderEntries;
            }
        }
    }
}

