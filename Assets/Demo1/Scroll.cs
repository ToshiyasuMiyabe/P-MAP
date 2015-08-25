using UnityEngine;
using System.Collections;

public class Scroll : MonoBehaviour {
	private Material mMaterial;
	private Vector2 mOffset;
	// Use this for initialization
	void Start () {
		mMaterial = GetComponent<Renderer>().material;
	}
	
	// Update is called once per frame
	void Update () {
		mOffset.x += Time.deltaTime * 0.1f;
		mMaterial.SetTextureOffset("_TexA", mOffset);
	}
}
