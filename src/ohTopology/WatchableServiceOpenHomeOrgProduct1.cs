﻿using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Xml;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Os;

namespace OpenHome.Av
{
    public interface IServiceOpenHomeOrgProduct1
    {
        IWatchable<string> Room { get; }
        IWatchable<string> Name { get; }
        IWatchable<uint> SourceIndex { get; }
        IWatchable<string> SourceXml { get; }
        IWatchable<bool> Standby { get; }

        void SetSourceIndex(uint aValue);
        void SetSourceIndexByName(string aValue);
        void SetStandby(bool aValue);
    }

    public interface IProduct : IServiceOpenHomeOrgProduct1
    {
        string Attributes { get; }
        string ManufacturerImageUri { get; }
        string ManufacturerInfo { get; }
        string ManufacturerName { get; }
        string ManufacturerUrl { get; }
        string ModelImageUri { get; }
        string ModelInfo { get; }
        string ModelName { get; }
        string ModelUrl  { get; }
        string ProductImageUri { get; }
        string ProductInfo { get; }
        string ProductUrl { get; }
    }

    public abstract class Product : IProduct, IWatchableService
    {
        // IDisposable methods

        public abstract void Dispose();

        public IService Create(IManagableWatchableDevice aDevice)
        {
            return new ServiceProduct(aDevice, this);
        }

        internal abstract IServiceOpenHomeOrgProduct1 Service { get; }

        // IServiceOpenHomeOrgProduct1 methods

        public IWatchable<string> Room
        {
            get
            {
                return Service.Room;
            }
        }

        public IWatchable<string> Name
        {
            get 
            {
                return Service.Name;
            }
        }

        public IWatchable<uint> SourceIndex
        {
            get 
            {
                return Service.SourceIndex;
            }
        }

        public IWatchable<string> SourceXml
        {
            get
            {
                return Service.SourceXml;
            }
        }

        public IWatchable<bool> Standby
        {
            get
            {
                return Service.Standby;
            }
        }

        public void SetSourceIndex(uint aValue)
        {
            Service.SetSourceIndex(aValue);
        }

        public void SetSourceIndexByName(string aValue)
        {
            Service.SetSourceIndexByName(aValue);
        }

        public void SetStandby(bool aValue)
        {
            Service.SetStandby(aValue);
        }

        // Product methods

        public string Attributes
        {
            get
            {
                return iAttributes;
            }
        }

        public string ManufacturerImageUri
        {
            get
            {
                return iManufacturerImageUri;
            }
        }

        public string ManufacturerInfo
        {
            get
            {
                return iManufacturerInfo;
            }
        }

        public string ManufacturerName
        {
            get
            {
                return iManufacturerName;
            }
        }

        public string ManufacturerUrl
        {
            get
            {
                return iManufacturerUrl;
            }
        }

        public string ModelImageUri
        {
            get
            {
                return iModelImageUri;
            }
        }

        public string ModelInfo
        {
            get
            {
                return iModelInfo;
            }
        }

        public string ModelName
        {
            get
            {
                return iModelName;
            }
        }

        public string ModelUrl
        {
            get
            {
                return iModelUrl;
            }
        }

        public string ProductImageUri
        {
            get
            {
                return iProductImageUri;
            }
        }

        public string ProductInfo
        {
            get
            {
                return iProductInfo;
            }
        }

        public string ProductUrl
        {
            get
            {
                return iProductUrl;
            }
        }

        protected string iAttributes;
        protected string iManufacturerImageUri;
        protected string iManufacturerInfo;
        protected string iManufacturerName;
        protected string iManufacturerUrl;
        protected string iModelImageUri;
        protected string iModelInfo;
        protected string iModelName;
        protected string iModelUrl;
        protected string iProductImageUri;
        protected string iProductInfo;
        protected string iProductUrl;
    }

