using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RagePixel2
{
	public class RagePixelState : ScriptableObject
	{
		[NonSerialized] public Action Repaint;

		[SerializeField] private SceneMode m_Mode;
		[SerializeField] private Color m_PaintColor = Color.green;
		[SerializeField] private Color m_ReplaceTargetColor = Color.blue;

		private RightMouseButtonHandler m_RightMouseButtonHandler;

		private PaintHandler m_PaintHandler;
		private FloodFillHandler m_FloodFillHandler;
		private ReplaceColorHandler m_ReplaceColorHandler;
		private ResizeHandler m_ResizeHandler;

		private Tool m_PreviousTool;
		private bool m_MouseIsDown;
		private Brush m_Brush;

		public Object target
		{
			get { return Selection.activeObject; }
		}

		public GameObject gameObject
		{
			get { return (target as GameObject); }
		}

		public Transform transform
		{
			get { return (target as GameObject) != null ? (target as GameObject).transform : null; }
		}

		public SpriteRenderer spriteRenderer
		{
			get { return (target as GameObject) != null ? (target as GameObject).GetComponent<SpriteRenderer> () : null; }
		}

		public Sprite sprite
		{
			get { return spriteRenderer != null ? spriteRenderer.sprite : null; }
		}

		public Color paintColor
		{
			get { return m_PaintColor; }
			set { m_PaintColor = value; }
		}

		public Color replaceTargetColor
		{
			get { return m_ReplaceTargetColor; }
			set
			{
				replaceColorHandler.ReplaceColor (sprite, paintColor, value);
				m_ReplaceTargetColor = value;
			}
		}

		public Brush brush
		{
			get
			{
				if (m_Brush == null)
					m_Brush = new Brush(1, 1, paintColor);
				return m_Brush;
			}
			set { m_Brush = value; }
		}

		public SceneMode mode
		{
			get { return m_Mode; }
			set
			{
				if (m_Mode != value)
				{
					OnModeChange (m_Mode, value);
					m_Mode = value;
				}
			}
		}

		public bool editingEnabled
		{
			get
			{
				return sprite != null &&
				       (PrefabUtility.GetPrefabType (target) == PrefabType.None ||
				        PrefabUtility.GetPrefabType (target) == PrefabType.PrefabInstance);
			}
		}

		public RightMouseButtonHandler rightMouseButtonHandler
		{
			get
			{
				if (m_RightMouseButtonHandler == null)
					m_RightMouseButtonHandler = new RightMouseButtonHandler ();
				return m_RightMouseButtonHandler;
			}
		}

		public PaintHandler paintHandler
		{
			get
			{
				if (m_PaintHandler == null)
					m_PaintHandler = new PaintHandler ();
				return m_PaintHandler;
			}
		}

		public FloodFillHandler floodFillHandler
		{
			get
			{
				if (m_FloodFillHandler == null)
					m_FloodFillHandler = new FloodFillHandler ();
				return m_FloodFillHandler;
			}
		}

		public ReplaceColorHandler replaceColorHandler
		{
			get
			{
				if (m_ReplaceColorHandler == null)
					m_ReplaceColorHandler = new ReplaceColorHandler ();
				return m_ReplaceColorHandler;
			}
		}

		public ResizeHandler resizeHandler
		{
			get
			{
				if (m_ResizeHandler == null)
					m_ResizeHandler = new ResizeHandler (sprite.textureRect.size);
				return m_ResizeHandler;
			}
		}

		public void OnEnable ()
		{
			hideFlags = HideFlags.HideAndDontSave;

			SceneView.onSceneGUIDelegate += OnSceneGUI;
			EditorApplication.playmodeStateChanged += OnPlayModeChanged;

			m_PreviousTool = Tool.Move;
			Tools.current = Tool.Move;
		}

		public void OnDisable ()
		{
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
			EditorApplication.playmodeStateChanged -= OnPlayModeChanged;
			ResetMode ();
		}

		public void DrawBasicPaintGizmo ()
		{
			Color color = m_MouseIsDown ? Color.white : new Color (1f, 1f, 1f, 0.4f);
			Color shadowColor = m_MouseIsDown ? Color.black : new Color (0f, 0f, 0f, 0.4f);
			Utility.DrawPaintGizmo (Event.current.mousePosition, color, shadowColor, brush, transform, sprite);
		}

		public Vector2 ScreenToPixel (Vector2 screenPosition)
		{
			return ScreenToPixel(screenPosition, true);
		}

		public Vector2 ScreenToPixel (Vector2 screenPosition, bool clamp)
		{
			return Utility.ScreenToPixel (screenPosition, transform, sprite, clamp);
		}

		public Vector2 PixelToScreen (Vector2 pixelPosition)
		{
			return PixelToScreen (pixelPosition, true);
		}

		public Vector2 PixelToScreen (Vector2 pixelPosition, bool clamp)
		{
			return Utility.PixelToScreen (pixelPosition, transform, sprite, clamp);
		}

		public void OnPlayModeChanged ()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
				ResetMode ();
		}

		private void ResetMode ()
		{
			m_Mode = SceneMode.Default;
			Tools.current = m_PreviousTool != Tool.None ? m_PreviousTool : Tool.Move;
			if (Repaint != null)
				Repaint ();
		}

		private void OnModeChange (SceneMode oldMode, SceneMode newMode)
		{
			PreserveToolsCurrent (oldMode, newMode);

			if (newMode == SceneMode.ReplaceColor)
				OnEnterColorReplacerMode ();
			if (oldMode == SceneMode.ReplaceColor)
				OnExitColorReplacerMode ();

			SceneView.RepaintAll ();
		}

		public void ApplyColorReplace ()
		{
			replaceColorHandler.Apply (sprite);
			paintColor = replaceTargetColor;
			mode = RagePixelState.SceneMode.Paint;
		}

		private void OnExitColorReplacerMode ()
		{
			replaceColorHandler.Cancel (sprite, paintColor);
		}

		private void OnEnterColorReplacerMode ()
		{
			replaceColorHandler.SaveSnapshot (sprite);
			replaceTargetColor = paintColor;
		}

		private void PreserveToolsCurrent (SceneMode oldMode, SceneMode newMode)
		{
			if (newMode != SceneMode.Default)
			{
				if (oldMode == SceneMode.Default)
					m_PreviousTool = Tools.current;
				Tools.current = Tool.None;
			}
			else
			{
				if (Tools.current == Tool.None)
					Tools.current = m_PreviousTool != Tool.None ? m_PreviousTool : Tool.Move;
			}
		}

		public void OnSceneGUI (SceneView sceneView)
		{
			if (sprite == null)
				return;

			IRagePixelMode handler = GetModeHandler ();
			if (handler == null)
				return;

			UpdateMouseIsDown ();
			UpdateHotControl ();
			UpdateCursor ();

			if (handler.AllowRightMouseButtonDefaultBehaviour ())
				TriggerModeEventHandler (rightMouseButtonHandler);

			if (!rightMouseButtonHandler.active)
				TriggerModeEventHandler (handler);
		}

		private void UpdateCursor ()
		{
			if (mode != SceneMode.Default)
				EditorGUIUtility.AddCursorRect (new Rect (0, 0, 10000, 10000), MouseCursor.ArrowPlus);
		}

		private void UpdateMouseIsDown ()
		{
			if (Event.current.type == EventType.MouseDown)
				m_MouseIsDown = true;
			if (Event.current.type == EventType.MouseUp)
				m_MouseIsDown = false;
		}

		private void UpdateHotControl ()
		{
			int id = GUIUtility.GetControlID ("RagePixel".GetHashCode (), FocusType.Passive);

			if (Event.current.type == EventType.MouseDown)
				GUIUtility.hotControl = id;
			if (Event.current.type == EventType.MouseUp)
				GUIUtility.hotControl = 0;
		}

		public void TriggerModeEventHandler (IRagePixelMode handler)
		{
			handler.OnSceneGUI (this);

			switch (Event.current.type)
			{
				case EventType.MouseDown:
					handler.OnMouseDown (this);
					break;
				case EventType.MouseUp:
					handler.OnMouseUp (this);
					break;
				case EventType.MouseMove:
					handler.OnMouseMove (this);
					break;
				case EventType.MouseDrag:
					handler.OnMouseDrag (this);
					break;
				case EventType.Repaint:
					handler.OnRepaint (this);
					break;
			}
		}

		private IRagePixelMode GetModeHandler ()
		{
			IRagePixelMode handler = null;

			switch (mode)
			{
				case SceneMode.Default:
					break;
				case SceneMode.Paint:
					handler = paintHandler;
					break;
				case SceneMode.FloodFill:
					handler = floodFillHandler;
					break;
				case SceneMode.ReplaceColor:
					handler = replaceColorHandler;
					break;
				case SceneMode.Resize:
					handler = resizeHandler;
					break;
			}
			return handler;
		}
		
		public enum SceneMode
		{
			Default = 0,
			Paint,
			FloodFill,
			ReplaceColor,
			Resize
		}
	}
}
