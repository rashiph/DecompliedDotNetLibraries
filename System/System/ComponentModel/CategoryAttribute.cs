namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.All)]
    public class CategoryAttribute : Attribute
    {
        private static CategoryAttribute action;
        private static CategoryAttribute appearance;
        private static CategoryAttribute asynchronous;
        private static CategoryAttribute behavior;
        private string categoryValue;
        private static CategoryAttribute data;
        private static CategoryAttribute defAttr;
        private static CategoryAttribute design;
        private static CategoryAttribute dragDrop;
        private static CategoryAttribute focus;
        private static CategoryAttribute format;
        private static CategoryAttribute key;
        private static CategoryAttribute layout;
        private bool localized;
        private static CategoryAttribute mouse;
        private static CategoryAttribute windowStyle;

        public CategoryAttribute() : this("Default")
        {
        }

        public CategoryAttribute(string category)
        {
            this.categoryValue = category;
            this.localized = false;
        }

        public override bool Equals(object obj)
        {
            return ((obj == this) || ((obj is CategoryAttribute) && this.Category.Equals(((CategoryAttribute) obj).Category)));
        }

        public override int GetHashCode()
        {
            return this.Category.GetHashCode();
        }

        protected virtual string GetLocalizedString(string value)
        {
            return (string) SR.GetObject("PropertyCategory" + value);
        }

        public override bool IsDefaultAttribute()
        {
            return this.Category.Equals(Default.Category);
        }

        public static CategoryAttribute Action
        {
            get
            {
                if (action == null)
                {
                    action = new CategoryAttribute("Action");
                }
                return action;
            }
        }

        public static CategoryAttribute Appearance
        {
            get
            {
                if (appearance == null)
                {
                    appearance = new CategoryAttribute("Appearance");
                }
                return appearance;
            }
        }

        public static CategoryAttribute Asynchronous
        {
            get
            {
                if (asynchronous == null)
                {
                    asynchronous = new CategoryAttribute("Asynchronous");
                }
                return asynchronous;
            }
        }

        public static CategoryAttribute Behavior
        {
            get
            {
                if (behavior == null)
                {
                    behavior = new CategoryAttribute("Behavior");
                }
                return behavior;
            }
        }

        public string Category
        {
            get
            {
                if (!this.localized)
                {
                    this.localized = true;
                    string localizedString = this.GetLocalizedString(this.categoryValue);
                    if (localizedString != null)
                    {
                        this.categoryValue = localizedString;
                    }
                }
                return this.categoryValue;
            }
        }

        public static CategoryAttribute Data
        {
            get
            {
                if (data == null)
                {
                    data = new CategoryAttribute("Data");
                }
                return data;
            }
        }

        public static CategoryAttribute Default
        {
            get
            {
                if (defAttr == null)
                {
                    defAttr = new CategoryAttribute();
                }
                return defAttr;
            }
        }

        public static CategoryAttribute Design
        {
            get
            {
                if (design == null)
                {
                    design = new CategoryAttribute("Design");
                }
                return design;
            }
        }

        public static CategoryAttribute DragDrop
        {
            get
            {
                if (dragDrop == null)
                {
                    dragDrop = new CategoryAttribute("DragDrop");
                }
                return dragDrop;
            }
        }

        public static CategoryAttribute Focus
        {
            get
            {
                if (focus == null)
                {
                    focus = new CategoryAttribute("Focus");
                }
                return focus;
            }
        }

        public static CategoryAttribute Format
        {
            get
            {
                if (format == null)
                {
                    format = new CategoryAttribute("Format");
                }
                return format;
            }
        }

        public static CategoryAttribute Key
        {
            get
            {
                if (key == null)
                {
                    key = new CategoryAttribute("Key");
                }
                return key;
            }
        }

        public static CategoryAttribute Layout
        {
            get
            {
                if (layout == null)
                {
                    layout = new CategoryAttribute("Layout");
                }
                return layout;
            }
        }

        public static CategoryAttribute Mouse
        {
            get
            {
                if (mouse == null)
                {
                    mouse = new CategoryAttribute("Mouse");
                }
                return mouse;
            }
        }

        public static CategoryAttribute WindowStyle
        {
            get
            {
                if (windowStyle == null)
                {
                    windowStyle = new CategoryAttribute("WindowStyle");
                }
                return windowStyle;
            }
        }
    }
}

