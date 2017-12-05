using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FirstScreen : MonoBehaviour 
{
    public static NetworkClient NetworkClient = new NetworkClient();

    [SerializeField] GameObject secondScreenPrefab;
    [SerializeField] Text responseText;

    public void OpenSecondScreen()
    {
        var s = Object.Instantiate(secondScreenPrefab);
        s.name = secondScreenPrefab.name;
        s.transform.SetParent(transform.parent, false);
    }

    public void SendNetworkRequest()
    {
        responseText.text = NetworkClient.SendServerRequest("i_need_data");
    }
}
