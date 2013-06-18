namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    [ControlValueProperty("FileBytes"), ValidationProperty("FileName"), Designer("System.Web.UI.Design.WebControls.PreviewControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class FileUpload : WebControl
    {
        public FileUpload() : base(HtmlTextWriterTag.Input)
        {
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "file");
            string uniqueID = this.UniqueID;
            if (uniqueID != null)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Name, uniqueID);
            }
            base.AddAttributesToRender(writer);
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            HtmlForm form = this.Page.Form;
            if ((form != null) && (form.Enctype.Length == 0))
            {
                form.Enctype = "multipart/form-data";
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            base.Render(writer);
        }

        public void SaveAs(string filename)
        {
            HttpPostedFile postedFile = this.PostedFile;
            if (postedFile != null)
            {
                postedFile.SaveAs(filename);
            }
        }

        [Bindable(true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public byte[] FileBytes
        {
            get
            {
                Stream fileContent = this.FileContent;
                if ((fileContent == null) || (fileContent == Stream.Null))
                {
                    return new byte[0];
                }
                long length = fileContent.Length;
                BinaryReader reader = new BinaryReader(fileContent);
                byte[] buffer = null;
                if (length > 0x7fffffffL)
                {
                    throw new HttpException(System.Web.SR.GetString("FileUpload_StreamTooLong"));
                }
                if (!fileContent.CanSeek)
                {
                    throw new HttpException(System.Web.SR.GetString("FileUpload_StreamNotSeekable"));
                }
                int position = (int) fileContent.Position;
                int count = (int) length;
                try
                {
                    fileContent.Seek(0L, SeekOrigin.Begin);
                    buffer = reader.ReadBytes(count);
                }
                finally
                {
                    fileContent.Seek((long) position, SeekOrigin.Begin);
                }
                if (buffer.Length != count)
                {
                    throw new HttpException(System.Web.SR.GetString("FileUpload_StreamLengthNotReached"));
                }
                return buffer;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Stream FileContent
        {
            get
            {
                if (this.PostedFile != null)
                {
                    return this.PostedFile.InputStream;
                }
                return Stream.Null;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public string FileName
        {
            get
            {
                HttpPostedFile postedFile = this.PostedFile;
                string str = string.Empty;
                if (postedFile == null)
                {
                    return str;
                }
                string fileName = postedFile.FileName;
                try
                {
                    return Path.GetFileName(fileName);
                }
                catch
                {
                    return fileName;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HasFile
        {
            get
            {
                HttpPostedFile postedFile = this.PostedFile;
                return ((postedFile != null) && (postedFile.ContentLength > 0));
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HttpPostedFile PostedFile
        {
            get
            {
                if ((this.Page != null) && this.Page.IsPostBack)
                {
                    return this.Context.Request.Files[this.UniqueID];
                }
                return null;
            }
        }
    }
}

