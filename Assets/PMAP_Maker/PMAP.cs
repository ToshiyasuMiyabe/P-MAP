using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;



[System.Serializable]
public class PMAP : MonoBehaviour
{
	[System.Serializable]
	public class Layer
	{
		public enum BlendType { Alpha, Add, Premultiplied};
		public Texture2D	mTexture;
		public TextureImporter mTextureImporter;
		public TextureImporterFormat mTextureFormat;	//一時的なバックアップ用
		public FilterMode mFilterMode;					//一時的なバックアップ用
		public bool mIsReadable;						//一時的なバックアップ用

		public BlendType	mBlendType = BlendType.Alpha;
		public float		mTransparency = 100.0f;
		public Layer Clone()
		{
			return (Layer)MemberwiseClone();
		}
	}

	[HideInInspector]	public List<Layer>	mLayers = new List<Layer>(0);
	[HideInInspector]	public bool			mAddProtect;	//加算部分の保護(スプライト用)
}

