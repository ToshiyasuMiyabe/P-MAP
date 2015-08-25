using UnityEngine;
using System.Collections;

public class RootObject : MonoBehaviour {
	public GameObject AlpObject;
	public GameObject AddObject;
	public int Num = 100;
	public float Range = 5.0f;
	// Use this for initialization
	void Start () {
		for(int i = 0; i < Num; i++)
		{
			Vector3 _Ofs = new Vector3(Random.Range(-Range,Range), Random.Range(-Range,Range), Random.Range(-Range,Range));
			GameObject _Alp = (GameObject)Instantiate (AlpObject, transform.position + _Ofs, transform.rotation);
			_Alp.transform.SetParent(transform, true );

			_Ofs = new Vector3(Random.Range(-Range,Range), Random.Range(-Range,Range), Random.Range(-Range,Range));
			GameObject _Add = (GameObject)Instantiate (AddObject, transform.position + _Ofs, transform.rotation);
			_Add.transform.SetParent(transform, true );
		}
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(Vector3.up * Time.deltaTime * 10.0f);
	}
}
