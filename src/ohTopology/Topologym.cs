﻿using System;
using System.Xml;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint;

namespace OpenHome.Av
{
    public interface ITopologymSender
    {
        bool Enabled { get; }
        IDevice Device { get; }
    }

    class TopologymSender : ITopologymSender
    {
        public static readonly TopologymSender Empty = new TopologymSender();

        private readonly bool iEnabled;
        private readonly IDevice iDevice;

        private TopologymSender()
        {
            iEnabled = false;
        }

        public TopologymSender(IDevice aDevice)
        {
            iEnabled = true;
            iDevice = aDevice;
        }

        public bool Enabled
        {
            get
            {
                return iEnabled;
            }
        }

        public IDevice Device
        {
            get
            {
                return iDevice;
            }
        }
    }

    public interface ITopologymGroup
    {
        string Id { get; }
        string Attributes { get; }
        IDevice Device { get; }

        IWatchable<string> Room { get; }
        IWatchable<string> Name { get; }
        IWatchable<bool> Standby { get; }
        IWatchable<uint> SourceIndex { get; }
        IEnumerable<IWatchable<ITopology2Source>> Sources { get; }
        IWatchable<ITopologymSender> Sender { get; }

        void SetStandby(bool aValue);
        void SetSourceIndex(uint aValue);
    }

    class TopologymGroup : ITopologymGroup
    {
        public TopologymGroup(INetwork aNetwork, ITopology2Group aGroup)
        {
            iGroup = aGroup;
            iSender = new Watchable<ITopologymSender>(aNetwork, "Sender", TopologymSender.Empty);
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("TopologymGroup.Dispose");
            }

            iDisposed = true;
        }

        public string Id
        {
            get
            {
                return iGroup.Id;
            }
        }

        public string Attributes
        {
            get
            {
                return iGroup.Attributes;
            }
        }

        public IDevice Device
        {
            get
            {
                return iGroup.Device;
            }
        }

        public IWatchable<string> Room
        {
            get
            {
                return iGroup.Room;
            }
        }

        public IWatchable<string> Name
        {
            get
            {
                return iGroup.Name;
            }
        }

        public IWatchable<bool> Standby
        {
            get
            {
                return iGroup.Standby;
            }
        }

        public IWatchable<uint> SourceIndex
        {
            get
            {
                return iGroup.SourceIndex;
            }
        }

        public IEnumerable<IWatchable<ITopology2Source>> Sources
        {
            get
            {
                return iGroup.Sources;
            }
        }

        public IWatchable<ITopologymSender> Sender
        {
            get
            {
                return iSender;
            }
        }

        public void SetStandby(bool aValue)
        {
            iGroup.SetStandby(aValue);
        }

        public void SetSourceIndex(uint aValue)
        {
            iGroup.SetSourceIndex(aValue);
        }

        internal void SetSender(ITopologymSender aSender)
        {
            iSender.Update(aSender);
        }

