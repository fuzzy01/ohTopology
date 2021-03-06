﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Os;

namespace OpenHome.Av
{
    public interface IProxyProduct : IProxy
    {
        IWatchable<string> Room { get; }
        IWatchable<string> Name { get; }
        IWatchable<uint> SourceIndex { get; }
        IWatchable<string> SourceXml { get; }
        IWatchable<bool> Standby { get; }

        Task SetSourceIndex(uint aValue);
        Task SetSourceIndexByName(string aValue);
        Task SetStandby(bool aValue);

        string Attributes { get; }
        string ManufacturerImageUri { get; }
        string ManufacturerInfo { get; }
        string ManufacturerName { get; }
        string ManufacturerUrl { get; }
        string ModelImageUri { get; }
        string ModelInfo { get; }
        string ModelName { get; }
        string ModelUrl { get; }
        string ProductImageUri { get; }
        string ProductInfo { get; }
        string ProductUrl { get; }
    }

    public abstract class ServiceProduct : Service
    {
        protected ServiceProduct(INetwork aNetwork)
            : base(aNetwork)
        {
            iRoom = new Watchable<string>(Network, "Room", string.Empty);
            iName = new Watchable<string>(Network, "Name", string.Empty);
            iSourceIndex = new Watchable<uint>(Network, "SourceIndex", 0);
            iSourceXml = new Watchable<string>(Network, "SourceXml", string.Empty);
            iStandby = new Watchable<bool>(Network, "Standby", false);
        }

        public override void Dispose()
        {
            base.Dispose();

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

        public override IProxy OnCreate(IDevice aDevice)
        {
            return new ProxyProduct(aDevice, this);
        }

        // IServiceProduct methods

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

        public abstract Task SetSourceIndex(uint aValue);
        public abstract Task SetSourceIndexByName(string aValue);
        public abstract Task SetStandby(bool aValue);

        // IProduct methods

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

        protected Watchable<string> iRoom;
        protected Watchable<string> iName;
        protected Watchable<uint> iSourceIndex;
        protected Watchable<string> iSourceXml;
        protected Watchable<bool> iStandby;
    }

    class ServiceProductNetwork : ServiceProduct
    {
        public ServiceProductNetwork(INetwork aNetwork, CpDevice aDevice)
            : base(aNetwork)
        {
            iSubscribed = new ManualResetEvent(false);
            iService = new CpProxyAvOpenhomeOrgProduct1(aDevice);

            iService.SetPropertyProductRoomChanged(HandleRoomChanged);
            iService.SetPropertyProductNameChanged(HandleNameChanged);
            iService.SetPropertySourceIndexChanged(HandleSourceIndexChanged);
            iService.SetPropertySourceXmlChanged(HandleSourceXmlChanged);
            iService.SetPropertyStandbyChanged(HandleStandbyChanged);

            iService.SetPropertyInitialEvent(HandleInitialEvent);
        }

        public override void Dispose()
        {
            iSubscribed.Dispose();
            iSubscribed = null;

            iService.Dispose();
            iService = null;

            base.Dispose();
        }

        protected override Task OnSubscribe()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.Subscribe();
                iSubscribed.WaitOne();
            });
            return task;
        }

        private void HandleInitialEvent()
        {
            iAttributes = iService.PropertyAttributes();
            iManufacturerImageUri = iService.PropertyManufacturerImageUri();
            iManufacturerInfo = iService.PropertyManufacturerInfo();
            iManufacturerName = iService.PropertyManufacturerName();
            iManufacturerUrl = iService.PropertyManufacturerUrl();
            iModelImageUri = iService.PropertyModelImageUri();
            iModelInfo = iService.PropertyModelInfo();
            iModelName = iService.PropertyModelName();
            iModelUrl = iService.PropertyModelUrl();
            iProductImageUri = iService.PropertyProductImageUri();
            iProductInfo = iService.PropertyProductInfo();
            iProductUrl = iService.PropertyProductUrl();

            iSubscribed.Set();
        }

        protected override void OnUnsubscribe()
        {
            iService.Unsubscribe();
            iSubscribed.Reset();
        }

        public override Task SetSourceIndex(uint aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncSetSourceIndex(aValue);
            });
            return task;
        }

        public override Task SetSourceIndexByName(string aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncSetSourceIndexByName(aValue);
            });
            return task;
        }

        public override Task SetStandby(bool aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncSetStandby(aValue);
            });
            return task;
        }

        private void HandleRoomChanged()
        {
            Network.Schedule(() =>
            {
                iRoom.Update(iService.PropertyProductRoom());
            });
        }

        private void HandleNameChanged()
        {
            Network.Schedule(() =>
            {
                iName.Update(iService.PropertyProductName());
            });
        }

        private void HandleSourceIndexChanged()
        {
            Network.Schedule(() =>
            {
                iSourceIndex.Update(iService.PropertySourceIndex());
            });
        }

        private void HandleSourceXmlChanged()
        {
            Network.Schedule(() =>
            {
                iSourceXml.Update(iService.PropertySourceXml());
            });
        }

        private void HandleStandbyChanged()
        {
            Network.Schedule(() =>
            {
                iStandby.Update(iService.PropertyStandby());
            });
        }

        private ManualResetEvent iSubscribed;
        private CpProxyAvOpenhomeOrgProduct1 iService;
    }

    internal class SourceXml
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

    class ServiceProductMock : ServiceProduct
    {
        public ServiceProductMock(INetwork aNetwork, string aRoom, string aName, uint aSourceIndex, SourceXml aSourceXmlFactory, bool aStandby,
            string aAttributes, string aManufacturerImageUri, string aManufacturerInfo, string aManufacturerName, string aManufacturerUrl, string aModelImageUri, string aModelInfo, string aModelName,
            string aModelUrl, string aProductImageUri, string aProductInfo, string aProductUrl)
            : base(aNetwork)
        {
            iSourceXmlFactory = aSourceXmlFactory;

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

            iRoom.Update(aRoom);
            iName.Update(aName);
            iSourceIndex.Update(aSourceIndex);
            iSourceXml.Update(iSourceXmlFactory.ToString());
            iStandby.Update(aStandby);
        }

        public override void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "attributes")
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
            else if (command == "room")
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

        public override Task SetSourceIndex(uint aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iSourceIndex.Update(aValue);
                });
            });
            return task;
        }

        public override Task SetSourceIndexByName(string aValue)
        {
            throw new NotSupportedException();
        }

        public override Task SetStandby(bool aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iStandby.Update(aValue);
                });
            });
            return task;
        }

        private SourceXml iSourceXmlFactory;
    }

    public class ProxyProduct : Proxy<ServiceProduct>, IProxyProduct
    {
        public ProxyProduct(IDevice aDevice, ServiceProduct aService)
            : base(aDevice, aService)
        {
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

        public Task SetSourceIndex(uint aValue)
        {
            return iService.SetSourceIndex(aValue);
        }

        public Task SetSourceIndexByName(string aValue)
        {
            return iService.SetSourceIndexByName(aValue);
        }

        public Task SetStandby(bool aValue)
        {
            return iService.SetStandby(aValue);
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
    }
}
