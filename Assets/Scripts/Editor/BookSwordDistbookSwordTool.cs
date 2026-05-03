using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

public class BookSwordDistbookSwordTool : EditorWindow
{
    private Texture2D	_spritesheet;
	private float		_minAlpha;
	private string		_path;
	private Vector2Int	_flipbookSize;
	private Vector2Int	_resolution;
	private Texture2D	_generatedTexture;

    [MenuItem("Tools/BookSword")]
    private static void Init()
    {
        GetWindow<BookSwordDistbookSwordTool>();
    }

    private void OnGUI()
    {
        _spritesheet = (Texture2D)EditorGUILayout.ObjectField("SpriteSheet", _spritesheet, typeof(Texture2D), false);
        _minAlpha = EditorGUILayout.Slider("Min Alpha", _minAlpha, 0, 1);
		_flipbookSize = EditorGUILayout.Vector2IntField("Flipbook size", _flipbookSize);
		_resolution = EditorGUILayout.Vector2IntField("Resolution of created texture", _resolution);

        if (GUILayout.Button("Generate"))
        {
            Generate();
        }
		
		GUILayout.Label(_generatedTexture);

        if (GUILayout.Button("Save"))
        {
			_path = EditorUtility.SaveFilePanelInProject("Save png", "Untitled", "png", "Please enter a file name to save the texture to");
			SaveTexture(_generatedTexture);
        }
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
		if (_resolution.x <= 0 || _resolution.y <= 0)
			throw new Exception("The resolution of the created image must be positive !");
		if (!Mathf.IsPowerOfTwo(_resolution.x) || !Mathf.IsPowerOfTwo(_resolution.y))
			Debug.LogWarning("The resolution should be a power of two for better performances !");
		Debug.LogWarning("Should add a test for the resolution to be a multiple of the flipbook");
	}

	private Texture2D	CreateTexture()
	{
		Texture2D	texture = new(_resolution.x, _resolution.y, TextureFormat.RGBA32, false);
		Color32[]	outputPixels = new Color32[_resolution.x * _resolution.y];
		Color32[]	spriteSheetPixels = _spritesheet.GetPixels32(0);
		Vector2Int	frameResolution = new(_resolution.x / _flipbookSize.x, _resolution.y / _flipbookSize.y);

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
				if (x < frameResolution.y - y)
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

					if (newPixelPos.x < 0 || newPixelPos.y < 0 || newPixelPos.x >= frameResolution.x || newPixelPos.y >= frameResolution.y || newPixelPos.x < frameResolution.y - newPixelPos.y)
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
		if (x < frameResolution.x / 2 || y < frameResolution.y / 2)
			return 0;
		int	index = GetPixelIndexInGeneratedTexture(frameX, frameY, x * 2 % frameResolution.x, y * 2 % frameResolution.y, frameResolution);

		return spriteSheetPixels[index].a;
	}

	private int	GetPixelIndexInGeneratedTexture(int frameX, int frameY, int x, int y, Vector2Int frameResolution)
	{
		return	(frameY * frameResolution.y + y) * _resolution.x + frameX * frameResolution.x + x;
	}

	private void	SaveTexture(Texture2D texture)
	{
		byte[]	bytes = texture.EncodeToPNG();
		System.IO.File.WriteAllBytes(_path, bytes);
		AssetDatabase.Refresh();
	}
}
