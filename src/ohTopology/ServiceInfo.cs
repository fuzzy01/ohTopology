﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Net.ControlPoint;
using OpenHome.Os;

namespace OpenHome.Av
{
    public interface IInfoDetails
    {
        uint BitDepth { get; }
        uint BitRate { get; }
        string CodecName { get; }
        uint Duration { get; }
        bool Lossless { get; }
        uint SampleRate { get; }
    }

    public interface IInfoMetadata
    {
        IMediaMetadata Metadata { get; }
        string Uri { get; }
    }

    public interface IInfoMetatext
    {
        string Metatext { get; }
    }

    public interface IProxyInfo : IProxy
    {
        IWatchable<IInfoDetails> Details { get; }
        IWatchable<IInfoMetadata> Metadata { get; }
        IWatchable<IInfoMetatext> Metatext { get; }
    }

    public class InfoDetails : IInfoDetails
    {
        internal InfoDetails()
        {
            iBitDepth = 0;
            iBitRate = 0;
            iCodecName = string.Empty;
            iDuration = 0;
            iLossless = false;
            iSampleRate = 0;
        }

        public InfoDetails(uint aBitDepth, uint aBitRate, string aCodecName, uint aDuration, bool aLossless, uint aSampleRate)
        {
            iBitDepth = aBitDepth;
            iBitRate = aBitRate;
            iCodecName = aCodecName;
            iDuration = aDuration;
            iLossless = aLossless;
            iSampleRate = aSampleRate;
        }

        public uint BitDepth
        {
            get
            {
                return iBitDepth;
            }
        }

        public uint BitRate
        {
            get
            {
                return iBitRate;
            }
        }

        public string CodecName
        {
            get
            {
                return iCodecName;
            }
        }

        public uint Duration
        {
            get
            {
                return iDuration;
            }
        }

        public bool Lossless
        {
            get
            {
                return iLossless;
            }
        }

        public uint SampleRate
        {
            get
            {
                return iSampleRate;
            }
        }

        private uint iBitDepth;
        private uint iBitRate;
        private string iCodecName;
        private uint iDuration;
        private bool iLossless;
        private uint iSampleRate;
    }

    public class InfoMetadata : IInfoMetadata
    {
        public static readonly IInfoMetadata Empty = new InfoMetadata();
        
        private InfoMetadata()
        {
            iMetadata = null;
            iUri = null;
        }

        public InfoMetadata(IMediaMetadata aMetadata, string aUri)
        {
            iMetadata = aMetadata;
            iUri = aUri;
        }

        public IMediaMetadata Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public string Uri
        {
            get
            {
                return iUri;
            }
        }

        private IMediaMetadata iMetadata;
        private string iUri;
    }

    public class InfoMetatext : IInfoMetatext
    {
        internal InfoMetatext()
        {
            iMetatext = string.Empty;
        }

        public InfoMetatext(string aMetatext)
        {
            iMetatext = aMetatext;
        }

        public string Metatext
        {
            get
            {
                return iMetatext;
            }
        }

        private string iMetatext;
    }

    public abstract class ServiceInfo : Service
    {
        protected ServiceInfo(INetwork aNetwork)
            : base(aNetwork)
        {
            iDetails = new Watchable<IInfoDetails>(Network, "Details", new InfoDetails());
            iMetadata = new Watchable<IInfoMetadata>(Network, "Metadata", InfoMetadata.Empty);
            iMetatext = new Watchable<IInfoMetatext>(Network, "Metatext", new InfoMetatext());
        }

        public override void Dispose()
        {
            base.Dispose();

            iDetails.Dispose();
            iDetails = null;

            iMetadata.Dispose();
            iMetadata = null;

            iMetatext.Dispose();
            iMetatext = null;
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return new ProxyInfo(aDevice, this);
        }

        public IWatchable<IInfoDetails> Details
        {
            get
            {
                return iDetails;
            }
        }

        public IWatchable<IInfoMetadata> Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public IWatchable<IInfoMetatext> Metatext
        {
            get
            {
                return iMetatext;
            }
        }

        protected Watchable<IInfoDetails> iDetails;
        protected Watchable<IInfoMetadata> iMetadata;
        protected Watchable<IInfoMetatext> iMetatext;
    }

    class ServiceInfoNetwork : ServiceInfo
    {
        public ServiceInfoNetwork(INetwork aNetwork, CpDevice aDevice)
            : base(aNetwork)
        {
            iThread = aNetwork;
            iSubscribed = new ManualResetEvent(false);
            iService = new CpProxyAvOpenhomeOrgInfo1(aDevice);

            iService.SetPropertyBitDepthChanged(HandleDetailsChanged);
            iService.SetPropertyMetadataChanged(HandleMetadataChanged);
            iService.SetPropertyMetatextChanged(HandleMetatextChanged);

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
            iSubscribed.Set();
        }

