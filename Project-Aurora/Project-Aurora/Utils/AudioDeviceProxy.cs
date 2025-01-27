﻿using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace Aurora.Utils {

    /// <summary>
    /// Utility class to make it easier to manage dealing with audio devices and input.
    /// Will handle the creation of devices if required. If another AudioDevice is using that device, they will share the same reference.
    /// Can be hot-swapped to a different device, moving all events to the newly selected device.
    /// </summary>
    public sealed class AudioDeviceProxy : IDisposable, NAudio.CoreAudioApi.Interfaces.IMMNotificationClient {

        public const string DEFAULT_DEVICE_ID = ""; // special ID to indicate the default device

        private static readonly MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();

        // Stores event handlers added to the proxy, so they can easily be added and removed from the MMDevice when it changes without
        // needing to rely on the consumer manually removing and re-adding the events.
        private EventHandler<WaveInEventArgs> waveInDataAvailable;

        // ID of currently selected device.
        private string deviceId;
        private bool defaultDeviceChanged = false;

        static AudioDeviceProxy() {
            // Tried using a static class to update the device lists when they changed, but it caused an AccessViolation. Will try look into this again in future
            //deviceEnumerator.RegisterEndpointNotificationCallback(new DeviceChangedHandler());
            RefreshDeviceLists();
        }

        /// <summary>Creates a new reference to the default audio device with the given flow direction.</summary>
        public AudioDeviceProxy(DataFlow flow) : this(DEFAULT_DEVICE_ID, flow) { }

        /// <summary>Creates a new reference to the audio device with the given ID with the given flow direction.</summary>
        public AudioDeviceProxy(string deviceId, DataFlow flow) {
            Flow = flow;
            DeviceId = deviceId ?? DEFAULT_DEVICE_ID;
            deviceEnumerator.RegisterEndpointNotificationCallback(this);
        }

        /// <summary>Indicates recorded data is available on the selected device.</summary>
        /// <remarks>This event is automatically reassigned to the new device when it is swapped.</remarks>
        public event EventHandler<WaveInEventArgs> WaveInDataAvailable {
            add {
                waveInDataAvailable += value; // Update stored event listeners
                if (WaveIn != null) WaveIn.DataAvailable += value; // If the device is valid, pass the event handler on
            }
            remove {
                waveInDataAvailable -= value; // Update stored event listeners
                if (WaveIn != null) WaveIn.DataAvailable -= value; // If the device is valid, pass the event handler on
            }
        }

        public MMDevice Device { get; private set; }
        public WasapiCapture WaveIn { get; private set; }

        /// <summary>Gets the currently assigned direction of this device.</summary>
        public DataFlow Flow { get; }

        /// <summary>Gets or sets the ID of the selected device.</summary>
        public string DeviceId {
            get => deviceId;
            set {
                value ??= DEFAULT_DEVICE_ID; // Ensure not-null (if null, assume default device)
                if (deviceId == value && !(defaultDeviceChanged && deviceId == DEFAULT_DEVICE_ID)) return;
                defaultDeviceChanged = false;
                deviceId = value;
                UpdateDevice();
            }
        }

        /// <summary>Gets a new MMDevice and wave in based on the current <see cref="DeviceId"/> and <see cref="Flow"/></summary>
        private void UpdateDevice() {
            // Release the current device (if any), removing any events as required
            if (WaveIn != null)
                WaveIn.DataAvailable -= waveInDataAvailable;
            DisposeCurrentDevice();

            // Get a new device with this ID and flow direction
            var mmDevice = deviceId == DEFAULT_DEVICE_ID
                ? deviceEnumerator.GetDefaultAudioEndpoint(Flow, Role.Multimedia) // Get default if no ID is provided
                : deviceEnumerator.EnumerateAudioEndPoints(Flow, DeviceState.Active).FirstOrDefault(d => d.ID == DeviceId); // Otherwise, get the one with this ID
            if (mmDevice == null) return;
            Device = mmDevice;

            // Get a WaveIn from the device and start it, adding any events as requied
            WaveIn = Flow == DataFlow.Render ? new WasapiLoopbackCapture(mmDevice) : new WasapiCapture(mmDevice);
            WaveIn.DataAvailable += waveInDataAvailable;
            WaveIn.StartRecording();
        }

        /// <summary>Disposes and clears the current <see cref="Device"/> and <see cref="WaveIn"/>.</summary>
        private void DisposeCurrentDevice() {
            Device?.Dispose();
            Device = null;

            WaveIn?.StopRecording();
            WaveIn?.Dispose();
            WaveIn = null;
        }

        private void AddPlaybackDevice(MMDevice device)
        {
            try
            {
                PlaybackDevices.Add(new KeyValuePair<string, string>(device.ID, device.DeviceFriendlyName));
            }
            finally
            {
                var selectedDevice = Global.Configuration.GSIAudioRenderDevice;
                if (selectedDevice.Equals(device.ID))
                {
                    //probably need to fill this. somehow this is still fine
                }
            }
        }

        private void AddRecordingDevice(MMDevice device)
        {
            try
            {
                RecordingDevices.Add(new KeyValuePair<string, string>(device.ID, device.DeviceFriendlyName));
            }
            finally
            {
                var selectedDevice = Global.Configuration.GSIAudioRenderDevice;
                if (selectedDevice.Equals(device.ID))
                {

                }
            }
        }

        #region Device Enumeration
        public static ObservableCollection<KeyValuePair<string, string>> PlaybackDevices { get; } = new ObservableCollection<KeyValuePair<string, string>>();
        public static ObservableCollection<KeyValuePair<string, string>> RecordingDevices { get; } = new ObservableCollection<KeyValuePair<string, string>>();
        public void OnDeviceAdded(string pwstrDeviceId)
        {
            var device = deviceEnumerator.GetDevice(pwstrDeviceId);
            switch (device.DataFlow)
            {
                case DataFlow.Render:
                    AddPlaybackDevice(device);
                    break;
                case DataFlow.Capture:
                    AddRecordingDevice(device);
                    break;
            }
        }
        public void OnDeviceRemoved(string deviceId)
        {
            var device = deviceEnumerator.GetDevice(deviceId);
            switch (device.DataFlow)
            {
                case DataFlow.Render:
                    PlaybackDevices.Remove(new KeyValuePair<string, string>(device.ID, device.DeviceFriendlyName));
                    break;
                case DataFlow.Capture:
                    RecordingDevices.Remove(new KeyValuePair<string, string>(device.ID, device.DeviceFriendlyName));
                    break;
            }
        }

        // Updates the target list with the devices of the given dataflow type.
        private static void RefreshDeviceList(ObservableCollection<KeyValuePair<string, string>> target, DataFlow flow) {
            // Note: clear the target then repopulate it to make it easier for data binding. If we re-created this, we could not use {x:Static}.
            target.Clear();
            target.Add(new KeyValuePair<string, string>(DEFAULT_DEVICE_ID, "Default")); // Add default device to to the top of the list
            foreach (var device in deviceEnumerator.EnumerateAudioEndPoints(flow, DeviceState.Active).OrderBy(d => d.DeviceFriendlyName))
                target.Add(new KeyValuePair<string, string>(device.ID, device.DeviceFriendlyName));            
        }

        // Refreshes both playback and recording devices lists.
        private static void RefreshDeviceLists() {
            RefreshDeviceList(PlaybackDevices, DataFlow.Render);
            RefreshDeviceList(RecordingDevices, DataFlow.Capture);
        }
        #endregion

        #region IMMNotificationClient Implementation

        /// <summary>
        /// Update the device when changed by the system.
        /// </summary>
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
            => defaultDeviceChanged = true;

        // Methods from interface not used
        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            var device = deviceEnumerator.GetDevice(deviceId);
            var kv = new KeyValuePair<string, string>(device.ID, device.DeviceFriendlyName);
            switch (device.DataFlow)
            {
                case DataFlow.Render:
                    if (!PlaybackDevices.Contains(kv))
                    {
                        AddPlaybackDevice(device);
                    }
                    break;
                case DataFlow.Capture:
                    RecordingDevices.Remove(new KeyValuePair<string, string>(device.ID, device.DeviceFriendlyName));
                    if (!RecordingDevices.Contains(kv))
                    {
                        AddRecordingDevice(device);
                    }
                    break;
            }
        }
        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }

        #endregion

        #region IDisposable Implementation
        private bool disposedValue = false;
        public void Dispose() => Dispose(true);
        void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing)
                    DisposeCurrentDevice();
                deviceEnumerator.UnregisterEndpointNotificationCallback(this);
                disposedValue = true;
            }
        }
        #endregion
    }
}
