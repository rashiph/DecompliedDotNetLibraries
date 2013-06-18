namespace System.Windows.Forms.VisualStyles
{
    using System;

    public class VisualStyleElement
    {
        private string className;
        internal static readonly int Count = 0x19;
        private int part;
        private int state;

        private VisualStyleElement(string className, int part, int state)
        {
            this.className = className;
            this.part = part;
            this.state = state;
        }

        public static VisualStyleElement CreateElement(string className, int part, int state)
        {
            return new VisualStyleElement(className, part, state);
        }

        public string ClassName
        {
            get
            {
                return this.className;
            }
        }

        public int Part
        {
            get
            {
                return this.part;
            }
        }

        public int State
        {
            get
            {
                return this.state;
            }
        }

        public static class Button
        {
            private static readonly string className = "BUTTON";

            public static class CheckBox
            {
                private static VisualStyleElement checkeddisabled;
                private static VisualStyleElement checkedhot;
                private static VisualStyleElement checkednormal;
                private static VisualStyleElement checkedpressed;
                private static VisualStyleElement mixeddisabled;
                private static VisualStyleElement mixedhot;
                private static VisualStyleElement mixednormal;
                private static VisualStyleElement mixedpressed;
                private static readonly int part = 3;
                private static VisualStyleElement uncheckeddisabled;
                private static VisualStyleElement uncheckedhot;
                private static VisualStyleElement uncheckednormal;
                private static VisualStyleElement uncheckedpressed;

                public static VisualStyleElement CheckedDisabled
                {
                    get
                    {
                        if (checkeddisabled == null)
                        {
                            checkeddisabled = new VisualStyleElement(VisualStyleElement.Button.className, part, 8);
                        }
                        return checkeddisabled;
                    }
                }

                public static VisualStyleElement CheckedHot
                {
                    get
                    {
                        if (checkedhot == null)
                        {
                            checkedhot = new VisualStyleElement(VisualStyleElement.Button.className, part, 6);
                        }
                        return checkedhot;
                    }
                }

                public static VisualStyleElement CheckedNormal
                {
                    get
                    {
                        if (checkednormal == null)
                        {
                            checkednormal = new VisualStyleElement(VisualStyleElement.Button.className, part, 5);
                        }
                        return checkednormal;
                    }
                }

                public static VisualStyleElement CheckedPressed
                {
                    get
                    {
                        if (checkedpressed == null)
                        {
                            checkedpressed = new VisualStyleElement(VisualStyleElement.Button.className, part, 7);
                        }
                        return checkedpressed;
                    }
                }

                public static VisualStyleElement MixedDisabled
                {
                    get
                    {
                        if (mixeddisabled == null)
                        {
                            mixeddisabled = new VisualStyleElement(VisualStyleElement.Button.className, part, 12);
                        }
                        return mixeddisabled;
                    }
                }

                public static VisualStyleElement MixedHot
                {
                    get
                    {
                        if (mixedhot == null)
                        {
                            mixedhot = new VisualStyleElement(VisualStyleElement.Button.className, part, 10);
                        }
                        return mixedhot;
                    }
                }

                public static VisualStyleElement MixedNormal
                {
                    get
                    {
                        if (mixednormal == null)
                        {
                            mixednormal = new VisualStyleElement(VisualStyleElement.Button.className, part, 9);
                        }
                        return mixednormal;
                    }
                }

                public static VisualStyleElement MixedPressed
                {
                    get
                    {
                        if (mixedpressed == null)
                        {
                            mixedpressed = new VisualStyleElement(VisualStyleElement.Button.className, part, 11);
                        }
                        return mixedpressed;
                    }
                }

                public static VisualStyleElement UncheckedDisabled
                {
                    get
                    {
                        if (uncheckeddisabled == null)
                        {
                            uncheckeddisabled = new VisualStyleElement(VisualStyleElement.Button.className, part, 4);
                        }
                        return uncheckeddisabled;
                    }
                }

                public static VisualStyleElement UncheckedHot
                {
                    get
                    {
                        if (uncheckedhot == null)
                        {
                            uncheckedhot = new VisualStyleElement(VisualStyleElement.Button.className, part, 2);
                        }
                        return uncheckedhot;
                    }
                }

                public static VisualStyleElement UncheckedNormal
                {
                    get
                    {
                        if (uncheckednormal == null)
                        {
                            uncheckednormal = new VisualStyleElement(VisualStyleElement.Button.className, part, 1);
                        }
                        return uncheckednormal;
                    }
                }

                public static VisualStyleElement UncheckedPressed
                {
                    get
                    {
                        if (uncheckedpressed == null)
                        {
                            uncheckedpressed = new VisualStyleElement(VisualStyleElement.Button.className, part, 3);
                        }
                        return uncheckedpressed;
                    }
                }
            }

            public static class GroupBox
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement normal;
                private static readonly int part = 4;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Button.className, part, 2);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Button.className, part, 1);
                        }
                        return normal;
                    }
                }
            }

            public static class PushButton
            {
                private static VisualStyleElement _default;
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 1;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Default
                {
                    get
                    {
                        if (_default == null)
                        {
                            _default = new VisualStyleElement(VisualStyleElement.Button.className, part, 5);
                        }
                        return _default;
                    }
                }

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Button.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Button.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Button.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Button.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class RadioButton
            {
                private static VisualStyleElement checkeddisabled;
                private static VisualStyleElement checkedhot;
                private static VisualStyleElement checkednormal;
                private static VisualStyleElement checkedpressed;
                private static readonly int part = 2;
                private static VisualStyleElement uncheckeddisabled;
                private static VisualStyleElement uncheckedhot;
                private static VisualStyleElement uncheckednormal;
                private static VisualStyleElement uncheckedpressed;

                public static VisualStyleElement CheckedDisabled
                {
                    get
                    {
                        if (checkeddisabled == null)
                        {
                            checkeddisabled = new VisualStyleElement(VisualStyleElement.Button.className, part, 8);
                        }
                        return checkeddisabled;
                    }
                }

                public static VisualStyleElement CheckedHot
                {
                    get
                    {
                        if (checkedhot == null)
                        {
                            checkedhot = new VisualStyleElement(VisualStyleElement.Button.className, part, 6);
                        }
                        return checkedhot;
                    }
                }

                public static VisualStyleElement CheckedNormal
                {
                    get
                    {
                        if (checkednormal == null)
                        {
                            checkednormal = new VisualStyleElement(VisualStyleElement.Button.className, part, 5);
                        }
                        return checkednormal;
                    }
                }

                public static VisualStyleElement CheckedPressed
                {
                    get
                    {
                        if (checkedpressed == null)
                        {
                            checkedpressed = new VisualStyleElement(VisualStyleElement.Button.className, part, 7);
                        }
                        return checkedpressed;
                    }
                }

                public static VisualStyleElement UncheckedDisabled
                {
                    get
                    {
                        if (uncheckeddisabled == null)
                        {
                            uncheckeddisabled = new VisualStyleElement(VisualStyleElement.Button.className, part, 4);
                        }
                        return uncheckeddisabled;
                    }
                }

                public static VisualStyleElement UncheckedHot
                {
                    get
                    {
                        if (uncheckedhot == null)
                        {
                            uncheckedhot = new VisualStyleElement(VisualStyleElement.Button.className, part, 2);
                        }
                        return uncheckedhot;
                    }
                }

                public static VisualStyleElement UncheckedNormal
                {
                    get
                    {
                        if (uncheckednormal == null)
                        {
                            uncheckednormal = new VisualStyleElement(VisualStyleElement.Button.className, part, 1);
                        }
                        return uncheckednormal;
                    }
                }

                public static VisualStyleElement UncheckedPressed
                {
                    get
                    {
                        if (uncheckedpressed == null)
                        {
                            uncheckedpressed = new VisualStyleElement(VisualStyleElement.Button.className, part, 3);
                        }
                        return uncheckedpressed;
                    }
                }
            }

            public static class UserButton
            {
                private static VisualStyleElement normal;
                private static readonly int part = 5;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Button.className, part, 0);
                        }
                        return normal;
                    }
                }
            }
        }

        public static class ComboBox
        {
            private static readonly string className = "COMBOBOX";

            internal static class Border
            {
                private static VisualStyleElement normal;
                private const int part = 4;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ComboBox.className, 4, 3);
                        }
                        return normal;
                    }
                }
            }

            public static class DropDownButton
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 1;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.ComboBox.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ComboBox.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ComboBox.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ComboBox.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            internal static class DropDownButtonLeft
            {
                private static VisualStyleElement normal;
                private const int part = 7;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ComboBox.className, 7, 2);
                        }
                        return normal;
                    }
                }
            }

            internal static class DropDownButtonRight
            {
                private static VisualStyleElement normal;
                private const int part = 6;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ComboBox.className, 6, 1);
                        }
                        return normal;
                    }
                }
            }

            internal static class ReadOnlyButton
            {
                private static VisualStyleElement normal;
                private const int part = 5;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ComboBox.className, 5, 2);
                        }
                        return normal;
                    }
                }
            }
        }

        public static class ExplorerBar
        {
            private static readonly string className = "EXPLORERBAR";

            public static class HeaderBackground
            {
                private static VisualStyleElement normal;
                private static readonly int part = 1;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class HeaderClose
            {
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 2;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class HeaderPin
            {
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 3;
                private static VisualStyleElement pressed;
                private static VisualStyleElement selectedhot;
                private static VisualStyleElement selectednormal;
                private static VisualStyleElement selectedpressed;

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 3);
                        }
                        return pressed;
                    }
                }

                public static VisualStyleElement SelectedHot
                {
                    get
                    {
                        if (selectedhot == null)
                        {
                            selectedhot = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 5);
                        }
                        return selectedhot;
                    }
                }

                public static VisualStyleElement SelectedNormal
                {
                    get
                    {
                        if (selectednormal == null)
                        {
                            selectednormal = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 4);
                        }
                        return selectednormal;
                    }
                }

                public static VisualStyleElement SelectedPressed
                {
                    get
                    {
                        if (selectedpressed == null)
                        {
                            selectedpressed = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 6);
                        }
                        return selectedpressed;
                    }
                }
            }

            public static class IEBarMenu
            {
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 4;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class NormalGroupBackground
            {
                private static VisualStyleElement normal;
                private static readonly int part = 5;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class NormalGroupCollapse
            {
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 6;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class NormalGroupExpand
            {
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 7;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class NormalGroupHead
            {
                private static VisualStyleElement normal;
                private static readonly int part = 8;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class SpecialGroupBackground
            {
                private static VisualStyleElement normal;
                private static readonly int part = 9;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class SpecialGroupCollapse
            {
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 10;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class SpecialGroupExpand
            {
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 11;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class SpecialGroupHead
            {
                private static VisualStyleElement normal;
                private static readonly int part = 12;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ExplorerBar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }
        }

        internal static class ExplorerTreeView
        {
            private static readonly string className = "Explorer::TreeView";

            public static class Glyph
            {
                private static VisualStyleElement closed;
                private static VisualStyleElement opened;
                private static readonly int part = 2;

                public static VisualStyleElement Closed
                {
                    get
                    {
                        if (closed == null)
                        {
                            closed = new VisualStyleElement(VisualStyleElement.ExplorerTreeView.className, part, 1);
                        }
                        return closed;
                    }
                }

                public static VisualStyleElement Opened
                {
                    get
                    {
                        if (opened == null)
                        {
                            opened = new VisualStyleElement(VisualStyleElement.ExplorerTreeView.className, part, 2);
                        }
                        return opened;
                    }
                }
            }
        }

        public static class Header
        {
            private static readonly string className = "HEADER";

            public static class Item
            {
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 1;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Header.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Header.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Header.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class ItemLeft
            {
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 2;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Header.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Header.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Header.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class ItemRight
            {
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 3;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Header.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Header.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Header.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class SortArrow
            {
                private static readonly int part = 4;
                private static VisualStyleElement sorteddown;
                private static VisualStyleElement sortedup;

                public static VisualStyleElement SortedDown
                {
                    get
                    {
                        if (sorteddown == null)
                        {
                            sorteddown = new VisualStyleElement(VisualStyleElement.Header.className, part, 2);
                        }
                        return sorteddown;
                    }
                }

                public static VisualStyleElement SortedUp
                {
                    get
                    {
                        if (sortedup == null)
                        {
                            sortedup = new VisualStyleElement(VisualStyleElement.Header.className, part, 1);
                        }
                        return sortedup;
                    }
                }
            }
        }

        public static class ListView
        {
            private static readonly string className = "LISTVIEW";

            public static class Detail
            {
                private static VisualStyleElement normal;
                private static readonly int part = 3;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ListView.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class EmptyText
            {
                private static VisualStyleElement normal;
                private static readonly int part = 5;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ListView.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class Group
            {
                private static VisualStyleElement normal;
                private static readonly int part = 2;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ListView.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class Item
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 1;
                private static VisualStyleElement selected;
                private static VisualStyleElement selectednotfocus;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.ListView.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ListView.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ListView.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Selected
                {
                    get
                    {
                        if (selected == null)
                        {
                            selected = new VisualStyleElement(VisualStyleElement.ListView.className, part, 3);
                        }
                        return selected;
                    }
                }

                public static VisualStyleElement SelectedNotFocus
                {
                    get
                    {
                        if (selectednotfocus == null)
                        {
                            selectednotfocus = new VisualStyleElement(VisualStyleElement.ListView.className, part, 5);
                        }
                        return selectednotfocus;
                    }
                }
            }

            public static class SortedDetail
            {
                private static VisualStyleElement normal;
                private static readonly int part = 4;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ListView.className, part, 0);
                        }
                        return normal;
                    }
                }
            }
        }

        public static class Menu
        {
            private static readonly string className = "MENU";

            public static class BarDropDown
            {
                private static VisualStyleElement normal;
                private static readonly int part = 4;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Menu.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class BarItem
            {
                private static VisualStyleElement normal;
                private static readonly int part = 3;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Menu.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class Chevron
            {
                private static VisualStyleElement normal;
                private static readonly int part = 5;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Menu.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class DropDown
            {
                private static VisualStyleElement normal;
                private static readonly int part = 2;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Menu.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class Item
            {
                private static VisualStyleElement demoted;
                private static VisualStyleElement normal;
                private static readonly int part = 1;
                private static VisualStyleElement selected;

                public static VisualStyleElement Demoted
                {
                    get
                    {
                        if (demoted == null)
                        {
                            demoted = new VisualStyleElement(VisualStyleElement.Menu.className, part, 3);
                        }
                        return demoted;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Menu.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Selected
                {
                    get
                    {
                        if (selected == null)
                        {
                            selected = new VisualStyleElement(VisualStyleElement.Menu.className, part, 2);
                        }
                        return selected;
                    }
                }
            }

            public static class Separator
            {
                private static VisualStyleElement normal;
                private static readonly int part = 6;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Menu.className, part, 0);
                        }
                        return normal;
                    }
                }
            }
        }

        public static class MenuBand
        {
            private static readonly string className = "MENUBAND";

            public static class NewApplicationButton
            {
                private static VisualStyleElement _checked;
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement hotchecked;
                private static VisualStyleElement normal;
                private static readonly int part = 1;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Checked
                {
                    get
                    {
                        if (_checked == null)
                        {
                            _checked = new VisualStyleElement(VisualStyleElement.MenuBand.className, part, 5);
                        }
                        return _checked;
                    }
                }

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.MenuBand.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.MenuBand.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement HotChecked
                {
                    get
                    {
                        if (hotchecked == null)
                        {
                            hotchecked = new VisualStyleElement(VisualStyleElement.MenuBand.className, part, 6);
                        }
                        return hotchecked;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.MenuBand.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.MenuBand.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class Separator
            {
                private static VisualStyleElement normal;
                private static readonly int part = 2;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.MenuBand.className, part, 0);
                        }
                        return normal;
                    }
                }
            }
        }

        public static class Page
        {
            private static readonly string className = "PAGE";

            public static class Down
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 2;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Page.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Page.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Page.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Page.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class DownHorizontal
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 4;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Page.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Page.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Page.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Page.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class Up
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 1;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Page.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Page.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Page.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Page.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class UpHorizontal
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 3;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Page.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Page.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Page.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Page.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }
        }

        public static class ProgressBar
        {
            private static readonly string className = "PROGRESS";

            public static class Bar
            {
                private static VisualStyleElement normal;
                private static readonly int part = 1;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ProgressBar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class BarVertical
            {
                private static VisualStyleElement normal;
                private static readonly int part = 2;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ProgressBar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class Chunk
            {
                private static VisualStyleElement normal;
                private static readonly int part = 3;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ProgressBar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class ChunkVertical
            {
                private static VisualStyleElement normal;
                private static readonly int part = 4;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ProgressBar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }
        }

        public static class Rebar
        {
            private static readonly string className = "REBAR";

            public static class Band
            {
                private static VisualStyleElement normal;
                private static readonly int part = 3;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Rebar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class Chevron
            {
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 4;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Rebar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Rebar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Rebar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class ChevronVertical
            {
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 5;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Rebar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Rebar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Rebar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class Gripper
            {
                private static VisualStyleElement normal;
                private static readonly int part = 1;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Rebar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class GripperVertical
            {
                private static VisualStyleElement normal;
                private static readonly int part = 2;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Rebar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }
        }

        public static class ScrollBar
        {
            private static readonly string className = "SCROLLBAR";

            public static class ArrowButton
            {
                private static VisualStyleElement downdisabled;
                private static VisualStyleElement downhot;
                private static VisualStyleElement downnormal;
                private static VisualStyleElement downpressed;
                private static VisualStyleElement leftdisabled;
                private static VisualStyleElement lefthot;
                private static VisualStyleElement leftnormal;
                private static VisualStyleElement leftpressed;
                private static readonly int part = 1;
                private static VisualStyleElement rightdisabled;
                private static VisualStyleElement righthot;
                private static VisualStyleElement rightnormal;
                private static VisualStyleElement rightpressed;
                private static VisualStyleElement updisabled;
                private static VisualStyleElement uphot;
                private static VisualStyleElement upnormal;
                private static VisualStyleElement uppressed;

                public static VisualStyleElement DownDisabled
                {
                    get
                    {
                        if (downdisabled == null)
                        {
                            downdisabled = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 8);
                        }
                        return downdisabled;
                    }
                }

                public static VisualStyleElement DownHot
                {
                    get
                    {
                        if (downhot == null)
                        {
                            downhot = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 6);
                        }
                        return downhot;
                    }
                }

                public static VisualStyleElement DownNormal
                {
                    get
                    {
                        if (downnormal == null)
                        {
                            downnormal = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 5);
                        }
                        return downnormal;
                    }
                }

                public static VisualStyleElement DownPressed
                {
                    get
                    {
                        if (downpressed == null)
                        {
                            downpressed = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 7);
                        }
                        return downpressed;
                    }
                }

                public static VisualStyleElement LeftDisabled
                {
                    get
                    {
                        if (leftdisabled == null)
                        {
                            leftdisabled = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 12);
                        }
                        return leftdisabled;
                    }
                }

                public static VisualStyleElement LeftHot
                {
                    get
                    {
                        if (lefthot == null)
                        {
                            lefthot = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 10);
                        }
                        return lefthot;
                    }
                }

                public static VisualStyleElement LeftNormal
                {
                    get
                    {
                        if (leftnormal == null)
                        {
                            leftnormal = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 9);
                        }
                        return leftnormal;
                    }
                }

                public static VisualStyleElement LeftPressed
                {
                    get
                    {
                        if (leftpressed == null)
                        {
                            leftpressed = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 11);
                        }
                        return leftpressed;
                    }
                }

                public static VisualStyleElement RightDisabled
                {
                    get
                    {
                        if (rightdisabled == null)
                        {
                            rightdisabled = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 0x10);
                        }
                        return rightdisabled;
                    }
                }

                public static VisualStyleElement RightHot
                {
                    get
                    {
                        if (righthot == null)
                        {
                            righthot = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 14);
                        }
                        return righthot;
                    }
                }

                public static VisualStyleElement RightNormal
                {
                    get
                    {
                        if (rightnormal == null)
                        {
                            rightnormal = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 13);
                        }
                        return rightnormal;
                    }
                }

                public static VisualStyleElement RightPressed
                {
                    get
                    {
                        if (rightpressed == null)
                        {
                            rightpressed = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 15);
                        }
                        return rightpressed;
                    }
                }

                public static VisualStyleElement UpDisabled
                {
                    get
                    {
                        if (updisabled == null)
                        {
                            updisabled = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 4);
                        }
                        return updisabled;
                    }
                }

                public static VisualStyleElement UpHot
                {
                    get
                    {
                        if (uphot == null)
                        {
                            uphot = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 2);
                        }
                        return uphot;
                    }
                }

                public static VisualStyleElement UpNormal
                {
                    get
                    {
                        if (upnormal == null)
                        {
                            upnormal = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 1);
                        }
                        return upnormal;
                    }
                }

                public static VisualStyleElement UpPressed
                {
                    get
                    {
                        if (uppressed == null)
                        {
                            uppressed = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 3);
                        }
                        return uppressed;
                    }
                }
            }

            public static class GripperHorizontal
            {
                private static VisualStyleElement normal;
                private static readonly int part = 8;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class GripperVertical
            {
                private static VisualStyleElement normal;
                private static readonly int part = 9;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class LeftTrackHorizontal
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 5;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class LowerTrackVertical
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 6;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class RightTrackHorizontal
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 4;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class SizeBox
            {
                private static VisualStyleElement leftalign;
                private static readonly int part = 10;
                private static VisualStyleElement rightalign;

                public static VisualStyleElement LeftAlign
                {
                    get
                    {
                        if (leftalign == null)
                        {
                            leftalign = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 2);
                        }
                        return leftalign;
                    }
                }

                public static VisualStyleElement RightAlign
                {
                    get
                    {
                        if (rightalign == null)
                        {
                            rightalign = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 1);
                        }
                        return rightalign;
                    }
                }
            }

            public static class ThumbButtonHorizontal
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 2;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class ThumbButtonVertical
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 3;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class UpperTrackVertical
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 7;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ScrollBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }
        }

        public static class Spin
        {
            private static readonly string className = "SPIN";

            public static class Down
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 2;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Spin.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Spin.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Spin.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Spin.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class DownHorizontal
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 4;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Spin.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Spin.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Spin.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Spin.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class Up
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 1;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Spin.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Spin.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Spin.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Spin.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class UpHorizontal
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 3;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Spin.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Spin.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Spin.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Spin.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }
        }

        public static class StartPanel
        {
            private static readonly string className = "STARTPANEL";

            public static class LogOff
            {
                private static VisualStyleElement normal;
                private static readonly int part = 8;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.StartPanel.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class LogOffButtons
            {
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 9;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.StartPanel.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.StartPanel.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.StartPanel.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class MorePrograms
            {
                private static VisualStyleElement normal;
                private static readonly int part = 2;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.StartPanel.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class MoreProgramsArrow
            {
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 3;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.StartPanel.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.StartPanel.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.StartPanel.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class PlaceList
            {
                private static VisualStyleElement normal;
                private static readonly int part = 6;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.StartPanel.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class PlaceListSeparator
            {
                private static VisualStyleElement normal;
                private static readonly int part = 7;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.StartPanel.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class Preview
            {
                private static VisualStyleElement normal;
                private static readonly int part = 11;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.StartPanel.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class ProgList
            {
                private static VisualStyleElement normal;
                private static readonly int part = 4;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.StartPanel.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class ProgListSeparator
            {
                private static VisualStyleElement normal;
                private static readonly int part = 5;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.StartPanel.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class UserPane
            {
                private static VisualStyleElement normal;
                private static readonly int part = 1;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.StartPanel.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class UserPicture
            {
                private static VisualStyleElement normal;
                private static readonly int part = 10;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.StartPanel.className, part, 0);
                        }
                        return normal;
                    }
                }
            }
        }

        public static class Status
        {
            private static readonly string className = "STATUS";

            public static class Bar
            {
                private static VisualStyleElement normal;
                private static readonly int part;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Status.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class Gripper
            {
                private static VisualStyleElement normal;
                private static readonly int part = 3;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Status.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class GripperPane
            {
                private static VisualStyleElement normal;
                private static readonly int part = 2;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Status.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class Pane
            {
                private static VisualStyleElement normal;
                private static readonly int part = 1;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Status.className, part, 0);
                        }
                        return normal;
                    }
                }
            }
        }

        public static class Tab
        {
            private static readonly string className = "TAB";

            public static class Body
            {
                private static VisualStyleElement normal;
                private static readonly int part = 10;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Tab.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class Pane
            {
                private static VisualStyleElement normal;
                private static readonly int part = 9;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Tab.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class TabItem
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 1;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Tab.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Tab.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Tab.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Tab.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class TabItemBothEdges
            {
                private static VisualStyleElement normal;
                private static readonly int part = 4;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Tab.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class TabItemLeftEdge
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 2;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Tab.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Tab.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Tab.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Tab.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class TabItemRightEdge
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 3;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Tab.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Tab.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Tab.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Tab.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class TopTabItem
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 5;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Tab.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Tab.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Tab.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Tab.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class TopTabItemBothEdges
            {
                private static VisualStyleElement normal;
                private static readonly int part = 8;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Tab.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class TopTabItemLeftEdge
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 6;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Tab.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Tab.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Tab.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Tab.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class TopTabItemRightEdge
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 7;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Tab.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Tab.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Tab.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Tab.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }
        }

        public static class TaskBand
        {
            private static readonly string className = "TASKBAND";

            public static class FlashButton
            {
                private static VisualStyleElement normal;
                private static readonly int part = 2;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TaskBand.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class FlashButtonGroupMenu
            {
                private static VisualStyleElement normal;
                private static readonly int part = 3;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TaskBand.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class GroupCount
            {
                private static VisualStyleElement normal;
                private static readonly int part = 1;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TaskBand.className, part, 0);
                        }
                        return normal;
                    }
                }
            }
        }

        public static class Taskbar
        {
            private static readonly string className = "TASKBAR";

            public static class BackgroundBottom
            {
                private static VisualStyleElement normal;
                private static readonly int part = 1;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Taskbar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class BackgroundLeft
            {
                private static VisualStyleElement normal;
                private static readonly int part = 4;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Taskbar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class BackgroundRight
            {
                private static VisualStyleElement normal;
                private static readonly int part = 2;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Taskbar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class BackgroundTop
            {
                private static VisualStyleElement normal;
                private static readonly int part = 3;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Taskbar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class SizingBarBottom
            {
                private static VisualStyleElement normal;
                private static readonly int part = 5;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Taskbar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class SizingBarLeft
            {
                private static VisualStyleElement normal;
                private static readonly int part = 8;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Taskbar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class SizingBarRight
            {
                private static VisualStyleElement normal;
                private static readonly int part = 6;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Taskbar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class SizingBarTop
            {
                private static VisualStyleElement normal;
                private static readonly int part = 7;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Taskbar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }
        }

        public static class TaskbarClock
        {
            private static readonly string className = "CLOCK";

            public static class Time
            {
                private static VisualStyleElement normal;
                private static readonly int part = 1;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TaskbarClock.className, part, 1);
                        }
                        return normal;
                    }
                }
            }
        }

        public static class TextBox
        {
            private static readonly string className = "EDIT";

            public static class Caret
            {
                private static VisualStyleElement normal;
                private static readonly int part = 2;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TextBox.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class TextEdit
            {
                private static VisualStyleElement _readonly;
                private static VisualStyleElement assist;
                private static VisualStyleElement disabled;
                private static VisualStyleElement focused;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 1;
                private static VisualStyleElement selected;

                public static VisualStyleElement Assist
                {
                    get
                    {
                        if (assist == null)
                        {
                            assist = new VisualStyleElement(VisualStyleElement.TextBox.className, part, 7);
                        }
                        return assist;
                    }
                }

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.TextBox.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Focused
                {
                    get
                    {
                        if (focused == null)
                        {
                            focused = new VisualStyleElement(VisualStyleElement.TextBox.className, part, 5);
                        }
                        return focused;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.TextBox.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TextBox.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement ReadOnly
                {
                    get
                    {
                        if (_readonly == null)
                        {
                            _readonly = new VisualStyleElement(VisualStyleElement.TextBox.className, part, 6);
                        }
                        return _readonly;
                    }
                }

                public static VisualStyleElement Selected
                {
                    get
                    {
                        if (selected == null)
                        {
                            selected = new VisualStyleElement(VisualStyleElement.TextBox.className, part, 3);
                        }
                        return selected;
                    }
                }
            }
        }

        public static class ToolBar
        {
            private static readonly string className = "TOOLBAR";

            internal static class Bar
            {
                private static VisualStyleElement normal;
                private static readonly int part;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class Button
            {
                private static VisualStyleElement _checked;
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement hotchecked;
                private static VisualStyleElement normal;
                private static readonly int part = 1;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Checked
                {
                    get
                    {
                        if (_checked == null)
                        {
                            _checked = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 5);
                        }
                        return _checked;
                    }
                }

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement HotChecked
                {
                    get
                    {
                        if (hotchecked == null)
                        {
                            hotchecked = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 6);
                        }
                        return hotchecked;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class DropDownButton
            {
                private static VisualStyleElement _checked;
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement hotchecked;
                private static VisualStyleElement normal;
                private static readonly int part = 2;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Checked
                {
                    get
                    {
                        if (_checked == null)
                        {
                            _checked = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 5);
                        }
                        return _checked;
                    }
                }

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement HotChecked
                {
                    get
                    {
                        if (hotchecked == null)
                        {
                            hotchecked = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 6);
                        }
                        return hotchecked;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class SeparatorHorizontal
            {
                private static VisualStyleElement normal;
                private static readonly int part = 5;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class SeparatorVertical
            {
                private static VisualStyleElement normal;
                private static readonly int part = 6;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class SplitButton
            {
                private static VisualStyleElement _checked;
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement hotchecked;
                private static VisualStyleElement normal;
                private static readonly int part = 3;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Checked
                {
                    get
                    {
                        if (_checked == null)
                        {
                            _checked = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 5);
                        }
                        return _checked;
                    }
                }

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement HotChecked
                {
                    get
                    {
                        if (hotchecked == null)
                        {
                            hotchecked = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 6);
                        }
                        return hotchecked;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class SplitButtonDropDown
            {
                private static VisualStyleElement _checked;
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement hotchecked;
                private static VisualStyleElement normal;
                private static readonly int part = 4;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Checked
                {
                    get
                    {
                        if (_checked == null)
                        {
                            _checked = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 5);
                        }
                        return _checked;
                    }
                }

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement HotChecked
                {
                    get
                    {
                        if (hotchecked == null)
                        {
                            hotchecked = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 6);
                        }
                        return hotchecked;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ToolBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }
        }

        public static class ToolTip
        {
            private static readonly string className = "TOOLTIP";

            public static class Balloon
            {
                private static VisualStyleElement link;
                private static VisualStyleElement normal;
                private static readonly int part = 3;

                public static VisualStyleElement Link
                {
                    get
                    {
                        if (link == null)
                        {
                            link = new VisualStyleElement(VisualStyleElement.ToolTip.className, part, 2);
                        }
                        return link;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ToolTip.className, part, 1);
                        }
                        return normal;
                    }
                }
            }

            public static class BalloonTitle
            {
                private static VisualStyleElement normal;
                private static readonly int part = 4;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ToolTip.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class Close
            {
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 5;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.ToolTip.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ToolTip.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.ToolTip.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class Standard
            {
                private static VisualStyleElement link;
                private static VisualStyleElement normal;
                private static readonly int part = 1;

                public static VisualStyleElement Link
                {
                    get
                    {
                        if (link == null)
                        {
                            link = new VisualStyleElement(VisualStyleElement.ToolTip.className, part, 2);
                        }
                        return link;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ToolTip.className, part, 1);
                        }
                        return normal;
                    }
                }
            }

            public static class StandardTitle
            {
                private static VisualStyleElement normal;
                private static readonly int part = 2;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.ToolTip.className, part, 0);
                        }
                        return normal;
                    }
                }
            }
        }

        public static class TrackBar
        {
            private static readonly string className = "TRACKBAR";

            public static class Thumb
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement focused;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 3;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 5);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Focused
                {
                    get
                    {
                        if (focused == null)
                        {
                            focused = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 4);
                        }
                        return focused;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class ThumbBottom
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement focused;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 4;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 5);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Focused
                {
                    get
                    {
                        if (focused == null)
                        {
                            focused = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 4);
                        }
                        return focused;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class ThumbLeft
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement focused;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 7;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 5);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Focused
                {
                    get
                    {
                        if (focused == null)
                        {
                            focused = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 4);
                        }
                        return focused;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class ThumbRight
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement focused;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 8;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 5);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Focused
                {
                    get
                    {
                        if (focused == null)
                        {
                            focused = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 4);
                        }
                        return focused;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class ThumbTop
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement focused;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 5;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 5);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Focused
                {
                    get
                    {
                        if (focused == null)
                        {
                            focused = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 4);
                        }
                        return focused;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class ThumbVertical
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement focused;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 6;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 5);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Focused
                {
                    get
                    {
                        if (focused == null)
                        {
                            focused = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 4);
                        }
                        return focused;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class Ticks
            {
                private static VisualStyleElement normal;
                private static readonly int part = 9;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 1);
                        }
                        return normal;
                    }
                }
            }

            public static class TicksVertical
            {
                private static VisualStyleElement normal;
                private static readonly int part = 10;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 1);
                        }
                        return normal;
                    }
                }
            }

            public static class Track
            {
                private static VisualStyleElement normal;
                private static readonly int part = 1;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 1);
                        }
                        return normal;
                    }
                }
            }

            public static class TrackVertical
            {
                private static VisualStyleElement normal;
                private static readonly int part = 2;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TrackBar.className, part, 1);
                        }
                        return normal;
                    }
                }
            }
        }

        public static class TrayNotify
        {
            private static readonly string className = "TRAYNOTIFY";

            public static class AnimateBackground
            {
                private static VisualStyleElement normal;
                private static readonly int part = 2;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TrayNotify.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class Background
            {
                private static VisualStyleElement normal;
                private static readonly int part = 1;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TrayNotify.className, part, 0);
                        }
                        return normal;
                    }
                }
            }
        }

        public static class TreeView
        {
            private static readonly string className = "TREEVIEW";

            public static class Branch
            {
                private static VisualStyleElement normal;
                private static readonly int part = 3;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TreeView.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class Glyph
            {
                private static VisualStyleElement closed;
                private static VisualStyleElement opened;
                private static readonly int part = 2;

                public static VisualStyleElement Closed
                {
                    get
                    {
                        if (closed == null)
                        {
                            closed = new VisualStyleElement(VisualStyleElement.TreeView.className, part, 1);
                        }
                        return closed;
                    }
                }

                public static VisualStyleElement Opened
                {
                    get
                    {
                        if (opened == null)
                        {
                            opened = new VisualStyleElement(VisualStyleElement.TreeView.className, part, 2);
                        }
                        return opened;
                    }
                }
            }

            public static class Item
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 1;
                private static VisualStyleElement selected;
                private static VisualStyleElement selectednotfocus;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.TreeView.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.TreeView.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.TreeView.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Selected
                {
                    get
                    {
                        if (selected == null)
                        {
                            selected = new VisualStyleElement(VisualStyleElement.TreeView.className, part, 3);
                        }
                        return selected;
                    }
                }

                public static VisualStyleElement SelectedNotFocus
                {
                    get
                    {
                        if (selectednotfocus == null)
                        {
                            selectednotfocus = new VisualStyleElement(VisualStyleElement.TreeView.className, part, 5);
                        }
                        return selectednotfocus;
                    }
                }
            }
        }

        public static class Window
        {
            private static readonly string className = "WINDOW";

            public static class Caption
            {
                private static VisualStyleElement active;
                private static VisualStyleElement disabled;
                private static VisualStyleElement inactive;
                private static readonly int part = 1;

                public static VisualStyleElement Active
                {
                    get
                    {
                        if (active == null)
                        {
                            active = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return active;
                    }
                }

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Inactive
                {
                    get
                    {
                        if (inactive == null)
                        {
                            inactive = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return inactive;
                    }
                }
            }

            public static class CaptionSizingTemplate
            {
                private static VisualStyleElement normal;
                private static readonly int part = 30;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class CloseButton
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 0x12;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class Dialog
            {
                private static VisualStyleElement normal;
                private static readonly int part = 0x1d;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class FrameBottom
            {
                private static VisualStyleElement active;
                private static VisualStyleElement inactive;
                private static readonly int part = 9;

                public static VisualStyleElement Active
                {
                    get
                    {
                        if (active == null)
                        {
                            active = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return active;
                    }
                }

                public static VisualStyleElement Inactive
                {
                    get
                    {
                        if (inactive == null)
                        {
                            inactive = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return inactive;
                    }
                }
            }

            public static class FrameBottomSizingTemplate
            {
                private static VisualStyleElement normal;
                private static readonly int part = 0x24;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class FrameLeft
            {
                private static VisualStyleElement active;
                private static VisualStyleElement inactive;
                private static readonly int part = 7;

                public static VisualStyleElement Active
                {
                    get
                    {
                        if (active == null)
                        {
                            active = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return active;
                    }
                }

                public static VisualStyleElement Inactive
                {
                    get
                    {
                        if (inactive == null)
                        {
                            inactive = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return inactive;
                    }
                }
            }

            public static class FrameLeftSizingTemplate
            {
                private static VisualStyleElement normal;
                private static readonly int part = 0x20;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class FrameRight
            {
                private static VisualStyleElement active;
                private static VisualStyleElement inactive;
                private static readonly int part = 8;

                public static VisualStyleElement Active
                {
                    get
                    {
                        if (active == null)
                        {
                            active = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return active;
                    }
                }

                public static VisualStyleElement Inactive
                {
                    get
                    {
                        if (inactive == null)
                        {
                            inactive = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return inactive;
                    }
                }
            }

            public static class FrameRightSizingTemplate
            {
                private static VisualStyleElement normal;
                private static readonly int part = 0x22;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class HelpButton
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 0x17;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class HorizontalScroll
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 0x19;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class HorizontalThumb
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 0x1a;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class MaxButton
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 0x11;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class MaxCaption
            {
                private static VisualStyleElement active;
                private static VisualStyleElement disabled;
                private static VisualStyleElement inactive;
                private static readonly int part = 5;

                public static VisualStyleElement Active
                {
                    get
                    {
                        if (active == null)
                        {
                            active = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return active;
                    }
                }

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Inactive
                {
                    get
                    {
                        if (inactive == null)
                        {
                            inactive = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return inactive;
                    }
                }
            }

            public static class MdiCloseButton
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 20;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class MdiHelpButton
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 0x18;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class MdiMinButton
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 0x10;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class MdiRestoreButton
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 0x16;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class MdiSysButton
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 14;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class MinButton
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 15;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class MinCaption
            {
                private static VisualStyleElement active;
                private static VisualStyleElement disabled;
                private static VisualStyleElement inactive;
                private static readonly int part = 3;

                public static VisualStyleElement Active
                {
                    get
                    {
                        if (active == null)
                        {
                            active = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return active;
                    }
                }

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Inactive
                {
                    get
                    {
                        if (inactive == null)
                        {
                            inactive = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return inactive;
                    }
                }
            }

            public static class RestoreButton
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 0x15;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class SmallCaption
            {
                private static VisualStyleElement active;
                private static VisualStyleElement disabled;
                private static VisualStyleElement inactive;
                private static readonly int part = 2;

                public static VisualStyleElement Active
                {
                    get
                    {
                        if (active == null)
                        {
                            active = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return active;
                    }
                }

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Inactive
                {
                    get
                    {
                        if (inactive == null)
                        {
                            inactive = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return inactive;
                    }
                }
            }

            public static class SmallCaptionSizingTemplate
            {
                private static VisualStyleElement normal;
                private static readonly int part = 0x1f;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class SmallCloseButton
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 0x13;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class SmallFrameBottom
            {
                private static VisualStyleElement active;
                private static VisualStyleElement inactive;
                private static readonly int part = 12;

                public static VisualStyleElement Active
                {
                    get
                    {
                        if (active == null)
                        {
                            active = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return active;
                    }
                }

                public static VisualStyleElement Inactive
                {
                    get
                    {
                        if (inactive == null)
                        {
                            inactive = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return inactive;
                    }
                }
            }

            public static class SmallFrameBottomSizingTemplate
            {
                private static VisualStyleElement normal;
                private static readonly int part = 0x25;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class SmallFrameLeft
            {
                private static VisualStyleElement active;
                private static VisualStyleElement inactive;
                private static readonly int part = 10;

                public static VisualStyleElement Active
                {
                    get
                    {
                        if (active == null)
                        {
                            active = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return active;
                    }
                }

                public static VisualStyleElement Inactive
                {
                    get
                    {
                        if (inactive == null)
                        {
                            inactive = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return inactive;
                    }
                }
            }

            public static class SmallFrameLeftSizingTemplate
            {
                private static VisualStyleElement normal;
                private static readonly int part = 0x21;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class SmallFrameRight
            {
                private static VisualStyleElement active;
                private static VisualStyleElement inactive;
                private static readonly int part = 11;

                public static VisualStyleElement Active
                {
                    get
                    {
                        if (active == null)
                        {
                            active = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return active;
                    }
                }

                public static VisualStyleElement Inactive
                {
                    get
                    {
                        if (inactive == null)
                        {
                            inactive = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return inactive;
                    }
                }
            }

            public static class SmallFrameRightSizingTemplate
            {
                private static VisualStyleElement normal;
                private static readonly int part = 0x23;

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 0);
                        }
                        return normal;
                    }
                }
            }

            public static class SmallMaxCaption
            {
                private static VisualStyleElement active;
                private static VisualStyleElement disabled;
                private static VisualStyleElement inactive;
                private static readonly int part = 6;

                public static VisualStyleElement Active
                {
                    get
                    {
                        if (active == null)
                        {
                            active = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return active;
                    }
                }

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Inactive
                {
                    get
                    {
                        if (inactive == null)
                        {
                            inactive = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return inactive;
                    }
                }
            }

            public static class SmallMinCaption
            {
                private static VisualStyleElement active;
                private static VisualStyleElement disabled;
                private static VisualStyleElement inactive;
                private static readonly int part = 4;

                public static VisualStyleElement Active
                {
                    get
                    {
                        if (active == null)
                        {
                            active = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return active;
                    }
                }

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Inactive
                {
                    get
                    {
                        if (inactive == null)
                        {
                            inactive = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return inactive;
                    }
                }
            }

            public static class SysButton
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 13;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class VerticalScroll
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 0x1b;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }

            public static class VerticalThumb
            {
                private static VisualStyleElement disabled;
                private static VisualStyleElement hot;
                private static VisualStyleElement normal;
                private static readonly int part = 0x1c;
                private static VisualStyleElement pressed;

                public static VisualStyleElement Disabled
                {
                    get
                    {
                        if (disabled == null)
                        {
                            disabled = new VisualStyleElement(VisualStyleElement.Window.className, part, 4);
                        }
                        return disabled;
                    }
                }

                public static VisualStyleElement Hot
                {
                    get
                    {
                        if (hot == null)
                        {
                            hot = new VisualStyleElement(VisualStyleElement.Window.className, part, 2);
                        }
                        return hot;
                    }
                }

                public static VisualStyleElement Normal
                {
                    get
                    {
                        if (normal == null)
                        {
                            normal = new VisualStyleElement(VisualStyleElement.Window.className, part, 1);
                        }
                        return normal;
                    }
                }

                public static VisualStyleElement Pressed
                {
                    get
                    {
                        if (pressed == null)
                        {
                            pressed = new VisualStyleElement(VisualStyleElement.Window.className, part, 3);
                        }
                        return pressed;
                    }
                }
            }
        }
    }
}

