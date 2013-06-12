namespace System.Windows.Forms
{
    using System;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
    public sealed class RelatedImageListAttribute : Attribute
    {
        private string relatedImageList;

        public RelatedImageListAttribute(string relatedImageList)
        {
            this.relatedImageList = relatedImageList;
        }

        public string RelatedImageList
        {
            get
            {
                return this.relatedImageList;
            }
        }
    }
}

