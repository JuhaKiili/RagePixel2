using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class RagePixelReplaceColor : IRagePixelMode
{
	private Color[] m_Snapshot;

	public void OnMouseDown(RagePixelEditorWindow owner)
	{
		return;
	}

	public void OnMouseUp(RagePixelEditorWindow owner)
	{
		return;
	}

	public void OnMouseDrag(RagePixelEditorWindow owner)
	{
		return;
	}

	public void OnMouseMove(RagePixelEditorWindow owner)
	{
		SceneView.RepaintAll();
	}

	public void OnRepaint(RagePixelEditorWindow owner)
	{
		owner.DrawBasicPaintGizmo();
	}

	public void SaveSnapshot (Sprite sprite)
	{
		Texture2D texture = sprite.texture;
		Rect textureRect = sprite.textureRect;
		m_Snapshot = texture.GetPixels ((int) textureRect.xMin, (int) textureRect.yMin, (int) textureRect.width, (int) textureRect.height);
	}

	public void ReplaceColor (Sprite sprite, Color from, Color to)
	{
		Texture2D texture = sprite.texture;
		Rect textureRect = sprite.textureRect;
		int minX = (int)textureRect.xMin;
		int maxX = (int)textureRect.xMax;
		int minY = (int)textureRect.yMin;
		int maxY = (int)textureRect.yMax;

		int index = 0;
		for (int y = minY; y < maxY; y++)
			for (int x = minX; x < maxX; x++)
				if (RagePixelUtility.SameColor (m_Snapshot[index++], from))
					texture.SetPixel (x, y, to);
				
		texture.Apply();
		SceneView.RepaintAll();
	}

	public void Apply (Sprite sprite)
	{
		RagePixelUtility.SaveImageData (sprite);
		m_Snapshot = null;
	}

	public void Cancel(Sprite sprite, Color originalColor)
	{
		if (m_Snapshot == null)
			return;

		ReplaceColor (sprite, originalColor, originalColor);
		m_Snapshot = null;
	}

	public bool AllowRMBColorPick()
	{
		return true;
	}
}

