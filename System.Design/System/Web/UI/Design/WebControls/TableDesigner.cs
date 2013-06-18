namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SupportsPreviewControl(true), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class TableDesigner : ControlDesigner
    {
        public override string GetDesignTimeHtml()
        {
            Table viewControl = (Table) base.ViewControl;
            TableRowCollection rows = viewControl.Rows;
            bool flag = rows.Count == 0;
            bool flag2 = false;
            if (flag)
            {
                TableRow row = new TableRow();
                rows.Add(row);
                TableCell cell = new TableCell {
                    Text = "###"
                };
                rows[0].Cells.Add(cell);
            }
            else
            {
                flag2 = true;
                for (int i = 0; i < rows.Count; i++)
                {
                    if (rows[i].Cells.Count != 0)
                    {
                        flag2 = false;
                        break;
                    }
                }
                if (flag2)
                {
                    TableCell cell2 = new TableCell {
                        Text = "###"
                    };
                    rows[0].Cells.Add(cell2);
                }
            }
            if (!flag)
            {
                foreach (TableRow row2 in rows)
                {
                    foreach (TableCell cell3 in row2.Cells)
                    {
                        if ((cell3.Text.Length == 0) && !cell3.HasControls())
                        {
                            cell3.Text = "###";
                        }
                    }
                }
            }
            return base.GetDesignTimeHtml();
        }
    }
}

