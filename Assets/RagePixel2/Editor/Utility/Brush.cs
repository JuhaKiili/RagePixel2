using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RagePixel2
{
	public class Brush
	{
		public Color[] m_Colors;
		public int m_SizeX = 0;
		public int m_SizeY = 0;

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
