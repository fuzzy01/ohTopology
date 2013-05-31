﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public class RoomDetails : IInfoDetails
    {
        public RoomDetails()
        {
            iEnabled = false;
        }

        public RoomDetails(IInfoDetails aDetails)
        {
            iEnabled = true;
            iBitDepth = aDetails.BitDepth;
            iBitRate = aDetails.BitRate;
            iCodecName = aDetails.CodecName;
            iDuration = aDetails.Duration;
            iLossless = aDetails.Lossless;
            iSampleRate = aDetails.SampleRate;
        }

        public bool Enabled
        {
            get
            {
                return iEnabled;
            }
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

        private bool iEnabled;
        private uint iBitDepth;
        private uint iBitRate;
        private string iCodecName;
        private uint iDuration;
        private bool iLossless;
        private uint iSampleRate;
    }

    public class RoomMetadata : IInfoMetadata
    {
        public RoomMetadata()
        {
            iEnabled = false;
        }

        public RoomMetadata(IInfoMetadata aMetadata)
        {
            iEnabled = true;
            iMetadata = aMetadata.Metadata;
            iUri = aMetadata.Uri;
        }

        public bool Enabled
        {
            get
            {
                return iEnabled;
            }
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

        private bool iEnabled;
        private IMediaMetadata iMetadata;
        private string iUri;
    }

    public class RoomMetatext : IInfoMetatext
    {
        public RoomMetatext()
        {
            iEnabled = false;
        }

        public RoomMetatext(IInfoMetatext aMetatext)
        {
            iEnabled = true;
            iMetatext = aMetatext.Metatext;
        }

        public bool Enabled
        {
            get
            {
                return iEnabled;
            }
        }

        public string Metatext
        {
            get
            {
                return iMetatext;
            }
        }

        private bool iEnabled;
        private string iMetatext;
    }

    class InfoWatcher : IWatcher<IInfoDetails>, IWatcher<IInfoMetadata>, IWatcher<IInfoMetatext>, IDisposable
    {
        public InfoWatcher(IWatchableThread aThread, IDevice aDevice, Watchable<RoomDetails> aDetails, Watchable<RoomMetadata> aMetadata, Watchable<RoomMetatext> aMetatext)
        {
            iDisposed = false;

            iDevice = aDevice;
            iDetails = aDetails;
            iMetadata = aMetadata;
            iMetatext = aMetatext;

            iDevice.Create<IProxyInfo>((info) =>
            {
                aThread.Schedule(() =>
                {
                    if (!iDisposed)
                    {
                        iInfo = info;

                        iInfo.Details.AddWatcher(this);
                        iInfo.Metadata.AddWatcher(this);
                        iInfo.Metatext.AddWatcher(this);
                    }
                    else
                    {
                        info.Dispose();
                    }
                });
            });
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("InfoWatcher.Dispose");
            }

            if (iInfo != null)
            {
                iInfo.Details.RemoveWatcher(this);
                iInfo.Metadata.RemoveWatcher(this);
                iInfo.Metatext.RemoveWatcher(this);

                iInfo.Dispose();
                iInfo = null;
            }

            iDisposed = true;
        }

        public void ItemOpen(string aId, IInfoDetails aValue)
        {
            iDetails.Update(new RoomDetails(aValue));
        }

        public void ItemUpdate(string aId, IInfoDetails aValue, IInfoDetails aPrevious)
        {
            iDetails.Update(new RoomDetails(aValue));
        }

        public void ItemClose(string aId, IInfoDetails aValue)
        {
            iDetails.Update(new RoomDetails());
        }

        public void ItemOpen(string aId, IInfoMetadata aValue)
        {
            iMetadata.Update(new RoomMetadata(aValue));
        }

        public void ItemUpdate(string aId, IInfoMetadata aValue, IInfoMetadata aPrevious)
        {
            iMetadata.Update(new RoomMetadata(aValue));
        }

        public void ItemClose(string aId, IInfoMetadata aValue)
        {
            iMetadata.Update(new RoomMetadata());
        }

        public void ItemOpen(string aId, IInfoMetatext aValue)
        {
            iMetatext.Update(new RoomMetatext(aValue));
        }

        public void ItemUpdate(string aId, IInfoMetatext aValue, IInfoMetatext aPrevious)
        {
            iMetatext.Update(new RoomMetatext(aValue));
        }

        public void ItemClose(string aId, IInfoMetatext aValue)
        {
            iMetatext.Update(new RoomMetatext());
        }

        private bool iDisposed;
        private IProxyInfo iInfo;

        private IDevice iDevice;
        private Watchable<RoomDetails> iDetails;
        private Watchable<RoomMetadata> iMetadata;
        private Watchable<RoomMetatext> iMetatext;
    }

    public interface IJoinable
    {
        void Join(Action aAction);
        void UnJoin(Action aAction);
    }

    public class RoomSenderMetadata : ISenderMetadata
    {
        public RoomSenderMetadata()
        {
            iEnabled = false;
        }

        public RoomSenderMetadata(ISenderMetadata aMetadata)
        {
            iEnabled = true;
            iName = aMetadata.Name;
            iUri = aMetadata.Uri;
            iArtworkUri = aMetadata.ArtworkUri;
        }

        public bool Enabled
        {
            get { return iEnabled; }
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

        private bool iEnabled;
        private string iName;
        private string iUri;
        private string iArtworkUri;
    }

    public interface IZone
    {
        bool Active { get; }
        string Udn { get; }
        IStandardRoom Room { get; }
        IWatchableUnordered<IStandardRoom> Listeners { get; }

        void AddToZone(IStandardRoom aRoom);
        void RemoveFromZone(IStandardRoom aRoom);
    }

    public class Zone : IZone
    {
        public Zone(bool aActive, string aUdn, StandardRoom aRoom)
        {
            iActive = aActive;
            iUdn = aUdn;
            iRoom = aRoom;

            iListeners = new WatchableUnordered<IStandardRoom>(aRoom.WatchableThread);
        }

        public bool Active
        {
            get
            {
                return iActive;
            }
        }

        public string Udn
        {
            get
            {
                return iUdn;
            }
        }

        public IStandardRoom Room
        {
            get
            {
                return iRoom;
            }
        }

        public IWatchableUnordered<IStandardRoom> Listeners
        {
            get
            {
                return iListeners;
            }
        }

        public void AddToZone(IStandardRoom aRoom)
        {
            if (!iActive)
            {
                throw new NotSupportedException();
            }

            aRoom.Play("Receiver", iUdn);
            iListeners.Add(aRoom);
        }

        public void RemoveFromZone(IStandardRoom aRoom)
        {
            if (!iActive)
            {
                throw new NotSupportedException();
            }

            aRoom.Play("Receiver", string.Empty);
            iListeners.Remove(aRoom);
        }

        private bool iActive;
        private string iUdn;
        private StandardRoom iRoom;
        private WatchableUnordered<IStandardRoom> iListeners;
    }

    public interface IStandardRoom : IJoinable
    {
        string Name { get; }

        IWatchable<EStandby> Standby { get; }
        IWatchable<RoomDetails> Details { get; }
        IWatchable<RoomMetadata> Metadata { get; }
        IWatchable<RoomMetatext> Metatext { get; }

        // multi-room interface
        IWatchable<IZone> Zone { get; }
        IWatchable<bool> Zoneable { get; }

        IWatchable<ITopology4Source> Source { get; }
        IWatchable<IEnumerable<ITopology4Source>> Sources { get; }
        IEnumerable<ITopology4Group> Senders { get; }

        void SetStandby(bool aValue);
        void Play(string aMode, string aUri);

        IWatchableThread WatchableThread { get; }
    }

    public class StandardRoom : IStandardRoom, IWatcher<IEnumerable<ITopology4Root>>, IWatcher<IEnumerable<ITopology4Group>>, IWatcher<ITopology4Source>, IDisposable
    {
        public StandardRoom(IWatchableThread aThread, StandardHouse aHouse, ITopology4Room aRoom)
        {
            iDisposed = false;

            iThread = aThread;
            iHouse = aHouse;
            iRoom = aRoom;

            iJoiners = new List<Action>();
            iRoots = new List<ITopology4Root>();
            iCurrentSources = new List<ITopology4Source>();
            iSources = new List<ITopology4Source>();
            iSenders = new List<ITopology4Group>();

            iDetails = new Watchable<RoomDetails>(iThread, "Details", new RoomDetails());
            iMetadata = new Watchable<RoomMetadata>(iThread, "Metadata", new RoomMetadata());
            iMetatext = new Watchable<RoomMetatext>(iThread, "Metatext", new RoomMetatext());

            iZoneable = new Watchable<bool>(aThread, "HasReceiver", false);
            iZone = new Watchable<IZone>(aThread, "Zone", new Zone(false, string.Empty, this));

            iRoom.Roots.AddWatcher(this);
        }

        public void Dispose()
        {
            iThread.Execute(() =>
            {
                iRoom.Roots.RemoveWatcher(this);
            });

            List<Action> linked = new List<Action>(iJoiners);
            foreach (Action a in linked)
            {
                a();
            }
            if (iJoiners.Count > 0)
            {
                throw new Exception("iJoiners.Count > 0");
            }
            iJoiners = null;

            iDetails.Dispose();
            iDetails = null;

            iMetadata.Dispose();
            iMetadata = null;

            iMetatext.Dispose();
            iMetatext = null;

            iZoneable.Dispose();
            iZoneable = null;

            iZone.Dispose();
            iZone = null;

            iRoots = null;
            iRoom = null;
            iHouse = null;
            iThread = null;

            iDisposed = true;
        }

        public string Name
        {
            get
            {
                return iRoom.Name;
            }
        }

        public IWatchable<EStandby> Standby
        {
            get
            {
                return iRoom.Standby;
            }
        }

        public IWatchable<RoomDetails> Details
        {
            get
            {
                return iDetails;
            }
        }

        public IWatchable<RoomMetadata> Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public IWatchable<RoomMetatext> Metatext
        {
            get
            {
                return iMetatext;
            }
        }

        public IWatchable<ITopology4Source> Source
        {
            get
            {
                return iWatchableSource;
            }
        }

        public IWatchable<IEnumerable<ITopology4Source>> Sources
        {
            get
            {
                return iWatchableSources;
            }
        }

        public void Join(Action aAction)
        {
            iJoiners.Add(aAction);
        }

        public void UnJoin(Action aAction)
        {
            iJoiners.Remove(aAction);
        }

        public void SetStandby(bool aValue)
        {
            iRoom.SetStandby(aValue);
        }

        public IWatchable<IZone> Zone
        {
            get
            {
                return iZone;
            }
        }

        public IWatchable<bool> Zoneable
        {
            get
            {
                return iZoneable;
            }
        }

        public IEnumerable<ITopology4Group> Senders
        {
            get
            {
                return iSenders;
            }
        }

        public void Play(string aMode, string aUri)
        {
            if (aMode == "Playlist")
            {
                uint id = uint.Parse(aUri);
                foreach (ITopology4Source s in iCurrentSources)
                {
                    if (s.Type == "Playlist")
                    {
                        s.Device.Create<IProxyPlaylist>((playlist) =>
                        {
                            iThread.Schedule(() =>
                            {
                                playlist.SeekId(id);
                                playlist.Dispose();
                            });
                        });
                        return;
                    }
                }
            }
            else if (aMode == "Radio")
            {
                string[] split = aUri.Split(new char[] { ' ' }, 2);
                if (split.Length == 2)
                {
                    uint id = uint.Parse(split[0]);
                    string uri = split[1];
                    foreach (ITopology4Source s in iCurrentSources)
                    {
                        if (s.Type == "Radio")
                        {
                            s.Device.Create<IProxyRadio>((radio) =>
                            {
                                iThread.Schedule(() =>
                                {
                                    radio.SetId(id, uri);
                                    radio.Dispose();
                                });
                            });
                            return;
                        }
                    }
                }
                else
                {
                    throw new FormatException();
                }
            }
            else if (aMode == "Receiver")
            {
                string udn = aUri;
                foreach (ITopology4Source s in iCurrentSources)
                {
                    if (s.Type == "Receiver")
                    {
                        s.Device.Create<IProxyReceiver>((receiver) =>
                        {
                            ITopology4Group g = iHouse.Sender(udn);
                            g.Device.Create<IProxySender>((sender) =>
                            {
                                iThread.Schedule(() =>
                                {
                                    if (!iDisposed)
                                    {
                                        Task action = receiver.SetSender(sender.Metadata.Value);
                                        action.ContinueWith((Task) => { receiver.Play(); });
                                        receiver.Dispose();
                                        sender.Dispose();
                                    }
                                    else
                                    {
                                        receiver.Dispose();
                                        sender.Dispose();
                                    }
                                });
                            });
                        });
                        return;
                    }
                }
            }
            else if (aMode == "External")
            {
                uint index = uint.Parse(aUri);
                foreach (ITopology4Source s in iCurrentSources)
                {
                    if (s.Index == index)
                    {
                        s.Select();
                        return;
                    }
                }
            }
            else
            {
                throw new NotSupportedException(aMode);
            }
        }

        public void Play(string aUri, IMediaMetadata aMetadata)
        {
            foreach (ITopology4Source s in iCurrentSources)
            {
                if (s.Type == "Radio")
                {
                    s.Device.Create<IProxyRadio>((radio) =>
                    {
                        iThread.Schedule(() =>
                        {
                            radio.SetChannel(aUri, aMetadata);
                            radio.Dispose();
                        });
                    });
                    return;
                }
            }
        }

        public IWatchableThread WatchableThread
        {
            get
            {
                return iThread;
            }
        }

        public void UnorderedOpen()
        {
        }

        public void UnorderedInitialised()
        {
        }

        public void UnorderedClose()
        {
        }

        public void ItemOpen(string aId, IEnumerable<ITopology4Root> aValue)
        {
            iWatchableSources = new Watchable<IEnumerable<ITopology4Source>>(iThread, "Sources", new List<ITopology4Source>());
            
            iRoots = aValue;

            foreach (ITopology4Root r in aValue)
            {
                iSources.AddRange(r.Sources);
                r.Source.AddWatcher(this);
                r.Senders.AddWatcher(this);
            }

            iWatchableSources.Update(iSources);
            SelectFirstSource();

            EvaluateZoneable();
        }

        public void ItemUpdate(string aId, IEnumerable<ITopology4Root> aValue, IEnumerable<ITopology4Root> aPrevious)
        {
            iSources = new List<ITopology4Source>();

            foreach (ITopology4Root r in aPrevious)
            {
                r.Source.RemoveWatcher(this);
                r.Senders.RemoveWatcher(this);
            }

            iRoots = aValue;

            foreach (ITopology4Root r in aValue)
            {
                iSources.AddRange(r.Sources);
                r.Source.AddWatcher(this);
                r.Senders.AddWatcher(this);
            }

            iWatchableSources.Update(iSources);
            SelectSource();

            EvaluateZoneable();
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Root> aValue)
        {
            iRoots = null;

            foreach (ITopology4Root r in aValue)
            {
                r.Source.RemoveWatcher(this);
                r.Senders.RemoveWatcher(this);
            }

            iInfoWatcher.Dispose();
            iInfoWatcher = null;

            iWatchableSources.Dispose();
            iWatchableSources = null;

            iSource = null;

            EvaluateZoneable();
        }

        public void ItemOpen(string aId, ITopology4Source aValue)
        {
            iCurrentSources.Add(aValue);
        }

        public void ItemUpdate(string aId, ITopology4Source aValue, ITopology4Source aPrevious)
        {
            iCurrentSources.Remove(aPrevious);
            iCurrentSources.Add(aValue);

            SelectSource();
        }

        public void ItemClose(string aId, ITopology4Source aValue)
        {
            iCurrentSources.Remove(aValue);
        }

        private void SelectFirstSource()
        {
            ITopology4Source source = iCurrentSources[0];
            iWatchableSource = new Watchable<ITopology4Source>(iThread, "Source", source);
            iInfoWatcher = new InfoWatcher(iThread, source.Device, iDetails, iMetadata, iMetatext);
            iSource = source;
        }

        private void SelectSource()
        {
            ITopology4Source source = iCurrentSources[0];

            foreach (ITopology4Source s in iCurrentSources)
            {
                // if we find the same source as was previously selected
                if (iSource.Index == s.Index && iSource.Device == s.Device)
                {
                    // if same source has different data update the source
                    if (!Topology4SourceComparer.Equals(iSource, source))
                    {
                        iWatchableSource.Update(s);
                        iSource = s;
                        return;
                    }

                    iSource = s;
                    return;
                }
            }

            if (iSource.Device != source.Device)
            {
                iInfoWatcher.Dispose();
                iInfoWatcher = null;

                iInfoWatcher = new InfoWatcher(iThread, source.Device, iDetails, iMetadata, iMetatext);
            }

            iWatchableSource.Update(source);
            iSource = source;
        }

        public void ItemOpen(string aId, IEnumerable<ITopology4Group> aValue)
        {
            iSenders.AddRange(aValue);
            EvaluateZone(aValue);
        }

        public void ItemUpdate(string aId, IEnumerable<ITopology4Group> aValue, IEnumerable<ITopology4Group> aPrevious)
        {
            foreach (ITopology4Group g in aPrevious)
            {
                iSenders.Remove(g);
            }
            iSenders.AddRange(aValue);
            EvaluateZone(aValue);
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Group> aValue)
        {
            foreach (ITopology4Group g in aValue)
            {
                iSenders.Remove(g);
            }
        }

        private void EvaluateZone(IEnumerable<ITopology4Group> aValue)
        {
            if (iRoots.Count() == 1)
            {
                ITopology4Root root = iRoots.First();
                foreach (ITopology4Group g in aValue)
                {
                    if (root == g)
                    {
                        if (!iZone.Value.Active)
                        {
                            iZone.Update(new Zone(true, g.Device.Udn, this));
                        }
                        break;
                    }
                }
            }
            else
            {
                if (iZone.Value.Active)
                {
                    iZone.Update(new Zone(false, string.Empty, this));
                }
            }
        }

        private void EvaluateZoneable()
        {
            foreach (ITopology4Source s in iCurrentSources)
            {
                if (s.Type == "Receiver")
                {
                    iZoneable.Update(true);
                    return;
                }
            }

            iZoneable.Update(false);
        }

        private bool iDisposed;
        private IWatchableThread iThread;
        private StandardHouse iHouse;
        private ITopology4Room iRoom;

        private IEnumerable<ITopology4Root> iRoots;
        private List<Action> iJoiners;
        private ITopology4Source iSource;
        private List<ITopology4Source> iCurrentSources;
        private List<ITopology4Source> iSources;
        private List<ITopology4Group> iSenders;
        private Watchable<ITopology4Source> iWatchableSource;
        private Watchable<IEnumerable<ITopology4Source>> iWatchableSources;

        private Watchable<bool> iZoneable;
        private Watchable<IZone> iZone;

        private InfoWatcher iInfoWatcher;
        private Watchable<RoomDetails> iDetails;
        private Watchable<RoomMetadata> iMetadata;
        private Watchable<RoomMetatext> iMetatext;
    }

    public interface IStandardHouse
    {
        IWatchableOrdered<IStandardRoom> Rooms { get; }
        IWatchableOrdered<IProxyMediaServer> Servers { get; }
    }

    public class StandardHouse : IUnorderedWatcher<ITopology4Room>, IUnorderedWatcher<IDevice>, IStandardHouse, IDisposable
    {
        public StandardHouse(INetwork aNetwork, ITopology4 aTopology4)
        {
            iNetwork = aNetwork;
            iTopology4 = aTopology4;

            iWatchableServers = new WatchableOrdered<IProxyMediaServer>(iNetwork);
            iMediaServers = iNetwork.Create<IProxyMediaServer>();
            iServerLookup = new Dictionary<IDevice, IProxyMediaServer>();
 
            iWatchableRooms = new WatchableOrdered<IStandardRoom>(iNetwork);
            iRoomLookup = new Dictionary<ITopology4Room, StandardRoom>();

            iNetwork.Schedule(() =>
            {
                iMediaServers.AddWatcher(this);
                iTopology4.Rooms.AddWatcher(this);
            });
        }

        public void Dispose()
        {
            if (iTopology4 == null)
            {
                throw new ObjectDisposedException("StandardHouse.Dispose");
            }

            iNetwork.Execute(() =>
            {
                iMediaServers.RemoveWatcher(this);

                foreach (var kvp in iServerLookup)
                {
                    kvp.Value.Dispose();
                }

                iTopology4.Rooms.RemoveWatcher(this);

                foreach (var kvp in iRoomLookup)
                {
                    kvp.Value.Dispose();
                }
            });
            iWatchableRooms.Dispose();
            iWatchableRooms = null;

            iRoomLookup.Clear();
            iRoomLookup = null;

            iWatchableServers.Dispose();
            iWatchableServers = null;

            iServerLookup = null;

            iTopology4 = null;
        }

        public IWatchableOrdered<IStandardRoom> Rooms
        {
            get
            {
                return iWatchableRooms;
            }
        }

        public IWatchableOrdered<IProxyMediaServer> Servers
        {
            get
            {
                return iWatchableServers;
            }
        }

        public ITopology4Group Sender(string aUdn)
        {
            foreach (StandardRoom r in iRoomLookup.Values)
            {
                foreach (ITopology4Group s in r.Senders)
                {
                    if (s.Device.Udn == aUdn)
                    {
                        return s;
                    }
                }
            }

            return null;
        }

        // IUnorderedWatcher<ITopology4Room>

        public void UnorderedOpen()
        {
        }

        public void UnorderedInitialised()
        {
        }

        public void UnorderedAdd(ITopology4Room aRoom)
        {
            StandardRoom room = new StandardRoom(iNetwork, this, aRoom);

            // calculate where to insert the room
            int index = 0;
            foreach (IStandardRoom r in iWatchableRooms.Values)
            {
                if (room.Name.CompareTo(r.Name) < 0)
                {
                    break;
                }
                ++index;
            }

            // insert the room
            iRoomLookup.Add(aRoom, room);
            iWatchableRooms.Add(room, (uint)index);
        }

        public void UnorderedRemove(ITopology4Room aRoom)
        {
            // remove the corresponding Room from the watchable collection
            StandardRoom room = iRoomLookup[aRoom];

            iRoomLookup.Remove(aRoom);
            iWatchableRooms.Remove(room);

            // schedule the Room object for disposal
            iNetwork.Schedule(() =>
            {
                room.Dispose();
            });
        }

        public void UnorderedAdd(IDevice aItem)
        {
            aItem.Create<IProxyMediaServer>((t) =>
            {
                iNetwork.Schedule(() =>
                {
                    // calculate where to insert the server
                    int index = 0;
                    foreach (IProxyMediaServer ms in iWatchableServers.Values)
                    {
                        if (t.ProductName.CompareTo(ms.ProductName) < 0)
                        {
                            break;
                        }
                        ++index;
                    }

                    // insert the server
                    iServerLookup.Add(aItem, t);
                    iWatchableServers.Add(t, (uint)index);
                });
            });
        }

        public void UnorderedRemove(IDevice aItem)
        {
            // remove the corresponding server from the watchable collection
            IProxyMediaServer proxy = iServerLookup[aItem];

            iServerLookup.Remove(aItem);
            iWatchableServers.Remove(proxy);

            // schedule the server object for disposal
            iNetwork.Schedule(() =>
            {
                proxy.Dispose();
            });
        }

        public void UnorderedClose()
        {
        }

        private INetwork iNetwork;
        private ITopology4 iTopology4;

        private WatchableOrdered<IProxyMediaServer> iWatchableServers;
        private IWatchableUnordered<IDevice> iMediaServers;
        private Dictionary<IDevice, IProxyMediaServer> iServerLookup;

        private WatchableOrdered<IStandardRoom> iWatchableRooms;
        private Dictionary<ITopology4Room, StandardRoom> iRoomLookup;
    }
}
