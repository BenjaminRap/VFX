using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

public class BookSwordDistTool : EditorWindow
{
    private Texture2D	_spritesheet;
	private float		_minAlpha = 0.5f;
	private string		_path = "Assets/Textures/Untitled.png";
	private Vector2Int	_flipbookSize = Vector2Int.one;
	private string		_savePath = "Assets";
	private string		_saveName = "Untitled";

	private Texture2D	_generatedTexture;

	Vector2 scrollPosition = Vector2.zero;

    [MenuItem("Tools/BookSword")]
    private static void Init()
    {
        GetWindow<BookSwordDistTool>();
    }

	private void	Awake()
	{
		if (EditorPrefs.HasKey("BookSword_MinAlpha"))
			_minAlpha = EditorPrefs.GetFloat("BookSword_MinAlpha");
		if (EditorPrefs.HasKey("BookSword_FlipbookSizeX"))
			_flipbookSize.x = EditorPrefs.GetInt("BookSword_FlipbookSizeX");
		if (EditorPrefs.HasKey("BookSword_FlipbookSizeY"))
			_flipbookSize.y = EditorPrefs.GetInt("BookSword_FlipbookSizeY");
		if (EditorPrefs.HasKey("BookSword_TexturePath"))
		{
			string	texturePath = EditorPrefs.GetString("BookSword_TexturePath");

			Texture2D	texture = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));

