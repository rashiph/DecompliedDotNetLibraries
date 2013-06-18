namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [ParseChildren(true, "ChildNodes")]
    public class TreeNode : IStateManager, ICloneable
    {
        private TreeNodeCollection _childNodes;
        private object _dataItem;
        private int _depth;
        private int _index;
        private string _internalValuePath;
        private bool _isRoot;
        private bool _isTrackingViewState;
        private bool _modifyCheckedNodes;
        private TreeView _owner;
        private TreeNode _parent;
        private string _parentIsLast;
        private bool _populateDesired;
        private int _selectDesired;
        private string _toggleNodeAttributeValue;
        private string _valuePath;
        private StateBag _viewState;

        public TreeNode()
        {
            this._depth = -2;
            this._selectDesired = 0;
        }

        public TreeNode(string text) : this(text, null, null, null, null)
        {
        }

        public TreeNode(string text, string value) : this(text, value, null, null, null)
        {
        }

        protected internal TreeNode(TreeView owner, bool isRoot) : this()
        {
            this._owner = owner;
            this._isRoot = isRoot;
        }

        public TreeNode(string text, string value, string imageUrl) : this(text, value, imageUrl, null, null)
        {
        }

        public TreeNode(string text, string value, string imageUrl, string navigateUrl, string target) : this()
        {
            if (text != null)
            {
                this.Text = text;
            }
            if (value != null)
            {
                this.Value = value;
            }
            if (!string.IsNullOrEmpty(imageUrl))
            {
                this.ImageUrl = imageUrl;
            }
            if (!string.IsNullOrEmpty(navigateUrl))
            {
                this.NavigateUrl = navigateUrl;
            }
            if (!string.IsNullOrEmpty(target))
            {
                this.Target = target;
            }
        }

        private void ApplyAttributeList(HtmlTextWriter writer, ArrayList list)
        {
            for (int i = 0; i < list.Count; i += 2)
            {
                object obj2 = list[i];
                if (obj2 is string)
                {
                    writer.AddAttribute((string) obj2, (string) list[i + 1]);
                }
                else
                {
                    writer.AddAttribute((HtmlTextWriterAttribute) obj2, (string) list[i + 1]);
                }
            }
        }

        protected virtual object Clone()
        {
            TreeNode node = new TreeNode {
                Checked = this.Checked,
                Expanded = this.Expanded,
                ImageUrl = this.ImageUrl,
                ImageToolTip = this.ImageToolTip,
                NavigateUrl = this.NavigateUrl,
                PopulateOnDemand = this.PopulateOnDemand,
                SelectAction = this.SelectAction,
                Selected = this.Selected
            };
            if (this.ViewState["ShowCheckBox"] != null)
            {
                node.ShowCheckBox = this.ShowCheckBox;
            }
            node.Target = this.Target;
            node.Text = this.Text;
            node.ToolTip = this.ToolTip;
            node.Value = this.Value;
            return node;
        }

        public void Collapse()
        {
            this.Expanded = false;
        }

        public void CollapseAll()
        {
            this.SetExpandedRecursive(false);
        }

        public void Expand()
        {
            this.Expanded = true;
        }

        public void ExpandAll()
        {
            this.SetExpandedRecursive(true);
        }

        internal bool GetEffectiveShowCheckBox()
        {
            return this.GetEffectiveShowCheckBox(this.GetTreeNodeType());
        }

        private bool GetEffectiveShowCheckBox(TreeNodeTypes type)
        {
            if (this.ShowCheckBox == true)
            {
                return true;
            }
            if (this.ShowCheckBox == false)
            {
                return false;
            }
            return ((this._owner.ShowCheckBoxes & type) != TreeNodeTypes.None);
        }

        internal TreeNode GetParentInternal()
        {
            return this._parent;
        }

        private string GetPopulateNodeAttribute(HtmlTextWriter writer, string myId, string selectId, string selectImageId, string lineType, int depth, bool[] isLast)
        {
            string str = string.Empty;
            if (this._parentIsLast == null)
            {
                char[] chArray = new char[depth + 1];
                for (int i = 0; i < (depth + 1); i++)
                {
                    if (isLast[i])
                    {
                        chArray[i] = 't';
                    }
                    else
                    {
                        chArray[i] = 'f';
                    }
                }
                this._parentIsLast = new string(chArray);
            }
            string str2 = this.Index.ToString(CultureInfo.InvariantCulture);
            if (this._owner.IsNotIE)
            {
                str = string.Concat(new object[] { 
                    "javascript:TreeView_PopulateNode(", this._owner.ClientDataObjectID, ",", str2, ",document.getElementById('", myId, "'),document.getElementById('", selectId, "'),", (selectImageId.Length == 0) ? "null" : ("document.getElementById('" + selectImageId + "')"), ",'", lineType, "','", Util.QuoteJScriptString(this.Text, true), "','", Util.QuoteJScriptString(this.InternalValuePath, true), 
                    "','", this.DataBound ? 't' : 'f', "','", Util.QuoteJScriptString(this.DataPath, true), "','", this._parentIsLast, "')"
                 });
            }
            else
            {
                str = string.Concat(new object[] { 
                    "javascript:TreeView_PopulateNode(", this._owner.ClientDataObjectID, ",", str2, ",", myId, ",", selectId, ",", (selectImageId.Length == 0) ? "null" : selectImageId, ",'", lineType, "','", Util.QuoteJScriptString(this.Text, true), "','", Util.QuoteJScriptString(this.InternalValuePath, true), 
                    "','", this.DataBound ? 't' : 'f', "','", Util.QuoteJScriptString(this.DataPath, true), "','", this._parentIsLast, "')"
                 });
            }
            if (this._owner.Page != null)
            {
                this._owner.Page.ClientScript.RegisterForEventValidation(this._owner.UniqueID, str2 + this.Text + this.InternalValuePath + this.DataPath);
            }
            return str;
        }

        private string GetToggleNodeAttributeValue(string myId, string lineType)
        {
            if (this._toggleNodeAttributeValue == null)
            {
                if (this._owner.IsNotIE)
                {
                    this._toggleNodeAttributeValue = "javascript:TreeView_ToggleNode(" + this._owner.ClientDataObjectID + "," + this.Index.ToString(CultureInfo.InvariantCulture) + ",document.getElementById('" + myId + "'),'" + lineType + "',document.getElementById('" + myId + "Nodes'))";
                }
                else
                {
                    this._toggleNodeAttributeValue = "javascript:TreeView_ToggleNode(" + this._owner.ClientDataObjectID + "," + this.Index.ToString(CultureInfo.InvariantCulture) + "," + myId + ",'" + lineType + "'," + myId + "Nodes)";
                }
            }
            return this._toggleNodeAttributeValue;
        }

        private TreeNodeTypes GetTreeNodeType()
        {
            TreeNodeTypes leaf = TreeNodeTypes.Leaf;
            if ((this.Depth == 0) && (this.ChildNodes.Count > 0))
            {
                return TreeNodeTypes.Root;
            }
            if ((this.ChildNodes.Count <= 0) && !this.PopulateOnDemand)
            {
                return leaf;
            }
            return TreeNodeTypes.Parent;
        }

        protected virtual void LoadViewState(object state)
        {
            object[] objArray = (object[]) state;
            if (objArray != null)
            {
                if (objArray[0] != null)
                {
                    ((IStateManager) this.ViewState).LoadViewState(objArray[0]);
                    this.NotifyOwnerChecked();
                }
                if (objArray[1] != null)
                {
                    ((IStateManager) this.ChildNodes).LoadViewState(objArray[1]);
                }
            }
        }

        private void NotifyOwnerChecked()
        {
            if (this._owner == null)
            {
                this._modifyCheckedNodes = true;
            }
            else
            {
                object obj2 = this.ViewState["Checked"];
                if ((obj2 != null) && ((bool) obj2))
                {
                    if (!this._owner.CheckedNodes.Contains(this))
                    {
                        this._owner.CheckedNodes.Add(this);
                    }
                }
                else
                {
                    this._owner.CheckedNodes.Remove(this);
                }
            }
        }

        internal void Populate()
        {
            if (!this.Populated && (this.ChildNodes.Count == 0))
            {
                if (this._owner != null)
                {
                    this._owner.PopulateNode(this);
                }
                else
                {
                    this._populateDesired = true;
                }
            }
        }

        internal void Render(HtmlTextWriter writer, int position, bool[] isLast, bool enabled)
        {
            string str = string.Empty;
            str = this._owner.CreateNodeId(this.Index);
            int depth = this.Depth;
            bool flag = false;
            if (depth > -1)
            {
                flag = isLast[depth];
            }
            bool flag2 = this.Expanded == true;
            TreeNodeStyle style = this._owner.GetStyle(this);
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            if (((style != null) && !style.NodeSpacing.IsEmpty) && ((depth != 0) || (position != 0)))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, style.NodeSpacing.ToString(CultureInfo.InvariantCulture));
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            if (depth > 0)
            {
                for (int i = 0; i < depth; i++)
                {
                    if (writer is Html32TextWriter)
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Width, this._owner.NodeIndent.ToString(CultureInfo.InvariantCulture) + "px");
                        writer.RenderBeginTag(HtmlTextWriterTag.Table);
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        if (this._owner.ShowLines && !isLast[i])
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.Src, this._owner.GetImageUrl(6));
                            writer.AddAttribute(HtmlTextWriterAttribute.Alt, string.Empty);
                            writer.RenderBeginTag(HtmlTextWriterTag.Img);
                            writer.RenderEndTag();
                        }
                        writer.RenderEndTag();
                        writer.RenderEndTag();
                        writer.RenderEndTag();
                        writer.RenderEndTag();
                    }
                    else
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        writer.Write("<div style=\"width:" + this._owner.NodeIndent.ToString(CultureInfo.InvariantCulture) + "px;height:1px\">");
                        if (this._owner.ShowLines && !isLast[i])
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.Src, this._owner.GetImageUrl(6));
                            writer.AddAttribute(HtmlTextWriterAttribute.Alt, string.Empty);
                            writer.RenderBeginTag(HtmlTextWriterTag.Img);
                            writer.RenderEndTag();
                        }
                        writer.Write("</div>");
                        writer.RenderEndTag();
                    }
                }
            }
            bool flag3 = (this.PopulateOnDemand || (this.ChildNodes.Count > 0)) && this._owner.ShowExpandCollapse;
            string imageUrl = string.Empty;
            string lineType = " ";
            string format = string.Empty;
            if (flag)
            {
                if (flag3)
                {
                    if (flag2)
                    {
                        if (this._owner.ShowLines)
                        {
                            if (depth == 0)
                            {
                                if (position == 0)
                                {
                                    lineType = "-";
                                    imageUrl = this._owner.GetImageUrl(0x12);
                                    format = this._owner.CollapseImageToolTip;
                                }
                                else
                                {
                                    lineType = "l";
                                    imageUrl = this._owner.GetImageUrl(15);
                                    format = this._owner.CollapseImageToolTip;
                                }
                            }
                            else
                            {
                                lineType = "l";
                                imageUrl = this._owner.GetImageUrl(15);
                                format = this._owner.CollapseImageToolTip;
                            }
                        }
                        else
                        {
                            imageUrl = this._owner.GetImageUrl(5);
                            format = this._owner.CollapseImageToolTip;
                        }
                    }
                    else if (this._owner.ShowLines)
                    {
                        if (depth == 0)
                        {
                            if (position == 0)
                            {
                                lineType = "-";
                                imageUrl = this._owner.GetImageUrl(0x11);
                                format = this._owner.ExpandImageToolTip;
                            }
                            else
                            {
                                lineType = "l";
                                imageUrl = this._owner.GetImageUrl(14);
                                format = this._owner.ExpandImageToolTip;
                            }
                        }
                        else
                        {
                            lineType = "l";
                            imageUrl = this._owner.GetImageUrl(14);
                            format = this._owner.ExpandImageToolTip;
                        }
                    }
                    else
                    {
                        imageUrl = this._owner.GetImageUrl(4);
                        format = this._owner.ExpandImageToolTip;
                    }
                }
                else if (this._owner.ShowLines)
                {
                    if (depth == 0)
                    {
                        if (position == 0)
                        {
                            lineType = "-";
                            imageUrl = this._owner.GetImageUrl(0x10);
                        }
                        else
                        {
                            lineType = "l";
                            imageUrl = this._owner.GetImageUrl(13);
                        }
                    }
                    else
                    {
                        lineType = "l";
                        imageUrl = this._owner.GetImageUrl(13);
                    }
                }
                else if (this._owner.ShowExpandCollapse)
                {
                    imageUrl = this._owner.GetImageUrl(3);
                }
            }
            else if (flag3)
            {
                if (flag2)
                {
                    if (this._owner.ShowLines)
                    {
                        if (depth == 0)
                        {
                            if (position == 0)
                            {
                                lineType = "r";
                                imageUrl = this._owner.GetImageUrl(9);
                                format = this._owner.CollapseImageToolTip;
                            }
                            else
                            {
                                lineType = "t";
                                imageUrl = this._owner.GetImageUrl(12);
                                format = this._owner.CollapseImageToolTip;
                            }
                        }
                        else
                        {
                            lineType = "t";
                            imageUrl = this._owner.GetImageUrl(12);
                            format = this._owner.CollapseImageToolTip;
                        }
                    }
                    else
                    {
                        imageUrl = this._owner.GetImageUrl(5);
                        format = this._owner.CollapseImageToolTip;
                    }
                }
                else if (this._owner.ShowLines)
                {
                    if (depth == 0)
                    {
                        if (position == 0)
                        {
                            lineType = "r";
                            imageUrl = this._owner.GetImageUrl(8);
                            format = this._owner.ExpandImageToolTip;
                        }
                        else
                        {
                            lineType = "t";
                            imageUrl = this._owner.GetImageUrl(11);
                            format = this._owner.ExpandImageToolTip;
                        }
                    }
                    else
                    {
                        lineType = "t";
                        imageUrl = this._owner.GetImageUrl(11);
                        format = this._owner.ExpandImageToolTip;
                    }
                }
                else
                {
                    imageUrl = this._owner.GetImageUrl(4);
                    format = this._owner.ExpandImageToolTip;
                }
            }
            else if (this._owner.ShowLines)
            {
                if (depth == 0)
                {
                    if (position == 0)
                    {
                        lineType = "r";
                        imageUrl = this._owner.GetImageUrl(7);
                    }
                    else
                    {
                        lineType = "t";
                        imageUrl = this._owner.GetImageUrl(10);
                    }
                }
                else
                {
                    lineType = "t";
                    imageUrl = this._owner.GetImageUrl(10);
                }
            }
            else if (this._owner.ShowExpandCollapse)
            {
                imageUrl = this._owner.GetImageUrl(3);
            }
            TreeNodeTypes treeNodeType = this.GetTreeNodeType();
            string levelImageUrl = string.Empty;
            if (this.ImageUrl.Length > 0)
            {
                levelImageUrl = this._owner.ResolveClientUrl(this.ImageUrl);
            }
            else if (((depth < this._owner.LevelStyles.Count) && (this._owner.LevelStyles[depth] != null)) && (style.ImageUrl.Length > 0))
            {
                levelImageUrl = this._owner.GetLevelImageUrl(depth);
            }
            else
            {
                switch (treeNodeType)
                {
                    case TreeNodeTypes.Root:
                        levelImageUrl = this._owner.GetImageUrl(0);
                        break;

                    case TreeNodeTypes.Parent:
                        levelImageUrl = this._owner.GetImageUrl(1);
                        break;

                    case TreeNodeTypes.Leaf:
                        levelImageUrl = this._owner.GetImageUrl(2);
                        break;
                }
            }
            string selectImageId = string.Empty;
            if (levelImageUrl.Length > 0)
            {
                selectImageId = this.SelectID + "i";
            }
            if (imageUrl.Length > 0)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                if (flag3)
                {
                    if (this._owner.RenderClientScript && !this._owner.CustomExpandCollapseHandlerExists)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Id, str);
                        if (this.PopulateOnDemand)
                        {
                            if (this._owner.PopulateNodesFromClient)
                            {
                                if (this.ChildNodes.Count != 0)
                                {
                                    throw new InvalidOperationException(System.Web.SR.GetString("TreeView_PopulateOnlyEmptyNodes", new object[] { this._owner.ID }));
                                }
                                writer.AddAttribute(HtmlTextWriterAttribute.Href, this.GetPopulateNodeAttribute(writer, str, this.SelectID, selectImageId, lineType, depth, isLast));
                            }
                            else
                            {
                                string str7 = "javascript:0";
                                if (this._owner.Page != null)
                                {
                                    str7 = this._owner.Page.ClientScript.GetPostBackClientHyperlink(this._owner, "t" + this.InternalValuePath, true, true);
                                }
                                writer.AddAttribute(HtmlTextWriterAttribute.Href, str7);
                            }
                        }
                        else
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.Href, this.GetToggleNodeAttributeValue(str, lineType));
                        }
                    }
                    else
                    {
                        string str8 = "javascript:0";
                        if (this._owner.Page != null)
                        {
                            str8 = this._owner.Page.ClientScript.GetPostBackClientHyperlink(this._owner, "t" + this.InternalValuePath, true);
                        }
                        writer.AddAttribute(HtmlTextWriterAttribute.Href, str8);
                    }
                    if (enabled)
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.A);
                    }
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, imageUrl);
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0");
                    if (format.Length > 0)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Alt, string.Format(CultureInfo.CurrentCulture, format, new object[] { this.Text }));
                    }
                    else
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Alt, string.Empty);
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag();
                    if (enabled)
                    {
                        writer.RenderEndTag();
                    }
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, imageUrl);
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, string.Empty);
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag();
                }
                writer.RenderEndTag();
            }
            ArrayList list = new ArrayList();
            if (this.NavigateUrl.Length > 0)
            {
                list.Add(HtmlTextWriterAttribute.Href);
                list.Add(this._owner.ResolveClientUrl(this.NavigateUrl));
                string target = this.ViewState["Target"] as string;
                if (target == null)
                {
                    target = this._owner.Target;
                }
                if (target.Length > 0)
                {
                    list.Add(HtmlTextWriterAttribute.Target);
                    list.Add(target);
                    if (this._owner.RenderClientScript)
                    {
                        string firstScript = string.Empty;
                        if ((((this._owner.Page != null) && this._owner.Page.SupportsStyleSheets) && (this.SelectAction == TreeNodeSelectAction.Select)) || (this.SelectAction == TreeNodeSelectAction.SelectExpand))
                        {
                            firstScript = Util.MergeScript(firstScript, "TreeView_SelectNode(" + this._owner.ClientDataObjectID + ", this,'" + this.SelectID + "');");
                        }
                        if ((this.SelectAction == TreeNodeSelectAction.Expand) || (this.SelectAction == TreeNodeSelectAction.SelectExpand))
                        {
                            if (this.PopulateOnDemand)
                            {
                                firstScript = Util.MergeScript(firstScript, this._owner.Page.ClientScript.GetPostBackClientHyperlink(this._owner, "t" + this.InternalValuePath, true, true));
                            }
                            else if (!this._owner.CustomExpandCollapseHandlerExists && flag3)
                            {
                                firstScript = Util.MergeScript(firstScript, this.GetToggleNodeAttributeValue(str, lineType));
                            }
                        }
                        if (firstScript.Length != 0)
                        {
                            list.Add("onclick");
                            list.Add(firstScript);
                        }
                    }
                }
            }
            else if ((this._owner.RenderClientScript && (this.SelectAction == TreeNodeSelectAction.Expand)) && !this._owner.CustomExpandCollapseHandlerExists)
            {
                if (this.PopulateOnDemand)
                {
                    if (this._owner.PopulateNodesFromClient)
                    {
                        list.Add(HtmlTextWriterAttribute.Href);
                        list.Add(this.GetPopulateNodeAttribute(writer, str, this.SelectID, selectImageId, lineType, depth, isLast));
                    }
                    else
                    {
                        list.Add(HtmlTextWriterAttribute.Href);
                        string str11 = "javascript:0";
                        if (this._owner.Page != null)
                        {
                            str11 = this._owner.Page.ClientScript.GetPostBackClientHyperlink(this._owner, "t" + this.InternalValuePath, true, true);
                        }
                        list.Add(str11);
                    }
                }
                else if (flag3)
                {
                    list.Add(HtmlTextWriterAttribute.Href);
                    list.Add(this.GetToggleNodeAttributeValue(str, lineType));
                }
            }
            else if (this.SelectAction != TreeNodeSelectAction.None)
            {
                list.Add(HtmlTextWriterAttribute.Href);
                if (this._owner.Page != null)
                {
                    string str12 = this._owner.Page.ClientScript.GetPostBackClientHyperlink(this._owner, "s" + this.InternalValuePath, true, true);
                    list.Add(str12);
                    if (this._owner.RenderClientScript)
                    {
                        list.Add("onclick");
                        list.Add("TreeView_SelectNode(" + this._owner.ClientDataObjectID + ", this,'" + this.SelectID + "');");
                    }
                }
                else
                {
                    list.Add("javascript:0");
                }
            }
            if (this.ToolTip.Length > 0)
            {
                list.Add(HtmlTextWriterAttribute.Title);
                list.Add(this.ToolTip);
            }
            if (levelImageUrl.Length > 0)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                this.ApplyAttributeList(writer, list);
                writer.AddAttribute(HtmlTextWriterAttribute.Id, selectImageId);
                if (enabled && (this.SelectAction != TreeNodeSelectAction.None))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Tabindex, "-1");
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Src, levelImageUrl);
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0");
                if (this.ImageToolTip.Length > 0)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, this.ImageToolTip);
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, string.Empty);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                if (enabled && (this.SelectAction != TreeNodeSelectAction.None))
                {
                    writer.RenderEndTag();
                }
                writer.RenderEndTag();
            }
            if (!this._owner.NodeWrap)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            }
            if ((this._owner.Page != null) && this._owner.Page.SupportsStyleSheets)
            {
                string cssClassName = this._owner.GetCssClassName(this, false);
                if (cssClassName.Trim().Length > 0)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClassName);
                }
            }
            else if (style != null)
            {
                style.AddAttributesToRender(writer);
            }
            if (this._owner.EnableHover && (this.SelectAction != TreeNodeSelectAction.None))
            {
                writer.AddAttribute("onmouseover", "TreeView_HoverNode(" + this._owner.ClientDataObjectID + ", this)");
                writer.AddAttribute("onmouseout", "TreeView_UnhoverNode(this)");
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            if (this.GetEffectiveShowCheckBox(treeNodeType))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
                string str14 = str + "CheckBox";
                writer.AddAttribute(HtmlTextWriterAttribute.Name, str14);
                writer.AddAttribute(HtmlTextWriterAttribute.Id, str14);
                if (this.Checked)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
                }
                if (!enabled)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
                    if ((!this._owner.Enabled && (this._owner.RenderingCompatibility >= VersionUtil.Framework40)) && !string.IsNullOrEmpty(WebControl.DisabledCssClass))
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, WebControl.DisabledCssClass);
                    }
                }
                if (this.ToolTip.Length > 0)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, this.ToolTip);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Input);
                writer.RenderEndTag();
            }
            this.RenderPreText(writer);
            if ((this._owner.Page != null) && this._owner.Page.SupportsStyleSheets)
            {
                bool flag4;
                string str15 = this._owner.GetCssClassName(this, true, out flag4);
                if (str15.Trim().Length > 0)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, str15);
                    if (flag4)
                    {
                        writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "none");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "1em");
                    }
                }
            }
            else if (style != null)
            {
                style.HyperLinkStyle.AddAttributesToRender(writer);
            }
            this.ApplyAttributeList(writer, list);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.SelectID);
            if ((this.SelectAction == TreeNodeSelectAction.None) || !enabled)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
                writer.Write(this.Text);
                writer.RenderEndTag();
            }
            else
            {
                if (!this._owner.AccessKeyRendered && (this._owner.AccessKey.Length != 0))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, this._owner.AccessKey, true);
                    this._owner.AccessKeyRendered = true;
                }
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write(this.Text);
                writer.RenderEndTag();
            }
            this.RenderPostText(writer);
            writer.RenderEndTag();
            writer.RenderEndTag();
            if ((style != null) && !style.NodeSpacing.IsEmpty)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, style.NodeSpacing.ToString(CultureInfo.InvariantCulture));
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
            writer.RenderEndTag();
            if (this.ChildNodes.Count > 0)
            {
                if (isLast.Length < (depth + 2))
                {
                    bool[] destinationArray = new bool[depth + 5];
                    Array.Copy(isLast, 0, destinationArray, 0, isLast.Length);
                    isLast = destinationArray;
                }
                if (this._owner.RenderClientScript)
                {
                    if (!flag2)
                    {
                        writer.AddStyleAttribute("display", "none");
                    }
                    else
                    {
                        writer.AddStyleAttribute("display", "block");
                    }
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, str + "Nodes");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    this.RenderChildNodes(writer, depth, isLast, enabled);
                    writer.RenderEndTag();
                }
                else if (flag2)
                {
                    this.RenderChildNodes(writer, depth, isLast, enabled);
                }
            }
        }

        internal void RenderChildNodes(HtmlTextWriter writer, int depth, bool[] isLast, bool enabled)
        {
            TreeNodeStyle style = this._owner.GetStyle(this);
            if (!style.ChildNodesPadding.IsEmpty)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Height, style.ChildNodesPadding.ToString(CultureInfo.InvariantCulture));
                writer.RenderBeginTag(HtmlTextWriterTag.Table);
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
            for (int i = 0; i < this.ChildNodes.Count; i++)
            {
                TreeNode node = this.ChildNodes[i];
                isLast[depth + 1] = i == (this.ChildNodes.Count - 1);
                node.Render(writer, i, isLast, enabled);
            }
            if (!isLast[depth] && !style.ChildNodesPadding.IsEmpty)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Height, style.ChildNodesPadding.ToString(CultureInfo.InvariantCulture));
                writer.RenderBeginTag(HtmlTextWriterTag.Table);
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
        }

        protected virtual void RenderPostText(HtmlTextWriter writer)
        {
        }

        protected virtual void RenderPreText(HtmlTextWriter writer)
        {
        }

        internal void ResetValuePathRecursive()
        {
            if (this._valuePath != null)
            {
                this._valuePath = null;
                foreach (TreeNode node in this.ChildNodes)
                {
                    node.ResetValuePathRecursive();
                }
            }
        }

        protected virtual object SaveViewState()
        {
            object[] objArray = new object[2];
            if (this._viewState != null)
            {
                objArray[0] = ((IStateManager) this._viewState).SaveViewState();
            }
            if (this._childNodes != null)
            {
                objArray[1] = ((IStateManager) this._childNodes).SaveViewState();
            }
            if ((objArray[0] == null) && (objArray[1] == null))
            {
                return null;
            }
            return objArray;
        }

        public void Select()
        {
            this.Selected = true;
        }

        internal void SetDataBound(bool dataBound)
        {
            this.ViewState["DataBound"] = dataBound;
        }

        internal void SetDataItem(object dataItem)
        {
            this._dataItem = dataItem;
        }

        internal void SetDataPath(string dataPath)
        {
            this.ViewState["DataPath"] = dataPath;
        }

        internal void SetDirty()
        {
            this.ViewState.SetDirty(true);
            if (this.ChildNodes.Count > 0)
            {
                this.ChildNodes.SetDirty();
            }
        }

        private void SetExpandedRecursive(bool value)
        {
            this.Expanded = new bool?(value);
            if (this.ChildNodes.Count > 0)
            {
                for (int i = 0; i < this.ChildNodes.Count; i++)
                {
                    this.ChildNodes[i].SetExpandedRecursive(value);
                }
            }
        }

        internal void SetOwner(TreeView owner)
        {
            this._owner = owner;
            if (this._selectDesired == 1)
            {
                this._selectDesired = 0;
                this.Selected = true;
            }
            else if (this._selectDesired == -1)
            {
                this._selectDesired = 0;
                this.Selected = false;
            }
            if (this._populateDesired)
            {
                this._populateDesired = false;
                this.Populate();
            }
            if (this._modifyCheckedNodes && (this._owner != null))
            {
                this._modifyCheckedNodes = false;
                if (this.Checked)
                {
                    if (!this._owner.CheckedNodes.Contains(this))
                    {
                        this._owner.CheckedNodes.Add(this);
                    }
                }
                else
                {
                    this._owner.CheckedNodes.Remove(this);
                }
            }
            foreach (TreeNode node in this.ChildNodes)
            {
                node.SetOwner(this._owner);
            }
        }

        internal void SetParent(TreeNode parent)
        {
            this._parent = parent;
            this.SetPath(null);
        }

        internal void SetPath(string newPath)
        {
            this._internalValuePath = newPath;
            this._depth = -2;
        }

        internal void SetSelected(bool value)
        {
            this.ViewState["Selected"] = value;
            if (this._owner == null)
            {
                this._selectDesired = value ? 1 : -1;
            }
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        void IStateManager.LoadViewState(object state)
        {
            this.LoadViewState(state);
        }

        object IStateManager.SaveViewState()
        {
            return this.SaveViewState();
        }

        void IStateManager.TrackViewState()
        {
            this.TrackViewState();
        }

        public void ToggleExpandState()
        {
            this.Expanded = new bool?(this.Expanded != true);
        }

        protected void TrackViewState()
        {
            this._isTrackingViewState = true;
            if (this._viewState != null)
            {
                ((IStateManager) this._viewState).TrackViewState();
            }
            if (this._childNodes != null)
            {
                ((IStateManager) this._childNodes).TrackViewState();
            }
        }

        [DefaultValue(false), WebSysDescription("TreeNode_Checked")]
        public bool Checked
        {
            get
            {
                object obj2 = this.ViewState["Checked"];
                if (obj2 == null)
                {
                    return false;
                }
                return (bool) obj2;
            }
            set
            {
                this.ViewState["Checked"] = value;
                this.NotifyOwnerChecked();
            }
        }

        internal bool CheckedSet
        {
            get
            {
                return (this.ViewState["Checked"] != null);
            }
        }

        [PersistenceMode(PersistenceMode.InnerDefaultProperty), DefaultValue((string) null), MergableProperty(false), Browsable(false)]
        public TreeNodeCollection ChildNodes
        {
            get
            {
                if (this._childNodes == null)
                {
                    this._childNodes = new TreeNodeCollection(this);
                }
                return this._childNodes;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(false)]
        public bool DataBound
        {
            get
            {
                object obj2 = this.ViewState["DataBound"];
                if (obj2 == null)
                {
                    return false;
                }
                return (bool) obj2;
            }
        }

        [Browsable(false), DefaultValue((string) null)]
        public object DataItem
        {
            get
            {
                return this._dataItem;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), DefaultValue("")]
        public string DataPath
        {
            get
            {
                string str = (string) this.ViewState["DataPath"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Depth
        {
            get
            {
                if (this._depth == -2)
                {
                    if (this._isRoot)
                    {
                        return -1;
                    }
                    if (this.Parent == null)
                    {
                        if (this._owner == null)
                        {
                            return 0;
                        }
                        this._depth = this.InternalValuePath.Split(new char[] { '\\' }).Length - 1;
                    }
                    else
                    {
                        this._depth = this.Parent.Depth + 1;
                    }
                }
                return this._depth;
            }
        }

        [DefaultValue(typeof(bool?), ""), WebSysDescription("TreeNode_Expanded")]
        public bool? Expanded
        {
            get
            {
                object obj2 = this.ViewState["Expanded"];
                if (obj2 == null)
                {
                    return null;
                }
                return (bool?) obj2;
            }
            set
            {
                bool? expanded = this.Expanded;
                this.ViewState["Expanded"] = value;
                if ((value != expanded) && ((this._owner == null) || !this._owner.DesignMode))
                {
                    if (value == true)
                    {
                        if (this.PopulateOnDemand)
                        {
                            if (this._owner == null)
                            {
                                this._populateDesired = true;
                            }
                            else if (!this._owner.LoadingNodeState)
                            {
                                this.Populate();
                            }
                        }
                        if (this._owner != null)
                        {
                            this._owner.RaiseTreeNodeExpanded(this);
                        }
                    }
                    else if ((value == false) && (((expanded == true) && (this.ChildNodes.Count > 0)) && (this._owner != null)))
                    {
                        this._owner.RaiseTreeNodeCollapsed(this);
                    }
                }
            }
        }

        [Localizable(true), WebSysDescription("TreeNode_ImageToolTip"), DefaultValue("")]
        public string ImageToolTip
        {
            get
            {
                string str = (string) this.ViewState["ImageToolTip"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["ImageToolTip"] = value;
            }
        }

        [WebSysDescription("TreeNode_ImageUrl"), UrlProperty, DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ImageUrl
        {
            get
            {
                string str = (string) this.ViewState["ImageUrl"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["ImageUrl"] = value;
            }
        }

        internal int Index
        {
            get
            {
                return this._index;
            }
            set
            {
                this._index = value;
            }
        }

        internal string InternalValuePath
        {
            get
            {
                if (this._internalValuePath == null)
                {
                    if (this._parent == null)
                    {
                        return string.Empty;
                    }
                    List<string> list = new List<string> {
                        TreeView.Escape(this.Value)
                    };
                    for (TreeNode node = this._parent; (node != null) && !node._isRoot; node = node._parent)
                    {
                        if (node._internalValuePath != null)
                        {
                            list.Add(node._internalValuePath);
                            break;
                        }
                        list.Add(TreeView.Escape(node.Value));
                    }
                    list.Reverse();
                    this._internalValuePath = string.Join('\\'.ToString(), list.ToArray());
                }
                return this._internalValuePath;
            }
        }

        protected bool IsTrackingViewState
        {
            get
            {
                return this._isTrackingViewState;
            }
        }

        [WebSysDescription("TreeNode_NavigateUrl"), UrlProperty, DefaultValue(""), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string NavigateUrl
        {
            get
            {
                string str = (string) this.ViewState["NavigateUrl"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["NavigateUrl"] = value;
            }
        }

        internal TreeView Owner
        {
            get
            {
                return this._owner;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TreeNode Parent
        {
            get
            {
                if ((this._parent != null) && !this._parent._isRoot)
                {
                    return this._parent;
                }
                return null;
            }
        }

        internal bool Populated
        {
            get
            {
                object obj2 = this.ViewState["Populated"];
                if (obj2 == null)
                {
                    return false;
                }
                return (bool) obj2;
            }
            set
            {
                this.ViewState["Populated"] = value;
            }
        }

        [WebSysDescription("TreeNode_PopulateOnDemand"), DefaultValue(false)]
        public bool PopulateOnDemand
        {
            get
            {
                object obj2 = this.ViewState["PopulateOnDemand"];
                if (obj2 == null)
                {
                    return false;
                }
                return (bool) obj2;
            }
            set
            {
                this.ViewState["PopulateOnDemand"] = value;
                if (value && (this.Expanded == true))
                {
                    this.Expanded = null;
                }
            }
        }

        [DefaultValue(false)]
        internal bool PreserveChecked
        {
            get
            {
                object obj2 = this.ViewState["PreserveChecked"];
                if (obj2 == null)
                {
                    return false;
                }
                return (bool) obj2;
            }
            set
            {
                this.ViewState["PreserveChecked"] = value;
            }
        }

        [DefaultValue(0), WebSysDescription("TreeNode_SelectAction")]
        public TreeNodeSelectAction SelectAction
        {
            get
            {
                object obj2 = this.ViewState["SelectAction"];
                if (obj2 == null)
                {
                    return TreeNodeSelectAction.Select;
                }
                return (TreeNodeSelectAction) obj2;
            }
            set
            {
                this.ViewState["SelectAction"] = value;
            }
        }

        [DefaultValue(false), WebSysDescription("TreeNode_Selected")]
        public bool Selected
        {
            get
            {
                object obj2 = this.ViewState["Selected"];
                if (obj2 == null)
                {
                    return false;
                }
                return (bool) obj2;
            }
            set
            {
                this.SetSelected(value);
                if (this._owner == null)
                {
                    this._selectDesired = value ? 1 : -1;
                }
                else if (value)
                {
                    this._owner.SetSelectedNode(this);
                }
                else if (this == this._owner.SelectedNode)
                {
                    this._owner.SetSelectedNode(null);
                }
            }
        }

        internal string SelectID
        {
            get
            {
                if (this._owner.ShowExpandCollapse)
                {
                    return this._owner.CreateNodeTextId(this.Index);
                }
                return this._owner.CreateNodeId(this.Index);
            }
        }

        [WebSysDescription("TreeNode_ShowCheckBox"), DefaultValue(typeof(bool?), "")]
        public bool? ShowCheckBox
        {
            get
            {
                object obj2 = this.ViewState["ShowCheckBox"];
                if (obj2 == null)
                {
                    return null;
                }
                return (bool?) obj2;
            }
            set
            {
                this.ViewState["ShowCheckBox"] = value;
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return this.IsTrackingViewState;
            }
        }

        [DefaultValue(""), WebSysDescription("TreeNode_Target")]
        public string Target
        {
            get
            {
                string str = (string) this.ViewState["Target"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["Target"] = value;
            }
        }

        [WebSysDescription("TreeNode_Text"), Localizable(true), DefaultValue("")]
        public string Text
        {
            get
            {
                string str = (string) this.ViewState["Text"];
                if (str == null)
                {
                    str = (string) this.ViewState["Value"];
                    if (str == null)
                    {
                        return string.Empty;
                    }
                }
                return str;
            }
            set
            {
                this.ViewState["Text"] = value;
            }
        }

        [WebSysDescription("TreeNode_ToolTip"), DefaultValue(""), Localizable(true)]
        public string ToolTip
        {
            get
            {
                string str = (string) this.ViewState["ToolTip"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["ToolTip"] = value;
            }
        }

        [Localizable(true), WebSysDescription("TreeNode_Value"), DefaultValue("")]
        public string Value
        {
            get
            {
                string str = (string) this.ViewState["Value"];
                if (str == null)
                {
                    str = (string) this.ViewState["Text"];
                    if (str == null)
                    {
                        return string.Empty;
                    }
                }
                return str;
            }
            set
            {
                this.ViewState["Value"] = value;
                this.ResetValuePathRecursive();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public string ValuePath
        {
            get
            {
                if (this._valuePath == null)
                {
                    if (this._parent != null)
                    {
                        string valuePath = this._parent.ValuePath;
                        this._valuePath = ((valuePath.Length == 0) && (this._parent.Depth == -1)) ? this.Value : (valuePath + this._owner.PathSeparator + this.Value);
                        return this._valuePath;
                    }
                    if ((this.Owner == null) || string.IsNullOrEmpty(this.InternalValuePath))
                    {
                        return string.Empty;
                    }
                    string[] strArray = this.InternalValuePath.Split(new char[] { '\\' });
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        strArray[i] = TreeView.UnEscape(strArray[i]);
                    }
                    this._valuePath = string.Join(this.Owner.PathSeparator.ToString(), strArray);
                }
                return this._valuePath;
            }
        }

        private StateBag ViewState
        {
            get
            {
                if (this._viewState == null)
                {
                    this._viewState = new StateBag();
                    if (this._isTrackingViewState)
                    {
                        ((IStateManager) this._viewState).TrackViewState();
                    }
                }
                return this._viewState;
            }
        }
    }
}

