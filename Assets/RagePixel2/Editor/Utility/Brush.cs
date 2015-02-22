using Assets.RagePixel2.Editor.Utility;
using UnityEngine;
using UnityEditor;

namespace RagePixel2
{
	public class Brush
	{
		public Color[] m_Colors;
		public IntVector2 m_Size;
		public IntVector2 m_BrushPivot;

		private Texture2D m_BrushTexture; 
		public Texture2D brushTexture
		{
			get
			{
				if (m_Size.x > 0 && m_Size.y > 0)
				{
					if (m_BrushTexture == null)
					{
						m_BrushTexture = new Texture2D(m_Size.x, m_Size.y);
						m_BrushTexture.filterMode = FilterMode.Point;
						m_BrushTexture.hideFlags = HideFlags.HideAndDontSave;
						m_BrushTexture.SetPixels(Utility.GetYReversed(m_Colors, m_Size));
						m_BrushTexture.Apply();
					}
					else if (m_BrushTexture.width != m_Size.x || m_BrushTexture.height != m_Size.y)
					{
						m_BrushTexture.Resize (m_Size.x, m_Size.y);
						m_BrushTexture.SetPixels (Utility.GetYReversed(m_Colors, m_Size));
						m_BrushTexture.Apply();
					}
				}
				else
				{
					m_BrushTexture = new Texture2D(1,1);
					m_BrushTexture.hideFlags = HideFlags.HideAndDontSave;
				}
				return m_BrushTexture;
			}
		}
		
		public Brush (IntVector2 size, Color color)
		{
			m_Size = size;
			m_Colors = new Color[m_Size.x * m_Size.y];

			for (int i = 0; i < m_Colors.Length; i++)
				m_Colors[i] = color;
		}

		public Brush(IntVector2 size, Color[] colors)
		{
			m_Size = size;
			m_Colors = colors;
		}

		public Brush (Texture2D texture, Rect area)
		{
			m_Size = new IntVector2(area.width, area.height);
			m_Colors = texture.GetPixels ((int)area.xMin, (int)area.yMin, m_Size.x, m_Size.y);
		}
	}
}
