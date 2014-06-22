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
				m_MouseIsDown = true;
				Vector2 p = ScreenToPixel(Event.current.mousePosition);
				
				if (Event.current.button == 0)
					GUIUtility.hotControl = id;
				else
					m_PaintColor = sprite.texture.GetPixel((int)p.x, (int)p.y);

				m_LastMousePixel = p;
				Event.current.Use ();
				Repaint();
				break;
			case (EventType.MouseDrag):
				if (Event.current.button == 0)
				{
					Vector2 pixel = ScreenToPixel (Event.current.mousePosition);
					if (m_LastMousePixel != null)
						RagePixelUtility.DrawPixelLine(sprite.texture, m_PaintColor, (int)m_LastMousePixel.Value.x, (int)m_LastMousePixel.Value.y, (int)pixel.x, (int)pixel.y);
					m_LastMousePixel = pixel;
				}
				break;
			case (EventType.MouseMove):
				SceneView.RepaintAll();
				break;
			case (EventType.MouseUp):
				m_MouseIsDown = false;
				m_LastMousePixel = null;
				if (Event.current.button == 0)
					RagePixelUtility.SaveImageData(sprite);
				GUIUtility.hotControl = 0;
				Event.current.Use();
				break;
			case (EventType.Repaint):
				DrawPaintGizmo();
				break;
		}
	}

	public void PaintColorOnGUI ()
	{
		// internal static Color ColorField (Rect position, GUIContent label, Color value, bool showEyedropper, bool showAlpha)
		object[] parameters = new object[5];
		parameters[0] = EditorGUILayout.GetControlRect(false, GUILayout.Width(k_SceneButtonSize), GUILayout.Height(k_SceneButtonSize));
		parameters[1] = new GUIContent ("");
		parameters[2] = m_PaintColor;
		parameters[3] = false;
		parameters[4] = true;

		Type[] types = new Type[5];
		types[0] = typeof (Rect);
		types[1] = typeof (GUIContent);
		types[2] = typeof (Color);
		types[3] = typeof (bool);
		types[4] = typeof (bool);
		
		//Debug.Log(RagePixelReflection.GetEditorType("EditorGUI"));
		object returnValue = RagePixelReflection.InvokeEditorStatic("EditorGUI", "ColorField", parameters, types);
		m_PaintColor = (Color)returnValue;

		//const float k_ColorPipetteIconWidth = 20f;
		//m_PaintColor = EditorGUILayout.ColorField (m_PaintColor, GUILayout.Width (k_SceneButtonSize + k_ColorPipetteIconWidth));
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
		Vector2 pixel = ScreenToPixel(Event.current.mousePosition);

		Vector3[] screenPolyLine = new Vector3[5];
		screenPolyLine[0] = PixelToScreen(new Vector2(Mathf.FloorToInt(pixel.x), Mathf.FloorToInt(pixel.y)));
		screenPolyLine[1] = PixelToScreen(new Vector2(Mathf.FloorToInt(pixel.x+1), Mathf.FloorToInt(pixel.y)));
		screenPolyLine[2] = PixelToScreen(new Vector2(Mathf.FloorToInt(pixel.x+1), Mathf.FloorToInt(pixel.y+1)));
		screenPolyLine[3] = PixelToScreen(new Vector2(Mathf.FloorToInt(pixel.x), Mathf.FloorToInt(pixel.y+1)));
		screenPolyLine[4] = screenPolyLine[0];

		Vector3[] shadowPolyLine = new Vector3[5];
		for (int i = 0; i < screenPolyLine.Length; i++)
			shadowPolyLine[i] = screenPolyLine[i] + new Vector3(1f, 1f, 0f);

		Handles.BeginGUI();
		Handles.color = m_MouseIsDown ? Color.black : new Color(0f, 0f, 0f, 0.4f);
		Handles.DrawPolyLine(shadowPolyLine);
		Handles.color = m_MouseIsDown ? Color.white : new Color(1f, 1f, 1f, 0.4f);
		Handles.DrawPolyLine(screenPolyLine);
		Handles.EndGUI();
	}

	private Vector2 ScreenToPixel (Vector2 screenPosition)
	{
		Vector3 localPosition = RagePixelUtility.ScreenToLocal (screenPosition, transform);
		Vector2 uvPosition = RagePixelUtility.LocalToUV(localPosition, sprite);
		Vector2 pixelPosition = RagePixelUtility.UVToPixel(uvPosition, sprite);
		return pixelPosition;
	}

	private Vector2 PixelToScreen(Vector2 pixelPosition)
	{
		Vector2 uvPosition = RagePixelUtility.PixelToUV(pixelPosition, sprite);
		Vector3 localPosition = RagePixelUtility.UVToLocal(uvPosition, sprite);
		Vector2 screenPosition = RagePixelUtility.LocalToScreen(localPosition, transform);
		return screenPosition;
	}
}
