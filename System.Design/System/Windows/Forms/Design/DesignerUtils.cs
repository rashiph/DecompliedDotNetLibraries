namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal static class DesignerUtils
    {
        public static readonly ContentAlignment anyMiddleAlignment = (ContentAlignment.MiddleRight | ContentAlignment.MiddleCenter | ContentAlignment.MiddleLeft);
        public static readonly ContentAlignment anyTopAlignment = (ContentAlignment.TopRight | ContentAlignment.TopCenter | ContentAlignment.TopLeft);
        private static Bitmap boxImage = null;
        public static int BOXIMAGESIZE = 0x10;
        public static int CONTAINERGRABHANDLESIZE = 15;
        public static int DEFAULTCOLUMNCOUNT = 2;
        public static int DEFAULTFORMPADDING = 9;
        public static int DEFAULTROWCOUNT = 2;
        private static IntPtr grabHandleFillBrush = System.Design.SafeNativeMethods.CreateSolidBrush(ColorTranslator.ToWin32(SystemColors.ControlText));
        private static IntPtr grabHandleFillBrushPrimary = System.Design.SafeNativeMethods.CreateSolidBrush(ColorTranslator.ToWin32(SystemColors.Window));
        private static IntPtr grabHandlePen = System.Design.SafeNativeMethods.CreatePen(System.Design.NativeMethods.PS_SOLID, 1, ColorTranslator.ToWin32(SystemColors.Window));
        private static IntPtr grabHandlePenPrimary = System.Design.SafeNativeMethods.CreatePen(System.Design.NativeMethods.PS_SOLID, 1, ColorTranslator.ToWin32(SystemColors.ControlText));
        public static int HANDLEOVERLAP = 2;
        public static int HANDLESIZE = 7;
        private static SolidBrush hoverBrush = new SolidBrush(Color.FromArgb(50, SystemColors.Highlight));
        public static int LOCKEDSELECTIONBORDEROFFSET_X = (((LOCKHANDLEWIDTH - SELECTIONBORDERSIZE) / 2) - LOCKHANDLEOVERLAP);
        public static int LOCKEDSELECTIONBORDEROFFSET_Y = (((LOCKHANDLEHEIGHT - SELECTIONBORDERSIZE) / 2) - LOCKHANDLEOVERLAP);
        public static int LOCKHANDLEHEIGHT = 9;
        public static int LOCKHANDLEHEIGHT_LOWER = 6;
        public static int LOCKHANDLELOWER_OFFSET = (LOCKHANDLEHEIGHT - LOCKHANDLEHEIGHT_LOWER);
        public static int LOCKHANDLEOVERLAP = 2;
        public static int LOCKHANDLESIZE_UPPER = 5;
        public static int LOCKHANDLEUPPER_OFFSET = ((LOCKHANDLEWIDTH_LOWER - LOCKHANDLESIZE_UPPER) / 2);
        public static int LOCKHANDLEWIDTH = 7;
        public static int LOCKHANDLEWIDTH_LOWER = 7;
        public static int MINCONTROLBITMAPSIZE = 1;
        private static Size minDragSize = Size.Empty;
        public static int MINIMUMSTYLEPERCENT = 50;
        public static int MINIMUMSTYLESIZE = 20;
        public static int MINUMUMSTYLESIZEDRAG = 8;
        public static int NORESIZEBORDEROFFSET = ((NORESIZEHANDLESIZE - SELECTIONBORDERSIZE) / 2);
        public static int NORESIZEHANDLESIZE = 5;
        public static int RESIZEGLYPHSIZE = 4;
        private static HatchBrush selectionBorderBrush = new HatchBrush(HatchStyle.Percent50, SystemColors.ControlDarkDark, Color.Transparent);
        public static int SELECTIONBORDERHITAREA = 3;
        public static int SELECTIONBORDEROFFSET = (((HANDLESIZE - SELECTIONBORDERSIZE) / 2) - HANDLEOVERLAP);
        public static int SELECTIONBORDERSIZE = 1;
        public static int SNAPELINEDELAY = 0x3e8;

        public static void ApplyListViewThemeStyles(ListView listView)
        {
            if (listView == null)
            {
                throw new ArgumentNullException("listView");
            }
            IntPtr handle = listView.Handle;
            System.Design.SafeNativeMethods.SetWindowTheme(handle, "Explorer", null);
            ListView_SetExtendedListViewStyleEx(handle, 0x10000, 0x10000);
        }

        public static void ApplyTreeViewThemeStyles(TreeView treeView)
        {
            if (treeView == null)
            {
                throw new ArgumentNullException("treeView");
            }
            treeView.HotTracking = true;
            treeView.ShowLines = false;
            IntPtr handle = treeView.Handle;
            System.Design.SafeNativeMethods.SetWindowTheme(handle, "Explorer", null);
            int extendedStyle = TreeView_GetExtendedStyle(handle) | 0x44;
            TreeView_SetExtendedStyle(handle, extendedStyle, 0);
        }

        public static IContainer CheckForNestedContainer(IContainer container)
        {
            NestedContainer container2 = container as NestedContainer;
            if (container2 != null)
            {
                return container2.Owner.Site.Container;
            }
            return container;
        }

        public static ICollection CopyDragObjects(ICollection objects, IServiceProvider svcProvider)
        {
            if ((objects != null) && (svcProvider != null))
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ComponentSerializationService service = svcProvider.GetService(typeof(ComponentSerializationService)) as ComponentSerializationService;
                    IDesignerHost host = svcProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if ((service != null) && (host != null))
                    {
                        SerializationStore store = null;
                        store = service.CreateStore();
                        foreach (IComponent component in GetCopySelection(objects, host))
                        {
                            service.Serialize(store, component);
                        }
                        store.Close();
                        ICollection is2 = service.Deserialize(store);
                        ArrayList list = new ArrayList(objects.Count);
                        foreach (IComponent component2 in is2)
                        {
                            Control control = component2 as Control;
                            if ((control != null) && (control.Parent == null))
                            {
                                list.Add(component2);
                            }
                            else if (control == null)
                            {
                                ToolStripItem item = component2 as ToolStripItem;
                                if ((item != null) && (item.GetCurrentParent() == null))
                                {
                                    list.Add(component2);
                                }
                            }
                        }
                        return list;
                    }
                }
                finally
                {
                    Cursor.Current = current;
                }
            }
            return null;
        }

        private static void DrawDragBorder(Graphics g, Size imageSize, int borderSize, Color backColor)
        {
            Pen controlDarkDark = SystemPens.ControlDarkDark;
            if ((backColor != Color.Empty) && (backColor.GetBrightness() < 0.5))
            {
                controlDarkDark = SystemPens.ControlLight;
            }
            g.DrawLine(controlDarkDark, 1, 0, imageSize.Width - 2, 0);
            g.DrawLine(controlDarkDark, 1, imageSize.Height - 1, imageSize.Width - 2, imageSize.Height - 1);
            g.DrawLine(controlDarkDark, 0, 1, 0, imageSize.Height - 2);
            g.DrawLine(controlDarkDark, imageSize.Width - 1, 1, imageSize.Width - 1, imageSize.Height - 2);
            for (int i = 1; i < borderSize; i++)
            {
                g.DrawRectangle(controlDarkDark, i, i, imageSize.Width - (2 + i), imageSize.Height - (2 + i));
            }
        }

        public static void DrawFrame(Graphics g, Region resizeBorder, FrameStyle style, Color backColor)
        {
            Brush brush;
            Color controlDarkDark = SystemColors.ControlDarkDark;
            if ((backColor != Color.Empty) && (backColor.GetBrightness() < 0.5))
            {
                controlDarkDark = SystemColors.ControlLight;
            }
            switch (style)
            {
                case FrameStyle.Dashed:
                    brush = new HatchBrush(HatchStyle.Percent50, controlDarkDark, Color.Transparent);
                    break;

                default:
                    brush = new SolidBrush(controlDarkDark);
                    break;
            }
            g.FillRegion(brush, resizeBorder);
            brush.Dispose();
        }

        public static void DrawGrabHandle(Graphics graphics, Rectangle bounds, bool isPrimary, Glyph glyph)
        {
            IntPtr hdc = graphics.GetHdc();
            try
            {
                IntPtr handle = System.Design.SafeNativeMethods.SelectObject(new HandleRef(glyph, hdc), new HandleRef(glyph, isPrimary ? grabHandleFillBrushPrimary : grabHandleFillBrush));
                IntPtr ptr3 = System.Design.SafeNativeMethods.SelectObject(new HandleRef(glyph, hdc), new HandleRef(glyph, isPrimary ? grabHandlePenPrimary : grabHandlePen));
                System.Design.SafeNativeMethods.RoundRect(new HandleRef(glyph, hdc), bounds.Left, bounds.Top, bounds.Right, bounds.Bottom, 2, 2);
                System.Design.SafeNativeMethods.SelectObject(new HandleRef(glyph, hdc), new HandleRef(glyph, handle));
                System.Design.SafeNativeMethods.SelectObject(new HandleRef(glyph, hdc), new HandleRef(glyph, ptr3));
            }
            finally
            {
                graphics.ReleaseHdcInternal(hdc);
            }
        }

        public static void DrawLockedHandle(Graphics graphics, Rectangle bounds, bool isPrimary, Glyph glyph)
        {
            IntPtr hdc = graphics.GetHdc();
            try
            {
                IntPtr handle = System.Design.SafeNativeMethods.SelectObject(new HandleRef(glyph, hdc), new HandleRef(glyph, grabHandlePenPrimary));
                IntPtr ptr3 = System.Design.SafeNativeMethods.SelectObject(new HandleRef(glyph, hdc), new HandleRef(glyph, grabHandleFillBrushPrimary));
                System.Design.SafeNativeMethods.RoundRect(new HandleRef(glyph, hdc), bounds.Left + LOCKHANDLEUPPER_OFFSET, bounds.Top, (bounds.Left + LOCKHANDLEUPPER_OFFSET) + LOCKHANDLESIZE_UPPER, bounds.Top + LOCKHANDLESIZE_UPPER, 2, 2);
                System.Design.SafeNativeMethods.SelectObject(new HandleRef(glyph, hdc), new HandleRef(glyph, isPrimary ? grabHandleFillBrushPrimary : grabHandleFillBrush));
                System.Design.SafeNativeMethods.Rectangle(new HandleRef(glyph, hdc), bounds.Left, bounds.Top + LOCKHANDLELOWER_OFFSET, bounds.Right, bounds.Bottom);
                System.Design.SafeNativeMethods.SelectObject(new HandleRef(glyph, hdc), new HandleRef(glyph, ptr3));
                System.Design.SafeNativeMethods.SelectObject(new HandleRef(glyph, hdc), new HandleRef(glyph, handle));
            }
            finally
            {
                graphics.ReleaseHdcInternal(hdc);
            }
        }

        public static void DrawNoResizeHandle(Graphics graphics, Rectangle bounds, bool isPrimary, Glyph glyph)
        {
            IntPtr hdc = graphics.GetHdc();
            try
            {
                IntPtr handle = System.Design.SafeNativeMethods.SelectObject(new HandleRef(glyph, hdc), new HandleRef(glyph, isPrimary ? grabHandleFillBrushPrimary : grabHandleFillBrush));
                IntPtr ptr3 = System.Design.SafeNativeMethods.SelectObject(new HandleRef(glyph, hdc), new HandleRef(glyph, grabHandlePenPrimary));
                System.Design.SafeNativeMethods.Rectangle(new HandleRef(glyph, hdc), bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
                System.Design.SafeNativeMethods.SelectObject(new HandleRef(glyph, hdc), new HandleRef(glyph, handle));
                System.Design.SafeNativeMethods.SelectObject(new HandleRef(glyph, hdc), new HandleRef(glyph, ptr3));
            }
            finally
            {
                graphics.ReleaseHdcInternal(hdc);
            }
        }

        public static void DrawResizeBorder(Graphics g, Region resizeBorder, Color backColor)
        {
            Brush controlDarkDark = SystemBrushes.ControlDarkDark;
            if ((backColor != Color.Empty) && (backColor.GetBrightness() < 0.5))
            {
                controlDarkDark = SystemBrushes.ControlLight;
            }
            g.FillRegion(controlDarkDark, resizeBorder);
        }

        public static void DrawSelectionBorder(Graphics graphics, Rectangle bounds)
        {
            graphics.FillRectangle(selectionBorderBrush, bounds);
        }

        public static ICollection FilterGenericTypes(ICollection types)
        {
            if ((types == null) || (types.Count == 0))
            {
                return types;
            }
            ArrayList list = new ArrayList(types.Count);
            foreach (System.Type type in types)
            {
                if (!type.ContainsGenericParameters)
                {
                    list.Add(type);
                }
            }
            return list;
        }

        public static void GenerateSnapShot(Control control, ref Image image, int borderSize, double opacity, Color backColor)
        {
            if (!GenerateSnapShotWithWM_PRINT(control, ref image))
            {
                GenerateSnapShotWithBitBlt(control, ref image);
            }
            if ((opacity < 1.0) && (opacity > 0.0))
            {
                SetImageAlpha((Bitmap) image, opacity);
            }
            if (borderSize > 0)
            {
                using (Graphics graphics = Graphics.FromImage(image))
                {
                    DrawDragBorder(graphics, image.Size, borderSize, backColor);
                }
            }
        }

        public static void GenerateSnapShotWithBitBlt(Control control, ref Image image)
        {
            HandleRef hWnd = new HandleRef(control, control.Handle);
            IntPtr dC = System.Design.UnsafeNativeMethods.GetDC(hWnd);
            image = new Bitmap(Math.Max(control.Width, MINCONTROLBITMAPSIZE), Math.Max(control.Height, MINCONTROLBITMAPSIZE), PixelFormat.Format32bppPArgb);
            using (Graphics graphics = Graphics.FromImage(image))
            {
                if (control.BackColor == Color.Transparent)
                {
                    graphics.Clear(SystemColors.Control);
                }
                IntPtr hdc = graphics.GetHdc();
                System.Design.SafeNativeMethods.BitBlt(hdc, 0, 0, image.Width, image.Height, dC, 0, 0, 0xcc0020);
                graphics.ReleaseHdc(hdc);
            }
        }

        public static bool GenerateSnapShotWithWM_PRINT(Control control, ref Image image)
        {
            IntPtr handle = control.Handle;
            image = new Bitmap(Math.Max(control.Width, MINCONTROLBITMAPSIZE), Math.Max(control.Height, MINCONTROLBITMAPSIZE), PixelFormat.Format32bppPArgb);
            if (control.BackColor == Color.Transparent)
            {
                using (Graphics graphics = Graphics.FromImage(image))
                {
                    graphics.Clear(SystemColors.Control);
                }
            }
            Color color = Color.FromArgb(0xff, 0xfc, 0xba, 0xee);
            ((Bitmap) image).SetPixel(image.Width / 2, image.Height / 2, color);
            using (Graphics graphics2 = Graphics.FromImage(image))
            {
                IntPtr hdc = graphics2.GetHdc();
                System.Design.NativeMethods.SendMessage(handle, 0x317, hdc, (IntPtr) 30);
                graphics2.ReleaseHdc(hdc);
            }
            if (((Bitmap) image).GetPixel(image.Width / 2, image.Height / 2).Equals(color))
            {
                return false;
            }
            return true;
        }

        public static Size GetAdornmentDimensions(AdornmentType adornmentType)
        {
            switch (adornmentType)
            {
                case AdornmentType.GrabHandle:
                    return new Size(HANDLESIZE, HANDLESIZE);

                case AdornmentType.ContainerSelector:
                case AdornmentType.Maximum:
                    return new Size(CONTAINERGRABHANDLESIZE, CONTAINERGRABHANDLESIZE);
            }
            return new Size(0, 0);
        }

        internal static void GetAssociatedComponents(IComponent component, IDesignerHost host, ArrayList list)
        {
            if (host != null)
            {
                ComponentDesigner designer = host.GetDesigner(component) as ComponentDesigner;
                if (designer != null)
                {
                    foreach (IComponent component2 in designer.AssociatedComponents)
                    {
                        if (component2.Site != null)
                        {
                            list.Add(component2);
                            GetAssociatedComponents(component2, host, list);
                        }
                    }
                }
            }
        }

        public static Rectangle GetBoundsForNoResizeSelectionType(Rectangle originalBounds, SelectionBorderGlyphType type)
        {
            return GetBoundsForSelectionType(originalBounds, type, SELECTIONBORDERSIZE, NORESIZEBORDEROFFSET);
        }

        public static Rectangle GetBoundsForSelectionType(Rectangle originalBounds, SelectionBorderGlyphType type)
        {
            return GetBoundsForSelectionType(originalBounds, type, SELECTIONBORDERSIZE, SELECTIONBORDEROFFSET);
        }

        public static Rectangle GetBoundsForSelectionType(Rectangle originalBounds, SelectionBorderGlyphType type, int borderSize)
        {
            Rectangle empty = Rectangle.Empty;
            switch (type)
            {
                case SelectionBorderGlyphType.Top:
                    return new Rectangle(originalBounds.Left - borderSize, originalBounds.Top - borderSize, originalBounds.Width + (2 * borderSize), borderSize);

                case SelectionBorderGlyphType.Bottom:
                    return new Rectangle(originalBounds.Left - borderSize, originalBounds.Bottom, originalBounds.Width + (2 * borderSize), borderSize);

                case SelectionBorderGlyphType.Left:
                    return new Rectangle(originalBounds.Left - borderSize, originalBounds.Top - borderSize, borderSize, originalBounds.Height + (2 * borderSize));

                case SelectionBorderGlyphType.Right:
                    return new Rectangle(originalBounds.Right, originalBounds.Top - borderSize, borderSize, originalBounds.Height + (2 * borderSize));

                case SelectionBorderGlyphType.Body:
                    return originalBounds;
            }
            return empty;
        }

        private static Rectangle GetBoundsForSelectionType(Rectangle originalBounds, SelectionBorderGlyphType type, int bordersize, int offset)
        {
            Rectangle rectangle = GetBoundsForSelectionType(originalBounds, type, bordersize);
            if (offset != 0)
            {
                switch (type)
                {
                    case SelectionBorderGlyphType.Top:
                        rectangle.Offset(-offset, -offset);
                        rectangle.Width += 2 * offset;
                        return rectangle;

                    case SelectionBorderGlyphType.Bottom:
                        rectangle.Offset(-offset, offset);
                        rectangle.Width += 2 * offset;
                        return rectangle;

                    case SelectionBorderGlyphType.Left:
                        rectangle.Offset(-offset, -offset);
                        rectangle.Height += 2 * offset;
                        return rectangle;

                    case SelectionBorderGlyphType.Right:
                        rectangle.Offset(offset, -offset);
                        rectangle.Height += 2 * offset;
                        return rectangle;

                    case SelectionBorderGlyphType.Body:
                        return originalBounds;
                }
            }
            return rectangle;
        }

        public static Rectangle GetBoundsFromToolboxSnapDragDropInfo(ToolboxSnapDragDropEventArgs e, Rectangle originalBounds, bool isMirrored)
        {
            Rectangle rectangle = originalBounds;
            if (e.Offset != Point.Empty)
            {
                if ((e.SnapDirections & ToolboxSnapDragDropEventArgs.SnapDirection.Top) != ToolboxSnapDragDropEventArgs.SnapDirection.None)
                {
                    rectangle.Y += e.Offset.Y;
                }
                else if ((e.SnapDirections & ToolboxSnapDragDropEventArgs.SnapDirection.Bottom) != ToolboxSnapDragDropEventArgs.SnapDirection.None)
                {
                    rectangle.Y = (originalBounds.Y - originalBounds.Height) + e.Offset.Y;
                }
                if (!isMirrored)
                {
                    if ((e.SnapDirections & ToolboxSnapDragDropEventArgs.SnapDirection.Left) != ToolboxSnapDragDropEventArgs.SnapDirection.None)
                    {
                        rectangle.X += e.Offset.X;
                        return rectangle;
                    }
                    if ((e.SnapDirections & ToolboxSnapDragDropEventArgs.SnapDirection.Right) != ToolboxSnapDragDropEventArgs.SnapDirection.None)
                    {
                        rectangle.X = (originalBounds.X - originalBounds.Width) + e.Offset.X;
                    }
                    return rectangle;
                }
                if ((e.SnapDirections & ToolboxSnapDragDropEventArgs.SnapDirection.Left) != ToolboxSnapDragDropEventArgs.SnapDirection.None)
                {
                    rectangle.X = (originalBounds.X - originalBounds.Width) - e.Offset.X;
                    return rectangle;
                }
                if ((e.SnapDirections & ToolboxSnapDragDropEventArgs.SnapDirection.Right) != ToolboxSnapDragDropEventArgs.SnapDirection.None)
                {
                    rectangle.X -= e.Offset.X;
                }
            }
            return rectangle;
        }

        private static ICollection GetCopySelection(ICollection objects, IDesignerHost host)
        {
            if ((objects == null) || (host == null))
            {
                return null;
            }
            ArrayList list = new ArrayList();
            foreach (IComponent component in objects)
            {
                list.Add(component);
                GetAssociatedComponents(component, host, list);
            }
            return list;
        }

        public static object GetOptionValue(IServiceProvider provider, string name)
        {
            object optionValue = null;
            if (provider != null)
            {
                DesignerOptionService service = provider.GetService(typeof(DesignerOptionService)) as DesignerOptionService;
                if (service != null)
                {
                    PropertyDescriptor descriptor = service.Options.Properties[name];
                    if (descriptor != null)
                    {
                        optionValue = descriptor.GetValue(null);
                    }
                    return optionValue;
                }
                IDesignerOptionService service2 = provider.GetService(typeof(IDesignerOptionService)) as IDesignerOptionService;
                if (service2 != null)
                {
                    optionValue = service2.GetOptionValue(@"WindowsFormsDesigner\General", name);
                }
            }
            return optionValue;
        }

        public static int GetTextBaseline(Control ctrl, ContentAlignment alignment)
        {
            Rectangle clientRectangle = ctrl.ClientRectangle;
            int num = 0;
            int tmHeight = 0;
            using (Graphics graphics = ctrl.CreateGraphics())
            {
                IntPtr hdc = graphics.GetHdc();
                IntPtr handle = ctrl.Font.ToHfont();
                try
                {
                    IntPtr ptr3 = System.Design.SafeNativeMethods.SelectObject(new HandleRef(ctrl, hdc), new HandleRef(ctrl, handle));
                    System.Design.NativeMethods.TEXTMETRIC tm = new System.Design.NativeMethods.TEXTMETRIC();
                    System.Design.SafeNativeMethods.GetTextMetrics(new HandleRef(ctrl, hdc), tm);
                    num = tm.tmAscent + 1;
                    tmHeight = tm.tmHeight;
                    System.Design.SafeNativeMethods.SelectObject(new HandleRef(ctrl, hdc), new HandleRef(ctrl, ptr3));
                }
                finally
                {
                    System.Design.SafeNativeMethods.DeleteObject(new HandleRef(ctrl.Font, handle));
                    graphics.ReleaseHdc(hdc);
                }
            }
            if ((alignment & anyTopAlignment) != ((ContentAlignment) 0))
            {
                return (clientRectangle.Top + num);
            }
            if ((alignment & anyMiddleAlignment) != ((ContentAlignment) 0))
            {
                return (((clientRectangle.Top + (clientRectangle.Height / 2)) - (tmHeight / 2)) + num);
            }
            return ((clientRectangle.Bottom - tmHeight) + num);
        }

        public static string GetUniqueSiteName(IDesignerHost host, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            INameCreationService service = (INameCreationService) host.GetService(typeof(INameCreationService));
            if (service == null)
            {
                return null;
            }
            if (host.Container.Components[name] == null)
            {
                if (!service.IsValidName(name))
                {
                    return null;
                }
                return name;
            }
            string str = name;
            for (int i = 1; !service.IsValidName(str); i++)
            {
                str = name + i.ToString(CultureInfo.InvariantCulture);
            }
            return str;
        }

        private static void ListView_SetExtendedListViewStyleEx(IntPtr handle, int mask, int extendedStyle)
        {
            System.Design.NativeMethods.SendMessage(handle, 0x1036, new IntPtr(mask), new IntPtr(extendedStyle));
        }

        private static unsafe void SetImageAlpha(Bitmap b, double opacity)
        {
            if (opacity != 1.0)
            {
                byte[] buffer = new byte[0x100];
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = (byte) (i * opacity);
                }
                BitmapData bitmapdata = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                try
                {
                    int num2 = bitmapdata.Height * bitmapdata.Width;
                    int* numPtr = (int*) bitmapdata.Scan0;
                    byte* numPtr2 = (byte*) (numPtr + num2);
                    for (byte* numPtr3 = (byte*) (numPtr + 3); numPtr3 < numPtr2; numPtr3 += 4)
                    {
                        numPtr3[0] = buffer[numPtr3[0]];
                    }
                }
                finally
                {
                    b.UnlockBits(bitmapdata);
                }
            }
        }

        public static void SyncBrushes()
        {
            hoverBrush.Dispose();
            hoverBrush = new SolidBrush(Color.FromArgb(50, SystemColors.Highlight));
            selectionBorderBrush.Dispose();
            selectionBorderBrush = new HatchBrush(HatchStyle.Percent50, SystemColors.ControlDarkDark, Color.Transparent);
            System.Design.SafeNativeMethods.DeleteObject(new HandleRef(null, grabHandleFillBrushPrimary));
            grabHandleFillBrushPrimary = System.Design.SafeNativeMethods.CreateSolidBrush(ColorTranslator.ToWin32(SystemColors.Window));
            System.Design.SafeNativeMethods.DeleteObject(new HandleRef(null, grabHandleFillBrush));
            grabHandleFillBrush = System.Design.SafeNativeMethods.CreateSolidBrush(ColorTranslator.ToWin32(SystemColors.ControlText));
            System.Design.SafeNativeMethods.DeleteObject(new HandleRef(null, grabHandlePenPrimary));
            grabHandlePenPrimary = System.Design.SafeNativeMethods.CreatePen(System.Design.NativeMethods.PS_SOLID, 1, ColorTranslator.ToWin32(SystemColors.ControlText));
            System.Design.SafeNativeMethods.DeleteObject(new HandleRef(null, grabHandlePen));
            grabHandlePen = System.Design.SafeNativeMethods.CreatePen(System.Design.NativeMethods.PS_SOLID, 1, ColorTranslator.ToWin32(SystemColors.Window));
        }

        private static int TreeView_GetExtendedStyle(IntPtr handle)
        {
            return System.Design.NativeMethods.SendMessage(handle, 0x112d, IntPtr.Zero, IntPtr.Zero).ToInt32();
        }

        private static void TreeView_SetExtendedStyle(IntPtr handle, int extendedStyle, int mask)
        {
            System.Design.NativeMethods.SendMessage(handle, 0x112c, new IntPtr(mask), new IntPtr(extendedStyle));
        }

        public static bool UseSnapLines(IServiceProvider provider)
        {
            bool flag = true;
            object obj2 = null;
            DesignerOptionService service = provider.GetService(typeof(DesignerOptionService)) as DesignerOptionService;
            if (service != null)
            {
                PropertyDescriptor descriptor = service.Options.Properties["UseSnapLines"];
                if (descriptor != null)
                {
                    obj2 = descriptor.GetValue(null);
                }
            }
            if ((obj2 != null) && (obj2 is bool))
            {
                flag = (bool) obj2;
            }
            return flag;
        }

        public static Image BoxImage
        {
            get
            {
                if (boxImage == null)
                {
                    boxImage = new Bitmap(BOXIMAGESIZE, BOXIMAGESIZE, PixelFormat.Format32bppPArgb);
                    using (Graphics graphics = Graphics.FromImage(boxImage))
                    {
                        graphics.FillRectangle(new SolidBrush(SystemColors.InactiveBorder), 0, 0, BOXIMAGESIZE, BOXIMAGESIZE);
                        graphics.DrawRectangle(new Pen(SystemColors.ControlDarkDark), 0, 0, BOXIMAGESIZE - 1, BOXIMAGESIZE - 1);
                    }
                }
                return boxImage;
            }
        }

        public static Brush HoverBrush
        {
            get
            {
                return hoverBrush;
            }
        }

        public static Point LastCursorPoint
        {
            get
            {
                int messagePos = System.Design.SafeNativeMethods.GetMessagePos();
                return new Point(System.Design.NativeMethods.Util.SignedLOWORD(messagePos), System.Design.NativeMethods.Util.SignedHIWORD(messagePos));
            }
        }

        public static Size MinDragSize
        {
            get
            {
                if (minDragSize == Size.Empty)
                {
                    Size dragSize = SystemInformation.DragSize;
                    Size doubleClickSize = SystemInformation.DoubleClickSize;
                    minDragSize.Width = Math.Max(dragSize.Width, doubleClickSize.Width);
                    minDragSize.Height = Math.Max(dragSize.Height, doubleClickSize.Height);
                }
                return minDragSize;
            }
        }
    }
}

