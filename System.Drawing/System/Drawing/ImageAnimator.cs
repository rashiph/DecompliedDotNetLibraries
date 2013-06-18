namespace System.Drawing
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.Threading;

    public sealed class ImageAnimator
    {
        private static Thread animationThread;
        private static bool anyFrameDirty;
        private static List<ImageInfo> imageInfoList;
        private static ReaderWriterLock rwImgListLock = new ReaderWriterLock();
        [ThreadStatic]
        private static int threadWriterLockWaitCount;

        private ImageAnimator()
        {
        }

        public static void Animate(Image image, EventHandler onFrameChangedHandler)
        {
            if (image != null)
            {
                ImageInfo item = null;
                lock (image)
                {
                    item = new ImageInfo(image);
                }
                StopAnimate(image, onFrameChangedHandler);
                bool isReaderLockHeld = rwImgListLock.IsReaderLockHeld;
                LockCookie lockCookie = new LockCookie();
                threadWriterLockWaitCount++;
                try
                {
                    if (isReaderLockHeld)
                    {
                        lockCookie = rwImgListLock.UpgradeToWriterLock(-1);
                    }
                    else
                    {
                        rwImgListLock.AcquireWriterLock(-1);
                    }
                }
                finally
                {
                    threadWriterLockWaitCount--;
                }
                try
                {
                    if (item.Animated)
                    {
                        if (imageInfoList == null)
                        {
                            imageInfoList = new List<ImageInfo>();
                        }
                        item.FrameChangedHandler = onFrameChangedHandler;
                        imageInfoList.Add(item);
                        if (animationThread == null)
                        {
                            animationThread = new Thread(new ThreadStart(ImageAnimator.AnimateImages50ms));
                            animationThread.Name = typeof(ImageAnimator).Name;
                            animationThread.IsBackground = true;
                            animationThread.Start();
                        }
                    }
                }
                finally
                {
                    if (isReaderLockHeld)
                    {
                        rwImgListLock.DowngradeFromWriterLock(ref lockCookie);
                    }
                    else
                    {
                        rwImgListLock.ReleaseWriterLock();
                    }
                }
            }
        }

        private static void AnimateImages50ms()
        {
            while (true)
            {
                rwImgListLock.AcquireReaderLock(-1);
                try
                {
                    for (int i = 0; i < imageInfoList.Count; i++)
                    {
                        ImageInfo info = imageInfoList[i];
                        info.FrameTimer += 5;
                        if (info.FrameTimer >= info.FrameDelay(info.Frame))
                        {
                            info.FrameTimer = 0;
                            if ((info.Frame + 1) < info.FrameCount)
                            {
                                info.Frame++;
                            }
                            else
                            {
                                info.Frame = 0;
                            }
                            if (info.FrameDirty)
                            {
                                anyFrameDirty = true;
                            }
                        }
                    }
                }
                finally
                {
                    rwImgListLock.ReleaseReaderLock();
                }
                Thread.Sleep(50);
            }
        }

        public static bool CanAnimate(Image image)
        {
            if (image != null)
            {
                lock (image)
                {
                    foreach (Guid guid in image.FrameDimensionsList)
                    {
                        FrameDimension dimension = new FrameDimension(guid);
                        if (dimension.Equals(FrameDimension.Time))
                        {
                            return (image.GetFrameCount(FrameDimension.Time) > 1);
                        }
                    }
                }
            }
            return false;
        }

        public static void StopAnimate(Image image, EventHandler onFrameChangedHandler)
        {
            if ((image != null) && (imageInfoList != null))
            {
                bool isReaderLockHeld = rwImgListLock.IsReaderLockHeld;
                LockCookie lockCookie = new LockCookie();
                threadWriterLockWaitCount++;
                try
                {
                    if (isReaderLockHeld)
                    {
                        lockCookie = rwImgListLock.UpgradeToWriterLock(-1);
                    }
                    else
                    {
                        rwImgListLock.AcquireWriterLock(-1);
                    }
                }
                finally
                {
                    threadWriterLockWaitCount--;
                }
                try
                {
                    for (int i = 0; i < imageInfoList.Count; i++)
                    {
                        ImageInfo item = imageInfoList[i];
                        if (image == item.Image)
                        {
                            if ((onFrameChangedHandler == item.FrameChangedHandler) || ((onFrameChangedHandler != null) && onFrameChangedHandler.Equals(item.FrameChangedHandler)))
                            {
                                imageInfoList.Remove(item);
                            }
                            return;
                        }
                    }
                }
                finally
                {
                    if (isReaderLockHeld)
                    {
                        rwImgListLock.DowngradeFromWriterLock(ref lockCookie);
                    }
                    else
                    {
                        rwImgListLock.ReleaseWriterLock();
                    }
                }
            }
        }

        public static void UpdateFrames()
        {
            if ((anyFrameDirty && (imageInfoList != null)) && (threadWriterLockWaitCount <= 0))
            {
                rwImgListLock.AcquireReaderLock(-1);
                try
                {
                    foreach (ImageInfo info in imageInfoList)
                    {
                        lock (info.Image)
                        {
                            info.UpdateFrame();
                        }
                    }
                    anyFrameDirty = false;
                }
                finally
                {
                    rwImgListLock.ReleaseReaderLock();
                }
            }
        }

        public static void UpdateFrames(Image image)
        {
            if (((anyFrameDirty && (image != null)) && (imageInfoList != null)) && (threadWriterLockWaitCount <= 0))
            {
                rwImgListLock.AcquireReaderLock(-1);
                try
                {
                    bool flag = false;
                    bool flag2 = false;
                    foreach (ImageInfo info in imageInfoList)
                    {
                        if (info.Image == image)
                        {
                            if (info.FrameDirty)
                            {
                                lock (info.Image)
                                {
                                    info.UpdateFrame();
                                }
                            }
                            flag2 = true;
                        }
                        if (info.FrameDirty)
                        {
                            flag = true;
                        }
                        if (flag && flag2)
                        {
                            break;
                        }
                    }
                    anyFrameDirty = flag;
                }
                finally
                {
                    rwImgListLock.ReleaseReaderLock();
                }
            }
        }

        private class ImageInfo
        {
            private bool animated;
            private int frame;
            private int frameCount;
            private int[] frameDelay;
            private bool frameDirty;
            private int frameTimer;
            private System.Drawing.Image image;
            private EventHandler onFrameChangedHandler;
            private const int PropertyTagFrameDelay = 0x5100;

            public ImageInfo(System.Drawing.Image image)
            {
                this.image = image;
                this.animated = ImageAnimator.CanAnimate(image);
                if (this.animated)
                {
                    this.frameCount = image.GetFrameCount(FrameDimension.Time);
                    PropertyItem propertyItem = image.GetPropertyItem(0x5100);
                    if (propertyItem != null)
                    {
                        byte[] buffer = propertyItem.Value;
                        this.frameDelay = new int[this.FrameCount];
                        for (int i = 0; i < this.FrameCount; i++)
                        {
                            this.frameDelay[i] = ((buffer[i * 4] + (0x100 * buffer[(i * 4) + 1])) + (0x10000 * buffer[(i * 4) + 2])) + (0x1000000 * buffer[(i * 4) + 3]);
                        }
                    }
                }
                else
                {
                    this.frameCount = 1;
                }
                if (this.frameDelay == null)
                {
                    this.frameDelay = new int[this.FrameCount];
                }
            }

            public int FrameDelay(int frame)
            {
                return this.frameDelay[frame];
            }

            protected void OnFrameChanged(EventArgs e)
            {
                if (this.onFrameChangedHandler != null)
                {
                    this.onFrameChangedHandler(this.image, e);
                }
            }

            internal void UpdateFrame()
            {
                if (this.frameDirty)
                {
                    this.image.SelectActiveFrame(FrameDimension.Time, this.Frame);
                    this.frameDirty = false;
                }
            }

            public bool Animated
            {
                get
                {
                    return this.animated;
                }
            }

            public int Frame
            {
                get
                {
                    return this.frame;
                }
                set
                {
                    if (this.frame != value)
                    {
                        if ((value < 0) || (value >= this.FrameCount))
                        {
                            throw new ArgumentException(System.Drawing.SR.GetString("InvalidFrame"), "value");
                        }
                        if (this.Animated)
                        {
                            this.frame = value;
                            this.frameDirty = true;
                            this.OnFrameChanged(EventArgs.Empty);
                        }
                    }
                }
            }

            public EventHandler FrameChangedHandler
            {
                get
                {
                    return this.onFrameChangedHandler;
                }
                set
                {
                    this.onFrameChangedHandler = value;
                }
            }

            public int FrameCount
            {
                get
                {
                    return this.frameCount;
                }
            }

            public bool FrameDirty
            {
                get
                {
                    return this.frameDirty;
                }
            }

            internal int FrameTimer
            {
                get
                {
                    return this.frameTimer;
                }
                set
                {
                    this.frameTimer = value;
                }
            }

            internal System.Drawing.Image Image
            {
                get
                {
                    return this.image;
                }
            }
        }
    }
}