        protected override void OnUnsubscribe()
        {
            iService.Unsubscribe();
            iSubscribed.Reset();
        }

        private void HandleDetailsChanged()
        {
            iThread.Schedule(() =>
            {
                iDetails.Update(
                    new InfoDetails(
                        iService.PropertyBitDepth(),
                        iService.PropertyBitRate(),
                        iService.PropertyCodecName(),
                        iService.PropertyDuration(),
                        iService.PropertyLossless(),
                        iService.PropertySampleRate()
                    ));
            });
        }

        private void HandleMetadataChanged()
        {
            iThread.Schedule(() =>
            {
                iMetadata.Update(
                    new InfoMetadata(
                        Network.TagManager.FromDidlLite(iService.PropertyMetadata()),
                        iService.PropertyUri()
                    ));
            });
        }

        private void HandleMetatextChanged()
        {
            iThread.Schedule(() =>
            {
                iMetatext.Update(new InfoMetatext(iService.PropertyMetatext()));
            });
        }

        private IWatchableThread iThread;
        private ManualResetEvent iSubscribed;
        private CpProxyAvOpenhomeOrgInfo1 iService;
    }

    class ServiceInfoMock : ServiceInfo, IMockable
    {
        public ServiceInfoMock(INetwork aNetwork, IInfoDetails aDetails, IInfoMetadata aMetadata, IInfoMetatext aMetatext)
            : base(aNetwork)
        {
            iDetails.Update(aDetails);
            iMetadata.Update(aMetadata);
            iMetatext.Update(aMetatext);
        }

        public override void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "details")
            {
                IEnumerable<string> value = aValue.Skip(1);
                if (value.Count() != 6)
                {
                    throw new NotSupportedException();
                }
                IInfoDetails details = new InfoDetails(
                    uint.Parse(value.ElementAt(0)),
                    uint.Parse(value.ElementAt(1)),
                    value.ElementAt(2),
                    uint.Parse(value.ElementAt(3)),
                    bool.Parse(value.ElementAt(4)),
                    uint.Parse(value.ElementAt(5)));
                iDetails.Update(details);
            }
            else if (command == "metadata")
            {
                IEnumerable<string> value = aValue.Skip(1);
                if (value.Count() != 2)
                {
                    throw new NotSupportedException();
                }
                
                /*XmlDocument document = new XmlDocument();
                XmlNamespaceManager nsManager = new XmlNamespaceManager(document.NameTable);
                nsManager.AddNamespace("didl", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");
                nsManager.AddNamespace("upnp", "urn:schemas-upnp-org:metadata-1-0/upnp/");
                nsManager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
                nsManager.AddNamespace("ldl", "urn:linn-co-uk/DIDL-Lite");

                XmlNode didl = document.CreateElement("DIDL-Lite", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");

                XmlNode item = document.CreateElement("item", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");

                XmlNode title = document.CreateElement("dc:title", "http://purl.org/dc/elements/1.1/");
                title.AppendChild(document.CreateTextNode(value.ElementAt(0)));
                item.AppendChild(title);

                XmlNode c = document.CreateElement("upnp:class", "urn:schemas-upnp-org:metadata-1-0/upnp/");
                c.AppendChild(document.CreateTextNode("object.item.audioItem"));
                item.AppendChild(c);

                XmlNode res = document.CreateElement("res", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");
                res.AppendChild(document.CreateTextNode(value.ElementAt(1)));
                item.AppendChild(res);

                didl.AppendChild(item);

                document.AppendChild(didl);*/

                IInfoMetadata metadata = new InfoMetadata(Network.TagManager.FromDidlLite(value.ElementAt(0)), value.ElementAt(1));
                iMetadata.Update(metadata);
            }
            else if (command == "metatext")
            {
                IEnumerable<string> value = aValue.Skip(1);
                if (value.Count() != 1)
                {
                    throw new NotSupportedException();
                }
                IInfoMetatext metatext = new InfoMetatext(value.ElementAt(0));
                iMetatext.Update(metatext);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }

    public class ProxyInfo : Proxy<ServiceInfo>, IProxyInfo
    {
        public ProxyInfo(IDevice aDevice, ServiceInfo aService)
            : base(aDevice, aService)
        {
        }

        public IWatchable<IInfoDetails> Details
        {
            get { return iService.Details; }
        }

        public IWatchable<IInfoMetadata> Metadata
        {
            get { return iService.Metadata; }
        }

        public IWatchable<IInfoMetatext> Metatext
        {
            get { return iService.Metatext; }
        }
    }
}
