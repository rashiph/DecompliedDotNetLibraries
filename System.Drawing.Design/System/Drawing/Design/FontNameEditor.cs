namespace System.Drawing.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class FontNameEditor : UITypeEditor
    {
        private static void DrawFontSample(PaintValueEventArgs e, FontFamily fontFamily, FontStyle fontStyle)
        {
            float emSize = (float) (((double) e.Bounds.Height) / 1.2);
            Font font = new Font(fontFamily, emSize, fontStyle, GraphicsUnit.Pixel);
            if (font != null)
            {
                try
                {
                    e.Graphics.DrawString("abcd", font, SystemBrushes.ActiveCaptionText, e.Bounds);
                }
                finally
                {
                    font.Dispose();
                }
            }
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            string name = e.Value as string;
            switch (name)
            {
                case null:
                    break;

                case "":
                    return;

                default:
                {
                    e.Graphics.FillRectangle(SystemBrushes.ActiveCaption, e.Bounds);
                    FontFamily fontFamily = null;
                    try
                    {
                        fontFamily = new FontFamily(name);
                    }
                    catch
                    {
                    }
                    if (fontFamily != null)
                    {
                        try
                        {
                            DrawFontSample(e, fontFamily, FontStyle.Regular);
                        }
                        catch
                        {
                            try
                            {
                                DrawFontSample(e, fontFamily, FontStyle.Italic);
                            }
                            catch
                            {
                                try
                                {
                                    DrawFontSample(e, fontFamily, FontStyle.Bold);
                                }
                                catch
                                {
                                    try
                                    {
                                        DrawFontSample(e, fontFamily, FontStyle.Italic | FontStyle.Bold);
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                        finally
                        {
                            fontFamily.Dispose();
                        }
                    }
                    e.Graphics.DrawLine(SystemPens.WindowFrame, e.Bounds.Right, e.Bounds.Y, e.Bounds.Right, e.Bounds.Bottom);
                    break;
                }
            }
        }
    }
}

