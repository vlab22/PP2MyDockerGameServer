namespace shared
{
    public class ValidClientRequest : ASerializable
    {
        public string serverCode;
        
        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(serverCode);
        }

        public override void Deserialize(Packet pPacket)
        {
            serverCode = pPacket.ReadString();
        }
    }
}