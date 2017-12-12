using System.Collections;
using UnityEngine;

public class ObjectThatWillBeDestroyedInSecond : MonoBehaviour {

	IEnumerator Start ()
	{
		yield return new WaitForSeconds(1);
		Destroy(gameObject);
	}
}
