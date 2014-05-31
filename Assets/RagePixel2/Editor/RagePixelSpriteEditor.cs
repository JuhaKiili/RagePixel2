using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor (typeof (RagePixelSprite))]
public class RagePixelSpriteEditor : Editor
{
	private const float k_DefaultSceneButtonWidth = 32f;

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
		if (GUILayout.Button ("Create"))
			spriteRenderer.sprite = RagePixelUtility.CreateNewSprite ();
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

		int id = GUIUtility.GetControlID ("Paint".GetHashCode (), FocusType.Passive);

		if (Event.current.type == EventType.MouseDown)
		{
			GUIUtility.hotControl = id;
			Event.current.Use ();
		}
		if (Event.current.type == EventType.MouseDrag)
		{
			Vector2 pixel = ScreenToPixel (Event.current.mousePosition);
			sprite.texture.SetPixel((int)pixel.x, (int)pixel.y, paintColor);
			sprite.texture.Apply ();
		}
		if (Event.current.type == EventType.MouseUp)
			GUIUtility.hotControl = 0;
	}

	private void SceneViewToolbarOnGUI ()
	{
		GUILayout.BeginVertical ();
		HandleColorPickerGUI ();
		GUILayout.EndVertical ();
	}

	public void HandleColorPickerGUI()
	{
		if (GUI.Button(paintColorPickerGUI.gizmoBounds, paintColorPickerGUI.colorGizmoTexture))
			paintColorPickerGUI.visible = !paintColorPickerGUI.visible;

		if (paintColorPickerGUI.visible)
		{
			if (paintColorPickerGUI.HandleGUIEvent (Event.current))
				Event.current.Use ();
			
			GUI.DrawTexture(paintColorPickerGUI.bounds, paintColorPickerGUI.colorPickerTexture);
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
