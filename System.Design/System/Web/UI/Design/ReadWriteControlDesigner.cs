namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.UI.WebControls;

    [Obsolete("The recommended alternative is ContainerControlDesigner because it uses an EditableDesignerRegion for editing the content. Designer regions allow for better control of the content being edited. http://go.microsoft.com/fwlink/?linkid=14202"), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ReadWriteControlDesigner : ControlDesigner
    {
        public ReadWriteControlDesigner()
        {
            base.ReadOnlyInternal = false;
        }

        public override string GetDesignTimeHtml()
        {
            return base.CreatePlaceHolderDesignTimeHtml();
        }

        protected virtual void MapPropertyToStyle(string propName, object varPropValue)
        {
            if ((this.BehaviorInternal != null) && ((propName != null) && (varPropValue != null)))
            {
                try
                {
                    if (propName.Equals("BackColor"))
                    {
                        this.BehaviorInternal.SetStyleAttribute("backgroundColor", true, varPropValue, true);
                    }
                    else if (propName.Equals("ForeColor"))
                    {
                        this.BehaviorInternal.SetStyleAttribute("color", true, varPropValue, true);
                    }
                    else if (propName.Equals("BorderWidth"))
                    {
                        string str = Convert.ToString(varPropValue, CultureInfo.InvariantCulture);
                        this.BehaviorInternal.SetStyleAttribute("borderWidth", true, str, true);
                    }
                    else if (propName.Equals("BorderStyle"))
                    {
                        string str2;
                        if (((BorderStyle) varPropValue) == BorderStyle.NotSet)
                        {
                            str2 = string.Empty;
                        }
                        else
                        {
                            str2 = Enum.Format(typeof(BorderStyle), (BorderStyle) varPropValue, "G");
                        }
                        this.BehaviorInternal.SetStyleAttribute("borderStyle", true, str2, true);
                    }
                    else if (propName.Equals("BorderColor"))
                    {
                        this.BehaviorInternal.SetStyleAttribute("borderColor", true, Convert.ToString(varPropValue, CultureInfo.InvariantCulture), true);
                    }
                    else if (propName.Equals("Height"))
                    {
                        this.BehaviorInternal.SetStyleAttribute("height", true, Convert.ToString(varPropValue, CultureInfo.InvariantCulture), true);
                    }
                    else if (propName.Equals("Width"))
                    {
                        this.BehaviorInternal.SetStyleAttribute("width", true, Convert.ToString(varPropValue, CultureInfo.InvariantCulture), true);
                    }
                    else if (propName.Equals("Font.Name"))
                    {
                        this.BehaviorInternal.SetStyleAttribute("fontFamily", true, Convert.ToString(varPropValue, CultureInfo.InvariantCulture), true);
                    }
                    else if (propName.Equals("Font.Size"))
                    {
                        this.BehaviorInternal.SetStyleAttribute("fontSize", true, Convert.ToString(varPropValue, CultureInfo.InvariantCulture), true);
                    }
                    else if (propName.Equals("Font.Bold"))
                    {
                        string str3;
                        if ((bool) varPropValue)
                        {
                            str3 = "bold";
                        }
                        else
                        {
                            str3 = "normal";
                        }
                        this.BehaviorInternal.SetStyleAttribute("fontWeight", true, str3, true);
                    }
                    else if (propName.Equals("Font.Italic"))
                    {
                        string str4;
                        if ((bool) varPropValue)
                        {
                            str4 = "italic";
                        }
                        else
                        {
                            str4 = "normal";
                        }
                        this.BehaviorInternal.SetStyleAttribute("fontStyle", true, str4, true);
                    }
                    else if (propName.Equals("Font.Underline"))
                    {
                        string str5 = (string) this.BehaviorInternal.GetStyleAttribute("textDecoration", true, true);
                        if ((bool) varPropValue)
                        {
                            if (str5 == null)
                            {
                                str5 = "underline";
                            }
                            else if (str5.ToLower(CultureInfo.InvariantCulture).IndexOf("underline", StringComparison.Ordinal) < 0)
                            {
                                str5 = str5 + " underline";
                            }
                            this.BehaviorInternal.SetStyleAttribute("textDecoration", true, str5, true);
                        }
                        else if (str5 != null)
                        {
                            int index = str5.ToLower(CultureInfo.InvariantCulture).IndexOf("underline", StringComparison.Ordinal);
                            if (index >= 0)
                            {
                                string str6 = str5.Substring(0, index);
                                if ((index + 9) < str5.Length)
                                {
                                    str6 = " " + str5.Substring(index + 9);
                                }
                                this.BehaviorInternal.SetStyleAttribute("textDecoration", true, str6, true);
                            }
                        }
                    }
                    else if (propName.Equals("Font.Strikeout"))
                    {
                        string str7 = (string) this.BehaviorInternal.GetStyleAttribute("textDecoration", true, true);
                        if ((bool) varPropValue)
                        {
                            if (str7 == null)
                            {
                                str7 = "line-through";
                            }
                            else if (str7.ToLower(CultureInfo.InvariantCulture).IndexOf("line-through", StringComparison.Ordinal) < 0)
                            {
                                str7 = str7 + " line-through";
                            }
                            this.BehaviorInternal.SetStyleAttribute("textDecoration", true, str7, true);
                        }
                        else if (str7 != null)
                        {
                            int length = str7.ToLower(CultureInfo.InvariantCulture).IndexOf("line-through", StringComparison.Ordinal);
                            if (length >= 0)
                            {
                                string str8 = str7.Substring(0, length);
                                if ((length + 12) < str7.Length)
                                {
                                    str8 = " " + str7.Substring(length + 12);
                                }
                                this.BehaviorInternal.SetStyleAttribute("textDecoration", true, str8, true);
                            }
                        }
                    }
                    else if (propName.Equals("Font.Overline"))
                    {
                        string str9 = (string) this.BehaviorInternal.GetStyleAttribute("textDecoration", true, true);
                        if ((bool) varPropValue)
                        {
                            if (str9 == null)
                            {
                                str9 = "overline";
                            }
                            else if (str9.ToLower(CultureInfo.InvariantCulture).IndexOf("overline", StringComparison.Ordinal) < 0)
                            {
                                str9 = str9 + " overline";
                            }
                            this.BehaviorInternal.SetStyleAttribute("textDecoration", true, str9, true);
                        }
                        else if (str9 != null)
                        {
                            int num3 = str9.ToLower(CultureInfo.InvariantCulture).IndexOf("overline", StringComparison.Ordinal);
                            if (num3 >= 0)
                            {
                                string str10 = str9.Substring(0, num3);
                                if ((num3 + 8) < str9.Length)
                                {
                                    str10 = " " + str9.Substring(num3 + 8);
                                }
                                this.BehaviorInternal.SetStyleAttribute("textDecoration", true, str10, true);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        [Obsolete("The recommended alternative is ControlDesigner.Tag. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected override void OnBehaviorAttached()
        {
            base.OnBehaviorAttached();
            if (base.IsWebControl)
            {
                WebControl component = (WebControl) base.Component;
                string varPropValue = ColorTranslator.ToHtml(component.BackColor);
                if (varPropValue.Length > 0)
                {
                    this.MapPropertyToStyle("BackColor", varPropValue);
                }
                varPropValue = ColorTranslator.ToHtml(component.ForeColor);
                if (varPropValue.Length > 0)
                {
                    this.MapPropertyToStyle("ForeColor", varPropValue);
                }
                varPropValue = ColorTranslator.ToHtml(component.BorderColor);
                if (varPropValue.Length > 0)
                {
                    this.MapPropertyToStyle("BorderColor", varPropValue);
                }
                BorderStyle borderStyle = component.BorderStyle;
                if (borderStyle != BorderStyle.NotSet)
                {
                    this.MapPropertyToStyle("BorderStyle", borderStyle);
                }
                Unit borderWidth = component.BorderWidth;
                if (!borderWidth.IsEmpty && (borderWidth.Value != 0.0))
                {
                    this.MapPropertyToStyle("BorderWidth", borderWidth.ToString(CultureInfo.InvariantCulture));
                }
                Unit width = component.Width;
                if (!width.IsEmpty && (width.Value != 0.0))
                {
                    this.MapPropertyToStyle("Width", width.ToString(CultureInfo.InvariantCulture));
                }
                Unit height = component.Height;
                if (!height.IsEmpty && (height.Value != 0.0))
                {
                    this.MapPropertyToStyle("Height", height.ToString(CultureInfo.InvariantCulture));
                }
                string name = component.Font.Name;
                if (name.Length != 0)
                {
                    this.MapPropertyToStyle("Font.Name", name);
                }
                FontUnit size = component.Font.Size;
                if (size != FontUnit.Empty)
                {
                    this.MapPropertyToStyle("Font.Size", size.ToString(CultureInfo.InvariantCulture));
                }
                bool bold = component.Font.Bold;
                if (bold)
                {
                    this.MapPropertyToStyle("Font.Bold", bold);
                }
                bold = component.Font.Italic;
                if (bold)
                {
                    this.MapPropertyToStyle("Font.Italic", bold);
                }
                bold = component.Font.Underline;
                if (bold)
                {
                    this.MapPropertyToStyle("Font.Underline", bold);
                }
                bold = component.Font.Strikeout;
                if (bold)
                {
                    this.MapPropertyToStyle("Font.Strikeout", bold);
                }
                bold = component.Font.Overline;
                if (bold)
                {
                    this.MapPropertyToStyle("Font.Overline", bold);
                }
            }
        }

        public override void OnComponentChanged(object sender, ComponentChangedEventArgs ce)
        {
            base.OnComponentChanged(sender, ce);
            if (!base.IsIgnoringComponentChanges && (base.IsWebControl && (base.DesignTimeElementInternal != null)))
            {
                MemberDescriptor member = ce.Member;
                object newValue = ce.NewValue;
                Type type = Type.GetType("System.ComponentModel.ReflectPropertyDescriptor, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                if ((member != null) && (member.GetType() == type))
                {
                    PropertyDescriptor descriptor2 = (PropertyDescriptor) member;
                    if (member.Name.Equals("Font"))
                    {
                        WebControl component = (WebControl) base.Component;
                        newValue = component.Font.Name;
                        this.MapPropertyToStyle("Font.Name", newValue);
                        newValue = component.Font.Size;
                        this.MapPropertyToStyle("Font.Size", newValue);
                        newValue = component.Font.Bold;
                        this.MapPropertyToStyle("Font.Bold", newValue);
                        newValue = component.Font.Italic;
                        this.MapPropertyToStyle("Font.Italic", newValue);
                        newValue = component.Font.Underline;
                        this.MapPropertyToStyle("Font.Underline", newValue);
                        newValue = component.Font.Strikeout;
                        this.MapPropertyToStyle("Font.Strikeout", newValue);
                        newValue = component.Font.Overline;
                        this.MapPropertyToStyle("Font.Overline", newValue);
                    }
                    else if (newValue != null)
                    {
                        if (descriptor2.PropertyType == typeof(Color))
                        {
                            newValue = ColorTranslator.ToHtml((Color) newValue);
                        }
                        this.MapPropertyToStyle(descriptor2.Name, newValue);
                    }
                }
            }
        }

        public override void UpdateDesignTimeHtml()
        {
        }
    }
}

