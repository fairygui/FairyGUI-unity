using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	public interface EMRenderTarget
	{
		int EM_sortingOrder { get; }

		void EM_BeforeUpdate();
		void EM_Update(UpdateContext context);
		void EM_Reload();
	}

	public class EMRenderSupport
	{
		public static bool orderChanged;

		static UpdateContext _updateContext;
		static List<EMRenderTarget> _targets = new List<EMRenderTarget>();

		public static bool packageListReady { get; private set; }

		public static bool hasTarget
		{
			get { return _targets.Count > 0; }
		}

		public static void Add(EMRenderTarget value)
		{
			if (!_targets.Contains(value))
				_targets.Add(value);
			orderChanged = true;
		}

		public static void Remove(EMRenderTarget value)
		{
			_targets.Remove(value);
		}

		public static void Update()
		{
			if (Application.isPlaying)
				return;

			if (_updateContext == null)
				_updateContext = new UpdateContext();

			if (orderChanged)
			{
				_targets.Sort(CompareDepth);
				orderChanged = false;
			}

			int cnt = _targets.Count;
			for (int i = 0; i < cnt; i++)
			{
				EMRenderTarget panel = _targets[i];
				panel.EM_BeforeUpdate();
			}

			if (packageListReady)
			{
				_updateContext.Begin();
				for (int i = 0; i < cnt; i++)
				{
					EMRenderTarget panel = _targets[i];
					panel.EM_Update(_updateContext);
				}
				_updateContext.End();
			}
		}

		public static void Reload()
		{
			if (Application.isPlaying)
				return;

			packageListReady = true;

			int cnt = _targets.Count;
			for (int i = 0; i < cnt; i++)
			{
				EMRenderTarget panel = _targets[i];
				panel.EM_Reload();
			}
		}

		static int CompareDepth(EMRenderTarget c1, EMRenderTarget c2)
		{
			return c1.EM_sortingOrder - c2.EM_sortingOrder;
		}
	}
}
