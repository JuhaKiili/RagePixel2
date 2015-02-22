using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.RagePixel2.Editor.Utility;
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

		private Hashtable modeHandlerCache;
		
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
			set
			{
				if (m_PaintColor != value)
				{
					m_Brush = null;
					m_PaintColor = value;
				}
			}
		}

		public Color replaceTargetColor
		{
			get { return m_ReplaceTargetColor; }
			set
			{
				GetModeHandler<ReplaceColorHandler>().ReplaceColor (sprite, paintColor, value);
				m_ReplaceTargetColor = value;
			}
		}

		public Brush brush
		{
			get
			{
				if (m_Brush == null)
					m_Brush = new Brush(new IntVector2(1, 1), paintColor);
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

		public void OnEnable ()
		{
			hideFlags = HideFlags.HideAndDontSave;

			SceneView.onSceneGUIDelegate += OnSceneGUI;
			EditorApplication.playmodeStateChanged += OnPlayModeChanged;

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
			Color color = m_MouseIsDown ? Color.white : new Color (0.8f, 0.8f, 0.8f, 1f);
			Color shadowColor = m_MouseIsDown ? Color.black : new Color (0.1f, 0.1f, 0.1f, 1f);
			Utility.DrawPaintGizmo (Event.current.mousePosition, color, shadowColor, brush, transform, sprite);
		}

		public void DrawSpriteBounds ()
		{
			Utility.DrawSpriteBounds(new Color(0.8f, 0.8f, 0.8f, 1f), new Color(0.1f, 0.1f, 0.1f, 1f), transform, sprite);
		}

		public IntVector2 ScreenToPixel (Vector2 screenPosition)
		{
			return ScreenToPixel(screenPosition, true);
		}

		public IntVector2 ScreenToPixel (Vector2 screenPosition, bool clamp)
		{
			return Utility.ScreenToPixel (screenPosition, transform, sprite, clamp);
		}

		public Vector2 PixelToScreen (IntVector2 pixelPosition)
		{
			return PixelToScreen (pixelPosition, true);
		}

		public Vector2 PixelToScreen (IntVector2 pixelPosition, bool clamp)
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
			Tools.current = Tool.Move;
			if (Repaint != null)
				Repaint ();
		}

		private void OnModeChange (SceneMode oldMode, SceneMode newMode)
		{
			UpdateToolsCurrent (oldMode, newMode);

			if (newMode == SceneMode.ReplaceColor)
				OnEnterColorReplacerMode ();
			if (oldMode == SceneMode.ReplaceColor)
				OnExitColorReplacerMode ();

			SceneView.RepaintAll ();
		}

		private void OnUserToolChange ()
		{
			m_Mode = SceneMode.Default;
			Repaint ();
		}

		public void ApplyColorReplace ()
		{
			GetModeHandler<ReplaceColorHandler>().Apply (sprite);
			paintColor = replaceTargetColor;
			mode = RagePixelState.SceneMode.Paint;
		}

		private void OnExitColorReplacerMode ()
		{
			GetModeHandler<ReplaceColorHandler>().Cancel (sprite, paintColor);
		}

		private void OnEnterColorReplacerMode ()
		{
			GetModeHandler<ReplaceColorHandler>().SaveSnapshot (sprite);
			replaceTargetColor = paintColor;
			Utility.ShowColorPicker(replaceTargetColor);
		}

		private void UpdateToolsCurrent (SceneMode oldMode, SceneMode newMode)
		{
			if (newMode != SceneMode.Default)
			{
				Tools.current = Tool.None;
			}
			else
			{
				if (Tools.current == Tool.None)
					Tools.current = Tool.Move;					
			}
		}

		public void OnSceneGUI (SceneView sceneView)
		{
			if (Tools.current != Tool.None)
				OnUserToolChange();

			if (sprite == null && mode != SceneMode.CreateSprite)
				return;

			IRagePixelMode handler = GetModeHandler();
			if (handler == null)
				return;

			UpdateMouseIsDown();
			UpdateHotControl();
			UpdateCursor();

			if (handler.AllowPickingDefaultBehaviour ())
				TriggerModeEventHandler (GetModeHandler<PickingHandler>());

			if (!GetModeHandler<PickingHandler>().active)
				TriggerModeEventHandler (handler);
		}

		private void UpdateCursor ()
		{
			if (mode != SceneMode.Default && Event.current.button == 0)
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

			if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
				GUIUtility.hotControl = id;
			if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
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
					handler = GetModeHandler<PaintHandler>();
					break;
				case SceneMode.FloodFill:
					handler = GetModeHandler<FloodFillHandler>();
					break;
				case SceneMode.ReplaceColor:
					handler = GetModeHandler<ReplaceColorHandler>();
					break;
				case SceneMode.Resize:
					handler = GetModeHandler<ResizeHandler>();
					break;
				case SceneMode.CreateSprite:
					handler = GetModeHandler<CreateSpriteHandler>();
					break;
			}
			return handler;
		}

		private T GetModeHandler<T> () where T : new ()
		{
			if (modeHandlerCache == null)
				modeHandlerCache = new Hashtable ();
			else if (modeHandlerCache.ContainsKey (typeof (T)))
				return (T)modeHandlerCache[typeof (T)];

			T newInstance = new T ();
			modeHandlerCache.Add (typeof(T), newInstance);
			return newInstance;
		}

		public enum SceneMode
		{
			Default = 0,
			Paint,
			FloodFill,
			ReplaceColor,
			Resize,
			CreateSprite
		}
	}
}
