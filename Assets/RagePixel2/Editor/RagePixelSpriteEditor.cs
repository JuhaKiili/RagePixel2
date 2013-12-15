using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor (typeof (RagePixelSprite))]
public class RagePixelSpriteEditor : Editor
{
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
	
	private bool colorPickerVisible
	{
		get
		{
			bool? visible = RagePixelReflection.GetEditorStatic("ColorPicker", "visible") as bool?;
			return visible.GetValueOrDefault ();
		}
	}

	private Color? colorPickerColor
	{
		get
		{
			bool? visible = RagePixelReflection.GetEditorStatic ("ColorPicker", "visible") as bool?;
			if (visible.GetValueOrDefault())
				return RagePixelReflection.GetEditorStatic("ColorPicker", "color") as Color?;
			return null;
		}
		set { RagePixelReflection.SetEditorStatic ("ColorPicker", "color", new object[] { value }); }
	}

	private Color m_PaintColor;
	public Color paintColor
	{
		get
		{
			if (colorPickerColor != null)
				m_PaintColor = colorPickerColor.GetValueOrDefault();
			return m_PaintColor;
		}
	}

	private EditorWindow m_ColorPickerWindow;
	public EditorWindow colorPickerWindow
	{
		get { return RagePixelReflection.GetEditorStatic ("ColorPicker", "get") as EditorWindow; }
	}

	public override void OnInspectorGUI ()
	{
		if (GUILayout.Button ("Color Picker"))
		{
			colorPickerWindow.Show();
			colorPickerColor = m_PaintColor;
		}
	}

	public void OnSceneGUI ()
	{
		int id = GUIUtility.GetControlID ("Paint".GetHashCode (), FocusType.Passive);

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
		{
			GUIUtility.hotControl = 0;
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
