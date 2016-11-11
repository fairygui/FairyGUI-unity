using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class PlayState
	{
		/// <summary>
		/// 是否已播放到结尾
		/// </summary>
		public bool reachEnding { get; private set; }

		/// <summary>
		/// 是否已反向播放
		/// </summary>
		public bool reversed { get; private set; }

		/// <summary>
		/// 重复次数
		/// </summary>
		public int repeatedCount { get; private set; }

		/// <summary>
		/// 是否忽略TimeScale的影响，即在TimeScale改变后依然保持原有的播放速度
		/// </summary>
		public bool ignoreTimeScale;

		int _curFrame; //当前帧
		float _lastTime;
		float _curFrameDelay; //当前帧延迟
		uint _lastUpdateFrameId;

		public PlayState()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mc"></param>
		/// <param name="context"></param>
		public void Update(MovieClip mc, UpdateContext context)
		{
			if (_lastUpdateFrameId == UpdateContext.frameId) //PlayState may be shared, only update once per frame
				return;

			_lastUpdateFrameId = UpdateContext.frameId;
			float time = Time.time;
			float elapsed = time - _lastTime;
			if (ignoreTimeScale && Time.timeScale != 0)
				elapsed /= Time.timeScale;
			_lastTime = time;

			reachEnding = false;
			_curFrameDelay += elapsed;
			float interval = mc.interval + mc.frames[_curFrame].addDelay + ((_curFrame == 0 && repeatedCount > 0) ? mc.repeatDelay : 0);
			if (_curFrameDelay < interval)
				return;

			_curFrameDelay -= interval;
			if (_curFrameDelay > mc.interval)
				_curFrameDelay = mc.interval;

			if (mc.swing)
			{
				if (reversed)
				{
					_curFrame--;
					if (_curFrame < 0)
					{
						_curFrame = Mathf.Min(1, mc.frameCount - 1);
						repeatedCount++;
						reversed = !reversed;
					}
				}
				else
				{
					_curFrame++;
					if (_curFrame > mc.frameCount - 1)
					{
						_curFrame = Mathf.Max(0, mc.frameCount - 2);
						repeatedCount++;
						reachEnding = true;
						reversed = !reversed;
					}
				}
			}
			else
			{
				_curFrame++;
				if (_curFrame > mc.frameCount - 1)
				{
					_curFrame = 0;
					repeatedCount++;
					reachEnding = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int currrentFrame
		{
			get { return _curFrame; }
			set { _curFrame = value; _curFrameDelay = 0; }
		}

		/// <summary>
		/// 
		/// </summary>
		public void Rewind()
		{
			_curFrame = 0;
			_curFrameDelay = 0;
			reversed = false;
			reachEnding = false;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Reset()
		{
			_curFrame = 0;
			_curFrameDelay = 0;
			repeatedCount = 0;
			reachEnding = false;
			reversed = false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="src"></param>
		public void Copy(PlayState src)
		{
			_curFrame = src._curFrame;
			_curFrameDelay = src._curFrameDelay;
			repeatedCount = src.repeatedCount;
			reachEnding = src.reachEnding;
			reversed = src.reversed;
		}
	}
}