			if (texture == null)
				Debug.LogError("Could not load the spritesheet texture at path : " + texturePath);
			else
				_spritesheet = texture;
		}
		if (EditorPrefs.HasKey("BookSword_SavePath"))
			_savePath = EditorPrefs.GetString("BookSword_SavePath");
		if (EditorPrefs.HasKey("BookSword_SaveName"))
			_saveName = EditorPrefs.GetString("BookSword_SaveName");
	}

	private void	OnDestroy()
	{
		EditorPrefs.SetFloat("BookSword_MinAlpha", _minAlpha);
		EditorPrefs.SetInt("BookSword_FlipbookSizeX", _flipbookSize.x);
		EditorPrefs.SetInt("BookSword_FlipbookSizeY", _flipbookSize.y);
		if (_spritesheet != null)
		{
			string	path = AssetDatabase.GetAssetPath(_spritesheet);

			EditorPrefs.SetString("BookSword_TexturePath", path);
		}
		else if (EditorPrefs.HasKey("BookSword_TexturePath"))
			EditorPrefs.DeleteKey("BookSword_TexturePath");
		EditorPrefs.SetString("BookSword_SaveName", _saveName);
		EditorPrefs.SetString("BookSword_SavePath", _savePath);
	}

    private void OnGUI()
    {
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
        _spritesheet = (Texture2D)EditorGUILayout.ObjectField("SpriteSheet", _spritesheet, typeof(Texture2D), false);
        _minAlpha = EditorGUILayout.Slider("Min Alpha", _minAlpha, 0, 1);
		_flipbookSize = EditorGUILayout.Vector2IntField("Flipbook size", _flipbookSize);

        if (GUILayout.Button("Generate"))
        {
            Generate();
        }
		
		GUILayout.Label(_generatedTexture);

        if (GUILayout.Button("Save"))
        {
			_path = EditorUtility.SaveFilePanelInProject("Save png", _saveName, "png", "Please enter a file name to save the texture to", _savePath);
			if (_path.Length != 0)
			{
				_savePath = Path.GetDirectoryName(_path);
				_saveName = Path.GetFileNameWithoutExtension(_path);
				SaveTexture(_generatedTexture);
			}
        }
		GUILayout.EndScrollView();
    }

    private void Generate()
    {
		CheckArgumentValidity();
		Debug.LogWarning("Don't forget to turn off the texture Read/Write after generating the file !");
		_generatedTexture = CreateTexture();
    }

	private void	CheckArgumentValidity()
	{
		if (_spritesheet == null)
			throw new Exception("The spriteSheet is null");
		if (!_spritesheet.isReadable)
			throw new Exception("The spriteSheet is not readable, you should check Read/Write and retry");
		if (_minAlpha < 0 || _minAlpha > 1)
			throw new Exception("The minAlpha should be in range [0, 1]");
		if (_flipbookSize.x <= 0 || _flipbookSize.y <= 0)
			throw new Exception("The flipbbok size must be positive !");
		if (_spritesheet.width / _flipbookSize.x % 1 != 0)
			throw new Exception("The spritesheet width should be a multiple of the flipbook size x");
		if (_spritesheet.height / _flipbookSize.y % 1 != 0)
			throw new Exception("The spritesheet height should be a multiple of the flipbook size y");
	}

	private Texture2D	CreateTexture()
	{
		Texture2D	texture = new(_spritesheet.width, _spritesheet.height, TextureFormat.RGBA32, false);
		Color32[]	outputPixels = new Color32[_spritesheet.width * _spritesheet.height];
		Color32[]	spriteSheetPixels = _spritesheet.GetPixels32(0);
		Vector2Int	frameResolution = new(_spritesheet.width / _flipbookSize.x, _spritesheet.height / _flipbookSize.y);

		for (int y = 0; y < _flipbookSize.y; y++)
		{
			for (int x = 0; x < _flipbookSize.x; x++)
			{
				GenerateFrame(x, y, outputPixels, frameResolution, spriteSheetPixels);
			}
		}
		texture.SetPixelData(outputPixels, 0);
		texture.Apply();
		return texture;
	}

	private void	GenerateFrame(int frameX, int frameY, Color32[] outputPixels, Vector2Int frameResolution, Color32[] spriteSheetPixels)
	{
		Queue<Vector2Int>	_closePixels = new();

		for (int y = 0; y < frameResolution.y; y++)
		{
			for (int x = 0; x < frameResolution.x; x++)
			{
				if (x > frameResolution.y - y)
					continue ;

				int		index = GetPixelIndexInGeneratedTexture(frameX, frameY, x, y, frameResolution);
				byte	alpha = GetSpriteSheetPixelAlpha(frameX, frameY, x, y, frameResolution, spriteSheetPixels);

				if (alpha < _minAlpha)
					outputPixels[index] = new Color32(255, 255, 255, 0);
				else
				{
					outputPixels[index] = new Color32(255, 255, 255, 255);
					_closePixels.Enqueue(new Vector2Int(x, y));
				}

			}	    
		}
		while (_closePixels.Count != 0)
		{
			Vector2Int	pixelPos = _closePixels.Dequeue();
			int			outputPixelIndex = GetPixelIndexInGeneratedTexture(frameX, frameY, pixelPos.x, pixelPos.y, frameResolution);
			byte		centerAlpha = outputPixels[outputPixelIndex].a;
			byte		sideAlpha = (byte)Math.Max(centerAlpha - 6, 0);
			byte		diagonalAlpha = (byte)Math.Max(centerAlpha - 9, 0);

			for (int y = -1; y < 2; y++)
			{
			    for (int x = -1; x < 2; x++)
			    {
					if (y == 0 && x == 0)
						continue ;
					Vector2Int	newPixelPos = new(pixelPos.x + x, pixelPos.y + y);

					if (newPixelPos.x < 0 || newPixelPos.y < 0 || newPixelPos.x >= frameResolution.x
							|| newPixelPos.y >= frameResolution.y || newPixelPos.x > frameResolution.y - newPixelPos.y)
						continue ;
					int			newOutputPixelIndex = GetPixelIndexInGeneratedTexture(frameX, frameY, newPixelPos.x, newPixelPos.y, frameResolution);
					byte		newAlpha = (x != 0 && y != 0) ? diagonalAlpha : sideAlpha;
					byte		currentAlpha = outputPixels[newOutputPixelIndex].a;

					if (newAlpha <= currentAlpha)
						continue ;
					outputPixels[newOutputPixelIndex].a = newAlpha;
					if (!_closePixels.Contains(newPixelPos))
						_closePixels.Enqueue(newPixelPos);
			    }
			}
		}
	}

	private byte	GetSpriteSheetPixelAlpha(int frameX, int frameY, int x, int y, Vector2Int frameResolution, Color32[] spriteSheetPixels)
	{
		if (x > frameResolution.x / 2 || y > frameResolution.y / 2)
			return 0;
		int	index = GetPixelIndexInGeneratedTexture(frameX, frameY, x * 2 % frameResolution.x, y * 2 % frameResolution.y, frameResolution);

		return spriteSheetPixels[index].a;
	}

	private int	GetPixelIndexInGeneratedTexture(int frameX, int frameY, int x, int y, Vector2Int frameResolution)
	{
		return	(frameY * frameResolution.y + y) * _spritesheet.width + frameX * frameResolution.x + x;
	}

	private void	SaveTexture(Texture2D texture)
	{
		byte[]	bytes = texture.EncodeToPNG();
		File.WriteAllBytes(_path, bytes);
		AssetDatabase.Refresh();
	}
}
