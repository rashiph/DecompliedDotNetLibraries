namespace System.Web.UI.Design
{
    using System;
    using System.Collections;

    public abstract class WebFormsReferenceManager
    {
        protected WebFormsReferenceManager()
        {
        }

        public abstract ICollection GetRegisterDirectives();
        public abstract string GetTagPrefix(Type objectType);
        public abstract Type GetType(string tagPrefix, string tagName);
        public abstract string GetUserControlPath(string tagPrefix, string tagName);
        public abstract string RegisterTagPrefix(Type objectType);
    }
}

