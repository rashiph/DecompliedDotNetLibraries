namespace System.Web.UI.Design
{
    using System;

    public class ViewRendering
    {
        private string _content;
        private DesignerRegionCollection _regions;
        private bool _visible;

        public ViewRendering(string content, DesignerRegionCollection regions) : this(content, regions, true)
        {
        }

        public ViewRendering(string content, DesignerRegionCollection regions, bool visible)
        {
            this._content = content;
            this._regions = regions;
            this._visible = visible;
        }

        public string Content
        {
            get
            {
                if (this._content == null)
                {
                    return string.Empty;
                }
                return this._content;
            }
        }

        public DesignerRegionCollection Regions
        {
            get
            {
                if (this._regions == null)
                {
                    this._regions = new DesignerRegionCollection();
                }
                return this._regions;
            }
        }

        public bool Visible
        {
            get
            {
                return this._visible;
            }
        }
    }
}

