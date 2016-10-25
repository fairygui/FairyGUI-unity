using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class MovieClip : Image
	{
		/// <summary>
		/// 
		/// </summary>
		public struct Frame
		{
			public Rect rect;
			public float addDelay;
			public Rect uvRect;
		}

		/// <summary>
		/// 
		/// </summary>
		public float interval;

		/// <summary>
		/// 
		/// </summary>
		public bool swing;

		/// <summary>
		/// 
		/// </summary>
		public float repeatDelay;

		/// <summary>
		/// 
		/// </summary>
		public int frameCount { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public Frame[] frames { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public PlayState playState { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onPlayEnd { get; private set; }

		int _currentFrame;
		bool _playing;
		int _start;
		int _end;
		int _times;
		int _endAt;
		int _status; //0-none, 1-next loop, 2-ending, 3-ended
		bool _forceDraw;
		EventCallback0 _playEndDelegate;

		/// <summary>
		/// 
		/// </summary>
		public MovieClip()
		{
			playState = new PlayState();
			interval = 0.1f;
			_playing = true;

			onPlayEnd = new EventListener(this, "onPlayEnd");
			_playEndDelegate = () => { onPlayEnd.Call(); };

			SetPlaySettings();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="frames"></param>
		/// <param name="boundsRect"></param>
		public void SetData(NTexture texture, Frame[] frames, Rect boundsRect)
		{
			this.frames = frames;
			this.frameCount = frames.Length;
			_contentRect = boundsRect;

			if (_end == -1 || _end > frameCount - 1)
				_end = frameCount - 1;
			if (_endAt == -1 || _endAt > frameCount - 1)
				_endAt = frameCount - 1;
			playState.Rewind();

			graphics.texture = texture;
			OnSizeChanged(true, true);
			InvalidateBatchingState();
			_forceDraw = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			this.frameCount = 0;
			graphics.texture = null;
			graphics.ClearMesh();
		}

		/// <summary>
		/// 
		/// </summary>
		public bool playing
		{
			get { return _playing; }
			set { _playing = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int currentFrame
		{
			get { return _currentFrame; }
			set
			{
				if (_currentFrame != value)
				{
					_currentFrame = value;
					playState.currrentFrame = value;
					if (frameCount > 0)
						_forceDraw = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void SetPlaySettings()
		{
			SetPlaySettings(0, -1, 0, -1);
		}

		/// <summary>
		/// 从start帧开始，播放到end帧（-1表示结尾），重复times次（0表示无限循环），循环结束后，停止在endAt帧（-1表示参数end）
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="times"></param>
		/// <param name="endAt"></param>
		public void SetPlaySettings(int start, int end, int times, int endAt)
		{
			_start = start;
			_end = end;
			if (_end == -1 || _end > frameCount - 1)
				_end = frameCount - 1;
			_times = times;
			_endAt = endAt;
			if (_endAt == -1)
				_endAt = _end;
			this.currentFrame = start;
			_status = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		public override void Update(UpdateContext context)
		{
			if (_playing && frameCount != 0 && _status != 3)
			{
				playState.Update(this, context);
				if (_forceDraw || _currentFrame != playState.currrentFrame)
				{
					if (_status == 1)
					{
						_currentFrame = _start;
						playState.currrentFrame = _currentFrame;
						_status = 0;
					}
					else if (_status == 2)
					{
						_currentFrame = _endAt;
						playState.currrentFrame = _currentFrame;
						_status = 3;

						UpdateContext.OnEnd += _playEndDelegate;
					}
					else
					{
						_currentFrame = playState.currrentFrame;
						if (_currentFrame == _end)
						{
							if (_times > 0)
							{
								_times--;
								if (_times == 0)
									_status = 2;
								else
									_status = 1;
							}
						}
					}
					DrawFrame();
				}
			}
			else if(_forceDraw)
				DrawFrame();

			base.Update(context);
		}

		void DrawFrame()
		{
			_forceDraw = false;

			if (_currentFrame >= frames.Length)
				graphics.ClearMesh();
			else
			{
				Frame frame = frames[_currentFrame];

				if (frame.rect.width == 0)
					graphics.ClearMesh();
				else
				{
					Rect uvRect = frame.uvRect;
					if (_flip != FlipType.None)
						ToolSet.FlipRect(ref uvRect, _flip);

					graphics.SetOneQuadMesh(frame.rect, uvRect, _color);
				}
			}
		}

		protected override void Rebuild()
		{
			if (_texture != null)
				base.Rebuild();
			else if (frameCount > 0)
			{
				_requireUpdateMesh = false;
				DrawFrame();
			}
		}
	}
}
