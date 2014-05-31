using System.IO;
using System.Net.Mime;
using UnityEditor;
using UnityEngine;
using System.Collections;

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

	private static Sprite CreateNewSprite (string path)
	{
		Texture2D newTexture = CreateDefaultSpriteTexture();

		File.WriteAllBytes(path, newTexture.EncodeToPNG());
		newTexture = null;

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
		textureImporter.textureType = TextureImporterType.Sprite;
		textureImporter.filterMode = FilterMode.Point;
		textureImporter.isReadable = true;
	}

	private static Texture2D CreateDefaultSpriteTexture ()
	{
		return new Texture2D (32, 32);
	}
}
