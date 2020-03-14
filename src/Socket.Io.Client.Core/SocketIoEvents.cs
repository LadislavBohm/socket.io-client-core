using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using Socket.Io.Client.Core.Model.Response;
using Socket.Io.Client.Core.Model.SocketEvent;
using Socket.Io.Client.Core.Model.SocketIo;

namespace Socket.Io.Client.Core
{
    public class SocketIoEvents : IDisposable
    {
        internal ISubject<DisconnectEvent> DisconnectSubject { get; } = new Subject<DisconnectEvent>();
        internal ISubject<HandshakeResponse> HandshakeSubject { get; } = new Subject<HandshakeResponse>();
        internal ISubject<PongResponse> PongSubject { get; } = new Subject<PongResponse>();
        internal ISubject<ErrorEvent> ErrorSubject { get; } = new Subject<ErrorEvent>();
        internal ISubject<Unit> OpenSubject { get; } = new Subject<Unit>();
        internal ISubject<Packet> PacketSubject { get; } = new Subject<Packet>();
        internal ISubject<ProbeErrorEvent> ProbeErrorSubject { get; } = new Subject<ProbeErrorEvent>();
        internal ISubject<Unit> ConnectSubject { get; } = new Subject<Unit>();
        internal ISubject<EventMessageEvent> EventMessageSubject { get; } = new Subject<EventMessageEvent>();
        internal ISubject<AckMessageEvent> AckMessageSubject { get; } = new Subject<AckMessageEvent>();


        public IObservable<HandshakeResponse> OnHandshake => HandshakeSubject.AsObservable();
        public IObservable<DisconnectEvent> OnDisconnect => DisconnectSubject.AsObservable();
        public IObservable<PongResponse> OnPong => PongSubject.AsObservable();
        public IObservable<ErrorEvent> OnError => ErrorSubject.AsObservable();
        public IObservable<Unit> OnOpen => OpenSubject.AsObservable();
        public IObservable<Packet> OnPacket => PacketSubject.AsObservable();
        public IObservable<ProbeErrorEvent> OnProbeError => ProbeErrorSubject.AsObservable();
        public IObservable<Unit> OnConnect => ConnectSubject.AsObservable();

        public void Dispose()
        {
            var subjects = typeof(SocketIoEvents).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(p => 
                    p.PropertyType.IsGenericType && 
                    p.PropertyType.GetGenericTypeDefinition() == typeof(ISubject<>))
                .Select(p => p.GetValue(this))
                .OfType<IDisposable>()
                .ToList();

            foreach (IDisposable subject in subjects)
            {
                subject.Dispose();
            }
        }
    }
}
