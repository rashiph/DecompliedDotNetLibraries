namespace Microsoft.VisualBasic.MyServices
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [EditorBrowsable(EditorBrowsableState.Never), HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class ClipboardProxy
    {
        internal ClipboardProxy()
        {
        }

        public void Clear()
        {
            Clipboard.Clear();
        }

        public bool ContainsAudio()
        {
            return Clipboard.ContainsAudio();
        }

        public bool ContainsData(string format)
        {
            return Clipboard.ContainsData(format);
        }

        public bool ContainsFileDropList()
        {
            return Clipboard.ContainsFileDropList();
        }

        public bool ContainsImage()
        {
            return Clipboard.ContainsImage();
        }

        public bool ContainsText()
        {
            return Clipboard.ContainsText();
        }

        public bool ContainsText(TextDataFormat format)
        {
            return Clipboard.ContainsText(format);
        }

        public Stream GetAudioStream()
        {
            return Clipboard.GetAudioStream();
        }

        public object GetData(string format)
        {
            return Clipboard.GetData(format);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public IDataObject GetDataObject()
        {
            return Clipboard.GetDataObject();
        }

        public StringCollection GetFileDropList()
        {
            return Clipboard.GetFileDropList();
        }

        public Image GetImage()
        {
            return Clipboard.GetImage();
        }

        public string GetText()
        {
            return Clipboard.GetText();
        }

        public string GetText(TextDataFormat format)
        {
            return Clipboard.GetText(format);
        }

        public void SetAudio(byte[] audioBytes)
        {
            Clipboard.SetAudio(audioBytes);
        }

        public void SetAudio(Stream audioStream)
        {
            Clipboard.SetAudio(audioStream);
        }

        public void SetData(string format, object data)
        {
            Clipboard.SetData(format, data);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void SetDataObject(DataObject data)
        {
            Clipboard.SetDataObject(data);
        }

        public void SetFileDropList(StringCollection filePaths)
        {
            Clipboard.SetFileDropList(filePaths);
        }

        public void SetImage(Image image)
        {
            Clipboard.SetImage(image);
        }

        public void SetText(string text)
        {
            Clipboard.SetText(text);
        }

        public void SetText(string text, TextDataFormat format)
        {
            Clipboard.SetText(text, format);
        }
    }
}

