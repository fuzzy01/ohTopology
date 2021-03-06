﻿using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IStandardRoomController : ISourceController, IVolumeController
    {
        string Name { get; }

        IWatchable<bool> Active { get; }

        IWatchable<bool> HasInfoNext { get; }
        IWatchable<IInfoMetadata> InfoNext { get; }

        IWatchable<bool> HasSourceControl { get; }
        IWatchable<string> TransportState { get; }
        IWatchable<bool> CanPause { get; }
        IWatchable<bool> CanSkip { get; }
        IWatchable<bool> CanSeek { get; }
        IWatchable<bool> HasPlayMode { get; }
        IWatchable<bool> Repeat { get; }
        IWatchable<bool> Shuffle { get; }

        IWatchable<EStandby> Standby { get; }
        void SetStandby(bool aValue);
    }

    public class StandardRoomController : IWatcher<ITopology4Source>, IStandardRoomController, IDisposable
    {
        public StandardRoomController(IStandardRoom aRoom)
        {
            iNetwork = aRoom.Network;
            iRoom = aRoom;

            iLock = new object();
            iIsActive = true;
            iActive = new Watchable<bool>(iNetwork, "Active", true);

            iVolumeController = new StandardVolumeController(aRoom);

            iHasSourceControl = new Watchable<bool>(iNetwork, "HasSourceControl", false);
            iHasInfoNext = new Watchable<bool>(iNetwork, "HasInfoNext", false);
            iInfoNext = new Watchable<IInfoMetadata>(iNetwork, "InfoNext", new RoomMetadata());

            iCanPause = new Watchable<bool>(iNetwork, "CanPause", false);
            iCanSkip = new Watchable<bool>(iNetwork, "CanSkip", false);
            iCanSeek = new Watchable<bool>(iNetwork, "CanSeek", false);
            iTransportState = new Watchable<string>(iNetwork, "TransportState", string.Empty);

            iHasPlayMode = new Watchable<bool>(iNetwork, "HasPlayMode", false);
            iShuffle = new Watchable<bool>(iNetwork, "Shuffle", false);
            iRepeat = new Watchable<bool>(iNetwork, "Repeat", false);

            iRoom.Source.AddWatcher(this);

            iRoom.Join(SetInactive);
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iIsActive)
                {
                    iNetwork.Execute(() =>
                    {
                        iRoom.Source.RemoveWatcher(this);
                    });
                    iRoom.UnJoin(SetInactive);
                }
            }

            iRoom = null;

            iActive.Dispose();
            iActive = null;

            iHasSourceControl.Dispose();
            iHasSourceControl = null;

            iHasInfoNext.Dispose();
            iHasInfoNext = null;

            iInfoNext.Dispose();
            iInfoNext = null;

            iVolumeController.Dispose();
            iVolumeController = null;

            iTransportState.Dispose();
            iTransportState = null;

            iCanPause.Dispose();
            iCanPause = null;

            iCanSkip.Dispose();
            iCanSkip = null;

            iCanSeek.Dispose();
            iCanSeek = null;

            iHasPlayMode.Dispose();
            iHasPlayMode = null;

            iShuffle.Dispose();
            iShuffle = null;

            iRepeat.Dispose();
            iRepeat = null;
        }

        private void SetInactive()
        {
            lock (iLock)
            {
                iIsActive = false;

                iActive.Update(false);

                iRoom.Source.RemoveWatcher(this);
                iRoom.UnJoin(SetInactive);
            }
        }

        public IWatchable<bool> Active
        {
            get
            {
                return iActive;
            }
        }

        public IWatchable<EStandby> Standby
        {
            get
            {
                return iRoom.Standby;
            }
        }

        public void SetStandby(bool aValue)
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    iRoom.SetStandby(aValue);
                }
            });
        }

        public void SetRepeat(bool aValue)
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasSourceControl.Value)
                    {
                        iSourceController.SetRepeat(aValue);
                    }
                }
            });
        }

        public void SetShuffle(bool aValue)
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasSourceControl.Value)
                    {
                        iSourceController.SetShuffle(aValue);
                    }
                }
            });
        }

        public IWatchable<bool> HasVolume
        {
            get
            {
                return iVolumeController.HasVolume;
            }
        }

        public IWatchable<bool> Mute
        {
            get
            {
                return iVolumeController.Mute;
            }
        }

        public IWatchable<uint> Volume
        {
            get
            {
                return iVolumeController.Volume;
            }
        }

        public IWatchable<uint> VolumeLimit
        {
            get
            {
                return iVolumeController.VolumeLimit;
            }
        }

        public void SetMute(bool aMute)
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    iVolumeController.SetMute(aMute);
                }
            });
        }

        public void SetVolume(uint aVolume)
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                   iVolumeController.SetVolume(aVolume);
                }
            });
        }

        public void VolumeInc()
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    iVolumeController.VolumeInc();
                }
            });
        }

        public void VolumeDec()
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    iVolumeController.VolumeDec();
                }
            });
        }

        public string Name
        {
            get
            {
                return iRoom.Name;
            }
        }

        public IWatchable<bool> HasSourceControl
        {
            get
            {
                return iHasSourceControl;
            }
        }

        public IWatchable<bool> HasInfoNext
        {
            get
            {
                return iHasInfoNext;
            }
        }

        public IWatchable<IInfoMetadata> InfoNext
        {
            get
            {
                return iInfoNext;
            }
        }

        public IWatchable<string> TransportState
        {
            get
            {
                return iTransportState;
            }
        }

        public IWatchable<bool> CanPause
        {
            get
            {
                return iCanPause;
            }
        }

        public void Play()
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasSourceControl.Value)
                    {
                        iSourceController.Play();
                    }
                }
            });
        }

        public void Pause()
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasSourceControl.Value)
                    {
                        iSourceController.Pause();
                    }
                }
            });
        }

        public void Stop()
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasSourceControl.Value)
                    {
                        iSourceController.Stop();
                    }
                }
            });
        }

        public IWatchable<bool> CanSkip
        {
            get
            {
                return iCanSkip;
            }
        }

        public void Previous()
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasSourceControl.Value)
                    {
                        iSourceController.Previous();
                    }
                }
            });
        }

        public void Next()
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasSourceControl.Value)
                    {
                        iSourceController.Next();
                    }
                }
            });
        }

        public IWatchable<bool> CanSeek
        {
            get
            {
                return iCanSeek;
            }
        }

        public void Seek(uint aSeconds)
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasSourceControl.Value)
                    {
                        iSourceController.Seek(aSeconds);
                    }
                }
            });
        }

        public IWatchable<bool> HasPlayMode
        {
            get
            {
                return iHasPlayMode;
            }
        }

        public IWatchable<bool> Repeat
        {
            get
            {
                return iRepeat;
            }
        }

        public IWatchable<bool> Shuffle
        {
            get
            {
                return iShuffle;
            }
        }

        public void ItemOpen(string aId, ITopology4Source aValue)
        {
            iSourceController = SourceController.Create(aValue, iHasSourceControl, iHasInfoNext, iInfoNext, iTransportState, iCanPause, iCanSkip, iCanSeek, iHasPlayMode, iShuffle, iRepeat);
        }

        public void ItemUpdate(string aId, ITopology4Source aValue, ITopology4Source aPrevious)
        {
            if (iSourceController != null)
            {
                iSourceController.Dispose();
                iSourceController = null;
            }

            iSourceController = SourceController.Create(aValue, iHasSourceControl, iHasInfoNext, iInfoNext, iTransportState, iCanPause, iCanSkip, iCanSeek, iHasPlayMode, iShuffle, iRepeat);
        }

        public void ItemClose(string aId, ITopology4Source aValue)
        {
            if (iSourceController != null)
            {
                iSourceController.Dispose();
                iSourceController = null;
            }
        }

        private INetwork iNetwork;
        private IStandardRoom iRoom;

        private object iLock;
        private bool iIsActive;
        private Watchable<bool> iActive;

        private StandardVolumeController iVolumeController;

        private ISourceController iSourceController;
        private Watchable<bool> iHasSourceControl;
        private Watchable<bool> iHasInfoNext;
        private Watchable<IInfoMetadata> iInfoNext;
        private Watchable<string> iTransportState;
        private Watchable<bool> iCanPause;
        private Watchable<bool> iCanSkip;
        private Watchable<bool> iCanSeek;
        private Watchable<bool> iHasPlayMode;
        private Watchable<bool> iShuffle;
        private Watchable<bool> iRepeat;
    }
}
