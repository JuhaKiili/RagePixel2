using System.IO;
using System.Reflection;
using Assets.RagePixel2.Editor;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using System.Collections;

[CustomEditor (typeof (RagePixelSprite))]
public class RagePixelSpriteEditor : Editor
{
	private const float k_SceneButtonSize = 32f;

	public RagePixelSprite ragePixelSprite 
	{
		get { return (target as RagePixelSprite); }
	}

	public Transform transform
	{
		get { return ragePixelSprite.transform; }
	}

	public SpriteRenderer spriteRenderer
	{
		get { return ragePixelSprite.GetComponent<SpriteRenderer> (); }
	}

	public Sprite sprite
	{
		get { return spriteRenderer.sprite; }
	}

	public Color paintColor
	{
		get { return paintColorPickerGUI.selectedColor; }
	}

	public EditorWindow colorPickerWindow
	{
		get { return RagePixelReflection.GetEditorStatic ("ColorPicker", "get") as EditorWindow; }
	}

	private RagePixelColorPickerGUI m_PaintColorPickerGUI;
	public RagePixelColorPickerGUI paintColorPickerGUI
	{
		get
		{
			if (m_PaintColorPickerGUI == null)
			{
				m_PaintColorPickerGUI = new RagePixelColorPickerGUI();
				m_PaintColorPickerGUI.gizmoVisible = false;
				m_PaintColorPickerGUI.visible = false;
				m_PaintColorPickerGUI.gizmoPositionX = 5;
				m_PaintColorPickerGUI.gizmoPositionY = 5;
				m_PaintColorPickerGUI.positionX = m_PaintColorPickerGUI.gizmoPositionX + m_PaintColorPickerGUI.gizmoPixelWidth;
				m_PaintColorPickerGUI.positionY = m_PaintColorPickerGUI.gizmoPositionY;
			}
			return m_PaintColorPickerGUI;
		}
	}

	[SerializeField] private static Tool s_PreviousTool;
	
	public enum SceneMode { Default=0, Paint }
	[SerializeField] private SceneMode m_Mode = SceneMode.Default;
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
				s_PreviousTool = Tools.current;
			Tools.current = Tool.None;
		}
		else
		{
			Tools.current = s_PreviousTool != Tool.None ? s_PreviousTool : Tool.Move;
		}
	}

	[MenuItem("GameObject/Create Other/RagePixel Sprite")]
	public static void CreateSpriteMenuItem()
	{
		GameObject gameObject = new GameObject();
		gameObject.name = "New Sprite";
		gameObject.transform.position = RagePixelUtility.GetSceneViewCenter();
		gameObject.AddComponent<RagePixelSprite>();
		gameObject.GetComponent<SpriteRenderer>().sprite = RagePixelUtility.CreateNewSprite();
		gameObject.GetComponent<SpriteRenderer>().sharedMaterial = RagePixelResources.defaultMaterial;
		Selection.activeGameObject = gameObject;
		SceneView.FrameLastActiveSceneView ();
	}
	
	public override void OnInspectorGUI ()
	{

	}

	public void OnSceneGUI ()
	{
		Handles.BeginGUI();
		SceneViewToolbarOnGUI();
		Handles.EndGUI();
		
		HandlePainting ();
	}

	public void Update()
	{
		
	}

	private void HandlePainting ()
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

	private void HandleModePaint ()
	{
		int id = GUIUtility.GetControlID("Paint".GetHashCode(), FocusType.Passive);
		EditorGUIUtility.AddCursorRect(new Rect(0, 0, 10000, 10000), MouseCursor.ArrowPlus);
	
		switch (Event.current.type)
		{
			case (EventType.MouseDown):
				GUIUtility.hotControl = id;
				Event.current.Use ();
				break;
			case (EventType.MouseDrag):
				Vector2 pixel = ScreenToPixel (Event.current.mousePosition);
				sprite.texture.SetPixel ((int) pixel.x, (int) pixel.y, paintColor);
				sprite.texture.Apply ();
				break;
			case (EventType.MouseMove):
				SceneView.RepaintAll();
				break;
			case (EventType.MouseUp):
				RagePixelUtility.SaveImageData(sprite);
				GUIUtility.hotControl = 0;
				break;
			case (EventType.Repaint):
				DrawPaintGizmo();
				break;
		}
	}

	private void SceneViewToolbarOnGUI ()
	{
		GUILayout.BeginArea(new Rect(2, 2, 500, 200));
		GUILayout.BeginHorizontal();
		ColorPickerOnGUI ();
		GUILayout.EndHorizontal();
		GUILayout.EndArea();

		GUILayout.FlexibleSpace();
		GUILayout.BeginVertical();
		ArrowOnGUI();
		PencilOnGUI();
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
	}

	public void ColorPickerOnGUI ()
	{
		if (GUILayout.Button(paintColorPickerGUI.colorGizmoTexture, GUILayout.Width(k_SceneButtonSize), GUILayout.Height(k_SceneButtonSize)))
			paintColorPickerGUI.visible = !paintColorPickerGUI.visible;

		if (paintColorPickerGUI.visible)
		{
			if (paintColorPickerGUI.HandleGUIEvent (Event.current))
				Event.current.Use ();
			
			GUI.DrawTexture(paintColorPickerGUI.bounds, paintColorPickerGUI.colorPickerTexture);
		}
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
		Handles.color = Color.black;
		Handles.DrawPolyLine(shadowPolyLine);
		Handles.color = Color.white;
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
