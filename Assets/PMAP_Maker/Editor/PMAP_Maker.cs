using UnityEngine;
using UnityEditor;              //エディタクラス系用
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode()]
public class PMAP_Maker: EditorWindow 
{
	List<PMAP.Layer>		mLayers = new List<PMAP.Layer>();
	bool					mAddProtect = false;
	Vector2					mScrollPos;
	string					mFilePath = "";
	[MenuItem("PMAP/PMAP_Maker")]
    static void Open()
    {
		PMAP_Maker mWindow = (PMAP_Maker)EditorWindow.GetWindow(typeof(PMAP_Maker));
		mWindow.minSize = new Vector2(320.0f, 320.0f);
//		mWindow.maxSize = new Vector2(480.0f, 480.0f);

		mWindow.OnSelectionChange();
	}
	void OnDestroy()
    {
    }

	void OnSelectionChange ()
	{
		if(Selection.activeObject)
		{
			System.Type _t = Selection.activeObject.GetType() ;
			if(!((_t.Name == "Texture2D") || (_t.Name == "GameObject"))) return;	//テクスチャ・プレハブ以外は無効
			//プレハブ読み込み
			string _Path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
			_Path = System.IO.Path.GetDirectoryName(_Path) + "/" + System.IO.Path.GetFileNameWithoutExtension(_Path); 
			GameObject _Prefab = AssetDatabase.LoadAssetAtPath(_Path + ".prefab", typeof(GameObject)) as GameObject;
			if(_Prefab)
			{
                if (!_Prefab.GetComponent<PMAP>())	return;

				mFilePath = _Path;
				mLayers.Clear();
				foreach (PMAP.Layer Class in _Prefab.GetComponent<PMAP>().mLayers)
				{
					PMAP.Layer StructTest = Class.Clone();
					mLayers.Add(StructTest);
				}
				mAddProtect = _Prefab.GetComponent<PMAP>().mAddProtect;
				Repaint();	//再描画
			}
		}
	}
	    
