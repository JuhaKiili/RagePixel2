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

	private static Tool s_PreviousTool;
	public enum SceneMode { Default=0, Paint }
	public SceneMode mode = SceneMode.Default;

	[MenuItem("GameObject/Create Other/RagePixel Sprite")]
	public static void CreateSpriteMenuItem()
	{
		GameObject gameObject = new GameObject();
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
		{
			Tools.current = s_PreviousTool;
			mode = SceneMode.Default;
		}
	}

	public void PencilOnGUI ()
	{
		EditorGUI.BeginChangeCheck();
		GUILayout.Toggle(mode == SceneMode.Paint, RagePixelResources.pencil, GUI.skin.button, GUILayout.Width(k_SceneButtonSize), GUILayout.Height(k_SceneButtonSize));
		if (EditorGUI.EndChangeCheck() && mode != SceneMode.Paint)
		{
			s_PreviousTool = Tools.current;
			Tools.current = Tool.None;
			mode = SceneMode.Paint;
		}
	}

	private void DrawPaintGizmo ()
	{
		Vector2 p = ScreenToPixel(Event.current.mousePosition);

		Vector2[] uvPoints = new Vector2[4];
		uvPoints[0] = RagePixelUtility.PixelToUV(new Vector2(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y)), sprite);
		uvPoints[1] = RagePixelUtility.PixelToUV(new Vector2(Mathf.FloorToInt(p.x) + 1, Mathf.FloorToInt(p.y)), sprite);
		uvPoints[2] = RagePixelUtility.PixelToUV(new Vector2(Mathf.FloorToInt(p.x) + 1, Mathf.FloorToInt(p.y) + 1), sprite);
		uvPoints[3] = RagePixelUtility.PixelToUV(new Vector2(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y) + 1), sprite);

		Vector3[] worldPoints = new Vector3[5];
		worldPoints[0] = transform.TransformPoint(RagePixelUtility.UVToLocal(uvPoints[0], sprite));
		worldPoints[1] = transform.TransformPoint(RagePixelUtility.UVToLocal(uvPoints[1], sprite));
		worldPoints[2] = transform.TransformPoint(RagePixelUtility.UVToLocal(uvPoints[2], sprite));
		worldPoints[3] = transform.TransformPoint(RagePixelUtility.UVToLocal(uvPoints[3], sprite));
		worldPoints[4] = worldPoints[0];

		Vector3[] screenPoints = new Vector3[5];
		for (int i = 0; i < worldPoints.Length; i++)
			screenPoints[i] = RagePixelUtility.WorldToScreen(worldPoints[i], transform);

		Vector3[] shadowPoints = new Vector3[5];
		for (int i = 0; i < screenPoints.Length; i++)
			shadowPoints[i] = screenPoints[i] + new Vector3(1f, 1f, 0f);

		Handles.BeginGUI();
		Handles.color = Color.black;
		Handles.DrawPolyLine(shadowPoints);
		Handles.color = Color.white;
		Handles.DrawPolyLine(screenPoints);
		Handles.EndGUI();
	}

	private Vector2 ScreenToPixel (Vector2 screenPosition)
	{
		Vector3 localPosition = RagePixelUtility.ScreenToLocal (screenPosition, transform);
		Vector2 uvPosition = RagePixelUtility.LocalToUV(localPosition, sprite);
		Vector2 pixelPosition = RagePixelUtility.UVToPixel(uvPosition, sprite);
		return pixelPosition;
	}
}
