using System.Text;

namespace Socket.Io.Client.Core.Model.SocketIo
{
    public class Packet
    {
        internal Packet(EngineIoType engineIoType, SocketIoType? socketIoType)
        {
            EngineIoType = engineIoType;
            SocketIoType = socketIoType;
        }

        public Packet(EngineIoType engineIoType, SocketIoType? socketIoType, string ns, string data, int? id, int attachments, string query)
        {
            Id = id;
            EngineIoType = engineIoType;
            SocketIoType = socketIoType;
            Namespace = ns;
            Data = data;
            Attachments = attachments;
            Query = query;
        }
        
        public int? Id { get; }
        public EngineIoType EngineIoType { get; }
        public SocketIoType? SocketIoType { get; }
        public string Namespace { get; }
        public string Data { get; }
        public int Attachments { get; }
        public string Query { get; }
    
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (EngineIoType != EngineIoType.Open) sb.Append((int) EngineIoType);
            if (SocketIoType.HasValue) sb.Append((int) SocketIoType.Value);
            if (Id.HasValue) sb.Append(Id.Value);
            if (Namespace != Core.SocketIo.DefaultNamespace) sb.Append(Namespace);
            sb.Append(Data);
            return sb.ToString();
        }
    
        public Packet WithNamespace(string ns) => new Packet(EngineIoType, SocketIoType, ns, Data, Id, Attachments, Query);
    }
}