        private bool iDisposed;
        private ITopology2Group iGroup;
        private Watchable<ITopologymSender> iSender;
    }

    class ReceiverWatcher : IWatcher<string>, IWatcher<IInfoMetadata>, IWatcher<ITopology2Source>, IDisposable
    {
        private Topologym iTopology;
        private TopologymGroup iGroup;
        private IProxyReceiver iReceiver;
        private string iTransportState;
        private IInfoMetadata iMetadata;

        public ReceiverWatcher(Topologym aTopology, TopologymGroup aGroup)
        {
            iTopology = aTopology;
            iGroup = aGroup;

            foreach (IWatchable<ITopology2Source> s in iGroup.Sources)
            {
                s.AddWatcher(this);
            }
        }

        public void Dispose()
        {
            foreach (IWatchable<ITopology2Source> s in iGroup.Sources)
            {
                s.RemoveWatcher(this);
            }

            if (iReceiver != null)
            {
                iReceiver.TransportState.RemoveWatcher(this);
                iReceiver.Metadata.RemoveWatcher(this);

                iReceiver.Dispose();
                iReceiver = null;
            }

            SetSender(TopologymSender.Empty);

            iGroup = null;
            iTopology = null;
        }

        public string ListeningToUri
        {
            get
            {
                if (string.IsNullOrEmpty(iTransportState) || iTransportState != "Playing")
                {
                    return null;
                }
                return iMetadata.Uri;
            }
        }

        public void SetSender(ITopologymSender aSender)
        {
            iGroup.SetSender(aSender);
        }

        public void ItemOpen(string aId, string aValue)
        {
            iTransportState = aValue;
        }

        public void ItemUpdate(string aId, string aValue, string aPrevious)
        {
            iTransportState = aValue;
            iTopology.ReceiverChanged(this);
        }

        public void ItemClose(string aId, string aValue)
        {
            iTransportState = null;
        }

        public void ItemOpen(string aId, IInfoMetadata aValue)
        {
            iMetadata = aValue;
        }

        public void ItemUpdate(string aId, IInfoMetadata aValue, IInfoMetadata aPrevious)
        {
            iMetadata = aValue;
            iTopology.ReceiverChanged(this);
        }

        public void ItemClose(string aId, IInfoMetadata aValue)
        {
            iMetadata = null;
        }

        public void ItemOpen(string aId, ITopology2Source aValue)
        {
            if (aValue.Type == "Receiver")
            {
                iGroup.Device.Create<IProxyReceiver>((receiver) =>
                {
                    iReceiver = receiver;
                    iReceiver.TransportState.AddWatcher(this);
                    iReceiver.Metadata.AddWatcher(this);
                    iTopology.ReceiverChanged(this);
                });
            }
        }

        public void ItemUpdate(string aId, ITopology2Source aValue, ITopology2Source aPrevious)
        {
        }

        public void ItemClose(string aId, ITopology2Source aValue)
        {
        }
    }

    class SenderWatcher : IWatcher<ISenderMetadata>, IWatcher<string>, IDisposable
    {
        private Topologym iTopology;
        private IDevice iDevice;
        private IProxySender iSender;
        private bool iEnabled;
        private ISenderMetadata iMetadata;

        public SenderWatcher(Topologym aTopology, ITopology2Group aGroup)
        {
            iTopology = aTopology;
            iDevice = aGroup.Device;
            iMetadata = SenderMetadata.Empty;

            aGroup.Device.Create<IProxySender>((sender) =>
            {
                iSender = sender;
                iSender.Status.AddWatcher(this);
                iSender.Metadata.AddWatcher(this);
                iTopology.SenderChanged(this, SenderMetadata.Empty);
            });
        }

        public void Dispose()
        {
            ISenderMetadata previous = iMetadata;

            if (iSender != null)
            {
                iSender.Status.RemoveWatcher(this);
                iSender.Metadata.RemoveWatcher(this);

                iSender.Dispose();
                iSender = null;
            }

            iTopology.SenderChanged(this, previous);

            iDevice = null;
            iTopology = null;
        }

        public bool Enabled
        {
            get
            {
                return iEnabled;
            }
        }

        public string Uri
        {
            get
            {
                return iMetadata.Uri;
            }
        }

        public IDevice Device
        {
            get
            {
                return iDevice;
            }
        }

        public void ItemOpen(string aId, ISenderMetadata aValue)
        {
            iMetadata = aValue;
        }

        public void ItemUpdate(string aId, ISenderMetadata aValue, ISenderMetadata aPrevious)
        {
            iMetadata = aValue;
            iTopology.SenderChanged(this, aPrevious);
        }

        public void ItemClose(string aId, ISenderMetadata aValue)
        {
            iMetadata = SenderMetadata.Empty;
        }

        public void ItemOpen(string aId, string aValue)
        {
            iEnabled = (aValue == "Enabled");
        }

        public void ItemUpdate(string aId, string aValue, string aPrevious)
        {
            iEnabled = (aValue == "Enabled");
            iTopology.SenderChanged(this, null);
        }

        public void ItemClose(string aId, string aValue)
        {
            iEnabled = false;
        }
    }

    public interface ITopologym
    {
        IWatchableUnordered<ITopologymGroup> Groups { get; }
        INetwork Network { get; }
    }

    public class Topologym : ITopologym, IUnorderedWatcher<ITopology2Group>, IDisposable
    {
        public Topologym(ITopology2 aTopology2)
        {
            iDisposed = false;

            iNetwork = aTopology2.Network;
            iTopology2 = aTopology2;

            iGroups = new WatchableUnordered<ITopologymGroup>(iNetwork);

            iGroupLookup = new Dictionary<ITopology2Group, TopologymGroup>();
            iReceiverLookup = new Dictionary<ITopology2Group, ReceiverWatcher>();
            iSenderLookup = new Dictionary<ITopology2Group, SenderWatcher>();

            iNetwork.Schedule(() =>
            {
                iTopology2.Groups.AddWatcher(this);
            });
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Topologym.Dispose");
            }

            iNetwork.Execute(() =>
            {
                iTopology2.Groups.RemoveWatcher(this);

                foreach (TopologymGroup g in iGroupLookup.Values)
                {
                    g.Dispose();
                }

                foreach (ReceiverWatcher r in iReceiverLookup.Values)
                {
                    r.Dispose();
                }

                foreach (SenderWatcher s in iSenderLookup.Values)
                {
                    s.Dispose();
                }
            });
            iTopology2 = null;

            iGroupLookup = null;
            iReceiverLookup = null;
            iSenderLookup = null;

            iGroups.Dispose();
            iGroups = null;

            iDisposed = true;
        }

        public IWatchableUnordered<ITopologymGroup> Groups
        {
            get
            {
                return iGroups;
            }
        }

        public INetwork Network
        {
            get
            {
                return iNetwork;
            }
        }

        public void UnorderedOpen()
        {
        }

        public void UnorderedInitialised()
        {
        }

        public void UnorderedAdd(ITopology2Group aItem)
        {
            TopologymGroup group = new TopologymGroup(iNetwork, aItem);
            iReceiverLookup.Add(aItem, new ReceiverWatcher(this, group));
            if (aItem.Attributes.Contains("Sender"))
            {
                iSenderLookup.Add(aItem, new SenderWatcher(this, aItem));
            }
            iGroupLookup.Add(aItem, group);
            iGroups.Add(group);
        }

        public void UnorderedRemove(ITopology2Group aItem)
        {
            TopologymGroup group;
            if (iGroupLookup.TryGetValue(aItem, out group))
            {
                // schedule higher layer notification
                iGroups.Remove(group);
                iGroupLookup.Remove(aItem);

                iReceiverLookup[aItem].Dispose();
                iReceiverLookup.Remove(aItem);

                iSenderLookup[aItem].Dispose();
                iSenderLookup.Remove(aItem);

                group.Dispose();
            }
        }

        public void UnorderedClose()
        {
        }

        internal void ReceiverChanged(ReceiverWatcher aReceiver)
        {
            foreach (SenderWatcher s in iSenderLookup.Values)
            {
                if (string.IsNullOrEmpty(aReceiver.ListeningToUri))
                {
                    aReceiver.SetSender(TopologymSender.Empty);
                }
                else if (aReceiver.ListeningToUri == s.Uri)
                {
                    // set TopologymGroup sender
                    aReceiver.SetSender(new TopologymSender(s.Device));
                }
            }
        }

        internal void SenderChanged(SenderWatcher aSender, ISenderMetadata aPrevious)
        {
            foreach (ReceiverWatcher r in iReceiverLookup.Values)
            {
                if (aPrevious != null && aPrevious.Uri == r.ListeningToUri)
                {
                    r.SetSender(TopologymSender.Empty);
                }
                else if (aSender.Uri == r.ListeningToUri)
                {
                    // set TopologymGroup sender
                    if (aSender.Enabled)
                    {
                        r.SetSender(new TopologymSender(aSender.Device));
                    }
                    else
                    {
                        r.SetSender(TopologymSender.Empty);
                    }
                }
            }
        }

        private bool iDisposed;

        private INetwork iNetwork;
        private ITopology2 iTopology2;
        private Dictionary<ITopology2Group, TopologymGroup> iGroupLookup;
        private Dictionary<ITopology2Group, ReceiverWatcher> iReceiverLookup;
        private Dictionary<ITopology2Group, SenderWatcher> iSenderLookup;
        private WatchableUnordered<ITopologymGroup> iGroups;
    }
}
