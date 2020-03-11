namespace Socket.Io.Client.Core.Model
{
    public class Packet
    {
        internal Packet(PacketType type, PacketSubType? subType)
        {
            Type = type;
            SubType = subType;
        }

        public Packet(PacketType type, PacketSubType? subType, string ns, string data, int? id, int attachments, string query)
        {
            Id = id;
            Type = type;
            SubType = subType;
            Namespace = ns;
            Data = data;
            Attachments = attachments;
            Query = query;
        }
        
        public int? Id { get; }
        public PacketType Type { get; }
        public PacketSubType? SubType { get; }
        public string Namespace { get; }
        public string Data { get; }
        public int Attachments { get; }
        public string Query { get; }
    
        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Type)}: {Type}, {nameof(Namespace)}: {Namespace}, {nameof(Data)}: {Data}";
        }
    
        public Packet WithNamespace(string ns) => new Packet(Type, SubType, ns, Data, Id, Attachments, Query);
    }
}