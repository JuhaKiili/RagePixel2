using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RagePixel2
{
	public class RagePixelEditorWindow : EditorWindow
	{
		[SerializeField] private RagePixelState m_State;

		public void OnEnable ()
		{
			title = "RagePixel";

			if (m_State == null)
				m_State = CreateInstance<RagePixelState> ();

			m_State.Repaint += this.Repaint;
		}

		public void OnDisable ()
		{
			if (m_State != null)
				m_State.Repaint -= this.Repaint;
		}

		public void OnGUI ()
		{
			GUILayout.BeginHorizontal ();
			ArrowOnGUI ();
			EditorGUI.BeginDisabledGroup (!m_State.editingEnabled);
			PaintColorOnGUI ();
			GUILayout.EndHorizontal ();
			GUILayout.Space (k_VerticalSpaceBetweenButtons);
			GUILayout.BeginHorizontal ();
			PencilOnGUI ();
			FloodFillOnGUI ();
			GUILayout.EndHorizontal ();
			GUILayout.Space (k_VerticalSpaceBetweenButtons);
			GUILayout.BeginHorizontal ();
			ResizeOnGUI ();
			GUILayout.EndHorizontal ();
			EditorGUI.EndDisabledGroup ();
		}

		public void PaintColorOnGUI ()
		{
			m_State.paintColor = Utility.PaintColorField (m_State.paintColor, k_ButtonSize, k_ButtonSize);
			if (m_State.mode == RagePixelState.SceneMode.ReplaceColor)
			{
				m_State.replaceTargetColor = Utility.PaintColorField (m_State.replaceTargetColor, k_ButtonSize,
					k_ButtonSize);
				if (GUILayout.Button ("OK", GUILayout.Width (k_ButtonSize), GUILayout.Height ((k_ButtonSize))))
					m_State.ApplyColorReplace ();
			}
			else
			{
				BasicModeButton (RagePixelState.SceneMode.ReplaceColor, Resources.arrowRight);
			}
		}

		public void ArrowOnGUI ()
		{
			BasicModeButton (RagePixelState.SceneMode.Default, Resources.arrow);
		}

		public void PencilOnGUI ()
		{
			BasicModeButton (RagePixelState.SceneMode.Paint, Resources.pencil);
		}

		public void FloodFillOnGUI ()
		{
			BasicModeButton (RagePixelState.SceneMode.FloodFill, Resources.floodfill);
		}

		public void ResizeOnGUI ()
		{
			BasicModeButton (RagePixelState.SceneMode.Resize, Resources.resize);
		}

		public void OnSelectionChange ()
		{
			Repaint ();
		}

		private void BasicModeButton (RagePixelState.SceneMode buttonMode, Texture2D icon)
		{
			EditorGUI.BeginChangeCheck ();
			GUILayout.Toggle (m_State.mode == buttonMode, icon, GUI.skin.button, GUILayout.Width (k_ButtonSize), GUILayout.Height (k_ButtonSize));
			if (EditorGUI.EndChangeCheck () && m_State.mode != buttonMode)
				m_State.mode = buttonMode;
		}

		[MenuItem ("Window/RagePixel")]
		private static void Init ()
		{
			GetWindow (typeof (RagePixelEditorWindow));
		}

		[MenuItem ("GameObject/Create Other/RagePixel Sprite")]
		public static void CreateSpriteMenuItem ()
		{
			GameObject gameObject = new GameObject ();
			gameObject.name = "New Sprite";
			gameObject.AddComponent<SpriteRenderer> ();
			gameObject.transform.position = Utility.GetSceneViewCenter ();
			gameObject.GetComponent<SpriteRenderer> ().sprite = Utility.CreateNewSprite ();
			gameObject.GetComponent<SpriteRenderer> ().sharedMaterial = Resources.defaultMaterial;
			Selection.activeGameObject = gameObject;
			SceneView.FrameLastActiveSceneView ();
		}

		private const float k_ButtonSize = 32f;
		const float k_VerticalSpaceBetweenButtons = 2f;
	}
}
