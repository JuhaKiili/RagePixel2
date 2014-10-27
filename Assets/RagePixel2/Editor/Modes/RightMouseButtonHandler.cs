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
		private Vector2? m_PixelCenterOffset = null;

		public bool active
		{
			get { return m_MarqueeStart != null && m_MarqueeEnd != null; }
		}

		public Rect rect
		{
			get { return Utility.GetPixelMarqueeRect ((Vector2)m_MarqueeStart - (Vector2)m_PixelCenterOffset, (Vector2)m_MarqueeEnd - (Vector2)m_PixelCenterOffset); }
		}

		public void OnSceneGUI (RagePixelState state)
		{

		}

		public void OnMouseDown (RagePixelState state)
		{
			if (Event.current.button == 0 || active)
				return;

			Vector2 mouse = GetMousePixel (state, false);
			
			if (!Utility.PixelInBounds (mouse, state.sprite))
			{
				state.paintColor = new Color (0f, 0f, 0f, 0f);
				Event.current.Use ();
				state.Repaint ();
				return;
			}

			Color newColor = state.sprite.texture.GetPixel ((int)mouse.x, (int)mouse.y);

			if (state.mode == RagePixelState.SceneMode.ReplaceColor)
				state.replaceTargetColor = newColor;
			else
				state.paintColor = newColor;

			Event.current.Use ();
			m_MarqueeStart = mouse;
			m_MarqueeEnd = mouse;
			m_PixelCenterOffset = GetPixelCenterOffset (mouse);
			state.Repaint ();
		}

		public void OnMouseUp (RagePixelState state)
		{
			if (Event.current.button == 0 || !active)
				return;

			Rect r = rect;
			r.height += 1;
			r.width += 1;
			state.brush = new Brush(state.sprite.texture, r);
			m_MarqueeStart = null;
			m_MarqueeEnd = null;
			m_PixelCenterOffset = null;
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

			Rect r = rect;
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
			return state.ScreenToPixel (Event.current.mousePosition, clamp);
		}

		private Vector2 GetPixelCenterOffset (Vector2 pos)
		{
			return new Vector2(pos.x - .5f - Mathf.Round (pos.x - .5f), pos.y - .5f - Mathf.Round (pos.y - .5f));
		}
	}
}
