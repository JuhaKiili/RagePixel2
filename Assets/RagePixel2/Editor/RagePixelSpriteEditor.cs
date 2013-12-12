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

	public override void OnInspectorGUI ()
	{

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
			sprite.texture.SetPixel ((int) pixel.x, (int) pixel.y, Color.red);
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
