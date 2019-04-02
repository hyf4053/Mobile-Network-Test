public static class NetOP{
    public const int None = 0;
    public const int FLAG = 1;
}

[System.Serializable]
public abstract class NetMsg
{
    public byte OperationCode {set;get;}
    public NetMsg(){
        OperationCode = NetOP.None;
    }
}
