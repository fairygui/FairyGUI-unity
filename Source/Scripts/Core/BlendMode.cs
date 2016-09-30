using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/*关于BlendMode.Off, 这种模式相当于Blend Off指令的效果。当然，在着色器里使用Blend Off指令可以获得更高的效率，
		但因为Image着色器本身就有多个关键字，复制一个这样的着色器代价太大，所有为了节省Shader数量便增加了这样一种模式，也是可以接受的。
	*/
	
	/// <summary>
	/// 
	/// </summary>
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
		Off,
		Custom1,
		Custom2,
		Custom3
	}

	/// <summary>
	/// 
	/// </summary>
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

			//Off
			(float)UnityEngine.Rendering.BlendMode.One,
			(float)UnityEngine.Rendering.BlendMode.Zero,

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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mat"></param>
		/// <param name="blendMode"></param>
		public static void Apply(Material mat, BlendMode blendMode)
		{
			int index = (int)blendMode * 2;
			mat.SetFloat("_BlendSrcFactor", Factors[index]);
			mat.SetFloat("_BlendDstFactor", Factors[index + 1]);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="blendMode"></param>
		/// <param name="srcFactor"></param>
		/// <param name="dstFactor"></param>
		public static void Override(BlendMode blendMode, UnityEngine.Rendering.BlendMode srcFactor, UnityEngine.Rendering.BlendMode dstFactor)
		{
			int index = (int)blendMode * 2;
			Factors[index] = (float)srcFactor;
			Factors[index + 1] = (float)dstFactor;
		}
	}
}
