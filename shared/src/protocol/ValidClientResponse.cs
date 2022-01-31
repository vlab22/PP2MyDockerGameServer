namespace shared
{
    public class ValidClientResponse : ASerializable
    {
        public string code;
        
        public override void Serialize(Packet pPacket)
        {
           pPacket.Write(code);
        }

        public override void Deserialize(Packet pPacket)
        {
            code = pPacket.ReadString();
        }
    }
}