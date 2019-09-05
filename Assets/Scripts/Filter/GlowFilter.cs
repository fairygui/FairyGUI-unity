using UnityEngine;

namespace FairyGUI
{
	/*
	 * 消耗非常高，注意要使用cacheAsBitmap，否则drawcall让你吐血
	 */
    public class GlowFilter: IFilter
	{
		/// <summary>
		/// 
		/// </summary>
		public Color color = Color.black;
		
		public float samplerArea = 1f;


		public int downSample = 2;

		public int quality= 1;
		
		public float strength = 1.0f;


		private Material _material = null;

		private Material material
		{
			get
			{
				if (_material == null)
					_material = new Material(ShaderConfig.GetShader("FairyGUI/GlowFilter"));
				return _material;
			}
		}


		DisplayObject _target;
		public DisplayObject target
		{
			get { return _target; }
			set
			{
				_target = value;
				Margin margin= new Margin();
				margin.left = 50;
				margin.right = 50;
				margin.top = 50;
				margin.bottom = 50;
				_target.EnterPaintingMode(1,margin );
				_target.onPaint += OnRenderImage;
			}
		}

		public void Dispose()
		{
			_target.LeavePaintingMode(1);
			_target.onPaint -= OnRenderImage;
			_target = null;

			if (Application.isPlaying)
			{
				Material.Destroy(material);
			}
			else
			{
				Material.DestroyImmediate(material);
			}
		}

		public void Update()
		{
		}


		void OnRenderImage()
		{

			RenderTexture sourceTexture = (RenderTexture)_target.paintingGraphics.texture.nativeTexture;

			//对RT进行Blur处理  
			RenderTexture temp1 =
				RenderTexture.GetTemporary(sourceTexture.width >> downSample, sourceTexture.height >> downSample, 0);
			RenderTexture temp2 =
				RenderTexture.GetTemporary(sourceTexture.width >> downSample, sourceTexture.height >> downSample, 0);

			//高斯模糊，两次模糊，横向纵向，使用pass0进行高斯模糊  
			material.SetVector("_offsets", new Vector4(0, samplerArea, 0, 0));
			Graphics.Blit(sourceTexture, temp1, material, 0);
			material.SetVector("_offsets", new Vector4(samplerArea, 0, 0, 0));
			Graphics.Blit(temp1, temp2, material, 0);

			//如果有叠加再进行迭代模糊处理  
			for (int i = 0; i < quality; i++)
			{
				material.SetVector("_offsets", new Vector4(0, samplerArea, 0, 0));
				Graphics.Blit(temp2, temp1, material, 0);
				material.SetVector("_offsets", new Vector4(samplerArea, 0, 0, 0));
				Graphics.Blit(temp1, temp2, material, 0);
			}

			//用模糊图和原始图计算出轮廓图
			material.SetTexture("_BlurTex", temp1);
			Graphics.Blit(sourceTexture, temp1, material, 1);


			//轮廓图和场景图叠加  
			material.SetTexture("_BlurTex", temp1);
			material.SetFloat("_OutlineStrength", strength);
			material.SetColor("_OutlineColor", color);
			Graphics.Blit(sourceTexture, sourceTexture, material, 2);

			
			RenderTexture.ReleaseTemporary(temp1);
			RenderTexture.ReleaseTemporary(temp2);
		}

	}
}