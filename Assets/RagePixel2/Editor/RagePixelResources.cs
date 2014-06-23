using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.RagePixel2.Editor
{
	class RagePixelResources
	{
		private const string s_ArrowBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAW0lEQVQ4Ea3RMQoAIAxDURXvf2W1W4dfm6KZVMIjYGu/s04q5qByBUHAUBUJARW5AgqSAhkiATdEBiKkBBAy7ZHST/xX2p16uCAqE4CqL2YrcIEHns9+wTNGwAY88i/6p8M2mgAAAABJRU5ErkJggg==";
		private const string s_PencilBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAeElEQVQ4Ee1TWw6AMAibxvtunmSPC2OayFaiM/hpIj+jQDcKWQgOk9NqreIotyUgsdmsAzH5dQf/62N8jlnbkkEV+djkIYR3b4Xdo43Dqre1xuFHf51lY4zLLMdx04EmQN5T6j8vl9IxfK27nFgfZICsF7APAseBD1Qpy22Hqw6tAAAAAElFTkSuQmCC";
		private const string s_FloodFillBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAqklEQVQ4Ea2Q0RHEIAhE1YbitaP1aT2XinJZZ5Yh0UA+jh8w7FsCIRhxnJG37TAkIT41AbP3yXmU332f9NMHKDXsmUwGK9gyuRhY8JOJGLyBadJ7H2WtNQ6DtzDBUkpgnVprcm1OWGUC7GE66gQ3LzQMfTyDTPR+34JhkrSArsy6d58MDdaXI0Ks1/FgDpFdsIqGKFhNZg9ZDPC4m3iwrACYoY+qr83+3/MPGIJpXw0Y6mQAAAAASUVORK5CYII=";
		private const string s_ArrowRightBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAY0lEQVQ4Ee1QQQoAIAirHu7Bj1eLlBJCJOiUF9dsa5bSr9AP1F5WUCzhna1JFoEdCH/quRdmm8HkhgZpiUj1zKxYAEzCK4gYHamvDJBgW2F19/Cq9e7qHJFRSkTBlTj62Lv7DdDlKfbVti1NAAAAAElFTkSuQmCC";
		private const string s_ResizeBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAdUlEQVQ4Ee1TQQ7AIAiT/x/dfzcgKakDSbztMA5aS9uom3JrDSrRomWCb/0w4ppzCUmugjCPh/mgAsyFNlHQLgGmQiM5iGAN45B0x+l6EfCDL9xA96nKHv8MjHeHYY1jEJh3RuahjYBya+wosHmsxFNIcPqcH/zfdXNcCt6HAAAAAElFTkSuQmCC";
		private const string s_MarqueeBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAANUlEQVQ4EWNgoBT8/fPnP8gMcmlK7UfYTKpJMBeTqg9TPbkmkatv1AWYITCaDkBhAktR5NIAtaFusfCIq4AAAAAASUVORK5CYII=";
		private const string s_AnimationBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAqUlEQVQ4EZ1S2xHDIAwLuSzGCCzCaoxII3LiZMe0SfWDLGzZPNLmUGvtTnoXvjFA7kH71trsnHOenPt+LaUkaNMAAUXwb9BmRz/BZOXU/JpOqDYm8KImKI8a7JrwDzd3AAPf5dd0ZgIWowiIDP2UxsBvPolvR3jSVY3DCa4D2OfSIvD5Y3luiOAAOHBFNoZuPhIEghe3iqFr8cjTjixcrVHuuMRoY2Xi9Q8An1kMaDUJnAAAAABJRU5ErkJggg==";

		private static Texture2D s_Arrow;
		public static Texture2D arrow
		{
			get
			{
				if (s_Arrow == null)
					s_Arrow = Base64ToTexture (s_ArrowBase64);
				
				return s_Arrow;
			}
		}

		private static Texture2D s_Pencil;
		public static Texture2D pencil
		{
			get
			{
				if (s_Pencil == null)
					s_Pencil = Base64ToTexture (s_PencilBase64);

				return s_Pencil;
			}
		}

		private static Texture2D s_FloodFill;
		public static Texture2D floodfill
		{
			get
			{
				if (s_FloodFill == null)
					s_FloodFill = Base64ToTexture(s_FloodFillBase64);

				return s_FloodFill;
			}
		}

		private static Texture2D s_ArrowRight;
		public static Texture2D arrowRight
		{
			get
			{
				if (s_ArrowRight == null)
					s_ArrowRight = Base64ToTexture(s_ArrowRightBase64);

				return s_ArrowRight;
			}
		}

		private static Texture2D s_Resize;
		public static Texture2D resize
		{
			get
			{
				if (s_Resize == null)
					s_Resize = Base64ToTexture(s_ResizeBase64);

				return s_Resize;
			}
		}

		private static Texture2D s_Marquee;
		public static Texture2D marquee
		{
			get
			{
				if (s_Marquee == null)
					s_Marquee = Base64ToTexture(s_MarqueeBase64);

				return s_Marquee;
			}
		}

		private static Texture2D s_Animation;
		public static Texture2D animation
		{
			get
			{
				if (s_Animation == null)
					s_Animation = Base64ToTexture(s_AnimationBase64);

				return s_Animation;
			}
		}

		private static Material s_DefaultMaterial;
		public static Material defaultMaterial
		{
			get
			{
				if (s_DefaultMaterial == null)
				{
					UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath("Assets/RagePixelDefault.mat", typeof(Material));
					if (obj != null && obj as Material != null)
					{
						s_DefaultMaterial = obj as Material;
					}
					else
					{
						s_DefaultMaterial = new Material(Shader.Find("Sprites/Default"));
						s_DefaultMaterial.SetInt ("PixelSnap", 1);
						AssetDatabase.CreateAsset(s_DefaultMaterial, "Assets/RagePixelDefault.mat");
					}
				}
				return s_DefaultMaterial;
			}
		}

		private static Texture2D Base64ToTexture (string base64)
		{
			Texture2D t = new Texture2D(1,1);
			t.hideFlags = HideFlags.HideAndDontSave;
			t.LoadImage(System.Convert.FromBase64String(base64));
			return t;
		}
	}
}
