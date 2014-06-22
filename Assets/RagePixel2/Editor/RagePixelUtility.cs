using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using UnityEditor;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;

public static class RagePixelUtility 
{
	private const float k_UVEpsilon = 0.00001f;

	public static Vector2 UVToPixel(Vector2 uv, Sprite sprite)
	{
		if (sprite == null)
			return Vector2.zero;

		return new Vector2(Mathf.Min(uv.x, 1.0f - k_UVEpsilon) * sprite.texture.width, Mathf.Min(uv.y, 1.0f - k_UVEpsilon) * sprite.texture.height);
	}

	public static Vector2 PixelToUV(Vector2 pixel, Sprite sprite)
	{
		if (sprite == null)
			return Vector2.zero;

		return new Vector2(pixel.x / sprite.texture.width, pixel.y / sprite.texture.height);
	}

	public static Vector2 LocalToUV(Vector3 localPosition, Sprite sprite)
	{
		if (sprite == null)
			return Vector2.zero;

		Vector2 localNormalizedPosition = new Vector2(
			Mathf.Clamp01((localPosition.x - sprite.bounds.min.x) / sprite.bounds.size.x),
			Mathf.Clamp01((localPosition.y - sprite.bounds.min.y) / sprite.bounds.size.y)
			);

		Vector3 textureUV = new Vector2(
			localNormalizedPosition.x * (sprite.rect.width / sprite.texture.width) + sprite.textureRect.xMin,
			localNormalizedPosition.y * (sprite.rect.height / sprite.texture.height) + sprite.textureRect.yMin
			);

		return textureUV;
	}

	public static Vector3 UVToLocal(Vector2 uv, Sprite sprite)
	{
		if (sprite == null)
			return Vector2.zero;

		Vector2 localNormalizedPosition = new Vector2(
			Mathf.Clamp01((uv.x - sprite.textureRect.xMin) / (sprite.rect.width / sprite.texture.width)),
			Mathf.Clamp01((uv.y - sprite.textureRect.yMin) / (sprite.rect.height / sprite.texture.height))
			);

		Vector2 localPosition = new Vector2(
			localNormalizedPosition.x * sprite.bounds.size.x + sprite.bounds.min.x,
			localNormalizedPosition.y * sprite.bounds.size.y + sprite.bounds.min.y
			);

		return localPosition;
	}

	public static Vector3 LocalToWorld(Vector3 localPosition, Transform transform)
	{
		return transform.TransformPoint(localPosition);
	}

	public static Vector3 ScreenToLocal(Vector2 screenPosition, Transform transform)
	{
		return transform.InverseTransformPoint(ScreenToWorld(screenPosition, transform));
	}

	public static Vector2 LocalToScreen(Vector3 localPosition, Transform transform)
	{
		return WorldToScreen (transform.TransformPoint(localPosition), transform);
	}

	public static Vector3 ScreenToWorld(Vector2 screenPosition, Transform transform)
	{
		Ray mouseRay = HandleUtility.GUIPointToWorldRay(screenPosition);
		float dist;
		new Plane(transform.forward, Vector3.zero).Raycast(mouseRay, out dist);
		return mouseRay.GetPoint(dist);
	}

	public static Vector2 WorldToScreen(Vector3 worldPosition, Transform transform)
	{
		return HandleUtility.WorldToGUIPoint(worldPosition);
	}

	public static void DrawDebugPoint(Vector3 point)
	{
		Handles.color = Color.red;
		float size = HandleUtility.GetHandleSize(point) * 0.1f;
		Handles.DrawLine(point + Vector3.up * size, point + Vector3.down * size);
		Handles.DrawLine(point + Vector3.left * size, point + Vector3.right * size);
		Handles.DrawLine(point + Vector3.forward * size, point + Vector3.back * size);
	}

	public static void DrawPixelLine(Texture2D texture, Color color, int fromX, int fromY, int toX, int toY)
	{
		foreach (Vector2 pixel in GetPointsOnLine(fromX, fromY, toX, toY))
			texture.SetPixel((int)pixel.x, (int)pixel.y, color);
		texture.Apply();
	}

	// http://ericw.ca/notes/bresenhams-line-algorithm-in-csharp.html
	private static IEnumerable<Vector2> GetPointsOnLine(int x0, int y0, int x1, int y1)
	{
		bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
		if (steep)
		{
			int t;
			t = x0; // swap x0 and y0
			x0 = y0;
			y0 = t;
			t = x1; // swap x1 and y1
			x1 = y1;
			y1 = t;
		}
		if (x0 > x1)
		{
			int t;
			t = x0; // swap x0 and x1
			x0 = x1;
			x1 = t;
			t = y0; // swap y0 and y1
			y0 = y1;
			y1 = t;
		}
		int dx = x1 - x0;
		int dy = Math.Abs(y1 - y0);
		int error = dx / 2;
		int ystep = (y0 < y1) ? 1 : -1;
		int y = y0;
		for (int x = x0; x <= x1; x++)
		{
			yield return new Vector2((steep ? y : x), (steep ? x : y));
			error = error - dy;
			if (error < 0)
			{
				y += ystep;
				error += dx;
			}
		}
		yield break;
	}

	public static Sprite CreateNewSprite ()
	{
		string path = EditorUtility.SaveFilePanelInProject("New Sprite", "sprite", "png", "Save New Sprite");

		if (path.Length > 0)
			return CreateNewSprite (path);
		
		return null;
	}

	public static Vector3 GetSceneViewCenter()
	{
		if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null)
		{
			Camera sceneCamera = SceneView.lastActiveSceneView.camera;
			Plane plane = new Plane(Vector3.forward, Vector3.zero);
			float result = 0f;
			Ray ray = sceneCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
			if (plane.Raycast(ray, out result))
				return ray.GetPoint(result);
		}
		return Vector3.zero;
	}

	public static void SaveImageData (Sprite sprite)
	{
		Texture2D texture = sprite.texture;
		string path = AssetDatabase.GetAssetPath (texture);
		TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

		if (textureImporter == null)
			return;

		File.WriteAllBytes (path, texture.EncodeToPNG());
		//AssetDatabase.ImportAsset(path);
	}
	
	private static Sprite CreateNewSprite (string path)
	{
		Texture2D newTexture = CreateDefaultSpriteTexture();

		File.WriteAllBytes(path, newTexture.EncodeToPNG());
		Texture2D.DestroyImmediate (newTexture);

		AssetDatabase.ImportAsset(path);
		TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

		if (textureImporter == null)
			return null;

		SetDefaultSpriteSettings(ref textureImporter);

		AssetDatabase.ImportAsset(path);
		Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

		if (assets.Length <= 1 || assets[1] as Sprite == null)
			return null;

		return assets[1] as Sprite;
	}

	private static void SetDefaultSpriteSettings (ref TextureImporter textureImporter)
	{
		textureImporter.textureFormat = TextureImporterFormat.ARGB32;
		textureImporter.textureType = TextureImporterType.Advanced;
		textureImporter.spriteImportMode = SpriteImportMode.Single;
		textureImporter.npotScale = TextureImporterNPOTScale.None;
		textureImporter.filterMode = FilterMode.Point;
		textureImporter.isReadable = true;
	}

	private static Texture2D CreateDefaultSpriteTexture ()
	{
		return new Texture2D (32, 32);
	}
}
