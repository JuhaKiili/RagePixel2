using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.RagePixel2.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace RagePixel2
{
	public class CreateSpriteHandler : IRagePixelMode
	{
		private bool m_Dragging = false;
		private Vector2 m_DragStart;

		private Rect pixelRect
		{
			get
			{
				IntVector2 p1 = Utility.WorldToPixel(dragStartWorld);
				IntVector2 p2 = Utility.WorldToPixel(dragEndWorld);
				return Utility.GetPixelMarqueeRect (p1, p2);
			}
		}

		private Vector2 dragStartWorld
		{
			get { return Utility.ScreenToWorld (m_DragStart); }
		}

		private Vector2 dragEndWorld
		{
			get { return Utility.ScreenToWorld (Event.current.mousePosition); }
		}

		private Vector2 worldMinPointSnapped
		{
			get { return Utility.PixelToWorld(new IntVector2(pixelRect.xMin, pixelRect.yMin)); }
		}

		private Vector2 worldMaxPointSnapped
		{
			get { return Utility.PixelToWorld(new IntVector2(pixelRect.xMax, pixelRect.yMax)); }
		}

		public void OnSceneGUI (RagePixelState state)
		{
			return;
		}

		public void OnMouseDown (RagePixelState state)
		{
			if (Event.current.button == 0)
			{
				m_DragStart = Event.current.mousePosition;
				m_Dragging = true;
				Event.current.Use ();
			}
		}

		public void OnMouseUp (RagePixelState state)
		{
			if (Event.current.button == 0)
			{
				CreateSprite ();
				m_Dragging = false;
				state.mode = RagePixelState.SceneMode.Default;
				Event.current.Use ();
			}
		}

		public void OnMouseDrag (RagePixelState state)
		{
			Event.current.Use();
		}

		public void OnMouseMove (RagePixelState state)
		{
			return;
		}

		public void OnRepaint (RagePixelState state)
		{
			if (m_Dragging)
			{
				DrawMarquee ();
				DrawSizeLabel ();
			}
		}

		private void DrawMarquee ()
		{
			Utility.DrawRectangle (worldMinPointSnapped, worldMaxPointSnapped, Color.white);
		}

		private void DrawSizeLabel ()
		{
			GUIContent labelContent = new GUIContent((int)(pixelRect.width) + " x " + (int)(pixelRect.height));
			Vector2 labelSize = GUI.skin.label.CalcSize (labelContent);
			
			Vector2 offset = new Vector2(dragFromLeftToRight ? 16f : -labelSize.x - 8f, -labelSize.y * .5f);
			Vector2 labelScreenPos = Event.current.mousePosition + offset;

			Rect labelRect = new Rect (labelScreenPos.x, labelScreenPos.y, labelSize.x, labelSize.y);

			Handles.BeginGUI ();
			GUI.Label (labelRect, labelContent);
			Handles.EndGUI ();
		}

		private void CreateSprite ()
		{
			Sprite sprite = Utility.CreateNewSprite ((int)pixelRect.width, (int)pixelRect.height);
			
			if (sprite == null)
				return;

			GameObject gameObject = new GameObject();
			gameObject.name = sprite.name;
			gameObject.AddComponent<SpriteRenderer> ();
			gameObject.transform.position = (worldMinPointSnapped + worldMaxPointSnapped) * .5f;
			gameObject.GetComponent<SpriteRenderer> ().sprite = sprite;
			gameObject.GetComponent<SpriteRenderer> ().sharedMaterial = Resources.defaultMaterial;
			Selection.activeGameObject = gameObject;
			SceneView.lastActiveSceneView.Focus();
		}

		private bool dragFromLeftToRight
		{
			get { return m_DragStart.x <= Event.current.mousePosition.x; }
		}

		public bool AllowPickingDefaultBehaviour ()
		{
			return false;
		}
	}
}
