using UnityEngine;
using UnityEditor;

namespace RagePixel2
{
	public class Brush
	{
		public Color[] m_Colors;
		public int m_SizeX = 0;
		public int m_SizeY = 0;

		private Texture2D m_BrushTexture; 
		public Texture2D brushTexture
		{
			get
			{
				if (m_SizeX > 0 && m_SizeY > 0)
				{
					if (m_BrushTexture == null)
					{
						m_BrushTexture = new Texture2D (m_SizeX, m_SizeY);
						m_BrushTexture.filterMode = FilterMode.Point;
						m_BrushTexture.hideFlags = HideFlags.HideAndDontSave;
						m_BrushTexture.SetPixels (Utility.GetYReversed(m_Colors, m_SizeX, m_SizeY));
						m_BrushTexture.Apply();
					}
					else if (m_BrushTexture.width != m_SizeX || m_BrushTexture.height != m_SizeY)
					{
						m_BrushTexture.Resize (m_SizeX, m_SizeY);
						m_BrushTexture.SetPixels (Utility.GetYReversed(m_Colors, m_SizeX, m_SizeY));
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
		
		public Brush (int sizeX, int sizeY, Color color)
		{
			m_SizeX = sizeX;
			m_SizeY = sizeY;
			m_Colors = new Color[m_SizeX * m_SizeY];

			for (int i = 0; i < m_Colors.Length; i++)
				m_Colors[i] = color;
		}

		public Brush (int sizeX, int sizeY, Color[] colors)
		{
			m_SizeX = sizeX;
			m_SizeY = sizeY;
			m_Colors = colors;
		}

		public Brush (Texture2D texture, Rect area)
		{
			m_SizeX = (int)area.width;
			m_SizeY = (int)area.height;
			m_Colors = texture.GetPixels ((int)area.xMin, (int)area.yMin, m_SizeX, m_SizeY);
		}
	}
}