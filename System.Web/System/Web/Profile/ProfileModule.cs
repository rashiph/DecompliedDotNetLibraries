namespace System.Web.Profile
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;

    public sealed class ProfileModule : IHttpModule
    {
        private static object s_Lock = new object();

        public event ProfileMigrateEventHandler MigrateAnonymous;

        public event ProfileEventHandler Personalize;

        public event ProfileAutoSaveEventHandler ProfileAutoSaving;

        public void Dispose()
        {
        }

        public void Init(HttpApplication app)
        {
            if (ProfileManager.Enabled)
            {
                app.AcquireRequestState += new EventHandler(this.OnEnter);
                if (ProfileManager.AutomaticSaveEnabled)
                {
                    app.EndRequest += new EventHandler(this.OnLeave);
                }
            }
        }

        private void OnEnter(object source, EventArgs eventArgs)
        {
            HttpContext context = ((HttpApplication) source).Context;
            this.OnPersonalize(new ProfileEventArgs(context));
            if ((context.Request.IsAuthenticated && !string.IsNullOrEmpty(context.Request.AnonymousID)) && (this._MigrateEventHandler != null))
            {
                ProfileMigrateEventArgs e = new ProfileMigrateEventArgs(context, context.Request.AnonymousID);
                this._MigrateEventHandler(this, e);
            }
        }

        private void OnLeave(object source, EventArgs eventArgs)
        {
            HttpApplication application = (HttpApplication) source;
            HttpContext context = application.Context;
            if ((context._Profile != null) && (context._Profile != ProfileBase.SingletonInstance))
            {
                if (this._AutoSaveEventHandler != null)
                {
                    ProfileAutoSaveEventArgs e = new ProfileAutoSaveEventArgs(context);
                    this._AutoSaveEventHandler(this, e);
                    if (!e.ContinueWithProfileAutoSave)
                    {
                        return;
                    }
                }
                context.Profile.Save();
            }
        }

        private void OnPersonalize(ProfileEventArgs e)
        {
            if (this._eventHandler != null)
            {
                this._eventHandler(this, e);
            }
            if (e.Profile != null)
            {
                e.Context._Profile = e.Profile;
            }
            else
            {
                e.Context._ProfileDelayLoad = true;
            }
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.SerializationFormatter)]
        internal static void ParseDataFromDB(string[] names, string values, byte[] buf, SettingsPropertyValueCollection properties)
        {
            if (((names != null) && (values != null)) && ((buf != null) && (properties != null)))
            {
                try
                {
                    for (int i = 0; i < (names.Length / 4); i++)
                    {
                        string str = names[i * 4];
                        SettingsPropertyValue value2 = properties[str];
                        if (value2 != null)
                        {
                            int startIndex = int.Parse(names[(i * 4) + 2], CultureInfo.InvariantCulture);
                            int length = int.Parse(names[(i * 4) + 3], CultureInfo.InvariantCulture);
                            if ((length == -1) && !value2.Property.PropertyType.IsValueType)
                            {
                                value2.PropertyValue = null;
                                value2.IsDirty = false;
                                value2.Deserialized = true;
                            }
                            if (((names[(i * 4) + 1] == "S") && (startIndex >= 0)) && ((length > 0) && (values.Length >= (startIndex + length))))
                            {
                                value2.SerializedValue = values.Substring(startIndex, length);
                            }
                            if (((names[(i * 4) + 1] == "B") && (startIndex >= 0)) && ((length > 0) && (buf.Length >= (startIndex + length))))
                            {
                                byte[] dst = new byte[length];
                                Buffer.BlockCopy(buf, startIndex, dst, 0, length);
                                value2.SerializedValue = dst;
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.SerializationFormatter)]
        internal static void PrepareDataForSaving(ref string allNames, ref string allValues, ref byte[] buf, bool binarySupported, SettingsPropertyValueCollection properties, bool userIsAuthenticated)
        {
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            MemoryStream stream = binarySupported ? new MemoryStream() : null;
            try
            {
                try
                {
                    bool flag = false;
                    foreach (SettingsPropertyValue value2 in properties)
                    {
                        if (value2.IsDirty && (userIsAuthenticated || ((bool) value2.Property.Attributes["AllowAnonymous"])))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        return;
                    }
                    foreach (SettingsPropertyValue value3 in properties)
                    {
                        if ((userIsAuthenticated || ((bool) value3.Property.Attributes["AllowAnonymous"])) && (value3.IsDirty || !value3.UsingDefaultValue))
                        {
                            int length = 0;
                            int position = 0;
                            string str = null;
                            if (value3.Deserialized && (value3.PropertyValue == null))
                            {
                                length = -1;
                            }
                            else
                            {
                                object serializedValue = value3.SerializedValue;
                                if (serializedValue == null)
                                {
                                    length = -1;
                                }
                                else
                                {
                                    if (!(serializedValue is string) && !binarySupported)
                                    {
                                        serializedValue = Convert.ToBase64String((byte[]) serializedValue);
                                    }
                                    if (serializedValue is string)
                                    {
                                        str = (string) serializedValue;
                                        length = str.Length;
                                        position = builder2.Length;
                                    }
                                    else
                                    {
                                        byte[] buffer = (byte[]) serializedValue;
                                        position = (int) stream.Position;
                                        stream.Write(buffer, 0, buffer.Length);
                                        stream.Position = position + buffer.Length;
                                        length = buffer.Length;
                                    }
                                }
                            }
                            builder.Append(value3.Name + ":" + ((str != null) ? "S" : "B") + ":" + position.ToString(CultureInfo.InvariantCulture) + ":" + length.ToString(CultureInfo.InvariantCulture) + ":");
                            if (str != null)
                            {
                                builder2.Append(str);
                            }
                        }
                    }
                    if (binarySupported)
                    {
                        buf = stream.ToArray();
                    }
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                }
            }
            catch
            {
                throw;
            }
            allNames = builder.ToString();
            allValues = builder2.ToString();
        }
    }
}

