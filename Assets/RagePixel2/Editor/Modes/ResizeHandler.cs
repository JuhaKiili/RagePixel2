using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace RagePixel2
{
	public class ResizeHandler : IRagePixelMode
	{
		private const float k_MinSize = 2f;
		private const float k_MaxSize = 2048f;
		private Vector2 m_Size;

		public ResizeHandler (Vector2 spriteSize)
		{
			m_Size = spriteSize;
		}

		public void OnSceneGUI (RagePixelState state)
		{
			Vector2 uvPos = Utility.PixelToUV (m_Size, state.sprite, false);
			Vector3 localPos = Utility.UVToLocal (uvPos, state.sprite, false);
			Vector3 worldPos = Utility.LocalToWorld (localPos, state.transform);
			
			EditorGUI.BeginChangeCheck();
			worldPos = Handles.FreeMoveHandle (worldPos, Quaternion.identity, HandleUtility.GetHandleSize (worldPos) * 0.1f, Vector3.zero, Handles.RectangleCap);
			if (EditorGUI.EndChangeCheck ())
			{
				localPos = Utility.WorldToLocal (worldPos, state.transform);
				uvPos = Utility.LocalToUV (localPos, state.sprite, false);
				m_Size = Utility.UVToPixel (uvPos, state.sprite, false);
				m_Size = new Vector2((int)Mathf.Clamp (m_Size.x, k_MinSize, k_MaxSize), (int)Mathf.Clamp (m_Size.y, k_MinSize, k_MaxSize));
				state.Repaint ();
			}
		}

		public void OnMouseDown (RagePixelState state)
		{
			
		}

		public void OnMouseUp (RagePixelState state)
		{
			Sprite sprite = state.sprite;
			Color[] pixels = Utility.GetPixels (sprite);
			int originalWidth = (int)sprite.textureRect.width;
			int originalHeight = (int)sprite.textureRect.height;

			Texture2D texture = sprite.texture;

			int width = (int)m_Size.x;
			int height = (int)m_Size.y;

			texture.Resize (width, height, texture.format, texture.mipmapCount > 0);
			texture.SetPixels32 (Utility.GetDefaultPixels (texture.width, texture.height));

			for (int y = 0; y < height; y++)
				for (int x = 0; x < width; x++)
					if (x < originalWidth && y < originalHeight)
						texture.SetPixel (x, y, pixels [y * originalWidth + x]);

			texture.Apply ();
			Utility.SaveImageData (sprite, true);
			ShiftTransformAfterResize (state.transform, sprite, originalWidth, originalHeight, width, height);
			SceneView.RepaintAll ();
		}

		public void OnMouseDrag (RagePixelState state)
		{
			
		}

		public void OnMouseMove (RagePixelState state)
		{
			
		}

		public void OnRepaint (RagePixelState state)
		{
			DrawGizmo (state);
			DrawSizeLabel (state);
		}

		public bool AllowRMBColorPick ()
		{
			return false;
		}

		private void DrawSizeLabel (RagePixelState state)
		{
			Vector2 handleScreenPos = state.PixelToScreen (m_Size, false);
			Vector2 labelScreenPos = handleScreenPos + new Vector2 (16f, -8f);

			Handles.BeginGUI ();
			GUI.Label (new Rect (labelScreenPos.x, labelScreenPos.y, 100f, 100f), (int)m_Size.x + " x " + (int)m_Size.y);
			Handles.EndGUI ();
		}

		private void DrawGizmo (RagePixelState state)
		{
			Utility.DrawRectangle (
				Utility.PixelToWorld (Vector2.zero, state.transform, state.sprite, false),
				Utility.PixelToWorld (m_Size, state.transform, state.sprite, false), Color.white
				);
		}

		private void ShiftTransformAfterResize (Transform transform, Sprite sprite, int oldWidth, int oldHeight, int newWidth, int newHeight)
		{
			Vector2 pivot = Utility.GetNormalizedPivot (sprite);
			float pixelsToUnits = Utility.GetPixelsToUnits (sprite);
			Vector2 sizeDelta = new Vector2(newWidth - oldWidth, newHeight - oldHeight);

			transform.Translate (Vector2.Scale (sizeDelta, pivot) / pixelsToUnits, Space.Self);
		}
	}
}
