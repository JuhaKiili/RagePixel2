using UnityEditor;
using UnityEngine;

namespace RagePixel2
{
	public class ReplaceColorHandler : IRagePixelMode
	{
		private Color[] m_Snapshot;

		public void OnSceneGUI (RagePixelState state)
		{
			return;
		}

		public void OnMouseDown (RagePixelState state)
		{
			return;
		}

		public void OnMouseUp (RagePixelState state)
		{
			return;
		}

		public void OnMouseDrag (RagePixelState state)
		{
			return;
		}

		public void OnMouseMove (RagePixelState state)
		{
			SceneView.RepaintAll ();
		}

		public void OnRepaint (RagePixelState state)
		{
			state.DrawBasicPaintGizmo ();
		}

		public void SaveSnapshot (Sprite sprite)
		{
			Texture2D texture = sprite.texture;
			Rect textureRect = sprite.textureRect;
			m_Snapshot = texture.GetPixels ((int)textureRect.xMin, (int)textureRect.yMin, (int)textureRect.width,
				(int)textureRect.height);
		}

		public void ReplaceColor (Sprite sprite, Color from, Color to)
		{
			Texture2D texture = sprite.texture;
			Rect textureRect = sprite.textureRect;
			int minX = (int)textureRect.xMin;
			int maxX = (int)textureRect.xMax;
			int minY = (int)textureRect.yMin;
			int maxY = (int)textureRect.yMax;

			int index = 0;
			for (int y = minY; y < maxY; y++)
			{
				for (int x = minX; x < maxX; x++)
				{
					if (Utility.SameColor (m_Snapshot[index++], from))
						texture.SetPixel (x, y, to);
				}
			}

			texture.Apply ();
			SceneView.RepaintAll ();
		}

		public void Apply (Sprite sprite)
		{
			Utility.SaveImageData (sprite);
			m_Snapshot = null;
		}

		public void Cancel (Sprite sprite, Color originalColor)
		{
			if (m_Snapshot == null)
				return;

			ReplaceColor (sprite, originalColor, originalColor);
			m_Snapshot = null;
		}

		public bool AllowRMBColorPick ()
		{
			return true;
		}
	}
}
