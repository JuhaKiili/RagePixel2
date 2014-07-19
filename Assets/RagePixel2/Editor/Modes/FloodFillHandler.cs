using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RagePixel2
{
	public class FloodFillHandler : IRagePixelMode
	{
		public void OnSceneGUI (RagePixelState state)
		{
			return;
		}

		public void OnMouseDown (RagePixelState state)
		{
			if (Event.current.button == 0)
			{
				Vector2 pixel = state.ScreenToPixel (Event.current.mousePosition);
				Color oldColor = state.sprite.texture.GetPixel ((int)pixel.x, (int)pixel.y);
				Texture2D texture = state.sprite.texture;
				Rect spriteRect = state.sprite.textureRect;
				FloodFill (oldColor, state.paintColor, texture, (int)pixel.x, (int)pixel.y, (int)spriteRect.xMin,
					(int)spriteRect.yMin, (int)spriteRect.xMax, (int)spriteRect.yMax);
				texture.Apply ();
				Event.current.Use ();
			}
		}

		public void OnMouseUp (RagePixelState state)
		{
			if (Event.current.button == 0)
			{
				Utility.SaveImageData (state.sprite, false);
				Event.current.Use ();
			}
		}

		public void OnMouseDrag (RagePixelState state)
		{
			return;
		}

		public void OnMouseMove (RagePixelState state)
		{
			SceneView.RepaintAll ();
		}

		public void OnRepaint (RagePixelState state)
		{
			state.DrawBasicPaintGizmo ();
		}

		public bool AllowRightMouseButtonDefaultBehaviour ()
		{
			return true;
		}

		private static void FloodFill (Color oldColor, Color color, Texture2D tex, int fX, int fY, int minX, int minY,
			int maxX, int maxY)
		{
			if (Utility.SameColor (oldColor, color)) //just in case.
				return;

			int width = maxX - minX;
			int height = maxY - minY;

			Color[] colors = tex.GetPixels (minX, minY, width, height); //store the colors into a temporary buffer
			Stack<Texel> stack = new Stack<Texel> (); //non-recursive stack
			stack.Push (new Texel (fX, fY)); //original target
			while (stack.Count > 0)
			{
				Texel n = stack.Pop ();
				int index = (n.Y - minY)*width + (n.X - minX); //index into temporary buffer
				bool pixelIsInTheSprite = n.X >= minX && n.X < maxX && n.Y >= minY && n.Y < maxY;

				if (pixelIsInTheSprite)
				{
					bool colorMatches = Utility.SameColor (colors[index], oldColor);
					if (colorMatches)
					{
						colors[index] = color;
						stack.Push (n + new Texel (-1, 0)); //
						stack.Push (n + new Texel (1, 0)); // add to stack in all 4 directions
						stack.Push (n + new Texel (0, 1)); //
						stack.Push (n + new Texel (0, -1)); //
					}
				}
			}

			tex.SetPixels (minX, minY, width, height, colors); //put the temporary buffer back into the texture
		}
	}
}