    public class ServiceOpenHomeOrgProduct1 : IServiceOpenHomeOrgProduct1, IDisposable
    {
        public ServiceOpenHomeOrgProduct1(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgProduct1 aService)
        {
            iLock = new object();
            iDisposed = false;

            lock (iLock)
            {
                iService = aService;

                iService.SetPropertyProductNameChanged(HandleRoomChanged);
                iService.SetPropertyProductNameChanged(HandleNameChanged);
                iService.SetPropertySourceIndexChanged(HandleSourceIndexChanged);
                iService.SetPropertySourceXmlChanged(HandleSourceXmlChanged);
                iService.SetPropertyStandbyChanged(HandleStandbyChanged);

                iRoom = new Watchable<string>(aThread, string.Format("Room({0})", aId), iService.PropertyProductRoom());
                iName = new Watchable<string>(aThread, string.Format("Name({0})", aId), iService.PropertyProductName());
                iSourceIndex = new Watchable<uint>(aThread, string.Format("SourceIndex({0})", aId), iService.PropertySourceIndex());
                iSourceXml = new Watchable<string>(aThread, string.Format("SourceXml({0})", aId), iService.PropertySourceXml());
                iStandby = new Watchable<bool>(aThread, string.Format("Standby({0})", aId), iService.PropertyStandby());
            }
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgProduct1.Dispose");
                }

                iService.Dispose();
                iService = null;

                iRoom.Dispose();
                iRoom = null;

                iName.Dispose();
                iName = null;

                iSourceIndex.Dispose();
                iSourceIndex = null;

                iSourceXml.Dispose();
                iSourceXml = null;

                iStandby.Dispose();
                iStandby = null;

