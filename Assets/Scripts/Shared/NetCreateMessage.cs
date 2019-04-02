[System.Serializable]
public class NetCreateMessage : NetMsg
{
   public NetCreateMessage(){
       OperationCode = NetOP.FLAG;
   }
   public string information {set;get;}
}
