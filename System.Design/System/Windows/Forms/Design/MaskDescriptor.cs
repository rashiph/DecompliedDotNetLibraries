namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    public abstract class MaskDescriptor
    {
        protected MaskDescriptor()
        {
        }

        public override bool Equals(object maskDescriptor)
        {
            MaskDescriptor descriptor = maskDescriptor as MaskDescriptor;
            if (!IsValidMaskDescriptor(descriptor) || !IsValidMaskDescriptor(this))
            {
                return (this == maskDescriptor);
            }
            return ((this.Mask == descriptor.Mask) && (this.ValidatingType == descriptor.ValidatingType));
        }

        public override int GetHashCode()
        {
            string mask = this.Mask;
            if (this.ValidatingType != null)
            {
                mask = mask + this.ValidatingType.ToString();
            }
            return mask.GetHashCode();
        }

        public static bool IsValidMaskDescriptor(MaskDescriptor maskDescriptor)
        {
            string str;
            return IsValidMaskDescriptor(maskDescriptor, out str);
        }

        public static bool IsValidMaskDescriptor(MaskDescriptor maskDescriptor, out string validationErrorDescription)
        {
            validationErrorDescription = string.Empty;
            if (maskDescriptor == null)
            {
                validationErrorDescription = System.Design.SR.GetString("MaskDescriptorNull");
                return false;
            }
            if ((string.IsNullOrEmpty(maskDescriptor.Mask) || string.IsNullOrEmpty(maskDescriptor.Name)) || string.IsNullOrEmpty(maskDescriptor.Sample))
            {
                validationErrorDescription = System.Design.SR.GetString("MaskDescriptorNullOrEmptyRequiredProperty");
                return false;
            }
            MaskedTextProvider maskedTextProvider = new MaskedTextProvider(maskDescriptor.Mask, maskDescriptor.Culture);
            MaskedTextBox box = new MaskedTextBox(maskedTextProvider) {
                SkipLiterals = true,
                ResetOnPrompt = true,
                ResetOnSpace = true,
                ValidatingType = maskDescriptor.ValidatingType,
                FormatProvider = maskDescriptor.Culture,
                Culture = maskDescriptor.Culture
            };
            box.TypeValidationCompleted += new TypeValidationEventHandler(MaskDescriptor.maskedTextBox1_TypeValidationCompleted);
            box.MaskInputRejected += new MaskInputRejectedEventHandler(MaskDescriptor.maskedTextBox1_MaskInputRejected);
            box.Text = maskDescriptor.Sample;
            if ((box.Tag == null) && (maskDescriptor.ValidatingType != null))
            {
                box.ValidateText();
            }
            if (box.Tag != null)
            {
                validationErrorDescription = box.Tag.ToString();
            }
            return (validationErrorDescription.Length == 0);
        }

        private static void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            MaskedTextBox box = sender as MaskedTextBox;
            box.Tag = MaskedTextBoxDesigner.GetMaskInputRejectedErrorMessage(e);
        }

        private static void maskedTextBox1_TypeValidationCompleted(object sender, TypeValidationEventArgs e)
        {
            if (!e.IsValidInput)
            {
                MaskedTextBox box = sender as MaskedTextBox;
                box.Tag = e.Message;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}<Name={1}, Mask={2}, ValidatingType={3}", new object[] { base.GetType(), (this.Name != null) ? this.Name : "null", (this.Mask != null) ? this.Mask : "null", (this.ValidatingType != null) ? this.ValidatingType.ToString() : "null" });
        }

        public virtual CultureInfo Culture
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture;
            }
        }

        public abstract string Mask { get; }

        public abstract string Name { get; }

        public abstract string Sample { get; }

        public abstract System.Type ValidatingType { get; }
    }
}

