using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class ShaderConfig
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public delegate Shader GetFunction(string name);

		/// <summary>
		/// 
		/// </summary>
		public static GetFunction Get = Shader.Find;

		/// <summary>
		/// 
		/// </summary>
		public static string imageShader = "FairyGUI/Image";

		/// <summary>
		/// 
		/// </summary>
		public static string textShader = "FairyGUI/Text";

		/// <summary>
		/// 
		/// </summary>
		public static string bmFontShader = "FairyGUI/BMFont";

		/// <summary>
		/// 
		/// </summary>
		public class PropertyIDs
		{
			public int _ClipBox;
			public int _ClipSoftness;
			public int _AlphaTex;
			public int _StencilComp;
			public int _Stencil;
			public int _StencilOp;
			public int _StencilReadMask;
			public int _ColorMask;
			public int _ColorMatrix;
			public int _ColorOffset;
			public int _BlendSrcFactor;
			public int _BlendDstFactor;
			public int _ColorOption;
		}
		internal static PropertyIDs _properyIDs;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Shader GetShader(string name)
		{
			Shader shader = Get(name);
			if (shader == null)
			{
				Debug.LogWarning("FairyGUI: shader not found: " + name);
				shader = Shader.Find("UI/Default");
			}
			shader.hideFlags = DisplayOptions.hideFlags;

			if (_properyIDs == null)
				InitPropertyIDs();

			return shader;
		}

		/// <summary>
		/// 
		/// </summary>
		public static PropertyIDs propertyIDs
		{
			get
			{
				if (_properyIDs == null)
					InitPropertyIDs();

				return _properyIDs;
			}
		}

		static void InitPropertyIDs()
		{
			_properyIDs = new PropertyIDs();
			_properyIDs._ClipBox = Shader.PropertyToID("_ClipBox");
			_properyIDs._ClipSoftness = Shader.PropertyToID("_ClipSoftness");
			_properyIDs._AlphaTex = Shader.PropertyToID("_AlphaTex");
			_properyIDs._StencilComp = Shader.PropertyToID("_StencilComp");
			_properyIDs._Stencil = Shader.PropertyToID("_Stencil");
			_properyIDs._StencilOp = Shader.PropertyToID("_StencilOp");
			_properyIDs._StencilReadMask = Shader.PropertyToID("_StencilReadMask");
			_properyIDs._ColorMask = Shader.PropertyToID("_ColorMask");
			_properyIDs._ColorMatrix = Shader.PropertyToID("_ColorMatrix");
			_properyIDs._ColorOffset = Shader.PropertyToID("_ColorOffset");
			_properyIDs._BlendSrcFactor = Shader.PropertyToID("_BlendSrcFactor");
			_properyIDs._BlendDstFactor = Shader.PropertyToID("_BlendDstFactor");
			_properyIDs._ColorOption = Shader.PropertyToID("_ColorOption");
		}
	}
}
