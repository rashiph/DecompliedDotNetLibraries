namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    public class TreeNodeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if ((destinationType == typeof(InstanceDescriptor)) && (value is TreeNode))
            {
                TreeNode node = (TreeNode) value;
                MemberInfo member = null;
                object[] arguments = null;
                if ((node.ImageIndex == -1) || (node.SelectedImageIndex == -1))
                {
                    if (node.Nodes.Count == 0)
                    {
                        member = typeof(TreeNode).GetConstructor(new System.Type[] { typeof(string) });
                        arguments = new object[] { node.Text };
                    }
                    else
                    {
                        member = typeof(TreeNode).GetConstructor(new System.Type[] { typeof(string), typeof(TreeNode[]) });
                        TreeNode[] dest = new TreeNode[node.Nodes.Count];
                        node.Nodes.CopyTo(dest, 0);
                        arguments = new object[] { node.Text, dest };
                    }
                }
                else if (node.Nodes.Count == 0)
                {
                    member = typeof(TreeNode).GetConstructor(new System.Type[] { typeof(string), typeof(int), typeof(int) });
                    arguments = new object[] { node.Text, node.ImageIndex, node.SelectedImageIndex };
                }
                else
                {
                    member = typeof(TreeNode).GetConstructor(new System.Type[] { typeof(string), typeof(int), typeof(int), typeof(TreeNode[]) });
                    TreeNode[] nodeArray2 = new TreeNode[node.Nodes.Count];
                    node.Nodes.CopyTo(nodeArray2, 0);
                    arguments = new object[] { node.Text, node.ImageIndex, node.SelectedImageIndex, nodeArray2 };
                }
                if (member != null)
                {
                    return new InstanceDescriptor(member, arguments, false);
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

