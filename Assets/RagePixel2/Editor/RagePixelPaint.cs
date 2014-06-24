using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

class RagePixelPaint : IRagePixelMode
{
	private Vector2? m_LastMousePixel;

	public void OnMouseDown (RagePixelEditorWindow owner)
	{
		if (Event.current.button != 0)
			return;

		Vector2 pixel = owner.ScreenToPixel(Event.current.mousePosition);
		owner.sprite.texture.SetPixel((int)pixel.x, (int)pixel.y, owner.paintColor);
		owner.sprite.texture.Apply ();
		m_LastMousePixel = pixel;
		owner.Repaint();
		Event.current.Use();
	}

	public void OnMouseUp (RagePixelEditorWindow owner)
	{
		if (Event.current.button != 0)
			return;

		m_LastMousePixel = null;
		RagePixelUtility.SaveImageData (owner.sprite);
		Event.current.Use();
	}

	public void OnMouseDrag (RagePixelEditorWindow owner)
	{
		if (Event.current.button != 0)
			return;

		Vector2 pixel = owner.ScreenToPixel(Event.current.mousePosition);
		if (m_LastMousePixel != null)
		{
			RagePixelUtility.DrawPixelLine (owner.sprite.texture, owner.paintColor, (int) m_LastMousePixel.Value.x,
				(int) m_LastMousePixel.Value.y, (int) pixel.x, (int) pixel.y);
		}
		m_LastMousePixel = pixel;
	}

	public void OnMouseMove (RagePixelEditorWindow owner)
	{
		SceneView.RepaintAll();
	}

	public void OnRepaint (RagePixelEditorWindow owner)
	{
		owner.DrawBasicPaintGizmo ();
	}

	public bool AllowRMBColorPick ()
	{
		return true;
	}
}

