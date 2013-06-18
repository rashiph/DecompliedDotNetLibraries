namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Reflection;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class TreeViewImageGenerator : DesignerForm
    {
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Label _folderNameLabel;
        private System.Windows.Forms.TextBox _folderNameTextBox;
        private LineImageInfo _imageInfo;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.TextBox _previewFrameTextBox;
        private System.Windows.Forms.Label _previewLabel;
        private System.Windows.Forms.Panel _previewPanel;
        private PictureBox _previewPictureBox;
        private ProgressBar _progressBar;
        private System.Windows.Forms.Label _progressBarLabel;
        private System.Windows.Forms.Label _propertiesLabel;
        private PropertyGrid _propertyGrid;
        private System.Web.UI.WebControls.TreeView _treeView;
        private static System.Drawing.Image defaultMinusImage;
        private static System.Drawing.Image defaultPlusImage;

        public TreeViewImageGenerator(System.Web.UI.WebControls.TreeView treeView) : base(treeView.Site)
        {
            this._previewPictureBox = new PictureBox();
            this._previewLabel = new System.Windows.Forms.Label();
            this._previewPanel = new System.Windows.Forms.Panel();
            this._previewFrameTextBox = new System.Windows.Forms.TextBox();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._folderNameLabel = new System.Windows.Forms.Label();
            this._folderNameTextBox = new System.Windows.Forms.TextBox();
            this._propertiesLabel = new System.Windows.Forms.Label();
            this._propertyGrid = new VsPropertyGrid(base.ServiceProvider);
            this._progressBar = new ProgressBar();
            this._progressBarLabel = new System.Windows.Forms.Label();
            this._previewPanel.SuspendLayout();
            base.SuspendLayout();
            this._previewPictureBox.Name = "_previewPictureBox";
            this._previewPictureBox.SizeMode = PictureBoxSizeMode.Normal;
            this._previewPictureBox.TabIndex = 10;
            this._previewPictureBox.TabStop = false;
            this._previewPictureBox.BackColor = Color.White;
            this._previewLabel.Location = new Point(12, 12);
            this._previewLabel.Name = "_previewLabel";
            this._previewLabel.Size = new Size(180, 14);
            this._previewLabel.TabIndex = 9;
            this._previewLabel.Text = System.Design.SR.GetString("TreeViewImageGenerator_Preview");
            this._previewPanel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._previewPanel.AutoScroll = true;
            this._previewPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._previewPanel.Controls.AddRange(new Control[] { this._previewPictureBox });
            this._previewPanel.Location = new Point(13, 0x1d);
            this._previewPanel.Name = "_previewPanel";
            this._previewPanel.Size = new Size(0xb2, 0xf2);
            this._previewPanel.TabIndex = 11;
            this._previewFrameTextBox.Multiline = true;
            this._previewFrameTextBox.Enabled = false;
            this._previewFrameTextBox.TabStop = false;
            this._previewFrameTextBox.Location = new Point(12, 0x1c);
            this._previewFrameTextBox.Size = new Size(180, 0xf4);
            this._okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._okButton.FlatStyle = FlatStyle.System;
            this._okButton.Location = new Point(0x178, 0x144);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new Size(0x4b, 0x17);
            this._okButton.TabIndex = 20;
            this._okButton.Text = System.Design.SR.GetString("OKCaption");
            this._okButton.Click += new EventHandler(this.OnOKButtonClick);
            this._cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._cancelButton.FlatStyle = FlatStyle.System;
            this._cancelButton.Location = new Point(0x1c8, 0x144);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new Size(0x4b, 0x17);
            this._cancelButton.TabIndex = 0x15;
            this._cancelButton.Text = System.Design.SR.GetString("CancelCaption");
            this._cancelButton.Click += new EventHandler(this.OnCancelButtonClick);
            this._folderNameLabel.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this._folderNameLabel.Location = new Point(0xd5, 0x117);
            this._folderNameLabel.Name = "_folderNameLabel";
            this._folderNameLabel.Size = new Size(0x13b, 14);
            this._folderNameLabel.TabIndex = 0x11;
            this._folderNameLabel.Text = System.Design.SR.GetString("TreeViewImageGenerator_FolderName");
            this._folderNameTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this._folderNameTextBox.Location = new Point(0xd5, 0x127);
            this._folderNameTextBox.Name = "_folderNameTextBox";
            this._folderNameTextBox.Size = new Size(0x13b, 20);
            this._folderNameTextBox.TabIndex = 0x12;
            this._folderNameTextBox.Text = System.Design.SR.GetString("TreeViewImageGenerator_DefaultFolderName");
            this._folderNameTextBox.WordWrap = false;
            this._folderNameTextBox.TextChanged += new EventHandler(this.OnFolderNameTextBoxTextChanged);
            this._progressBarLabel.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this._progressBarLabel.Location = new Point(12, 0x117);
            this._progressBarLabel.Name = "_progressBarLabel";
            this._progressBarLabel.Size = new Size(180, 14);
            this._progressBarLabel.Text = System.Design.SR.GetString("TreeViewImageGenerator_ProgressBarName");
            this._progressBarLabel.Visible = false;
            this._progressBar.Location = new Point(12, 0x127);
            this._progressBar.Size = new Size(180, 0x10);
            this._progressBar.Maximum = 0x10;
            this._progressBar.Minimum = 0;
            this._progressBar.Visible = false;
            this._propertiesLabel.Location = new Point(0xd5, 12);
            this._propertiesLabel.Name = "_propertiesLabel";
            this._propertiesLabel.Size = new Size(0x13b, 14);
            this._propertiesLabel.TabIndex = 12;
            this._propertiesLabel.Text = System.Design.SR.GetString("TreeViewImageGenerator_Properties");
            this._propertyGrid.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            this._propertyGrid.CommandsVisibleIfAvailable = true;
            this._propertyGrid.LargeButtons = false;
            this._propertyGrid.LineColor = SystemColors.ScrollBar;
            this._propertyGrid.Location = new Point(0xd5, 0x1c);
            this._propertyGrid.Name = "_propertyGrid";
            this._propertyGrid.PropertySort = PropertySort.Alphabetical;
            this._propertyGrid.Size = new Size(0x13b, 0xf4);
            this._propertyGrid.TabIndex = 13;
            this._propertyGrid.ToolbarVisible = true;
            this._propertyGrid.ViewBackColor = SystemColors.Window;
            this._propertyGrid.ViewForeColor = SystemColors.WindowText;
            this._propertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(this.OnPropertyGridPropertyValueChanged);
            base.AcceptButton = this._okButton;
            base.CancelButton = this._cancelButton;
            base.ClientSize = new Size(540, 0x167);
            base.Controls.AddRange(new Control[] { this._propertyGrid, this._propertiesLabel, this._progressBar, this._progressBarLabel, this._folderNameTextBox, this._folderNameLabel, this._cancelButton, this._okButton, this._previewPanel, this._previewLabel, this._previewFrameTextBox });
            this.MinimumSize = new Size(540, 0x167);
            base.Name = "TreeLineImageGenerator";
            this.Text = System.Design.SR.GetString("TreeViewImageGenerator_Title");
            base.Resize += new EventHandler(this.OnFormResize);
            this._previewPanel.ResumeLayout(false);
            base.InitializeForm();
            base.ResumeLayout(false);
            this._imageInfo = new LineImageInfo();
            this._propertyGrid.SelectedObject = this._imageInfo;
            this._treeView = treeView;
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Icon = null;
            this.UpdatePreview();
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            base.Close();
        }

        private void OnFolderNameTextBoxTextChanged(object sender, EventArgs e)
        {
            if (this._folderNameTextBox.Text.Trim().Length > 0)
            {
                this._okButton.Enabled = true;
            }
            else
            {
                this._okButton.Enabled = false;
            }
        }

        private void OnFormResize(object sender, EventArgs e)
        {
            this.UpdatePreview();
        }

        private void OnOKButtonClick(object sender, EventArgs e)
        {
            System.Drawing.Image defaultPlusImage;
            string str = this._folderNameTextBox.Text.Trim();
            if (str.Length == 0)
            {
                UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("TreeViewImageGenerator_MissingFolderName"));
                return;
            }
            if (str.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("TreeViewImageGenerator_InvalidFolderName", new object[] { str }));
                return;
            }
            IWebApplication service = (IWebApplication) this._treeView.Site.GetService(typeof(IWebApplication));
            if (service == null)
            {
                UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("TreeViewImageGenerator_ErrorWriting"));
                return;
            }
            IFolderProjectItem rootProjectItem = (IFolderProjectItem) service.RootProjectItem;
            IProjectItem projectItemFromUrl = service.GetProjectItemFromUrl(Path.Combine("~/", str));
            if ((projectItemFromUrl != null) && !(projectItemFromUrl is IFolderProjectItem))
            {
                UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("TreeViewImageGenerator_DocumentExists", new object[] { str }));
                return;
            }
            IFolderProjectItem folder = (IFolderProjectItem) projectItemFromUrl;
            if (folder == null)
            {
                if (UIServiceHelper.ShowMessage(base.ServiceProvider, System.Design.SR.GetString("TreeViewImageGenerator_NonExistentFolderName", new object[] { str }), System.Design.SR.GetString("TreeViewImageGenerator_Title"), MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        folder = rootProjectItem.AddFolder(str);
                        goto Label_015A;
                    }
                    catch
                    {
                        UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("TreeViewImageGenerator_ErrorCreatingFolder", new object[] { str }));
                    }
                }
                return;
            }
        Label_015A:
            defaultPlusImage = this._imageInfo.ExpandImage;
            if (defaultPlusImage == null)
            {
                defaultPlusImage = this.DefaultPlusImage;
            }
            System.Drawing.Image collapseImage = this._imageInfo.CollapseImage;
            if (collapseImage == null)
            {
                collapseImage = this.DefaultMinusImage;
            }
            System.Drawing.Image noExpandImage = this._imageInfo.NoExpandImage;
            int width = this._imageInfo.Width;
            if (width < 1)
            {
                UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("TreeViewImageGenerator_InvalidValue", new object[] { "Width" }));
            }
            else
            {
                int height = this._imageInfo.Height;
                if (height < 1)
                {
                    UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("TreeViewImageGenerator_InvalidValue", new object[] { "Height" }));
                }
                else
                {
                    int lineWidth = this._imageInfo.LineWidth;
                    if (lineWidth < 1)
                    {
                        UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("TreeViewImageGenerator_InvalidValue", new object[] { "LineWidth" }));
                    }
                    else
                    {
                        int lineStyle = (int) this._imageInfo.LineStyle;
                        Color lineColor = this._imageInfo.LineColor;
                        this._progressBar.Value = 0;
                        this._progressBar.Visible = true;
                        this._progressBarLabel.Visible = true;
                        try
                        {
                            bool overwrite = false;
                            bool flag2 = false;
                            Bitmap image = new Bitmap(width, height);
                            Graphics g = Graphics.FromImage(image);
                            g.FillRectangle(new SolidBrush(this._imageInfo.TransparentColor), 0, 0, width, height);
                            this.RenderImage(g, 0, 0, width, height, 'i', lineStyle, lineWidth, lineColor, null);
                            string name = "i.gif";
                            flag2 |= this.SaveTransparentGif(image, folder, "i.gif", ref overwrite);
                            this._progressBar.Value++;
                            string str3 = "-rtl ";
                            for (int i = 0; i < str3.Length; i++)
                            {
                                image = new Bitmap(width, height);
                                g = Graphics.FromImage(image);
                                g.FillRectangle(new SolidBrush(this._imageInfo.TransparentColor), 0, 0, width, height);
                                this.RenderImage(g, 0, 0, width, height, str3[i], lineStyle, lineWidth, lineColor, collapseImage);
                                g.Dispose();
                                name = "minus.gif";
                                if (str3[i] == '-')
                                {
                                    name = "dash" + name;
                                }
                                else if (str3[i] != ' ')
                                {
                                    name = str3[i] + name;
                                }
                                flag2 |= this.SaveTransparentGif(image, folder, name, ref overwrite);
                                this._progressBar.Value++;
                            }
                            for (int j = 0; j < str3.Length; j++)
                            {
                                image = new Bitmap(width, height);
                                g = Graphics.FromImage(image);
                                g.FillRectangle(new SolidBrush(this._imageInfo.TransparentColor), 0, 0, width, height);
                                this.RenderImage(g, 0, 0, width, height, str3[j], lineStyle, lineWidth, lineColor, defaultPlusImage);
                                g.Dispose();
                                name = "plus.gif";
                                if (str3[j] == '-')
                                {
                                    name = "dash" + name;
                                }
                                else if (str3[j] != ' ')
                                {
                                    name = str3[j] + name;
                                }
                                flag2 |= this.SaveTransparentGif(image, folder, name, ref overwrite);
                                this._progressBar.Value++;
                            }
                            for (int k = 0; k < str3.Length; k++)
                            {
                                image = new Bitmap(width, height);
                                g = Graphics.FromImage(image);
                                g.FillRectangle(new SolidBrush(this._imageInfo.TransparentColor), 0, 0, width, height);
                                this.RenderImage(g, 0, 0, width, height, str3[k], lineStyle, lineWidth, lineColor, noExpandImage);
                                g.Dispose();
                                name = ".gif";
                                if (str3[k] == '-')
                                {
                                    name = "dash" + name;
                                }
                                else if (str3[k] == ' ')
                                {
                                    name = "noexpand" + name;
                                }
                                else
                                {
                                    name = str3[k] + name;
                                }
                                flag2 |= this.SaveTransparentGif(image, folder, name, ref overwrite);
                                this._progressBar.Value++;
                            }
                            this._progressBar.Visible = false;
                            this._progressBarLabel.Visible = false;
                            if (flag2)
                            {
                                UIServiceHelper.ShowMessage(base.ServiceProvider, System.Design.SR.GetString("TreeViewImageGenerator_LineImagesGenerated", new object[] { str }));
                            }
                        }
                        catch
                        {
                            this._progressBar.Visible = false;
                            this._progressBarLabel.Visible = false;
                            UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("TreeViewImageGenerator_ErrorWriting", new object[] { str }));
                            return;
                        }
                        this._treeView.LineImagesFolder = "~/" + str;
                        base.DialogResult = DialogResult.OK;
                        base.Close();
                    }
                }
            }
        }

        private void OnPropertyGridPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            this.UpdatePreview();
        }

        private static unsafe System.Drawing.Image ReduceColors(Bitmap bitmap, int maxColors, int numBits, Color transparentColor)
        {
            byte* numPtr;
            if ((numBits < 3) || (numBits > 8))
            {
                throw new ArgumentOutOfRangeException("numBits");
            }
            if (maxColors < 0x10)
            {
                throw new ArgumentOutOfRangeException("maxColors");
            }
            int width = bitmap.Width;
            int height = bitmap.Height;
            Octree octree = new Octree(maxColors, numBits, transparentColor);
            for (int i = 0; i < width; i++)
            {
                for (int k = 0; k < height; k++)
                {
                    octree.AddColor(bitmap.GetPixel(i, k));
                }
            }
            ColorIndexTable colorIndexTable = octree.GetColorIndexTable();
            Bitmap bitmap2 = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            ColorPalette palette = bitmap2.Palette;
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bitmapdata = bitmap2.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            IntPtr ptr = bitmapdata.Scan0;
            if (bitmapdata.Stride > 0)
            {
                numPtr = (byte*) ptr.ToPointer();
            }
            else
            {
                numPtr = (byte*) (ptr.ToPointer() + (bitmapdata.Stride * (height - 1)));
            }
            int num5 = Math.Abs(bitmapdata.Stride);
            for (int j = 0; j < height; j++)
            {
                for (int m = 0; m < width; m++)
                {
                    byte* numPtr2 = (numPtr + (j * num5)) + m;
                    Color pixel = bitmap.GetPixel(m, j);
                    byte num8 = (byte) colorIndexTable[pixel];
                    numPtr2[0] = num8;
                }
            }
            colorIndexTable.CopyToColorPalette(palette);
            bitmap2.Palette = palette;
            bitmap2.UnlockBits(bitmapdata);
            return bitmap2;
        }

        private void RenderImage(Graphics g, int x, int y, int width, int height, char lineType, int lineStyle, int lineWidth, Color lineColor, System.Drawing.Image image)
        {
            Pen pen = new Pen(lineColor, (float) lineWidth);
            switch (lineStyle)
            {
                case 0:
                    pen.DashStyle = DashStyle.Dot;
                    break;

                case 1:
                    pen.DashStyle = DashStyle.Dash;
                    break;

                default:
                    pen.DashStyle = DashStyle.Solid;
                    break;
            }
            if (lineType == 'i')
            {
                g.DrawLine(pen, x + (width / 2), y, x + (width / 2), y + height);
            }
            else if (lineType == 'r')
            {
                g.DrawLine(pen, (int) (x + (width / 2)), (int) (y + (height / 2)), (int) (x + width), (int) (y + (height / 2)));
                g.DrawLine(pen, (int) (x + (width / 2)), (int) (y + (height / 2)), (int) (x + (width / 2)), (int) (y + height));
            }
            else if (lineType == 't')
            {
                g.DrawLine(pen, x + (width / 2), y, x + (width / 2), y + height);
                g.DrawLine(pen, (int) (x + (width / 2)), (int) (y + (height / 2)), (int) (x + width), (int) (y + (height / 2)));
            }
            else if (lineType == 'l')
            {
                g.DrawLine(pen, x + (width / 2), y, x + (width / 2), y + (height / 2));
                g.DrawLine(pen, (int) (x + (width / 2)), (int) (y + (height / 2)), (int) (x + width), (int) (y + (height / 2)));
            }
            else if (lineType == '-')
            {
                g.DrawLine(pen, (int) (x + (width / 2)), (int) (y + (height / 2)), (int) (x + width), (int) (y + (height / 2)));
            }
            if (image != null)
            {
                int num = Math.Min(image.Width, width);
                int num2 = Math.Min(image.Height, height);
                g.DrawImage(image, x + (((width - num) + 1) / 2), y + (((height - num2) + 1) / 2), num, num2);
            }
            pen.Dispose();
        }

        private bool SaveTransparentGif(Bitmap bitmap, IFolderProjectItem folder, string name, ref bool overwrite)
        {
            System.Drawing.Image image = ReduceColors(bitmap, 0x100, 5, this._imageInfo.TransparentColor);
            try
            {
                MemoryStream stream = new MemoryStream();
                image.Save(stream, ImageFormat.Gif);
                stream.Flush();
                stream.Capacity = (int) stream.Length;
                folder.AddDocument(name, stream.GetBuffer());
            }
            finally
            {
                image.Dispose();
            }
            return false;
        }

        private void UpdatePreview()
        {
            System.Drawing.Image expandImage = this._imageInfo.ExpandImage;
            if (expandImage == null)
            {
                expandImage = this.DefaultPlusImage;
            }
            System.Drawing.Image collapseImage = this._imageInfo.CollapseImage;
            if (collapseImage == null)
            {
                collapseImage = this.DefaultMinusImage;
            }
            System.Drawing.Image noExpandImage = this._imageInfo.NoExpandImage;
            int width = this._imageInfo.Width;
            if (width < 1)
            {
                UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("TreeViewImageGenerator_InvalidValue", new object[] { "Width" }));
            }
            else
            {
                int height = this._imageInfo.Height;
                if (height < 1)
                {
                    UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("TreeViewImageGenerator_InvalidValue", new object[] { "Height" }));
                }
                else
                {
                    int lineWidth = this._imageInfo.LineWidth;
                    if (lineWidth < 1)
                    {
                        UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("TreeViewImageGenerator_InvalidValue", new object[] { "LineWidth" }));
                    }
                    else
                    {
                        int lineStyle = (int) this._imageInfo.LineStyle;
                        Color lineColor = this._imageInfo.LineColor;
                        Font font = new Font("Tahoma", 10f);
                        Graphics graphics = Graphics.FromHwnd(base.Handle);
                        int num5 = (width * 2) + ((int) graphics.MeasureString(System.Design.SR.GetString("TreeViewImageGenerator_SampleParent", new object[] { 1 }), font).Width);
                        int num6 = Math.Max((int) graphics.MeasureString(System.Design.SR.GetString("TreeViewImageGenerator_SampleParent", new object[] { 1 }), font).Height, height);
                        graphics.Dispose();
                        int num7 = num6 * 6;
                        int num8 = Math.Max(width, this._treeView.NodeIndent);
                        Bitmap image = new Bitmap(Math.Max(num5, this._previewPanel.Width), Math.Max(num7, this._previewPanel.Height));
                        Graphics g = Graphics.FromImage(image);
                        int x = 5;
                        int y = 5;
                        g.FillRectangle(Brushes.White, x, y, num5, num7);
                        this.RenderImage(g, x, y, width, height, '-', lineStyle, lineWidth, lineColor, expandImage);
                        x += width;
                        g.DrawString(System.Design.SR.GetString("TreeViewImageGenerator_SampleRoot", new object[] { 1 }), font, Brushes.Black, (float) x, y + (((height - g.MeasureString(System.Design.SR.GetString("TreeViewImageGenerator_SampleRoot", new object[] { 1 }), font).Height) + 1f) / 2f));
                        y += num6;
                        x -= width;
                        this.RenderImage(g, x, y, width, height, 'r', lineStyle, lineWidth, lineColor, collapseImage);
                        x += width;
                        g.DrawString(System.Design.SR.GetString("TreeViewImageGenerator_SampleRoot", new object[] { 2 }), font, Brushes.Black, (float) x, y + (((height - g.MeasureString(System.Design.SR.GetString("TreeViewImageGenerator_SampleRoot", new object[] { 2 }), font).Height) + 1f) / 2f));
                        y += num6;
                        x -= width;
                        this.RenderImage(g, x, y, width, height, 'i', lineStyle, lineWidth, lineColor, null);
                        x += num8;
                        this.RenderImage(g, x, y, width, height, 't', lineStyle, lineWidth, lineColor, expandImage);
                        x += width;
                        g.DrawString(System.Design.SR.GetString("TreeViewImageGenerator_SampleParent", new object[] { 1 }), font, Brushes.Black, (float) x, y + (((height - g.MeasureString(System.Design.SR.GetString("TreeViewImageGenerator_SampleParent", new object[] { 1 }), font).Height) + 1f) / 2f));
                        y += num6;
                        x -= width + num8;
                        this.RenderImage(g, x, y, width, height, 'i', lineStyle, lineWidth, lineColor, null);
                        x += num8;
                        this.RenderImage(g, x, y, width, height, 't', lineStyle, lineWidth, lineColor, noExpandImage);
                        x += width;
                        g.DrawString(System.Design.SR.GetString("TreeViewImageGenerator_SampleLeaf", new object[] { 1 }), font, Brushes.Black, (float) x, y + (((height - g.MeasureString(System.Design.SR.GetString("TreeViewImageGenerator_SampleLeaf", new object[] { 1 }), font).Height) + 1f) / 2f));
                        y += num6;
                        x -= width + num8;
                        this.RenderImage(g, x, y, width, height, 'i', lineStyle, lineWidth, lineColor, null);
                        x += num8;
                        this.RenderImage(g, x, y, width, height, 'l', lineStyle, lineWidth, lineColor, noExpandImage);
                        x += width;
                        g.DrawString(System.Design.SR.GetString("TreeViewImageGenerator_SampleLeaf", new object[] { 2 }), font, Brushes.Black, (float) x, y + (((height - g.MeasureString(System.Design.SR.GetString("TreeViewImageGenerator_SampleLeaf", new object[] { 2 }), font).Height) + 1f) / 2f));
                        y += num6;
                        x -= width + num8;
                        this.RenderImage(g, x, y, width, height, 'l', lineStyle, lineWidth, lineColor, expandImage);
                        x += width;
                        g.DrawString(System.Design.SR.GetString("TreeViewImageGenerator_SampleRoot", new object[] { 3 }), font, Brushes.Black, (float) x, y + (((height - g.MeasureString(System.Design.SR.GetString("TreeViewImageGenerator_SampleRoot", new object[] { 3 }), font).Height) + 1f) / 2f));
                        g.Dispose();
                        image.MakeTransparent(this._imageInfo.TransparentColor);
                        this._previewPictureBox.Image = image;
                        this._previewPictureBox.Width = Math.Max(num5, this._previewPanel.Width);
                        this._previewPictureBox.Height = Math.Max(num7, this._previewPanel.Height);
                    }
                }
            }
        }

        private System.Drawing.Image DefaultMinusImage
        {
            get
            {
                if (defaultMinusImage == null)
                {
                    defaultMinusImage = new Bitmap(typeof(TreeViewImageGenerator), "Minus.gif");
                }
                return defaultMinusImage;
            }
        }

        private System.Drawing.Image DefaultPlusImage
        {
            get
            {
                if (defaultPlusImage == null)
                {
                    defaultPlusImage = new Bitmap(typeof(TreeViewImageGenerator), "Plus.gif");
                }
                return defaultPlusImage;
            }
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.TreeView.ImageGenerator";
            }
        }

        private class ColorIndexTable
        {
            private Color[] _colors;
            private IDictionary _table;

            internal ColorIndexTable(IDictionary table, Color[] colors)
            {
                this._table = table;
                this._colors = colors;
            }

            public void CopyToColorPalette(ColorPalette palette)
            {
                for (int i = 0; i < this._colors.Length; i++)
                {
                    palette.Entries[i] = this._colors[i];
                }
            }

            internal static int GetColorKey(Color c)
            {
                return ((((c.R & 0xff) << 0x10) | ((c.G & 0xff) << 8)) | (c.B & 0xff));
            }

            public int this[Color c]
            {
                get
                {
                    object obj2 = this._table[GetColorKey(c)];
                    if (obj2 == null)
                    {
                        return 0;
                    }
                    return (int) obj2;
                }
            }
        }

        private class LineImageInfo
        {
            private System.Drawing.Image _collapseImage;
            private System.Drawing.Image _expandImage;
            private int _height = 20;
            private Color _lineColor = Color.Black;
            private System.Web.UI.Design.WebControls.TreeViewImageGenerator.LineStyle _lineStyle = System.Web.UI.Design.WebControls.TreeViewImageGenerator.LineStyle.Dotted;
            private int _lineWidth = 1;
            private System.Drawing.Image _noExpandImage;
            private Color _transparentColor = Color.Magenta;
            private int _width = 0x13;
            private const int MaxSize = 300;

            [DefaultValue((string) null), System.Design.SRDescription("TreeViewImageGenerator_CollapseImage")]
            public System.Drawing.Image CollapseImage
            {
                get
                {
                    return this._collapseImage;
                }
                set
                {
                    this._collapseImage = value;
                }
            }

            [DefaultValue((string) null), System.Design.SRDescription("TreeViewImageGenerator_ExpandImage")]
            public System.Drawing.Image ExpandImage
            {
                get
                {
                    return this._expandImage;
                }
                set
                {
                    this._expandImage = value;
                }
            }

            [System.Design.SRDescription("TreeViewImageGenerator_LineImageHeight")]
            public int Height
            {
                get
                {
                    return this._height;
                }
                set
                {
                    if (value > 300)
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                    this._height = value;
                }
            }

            [System.Design.SRDescription("TreeViewImageGenerator_LineColor")]
            public Color LineColor
            {
                get
                {
                    return this._lineColor;
                }
                set
                {
                    this._lineColor = value;
                }
            }

            [System.Design.SRDescription("TreeViewImageGenerator_LineStyle")]
            public System.Web.UI.Design.WebControls.TreeViewImageGenerator.LineStyle LineStyle
            {
                get
                {
                    return this._lineStyle;
                }
                set
                {
                    this._lineStyle = value;
                }
            }

            [System.Design.SRDescription("TreeViewImageGenerator_LineWidth")]
            public int LineWidth
            {
                get
                {
                    return this._lineWidth;
                }
                set
                {
                    if (value > 300)
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                    this._lineWidth = value;
                }
            }

            [System.Design.SRDescription("TreeViewImageGenerator_NoExpandImage"), DefaultValue((string) null)]
            public System.Drawing.Image NoExpandImage
            {
                get
                {
                    return this._noExpandImage;
                }
                set
                {
                    this._noExpandImage = value;
                }
            }

            [DefaultValue(typeof(Color), "Magenta"), System.Design.SRDescription("TreeViewImageGenerator_TransparentColor")]
            public Color TransparentColor
            {
                get
                {
                    return this._transparentColor;
                }
                set
                {
                    this._transparentColor = value;
                }
            }

            [System.Design.SRDescription("TreeViewImageGenerator_LineImageWidth")]
            public int Width
            {
                get
                {
                    return this._width;
                }
                set
                {
                    if (value > 300)
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                    this._width = value;
                }
            }
        }

        private enum LineStyle
        {
            Dotted,
            Dashed,
            Solid
        }

        private class Octree
        {
            private bool _hasTransparency;
            private ArrayList _leafNodes;
            private ArrayList[] _levels;
            private int _maxColors;
            private int _numBits;
            private TreeViewImageGenerator.OctreeNode _root = new TreeViewImageGenerator.OctreeNode();
            private Color _transparentColor;

            public Octree(int maxColors, int numBits, Color transparentColor)
            {
                this._maxColors = maxColors;
                this._leafNodes = new ArrayList();
                this._numBits = numBits;
                this._transparentColor = transparentColor;
                if (!this._transparentColor.IsEmpty)
                {
                    this._hasTransparency = true;
                    this._maxColors--;
                }
                this._levels = new ArrayList[this._numBits - 1];
                for (int i = 0; i < this._levels.Length; i++)
                {
                    this._levels[i] = new ArrayList();
                }
            }

            public void AddColor(Color c)
            {
                if ((!this._hasTransparency || (this._transparentColor.R != c.R)) || ((this._transparentColor.G != c.G) || (this._transparentColor.B != c.B)))
                {
                    int depth = -1;
                    if (this._leafNodes.Count >= this._maxColors)
                    {
                        TreeViewImageGenerator.OctreeNode node = null;
                        for (int i = this._numBits - 2; i > 0; i--)
                        {
                            ArrayList list = this._levels[i];
                            if (list.Count > 0)
                            {
                                depth = i;
                                int pixelCount = -1;
                                for (int j = 0; j < list.Count; j++)
                                {
                                    TreeViewImageGenerator.OctreeNode node2 = (TreeViewImageGenerator.OctreeNode) list[j];
                                    if (node2.PixelCount > pixelCount)
                                    {
                                        node = node2;
                                        pixelCount = node2.PixelCount;
                                    }
                                }
                                break;
                            }
                        }
                        this.ReduceNode(node, depth);
                        this._leafNodes.Add(node);
                    }
                    TreeViewImageGenerator.OctreeNode node3 = this._root;
                    depth = 0;
                    bool flag = false;
                    while (depth < (this._numBits - 1))
                    {
                        int index = this.GetIndex(c, depth);
                        TreeViewImageGenerator.OctreeNode node4 = node3[index];
                        if (node4 == null)
                        {
                            node4 = new TreeViewImageGenerator.OctreeNode();
                            node3[index] = node4;
                            flag = true;
                            if (node3.NodeCount == 2)
                            {
                                this._levels[depth].Add(node3);
                            }
                        }
                        node3 = node4;
                        node3.AddColor(c);
                        if (node3.Reduced)
                        {
                            break;
                        }
                        depth++;
                    }
                    if (flag)
                    {
                        this._leafNodes.Add(node3);
                    }
                }
            }

            public TreeViewImageGenerator.ColorIndexTable GetColorIndexTable()
            {
                Hashtable table = new Hashtable();
                Color[] colors = new Color[this._maxColors];
                int index = 0;
                if (!this._transparentColor.IsEmpty)
                {
                    table[TreeViewImageGenerator.ColorIndexTable.GetColorKey(this._transparentColor)] = 0;
                    colors[0] = Color.FromArgb(0, this._transparentColor);
                    index = 1;
                }
                foreach (TreeViewImageGenerator.OctreeNode node in this._leafNodes)
                {
                    int num3 = 0;
                    int num4 = 0;
                    int num5 = 0;
                    foreach (Color color in node.Colors)
                    {
                        int colorKey = TreeViewImageGenerator.ColorIndexTable.GetColorKey(color);
                        table[colorKey] = index;
                        num3 += color.R;
                        num4 += color.G;
                        num5 += color.B;
                    }
                    int count = node.Colors.Count;
                    colors[index] = Color.FromArgb(0xff, num3 / count, num4 / count, num5 / count);
                    index++;
                }
                return new TreeViewImageGenerator.ColorIndexTable(table, colors);
            }

            private int GetIndex(Color c, int depth)
            {
                int num = 7 - depth;
                return (((((c.R >> (num & 0x1f)) & 1) << 2) | (((c.G >> (num & 0x1f)) & 1) << 1)) | ((c.B >> num) & 1));
            }

            private void ReduceNode(TreeViewImageGenerator.OctreeNode node, int depth)
            {
                ArrayList list = null;
                if (depth < (this._numBits - 2))
                {
                    list = this._levels[depth + 1];
                }
                for (int i = 0; i < 8; i++)
                {
                    TreeViewImageGenerator.OctreeNode node2 = node[i];
                    if (node2 != null)
                    {
                        if (depth < (this._numBits - 2))
                        {
                            this.ReduceNode(node2, depth + 1);
                        }
                        if (list != null)
                        {
                            list.Remove(node2);
                        }
                        if (node2.NodeCount == 0)
                        {
                            this._leafNodes.Remove(node2);
                        }
                        node[i] = null;
                    }
                    this._levels[depth].Remove(node);
                    node.Reduced = true;
                }
            }
        }

        private class OctreeNode
        {
            private ArrayList _colors = new ArrayList();
            private int _nodeCount = 0;
            private TreeViewImageGenerator.OctreeNode[] _nodes = new TreeViewImageGenerator.OctreeNode[8];
            private bool _reduced = false;

            public void AddColor(Color c)
            {
                this._colors.Add(c);
            }

            public ICollection Colors
            {
                get
                {
                    return this._colors;
                }
            }

            public TreeViewImageGenerator.OctreeNode this[int index]
            {
                get
                {
                    return this._nodes[index];
                }
                set
                {
                    this._nodes[index] = value;
                    if (this._nodes[index] == null)
                    {
                        this._nodeCount--;
                    }
                    else
                    {
                        this._nodeCount++;
                    }
                }
            }

            public int NodeCount
            {
                get
                {
                    return this._nodeCount;
                }
            }

            public int PixelCount
            {
                get
                {
                    return this._colors.Count;
                }
            }

            public bool Reduced
            {
                get
                {
                    return this._reduced;
                }
                set
                {
                    this._reduced = value;
                }
            }
        }
    }
}

