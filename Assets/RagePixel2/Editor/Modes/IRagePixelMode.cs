namespace RagePixel2
{
	public interface IRagePixelMode
	{
		void OnSceneGUI (RagePixelState state);
		void OnMouseDown (RagePixelState state);
		void OnMouseUp (RagePixelState state);
		void OnMouseDrag (RagePixelState state);
		void OnMouseMove (RagePixelState state);
		void OnRepaint (RagePixelState state);
		bool AllowPickingDefaultBehaviour ();
	}
}
