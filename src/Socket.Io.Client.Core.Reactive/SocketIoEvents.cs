using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Socket.Io.Client.Core.Reactive.Model.Response;
using Socket.Io.Client.Core.Reactive.Model.SocketEvent;
using Socket.Io.Client.Core.Reactive.Model.SocketIo;

namespace Socket.Io.Client.Core.Reactive
{
    public class SocketIoEvents
    {
        internal ISubject<DisconnectEvent> DisconnectSubject { get; } = new Subject<DisconnectEvent>();
        internal ISubject<HandshakeResponse> HandshakeSubject { get; } = new Subject<HandshakeResponse>();
        internal ISubject<PongResponse> PongSubject { get; } = new Subject<PongResponse>();
        internal ISubject<ErrorEvent> ErrorSubject { get; } = new Subject<ErrorEvent>();
        internal ISubject<Unit> OpenSubject { get; } = new Subject<Unit>();
        internal ISubject<Packet> PacketSubject { get; } = new Subject<Packet>();
        internal ISubject<ProbeErrorEvent> ProbeErrorSubject { get; } = new Subject<ProbeErrorEvent>();

        public IObservable<HandshakeResponse> OnHandshake => HandshakeSubject.AsObservable();
        public IObservable<DisconnectEvent> OnDisconnect => DisconnectSubject.AsObservable();
        public IObservable<PongResponse> OnPong => PongSubject.AsObservable();
        public IObservable<ErrorEvent> OnError => ErrorSubject.AsObservable();
        public IObservable<Unit> OnOpen => OpenSubject.AsObservable();
        public IObservable<Packet> OnPacket => PacketSubject.AsObservable();
        public IObservable<ProbeErrorEvent> OnProbeError => ProbeErrorSubject.AsObservable();
    }
}
