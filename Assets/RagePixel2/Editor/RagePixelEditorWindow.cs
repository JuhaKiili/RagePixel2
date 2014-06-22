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
		get {  return (target as GameObject) != null ? (target as GameObject).transform : null; }
	}

	public SpriteRenderer spriteRenderer
	{
		get { return (target as GameObject) != null ? (target as GameObject).GetComponent<SpriteRenderer>() : null; }
	}

	public Sprite sprite
	{
		get { return spriteRenderer != null ? spriteRenderer.sprite : null; }
	}

	private bool editingEnabled
	{
		get { return sprite != null && (PrefabUtility.GetPrefabType(target) == PrefabType.None || PrefabUtility.GetPrefabType(target) == PrefabType.PrefabInstance); }
	}

	private Color m_PaintColor = Color.green;
	private bool m_MouseIsDown;
	private Vector2? m_LastMousePixel = null;
	private Tool m_PreviousTool;
	
	public enum SceneMode { Default=0, Paint }
	private SceneMode m_Mode;
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

	private void OnModeChange (SceneMode oldMode, SceneMode newMode)
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

	[MenuItem ("Window/RagePixel")]
	static void Init ()
	{
		GetWindow(typeof(RagePixelEditorWindow));
	}
	
	[MenuItem("GameObject/Create Other/RagePixel Sprite")]
	public static void CreateSpriteMenuItem()
	{
		GameObject gameObject = new GameObject();
		gameObject.name = "New Sprite";
		gameObject.AddComponent<SpriteRenderer> ();
		gameObject.transform.position = RagePixelUtility.GetSceneViewCenter();
		gameObject.GetComponent<SpriteRenderer>().sprite = RagePixelUtility.CreateNewSprite();
		gameObject.GetComponent<SpriteRenderer>().sharedMaterial = RagePixelResources.defaultMaterial;
		Selection.activeGameObject = gameObject;
		SceneView.FrameLastActiveSceneView();
	}
	
	public void OnGUI () 
	{
		GUILayout.BeginHorizontal();
		ArrowOnGUI();
		EditorGUI.BeginDisabledGroup(!editingEnabled);
		PaintColorOnGUI();
		PencilOnGUI();
		EditorGUI.EndDisabledGroup();
		GUILayout.EndHorizontal();
	}

	public void OnSceneGUI (SceneView sceneView)
	{
		if (sprite == null)
			return;

		switch (mode)
		{
			case SceneMode.Default:
				break;
			case SceneMode.Paint:
				HandleModePaint();
				break;
		}
	}

	public void Update()
	{
		
	}

	public void OnEnable()
	{
		title = "RagePixel";
		SceneView.onSceneGUIDelegate += OnSceneGUI;
		EditorApplication.playmodeStateChanged += OnPlayModeChanged;
		m_PreviousTool = Tools.current;
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

	private void HandleModePaint ()
	{
		int id = GUIUtility.GetControlID("Paint".GetHashCode(), FocusType.Passive);
		EditorGUIUtility.AddCursorRect(new Rect(0, 0, 10000, 10000), MouseCursor.ArrowPlus);
	
		switch (Event.current.type)
		{
			case (EventType.MouseDown):
				GUIUtility.hotControl = id;
				HandleModePaintOnMouseDown ();
				break;
			case (EventType.MouseDrag):
				HandleModePaintOnMouseDrag ();
				break;
			case (EventType.MouseMove):
				SceneView.RepaintAll();
				break;
			case (EventType.MouseUp):
				HandleModePaintOnMouseUp ();
				GUIUtility.hotControl = 0;
				break;
			case (EventType.Repaint):
				DrawPaintGizmo();
				break;
		}
	}

	private void HandleModePaintOnMouseUp ()
	{
		m_MouseIsDown = false;
		m_LastMousePixel = null;
		if (Event.current.button == 0)
			RagePixelUtility.SaveImageData (sprite);
		Event.current.Use ();
	}

	private void HandleModePaintOnMouseDrag ()
	{
		if (Event.current.button != 0)
			return;

		Vector2 pixel = ScreenToPixel (Event.current.mousePosition);
		if (m_LastMousePixel != null)
			RagePixelUtility.DrawPixelLine (sprite.texture, m_PaintColor, (int) m_LastMousePixel.Value.x, (int) m_LastMousePixel.Value.y, (int) pixel.x, (int) pixel.y);
		
		m_LastMousePixel = pixel;
	}

	private void HandleModePaintOnMouseDown ()
	{
		m_MouseIsDown = true;
		Vector2 pixel = ScreenToPixel (Event.current.mousePosition);

		if (Event.current.button == 0)
		{
			sprite.texture.SetPixel ((int) pixel.x, (int) pixel.y, m_PaintColor);
			sprite.texture.Apply ();
		}
		else
			m_PaintColor = sprite.texture.GetPixel ((int) pixel.x, (int) pixel.y);

		m_LastMousePixel = pixel;
		Event.current.Use ();
		Repaint ();
	}

	public void PaintColorOnGUI ()
	{
		m_PaintColor = RagePixelUtility.PaintColorField (m_PaintColor, k_SceneButtonSize, k_SceneButtonSize);
	}

	public void ArrowOnGUI ()
	{
		EditorGUI.BeginChangeCheck();
		GUILayout.Toggle(mode == SceneMode.Default, RagePixelResources.arrow, GUI.skin.button, GUILayout.Width(k_SceneButtonSize), GUILayout.Height(k_SceneButtonSize));
		if (EditorGUI.EndChangeCheck() && mode != SceneMode.Default)
			mode = SceneMode.Default;
	}

	public void PencilOnGUI ()
	{
		EditorGUI.BeginChangeCheck();
		GUILayout.Toggle(mode == SceneMode.Paint, RagePixelResources.pencil, GUI.skin.button, GUILayout.Width(k_SceneButtonSize), GUILayout.Height(k_SceneButtonSize));
		if (EditorGUI.EndChangeCheck() && mode != SceneMode.Paint)
			mode = SceneMode.Paint;
	}

	private void DrawPaintGizmo ()
	{
		Color color = m_MouseIsDown ? Color.white : new Color(1f, 1f, 1f, 0.4f);
		Color shadowColor = m_MouseIsDown ? Color.black : new Color(0f, 0f, 0f, 0.4f);
		RagePixelUtility.DrawPaintGizmo (color, shadowColor, transform, sprite);
	}

	private Vector2 ScreenToPixel (Vector2 screenPosition)
	{
		return RagePixelUtility.ScreenToPixel(screenPosition, transform, sprite);
	}

	private Vector2 PixelToScreen(Vector2 pixelPosition)
	{
		return RagePixelUtility.PixelToScreen(pixelPosition, transform, sprite);
	}
}
