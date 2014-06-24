using System;
using System.IO;
using System.Reflection;
using Assets.RagePixel2.Editor;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;

public class RagePixelEditorWindow : EditorWindow
{
	private const float k_SceneButtonSize = 32f;
	public enum SceneMode { Default = 0, Paint, FloodFill }
	
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
		get { return (target as GameObject) != null ? (target as GameObject).GetComponent<SpriteRenderer>() : null; }
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

	public SceneMode mode
	{
		get { return m_Mode; }
		set
		{
			if (m_Mode != value)
			{
				OnModeChange(m_Mode, value);
				m_Mode = value;
			}
		}
	}

	private bool editingEnabled
	{
		get { return sprite != null && (PrefabUtility.GetPrefabType(target) == PrefabType.None || PrefabUtility.GetPrefabType(target) == PrefabType.PrefabInstance); }
	}

	// Modes

	private IRagePixelMode m_PaintHandler;
	public IRagePixelMode paintHandler
	{
		get 
		{ 
			if (m_PaintHandler == null) 
				m_PaintHandler = new RagePixelPaint();
			return m_PaintHandler;
		}
	}

	private IRagePixelMode m_FloodFillHandler;
	public IRagePixelMode floodFillHandler
	{
		get
		{
			if (m_FloodFillHandler == null)
				m_FloodFillHandler = new RagePixelFloodFill();
			return m_FloodFillHandler;
		}
	}

	private bool m_MouseIsDown;
	private Color m_PaintColor = Color.green;
	private Tool m_PreviousTool;
	private SceneMode m_Mode;

	public void OnGUI () 
	{
		GUILayout.BeginHorizontal();
		ArrowOnGUI();
		EditorGUI.BeginDisabledGroup(!editingEnabled);
		PaintColorOnGUI();
		PencilOnGUI();
		FloodFillOnGUI();
		EditorGUI.EndDisabledGroup();
		GUILayout.EndHorizontal();
	}

	public void PaintColorOnGUI()
	{
		m_PaintColor = RagePixelUtility.PaintColorField(m_PaintColor, k_SceneButtonSize, k_SceneButtonSize);
	}

	public void ArrowOnGUI()
	{
		BasicModeButton(SceneMode.Default, RagePixelResources.arrow);
	}

	public void PencilOnGUI()
	{
		BasicModeButton(SceneMode.Paint, RagePixelResources.pencil);
	}

	public void FloodFillOnGUI()
	{
		BasicModeButton(SceneMode.FloodFill, RagePixelResources.floodfill);
	}

	public void OnSceneGUI (SceneView sceneView)
	{
		if (sprite == null)
			return;
		
		UpdateHotControl ();
		UpdateCursor ();
		UpdateMouseIsDown ();
		
		IRagePixelMode handler = GetModeHandler ();
		
		if (handler == null)
			return;

		if (handler.AllowRMBColorPick ())
			HandleColorPicking ();

		TriggerModeHandler (handler);
	}

	private void UpdateCursor ()
	{
		if (mode != SceneMode.Default)
			EditorGUIUtility.AddCursorRect(new Rect(0, 0, 10000, 10000), MouseCursor.ArrowPlus);
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

	public void TriggerModeHandler(IRagePixelMode handler)
	{
		switch (Event.current.type)
		{
			case EventType.MouseDown:
				handler.OnMouseDown(this);
				break;
			case EventType.MouseUp:
				handler.OnMouseUp(this);
				break;
			case EventType.MouseMove:
				handler.OnMouseMove(this);
				break;
			case EventType.MouseDrag:
				handler.OnMouseDrag(this);
				break;
			case EventType.Repaint:
				handler.OnRepaint(this);
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
		}
		return handler;
	}

	private void HandleColorPicking ()
	{
		if (Event.current.type == EventType.MouseDown && Event.current.button > 0)
		{
			Vector2 pixel = ScreenToPixel (Event.current.mousePosition);
			paintColor = sprite.texture.GetPixel ((int) pixel.x, (int) pixel.y);
			Event.current.Use();
			Repaint();
		}
	}

	public void OnEnable()
	{
		title = "RagePixel";
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

	public void OnSelectionChange ()
	{
		Repaint();
	}

	public void DrawBasicPaintGizmo ()
	{
		Color color = m_MouseIsDown ? Color.white : new Color(1f, 1f, 1f, 0.4f);
		Color shadowColor = m_MouseIsDown ? Color.black : new Color(0f, 0f, 0f, 0.4f);
		RagePixelUtility.DrawPaintGizmo (Event.current.mousePosition, color, shadowColor, transform, sprite);
	}

	public Vector2 ScreenToPixel(Vector2 screenPosition)
	{
		return RagePixelUtility.ScreenToPixel(screenPosition, transform, sprite);
	}

	public Vector2 PixelToScreen(Vector2 pixelPosition)
	{
		return RagePixelUtility.PixelToScreen(pixelPosition, transform, sprite);
	}

	private void ResetMode ()
	{
		m_Mode = SceneMode.Default;
		Tools.current = m_PreviousTool != Tool.None ? m_PreviousTool : Tool.Move;
		Repaint();
	}

	private void OnPlayModeChanged ()
	{
		if (EditorApplication.isPlayingOrWillChangePlaymode)
			ResetMode();
	}
	
	private void BasicModeButton (SceneMode buttonMode, Texture2D icon)
	{
		EditorGUI.BeginChangeCheck();
		GUILayout.Toggle(mode == buttonMode, icon, GUI.skin.button, GUILayout.Width(k_SceneButtonSize), GUILayout.Height(k_SceneButtonSize));
		if (EditorGUI.EndChangeCheck() && mode != buttonMode)
			mode = buttonMode;
	}

	private void OnModeChange(SceneMode oldMode, SceneMode newMode)
	{
		if (newMode != SceneMode.Default)
		{
			if (oldMode == SceneMode.Default)
				m_PreviousTool = Tools.current;
			Tools.current = Tool.None;
		}
		else
		{
			Tools.current = m_PreviousTool != Tool.None ? m_PreviousTool : Tool.Move;
		}
	}

	[MenuItem("Window/RagePixel")]
	static void Init()
	{
		GetWindow(typeof(RagePixelEditorWindow));
	}

	[MenuItem("GameObject/Create Other/RagePixel Sprite")]
	public static void CreateSpriteMenuItem()
	{
		GameObject gameObject = new GameObject();
		gameObject.name = "New Sprite";
		gameObject.AddComponent<SpriteRenderer>();
		gameObject.transform.position = RagePixelUtility.GetSceneViewCenter();
		gameObject.GetComponent<SpriteRenderer>().sprite = RagePixelUtility.CreateNewSprite();
		gameObject.GetComponent<SpriteRenderer>().sharedMaterial = RagePixelResources.defaultMaterial;
		Selection.activeGameObject = gameObject;
		SceneView.FrameLastActiveSceneView();
	}
}
