namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class ToolStripSettingsManager
    {
        private Form form;
        private string formKey;

        internal ToolStripSettingsManager(Form owner, string formKey)
        {
            this.form = owner;
            this.formKey = formKey;
        }

        private void ApplySettings(ArrayList toolStripSettingsToApply)
        {
            if (toolStripSettingsToApply.Count != 0)
            {
                this.SuspendAllLayout(this.form);
                Dictionary<string, ToolStrip> itemLocationHash = this.BuildItemOriginationHash();
                Dictionary<object, List<SettingsStub>> dictionary2 = new Dictionary<object, List<SettingsStub>>();
                foreach (SettingsStub stub in toolStripSettingsToApply)
                {
                    object key = !string.IsNullOrEmpty(stub.ToolStripPanelName) ? stub.ToolStripPanelName : null;
                    if (key == null)
                    {
                        if (!string.IsNullOrEmpty(stub.Name))
                        {
                            ToolStrip toolStrip = ToolStripManager.FindToolStrip(this.form, stub.Name);
                            this.ApplyToolStripSettings(toolStrip, stub, itemLocationHash);
                        }
                    }
                    else
                    {
                        if (!dictionary2.ContainsKey(key))
                        {
                            dictionary2[key] = new List<SettingsStub>();
                        }
                        dictionary2[key].Add(stub);
                    }
                }
                foreach (ToolStripPanel panel in this.FindToolStripPanels(true, this.form.Controls))
                {
                    foreach (Control control in panel.Controls)
                    {
                        control.Visible = false;
                    }
                    string name = panel.Name;
                    if ((string.IsNullOrEmpty(name) && (panel.Parent is ToolStripContainer)) && !string.IsNullOrEmpty(panel.Parent.Name))
                    {
                        name = panel.Parent.Name + "." + panel.Dock.ToString();
                    }
                    panel.BeginInit();
                    if (dictionary2.ContainsKey(name))
                    {
                        List<SettingsStub> list2 = dictionary2[name];
                        if (list2 != null)
                        {
                            foreach (SettingsStub stub2 in list2)
                            {
                                if (!string.IsNullOrEmpty(stub2.Name))
                                {
                                    ToolStrip strip2 = ToolStripManager.FindToolStrip(this.form, stub2.Name);
                                    this.ApplyToolStripSettings(strip2, stub2, itemLocationHash);
                                    panel.Join(strip2, stub2.Location);
                                }
                            }
                        }
                    }
                    panel.EndInit();
                }
                this.ResumeAllLayout(this.form, true);
            }
        }

        private void ApplyToolStripSettings(ToolStrip toolStrip, SettingsStub settings, Dictionary<string, ToolStrip> itemLocationHash)
        {
            if (toolStrip != null)
            {
                toolStrip.Visible = settings.Visible;
                toolStrip.Size = settings.Size;
                string itemOrder = settings.ItemOrder;
                if (!string.IsNullOrEmpty(itemOrder))
                {
                    string[] strArray = itemOrder.Split(new char[] { ',' });
                    Regex regex = new Regex(@"(\S+)");
                    for (int i = 0; (i < toolStrip.Items.Count) && (i < strArray.Length); i++)
                    {
                        Match match = regex.Match(strArray[i]);
                        if ((match != null) && match.Success)
                        {
                            string str2 = match.Value;
                            if (!string.IsNullOrEmpty(str2) && itemLocationHash.ContainsKey(str2))
                            {
                                toolStrip.Items.Insert(i, itemLocationHash[str2].Items[str2]);
                            }
                        }
                    }
                }
            }
        }

        private Dictionary<string, ToolStrip> BuildItemOriginationHash()
        {
            ArrayList list = this.FindToolStrips(true, this.form.Controls);
            Dictionary<string, ToolStrip> dictionary = new Dictionary<string, ToolStrip>();
            if (list != null)
            {
                foreach (ToolStrip strip in list)
                {
                    foreach (ToolStripItem item in strip.Items)
                    {
                        if (!string.IsNullOrEmpty(item.Name))
                        {
                            dictionary[item.Name] = strip;
                        }
                    }
                }
            }
            return dictionary;
        }

        private ArrayList FindControls(System.Type baseType, bool searchAllChildren, Control.ControlCollection controlsToLookIn, ArrayList foundControls)
        {
            if ((controlsToLookIn == null) || (foundControls == null))
            {
                return null;
            }
            try
            {
                for (int i = 0; i < controlsToLookIn.Count; i++)
                {
                    if ((controlsToLookIn[i] != null) && baseType.IsAssignableFrom(controlsToLookIn[i].GetType()))
                    {
                        foundControls.Add(controlsToLookIn[i]);
                    }
                }
                if (!searchAllChildren)
                {
                    return foundControls;
                }
                for (int j = 0; j < controlsToLookIn.Count; j++)
                {
                    if (((controlsToLookIn[j] != null) && !(controlsToLookIn[j] is Form)) && ((controlsToLookIn[j].Controls != null) && (controlsToLookIn[j].Controls.Count > 0)))
                    {
                        foundControls = this.FindControls(baseType, searchAllChildren, controlsToLookIn[j].Controls, foundControls);
                    }
                }
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                {
                    throw;
                }
            }
            return foundControls;
        }

        private ArrayList FindToolStripPanels(bool searchAllChildren, Control.ControlCollection controlsToLookIn)
        {
            return this.FindControls(typeof(ToolStripPanel), true, this.form.Controls, new ArrayList());
        }

        private ArrayList FindToolStrips(bool searchAllChildren, Control.ControlCollection controlsToLookIn)
        {
            return this.FindControls(typeof(ToolStrip), true, this.form.Controls, new ArrayList());
        }

        internal static string GetItemOrder(ToolStrip toolStrip)
        {
            StringBuilder builder = new StringBuilder(toolStrip.Items.Count);
            for (int i = 0; i < toolStrip.Items.Count; i++)
            {
                builder.Append((toolStrip.Items[i].Name == null) ? "null" : toolStrip.Items[i].Name);
                if (i != (toolStrip.Items.Count - 1))
                {
                    builder.Append(",");
                }
            }
            return builder.ToString();
        }

        private string GetSettingsKey(ToolStrip toolStrip)
        {
            if (toolStrip != null)
            {
                return (this.formKey + "." + toolStrip.Name);
            }
            return string.Empty;
        }

        internal void Load()
        {
            ArrayList toolStripSettingsToApply = new ArrayList();
            foreach (ToolStrip strip in this.FindToolStrips(true, this.form.Controls))
            {
                if ((strip != null) && !string.IsNullOrEmpty(strip.Name))
                {
                    ToolStripSettings toolStripSettings = new ToolStripSettings(this.GetSettingsKey(strip));
                    if (!toolStripSettings.IsDefault)
                    {
                        toolStripSettingsToApply.Add(new SettingsStub(toolStripSettings));
                    }
                }
            }
            this.ApplySettings(toolStripSettingsToApply);
        }

        private void ResumeAllLayout(Control start, bool performLayout)
        {
            Control.ControlCollection controls = start.Controls;
            for (int i = 0; i < controls.Count; i++)
            {
                this.ResumeAllLayout(controls[i], performLayout);
            }
            start.ResumeLayout(performLayout);
        }

        internal void Save()
        {
            foreach (ToolStrip strip in this.FindToolStrips(true, this.form.Controls))
            {
                if ((strip != null) && !string.IsNullOrEmpty(strip.Name))
                {
                    ToolStripSettings settings = new ToolStripSettings(this.GetSettingsKey(strip));
                    SettingsStub stub = new SettingsStub(strip);
                    settings.ItemOrder = stub.ItemOrder;
                    settings.Name = stub.Name;
                    settings.Location = stub.Location;
                    settings.Size = stub.Size;
                    settings.ToolStripPanelName = stub.ToolStripPanelName;
                    settings.Visible = stub.Visible;
                    settings.Save();
                }
            }
        }

        private void SuspendAllLayout(Control start)
        {
            start.SuspendLayout();
            Control.ControlCollection controls = start.Controls;
            for (int i = 0; i < controls.Count; i++)
            {
                this.SuspendAllLayout(controls[i]);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SettingsStub
        {
            public bool Visible;
            public string ToolStripPanelName;
            public Point Location;
            public System.Drawing.Size Size;
            public string ItemOrder;
            public string Name;
            public SettingsStub(ToolStrip toolStrip)
            {
                this.ToolStripPanelName = string.Empty;
                ToolStripPanel parent = toolStrip.Parent as ToolStripPanel;
                if (parent != null)
                {
                    if (!string.IsNullOrEmpty(parent.Name))
                    {
                        this.ToolStripPanelName = parent.Name;
                    }
                    else if ((parent.Parent is ToolStripContainer) && !string.IsNullOrEmpty(parent.Parent.Name))
                    {
                        this.ToolStripPanelName = parent.Parent.Name + "." + parent.Dock.ToString();
                    }
                }
                this.Visible = toolStrip.Visible;
                this.Size = toolStrip.Size;
                this.Location = toolStrip.Location;
                this.Name = toolStrip.Name;
                this.ItemOrder = ToolStripSettingsManager.GetItemOrder(toolStrip);
            }

            public SettingsStub(ToolStripSettings toolStripSettings)
            {
                this.ToolStripPanelName = toolStripSettings.ToolStripPanelName;
                this.Visible = toolStripSettings.Visible;
                this.Size = toolStripSettings.Size;
                this.Location = toolStripSettings.Location;
                this.Name = toolStripSettings.Name;
                this.ItemOrder = toolStripSettings.ItemOrder;
            }
        }
    }
}

