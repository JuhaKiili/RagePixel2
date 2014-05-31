using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.RagePixel2.Editor
{
	class RagePixelResources
	{
		private const string s_ArrowBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAW0lEQVQ4Ea3RMQoAIAxDURXvf2W1W4dfm6KZVMIjYGu/s04q5qByBUHAUBUJARW5AgqSAhkiATdEBiKkBBAy7ZHST/xX2p16uCAqE4CqL2YrcIEHns9+wTNGwAY88i/6p8M2mgAAAABJRU5ErkJggg==";
		private const string s_PencilBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAeElEQVQ4Ee1TWw6AMAibxvtunmSPC2OayFaiM/hpIj+jQDcKWQgOk9NqreIotyUgsdmsAzH5dQf/62N8jlnbkkEV+djkIYR3b4Xdo43Dqre1xuFHf51lY4zLLMdx04EmQN5T6j8vl9IxfK27nFgfZICsF7APAseBD1Qpy22Hqw6tAAAAAElFTkSuQmCC";

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
					s_Pencil = Base64ToTexture(s_PencilBase64);

				return s_Pencil;
			}
		}

		private static Texture2D Base64ToTexture (string base64)
		{
			Texture2D t = new Texture2D(1,1);
			t.LoadImage(System.Convert.FromBase64String(base64));
			return t;
		}
	}
}
