﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Threading.Tasks;
using System.Threading;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Net.ControlPoint;
using OpenHome.Os;

namespace OpenHome.Av
{
    public interface ISenderMetadata
    {
        string Name { get; }
        string Uri { get; }
        string ArtworkUri { get; }
    }

    public interface IProxySender : IProxy
    {
        IWatchable<bool> Audio { get; }
        IWatchable<ISenderMetadata> Metadata { get; }
        IWatchable<string> Status { get; }

        string Attributes { get; }
        string PresentationUrl { get; }
    }

    public class SenderMetadata : ISenderMetadata
    {
        public static readonly SenderMetadata Empty = new SenderMetadata();

        private SenderMetadata()
        {
            iName = string.Empty;
            iUri = string.Empty;
            iArtworkUri = string.Empty;
        }

        public SenderMetadata(string aMetadata)
        {
            XmlDocument doc = new XmlDocument();
            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("didl", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");
            nsManager.AddNamespace("upnp", "urn:schemas-upnp-org:metadata-1-0/upnp/");
            nsManager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            doc.LoadXml(aMetadata);

            XmlNode name = doc.FirstChild.SelectSingleNode("didl:item/dc:title", nsManager);
            iName = name.FirstChild.Value;
            XmlNode uri = doc.FirstChild.SelectSingleNode("didl:item/didl:res", nsManager);
            iUri = uri.FirstChild.Value;
            XmlNode artworkUri = doc.FirstChild.SelectSingleNode("didl:item/upnp:albumArtURI", nsManager);
            iArtworkUri = artworkUri.FirstChild.Value;
        }

        public string Name
        {
            get { return iName; }
        }

        public string Uri
        {
            get { return iUri; }
        }

        public string ArtworkUri
        {
            get { return iArtworkUri; }
        }

        public override string ToString()
        {
            return base.ToString();
        }

        private string iName;
        private string iUri;
        private string iArtworkUri;
    }

    public abstract class ServiceSender : Service
    {
        protected ServiceSender(INetwork aNetwork)
            : base(aNetwork)
        {
            iAudio = new Watchable<bool>(Network, "Audio", false);
            iMetadata = new Watchable<ISenderMetadata>(Network, "Metadata", SenderMetadata.Empty);
            iStatus = new Watchable<string>(Network, "Status", string.Empty);
        }

        public override void Dispose()
        {
            base.Dispose();

            iAudio.Dispose();
            iAudio = null;

            iMetadata.Dispose();
            iMetadata = null;

            iStatus.Dispose();
            iStatus = null;
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return new ProxySender(aDevice, this);
        }

        public IWatchable<bool> Audio
        {
            get
            {
                return iAudio;
            }
        }

        public IWatchable<ISenderMetadata> Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public IWatchable<string> Status
        {
            get
            {
                return iStatus;
            }
        }

        public string Attributes
        {
            get
            {
                return iAttributes;
            }
        }

        public string PresentationUrl
        {
            get
            {
                return iPresentationUrl;
            }
        }

        protected string iAttributes;
        protected string iPresentationUrl;

        protected Watchable<bool> iAudio;
        protected Watchable<ISenderMetadata> iMetadata;
        protected Watchable<string> iStatus;
    }

    class ServiceSenderNetwork : ServiceSender
    {
        public ServiceSenderNetwork(INetwork aNetwork, CpDevice aDevice)
            : base(aNetwork)
        {
            iSubscribed = new ManualResetEvent(false);
            iService = new CpProxyAvOpenhomeOrgSender1(aDevice);

            iService.SetPropertyAudioChanged(HandleAudioChanged);
            iService.SetPropertyMetadataChanged(HandleMetadataChanged);
            iService.SetPropertyStatusChanged(HandleStatusChanged);

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
            iPresentationUrl = iService.PropertyPresentationUrl();

            iSubscribed.Set();
        }

        protected override void OnUnsubscribe()
        {
            iService.Unsubscribe();
            iSubscribed.Reset();
        }

        private void HandleAudioChanged()
        {
            Network.Schedule(() =>
            {
                iAudio.Update(iService.PropertyAudio());
            });
        }

        private void HandleMetadataChanged()
        {
            Network.Schedule(() =>
            {
                iMetadata.Update(new SenderMetadata(iService.PropertyMetadata()));
            });
        }

        private void HandleStatusChanged()
        {
            Network.Schedule(() =>
            {
                iStatus.Update(iService.PropertyStatus());
            });
        }

        private ManualResetEvent iSubscribed;
        private CpProxyAvOpenhomeOrgSender1 iService;
    }

    class ServiceSenderMock : ServiceSender, IMockable
    {
        public ServiceSenderMock(INetwork aNetwork, string aAttributes, string aPresentationUrl, bool aAudio, ISenderMetadata aMetadata, string aStatus)
            : base(aNetwork)
        {
            iAttributes = aAttributes;
            iPresentationUrl = aPresentationUrl;

            iAudio.Update(aAudio);
            iMetadata.Update(aMetadata);
            iStatus.Update(aStatus);
        }

        public override void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "attributes")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iAttributes = value.First();
            }
            else if (command == "presentationurl")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iPresentationUrl = value.First();
            }
            else if (command == "audio")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iAudio.Update(bool.Parse(value.First()));
            }
            else if (command == "metadata")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iMetadata.Update(new SenderMetadata(value.First()));
            }
            else if (command == "status")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iStatus.Update(value.First());
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }

    public class ProxySender : Proxy<ServiceSender>, IProxySender
    {
        public ProxySender(IDevice aDevice, ServiceSender aService)
            : base(aDevice, aService)
        {
        }

        public string Attributes
        {
            get { return iService.Attributes; }
        }

        public string PresentationUrl
        {
            get { return iService.PresentationUrl; }
        }

        public IWatchable<bool> Audio
        {
            get { return iService.Audio; }
        }

        public IWatchable<ISenderMetadata> Metadata
        {
            get { return iService.Metadata; }
        }

        public IWatchable<string> Status
        {
            get { return iService.Status; }
        }
    }
}
