﻿using System;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public class SourceControllerReceiver : IWatcher<string>, ISourceController
    {
        public SourceControllerReceiver(ITopology4Source aSource, Watchable<bool> aHasSourceControl,
            Watchable<bool> aHasInfoNext, Watchable<IInfoMetadata> aInfoNext, Watchable<string> aTransportState, Watchable<bool> aCanPause,
            Watchable<bool> aCanSkip, Watchable<bool> aCanSeek, Watchable<bool> aHasPlayMode, Watchable<bool> aShuffle, Watchable<bool> aRepeat)
        {
            iDisposed = false;

            iSource = aSource;

            iHasSourceControl = aHasSourceControl;
            iTransportState = aTransportState;

            aSource.Device.Create<IProxyReceiver>((receiver) =>
            {
                if (!iDisposed)
                {
                    iReceiver = receiver;

                    aHasInfoNext.Update(false);
                    aCanSkip.Update(false);
                    aCanPause.Update(true);
                    aHasPlayMode.Update(false);

                    iReceiver.TransportState.AddWatcher(this);

                    iHasSourceControl.Update(true);
                }
                else
                {
                    receiver.Dispose();
                }
            });
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("SourceControllerReceiver.Dispose");
            }

            if (iReceiver != null)
            {
                iHasSourceControl.Update(false);

                iReceiver.TransportState.RemoveWatcher(this);

                iReceiver.Dispose();
                iReceiver = null;
            }

            iHasSourceControl = null;
            iTransportState = null;

            iDisposed = true;
        }

        public void Play()
        {
            iReceiver.Play();
        }

        public void Pause()
        {
            throw new NotSupportedException();
        }

        public void Stop()
        {
            iReceiver.Stop();
        }

        public void Previous()
        {
            throw new NotSupportedException();
        }

        public void Next()
        {
            throw new NotSupportedException();
        }

        public void Seek(uint aSeconds)
        {
            throw new NotSupportedException();
        }

        public void SetShuffle(bool aValue)
        {
            throw new NotSupportedException();
        }

        public void SetRepeat(bool aValue)
        {
            throw new NotSupportedException();
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

        private bool iDisposed;

        private ITopology4Source iSource;
        private IProxyReceiver iReceiver;

        private Watchable<bool> iHasSourceControl;
        private Watchable<string> iTransportState;
    }
}
