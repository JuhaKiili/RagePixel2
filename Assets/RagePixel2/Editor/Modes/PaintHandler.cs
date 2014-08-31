using UnityEditor;
using UnityEngine;

namespace RagePixel2
{
	public class PaintHandler : IRagePixelMode
	{
		private Vector2? m_LastMousePixel;

		private bool activeDrag { get { return m_LastMousePixel != null; } }

		public void OnSceneGUI (RagePixelState state)
		{
			return;
		}

		public void OnMouseDown (RagePixelState state)
		{
			if (Event.current.button != 0)
				return;

			Vector2 pixel = state.ScreenToPixel (Event.current.mousePosition, false);

			if (!Utility.PixelInBounds (pixel, state.sprite))
				return;

			Vector2 minPixel = new Vector2 ((int)(pixel.x - state.brush.m_SizeX * .5f + .5f), (int)(pixel.y - state.brush.m_SizeY * .5f + .5f));
			
			state.sprite.texture.SetPixels ((int)minPixel.x, (int)minPixel.y, state.brush.m_SizeX, state.brush.m_SizeY, state.brush.m_Colors);
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
			if (Event.current.button != 0 || !activeDrag)
				return;

			Vector2 pixel = state.ScreenToPixel (Event.current.mousePosition);
			Utility.DrawPixelLine (state.sprite.texture, state.paintColor, (int)m_LastMousePixel.Value.x, (int)m_LastMousePixel.Value.y, (int)pixel.x, (int)pixel.y);
			m_LastMousePixel = pixel;
		}

		public void OnMouseMove (RagePixelState state)
		{
			SceneView.RepaintAll ();
		}

		public void OnRepaint (RagePixelState state)
		{
			state.DrawBasicPaintGizmo (activeDrag);
		}

		public bool AllowRightMouseButtonDefaultBehaviour ()
		{
			return true;
		}
	}
}
