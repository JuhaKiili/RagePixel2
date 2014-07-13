using UnityEditor;
using UnityEngine;

namespace RagePixel2
{
	public class PaintHandler : IRagePixelMode
	{
		private Vector2? m_LastMousePixel;

		public void OnSceneGUI (RagePixelState state)
		{
			return;
		}

		public void OnMouseDown (RagePixelState state)
		{
			if (Event.current.button != 0)
				return;

			Vector2 pixel = state.ScreenToPixel (Event.current.mousePosition);
			state.sprite.texture.SetPixel ((int)pixel.x, (int)pixel.y, state.paintColor);
			state.sprite.texture.Apply ();
			m_LastMousePixel = pixel;
			state.Repaint ();
			Event.current.Use ();
		}

		public void OnMouseUp (RagePixelState state)
		{
			if (Event.current.button != 0)
				return;

			m_LastMousePixel = null;
			Utility.SaveImageData (state.sprite, false);
			Event.current.Use ();
		}

		public void OnMouseDrag (RagePixelState state)
		{
			if (Event.current.button != 0)
				return;

			Vector2 pixel = state.ScreenToPixel (Event.current.mousePosition);
			if (m_LastMousePixel != null)
			{
				Utility.DrawPixelLine (state.sprite.texture, state.paintColor, (int)m_LastMousePixel.Value.x,
					(int)m_LastMousePixel.Value.y, (int)pixel.x, (int)pixel.y);
			}
			m_LastMousePixel = pixel;
		}

		public void OnMouseMove (RagePixelState state)
		{
			SceneView.RepaintAll ();
		}

		public void OnRepaint (RagePixelState state)
		{
			state.DrawBasicPaintGizmo ();
		}

		public bool AllowRMBColorPick ()
		{
			return true;
		}
	}
}
