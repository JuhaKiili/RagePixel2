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
				m_Size = new Vector2((int)m_Size.x, (int)m_Size.y);
				state.Repaint ();
			}
		}

		public void OnMouseDown (RagePixelState state)
		{
			
		}

		public void OnMouseUp (RagePixelState state)
		{
			
		}

		public void OnMouseDrag (RagePixelState state)
		{
			
		}

		public void OnMouseMove (RagePixelState state)
		{
			
		}

		public void OnRepaint (RagePixelState state)
		{
			Utility.DrawRectangle (
				Utility.PixelToWorld(Vector2.zero, state.transform, state.sprite, false), 
				Utility.PixelToWorld(m_Size, state.transform, state.sprite, false), Color.white
				);

			Vector2 handleScreenPos = state.PixelToScreen (m_Size, false);
			Vector2 labelScreenPos = handleScreenPos + new Vector2 (16f, -8f);

			Handles.BeginGUI();
			GUI.Label (new Rect(labelScreenPos.x, labelScreenPos.y, 100f, 100f), (int)m_Size.x + " x " + (int)m_Size.y);
			Handles.EndGUI();
		}

		public bool AllowRMBColorPick ()
		{
			return false;
		}
	}
}
