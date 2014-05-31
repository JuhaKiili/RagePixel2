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
		
		if (Event.current.type == EventType.MouseDown)
		{
			GUIUtility.hotControl = id;
			Event.current.Use ();
		}
		if (Event.current.type == EventType.MouseDrag)
		{
			Vector2 pixel = ScreenToPixel (Event.current.mousePosition);
			sprite.texture.SetPixel ((int) pixel.x, (int) pixel.y, paintColor);
			sprite.texture.Apply ();
		}
		if (Event.current.type == EventType.MouseUp)
			GUIUtility.hotControl = 0;
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
		if (GUILayout.Toggle(mode == SceneMode.Default, RagePixelResources.arrow, GUI.skin.button, GUILayout.Width(k_SceneButtonSize), GUILayout.Height(k_SceneButtonSize)) && GUI.changed)
		{
			Tools.current = s_PreviousTool;
			mode = SceneMode.Default;
		}
	}

	public void PencilOnGUI ()
	{
		if (GUILayout.Toggle(mode == SceneMode.Paint, RagePixelResources.pencil, GUI.skin.button, GUILayout.Width(k_SceneButtonSize), GUILayout.Height(k_SceneButtonSize)) && GUI.changed)
		{
			s_PreviousTool = Tools.current;
			Tools.current = Tool.None;
			mode = SceneMode.Paint;
		}
	}

	private Vector2 ScreenToPixel (Vector2 screenPosition)
	{
		Vector3 localPosition = RagePixelUtility.ScreenToLocal (screenPosition, transform);
		Vector2 uvPosition = RagePixelUtility.LocalToUV(localPosition, sprite);
		Vector2 pixelPosition = RagePixelUtility.UVToPixel(uvPosition, sprite);
		return pixelPosition;
	}
}