    public void DropAreaWindow()
    {//ウィンドウ領域内のドラッグ＆ドロップ
        Rect _Rect = new Rect(0, 0, this.position.xMax, this.position.yMax);
        Event _Event = Event.current;
        switch (_Event.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!_Rect.Contains(_Event.mousePosition))
                    return;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (_Event.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (Object _Object in DragAndDrop.objectReferences)
                    {
						if(_Object.GetType().Name == "Texture2D")
						{//投げられたテクスチャを追加
							PMAP.Layer _Layer = new PMAP.Layer();
							_Layer.mTexture = (Texture2D)_Object;
							mLayers.Insert(0,_Layer);
						}
					}
				}
                break;
        }
    }

	void OnGUI()
    {//GUI描画
//		float LayerW = this.position.width;
		float LayerH = 68;
		float LayerPosY = 1;
		GUILayout.BeginVertical(GUI.skin.box);
			GUILayout.Label("テクスチャをドロップして追加");
		GUILayout.EndVertical();

		mScrollPos = GUILayout.BeginScrollView(mScrollPos, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height - 100));        //スクロール開始
		int _Index = 0;
		int _RemoveIndex = -1;
		int _InsertIndex = -1;
		foreach (PMAP.Layer _Layer in mLayers)
		{
			GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
			GUILayout.BeginHorizontal(GUI.skin.box);
			GUI.backgroundColor = Color.white;

			GUILayout.BeginVertical(GUILayout.Width(16));
				GUI.contentColor = Color.yellow;
				GUI.enabled = (_Index != 0);
					if(GUILayout.Button("▲"))
					{//レイヤー上移動
						_RemoveIndex =  mLayers.IndexOf(_Layer);
						_InsertIndex = _RemoveIndex - 1;
					}
				GUI.enabled = true;
				GUI.enabled = (_Index != mLayers.Count - 1);
					if(GUILayout.Button("▼"))
					{//レイヤー下移動
						_RemoveIndex =  mLayers.IndexOf(_Layer);
						_InsertIndex = _RemoveIndex + 1;
					}
				GUI.enabled = true;
				GUI.contentColor = Color.white;
			GUILayout.EndVertical();

			GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
			GUILayout.Label(_Layer.mTexture, GUI.skin.box,GUILayout.Width(LayerH) ,GUILayout.Height(LayerH));
			GUI.backgroundColor = Color.white;

			GUILayout.BeginVertical(GUI.skin.box);

				GUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(_Layer.mTexture.name,EditorStyles.boldLabel);
					if(GUILayout.Button("×",GUILayout.Width(24)))
					{//削除ボタン
						_RemoveIndex =  mLayers.IndexOf(_Layer);
					}
				GUILayout.EndHorizontal();
				GUILayout.Space(4);
//				GUILayout.Space(6);

				GUILayout.BeginHorizontal();
					GUILayout.Label("ブレンドモード",GUILayout.Width(80));
					_Layer.mBlendType = (PMAP.Layer.BlendType) GUILayout.Toolbar( (int)_Layer.mBlendType,new string[]{"アルファ", "加算","事前乗算"} );
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
					GUILayout.Label("透明度 " + _Layer.mTransparency.ToString() + "%",GUILayout.Width(80));
					_Layer.mTransparency = (int)GUILayout.HorizontalSlider(_Layer.mTransparency,0,100);
				GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		
			GUILayout.EndHorizontal();

			if(_RemoveIndex >= 0)
			{//レイヤー削除・入れ替え
				PMAP.Layer _RLayer = mLayers[_RemoveIndex];
				mLayers.Remove(_RLayer);
				if(_InsertIndex >= 0)		mLayers.Insert(_InsertIndex,_RLayer);
				Repaint();
			}

			_Index++;
			LayerPosY += LayerH + 1;
		}
        GUILayout.EndScrollView();                                                                                      //スクロール完了


		GUILayout.BeginVertical(GUI.skin.box);

		mAddProtect = GUILayout.Toggle( mAddProtect,"加算部分の保護(スプライト用)" );
		GUILayout.BeginHorizontal();
			GUI.backgroundColor = Color.green;
			if (GUILayout.Button("名前を付けて保存", GUILayout.Width(120)))
			{
				CreateTexture();
			}
			GUI.backgroundColor = Color.white;
			if( GUILayout.Button("クリア", GUILayout.Width(80)))
			{
				ClearList();
			}
		GUILayout.EndHorizontal();

		GUI.backgroundColor = Color.yellow;
		if (!string.IsNullOrEmpty(mFilePath))
        {
			GUILayout.BeginHorizontal();
		        if (GUILayout.Button("上書き保存", GUILayout.Width(80)))
		        {
					SaveTexture();
				}
		        GUILayout.Label(mFilePath + ".png");
			GUILayout.EndHorizontal();
        }
		else
		{
			GUILayout.Space(21);
		}


		GUILayout.EndVertical();

		DropAreaWindow();	//ウィンドウ領域内のドラッグ＆ドロップ
	}
    void CreateTexture()
    {//名前を付けて保存
//		if(mFilePath == "")	mFilePath = "Assets/";
		string _Path = EditorUtility.SaveFilePanel("CreateTexture", System.IO.Path.GetDirectoryName (string.IsNullOrEmpty(mFilePath)?"Assets/":mFilePath), System.IO.Path.GetFileNameWithoutExtension (mFilePath), "png");
//		string _Path = EditorUtility.SaveFilePanel("CreateTexture", System.IO.Path.GetDirectoryName (mFilePath), System.IO.Path.GetFileNameWithoutExtension (mFilePath), "png");
//        string _Path =  EditorUtility.SaveFilePanelInProject("CreateTexture", "Demo1/Demo1.png", "png", "Create Texture");
        if (string.IsNullOrEmpty(_Path))         return;		//キャンセル
		_Path = System.IO.Path.GetDirectoryName (_Path) + "/" + System.IO.Path.GetFileNameWithoutExtension (_Path); 
		string _AssetsPath = Application.dataPath;
		int _Idx = _Path.IndexOf(_AssetsPath);
		if(_Idx != 0)
		{//Assets相対に出来ないPathは無効
            EditorUtility.DisplayDialog("PMAP Maker", "無効なpathです", "ok");
			return;
		}
	
		mFilePath = "Assets" + _Path.Remove(0, _AssetsPath.Length);
		SaveTexture();
	}

	void SaveTexture()
    {//上書き保存
//		mSerializedComponent.ApplyModifiedProperties();
		GameObject	_GameObject = new GameObject();
		PMAP _PMAP = _GameObject.AddComponent<PMAP>();
		_PMAP.mLayers = mLayers;
		_PMAP.mAddProtect = mAddProtect;
		Object Object = PrefabUtility.CreateEmptyPrefab(mFilePath + ".prefab"); 
		PrefabUtility.ReplacePrefab(_GameObject, Object);                  
		AssetDatabase.SaveAssets();                                        
		AssetDatabase.Refresh();                                           
		DestroyImmediate(_GameObject);

		{
			int TgtWidth = 1;
			int TgtHeight = 1;
			List<PMAP.Layer> _TextureLayers = mLayers;
			foreach (PMAP.Layer _Layer in _TextureLayers)
			{//一番大きい画像サイズに合わせる？
				if(_Layer.mTexture)
				{
					if(TgtWidth < _Layer.mTexture.width)	TgtWidth = _Layer.mTexture.width;
					if(TgtHeight < _Layer.mTexture.height)	TgtHeight = _Layer.mTexture.height;
				}
			}
			string newPath = mFilePath + ".png";
		
			if(System.IO.File.Exists(newPath))	//ファイルが存在しているか調べる
			{
//				if(!EditorUtility.DisplayDialog("PMAP Maker","\" " + newPath + "\" を\n" + "上書きしてもいいですか？","Yes","Cancel"))			return;

				System.IO.FileAttributes newPathAttrs = System.IO.File.GetAttributes(newPath);
				newPathAttrs &= ~System.IO.FileAttributes.ReadOnly;
				System.IO.File.SetAttributes(newPath, newPathAttrs);
			}
			Texture2D TgtTexture = new Texture2D(TgtWidth, TgtHeight, TextureFormat.ARGB32, false);
			
			foreach (PMAP.Layer _Layer in _TextureLayers)
			{//参照するTextureのパラメータをバックアップ後設定
				if(_Layer.mTexture)
				{
					_Layer.mTextureImporter = TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(_Layer.mTexture)) as TextureImporter;

					_Layer.mTextureFormat = _Layer.mTextureImporter.textureFormat;
					_Layer.mTextureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
					_Layer.mIsReadable = _Layer.mTextureImporter.isReadable;
					_Layer.mTextureImporter.isReadable = true;
					_Layer.mFilterMode = _Layer.mTextureImporter.filterMode;
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_Layer.mTexture));
				}
			}
			_TextureLayers.Reverse();
			for(int y = 0; y < TgtHeight; y++)
			{
				EditorUtility.DisplayProgressBar("PMAP Maker", "Create PMAP Texture", (float)y / (float)TgtHeight);
				float v = ((float)y + 0.5f) / (float)TgtHeight;
				for(int x = 0; x < TgtWidth; x++)
				{
					float u = ((float)x + 0.5f) / (float)TgtWidth;
					Color _Tgt = new Color(0.0f,0.0f,0.0f,1.0f);
					foreach (PMAP.Layer _Layer in _TextureLayers)
					{
						Texture2D _Tex = _Layer.mTexture;
						if(_Tex)
						{
							//GetPixelBilinear内で0.5テクセルオフセットが入っているようなので、テクスチャ毎にキャンセル
							Color _Col = _Tex.GetPixelBilinear(u - (0.5f / (float)_Tex.width), v - (0.5f / (float)_Tex.height));
	//						Color _Col = _Tex.GetPixelBilinear(u , v );
							float _t = _Layer.mTransparency / 100.0f;
							float _a = _Col.a * _t;

							switch (_Layer.mBlendType)
							{
								case PMAP.Layer.BlendType.Alpha:
									_Tgt.r = _Tgt.r * (1.0f - _a) + _Col.r * _a;
									_Tgt.g = _Tgt.g * (1.0f - _a) + _Col.g * _a;
									_Tgt.b = _Tgt.b * (1.0f - _a) + _Col.b * _a;
									_Tgt.a = _Tgt.a * (1.0f - _a);
									break;
								case PMAP.Layer.BlendType.Add:
									_Tgt.r = _Tgt.r + _Col.r * _a;
									_Tgt.g = _Tgt.g + _Col.g * _a;
									_Tgt.b = _Tgt.b + _Col.b * _a;
									break;
								case PMAP.Layer.BlendType.Premultiplied:
									_Tgt.r = _Tgt.r * (1.0f - _a) + _Col.r * _t;
									_Tgt.g = _Tgt.g * (1.0f - _a) + _Col.g * _t;
									_Tgt.b = _Tgt.b * (1.0f - _a) + _Col.b * _t;
									_Tgt.a = _Tgt.a * (1.0f - _a);
									break;
							}
						}
					}
					_Tgt.a = 1.0f - _Tgt.a;
					if(mAddProtect)
					{//スプライト用に加算部分を保護
						if ((_Tgt.a == 0.0f) && ((_Tgt.r > 0.0f) || (_Tgt.g > 0.0f) || (_Tgt.b > 0.0f)))
						{
							_Tgt.a = 1.0f / 255.0f;
						}
					}
					TgtTexture.SetPixel(x,y,_Tgt);
				}
			}
			_TextureLayers.Reverse();
			EditorUtility.ClearProgressBar();	
			foreach (PMAP.Layer _Layer in _TextureLayers)
			{//使用したTextureのパラメータを戻す
				if(_Layer.mTexture)
				{
					_Layer.mTextureImporter.textureFormat = _Layer.mTextureFormat;
					_Layer.mTextureImporter.isReadable = _Layer.mIsReadable;
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_Layer.mTexture));
				}
			}
			
			byte[] bytes = TgtTexture.EncodeToPNG();
			System.IO.File.WriteAllBytes(newPath, bytes);
			bytes = null;

			// Load the texture we just saved as a Texture2D
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

//			TextureImporter texImporter = AssetImporter.GetAtPath(newPath) as TextureImporter;
//			texImporter.isReadable = true;
//			AssetDatabase.ImportAsset( newPath );
//			AssetDatabase.Refresh();
		}
	}
	void ClearList() 
	{
		mLayers.Clear();
		mAddProtect = false;
		mFilePath = "";
		Selection.activeObject = null;
	}

}