                iDisposed = true;
            }
        }

        public IWatchable<string> Room
        {
            get
            {
                return iRoom;
            }
        }

        public IWatchable<string> Name
        {
            get
            {
                return iName;
            }
        }

        public IWatchable<uint> SourceIndex
        {
            get
            {
                return iSourceIndex;
            }
        }

        public IWatchable<string> SourceXml
        {
            get
            {
                return iSourceXml;
            }
        }

        public IWatchable<bool> Standby
        {
            get
            {
                return iStandby;
            }
        }

        public void SetSourceIndex(uint aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgProduct1.SetSourceIndex");
                }

                iService.BeginSetSourceIndex(aValue, null);
            }
        }

        public void SetSourceIndexByName(string aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgProduct1.SetSourceIndexByName");
                }

                iService.BeginSetSourceIndexByName(aValue, null);
            }
        }

        public void SetStandby(bool aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgProduct1.SetStandby");
                }

                iService.BeginSetStandby(aValue, null);
            }
        }

        private void HandleRoomChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }
                
                iRoom.Update(iService.PropertyProductRoom());
            }
        }

        private void HandleNameChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }
                
                iName.Update(iService.PropertyProductName());
            }
        }

        private void HandleSourceIndexChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iSourceIndex.Update(iService.PropertySourceIndex());
            }
        }

        private void HandleSourceXmlChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iSourceXml.Update(iService.PropertySourceXml());
            }
        }

        private void HandleStandbyChanged()
        {
            lock (iLock)
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        return;
                    }

                    iStandby.Update(iService.PropertyStandby());
                }
            }
        }

        private object iLock;
        private bool iDisposed;

        private CpProxyAvOpenhomeOrgProduct1 iService;

        private Watchable<string> iRoom;
        private Watchable<string> iName;
        private Watchable<uint> iSourceIndex;
        private Watchable<string> iSourceXml;
        private Watchable<bool> iStandby;
    }

    public class WatchableProductFactory : IWatchableServiceFactory
    {
        public WatchableProductFactory(IWatchableThread aThread, IWatchableThread aSubscribeThread)
        {
            iLock = new object();
            iDisposed = false;

            iThread = aThread;
            iSubscribeThread = aSubscribeThread;
        }

        public void Dispose()
        {
            iSubscribeThread.Execute(() =>
            {
                Unsubscribe();
                iDisposed = true;
            });
        }

        public void Subscribe(IWatchableDevice aDevice, Action<IWatchableService> aCallback)
        {
            iSubscribeThread.Schedule(() =>
            {
                if (!iDisposed && iService == null && iPendingService == null)
                {
                    WatchableDevice d = aDevice as WatchableDevice;
                    iPendingService = new CpProxyAvOpenhomeOrgProduct1(d.Device);
                    iPendingService.SetPropertyInitialEvent(delegate
                    {
                        lock (iLock)
                        {
                            if (iPendingService != null)
                            {
                                iService = new WatchableProduct(iThread, string.Format("{0}", aDevice.Udn), iPendingService);
                                iPendingService = null;
                                aCallback(iService);
                            }
                        }
                    });
                    iPendingService.Subscribe();
                }
            });
        }

        public void Unsubscribe()
        {
            iSubscribeThread.Schedule(() =>
            {
                lock (iLock)
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
            });
        }

        private object iLock;
        private bool iDisposed;
        private IWatchableThread iSubscribeThread;
        private CpProxyAvOpenhomeOrgProduct1 iPendingService;
        private WatchableProduct iService;
        private IWatchableThread iThread;
    }

    public class WatchableProduct : Product
    {
        public WatchableProduct(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgProduct1 aService)
        {
            iAttributes = aService.PropertyAttributes();
            iManufacturerImageUri = aService.PropertyManufacturerImageUri();
            iManufacturerInfo = aService.PropertyManufacturerInfo();
            iManufacturerName = aService.PropertyManufacturerName();
            iManufacturerUrl = aService.PropertyManufacturerUrl();
            iModelImageUri = aService.PropertyModelImageUri();
            iModelInfo = aService.PropertyModelInfo();
            iModelName = aService.PropertyModelName();
            iModelUrl = aService.PropertyModelUrl();
            iProductImageUri = aService.PropertyProductImageUri();
            iProductInfo = aService.PropertyProductInfo();
            iProductUrl = aService.PropertyProductUrl();

            iService = new ServiceOpenHomeOrgProduct1(aThread, aId, aService);
        }

        public override void Dispose()
        {
            iService.Dispose();
            iService = null;
        }

        internal override IServiceOpenHomeOrgProduct1 Service
        {
            get
            {
                return iService;
            }
        }

        private ServiceOpenHomeOrgProduct1 iService;
    }

    public class SourceXml
    {
        public class Source
        {
            public Source(string aName, string aType, bool aVisible)
            {
                iName = aName;
                iType = aType;
                iVisible = aVisible;
            }

            public string Name
            {
                get
                {
                    return iName;
                }
                set
                {
                    iName = value;
                }
            }

            public string Type
            {
                get
                {
                    return iType;
                }
            }

            public bool Visible
            {
                get
                {
                    return iVisible;
                }
                set
                {
                    iVisible = value;
                }
            }

            private string iName;
            private string iType;
            private bool iVisible;
        }

        public SourceXml(Source[] aSources)
        {
            iSources = aSources;
            CreateSourceXml();
        }

        public override string ToString()
        {
            return iSourceXml;
        }

        public void UpdateName(uint aIndex, string aName)
        {
            iSources[(int)aIndex].Name = aName;
            CreateSourceXml();
        }

        public void UpdateVisible(uint aIndex, bool aVisible)
        {
            iSources[(int)aIndex].Visible = aVisible;
            CreateSourceXml();
        }

        private void CreateSourceXml()
        {
            XmlDocument doc = new XmlDocument();

            XmlElement sources = doc.CreateElement("SourceList");

            foreach (Source s in iSources)
            {
                XmlElement source = doc.CreateElement("Source");

                XmlElement name = doc.CreateElement("Name");
                XmlElement type = doc.CreateElement("Type");
                XmlElement visible = doc.CreateElement("Visible");

                name.AppendChild(doc.CreateTextNode(s.Name));
                type.AppendChild(doc.CreateTextNode(s.Type));
                visible.AppendChild(doc.CreateTextNode(s.Visible.ToString()));

                source.AppendChild(name);
                source.AppendChild(type);
                source.AppendChild(visible);

                sources.AppendChild(source);
            }

            doc.AppendChild(sources);

            iSourceXml = doc.OuterXml;
        }

        private Source[] iSources;
        private string iSourceXml;
    }

    public class MockServiceOpenHomeOrgProduct1 : IServiceOpenHomeOrgProduct1, IMockable, IDisposable
    {
        public MockServiceOpenHomeOrgProduct1(IWatchableThread aThread, string aId, string aRoom, string aName, uint aSourceIndex, SourceXml aSourceXmlFactory, bool aStandby)
        {
            iSourceXmlFactory = aSourceXmlFactory;

            iRoom = new Watchable<string>(aThread, string.Format("Room({0})", aId), aRoom);
            iName = new Watchable<string>(aThread, string.Format("Name({0})", aId), aName);
            iSourceIndex = new Watchable<uint>(aThread, string.Format("SourceIndex({0})", aId), aSourceIndex);
            iSourceXml = new Watchable<string>(aThread, string.Format("SourceXml({0})", aId), iSourceXmlFactory.ToString());
            iStandby = new Watchable<bool>(aThread, string.Format("Standby({0})", aId), aStandby);
        }

        public void Dispose()
        {
            iRoom.Dispose();
            iRoom = null;

            iName.Dispose();
            iName = null;

            iSourceIndex.Dispose();
            iSourceIndex = null;

            iSourceXml.Dispose();
            iSourceXml = null;

            iStandby.Dispose();
            iStandby = null;
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "room")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iRoom.Update(string.Join(" ", value));
            }
            else if (command == "name")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iName.Update(string.Join(" ", value));
            }
            else if (command == "sourceindex")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iSourceIndex.Update(uint.Parse(value.First()));
            }
            else if (command == "standby")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iStandby.Update(bool.Parse(value.First()));
            }
            else if (command == "source")
            {
                IEnumerable<string> value = aValue.Skip(1);

                uint index = uint.Parse(value.First());

                value = value.Skip(1);

                string property = value.First();

                value = value.Skip(1);

                if (property == "name")
                {
                    iSourceXmlFactory.UpdateName(index, string.Join(" ", value));
                    iSourceXml.Update(iSourceXmlFactory.ToString());
                }
                else if (property == "visible")
                {
                    iSourceXmlFactory.UpdateVisible(index, bool.Parse(value.First()));
                    iSourceXml.Update(iSourceXmlFactory.ToString());
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public IWatchable<string> Room
        {
            get
            {
                return iRoom;
            }
        }

        public IWatchable<string> Name
        {
            get
            {
                return iName;
            }
        }

        public IWatchable<uint> SourceIndex
        {
            get
            {
                return iSourceIndex;
            }
        }

        public IWatchable<string> SourceXml
        {
            get
            {
                return iSourceXml;
            }
        }

        public IWatchable<bool> Standby
        {
            get
            {
                return iStandby;
            }
        }

        public void SetSourceIndex(uint aValue)
        {
            iSourceIndex.Update(aValue);
        }

        public void SetSourceIndexByName(string aValue)
        {
            throw new NotSupportedException();
        }

        public void SetStandby(bool aValue)
        {
            iStandby.Update(aValue);
        }

        private SourceXml iSourceXmlFactory;

        private Watchable<string> iRoom;
        private Watchable<string> iName;
        private Watchable<uint> iSourceIndex;
        private Watchable<string> iSourceXml;
        private Watchable<bool> iStandby;
    }

    public class MockWatchableProduct : Product, IMockable
    {
        public MockWatchableProduct(IWatchableThread aThread, string aId, string aRoom, string aName, uint aSourceIndex, SourceXml aSourceXmlFactory, bool aStandby,
            string aAttributes, string aManufacturerImageUri, string aManufacturerInfo, string aManufacturerName, string aManufacturerUrl, string aModelImageUri, string aModelInfo, string aModelName,
            string aModelUrl, string aProductImageUri, string aProductInfo, string aProductUrl)
        {
            iAttributes = aAttributes;
            iManufacturerImageUri = aManufacturerImageUri;
            iManufacturerInfo = aManufacturerInfo;
            iManufacturerName = aManufacturerName;
            iManufacturerUrl = aManufacturerUrl;
            iModelImageUri = aModelImageUri;
            iModelInfo = aModelInfo;
            iModelName = aModelName;
            iModelUrl = aModelUrl;
            iProductImageUri = aProductImageUri;
            iProductInfo = aProductInfo;
            iProductUrl = aProductUrl;

            iService = new MockServiceOpenHomeOrgProduct1(aThread, aId, aRoom, aName, aSourceIndex, aSourceXmlFactory, aStandby);
        }

        public override void Dispose()
        {
            iService.Dispose();
            iService = null;
        }

        internal override IServiceOpenHomeOrgProduct1 Service
        {
            get
            {
                return iService;
            }
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "room" || command == "name" || command == "sourceindex" || command == "standby" || command == "source")
            {
                iService.Execute(aValue);
            }
            else if (command == "attributes")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iAttributes = string.Join(" ", value);
            }
            else if (command == "manufacturerimageuri")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iManufacturerImageUri = string.Join(" ", value);
            }
            else if (command == "manufacturerinfo")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iManufacturerInfo = string.Join(" ", value);
            }
            else if (command == "manufacturername")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iManufacturerName = string.Join(" ", value);
            }
            else if (command == "manufacturerurl")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iManufacturerUrl = string.Join(" ", value);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private MockServiceOpenHomeOrgProduct1 iService;
    }

    public class ServiceProduct : IProduct, IService
    {
        public ServiceProduct(IManagableWatchableDevice aDevice, IProduct aService)
        {
            iDevice = aDevice;
            iService = aService;
        }

        public void Dispose()
        {
            iDevice.Unsubscribe<ServiceProduct>();
            iDevice = null;
        }

        public IWatchableDevice Device
        {
            get { return iDevice; }
        }

        public IWatchable<string> Room
        {
            get { return iService.Room; }
        }

        public IWatchable<string> Name
        {
            get { return iService.Name; }
        }

        public IWatchable<uint> SourceIndex
        {
            get { return iService.SourceIndex; }
        }

        public IWatchable<string> SourceXml
        {
            get { return iService.SourceXml; }
        }

        public IWatchable<bool> Standby
        {
            get { return iService.Standby; }
        }

        public void SetSourceIndex(uint aValue)
        {
            iService.SetSourceIndex(aValue);
        }

        public void SetSourceIndexByName(string aValue)
        {
            iService.SetSourceIndexByName(aValue);
        }

        public void SetStandby(bool aValue)
        {
            iService.SetStandby(aValue);
        }

        public string Attributes
        {
            get { return iService.Attributes; }
        }

        public string ManufacturerImageUri
        {
            get { return iService.ManufacturerImageUri; }
        }

        public string ManufacturerInfo
        {
            get { return iService.ManufacturerInfo; }
        }

        public string ManufacturerName
        {
            get { return iService.ManufacturerName; }
        }

        public string ManufacturerUrl
        {
            get { return iService.ManufacturerUrl; }
        }

        public string ModelImageUri
        {
            get { return iService.ModelImageUri; }
        }

        public string ModelInfo
        {
            get { return iService.ModelInfo; }
        }

        public string ModelName
        {
            get { return iService.ModelName; }
        }

        public string ModelUrl
        {
            get { return iService.ModelUrl; }
        }

        public string ProductImageUri
        {
            get { return iService.ProductImageUri; }
        }

        public string ProductInfo
        {
            get { return iService.ProductInfo; }
        }

        public string ProductUrl
        {
            get { return iService.ProductUrl; }
        }

        private IManagableWatchableDevice iDevice;
        private IProduct iService;
    }
}
