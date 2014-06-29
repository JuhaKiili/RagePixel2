namespace RagePixel2
{
	public interface IRagePixelMode
	{
		void OnSceneGUI (RagePixelEditorWindow owner);
		void OnMouseDown (RagePixelEditorWindow owner);
		void OnMouseUp (RagePixelEditorWindow owner);
		void OnMouseDrag (RagePixelEditorWindow owner);
		void OnMouseMove (RagePixelEditorWindow owner);
		void OnRepaint (RagePixelEditorWindow owner);
		bool AllowRMBColorPick ();
	}
}

