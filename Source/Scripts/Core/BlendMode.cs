using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	public enum BlendMode
	{
		Normal,
		None,
		Add,
		Multiply,
		Screen,
		Erase,
		Mask,
		Below,
		Custom1,
		Custom2,
		Custom3
	}

	public class BlendModeUtils
	{
		//Source指的是被计算的颜色，Destination是已经在屏幕上的颜色。
		//混合结果=Source * factor1 + Destination * factor2
		static float[] Factors = new float[] {
			//Normal
			(float)UnityEngine.Rendering.BlendMode.SrcAlpha,
			(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha,

			//None
			(float)UnityEngine.Rendering.BlendMode.One,
			(float)UnityEngine.Rendering.BlendMode.One,

			//Add
			(float)UnityEngine.Rendering.BlendMode.SrcAlpha,
			(float)UnityEngine.Rendering.BlendMode.One,

			//Multiply
			(float)UnityEngine.Rendering.BlendMode.DstColor,
			(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha,

			//Screen
			(float)UnityEngine.Rendering.BlendMode.One,
			(float)UnityEngine.Rendering.BlendMode.OneMinusSrcColor,

			//Erase
			(float)UnityEngine.Rendering.BlendMode.Zero,
			(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha,

			//Mask
			(float)UnityEngine.Rendering.BlendMode.Zero,
			(float)UnityEngine.Rendering.BlendMode.SrcAlpha,

			//Below
			(float)UnityEngine.Rendering.BlendMode.OneMinusDstAlpha,
			(float)UnityEngine.Rendering.BlendMode.DstAlpha,

			//Custom1
			(float)UnityEngine.Rendering.BlendMode.SrcAlpha,
			(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha,

			//Custom2
			(float)UnityEngine.Rendering.BlendMode.SrcAlpha,
			(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha,

			//Custom3
			(float)UnityEngine.Rendering.BlendMode.SrcAlpha,
			(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha
		};

		public static void Apply(Material mat, BlendMode blendMode)
		{
			int index = (int)blendMode * 2;
			mat.SetFloat("_BlendSrcFactor", Factors[index]);
			mat.SetFloat("_BlendDstFactor", Factors[index + 1]);
		}

		public static void Override(BlendMode blendMode, UnityEngine.Rendering.BlendMode srcFactor, UnityEngine.Rendering.BlendMode dstFactor)
		{
			int index = (int)blendMode * 2;
			Factors[index] = (float)srcFactor;
			Factors[index + 1] = (float)dstFactor;
		}
	}
}
