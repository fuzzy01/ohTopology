﻿using System;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    class VolumeController : IWatcher<bool>, IWatcher<uint>, IDisposable
    {
        public VolumeController(IWatchableThread aThread, IWatchableDevice aDevice, Watchable<bool> aHasVolume, Watchable<bool> aMute, Watchable<uint> aValue)
        {
            iLock = new object();
            iDisposed = false;

            iDevice = aDevice;

            iHasVolume = aHasVolume;
            iMute = aMute;
            iValue = aValue;

            iDevice.Create<ServiceVolume>((IWatchableDevice device, ServiceVolume volume) =>
            {
                lock (iLock)
                {
                    if (!iDisposed)
                    {
                        iVolume = volume;

                        iVolume.Mute.AddWatcher(this);
                        iVolume.Value.AddWatcher(this);

                        iHasVolume.Update(true);
                    }
                    else
                    {
                        volume.Dispose();
                    }
                }
            });
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("VolumeController.Dispose");
                }

                if (iVolume != null)
                {
                    iHasVolume.Update(false);

                    iVolume.Mute.RemoveWatcher(this);
                    iVolume.Value.RemoveWatcher(this);

                    iVolume.Dispose();
                    iVolume = null;
                }

                iDisposed = true;
            }
        }

        public IWatchableDevice Device
        {
            get
            {
                return iDevice;
            }
        }

        public void SetMute(bool aValue)
        {
            iVolume.SetMute(aValue);
        }

        public void SetVolume(uint aValue)
        {
            iVolume.SetVolume(aValue);
        }

        public void VolumeInc()
        {
            iVolume.VolumeInc();
        }

        public void VolumeDec()
        {
            iVolume.VolumeDec();
        }

        public void ItemOpen(string aId, bool aValue)
        {
            iMute.Update(aValue);
        }

        public void ItemUpdate(string aId, bool aValue, bool aPrevious)
        {
            iMute.Update(aValue);
        }

        public void ItemClose(string aId, bool aValue)
        {
        }

        public void ItemOpen(string aId, uint aValue)
        {
            iValue.Update(aValue);
        }

        public void ItemUpdate(string aId, uint aValue, uint aPrevious)
        {
            iValue.Update(aValue);
        }

        public void ItemClose(string aId, uint aValue)
        {
        }

        private object iLock;
        private bool iDisposed;
        private ServiceVolume iVolume;

        private IWatchableDevice iDevice;
        private Watchable<bool> iHasVolume;
        private Watchable<bool> iMute;
        private Watchable<uint> iValue;
    }
}