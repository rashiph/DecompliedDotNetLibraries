namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    internal class ImageListConverter : ComponentConverter
    {
        public ImageListConverter() : base(typeof(ImageList))
        {
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

