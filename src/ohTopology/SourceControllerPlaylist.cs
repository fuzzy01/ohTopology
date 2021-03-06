﻿using System;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public class SourceControllerPlaylist : IWatcher<string>, IWatcher<bool>, ISourceController
    {
        public SourceControllerPlaylist(ITopology4Source aSource, Watchable<bool> aHasSourceControl,
            Watchable<bool> aHasInfoNext, Watchable<IInfoMetadata> aInfoNext, Watchable<string> aTransportState, Watchable<bool> aCanPause,
            Watchable<bool> aCanSkip, Watchable<bool> aCanSeek, Watchable<bool> aHasPlayMode, Watchable<bool> aShuffle, Watchable<bool> aRepeat)
        {
            iDisposed = false;

            iSource = aSource;

            iHasSourceControl = aHasSourceControl;
            iInfoNext = aInfoNext;
            iCanPause = aCanPause;
            iCanSeek = aCanSeek;
            iTransportState = aTransportState;
            iHasPlayMode = aHasPlayMode;
            iShuffle = aShuffle;
            iRepeat = aRepeat;

            aSource.Device.Create<IProxyPlaylist>((playlist) =>
            {
                if (!iDisposed)
                {
                    iPlaylist = playlist;

                    aHasInfoNext.Update(true);
                    aCanSkip.Update(true);

                    iPlaylist.TransportState.AddWatcher(this);
                    iPlaylist.Shuffle.AddWatcher(this);
                    iPlaylist.Repeat.AddWatcher(this);

                    iHasPlayMode.Update(true);

                    iHasSourceControl.Update(true);
                }
                else
                {
                    playlist.Dispose();
                }
            });
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("SourceControllerPlaylist.Dispose");
            }

            if (iPlaylist != null)
            {
                iHasSourceControl.Update(false);
                iHasPlayMode.Update(false);

                iPlaylist.Shuffle.RemoveWatcher(this);
                iPlaylist.Repeat.RemoveWatcher(this);
                iPlaylist.TransportState.RemoveWatcher(this);

                iPlaylist.Dispose();
                iPlaylist = null;
            }

            iHasSourceControl = null;
            iInfoNext = null;
            iCanPause = null;
            iCanSeek = null;
            iTransportState = null;

            iDisposed = true;
        }

        public void Play()
        {
            iPlaylist.Play();
        }

        public void Pause()
        {
            iPlaylist.Pause();
        }

        public void Stop()
        {
            iPlaylist.Stop();
        }

        public void Previous()
        {
            iPlaylist.Previous();
        }

        public void Next()
        {
            iPlaylist.Next();
        }

        public void Seek(uint aSeconds)
        {
            iPlaylist.SeekSecondAbsolute(aSeconds);
        }

        public void SetRepeat(bool aValue)
        {
            iPlaylist.SetRepeat(aValue);
        }

        public void SetShuffle(bool aValue)
        {
            iPlaylist.SetShuffle(aValue);
        }

        public void ItemOpen(string aId, string aValue)
        {
            iTransportState.Update(aValue);
        }

        public void ItemUpdate(string aId, string aValue, string aPrevious)
        {
            iTransportState.Update(aValue);
        }

        public void ItemClose(string aId, string aValue)
        {
        }

        public void ItemOpen(string aId, bool aValue)
        {
            if (aId == "Shuffle")
            {
                iShuffle.Update(aValue);
            }
            if (aId == "Repeat")
            {
                iRepeat.Update(aValue);
            }
        }

        public void ItemUpdate(string aId, bool aValue, bool aPrevious)
        {
            if (aId == "Shuffle")
            {
                iShuffle.Update(aValue);
            }
            if (aId == "Repeat")
            {
                iRepeat.Update(aValue);
            }
        }

        public void ItemClose(string aId, bool aValue)
        {
        }

        private bool iDisposed;

        private ITopology4Source iSource;
        private IProxyPlaylist iPlaylist;

        private Watchable<bool> iHasSourceControl;
        private Watchable<IInfoMetadata> iInfoNext;
        private Watchable<bool> iCanPause;
        private Watchable<bool> iCanSeek;
        private Watchable<string> iTransportState;
        private Watchable<bool> iHasPlayMode;
        private Watchable<bool> iShuffle;
        private Watchable<bool> iRepeat;
    }
}
