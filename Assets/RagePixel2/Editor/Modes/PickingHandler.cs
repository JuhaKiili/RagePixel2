using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.RagePixel2.Editor.Utility;
using UnityEditorInternal;
using UnityEngine;

namespace RagePixel2
{
	public class PickingHandler : IRagePixelMode
	{
		private IntVector2? m_MarqueeStart = null;
		private IntVector2? m_MarqueeEnd = null;
		
		public bool active
		{
			get { return m_MarqueeStart != null && m_MarqueeEnd != null; }
		}

		public bool pickingEvent
		{
			get { return Event.current.button != 0 || !Event.current.control; }
		}

		public Rect rect
		{
			get { return Utility.GetPixelMarqueeRect (m_MarqueeStart.Value, m_MarqueeEnd.Value); }
		}

		public void OnSceneGUI (RagePixelState state)
		{

		}

		public void OnMouseDown (RagePixelState state)
		{
			if (Event.current.button != 0 || !Event.current.control || active)
				return;

			IntVector2 mouse = GetMousePixel(state, false);
			
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
			state.Repaint ();
		}

		public void OnMouseUp (RagePixelState state)
		{
			if (Event.current.button != 0 || !Event.current.control || !active)
				return;

			Rect r = rect;
			r.height += 1;
			r.width += 1;
			state.brush = new Brush(state.sprite.texture, r);
			
			IntVector2 pivot = m_MarqueeEnd.Value - m_MarqueeStart.Value;
			state.brush.m_BrushPivot = new IntVector2(Math.Max(0, pivot.x), Math.Max(0, pivot.y));
			
			m_MarqueeStart = null;
			m_MarqueeEnd = null;
			Event.current.Use ();
			state.Repaint ();
		}

		public void OnMouseDrag (RagePixelState state)
		{
			if (Event.current.button != 0 || !Event.current.control || !active)
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
				Utility.PixelToWorld(new IntVector2(r.xMin, r.yMin), state.transform, state.sprite, false),
				Utility.PixelToWorld(new IntVector2(r.xMax + 1, r.yMax + 1), state.transform, state.sprite, false), 
				Color.white
			);
		}

		public bool AllowPickingDefaultBehaviour ()
		{
			return false;
		}
		
		private IntVector2 GetMousePixel (RagePixelState state, bool clamp)
		{
			return state.ScreenToPixel (Event.current.mousePosition, clamp);
		}
	}
}
