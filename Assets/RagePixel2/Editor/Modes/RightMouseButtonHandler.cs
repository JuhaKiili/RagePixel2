using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditorInternal;
using UnityEngine;

namespace RagePixel2
{
	public class RightMouseButtonHandler : IRagePixelMode
	{
		private Vector2? m_MarqueeStart = null;
		private Vector2? m_MarqueeEnd = null;

		public bool active
		{
			get { return m_MarqueeStart != null && m_MarqueeEnd != null; }
		}

		public void OnSceneGUI (RagePixelState state)
		{

		}

		public void OnMouseDown (RagePixelState state)
		{
			if (Event.current.button == 0 || active)
				return;

			Vector2 pixel = GetMousePixel (state, false);

			if (!Utility.PixelInBounds (pixel, state.sprite))
				return;

			Color newColor = state.sprite.texture.GetPixel ((int)pixel.x, (int)pixel.y);

			if (state.mode == RagePixelState.SceneMode.ReplaceColor)
				state.replaceTargetColor = newColor;
			else
				state.paintColor = newColor;

			Event.current.Use ();
			m_MarqueeStart = pixel;
			m_MarqueeEnd = pixel;
			state.Repaint ();
		}

		public void OnMouseUp (RagePixelState state)
		{
			if (Event.current.button == 0 || !active)
				return;

			Rect r = Utility.GetPixelMarqueeRect ((Vector2)m_MarqueeStart, (Vector2)m_MarqueeEnd);
			r.height += 1;
			r.width += 1;
			state.brush = new Brush(state.sprite.texture, r);
			m_MarqueeStart = null;
			m_MarqueeEnd = null;
			Event.current.Use ();
			state.Repaint ();
		}

		public void OnMouseDrag (RagePixelState state)
		{
			if (Event.current.button == 0 || !active)
				return;

			m_MarqueeEnd = GetMousePixel (state, true);
			Event.current.Use();
		}

		public void OnMouseMove (RagePixelState state)
		{
			
		}

		public void OnRepaint (RagePixelState state)
		{
			if (!active)
				return;
			
			state.DrawSpriteBounds ();

			Rect r = Utility.GetPixelMarqueeRect ((Vector2)m_MarqueeStart, (Vector2)m_MarqueeEnd);
			Utility.DrawRectangle (
				Utility.PixelToWorld (new Vector2(r.xMin, r.yMin), state.transform, state.sprite, false),
				Utility.PixelToWorld (new Vector2(r.xMax + 1, r.yMax + 1), state.transform, state.sprite, false), 
				Color.white
			);
		}

		public bool AllowRightMouseButtonDefaultBehaviour ()
		{
			return false;
		}
		
		private Vector2 GetMousePixel (RagePixelState state, bool clamp)
		{
			Vector2 pixel = state.ScreenToPixel (Event.current.mousePosition, clamp);
			return new Vector2((int)pixel.x, (int)pixel.y);
		}
	}
}
