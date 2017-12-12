using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FirstScreen : MonoBehaviour 
{
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
        responseText.text = "Test answer";
    }
}
