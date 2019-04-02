
using UnityEngine;

public class IPAddress : MonoBehaviour
{
    public string ipAddress;
    public string ModifyIp(string ipAddress){
        int index1 = ipAddress.IndexOf("::ffff:");
        if(index1 != -1){
            Debug.Log("Find ::ffff:");
            string output = ipAddress.Replace("::ffff:","");
            return output;
        }else{
            return null;
        }
    }
}
