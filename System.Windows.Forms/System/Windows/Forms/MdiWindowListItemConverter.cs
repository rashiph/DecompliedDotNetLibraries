namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    internal class MdiWindowListItemConverter : ComponentConverter
    {
        public MdiWindowListItemConverter(System.Type type) : base(type)
        {
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            MenuStrip instance = context.Instance as MenuStrip;
            if (instance == null)
            {
                return base.GetStandardValues(context);
            }
            TypeConverter.StandardValuesCollection standardValues = base.GetStandardValues(context);
            ArrayList values = new ArrayList();
            int count = standardValues.Count;
            for (int i = 0; i < count; i++)
            {
                ToolStripItem item = standardValues[i] as ToolStripItem;
                if ((item != null) && (item.Owner == instance))
                {
                    values.Add(item);
                }
            }
            return new TypeConverter.StandardValuesCollection(values);
        }
    }
}

