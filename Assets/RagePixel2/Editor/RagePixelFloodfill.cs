using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;


public class RagePixelFloodFill : IRagePixelMode
{
	public void OnMouseDown (RagePixelEditorWindow owner)
	{
		if (Event.current.button == 0)
		{
			Vector2 pixel = owner.ScreenToPixel(Event.current.mousePosition);
			Color oldColor = owner.sprite.texture.GetPixel((int)pixel.x, (int)pixel.y);
			Texture2D texture = owner.sprite.texture;
			Rect spriteRect = owner.sprite.textureRect;
			FloodFill(oldColor, owner.paintColor, texture, (int)pixel.x, (int)pixel.y, (int)spriteRect.xMin, (int)spriteRect.yMin, (int)spriteRect.xMax, (int)spriteRect.yMax);
			texture.Apply();
			Event.current.Use();
		}
	}

	public void OnMouseUp (RagePixelEditorWindow owner)
	{
		if (Event.current.button == 0)
		{
			RagePixelUtility.SaveImageData(owner.sprite);
			Event.current.Use();
		}
	}

	public void OnMouseDrag (RagePixelEditorWindow owner)
	{
		return;
	}

	public void OnMouseMove (RagePixelEditorWindow owner)
	{
		SceneView.RepaintAll();
	}

	public void OnRepaint (RagePixelEditorWindow owner)
	{
		owner.DrawBasicPaintGizmo ();
	}

	public bool AllowRMBColorPick()
	{
		return true;
	}

	private static void FloodFill(Color oldColor, Color color, Texture2D tex, int fX, int fY, int minX, int minY, int maxX, int maxY)
	{
		if (RagePixelUtility.SameColor(oldColor, color)) //just in case.
			return;

		int width = maxX - minX;
		int height = maxY - minY;

		Color[] colors = tex.GetPixels(minX, minY, width, height); //store the colors into a temporary buffer
		Stack<RagePixelTexel> stack = new Stack<RagePixelTexel>(); //non-recursive stack
		stack.Push(new RagePixelTexel(fX, fY)); //original target
		while (stack.Count > 0)
		{
			RagePixelTexel n = stack.Pop();
			int index = (n.Y - minY) * width + (n.X - minX); //index into temporary buffer
			bool pixelIsInTheSprite = n.X >= minX && n.X < maxX && n.Y >= minY && n.Y < maxY;

			if (pixelIsInTheSprite)
			{
				bool colorMatches = RagePixelUtility.SameColor(colors[index], oldColor);
				if (colorMatches)
				{
					colors[index] = color;
					stack.Push(n + new RagePixelTexel(-1, 0)); //
					stack.Push(n + new RagePixelTexel(1, 0)); // add to stack in all 4 directions
					stack.Push(n + new RagePixelTexel(0, 1)); //
					stack.Push(n + new RagePixelTexel(0, -1)); //
				}
			}
		}

		tex.SetPixels(minX, minY, width, height, colors); //put the temporary buffer back into the texture
	}
}

