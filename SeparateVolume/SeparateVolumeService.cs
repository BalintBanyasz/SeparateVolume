using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SeparateVolume
{
    public partial class SeparateVolumeService : ServiceBase, NAudio.CoreAudioApi.Interfaces.IMMNotificationClient
    {
        public const string SUBKEY_NAME = "Software\\SeparateVolume";
        public const string KEY_MASTER_VOLUME_LEVEL = "MasterVolumeLevel";
        public const string KEY_MUTE = "Mute";

        private NAudio.CoreAudioApi.MMDeviceEnumerator deviceEnum;
        private NAudio.CoreAudioApi.Interfaces.IMMNotificationClient notifyClient;

        private VolumeStatus previousVolume;

        public SeparateVolumeService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            deviceEnum = new NAudio.CoreAudioApi.MMDeviceEnumerator();
            notifyClient = (NAudio.CoreAudioApi.Interfaces.IMMNotificationClient)this;
            deviceEnum.RegisterEndpointNotificationCallback(notifyClient);

            previousVolume = GetSavedVolumeStatus();
        }

        private class VolumeStatus
        {
            public float? MasterVolume { get; set; }
            public bool? Mute { get; set; }

            public VolumeStatus()
            {
                this.MasterVolume = null;
                this.Mute = null;
            }

            public VolumeStatus(float? masterVolume, bool? mute)
            {
                this.MasterVolume = masterVolume;
                this.Mute = mute;
            }
        }

        private VolumeStatus GetSavedVolumeStatus()
        {
            try
            {
                VolumeStatus volumeInfo = new VolumeStatus();
                Microsoft.Win32.RegistryKey key;
                key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(SUBKEY_NAME, false);
                if (key != null)
                {
                    float volume;
                    bool mute;

                    string volumeStr = (string)key.GetValue(KEY_MASTER_VOLUME_LEVEL, null);
                    string muteStr = (string)key.GetValue(KEY_MUTE, null);
                    key.Close();

                    if (float.TryParse(volumeStr, out volume))
                        volumeInfo.MasterVolume = volume;
                    if (bool.TryParse(muteStr, out mute))
                        volumeInfo.Mute = mute;
                }
                return volumeInfo;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void SetSavedVolumeStatus(VolumeStatus volumeInfo)
        {
            try
            {
                Microsoft.Win32.RegistryKey key;
                key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(SUBKEY_NAME, true);
                if (key == null)
                {
                    key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(SUBKEY_NAME);
                }

                if (key != null)
                {
                    if (volumeInfo.MasterVolume != null)
                        key.SetValue(KEY_MASTER_VOLUME_LEVEL, volumeInfo.MasterVolume);
                    if (volumeInfo.Mute != null)
                        key.SetValue(KEY_MUTE, volumeInfo.Mute);
                    key.Close();
                }
            }
            catch (Exception)
            {
                //
            }
        }

        protected override void OnStop()
        {
            if (deviceEnum != null)
            {
                deviceEnum.UnregisterEndpointNotificationCallback(notifyClient);
                notifyClient = null;
                deviceEnum = null;
            }
        }

        public void OnDefaultDeviceChanged(NAudio.CoreAudioApi.DataFlow flow, NAudio.CoreAudioApi.Role role, string defaultDeviceId)
        {
            //throw new NotImplementedException();
        }

        public void OnDeviceAdded(string pwstrDeviceId)
        {
            //throw new NotImplementedException();
        }

        public void OnDeviceRemoved(string deviceId)
        {
            //throw new NotImplementedException();
        }

        public void OnDeviceStateChanged(string deviceId, NAudio.CoreAudioApi.DeviceState newState)
        {
            if (newState == NAudio.CoreAudioApi.DeviceState.Active || newState == NAudio.CoreAudioApi.DeviceState.Unplugged)
            {
                // Get current volume level
                NAudio.CoreAudioApi.MMDeviceEnumerator deviceEnum = new NAudio.CoreAudioApi.MMDeviceEnumerator();
                NAudio.CoreAudioApi.MMDevice device = deviceEnum.EnumerateAudioEndPoints(NAudio.CoreAudioApi.DataFlow.Render, NAudio.CoreAudioApi.DeviceState.All).FirstOrDefault(dev => dev.DeviceFriendlyName.Equals("Realtek High Definition Audio"));
                
                if (device != null)
	            {
		            VolumeStatus currentVolume = new VolumeStatus(device.AudioEndpointVolume.MasterVolumeLevel, device.AudioEndpointVolume.Mute);

                    // Restore previous volume level
                    if (previousVolume.MasterVolume.HasValue)
                        device.AudioEndpointVolume.MasterVolumeLevel = previousVolume.MasterVolume.Value;
                    if (previousVolume.Mute.HasValue)
                        device.AudioEndpointVolume.Mute = previousVolume.Mute.Value;

                    // Save previous volume level
                    previousVolume = currentVolume;
                    SetSavedVolumeStatus(currentVolume);
	            }
            }
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, NAudio.CoreAudioApi.PropertyKey key)
        {
            //throw new NotImplementedException();
        }
    }
}
