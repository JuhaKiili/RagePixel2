namespace RagePixel2
{
	public class Texel
	{
		public int X;
		public int Y;

		public Texel ()
		{
			X = 0;
			Y = 0;
		}

		public Texel (int _x, int _y)
		{
			X = _x;
			Y = _y;
		}

		public static Texel operator + (Texel a, Texel b)
		{
			return new Texel (a.X + b.X, a.Y + b.Y);
		}
	}
}
