﻿using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;

namespace OpenHome.Av
{
    public interface IServiceOpenHomeOrgReceiver1
    {
        IWatchable<string> Metadata { get; }
        IWatchable<string> TransportState { get; }
        IWatchable<string> Uri { get; }
    }

    public abstract class Receiver : IWatchableService, IServiceOpenHomeOrgReceiver1, IDisposable
    {
        protected Receiver(string aId, IServiceOpenHomeOrgReceiver1 aService)
        {
            iId = aId;
            iService = aService;
        }

        public abstract void Dispose();

        public string Id
        {
            get
            {
                return iId;
            }
        }

        public string Type
        {
            get
            {
                return "AvOpenHomeReceiver1";
            }
        }

        public IWatchable<string> Metadata
        {
            get
            {
                return iService.Metadata;
            }
        }

        public IWatchable<string> TransportState
        {
            get
            {
                return iService.TransportState;
            }
        }

        public IWatchable<string> Uri
        {
            get
            {
                return iService.Uri;
            }
        }

        public string ProtocolInfo
        {
            get
            {
                return iProtocolInfo;
            }
        }

        private string iId;

        protected IServiceOpenHomeOrgReceiver1 iService;
        protected string iProtocolInfo;
    }

    public class ServiceOpenHomeOrgReceiver1 : IServiceOpenHomeOrgReceiver1, IDisposable
    {
        public ServiceOpenHomeOrgReceiver1(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgReceiver1 aService)
        {
            iLock = new object();
            iDisposed = false;

            lock (iLock)
            {
                iService = aService;

                iService.SetPropertyMetadataChanged(HandleMetadataChanged);
                iService.SetPropertyTransportStateChanged(HandleTransportStateChanged);
                iService.SetPropertyUriChanged(HandleUriChanged);

                iMetadata = new Watchable<string>(aThread, string.Format("Metadata({0})", aId), iService.PropertyMetadata());
                iTransportState = new Watchable<string>(aThread, string.Format("TransportState({0})", aId), iService.PropertyTransportState());
                iUri = new Watchable<string>(aThread, string.Format("Uri({0})", aId), iService.PropertyUri());
            }
        }
        
        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgReceiver1.Dispose");
                }

                iService = null;

                iDisposed = true;
            }
        }

        public IWatchable<string> Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public IWatchable<string> TransportState
        {
            get
            {
                return iTransportState;
            }
        }

        public IWatchable<string> Uri
        {
            get
            {
                return iUri;
            }
        }

        private void HandleMetadataChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iMetadata.Update(iService.PropertyMetadata());
            }
        }

        private void HandleTransportStateChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iTransportState.Update(iService.PropertyTransportState());
            }
        }

        private void HandleUriChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iUri.Update(iService.PropertyUri());
            }
        }

        private object iLock;
        private bool iDisposed;

        private CpProxyAvOpenhomeOrgReceiver1 iService;

        private Watchable<string> iMetadata;
        private Watchable<string> iTransportState;
        private Watchable<string> iUri;
    }

    public class MockServiceOpenHomeOrgReceiver1 : IServiceOpenHomeOrgReceiver1, IMockable, IDisposable
    {
        public MockServiceOpenHomeOrgReceiver1(IWatchableThread aThread, string aId, string aMetadata, string aTransportState, string aUri)
        {
            iMetadata = new Watchable<string>(aThread, string.Format("Metadata({0})", aId), aMetadata);
            iTransportState = new Watchable<string>(aThread, string.Format("TransportState({0})", aId), aTransportState);
            iUri = new Watchable<string>(aThread, string.Format("Uri({0})", aId), aUri);
        }

        public void Dispose()
        {
        }

        public IWatchable<string> Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public IWatchable<string> TransportState
        {
            get
            {
                return iTransportState;
            }
        }

        public IWatchable<string> Uri
        {
            get
            {
                return iUri;
            }
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "metadata")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iMetadata.Update(value.First());
            }
            else if (command == "transportstate")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iTransportState.Update(value.First());
            }
            else if (command == "uri")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iUri.Update(value.First());
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private Watchable<string> iMetadata;
        private Watchable<string> iTransportState;
        private Watchable<string> iUri;
    }

    public class WatchableReceiverFactory : IWatchableServiceFactory
    {
        public WatchableReceiverFactory(IWatchableThread aThread)
        {
            iThread = aThread;

            iService = null;
        }

        public void Subscribe(WatchableDevice aDevice, Action<IWatchableService> aCallback)
        {
            if (iService == null && iPendingService == null)
            {
                iPendingService = new CpProxyAvOpenhomeOrgReceiver1(aDevice.Device);
                iPendingService.SetPropertyInitialEvent(delegate
                {
                    iThread.Schedule(() =>
                    {
                        iService = new WatchableReceiver(iThread, string.Format("Receiver({0})", aDevice.Udn), iPendingService);
                        iPendingService = null;
                        aCallback(iService);
                    });
                });
                iPendingService.Subscribe();
            }
        }

        public void Unsubscribe()
        {
            if (iPendingService != null)
            {
                iPendingService.Dispose();
                iPendingService = null;
            }

            if (iService != null)
            {
                iService.Dispose();
                iService = null;
            }
        }

        private CpProxyAvOpenhomeOrgReceiver1 iPendingService;
        private WatchableReceiver iService;
        private IWatchableThread iThread;
    }

    public class WatchableReceiver : Receiver
    {
        public WatchableReceiver(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgReceiver1 aService)
            : base(aId, new ServiceOpenHomeOrgReceiver1(aThread, aId, aService))
        {
            iProtocolInfo = aService.PropertyProtocolInfo();

            iCpService = aService;
        }

        public override void Dispose()
        {
            if (iCpService != null)
            {
                iCpService.Dispose();
            }
        }

        private CpProxyAvOpenhomeOrgReceiver1 iCpService;
    }

    public class MockWatchableReceiver : Receiver, IMockable
    {
        public MockWatchableReceiver(IWatchableThread aThread, string aId, string aMetadata, string aProtocolInfo, string aTransportState, string aUri)
            : base(aId, new MockServiceOpenHomeOrgReceiver1(aThread, aId, aMetadata, aTransportState, aUri))
        {
            iProtocolInfo = aProtocolInfo;
        }

        public override void Dispose()
        {
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if(command == "metadata" || command == "transportstate" || command == "uri")
            {
                MockServiceOpenHomeOrgReceiver1 i = iService as MockServiceOpenHomeOrgReceiver1;
                i.Execute(aValue);
            }
            else if (command == "protocolinfo")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iProtocolInfo = string.Join(" ", value);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}