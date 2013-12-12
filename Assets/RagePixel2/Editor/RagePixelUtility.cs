using UnityEditor;
using UnityEngine;
using System.Collections;

public static class RagePixelUtility 
{
	public static Vector2 UVToPixel(Vector2 uv, Sprite sprite)
	{
		return new Vector2(uv.x * sprite.texture.width, uv.y * sprite.texture.height);
	}

	public static Vector2 LocalToUV(Vector3 localPosition, Sprite sprite)
	{
		Rect spriteUVRect = SpriteUV(sprite);
		
		Vector2 localNormalizedPosition = new Vector2(
			Mathf.Clamp01((localPosition.x - sprite.bounds.min.x) / sprite.bounds.size.x),
			Mathf.Clamp01((localPosition.y - sprite.bounds.min.y) / sprite.bounds.size.y)
			);

		Vector3 textureUV = new Vector2(
			localNormalizedPosition.x * (sprite.rect.width / sprite.texture.width) + spriteUVRect.xMin,
			localNormalizedPosition.y * (sprite.rect.height / sprite.texture.height) + spriteUVRect.yMin
			);

		return textureUV;
	}

	public static Rect SpriteUV (Sprite sprite)
	{
		return new Rect (
			sprite.rect.xMin / sprite.texture.width,
			sprite.rect.yMin / sprite.texture.height,
			sprite.rect.width / sprite.texture.width,
			sprite.rect.height / sprite.texture.height
			);
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
}