namespace System.Windows.Forms
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Windows.Forms.Layout;

    [ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), System.Windows.Forms.SRDescription("DescriptionPictureBox"), DefaultProperty("Image"), DefaultBindingProperty("Image"), Docking(DockingBehavior.Ask), Designer("System.Windows.Forms.Design.PictureBoxDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class PictureBox : Control, ISupportInitialize
    {
        private System.Windows.Forms.BorderStyle borderStyle;
        private int contentLength;
        private AsyncOperation currentAsyncLoadOperation;
        private bool currentlyAnimating;
        private System.Drawing.Image defaultErrorImage;
        [ThreadStatic]
        private static System.Drawing.Image defaultErrorImageForThread = null;
        private static readonly object defaultErrorImageKey = new object();
        private System.Drawing.Image defaultInitialImage;
        [ThreadStatic]
        private static System.Drawing.Image defaultInitialImageForThread = null;
        private static readonly object defaultInitialImageKey = new object();
        private System.Drawing.Image errorImage;
        private static readonly object EVENT_SIZEMODECHANGED = new object();
        private bool handleValid;
        private System.Drawing.Image image;
        private ImageInstallationType imageInstallationType;
        private string imageLocation;
        private System.Drawing.Image initialImage;
        private object internalSyncObject = new object();
        private SendOrPostCallback loadCompletedDelegate;
        private static readonly object loadCompletedKey = new object();
        private static readonly object loadProgressChangedKey = new object();
        private SendOrPostCallback loadProgressDelegate;
        private BitVector32 pictureBoxState;
        private const int PICTUREBOXSTATE_asyncOperationInProgress = 1;
        private const int PICTUREBOXSTATE_cancellationPending = 2;
        private const int PICTUREBOXSTATE_inInitialization = 0x40;
        private const int PICTUREBOXSTATE_needToLoadImageLocation = 0x20;
        private const int PICTUREBOXSTATE_useDefaultErrorImage = 8;
        private const int PICTUREBOXSTATE_useDefaultInitialImage = 4;
        private const int PICTUREBOXSTATE_waitOnLoad = 0x10;
        private const int readBlockSize = 0x1000;
        private byte[] readBuffer;
        private Size savedSize;
        private PictureBoxSizeMode sizeMode;
        private MemoryStream tempDownloadStream;
        private int totalBytesRead;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler CausesValidationChanged
        {
            add
            {
                base.CausesValidationChanged += value;
            }
            remove
            {
                base.CausesValidationChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler Enter
        {
            add
            {
                base.Enter += value;
            }
            remove
            {
                base.Enter -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler FontChanged
        {
            add
            {
                base.FontChanged += value;
            }
            remove
            {
                base.FontChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler ForeColorChanged
        {
            add
            {
                base.ForeColorChanged += value;
            }
            remove
            {
                base.ForeColorChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler ImeModeChanged
        {
            add
            {
                base.ImeModeChanged += value;
            }
            remove
            {
                base.ImeModeChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event KeyEventHandler KeyDown
        {
            add
            {
                base.KeyDown += value;
            }
            remove
            {
                base.KeyDown -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event KeyPressEventHandler KeyPress
        {
            add
            {
                base.KeyPress += value;
            }
            remove
            {
                base.KeyPress -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event KeyEventHandler KeyUp
        {
            add
            {
                base.KeyUp += value;
            }
            remove
            {
                base.KeyUp -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler Leave
        {
            add
            {
                base.Leave += value;
            }
            remove
            {
                base.Leave -= value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAsynchronous"), System.Windows.Forms.SRDescription("PictureBoxLoadCompletedDescr")]
        public event AsyncCompletedEventHandler LoadCompleted
        {
            add
            {
                base.Events.AddHandler(loadCompletedKey, value);
            }
            remove
            {
                base.Events.RemoveHandler(loadCompletedKey, value);
            }
        }

        [System.Windows.Forms.SRDescription("PictureBoxLoadProgressChangedDescr"), System.Windows.Forms.SRCategory("CatAsynchronous")]
        public event ProgressChangedEventHandler LoadProgressChanged
        {
            add
            {
                base.Events.AddHandler(loadProgressChangedKey, value);
            }
            remove
            {
                base.Events.RemoveHandler(loadProgressChangedKey, value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler RightToLeftChanged
        {
            add
            {
                base.RightToLeftChanged += value;
            }
            remove
            {
                base.RightToLeftChanged -= value;
            }
        }

        [System.Windows.Forms.SRDescription("PictureBoxOnSizeModeChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler SizeModeChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_SIZEMODECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_SIZEMODECHANGED, value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler TabIndexChanged
        {
            add
            {
                base.TabIndexChanged += value;
            }
            remove
            {
                base.TabIndexChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler TabStopChanged
        {
            add
            {
                base.TabStopChanged += value;
            }
            remove
            {
                base.TabStopChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler TextChanged
        {
            add
            {
                base.TextChanged += value;
            }
            remove
            {
                base.TextChanged -= value;
            }
        }

        public PictureBox()
        {
            base.SetState2(0x800, true);
            this.pictureBoxState = new BitVector32(12);
            base.SetStyle(ControlStyles.Selectable | ControlStyles.Opaque, false);
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            this.TabStop = false;
            this.savedSize = base.Size;
        }

        private void AdjustSize()
        {
            if (this.sizeMode == PictureBoxSizeMode.AutoSize)
            {
                base.Size = base.PreferredSize;
            }
            else
            {
                base.Size = this.savedSize;
            }
        }

        private void Animate()
        {
            this.Animate(((!base.DesignMode && base.Visible) && base.Enabled) && (this.ParentInternal != null));
        }

        private void Animate(bool animate)
        {
            if (animate != this.currentlyAnimating)
            {
                if (animate)
                {
                    if (this.image != null)
                    {
                        ImageAnimator.Animate(this.image, new EventHandler(this.OnFrameChanged));
                        this.currentlyAnimating = animate;
                    }
                }
                else if (this.image != null)
                {
                    ImageAnimator.StopAnimate(this.image, new EventHandler(this.OnFrameChanged));
                    this.currentlyAnimating = animate;
                }
            }
        }

        private void BeginGetResponseDelegate(object arg)
        {
            WebRequest state = (WebRequest) arg;
            state.BeginGetResponse(new AsyncCallback(this.GetResponseCallback), state);
        }

        private Uri CalculateUri(string path)
        {
            try
            {
                return new Uri(path);
            }
            catch (UriFormatException)
            {
                path = Path.GetFullPath(path);
                return new Uri(path);
            }
        }

        [System.Windows.Forms.SRDescription("PictureBoxCancelAsyncDescr"), System.Windows.Forms.SRCategory("CatAsynchronous")]
        public void CancelAsync()
        {
            this.pictureBoxState[2] = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.StopAnimate();
            }
            base.Dispose(disposing);
        }

        internal override Size GetPreferredSizeCore(Size proposedSize)
        {
            if (this.image == null)
            {
                return CommonProperties.GetSpecifiedBounds(this).Size;
            }
            Size size = this.SizeFromClientSize(Size.Empty) + base.Padding.Size;
            return (this.image.Size + size);
        }

        private void GetResponseCallback(IAsyncResult result)
        {
            if (this.pictureBoxState[2])
            {
                this.PostCompleted(null, true);
            }
            else
            {
                try
                {
                    WebResponse response = ((WebRequest) result.AsyncState).EndGetResponse(result);
                    this.contentLength = (int) response.ContentLength;
                    this.totalBytesRead = 0;
                    Stream responseStream = response.GetResponseStream();
                    responseStream.BeginRead(this.readBuffer, 0, 0x1000, new AsyncCallback(this.ReadCallBack), responseStream);
                }
                catch (Exception exception)
                {
                    this.PostCompleted(exception, false);
                }
            }
        }

        private Rectangle ImageRectangleFromSizeMode(PictureBoxSizeMode mode)
        {
            Rectangle rectangle = LayoutUtils.DeflateRect(base.ClientRectangle, base.Padding);
            if (this.image != null)
            {
                switch (mode)
                {
                    case PictureBoxSizeMode.Normal:
                    case PictureBoxSizeMode.AutoSize:
                        rectangle.Size = this.image.Size;
                        return rectangle;

                    case PictureBoxSizeMode.StretchImage:
                        return rectangle;

                    case PictureBoxSizeMode.CenterImage:
                        rectangle.X += (rectangle.Width - this.image.Width) / 2;
                        rectangle.Y += (rectangle.Height - this.image.Height) / 2;
                        rectangle.Size = this.image.Size;
                        return rectangle;

                    case PictureBoxSizeMode.Zoom:
                    {
                        Size size = this.image.Size;
                        float num = Math.Min((float) (((float) base.ClientRectangle.Width) / ((float) size.Width)), (float) (((float) base.ClientRectangle.Height) / ((float) size.Height)));
                        rectangle.Width = (int) (size.Width * num);
                        rectangle.Height = (int) (size.Height * num);
                        rectangle.X = (base.ClientRectangle.Width - rectangle.Width) / 2;
                        rectangle.Y = (base.ClientRectangle.Height - rectangle.Height) / 2;
                        return rectangle;
                    }
                }
            }
            return rectangle;
        }

        private void InstallNewImage(System.Drawing.Image value, ImageInstallationType installationType)
        {
            this.StopAnimate();
            this.image = value;
            LayoutTransaction.DoLayoutIf(this.AutoSize, this, this, PropertyNames.Image);
            this.Animate();
            if (installationType != ImageInstallationType.ErrorOrInitial)
            {
                this.AdjustSize();
            }
            this.imageInstallationType = installationType;
            base.Invalidate();
            CommonProperties.xClearPreferredSizeCache(this);
        }

        [System.Windows.Forms.SRDescription("PictureBoxLoad0Descr"), System.Windows.Forms.SRCategory("CatAsynchronous")]
        public void Load()
        {
            System.Drawing.Image errorImage;
            if ((this.imageLocation == null) || (this.imageLocation.Length == 0))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("PictureBoxNoImageLocation"));
            }
            this.pictureBoxState[0x20] = false;
            ImageInstallationType fromUrl = ImageInstallationType.FromUrl;
            try
            {
                Uri uri = this.CalculateUri(this.imageLocation);
                if (uri.IsFile)
                {
                    using (StreamReader reader = new StreamReader(uri.LocalPath))
                    {
                        errorImage = System.Drawing.Image.FromStream(reader.BaseStream);
                        goto Label_00C0;
                    }
                }
                using (WebClient client = new WebClient())
                {
                    using (Stream stream = client.OpenRead(uri.ToString()))
                    {
                        errorImage = System.Drawing.Image.FromStream(stream);
                    }
                }
            }
            catch
            {
                if (!base.DesignMode)
                {
                    throw;
                }
                errorImage = this.ErrorImage;
                fromUrl = ImageInstallationType.ErrorOrInitial;
            }
        Label_00C0:
            this.InstallNewImage(errorImage, fromUrl);
        }

        [System.Windows.Forms.SRCategory("CatAsynchronous"), System.Windows.Forms.SRDescription("PictureBoxLoad1Descr")]
        public void Load(string url)
        {
            this.ImageLocation = url;
            this.Load();
        }

        [System.Windows.Forms.SRDescription("PictureBoxLoadAsync0Descr"), System.Windows.Forms.SRCategory("CatAsynchronous")]
        public void LoadAsync()
        {
            if ((this.imageLocation == null) || (this.imageLocation.Length == 0))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("PictureBoxNoImageLocation"));
            }
            if (!this.pictureBoxState[1])
            {
                this.pictureBoxState[1] = true;
                if (((this.Image == null) || (this.imageInstallationType == ImageInstallationType.ErrorOrInitial)) && (this.InitialImage != null))
                {
                    this.InstallNewImage(this.InitialImage, ImageInstallationType.ErrorOrInitial);
                }
                this.currentAsyncLoadOperation = AsyncOperationManager.CreateOperation(null);
                if (this.loadCompletedDelegate == null)
                {
                    this.loadCompletedDelegate = new SendOrPostCallback(this.LoadCompletedDelegate);
                    this.loadProgressDelegate = new SendOrPostCallback(this.LoadProgressDelegate);
                    this.readBuffer = new byte[0x1000];
                }
                this.pictureBoxState[0x20] = false;
                this.pictureBoxState[2] = false;
                this.contentLength = -1;
                this.tempDownloadStream = new MemoryStream();
                WebRequest state = WebRequest.Create(this.CalculateUri(this.imageLocation));
                new WaitCallback(this.BeginGetResponseDelegate).BeginInvoke(state, null, null);
            }
        }

        [System.Windows.Forms.SRDescription("PictureBoxLoadAsync1Descr"), System.Windows.Forms.SRCategory("CatAsynchronous")]
        public void LoadAsync(string url)
        {
            this.ImageLocation = url;
            this.LoadAsync();
        }

        private void LoadCompletedDelegate(object arg)
        {
            AsyncCompletedEventArgs e = (AsyncCompletedEventArgs) arg;
            System.Drawing.Image errorImage = this.ErrorImage;
            ImageInstallationType errorOrInitial = ImageInstallationType.ErrorOrInitial;
            if (!e.Cancelled && (e.Error == null))
            {
                try
                {
                    errorImage = System.Drawing.Image.FromStream(this.tempDownloadStream);
                    errorOrInitial = ImageInstallationType.FromUrl;
                }
                catch (Exception exception)
                {
                    e = new AsyncCompletedEventArgs(exception, false, null);
                }
            }
            if (!e.Cancelled)
            {
                this.InstallNewImage(errorImage, errorOrInitial);
            }
            this.tempDownloadStream = null;
            this.pictureBoxState[2] = false;
            this.pictureBoxState[1] = false;
            this.OnLoadCompleted(e);
        }

        private void LoadProgressDelegate(object arg)
        {
            this.OnLoadProgressChanged((ProgressChangedEventArgs) arg);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            this.Animate();
        }

        private void OnFrameChanged(object o, EventArgs e)
        {
            if (!base.Disposing && !base.IsDisposed)
            {
                if (base.InvokeRequired && base.IsHandleCreated)
                {
                    lock (this.internalSyncObject)
                    {
                        if (this.handleValid)
                        {
                            base.BeginInvoke(new EventHandler(this.OnFrameChanged), new object[] { o, e });
                        }
                        return;
                    }
                }
                base.Invalidate();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnHandleCreated(EventArgs e)
        {
            lock (this.internalSyncObject)
            {
                this.handleValid = true;
            }
            base.OnHandleCreated(e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnHandleDestroyed(EventArgs e)
        {
            lock (this.internalSyncObject)
            {
                this.handleValid = false;
            }
            base.OnHandleDestroyed(e);
        }

        protected virtual void OnLoadCompleted(AsyncCompletedEventArgs e)
        {
            AsyncCompletedEventHandler handler = (AsyncCompletedEventHandler) base.Events[loadCompletedKey];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnLoadProgressChanged(ProgressChangedEventArgs e)
        {
            ProgressChangedEventHandler handler = (ProgressChangedEventHandler) base.Events[loadProgressChangedKey];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (this.pictureBoxState[0x20])
            {
                try
                {
                    if (this.WaitOnLoad)
                    {
                        this.Load();
                    }
                    else
                    {
                        this.LoadAsync();
                    }
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                    {
                        throw;
                    }
                    this.image = this.ErrorImage;
                }
            }
            if (this.image != null)
            {
                this.Animate();
                ImageAnimator.UpdateFrames();
                Rectangle rect = (this.imageInstallationType == ImageInstallationType.ErrorOrInitial) ? this.ImageRectangleFromSizeMode(PictureBoxSizeMode.CenterImage) : this.ImageRectangle;
                pe.Graphics.DrawImage(this.image, rect);
            }
            base.OnPaint(pe);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            this.Animate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (((this.sizeMode == PictureBoxSizeMode.Zoom) || (this.sizeMode == PictureBoxSizeMode.StretchImage)) || ((this.sizeMode == PictureBoxSizeMode.CenterImage) || (this.BackgroundImage != null)))
            {
                base.Invalidate();
            }
            this.savedSize = base.Size;
        }

        protected virtual void OnSizeModeChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_SIZEMODECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            this.Animate();
        }

        private void PostCompleted(Exception error, bool cancelled)
        {
            AsyncOperation currentAsyncLoadOperation = this.currentAsyncLoadOperation;
            this.currentAsyncLoadOperation = null;
            if (currentAsyncLoadOperation != null)
            {
                currentAsyncLoadOperation.PostOperationCompleted(this.loadCompletedDelegate, new AsyncCompletedEventArgs(error, cancelled, null));
            }
        }

        private void ReadCallBack(IAsyncResult result)
        {
            if (this.pictureBoxState[2])
            {
                this.PostCompleted(null, true);
            }
            else
            {
                Stream asyncState = (Stream) result.AsyncState;
                try
                {
                    int count = asyncState.EndRead(result);
                    if (count > 0)
                    {
                        this.totalBytesRead += count;
                        this.tempDownloadStream.Write(this.readBuffer, 0, count);
                        asyncState.BeginRead(this.readBuffer, 0, 0x1000, new AsyncCallback(this.ReadCallBack), asyncState);
                        if (this.contentLength != -1)
                        {
                            int progressPercentage = (int) (100f * (((float) this.totalBytesRead) / ((float) this.contentLength)));
                            if (this.currentAsyncLoadOperation != null)
                            {
                                this.currentAsyncLoadOperation.Post(this.loadProgressDelegate, new ProgressChangedEventArgs(progressPercentage, null));
                            }
                        }
                    }
                    else
                    {
                        this.tempDownloadStream.Seek(0L, SeekOrigin.Begin);
                        if (this.currentAsyncLoadOperation != null)
                        {
                            this.currentAsyncLoadOperation.Post(this.loadProgressDelegate, new ProgressChangedEventArgs(100, null));
                        }
                        this.PostCompleted(null, false);
                        Stream stream2 = asyncState;
                        asyncState = null;
                        stream2.Close();
                    }
                }
                catch (Exception exception)
                {
                    this.PostCompleted(exception, false);
                    if (asyncState != null)
                    {
                        asyncState.Close();
                    }
                }
            }
        }

        private void ResetErrorImage()
        {
            this.pictureBoxState[8] = true;
            this.errorImage = this.defaultErrorImage;
        }

        private void ResetImage()
        {
            this.InstallNewImage(null, ImageInstallationType.DirectlySpecified);
        }

        private void ResetInitialImage()
        {
            this.pictureBoxState[4] = true;
            this.initialImage = this.defaultInitialImage;
        }

        private bool ShouldSerializeErrorImage()
        {
            return !this.pictureBoxState[8];
        }

        private bool ShouldSerializeImage()
        {
            return ((this.imageInstallationType == ImageInstallationType.DirectlySpecified) && (this.Image != null));
        }

        private bool ShouldSerializeInitialImage()
        {
            return !this.pictureBoxState[4];
        }

        private void StopAnimate()
        {
            this.Animate(false);
        }

        void ISupportInitialize.BeginInit()
        {
            this.pictureBoxState[0x40] = true;
        }

        void ISupportInitialize.EndInit()
        {
            if (((this.ImageLocation != null) && (this.ImageLocation.Length != 0)) && this.WaitOnLoad)
            {
                this.Load();
            }
            this.pictureBoxState[0x40] = false;
        }

        public override string ToString()
        {
            return (base.ToString() + ", SizeMode: " + this.sizeMode.ToString("G"));
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool AllowDrop
        {
            get
            {
                return base.AllowDrop;
            }
            set
            {
                base.AllowDrop = value;
            }
        }

        [DefaultValue(0), DispId(-504), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("PictureBoxBorderStyleDescr")]
        public System.Windows.Forms.BorderStyle BorderStyle
        {
            get
            {
                return this.borderStyle;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.BorderStyle));
                }
                if (this.borderStyle != value)
                {
                    this.borderStyle = value;
                    base.RecreateHandle();
                    this.AdjustSize();
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool CausesValidation
        {
            get
            {
                return base.CausesValidation;
            }
            set
            {
                base.CausesValidation = value;
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                switch (this.borderStyle)
                {
                    case System.Windows.Forms.BorderStyle.FixedSingle:
                        createParams.Style |= 0x800000;
                        return createParams;

                    case System.Windows.Forms.BorderStyle.Fixed3D:
                        createParams.ExStyle |= 0x200;
                        return createParams;
                }
                return createParams;
            }
        }

        protected override System.Windows.Forms.ImeMode DefaultImeMode
        {
            get
            {
                return System.Windows.Forms.ImeMode.Disable;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(100, 50);
            }
        }

        [System.Windows.Forms.SRCategory("CatAsynchronous"), Localizable(true), System.Windows.Forms.SRDescription("PictureBoxErrorImageDescr"), RefreshProperties(RefreshProperties.All)]
        public System.Drawing.Image ErrorImage
        {
            get
            {
                if ((this.errorImage == null) && this.pictureBoxState[8])
                {
                    if (this.defaultErrorImage == null)
                    {
                        if (defaultErrorImageForThread == null)
                        {
                            defaultErrorImageForThread = new Bitmap(typeof(PictureBox), "ImageInError.bmp");
                        }
                        this.defaultErrorImage = defaultErrorImageForThread;
                    }
                    this.errorImage = this.defaultErrorImage;
                }
                return this.errorImage;
            }
            set
            {
                if (this.ErrorImage != value)
                {
                    this.pictureBoxState[8] = false;
                }
                this.errorImage = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override System.Drawing.Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        [System.Windows.Forms.SRDescription("PictureBoxImageDescr"), Localizable(true), Bindable(true), System.Windows.Forms.SRCategory("CatAppearance")]
        public System.Drawing.Image Image
        {
            get
            {
                return this.image;
            }
            set
            {
                this.InstallNewImage(value, ImageInstallationType.DirectlySpecified);
            }
        }

        [RefreshProperties(RefreshProperties.All), Localizable(true), System.Windows.Forms.SRDescription("PictureBoxImageLocationDescr"), DefaultValue((string) null), System.Windows.Forms.SRCategory("CatAsynchronous")]
        public string ImageLocation
        {
            get
            {
                return this.imageLocation;
            }
            set
            {
                this.imageLocation = value;
                this.pictureBoxState[0x20] = !string.IsNullOrEmpty(this.imageLocation);
                if (string.IsNullOrEmpty(this.imageLocation) && (this.imageInstallationType != ImageInstallationType.DirectlySpecified))
                {
                    this.InstallNewImage(null, ImageInstallationType.DirectlySpecified);
                }
                if ((this.WaitOnLoad && !this.pictureBoxState[0x40]) && !string.IsNullOrEmpty(this.imageLocation))
                {
                    this.Load();
                }
                base.Invalidate();
            }
        }

        private Rectangle ImageRectangle
        {
            get
            {
                return this.ImageRectangleFromSizeMode(this.sizeMode);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public System.Windows.Forms.ImeMode ImeMode
        {
            get
            {
                return base.ImeMode;
            }
            set
            {
                base.ImeMode = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), Localizable(true), System.Windows.Forms.SRCategory("CatAsynchronous"), System.Windows.Forms.SRDescription("PictureBoxInitialImageDescr")]
        public System.Drawing.Image InitialImage
        {
            get
            {
                if ((this.initialImage == null) && this.pictureBoxState[4])
                {
                    if (this.defaultInitialImage == null)
                    {
                        if (defaultInitialImageForThread == null)
                        {
                            defaultInitialImageForThread = new Bitmap(typeof(PictureBox), "PictureBox.Loading.bmp");
                        }
                        this.defaultInitialImage = defaultInitialImageForThread;
                    }
                    this.initialImage = this.defaultInitialImage;
                }
                return this.initialImage;
            }
            set
            {
                if (this.InitialImage != value)
                {
                    this.pictureBoxState[4] = false;
                }
                this.initialImage = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override System.Windows.Forms.RightToLeft RightToLeft
        {
            get
            {
                return base.RightToLeft;
            }
            set
            {
                base.RightToLeft = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), RefreshProperties(RefreshProperties.Repaint), Localizable(true), DefaultValue(0), System.Windows.Forms.SRDescription("PictureBoxSizeModeDescr")]
        public PictureBoxSizeMode SizeMode
        {
            get
            {
                return this.sizeMode;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 4))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(PictureBoxSizeMode));
                }
                if (this.sizeMode != value)
                {
                    if (value == PictureBoxSizeMode.AutoSize)
                    {
                        this.AutoSize = true;
                        base.SetStyle(ControlStyles.FixedHeight | ControlStyles.FixedWidth, true);
                    }
                    if (value != PictureBoxSizeMode.AutoSize)
                    {
                        this.AutoSize = false;
                        base.SetStyle(ControlStyles.FixedHeight | ControlStyles.FixedWidth, false);
                        this.savedSize = base.Size;
                    }
                    this.sizeMode = value;
                    this.AdjustSize();
                    base.Invalidate();
                    this.OnSizeModeChanged(EventArgs.Empty);
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int TabIndex
        {
            get
            {
                return base.TabIndex;
            }
            set
            {
                base.TabIndex = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public bool TabStop
        {
            get
            {
                return base.TabStop;
            }
            set
            {
                base.TabStop = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Bindable(false)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAsynchronous"), System.Windows.Forms.SRDescription("PictureBoxWaitOnLoadDescr"), DefaultValue(false), Localizable(true)]
        public bool WaitOnLoad
        {
            get
            {
                return this.pictureBoxState[0x10];
            }
            set
            {
                this.pictureBoxState[0x10] = value;
            }
        }

        private enum ImageInstallationType
        {
            DirectlySpecified,
            ErrorOrInitial,
            FromUrl
        }
    }
}

