using UnityEngine;
using UnityEditor;
using System;

public class BookSwordDistbookSwordTool : EditorWindow
{
    private Texture2D	_spritesheet;
	private float		_minAlpha;
	private string		_path;

    [MenuItem("Tools/BookSword")]
    private static void Init()
    {
        GetWindow<BookSwordDistbookSwordTool>();
    }

    private void OnGUI()
    {
        _spritesheet = (Texture2D)EditorGUILayout.ObjectField("SpriteSheet", _spritesheet, typeof(Texture2D), false);
        _minAlpha = EditorGUILayout.Slider("Min Alpha", _minAlpha, 0, 1);
		EditorGUILayout.BeginHorizontal();

		EditorGUILayout.LabelField(_path);
		if (GUILayout.Button("Set path"))
			_path = EditorUtility.SaveFilePanelInProject("Save png", "Untitled", "png", "Please enter a file name to save the texture to");


		EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Generate"))
        {
            Generate();
        }
    }

    private void Generate()
    {
		CheckArgumentValidity();
		Texture2D	texture = CreateTexture();
		SaveTexture(texture);
    }

	private void	CheckArgumentValidity()
	{
		if (_spritesheet == null)
			throw new Exception("The spriteSheet is null");
		if (_minAlpha < 0 || _minAlpha > 0)
			throw new Exception("The minAlpha should be in range [0, 1]");
	}

	private Texture2D	CreateTexture()
	{
		Texture2D	texture = new(256, 256, TextureFormat.RGBA32, false);

		return texture;
	}

	private void	SaveTexture(Texture2D texture)
	{
		byte[]	bytes = texture.EncodeToPNG();
		System.IO.File.WriteAllBytes(_path, bytes);
		AssetDatabase.Refresh();
	}
}
