using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.RagePixel2.Editor.Utility
{
	public struct IntVector2
	{
		private int m_X;
		private int m_Y;

		public int x { get { return m_X; } set { m_X = value; } }
		public int y { get { return m_Y; } set { m_Y = value; } }

		public IntVector2(int x, int y)
		{
			m_X = x;
			m_Y = y;
		}

		public IntVector2(float x, float y)
		{
			m_X = Mathf.FloorToInt(x);
			m_Y = Mathf.FloorToInt(y);
		}

		public IntVector2(Vector2 pos)
		{
			m_X = Mathf.FloorToInt(pos.x);
			m_Y = Mathf.FloorToInt(pos.y);
		}

		public void ClampToZero()
		{
			m_X = Mathf.Max(0, m_X);
			m_Y = Mathf.Max(0, m_Y);
		}

		public static IntVector2 operator +(IntVector2 a, IntVector2 b)
		{
			return new IntVector2(a.x + b.x, a.y + b.y);
		}

		public static IntVector2 operator +(IntVector2 a, Vector2 b)
		{
			return new IntVector2(a.x + (int)b.x, a.y + (int)b.y);
		}

		public static IntVector2 operator -(IntVector2 a, IntVector2 b)
		{
			return new IntVector2(a.x - b.x, a.y - b.y);
		}

		public static IntVector2 operator -(IntVector2 a, Vector2 b)
		{
			return new IntVector2(a.x - (int)b.x, a.y - (int)b.y);
		}

		public static IntVector2 operator *(IntVector2 a, int b)
		{
			return new IntVector2(a.x * b, a.y * b);
		}

		public override string ToString()
		{
			return "(" + m_X + "," + m_Y + ")";
		}

		public static bool operator ==(IntVector2 lhs, IntVector2 rhs)
		{
			return lhs.GetHashCode() == rhs.GetHashCode();
		}

		public static bool operator !=(IntVector2 lhs, IntVector2 rhs)
		{
			return lhs.GetHashCode() != rhs.GetHashCode();
		}

		public override bool Equals(object other)
		{
			if (!(other is IntVector2)) return false;

			IntVector2 rhs = (IntVector2)other;
			return x.Equals(rhs.x) && y.Equals(rhs.y);
		}

		public override int GetHashCode()
		{
			return x.GetHashCode() ^ (y.GetHashCode() << 2);
		}
	}
}
