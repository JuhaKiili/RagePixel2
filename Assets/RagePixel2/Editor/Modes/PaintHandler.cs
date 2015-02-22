using Assets.RagePixel2.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace RagePixel2
{
	public class PaintHandler : IRagePixelMode
	{
		private IntVector2? m_LastMousePixel;

		private bool activeDrag { get { return m_LastMousePixel != null; } }

		public void OnSceneGUI (RagePixelState state)
		{
			return;
		}

		public void OnMouseDown (RagePixelState state)
		{
			if (Event.current.button != 0)
				return;

			IntVector2 pixel = state.ScreenToPixel(Event.current.mousePosition, false);
			IntVector2 minPixel = pixel - state.brush.m_BrushPivot;

			Utility.SetPixelsClamped (state.sprite.texture, minPixel, state.brush.m_Size, state.brush.m_Colors);
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

			IntVector2 pixel = state.ScreenToPixel(Event.current.mousePosition, false);
			Utility.DrawPixelLine (state.sprite.texture, state.brush, m_LastMousePixel.Value, pixel);
			m_LastMousePixel = pixel;
		}

		public void OnMouseMove (RagePixelState state)
		{
			SceneView.RepaintAll ();
		}

		public void OnRepaint (RagePixelState state)
		{
			state.DrawBasicPaintGizmo ();
			state.DrawSpriteBounds ();
		}

		public bool AllowPickingDefaultBehaviour ()
		{
			return true;
		}
	}
}
