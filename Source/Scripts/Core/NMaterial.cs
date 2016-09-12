using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class NMaterial
	{
		public uint frameId;
		public uint clipId;
		public bool stencilSet;
		public BlendMode blendMode;
		public bool combined;

		public Material material;

		public NMaterial(Shader shader)
		{
			material = new Material(shader);
		}
	}
}
