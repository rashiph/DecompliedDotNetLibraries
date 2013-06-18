namespace System.Windows.Forms.Design
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal sealed class ToolStripDesignerUtils
    {
        [ThreadStatic]
        private static Dictionary<System.Type, ToolboxItem> CachedToolboxItems;
        [ThreadStatic]
        private static Dictionary<System.Type, Bitmap> CachedWinformsImages;
        [ThreadStatic]
        private static int CustomToolStripItemCount = 0;
        private static readonly System.Type[] NewItemTypesForMenuStrip = new System.Type[] { typeof(ToolStripMenuItem), typeof(ToolStripComboBox), typeof(ToolStripTextBox) };
        private static readonly System.Type[] NewItemTypesForStatusStrip = new System.Type[] { typeof(ToolStripStatusLabel), typeof(ToolStripProgressBar), typeof(ToolStripDropDownButton), typeof(ToolStripSplitButton) };
        private static readonly System.Type[] NewItemTypesForToolStrip = new System.Type[] { typeof(ToolStripButton), typeof(ToolStripLabel), typeof(ToolStripSplitButton), typeof(ToolStripDropDownButton), typeof(ToolStripSeparator), typeof(ToolStripComboBox), typeof(ToolStripTextBox), typeof(ToolStripProgressBar) };
        private static readonly System.Type[] NewItemTypesForToolStripDropDownMenu = new System.Type[] { typeof(ToolStripMenuItem), typeof(ToolStripComboBox), typeof(ToolStripSeparator), typeof(ToolStripTextBox) };
        public static ArrayList originalSelComps;
        private static string systemWindowsFormsNamespace = typeof(ToolStripItem).Namespace;
        private const int TOOLSTRIPCHARCOUNT = 9;
        private static System.Type toolStripItemType = typeof(ToolStripItem);

        private ToolStripDesignerUtils()
        {
        }

        public static void GetAdjustedBounds(ToolStripItem item, ref Rectangle r)
        {
            if (!(item is ToolStripControlHost) || !item.IsOnDropDown)
            {
                if ((item is ToolStripMenuItem) && item.IsOnDropDown)
                {
                    r.Inflate(-3, -2);
                    r.Width++;
                }
                else if ((item is ToolStripControlHost) && !item.IsOnDropDown)
                {
                    r.Inflate(0, -2);
                }
                else if ((item is ToolStripMenuItem) && !item.IsOnDropDown)
                {
                    r.Inflate(-3, -3);
                }
                else
                {
                    r.Inflate(-1, -1);
                }
            }
        }

        private static ToolboxItem GetCachedToolboxItem(System.Type itemType)
        {
            ToolboxItem toolboxItem = null;
            if (CachedToolboxItems == null)
            {
                CachedToolboxItems = new Dictionary<System.Type, ToolboxItem>();
            }
            else if (CachedToolboxItems.ContainsKey(itemType))
            {
                return CachedToolboxItems[itemType];
            }
            if (toolboxItem == null)
            {
                toolboxItem = ToolboxService.GetToolboxItem(itemType);
                if (toolboxItem == null)
                {
                    toolboxItem = new ToolboxItem(itemType);
                }
            }
            CachedToolboxItems[itemType] = toolboxItem;
            if ((CustomToolStripItemCount > 0) && ((CustomToolStripItemCount * 2) < CachedToolboxItems.Count))
            {
                CachedToolboxItems.Clear();
            }
            return toolboxItem;
        }

        public static ToolStripItem[] GetCustomItemMenuItems(IComponent component, EventHandler onClick, bool convertTo, IServiceProvider serviceProvider)
        {
            System.Type[] customItemTypes = GetCustomItemTypes(component, serviceProvider);
            ToolStripItem[] itemArray = new ToolStripItem[customItemTypes.Length];
            for (int i = 0; i < customItemTypes.Length; i++)
            {
                ItemTypeToolStripMenuItem item = new ItemTypeToolStripMenuItem(customItemTypes[i]) {
                    ConvertTo = convertTo
                };
                if (onClick != null)
                {
                    item.Click += onClick;
                }
                itemArray[i] = item;
            }
            return itemArray;
        }

        public static System.Type[] GetCustomItemTypes(IComponent component, ITypeDiscoveryService discoveryService)
        {
            if (discoveryService != null)
            {
                ICollection types = discoveryService.GetTypes(toolStripItemType, false);
                ToolStripItemDesignerAvailability designerVisibility = GetDesignerVisibility(GetToolStripFromComponent(component));
                System.Type[] standardItemTypes = GetStandardItemTypes(component);
                if (designerVisibility != ToolStripItemDesignerAvailability.None)
                {
                    ArrayList list = new ArrayList(types.Count);
                    foreach (System.Type type in types)
                    {
                        if ((type.IsAbstract || (!type.IsPublic && !type.IsNestedPublic)) || (type.ContainsGenericParameters || (type.GetConstructor(new System.Type[0]) == null)))
                        {
                            continue;
                        }
                        ToolStripItemDesignerAvailabilityAttribute attribute = (ToolStripItemDesignerAvailabilityAttribute) TypeDescriptor.GetAttributes(type)[typeof(ToolStripItemDesignerAvailabilityAttribute)];
                        if ((attribute != null) && ((attribute.ItemAdditionVisibility & designerVisibility) == designerVisibility))
                        {
                            bool flag = false;
                            foreach (System.Type type2 in standardItemTypes)
                            {
                                if (type2 == type)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                list.Add(type);
                            }
                        }
                    }
                    if (list.Count > 0)
                    {
                        System.Type[] array = new System.Type[list.Count];
                        list.CopyTo(array, 0);
                        CustomToolStripItemCount = list.Count;
                        return array;
                    }
                }
            }
            CustomToolStripItemCount = 0;
            return new System.Type[0];
        }

        public static System.Type[] GetCustomItemTypes(IComponent component, IServiceProvider serviceProvider)
        {
            ITypeDiscoveryService discoveryService = null;
            if (serviceProvider != null)
            {
                discoveryService = serviceProvider.GetService(typeof(ITypeDiscoveryService)) as ITypeDiscoveryService;
            }
            return GetCustomItemTypes(component, discoveryService);
        }

        private static ToolStripItemDesignerAvailability GetDesignerVisibility(ToolStrip toolStrip)
        {
            if (toolStrip is StatusStrip)
            {
                return ToolStripItemDesignerAvailability.StatusStrip;
            }
            if (toolStrip is MenuStrip)
            {
                return ToolStripItemDesignerAvailability.MenuStrip;
            }
            if (toolStrip is ToolStripDropDownMenu)
            {
                return ToolStripItemDesignerAvailability.ContextMenuStrip;
            }
            return ToolStripItemDesignerAvailability.ToolStrip;
        }

        private static Bitmap GetKnownToolboxBitmap(System.Type itemType)
        {
            if (CachedWinformsImages == null)
            {
                CachedWinformsImages = new Dictionary<System.Type, Bitmap>();
            }
            if (!CachedWinformsImages.ContainsKey(itemType))
            {
                Bitmap bitmap = ToolboxBitmapAttribute.GetImageFromResource(itemType, null, false) as Bitmap;
                CachedWinformsImages[itemType] = bitmap;
                return bitmap;
            }
            return CachedWinformsImages[itemType];
        }

        public static ToolStripDropDown GetNewItemDropDown(IComponent component, ToolStripItem currentItem, EventHandler onClick, bool convertTo, IServiceProvider serviceProvider)
        {
            NewItemsContextMenuStrip strip = new NewItemsContextMenuStrip(component, currentItem, onClick, convertTo, serviceProvider);
            strip.GroupOrdering.Add("StandardList");
            strip.GroupOrdering.Add("CustomList");
            foreach (ToolStripItem item in GetStandardItemMenuItems(component, onClick, convertTo))
            {
                strip.Groups["StandardList"].Items.Add(item);
                if (convertTo)
                {
                    ItemTypeToolStripMenuItem item2 = item as ItemTypeToolStripMenuItem;
                    if (((item2 != null) && (currentItem != null)) && (item2.ItemType == currentItem.GetType()))
                    {
                        item2.Enabled = false;
                    }
                }
            }
            foreach (ToolStripItem item3 in GetCustomItemMenuItems(component, onClick, convertTo, serviceProvider))
            {
                strip.Groups["CustomList"].Items.Add(item3);
                if (convertTo)
                {
                    ItemTypeToolStripMenuItem item4 = item3 as ItemTypeToolStripMenuItem;
                    if (((item4 != null) && (currentItem != null)) && (item4.ItemType == currentItem.GetType()))
                    {
                        item4.Enabled = false;
                    }
                }
            }
            IUIService service = serviceProvider.GetService(typeof(IUIService)) as IUIService;
            if (service != null)
            {
                strip.Renderer = (ToolStripProfessionalRenderer) service.Styles["VsRenderer"];
                strip.Font = (Font) service.Styles["DialogFont"];
            }
            strip.Populate();
            return strip;
        }

        public static ToolStripItem[] GetStandardItemMenuItems(IComponent component, EventHandler onClick, bool convertTo)
        {
            System.Type[] standardItemTypes = GetStandardItemTypes(component);
            ToolStripItem[] itemArray = new ToolStripItem[standardItemTypes.Length];
            for (int i = 0; i < standardItemTypes.Length; i++)
            {
                ItemTypeToolStripMenuItem item = new ItemTypeToolStripMenuItem(standardItemTypes[i]) {
                    ConvertTo = convertTo
                };
                if (onClick != null)
                {
                    item.Click += onClick;
                }
                itemArray[i] = item;
            }
            return itemArray;
        }

        public static System.Type[] GetStandardItemTypes(IComponent component)
        {
            ToolStrip toolStripFromComponent = GetToolStripFromComponent(component);
            if (toolStripFromComponent is MenuStrip)
            {
                return NewItemTypesForMenuStrip;
            }
            if (toolStripFromComponent is ToolStripDropDownMenu)
            {
                return NewItemTypesForToolStripDropDownMenu;
            }
            if (toolStripFromComponent is StatusStrip)
            {
                return NewItemTypesForStatusStrip;
            }
            return NewItemTypesForToolStrip;
        }

        public static Bitmap GetToolboxBitmap(System.Type itemType)
        {
            if (itemType.Namespace == systemWindowsFormsNamespace)
            {
                return GetKnownToolboxBitmap(itemType);
            }
            ToolboxItem cachedToolboxItem = GetCachedToolboxItem(itemType);
            if (cachedToolboxItem != null)
            {
                return cachedToolboxItem.Bitmap;
            }
            return GetKnownToolboxBitmap(typeof(Component));
        }

        public static string GetToolboxDescription(System.Type itemType)
        {
            string displayName = null;
            ToolboxItem cachedToolboxItem = GetCachedToolboxItem(itemType);
            if (cachedToolboxItem != null)
            {
                displayName = cachedToolboxItem.DisplayName;
            }
            if (displayName == null)
            {
                displayName = itemType.Name;
            }
            if (displayName.StartsWith("ToolStrip"))
            {
                return displayName.Substring(9);
            }
            return displayName;
        }

        private static ToolStrip GetToolStripFromComponent(IComponent component)
        {
            ToolStripItem item = component as ToolStripItem;
            if (item != null)
            {
                if (item is ToolStripDropDownItem)
                {
                    return ((ToolStripDropDownItem) item).DropDown;
                }
                return item.Owner;
            }
            return (component as ToolStrip);
        }

        public static void InvalidateSelection(ArrayList originalSelComps, ToolStripItem nextSelection, IServiceProvider provider, bool shiftPressed)
        {
            if ((nextSelection != null) && (provider != null))
            {
                Region r = null;
                Region region = null;
                int width = 1;
                int num2 = 2;
                ToolStripItemDesigner designer = null;
                bool flag = false;
                try
                {
                    Rectangle empty = Rectangle.Empty;
                    IDesignerHost host = (IDesignerHost) provider.GetService(typeof(IDesignerHost));
                    if (host != null)
                    {
                        foreach (Component component in originalSelComps)
                        {
                            ToolStripItem item = component as ToolStripItem;
                            if ((item != null) && (((originalSelComps.Count > 1) || ((originalSelComps.Count == 1) && (item.GetCurrentParent() != nextSelection.GetCurrentParent()))) || (((item is ToolStripSeparator) || (item is ToolStripControlHost)) || (!item.IsOnDropDown || item.IsOnOverflow))))
                            {
                                designer = host.GetDesigner(item) as ToolStripItemDesigner;
                                if (designer != null)
                                {
                                    empty = designer.GetGlyphBounds();
                                    GetAdjustedBounds(item, ref empty);
                                    empty.Inflate(width, width);
                                    if (r == null)
                                    {
                                        r = new Region(empty);
                                        empty.Inflate(-num2, -num2);
                                        r.Exclude(empty);
                                    }
                                    else
                                    {
                                        region = new Region(empty);
                                        empty.Inflate(-num2, -num2);
                                        region.Exclude(empty);
                                        r.Union(region);
                                    }
                                }
                                else if (item is DesignerToolStripControlHost)
                                {
                                    flag = true;
                                }
                            }
                        }
                    }
                    if (((r != null) || flag) || shiftPressed)
                    {
                        BehaviorService service = (BehaviorService) provider.GetService(typeof(BehaviorService));
                        if (service != null)
                        {
                            if (r != null)
                            {
                                service.Invalidate(r);
                            }
                            designer = host.GetDesigner(nextSelection) as ToolStripItemDesigner;
                            if (designer != null)
                            {
                                empty = designer.GetGlyphBounds();
                                GetAdjustedBounds(nextSelection, ref empty);
                                empty.Inflate(width, width);
                                r = new Region(empty);
                                empty.Inflate(-num2, -num2);
                                r.Exclude(empty);
                                service.Invalidate(r);
                            }
                        }
                    }
                }
                finally
                {
                    if (r != null)
                    {
                        r.Dispose();
                    }
                    if (region != null)
                    {
                        region.Dispose();
                    }
                }
            }
        }

        internal static class DisplayInformation
        {
            private static short bitsPerPixel;
            private static bool dropShadowEnabled;
            private static bool dropShadowSettingValid;
            private static bool highContrast;
            private static bool highContrastSettingValid;
            private static bool isTerminalServerSession;
            private static bool lowRes;
            private static bool lowResSettingValid;
            private static bool terminalSettingValid;

            static DisplayInformation()
            {
                SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(ToolStripDesignerUtils.DisplayInformation.UserPreferenceChanged);
                SystemEvents.DisplaySettingsChanged += new EventHandler(ToolStripDesignerUtils.DisplayInformation.DisplaySettingChanged);
            }

            private static void DisplaySettingChanged(object obj, EventArgs ea)
            {
                highContrastSettingValid = false;
                lowResSettingValid = false;
                terminalSettingValid = false;
                dropShadowSettingValid = false;
            }

            private static void UserPreferenceChanged(object obj, UserPreferenceChangedEventArgs ea)
            {
                highContrastSettingValid = false;
                lowResSettingValid = false;
                terminalSettingValid = false;
                dropShadowSettingValid = false;
                bitsPerPixel = 0;
            }

            public static short BitsPerPixel
            {
                get
                {
                    if (bitsPerPixel == 0)
                    {
                        new EnvironmentPermission(PermissionState.Unrestricted).Assert();
                        try
                        {
                            foreach (Screen screen in Screen.AllScreens)
                            {
                                if (bitsPerPixel == 0)
                                {
                                    bitsPerPixel = (short) screen.BitsPerPixel;
                                }
                                else
                                {
                                    bitsPerPixel = (short) Math.Min(screen.BitsPerPixel, bitsPerPixel);
                                }
                            }
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                    }
                    return bitsPerPixel;
                }
            }

            public static bool HighContrast
            {
                get
                {
                    if (!highContrastSettingValid)
                    {
                        highContrast = SystemInformation.HighContrast;
                        highContrastSettingValid = true;
                    }
                    return highContrast;
                }
            }

            public static bool IsDropShadowEnabled
            {
                get
                {
                    if (!dropShadowSettingValid)
                    {
                        dropShadowEnabled = SystemInformation.IsDropShadowEnabled;
                        dropShadowSettingValid = true;
                    }
                    return dropShadowEnabled;
                }
            }

            public static bool LowResolution
            {
                get
                {
                    if (!lowResSettingValid)
                    {
                        lowRes = BitsPerPixel <= 8;
                        lowResSettingValid = true;
                    }
                    return lowRes;
                }
            }

            public static bool TerminalServer
            {
                get
                {
                    if (!terminalSettingValid)
                    {
                        isTerminalServerSession = SystemInformation.TerminalServerSession;
                        terminalSettingValid = true;
                    }
                    return isTerminalServerSession;
                }
            }
        }
    }
}

