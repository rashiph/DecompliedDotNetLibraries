namespace System.Drawing.Design
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class UITypeEditor
    {
        static UITypeEditor()
        {
            Hashtable table = new Hashtable();
            table[typeof(DateTime)] = "System.ComponentModel.Design.DateTimeEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            table[typeof(Array)] = "System.ComponentModel.Design.ArrayEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            table[typeof(IList)] = "System.ComponentModel.Design.CollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            table[typeof(ICollection)] = "System.ComponentModel.Design.CollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            table[typeof(byte[])] = "System.ComponentModel.Design.BinaryEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            table[typeof(Stream)] = "System.ComponentModel.Design.BinaryEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            table[typeof(string[])] = "System.Windows.Forms.Design.StringArrayEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            table[typeof(Collection<string>)] = "System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            TypeDescriptor.AddEditorTable(typeof(UITypeEditor), table);
        }

        public object EditValue(IServiceProvider provider, object value)
        {
            return this.EditValue(null, provider, value);
        }

        public virtual object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            return value;
        }

        public UITypeEditorEditStyle GetEditStyle()
        {
            return this.GetEditStyle(null);
        }

        public virtual UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.None;
        }

        public bool GetPaintValueSupported()
        {
            return this.GetPaintValueSupported(null);
        }

        public virtual bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return false;
        }

        public virtual void PaintValue(PaintValueEventArgs e)
        {
        }

        public void PaintValue(object value, Graphics canvas, Rectangle rectangle)
        {
            this.PaintValue(new PaintValueEventArgs(null, value, canvas, rectangle));
        }

        public virtual bool IsDropDownResizable
        {
            get
            {
                return false;
            }
        }
    }
}

