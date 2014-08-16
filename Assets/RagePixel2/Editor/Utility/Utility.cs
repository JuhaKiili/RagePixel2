using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RagePixel2
{
	public static class Utility
	{
		private const float k_DefaultPixelsToUnits = 100f;
		private const float k_UVEpsilon = 0.00001f;

		public static Vector2 UVToPixel (Vector2 uv, Sprite sprite)
		{
			return UVToPixel (uv, sprite, true);
		}

		public static Vector2 UVToPixel (Vector2 uv, Sprite sprite, bool clamp)
		{
			if (sprite == null)
				return Vector2.zero;

			Vector2 result = new Vector2(uv.x, uv.y);

			if (clamp)
				result = new Vector2 (
					Mathf.Max (Mathf.Min (result.x, 1.0f - k_UVEpsilon), 0f),
					Mathf.Max (Mathf.Min (result.y, 1.0f - k_UVEpsilon), 0f)
					);

			result = new Vector2(result.x * sprite.texture.width, result.y * sprite.texture.height);

			return result;
		}

		public static Vector2 PixelToUV (Vector2 pixel, Sprite sprite)
		{
			return PixelToUV (pixel, sprite, true);
		}

		public static Vector2 PixelToUV (Vector2 pixel, Sprite sprite, bool clamp)
		{
			if (sprite == null)
				return Vector2.zero;

			Vector2 result = new Vector2 (pixel.x / sprite.texture.width, pixel.y / sprite.texture.height);

			if (clamp)
				result = new Vector2 (
					Mathf.Max (Mathf.Min (result.x, 1.0f - k_UVEpsilon), 0f),
					Mathf.Max (Mathf.Min (result.y, 1.0f - k_UVEpsilon), 0f)
					);

			return result;
		}

		public static Vector2 LocalToUV (Vector3 localPosition, Sprite sprite)
		{
			return LocalToUV (localPosition, sprite, true);
		}

		public static Vector2 LocalToUV (Vector3 localPosition, Sprite sprite, bool clamp)
		{
			if (sprite == null)
				return Vector2.zero;

			Vector2 localNormalizedPosition = new Vector2 (
				(localPosition.x - sprite.bounds.min.x) / sprite.bounds.size.x,
				(localPosition.y - sprite.bounds.min.y) / sprite.bounds.size.y
				);

			if (clamp)
				localNormalizedPosition = new Vector2 (
					Mathf.Clamp01 (localNormalizedPosition.x),
					Mathf.Clamp01 (localNormalizedPosition.y)
					);
			
			Vector3 textureUV = new Vector2 (
				localNormalizedPosition.x * (sprite.rect.width / sprite.texture.width) + sprite.textureRect.xMin,
				localNormalizedPosition.y * (sprite.rect.height / sprite.texture.height) + sprite.textureRect.yMin
				);

			return textureUV;
		}

		public static Vector3 UVToLocal (Vector2 uv, Sprite sprite)
		{
			return UVToLocal (uv, sprite, true);
		}

		public static Vector3 UVToLocal (Vector2 uv, Sprite sprite, bool clamp)
		{
			if (sprite == null)
				return Vector2.zero;

			Vector2 localNormalizedPosition = new Vector2 (
				(uv.x - sprite.textureRect.xMin) / (sprite.rect.width / sprite.texture.width),
				(uv.y - sprite.textureRect.yMin) / (sprite.rect.height / sprite.texture.height)
				);

			if (clamp)
			{
				localNormalizedPosition = new Vector2 (
					Mathf.Clamp01 (localNormalizedPosition.x),
					Mathf.Clamp01 (localNormalizedPosition.y)
					);
			}

			Vector2 localPosition = new Vector2 (
				localNormalizedPosition.x*sprite.bounds.size.x + sprite.bounds.min.x,
				localNormalizedPosition.y*sprite.bounds.size.y + sprite.bounds.min.y
				);

			return localPosition;
		}

		public static Vector3 WorldToLocal (Vector3 worldPosition, Transform transform)
		{
			return transform.InverseTransformPoint (worldPosition);
		}

		public static Vector3 LocalToWorld (Vector3 localPosition, Transform transform)
		{
			return transform.TransformPoint (localPosition);
		}

		public static Vector3 ScreenToLocal (Vector2 screenPosition, Transform transform)
		{
			return transform.InverseTransformPoint (ScreenToWorld (screenPosition, transform));
		}

		public static Vector2 LocalToScreen (Vector3 localPosition, Transform transform)
		{
			return WorldToScreen (transform.TransformPoint (localPosition), transform);
		}

		public static Vector3 ScreenToWorld (Vector2 screenPosition, Transform transform)
		{
			Ray mouseRay = HandleUtility.GUIPointToWorldRay (screenPosition);
			float dist;
			new Plane (transform.forward, Vector3.zero).Raycast (mouseRay, out dist);
			return mouseRay.GetPoint (dist);
		}

		public static Vector3 ScreenToWorld (Vector2 screenPosition)
		{
			Ray mouseRay = HandleUtility.GUIPointToWorldRay (screenPosition);
			float dist;
			new Plane (Vector3.back, Vector3.zero).Raycast (mouseRay, out dist);
			return mouseRay.GetPoint (dist);
		}

		public static Vector2 WorldToScreen (Vector3 worldPosition, Transform transform)
		{
			return HandleUtility.WorldToGUIPoint (worldPosition);
		}

		public static Vector2 ScreenToPixel (Vector2 screenPosition, Transform transform, Sprite sprite)
		{
			return ScreenToPixel (screenPosition, transform, sprite, true);
		}
			
		public static Vector2 ScreenToPixel (Vector2 screenPosition, Transform transform, Sprite sprite, bool clamp)
		{
			Vector3 localPosition = Utility.ScreenToLocal (screenPosition, transform);
			Vector2 uvPosition = Utility.LocalToUV (localPosition, sprite, clamp);
			Vector2 pixelPosition = Utility.UVToPixel (uvPosition, sprite, clamp);
			return pixelPosition;
		}

		public static Vector2 PixelToScreen (Vector2 pixelPosition, Transform transform, Sprite sprite)
		{
			return PixelToScreen (pixelPosition, transform, sprite, true);
		}

		public static Vector2 PixelToScreen (Vector2 pixelPosition, Transform transform, Sprite sprite, bool clamp)
		{
			Vector2 uvPosition = Utility.PixelToUV (pixelPosition, sprite, clamp);
			Vector3 localPosition = Utility.UVToLocal (uvPosition, sprite, clamp);
			Vector2 screenPosition = Utility.LocalToScreen (localPosition, transform);
			return screenPosition;
		}

		public static Vector2 WorldToPixel (Vector2 worldPosition)
		{
			return new Vector2(
				worldPosition.x * k_DefaultPixelsToUnits, 
				worldPosition.y * k_DefaultPixelsToUnits
				);
		}

		public static Vector2 PixelToWorld (Vector2 pixelPosition)
		{
			return new Vector2(
				pixelPosition.x / k_DefaultPixelsToUnits, 
				pixelPosition.y / k_DefaultPixelsToUnits
				);
		} 

		public static Vector2 WorldToPixel (Vector2 worldPosition, Transform transform, Sprite sprite)
		{
			return WorldToPixel (worldPosition, transform, sprite, true);
		}

		public static Vector2 WorldToPixel (Vector2 worldPosition, Transform transform, Sprite sprite, bool clamp)
		{
			Vector3 localPosition = Utility.WorldToLocal (worldPosition, transform);
			Vector2 uvPosition = Utility.LocalToUV (localPosition, sprite, clamp);
			Vector2 pixelPosition = Utility.UVToPixel (uvPosition, sprite, clamp);
			return pixelPosition;
		}

		public static Vector3 PixelToWorld (Vector2 pixelPosition, Transform transform, Sprite sprite)
		{
			return PixelToWorld (pixelPosition, transform, sprite, true);
		}

		public static Vector3 PixelToWorld (Vector2 pixelPosition, Transform transform, Sprite sprite, bool clamp)
		{
			Vector2 uvPosition = Utility.PixelToUV (pixelPosition, sprite, clamp);
			Vector3 localPosition = Utility.UVToLocal (uvPosition, sprite, clamp);
			Vector2 worldPosition = Utility.LocalToWorld (localPosition, transform);
			return worldPosition;
		}

		public static void DrawPaintGizmo (Vector2 screenPosition, Color color, Color shadowColor, Brush brush, Transform transform, Sprite sprite)
		{
			Vector2 pixel = ScreenToPixel (screenPosition, transform, sprite);

			Vector2 minPixel = new Vector2 ((int)(pixel.x - brush.m_SizeX * .5f + .5f), (int)(pixel.y - brush.m_SizeY * .5f + .5f));
			Vector2 maxPixel = new Vector2 ((int)(pixel.x + brush.m_SizeX * .5f + .5f), (int)(pixel.y + brush.m_SizeY * .5f + .5f));

			Vector3[] screenPolyLine = new Vector3[5];
			screenPolyLine[0] = PixelToScreen (new Vector2 (Mathf.FloorToInt (minPixel.x), Mathf.FloorToInt (minPixel.y)), transform,
				sprite);
			screenPolyLine[1] = PixelToScreen (new Vector2 (Mathf.FloorToInt (maxPixel.x), Mathf.FloorToInt (minPixel.y)),
				transform, sprite);
			screenPolyLine[2] = PixelToScreen (new Vector2 (Mathf.FloorToInt (maxPixel.x), Mathf.FloorToInt (maxPixel.y)),
				transform, sprite);
			screenPolyLine[3] = PixelToScreen (new Vector2 (Mathf.FloorToInt (minPixel.x), Mathf.FloorToInt (maxPixel.y)),
				transform, sprite);
			screenPolyLine[4] = screenPolyLine[0];

			Vector3[] shadowPolyLine = new Vector3[5];
			for (int i = 0; i < screenPolyLine.Length; i++)
				shadowPolyLine[i] = screenPolyLine[i] + new Vector3 (1f, 1f, 0f);

			Handles.BeginGUI ();
			GUI.DrawTexture (
				new Rect(
					screenPolyLine[0].x, 
					screenPolyLine[0].y, 
					screenPolyLine[2].x - screenPolyLine[0].x,
					screenPolyLine[2].y - screenPolyLine[0].y
					), brush.brushTexture
				);

			Handles.color = shadowColor;
			Handles.DrawPolyLine (shadowPolyLine);
			Handles.color = color;
			Handles.DrawPolyLine (screenPolyLine);

			Handles.EndGUI ();
		}

		public static void DrawRectangle (Vector3 worldPosition1, Vector3 worldPosition2, Color color)
		{
			Vector3[] polyLine = new Vector3[5];
			polyLine[0] = new Vector3(worldPosition1.x, worldPosition1.y, worldPosition1.z);
			polyLine[1] = new Vector3(worldPosition2.x, worldPosition1.y, worldPosition1.z);
			polyLine[2] = new Vector3(worldPosition2.x, worldPosition2.y, worldPosition1.z);
			polyLine[3] = new Vector3(worldPosition1.x, worldPosition2.y, worldPosition1.z);
			polyLine[4] = new Vector3(worldPosition1.x, worldPosition1.y, worldPosition1.z);
			Handles.color = color;
			
			Handles.DrawPolyLine (polyLine);
		}

		public static void DrawDebugPoint (Vector3 point)
		{
			Handles.color = Color.red;
			float size = HandleUtility.GetHandleSize (point)*0.1f;
			Handles.DrawLine (point + Vector3.up*size, point + Vector3.down*size);
			Handles.DrawLine (point + Vector3.left*size, point + Vector3.right*size);
			Handles.DrawLine (point + Vector3.forward*size, point + Vector3.back*size);
		}

		public static void DrawPixelLine (Texture2D texture, Color color, int fromX, int fromY, int toX, int toY)
		{
			foreach (Vector2 pixel in GetPointsOnLine (fromX, fromY, toX, toY))
				texture.SetPixel ((int)pixel.x, (int)pixel.y, color);
			texture.Apply ();
		}

		public static Color PaintColorField (Color value, float width, float height)
		{
			const int kColorFieldSizeBug = 2;
			const int kColorFieldLeftMarginBug = 1;

			// internal static Color ColorField (Rect position, GUIContent label, Color value, bool showEyedropper, bool showAlpha)
			object[] parameters = new object[5];

			Rect rect = EditorGUILayout.GetControlRect (false, GUILayout.Width (width), GUILayout.Height (height));
			rect.Set (rect.xMin + kColorFieldLeftMarginBug, rect.yMin + kColorFieldSizeBug, rect.width - kColorFieldSizeBug, rect.height - kColorFieldSizeBug);

			parameters[0] = rect;
			parameters[1] = new GUIContent ("");
			parameters[2] = value;
			parameters[3] = false;
			parameters[4] = true;

			Type[] types = new Type[5];
			types[0] = typeof (Rect);
			types[1] = typeof (GUIContent);
			types[2] = typeof (Color);
			types[3] = typeof (bool);
			types[4] = typeof (bool);

			object returnValue = Reflection.InvokeEditorStatic ("EditorGUI", "ColorField", parameters, types);

			return (Color)returnValue;
		}

		// http://ericw.ca/notes/bresenhams-line-algorithm-in-csharp.html
		private static IEnumerable<Vector2> GetPointsOnLine (int x0, int y0, int x1, int y1)
		{
			bool steep = Math.Abs (y1 - y0) > Math.Abs (x1 - x0);
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
			int dy = Math.Abs (y1 - y0);
			int error = dx/2;
			int ystep = (y0 < y1) ? 1 : -1;
			int y = y0;
			for (int x = x0; x <= x1; x++)
			{
				yield return new Vector2 ((steep ? y : x), (steep ? x : y));
				error = error - dy;
				if (error < 0)
				{
					y += ystep;
					error += dx;
				}
			}
			yield break;
		}

		public static Sprite CreateNewSprite (int sizeX, int sizeY)
		{
			string path = EditorUtility.SaveFilePanelInProject ("New Sprite", "sprite", "png", "Save New Sprite");

			if (path.Length > 0)
				return CreateNewSprite (path, sizeX, sizeY);

			return null;
		}

		public static Vector3 GetSceneViewCenter ()
		{
			if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null)
			{
				Camera sceneCamera = SceneView.lastActiveSceneView.camera;
				Plane plane = new Plane (Vector3.forward, Vector3.zero);
				float result = 0f;
				Ray ray = sceneCamera.ViewportPointToRay (new Vector3 (0.5f, 0.5f, 0f));
				if (plane.Raycast (ray, out result))
					return ray.GetPoint (result);
			}
			return Vector3.zero;
		}

		public static void SaveImageData (Sprite sprite, bool reimport)
		{
			Texture2D texture = sprite.texture;
			string path = AssetDatabase.GetAssetPath (texture);
			
			TextureImporter textureImporter = GetTextureImporter (sprite);

			if (textureImporter == null)
				return;

			File.WriteAllBytes (path, texture.EncodeToPNG ());

			if (reimport)
				AssetDatabase.ImportAsset(path);
		}

		public static bool SameColor (Color a, Color b)
		{
			const float epsilon = 0.01f;
			return Mathf.Abs (a.r - b.r) < epsilon && Mathf.Abs (a.g - b.g) < epsilon && Mathf.Abs (a.b - b.b) < epsilon &&
			       Mathf.Abs (a.a - b.a) < epsilon;
		}

		public static Color[] GetPixels (Sprite sprite)
		{
			Texture2D texture = sprite.texture;
			Rect textureRect = sprite.textureRect;
			return texture.GetPixels ((int)textureRect.xMin, (int)textureRect.yMin, (int)textureRect.width, (int)textureRect.height);
		}

		public static Color32[] GetDefaultPixels (int sizeX, int sizeY)
		{
			Color32[] pixels = new Color32[sizeX * sizeY];
			Color32 defaultColor = GetDefaultColor ();
			for (int i = 0; i < pixels.Length; i++)
				pixels[i] = defaultColor;

			return pixels;
		}

		public static Vector2 GetNormalizedPivot (Sprite sprite)
		{
			TextureImporter importer = GetTextureImporter (sprite);
			SpriteMetaData[] spritesheet = importer.spritesheet;
			
			foreach (SpriteMetaData metaData in spritesheet)
				if (metaData.name == sprite.name)
					return metaData.pivot;

			TextureImporterSettings settings = new TextureImporterSettings();
			importer.ReadTextureSettings (settings);
			
			if (settings.spriteAlignment == (int)SpriteAlignment.Custom)
				return importer.spritePivot;

			return GetAlignedPivot ((SpriteAlignment)settings.spriteAlignment);
		}

		public static float GetPixelsToUnits (Sprite sprite)
		{
			return sprite.textureRect.width / sprite.bounds.size.x;
		}

		public static Color[] GetYReversed (Color[] colors, int sizeX, int sizeY)
		{
			Color[] result = new Color[colors.Length];

			int i = 0;
			for (int y = sizeY - 1; y >= 0; y--)
				for (int x = 0; x < sizeX; x++)
					result[i++] = colors[y*sizeX + x];

			return result;
		}

		public static Rect GetPixelMarqueeRect (Vector2 pixelA, Vector2 pixelB)
		{
			int minX = Mathf.Min ((int)pixelA.x, (int)pixelB.x);
			int minY = Mathf.Min ((int)pixelA.y, (int)pixelB.y);
			int maxX = Mathf.Max ((int)pixelA.x, (int)pixelB.x);
			int maxY = Mathf.Max ((int)pixelA.y, (int)pixelB.y);

			return new Rect(minX, minY, maxX - minX, maxY - minY);
		}

		private static TextureImporter GetTextureImporter (Sprite sprite)
		{
			string path = AssetDatabase.GetAssetPath (sprite.texture);
			return AssetImporter.GetAtPath (path) as TextureImporter;
		}

		private static Sprite CreateNewSprite (string path, int sizeX, int sizeY)
		{
			Texture2D newTexture = CreateDefaultSpriteTexture (sizeX, sizeY);

			File.WriteAllBytes (path, newTexture.EncodeToPNG ());
			Texture2D.DestroyImmediate (newTexture);

			AssetDatabase.ImportAsset (path);
			TextureImporter textureImporter = AssetImporter.GetAtPath (path) as TextureImporter;

			if (textureImporter == null)
				return null;

			SetDefaultSpriteSettings (ref textureImporter);

			AssetDatabase.ImportAsset (path);
			Object[] assets = AssetDatabase.LoadAllAssetsAtPath (path);

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

			TextureImporterSettings settings = new TextureImporterSettings ();
			textureImporter.ReadTextureSettings (settings);
			settings.spriteMeshType = SpriteMeshType.FullRect;
			textureImporter.SetTextureSettings (settings);
		}

		private static Texture2D CreateDefaultSpriteTexture (int sizeX, int sizeY)
		{
			Texture2D t = new Texture2D (sizeX, sizeY);
			Color32[] colors = new Color32[sizeX*sizeY];
			Color defaultColor = GetDefaultColor ();

			for (int i = 1; i < colors.Length - 1; i++)
				colors[i] = defaultColor;

			colors[0] = new Color32 (128, 128, 128, 0);
			colors[sizeX - 1] = new Color32 (128, 128, 128, 0);
			colors[colors.Length - sizeX] = new Color32 (128, 128, 128, 0);
			colors[colors.Length - 1] = new Color32 (128, 128, 128, 0);

			t.SetPixels32 (colors);
			return t;
		}

		private static Vector2 GetAlignedPivot (SpriteAlignment alignment)
		{
			switch (alignment)
			{
				case SpriteAlignment.TopLeft:
					return new Vector2(0, 1);
				case SpriteAlignment.TopCenter:
					return new Vector2(.5f, 1);
				case SpriteAlignment.TopRight:
					return new Vector2(1, 1);
				case SpriteAlignment.LeftCenter:
					return new Vector2(0, .5f);
				case SpriteAlignment.Center:
					return new Vector2(.5f, .5f);
				case SpriteAlignment.RightCenter:
					return new Vector2(1, .5f);
				case SpriteAlignment.BottomLeft:
					return new Vector2(0, 0);
				case SpriteAlignment.BottomCenter:
					return new Vector2(.5f, 0);
				case SpriteAlignment.BottomRight:
					return new Vector2(1, 0);
			}
			return new Vector2(.5f, .5f);
		}

		private static Color GetDefaultColor ()
		{
			return new Color32 (128, 128, 128, 255);
		}
	}
}
