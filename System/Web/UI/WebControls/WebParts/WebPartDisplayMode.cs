namespace System.Web.UI.WebControls.WebParts
{
    using System;

    public abstract class WebPartDisplayMode
    {
        private string _name;

        protected WebPartDisplayMode(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            this._name = name;
        }

        public virtual bool IsEnabled(WebPartManager webPartManager)
        {
            if (this.RequiresPersonalization)
            {
                return webPartManager.Personalization.IsModifiable;
            }
            return true;
        }

        public virtual bool AllowPageDesign
        {
            get
            {
                return false;
            }
        }

        public virtual bool AssociatedWithToolZone
        {
            get
            {
                return false;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public virtual bool RequiresPersonalization
        {
            get
            {
                return false;
            }
        }

        public virtual bool ShowHiddenWebParts
        {
            get
            {
                return false;
            }
        }
    }
}

