namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    public class TemplateField : DataControlField
    {
        private ITemplate alternatingItemTemplate;
        private ITemplate editItemTemplate;
        private ITemplate footerTemplate;
        private ITemplate headerTemplate;
        private ITemplate insertItemTemplate;
        private ITemplate itemTemplate;

        protected override void CopyProperties(DataControlField newField)
        {
            ((TemplateField) newField).ConvertEmptyStringToNull = this.ConvertEmptyStringToNull;
            ((TemplateField) newField).AlternatingItemTemplate = this.AlternatingItemTemplate;
            ((TemplateField) newField).ItemTemplate = this.ItemTemplate;
            ((TemplateField) newField).FooterTemplate = this.FooterTemplate;
            ((TemplateField) newField).EditItemTemplate = this.EditItemTemplate;
            ((TemplateField) newField).HeaderTemplate = this.HeaderTemplate;
            ((TemplateField) newField).InsertItemTemplate = this.InsertItemTemplate;
            base.CopyProperties(newField);
        }

        protected override DataControlField CreateField()
        {
            return new TemplateField();
        }

        public override void ExtractValuesFromCell(IOrderedDictionary dictionary, DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
        {
            DataBoundControlHelper.ExtractValuesFromBindableControls(dictionary, cell);
            IBindableTemplate itemTemplate = this.ItemTemplate as IBindableTemplate;
            if (((rowState & DataControlRowState.Alternate) != DataControlRowState.Normal) && (this.AlternatingItemTemplate != null))
            {
                itemTemplate = this.AlternatingItemTemplate as IBindableTemplate;
            }
            if (((rowState & DataControlRowState.Edit) != DataControlRowState.Normal) && (this.EditItemTemplate != null))
            {
                itemTemplate = this.EditItemTemplate as IBindableTemplate;
            }
            else if (((rowState & DataControlRowState.Insert) != DataControlRowState.Normal) && this.InsertVisible)
            {
                if (this.InsertItemTemplate != null)
                {
                    itemTemplate = this.InsertItemTemplate as IBindableTemplate;
                }
                else if (this.EditItemTemplate != null)
                {
                    itemTemplate = this.EditItemTemplate as IBindableTemplate;
                }
            }
            if (itemTemplate != null)
            {
                bool convertEmptyStringToNull = this.ConvertEmptyStringToNull;
                foreach (DictionaryEntry entry in itemTemplate.ExtractValues(cell.BindingContainer))
                {
                    object obj2 = entry.Value;
                    if ((convertEmptyStringToNull && (obj2 is string)) && (((string) obj2).Length == 0))
                    {
                        dictionary[entry.Key] = null;
                    }
                    else
                    {
                        dictionary[entry.Key] = obj2;
                    }
                }
            }
        }

        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
        {
            base.InitializeCell(cell, cellType, rowState, rowIndex);
            ITemplate headerTemplate = null;
            switch (cellType)
            {
                case DataControlCellType.Header:
                    headerTemplate = this.headerTemplate;
                    break;

                case DataControlCellType.Footer:
                    headerTemplate = this.footerTemplate;
                    break;

                case DataControlCellType.DataCell:
                    headerTemplate = this.itemTemplate;
                    if ((rowState & DataControlRowState.Edit) == DataControlRowState.Normal)
                    {
                        if ((rowState & DataControlRowState.Insert) != DataControlRowState.Normal)
                        {
                            if (this.insertItemTemplate != null)
                            {
                                headerTemplate = this.insertItemTemplate;
                            }
                            else if (this.editItemTemplate != null)
                            {
                                headerTemplate = this.editItemTemplate;
                            }
                        }
                        else if (((rowState & DataControlRowState.Alternate) != DataControlRowState.Normal) && (this.alternatingItemTemplate != null))
                        {
                            headerTemplate = this.alternatingItemTemplate;
                        }
                        break;
                    }
                    if (this.editItemTemplate != null)
                    {
                        headerTemplate = this.editItemTemplate;
                    }
                    break;
            }
            if (headerTemplate != null)
            {
                cell.Text = string.Empty;
                headerTemplate.InstantiateIn(cell);
            }
            else if (cellType == DataControlCellType.DataCell)
            {
                cell.Text = "&nbsp;";
            }
        }

        public override void ValidateSupportsCallback()
        {
            throw new NotSupportedException(System.Web.SR.GetString("TemplateField_CallbacksNotSupported", new object[] { base.Control.ID }));
        }

        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(IDataItemContainer), BindingDirection.TwoWay), DefaultValue((string) null), WebSysDescription("TemplateField_AlternatingItemTemplate"), Browsable(false)]
        public virtual ITemplate AlternatingItemTemplate
        {
            get
            {
                return this.alternatingItemTemplate;
            }
            set
            {
                this.alternatingItemTemplate = value;
                this.OnFieldChanged();
            }
        }

        [WebSysDescription("ImageField_ConvertEmptyStringToNull"), WebCategory("Behavior"), DefaultValue(true)]
        public virtual bool ConvertEmptyStringToNull
        {
            get
            {
                object obj2 = base.ViewState["ConvertEmptyStringToNull"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                base.ViewState["ConvertEmptyStringToNull"] = value;
            }
        }

        [DefaultValue((string) null), TemplateContainer(typeof(IDataItemContainer), BindingDirection.TwoWay), Browsable(false), WebSysDescription("TemplateField_EditItemTemplate"), PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual ITemplate EditItemTemplate
        {
            get
            {
                return this.editItemTemplate;
            }
            set
            {
                this.editItemTemplate = value;
                this.OnFieldChanged();
            }
        }

        [DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(IDataItemContainer)), Browsable(false), WebSysDescription("TemplateField_FooterTemplate")]
        public virtual ITemplate FooterTemplate
        {
            get
            {
                return this.footerTemplate;
            }
            set
            {
                this.footerTemplate = value;
                this.OnFieldChanged();
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(IDataItemContainer)), DefaultValue((string) null), WebSysDescription("TemplateField_HeaderTemplate"), Browsable(false)]
        public virtual ITemplate HeaderTemplate
        {
            get
            {
                return this.headerTemplate;
            }
            set
            {
                this.headerTemplate = value;
                this.OnFieldChanged();
            }
        }

        [TemplateContainer(typeof(IDataItemContainer), BindingDirection.TwoWay), DefaultValue((string) null), WebSysDescription("TemplateField_InsertItemTemplate"), PersistenceMode(PersistenceMode.InnerProperty), Browsable(false)]
        public virtual ITemplate InsertItemTemplate
        {
            get
            {
                return this.insertItemTemplate;
            }
            set
            {
                this.insertItemTemplate = value;
                this.OnFieldChanged();
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(IDataItemContainer), BindingDirection.TwoWay), DefaultValue((string) null), WebSysDescription("TemplateField_ItemTemplate"), Browsable(false)]
        public virtual ITemplate ItemTemplate
        {
            get
            {
                return this.itemTemplate;
            }
            set
            {
                this.itemTemplate = value;
                this.OnFieldChanged();
            }
        }
    }
}

