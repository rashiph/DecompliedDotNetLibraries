namespace Microsoft.VisualBasic.Devices
{
    using Microsoft.VisualBasic;
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Media;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class Audio
    {
        private SoundPlayer m_Sound;

        [SecuritySafeCritical]
        private static void InternalStop(SoundPlayer sound)
        {
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
            try
            {
                sound.Stop();
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Play(string location)
        {
            this.Play(location, AudioPlayMode.Background);
        }

        public void Play(string location, AudioPlayMode playMode)
        {
            this.ValidateAudioPlayModeEnum(playMode, "playMode");
            SoundPlayer sound = new SoundPlayer(this.ValidateFilename(location));
            this.Play(sound, playMode);
        }

        public void Play(byte[] data, AudioPlayMode playMode)
        {
            if (data == null)
            {
                throw ExceptionUtils.GetArgumentNullException("data");
            }
            this.ValidateAudioPlayModeEnum(playMode, "playMode");
            MemoryStream stream = new MemoryStream(data);
            this.Play(stream, playMode);
            stream.Close();
        }

        public void Play(Stream stream, AudioPlayMode playMode)
        {
            this.ValidateAudioPlayModeEnum(playMode, "playMode");
            if (stream == null)
            {
                throw ExceptionUtils.GetArgumentNullException("stream");
            }
            this.Play(new SoundPlayer(stream), playMode);
        }

        private void Play(SoundPlayer sound, AudioPlayMode mode)
        {
            if (this.m_Sound != null)
            {
                InternalStop(this.m_Sound);
            }
            this.m_Sound = sound;
            switch (mode)
            {
                case AudioPlayMode.WaitToComplete:
                    this.m_Sound.PlaySync();
                    break;

                case AudioPlayMode.Background:
                    this.m_Sound.Play();
                    break;

                case AudioPlayMode.BackgroundLoop:
                    this.m_Sound.PlayLooping();
                    break;
            }
        }

        public void PlaySystemSound(SystemSound systemSound)
        {
            if (systemSound == null)
            {
                throw ExceptionUtils.GetArgumentNullException("systemSound");
            }
            systemSound.Play();
        }

        public void Stop()
        {
            SoundPlayer sound = new SoundPlayer();
            InternalStop(sound);
        }

        private void ValidateAudioPlayModeEnum(AudioPlayMode value, string paramName)
        {
            if ((value < AudioPlayMode.WaitToComplete) || (value > AudioPlayMode.BackgroundLoop))
            {
                throw new InvalidEnumArgumentException(paramName, (int) value, typeof(AudioPlayMode));
            }
        }

        private string ValidateFilename(string location)
        {
            if (location == "")
            {
                throw ExceptionUtils.GetArgumentNullException("location");
            }
            return location;
        }
    }
}

