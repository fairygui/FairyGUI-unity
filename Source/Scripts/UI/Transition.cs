using System;
using System.Collections.Generic;
using FairyGUI.Utils;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public delegate void PlayCompleteCallback();

	/// <summary>
	/// 
	/// </summary>
	public delegate void TransitionHook();

	/// <summary>
	/// 
	/// </summary>
	public class Transition
	{
		/// <summary>
		/// 动效的名称。在编辑器里设定。
		/// </summary>
		public string name { get; private set; }

		/// <summary>
		/// 自动播放的次数。
		/// </summary>
		public int autoPlayRepeat;

		/// <summary>
		/// 自动播放的延迟时间。
		/// </summary>
		public float autoPlayDelay;

		/// <summary>
		/// 当你启动了自动合批，动效里有涉及到XY、大小、旋转等的改变，如果你观察到元件的显示深度在播放过程中有错误，可以开启这个选项。
		/// </summary>
		public bool invalidateBatchingEveryFrame;

		GComponent _owner;
		List<TransitionItem> _items;
		int _totalTimes;
		int _totalTasks;
		bool _playing;
		float _ownerBaseX;
		float _ownerBaseY;
		PlayCompleteCallback _onComplete;
		int _options;
		bool _reversed;
		float _maxTime;
		bool _autoPlay;
		float _timeScale;

		const int FRAME_RATE = 24;

		const int OPTION_IGNORE_DISPLAY_CONTROLLER = 1;
		const int OPTION_AUTO_STOP_DISABLED = 2;
		const int OPTION_AUTO_STOP_AT_END = 4;

		public Transition(GComponent owner)
		{
			_owner = owner;
			_items = new List<TransitionItem>();
			autoPlayRepeat = 1;
			_timeScale = 1;
		}

		/// <summary>
		/// 动效是否自动播放。
		/// </summary>
		public bool autoPlay
		{
			get { return _autoPlay; }
			set
			{
				if (_autoPlay != value)
				{
					_autoPlay = value;
					if (_autoPlay)
					{
						if (_owner.onStage)
							Play(autoPlayRepeat, autoPlayDelay, null);
					}
					else
					{
						if (!_owner.onStage)
							Stop(false, true);
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Play()
		{
			Play(1, 0, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="onComplete"></param>
		public void Play(PlayCompleteCallback onComplete)
		{
			Play(1, 0, onComplete);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="times"></param>
		/// <param name="delay"></param>
		/// <param name="onComplete"></param>
		public void Play(int times, float delay, PlayCompleteCallback onComplete)
		{
			_Play(times, delay, onComplete, false);
		}

		/// <summary>
		/// 
		/// </summary>
		public void PlayReverse()
		{
			PlayReverse(1, 0, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="onComplete"></param>
		public void PlayReverse(PlayCompleteCallback onComplete)
		{
			PlayReverse(1, 0, onComplete);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="times"></param>
		/// <param name="delay"></param>
		/// <param name="onComplete"></param>
		public void PlayReverse(int times, float delay, PlayCompleteCallback onComplete)
		{
			_Play(times, delay, onComplete, true);
		}

		public void ChangeRepeat(int value)
		{
			_totalTimes = value;
		}

		void _Play(int times, float delay, PlayCompleteCallback onComplete, bool reverse)
		{
			Stop(true, true);

			_totalTimes = times;
			_reversed = reverse;

			InternalPlay(delay);
			_playing = _totalTasks > 0;
			if (_playing)
			{
				_onComplete = onComplete;

				if ((_options & OPTION_IGNORE_DISPLAY_CONTROLLER) != 0)
				{
					int cnt = _items.Count;
					for (int i = 0; i < cnt; i++)
					{
						TransitionItem item = _items[i];
						if (item.target != null && item.target != _owner)
							item.displayLockToken = item.target.AddDisplayLock();
					}
				}
			}
			else if (onComplete != null)
				onComplete();
		}

		/// <summary>
		/// 
		/// </summary>
		public void Stop()
		{
			Stop(true, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="setToComplete"></param>
		/// <param name="processCallback"></param>
		public void Stop(bool setToComplete, bool processCallback)
		{
			if (_playing)
			{
				_playing = false;
				_totalTasks = 0;
				_totalTimes = 0;
				PlayCompleteCallback func = _onComplete;
				_onComplete = null;

				int cnt = _items.Count;
				if (_reversed)
				{
					for (int i = cnt - 1; i >= 0; i--)
					{
						TransitionItem item = _items[i];
						if (item.target == null)
							continue;

						StopItem(item, setToComplete);
					}
				}
				else
				{
					for (int i = 0; i < cnt; i++)
					{
						TransitionItem item = _items[i];
						if (item.target == null)
							continue;

						StopItem(item, setToComplete);
					}
				}
				if (processCallback && func != null)
					func();
			}
		}

		void StopItem(TransitionItem item, bool setToComplete)
		{
			if (item.displayLockToken != 0)
			{
				item.target.ReleaseDisplayLock(item.displayLockToken);
				item.displayLockToken = 0;
			}

			if (item.type == TransitionActionType.ColorFilter)
				((TransitionItem_ColorFilter)item).ClearFilter();

			if (item.completed)
				return;

			if (item.tweener != null)
			{
				item.tweener.Kill();
				item.tweener = null;
			}

			if (item.type == TransitionActionType.Transition)
			{
				Transition trans = ((GComponent)item.target).GetTransition(((TransitionItem_Transition)item).transName);
				if (trans != null)
					trans.Stop(setToComplete, false);
			}
			else if (item.type == TransitionActionType.Shake)
			{
				((TransitionItem_Shake)item).Stop(true);
			}
			else
			{
				if (setToComplete)
				{
					if (item.tween)
					{
						if (!item.yoyo || item.repeat % 2 == 0)
							ApplyValue(item, _reversed ? item.startValue : item.endValue);
						else
							ApplyValue(item, _reversed ? item.endValue : item.startValue);
					}
					else if (item.type != TransitionActionType.Sound)
						ApplyValue(item, item.value);
				}
			}
		}

		public void Dispose()
		{
			int cnt = _items.Count;
			for (int i = 0; i < cnt; i++)
			{
				TransitionItem item = _items[i];
				if (item.tweener != null)
				{
					item.tweener.Kill();
					item.tweener = null;
				}
				else if (_playing && item.type == TransitionActionType.Shake)
				{
					((TransitionItem_Shake)item).Stop(false);
				}

				item.target = null;
				item.hook = null;
				item.hook2 = null;
			}

			_items.Clear();
			_playing = false;
			_onComplete = null;
		}

		/// <summary>
		/// 
		/// </summary>
		public bool playing
		{
			get { return _playing; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="label"></param>
		/// <param name="aParams"></param>
		public void SetValue(string label, params object[] aParams)
		{
			int cnt = _items.Count;
			TransitionValue value;
			for (int i = 0; i < cnt; i++)
			{
				TransitionItem item = _items[i];
				if (item.label == label)
				{
					if (item.tween)
						value = item.startValue;
					else
						value = item.value;
				}
				else if (item.label2 == label)
				{
					value = item.endValue;
				}
				else
					continue;

				switch (item.type)
				{
					case TransitionActionType.XY:
					case TransitionActionType.Size:
					case TransitionActionType.Pivot:
					case TransitionActionType.Scale:
					case TransitionActionType.Skew:
						value.b1 = true;
						value.b2 = true;
						value.f1 = Convert.ToSingle(aParams[0]);
						value.f2 = Convert.ToSingle(aParams[1]);
						break;

					case TransitionActionType.Alpha:
						value.f1 = Convert.ToSingle(aParams[0]);
						break;

					case TransitionActionType.Rotation:
						value.f1 = Convert.ToInt32(aParams[0]);
						break;

					case TransitionActionType.Color:
						value.AsColor = (Color)aParams[0];
						break;

					case TransitionActionType.Animation:
						((TransitionItem_Animation)item).frame = Convert.ToInt32(aParams[0]);
						if (aParams.Length > 1)
							((TransitionItem_Animation)item).playing = Convert.ToBoolean(aParams[1]);
						break;

					case TransitionActionType.Visible:
						((TransitionItem_Visible)item).visible = Convert.ToBoolean(aParams[0]);
						break;

					case TransitionActionType.Sound:
						((TransitionItem_Sound)item).sound = (string)aParams[0];
						if (aParams.Length > 1)
							((TransitionItem_Sound)item).volume = Convert.ToSingle(aParams[1]);
						break;

					case TransitionActionType.Transition:
						((TransitionItem_Transition)item).transName = (string)aParams[0];
						if (aParams.Length > 1)
							((TransitionItem_Transition)item).transRepeat = Convert.ToInt32(aParams[1]);
						break;

					case TransitionActionType.Shake:
						((TransitionItem_Shake)item).shakeAmplitude = Convert.ToSingle(aParams[0]);
						if (aParams.Length > 1)
							((TransitionItem_Shake)item).shakePeriod = Convert.ToSingle(aParams[1]);
						break;

					case TransitionActionType.ColorFilter:
						value.f1 = Convert.ToSingle(aParams[0]);
						value.f2 = Convert.ToSingle(aParams[1]);
						value.f3 = Convert.ToSingle(aParams[2]);
						value.f4 = Convert.ToSingle(aParams[3]);
						break;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="label"></param>
		/// <param name="callback"></param>
		public void SetHook(string label, TransitionHook callback)
		{
			int cnt = _items.Count;
			for (int i = 0; i < cnt; i++)
			{
				TransitionItem item = _items[i];
				if (item.label == label)
				{
					item.hook = callback;
					break;
				}
				else if (item.label2 == label)
				{
					item.hook2 = callback;
					break;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void ClearHooks()
		{
			int cnt = _items.Count;
			for (int i = 0; i < cnt; i++)
			{
				TransitionItem item = _items[i];
				item.hook = null;
				item.hook2 = null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="label"></param>
		/// <param name="newTarget"></param>
		public void SetTarget(string label, GObject newTarget)
		{
			int cnt = _items.Count;
			for (int i = 0; i < cnt; i++)
			{
				TransitionItem item = _items[i];
				if (item.label == label)
					item.targetId = newTarget.id;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="label"></param>
		/// <param name="value"></param>
		public void SetDuration(string label, float value)
		{
			int cnt = _items.Count;
			for (int i = 0; i < cnt; i++)
			{
				TransitionItem item = _items[i];
				if (item.tween && item.label == label)
					item.duration = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float timeScale
		{
			get { return _timeScale; }
			set
			{
				_timeScale = value;

				if (_playing)
				{
					int cnt = _items.Count;
					for (int i = 0; i < cnt; i++)
					{
						TransitionItem item = _items[i];
						if (item.tweener != null)
							item.tweener.timeScale = _timeScale;
					}
				}
			}
		}

		internal void UpdateFromRelations(string targetId, float dx, float dy)
		{
			int cnt = _items.Count;
			if (cnt == 0)
				return;

			for (int i = 0; i < cnt; i++)
			{
				TransitionItem item = _items[i];
				if (item.type == TransitionActionType.XY && item.targetId == targetId)
				{
					if (item.tween)
					{
						item.startValue.f1 += dx;
						item.startValue.f2 += dy;
						item.endValue.f1 += dx;
						item.endValue.f2 += dy;
					}
					else
					{
						item.value.f1 += dx;
						item.value.f2 += dy;
					}
				}
			}
		}

		internal void OnOwnerRemovedFromStage()
		{
			if ((_options & OPTION_AUTO_STOP_DISABLED) == 0)
				Stop((_options & OPTION_AUTO_STOP_AT_END) != 0 ? true : false, false);
		}

		void InternalPlay(float delay)
		{
			_ownerBaseX = _owner.x;
			_ownerBaseY = _owner.y;

			_totalTasks = 0;
			int cnt = _items.Count;
			for (int i = 0; i < cnt; i++)
			{
				TransitionItem item = _items[i];
				if (item.targetId.Length > 0)
					item.target = _owner.GetChildById(item.targetId);
				else
					item.target = _owner;
				if (item.target == null)
					continue;

				if (item.tween)
				{
					float startTime = delay;
					if (_reversed)
						startTime += (_maxTime - item.time - item.duration);
					else
						startTime += item.time;
					if (startTime > 0 && (item.type == TransitionActionType.XY || item.type == TransitionActionType.Size))
					{
						_totalTasks++;
						item.completed = false;
						item.tweener = DOVirtual.DelayedCall(startTime, item.delayedCallDelegate, true);
						item.tweener.SetRecyclable();
						if (_timeScale != 1)
							item.tweener.timeScale = _timeScale;
					}
					else
						StartTween(item, startTime);
				}
				else
				{
					float startTime = delay;
					if (_reversed)
						startTime += (_maxTime - item.time);
					else
						startTime += item.time;
					if (startTime == 0)
						ApplyValue(item, item.value);
					else
					{
						item.completed = false;
						_totalTasks++;
						item.tweener = DOVirtual.DelayedCall(startTime, item.delayedCallDelegate, true);
						item.tweener.SetRecyclable();
						if (_timeScale != 1)
							item.tweener.timeScale = _timeScale;
					}
				}
			}
		}

		void StartTween(TransitionItem item, float delay)
		{
			TransitionValue startValue;
			TransitionValue endValue;

			if (_reversed)
			{
				startValue = item.endValue;
				endValue = item.startValue;
			}
			else
			{
				startValue = item.startValue;
				endValue = item.endValue;
			}

			switch (item.type)
			{
				case TransitionActionType.XY:
				case TransitionActionType.Size:
					{
						if (item.type == TransitionActionType.XY)
						{
							if (item.target == _owner)
							{
								if (!startValue.b1)
									startValue.f1 = 0;
								if (!startValue.b2)
									startValue.f2 = 0;
							}
							else
							{
								if (!startValue.b1)
									startValue.f1 = item.target.x;
								if (!startValue.b2)
									startValue.f2 = item.target.y;
							}
						}
						else
						{
							if (!startValue.b1)
								startValue.f1 = item.target.width;
							if (!startValue.b2)
								startValue.f2 = item.target.height;
						}

						item.value.f1 = startValue.f1;
						item.value.f2 = startValue.f2;

						if (!endValue.b1)
							endValue.f1 = item.value.f1;
						if (!endValue.b2)
							endValue.f2 = item.value.f2;

						item.value.b1 = startValue.b1 || endValue.b1;
						item.value.b2 = startValue.b2 || endValue.b2;

						item.tweener = DOTween.To(((TransitionItem_Vector2)item).getter, ((TransitionItem_Vector2)item).setter,
							endValue.AsVec2, item.duration);
					}
					break;

				case TransitionActionType.Scale:
				case TransitionActionType.Skew:
					{
						item.value.f1 = startValue.f1;
						item.value.f2 = startValue.f2;
						item.tweener = DOTween.To(((TransitionItem_Vector2)item).getter, ((TransitionItem_Vector2)item).setter,
							endValue.AsVec2, item.duration);
						break;
					}

				case TransitionActionType.Alpha:
				case TransitionActionType.Rotation:
					{
						item.value.f1 = startValue.f1;
						item.tweener = DOTween.To(((TransitionItem_Float)item).getter, ((TransitionItem_Float)item).setter,
							endValue.f1, item.duration);
						break;
					}

				case TransitionActionType.Color:
					{
						item.value.Copy(startValue);
						item.tweener = DOTween.To(((TransitionItem_Color)item).getter, ((TransitionItem_Color)item).setter,
							endValue.AsColor, item.duration);
						break;
					}

				case TransitionActionType.ColorFilter:
					{
						item.value.Copy(startValue);
						item.tweener = DOTween.To(((TransitionItem_ColorFilter)item).getter, ((TransitionItem_ColorFilter)item).setter,
							endValue.AsVec4, item.duration);
						break;
					}
			}

			item.tweener.SetEase(item.easeType)
				.SetUpdate(true)
				.SetRecyclable()
				.OnStart(item.tweenStartDelegate)
				.OnUpdate(item.tweenUpdateDelegate)
				.OnComplete(item.tweenCompleteDelegate);

			if (delay > 0)
				item.tweener.SetDelay(delay);
			else
				ApplyValue(item, item.value);
			if (item.repeat != 0)
				item.tweener.SetLoops(item.repeat == -1 ? int.MaxValue : (item.repeat + 1), item.yoyo ? LoopType.Yoyo : LoopType.Restart);
			if (_timeScale != 1)
				item.tweener.timeScale = _timeScale;

			_totalTasks++;
			item.completed = false;
		}

		internal void OnDelayedCall(TransitionItem item)
		{
			if (item.tween)
			{
				item.tweener = null;
				_totalTasks--;

				StartTween(item, 0);
			}
			else
			{
				item.tweener = null;
				item.completed = true;
				_totalTasks--;

				ApplyValue(item, item.value);
				if (item.hook != null)
					item.hook();

				CheckAllComplete();
			}
		}

		internal void OnTweenComplete(TransitionItem item)
		{
			item.tweener = null;
			item.completed = true;
			_totalTasks--;

			if (item.hook2 != null)
				item.hook2();

			if (item.type == TransitionActionType.XY || item.type == TransitionActionType.Size
				|| item.type == TransitionActionType.Scale)
				_owner.InvalidateBatchingState(true);

			CheckAllComplete();
		}

		internal void OnInnerActionComplete(TransitionItem item)
		{
			_totalTasks--;
			item.completed = true;

			CheckAllComplete();
		}

		void CheckAllComplete()
		{
			if (_playing && _totalTasks == 0)
			{
				if (_totalTimes < 0)
				{
					InternalPlay(0);
				}
				else
				{
					_totalTimes--;
					if (_totalTimes > 0)
						InternalPlay(0);
					else
					{
						_playing = false;

						int cnt = _items.Count;
						for (int i = 0; i < cnt; i++)
						{
							TransitionItem item = _items[i];
							if (item.target != null)
							{
								if (item.displayLockToken != 0)
								{
									item.target.ReleaseDisplayLock(item.displayLockToken);
									item.displayLockToken = 0;
								}

								if (item.type == TransitionActionType.ColorFilter)
									((TransitionItem_ColorFilter)item).ClearFilter();
							}
						}

						if (_onComplete != null)
						{
							PlayCompleteCallback func = _onComplete;
							_onComplete = null;
							func();
						}
					}
				}
			}
		}

		internal void ApplyValue(TransitionItem item, TransitionValue value)
		{
			item.target._gearLocked = true;

			switch (item.type)
			{
				case TransitionActionType.XY:
					if (item.target == _owner)
					{
						float f1, f2;
						if (!value.b1)
							f1 = item.target.x;
						else
							f1 = value.f1 + _ownerBaseX;
						if (!value.b2)
							f2 = item.target.y;
						else
							f2 = value.f2 + _ownerBaseY;
						item.target.SetXY(f1, f2);
					}
					else
					{
						if (!value.b1)
							value.f1 = item.target.x;
						if (!value.b2)
							value.f2 = item.target.y;
						item.target.SetXY(value.f1, value.f2);
					}
					if (invalidateBatchingEveryFrame)
						_owner.InvalidateBatchingState(true);
					break;

				case TransitionActionType.Size:
					if (!value.b1)
						value.f1 = item.target.width;
					if (!value.b2)
						value.f2 = item.target.height;
					item.target.SetSize(value.f1, value.f2);
					if (invalidateBatchingEveryFrame)
						_owner.InvalidateBatchingState(true);
					break;

				case TransitionActionType.Pivot:
					item.target.SetPivot(value.f1, value.f2);
					if (invalidateBatchingEveryFrame)
						_owner.InvalidateBatchingState(true);
					break;

				case TransitionActionType.Alpha:
					item.target.alpha = value.f1;
					break;

				case TransitionActionType.Rotation:
					item.target.rotation = value.f1;
					if (invalidateBatchingEveryFrame)
						_owner.InvalidateBatchingState(true);
					break;

				case TransitionActionType.Scale:
					item.target.SetScale(value.f1, value.f2);
					if (invalidateBatchingEveryFrame)
						_owner.InvalidateBatchingState(true);
					break;

				case TransitionActionType.Skew:
					item.target.skew = value.AsVec2;
					if (invalidateBatchingEveryFrame)
						_owner.InvalidateBatchingState(true);
					break;

				case TransitionActionType.Color:
					((IColorGear)item.target).color = value.AsColor;
					break;

				case TransitionActionType.Animation:
					if (((TransitionItem_Animation)item).frame >= 0)
						((IAnimationGear)item.target).frame = ((TransitionItem_Animation)item).frame;
					((IAnimationGear)item.target).playing = ((TransitionItem_Animation)item).playing;
					break;

				case TransitionActionType.Visible:
					item.target.visible = ((TransitionItem_Visible)item).visible;
					break;

				case TransitionActionType.Transition:
					Transition trans = ((GComponent)item.target).GetTransition(((TransitionItem_Transition)item).transName);
					if (trans != null)
					{
						int tranRepeat = ((TransitionItem_Transition)item).transRepeat;
						if (tranRepeat == 0)
							trans.Stop(false, true);
						else if (trans.playing)
							trans._totalTimes = tranRepeat;
						else
						{
							item.completed = false;
							_totalTasks++;
							if (_reversed)
								trans.PlayReverse(tranRepeat, 0, ((TransitionItem_Transition)item).playCompleteDelegate);
							else
								trans.Play(tranRepeat, 0, ((TransitionItem_Transition)item).playCompleteDelegate);
							if (_timeScale != 1)
								trans.timeScale = _timeScale;
						}
					}
					break;

				case TransitionActionType.Sound:
					((TransitionItem_Sound)item).Play();
					break;

				case TransitionActionType.Shake:
					((TransitionItem_Shake)item).Start();
					_totalTasks++;
					item.completed = false;
					break;

				case TransitionActionType.ColorFilter:
					((TransitionItem_ColorFilter)item).SetFilter();
					break;
			}

			item.target._gearLocked = false;
		}

		public void Setup(XML xml)
		{
			this.name = xml.GetAttribute("name");
			_options = xml.GetAttributeInt("options");
			_autoPlay = xml.GetAttributeBool("autoPlay");
			if (_autoPlay)
			{
				this.autoPlayRepeat = xml.GetAttributeInt("autoPlayRepeat", 1);
				this.autoPlayDelay = xml.GetAttributeFloat("autoPlayDelay");
			}

			XMLList.Enumerator et = xml.GetEnumerator("item");
			while (et.MoveNext())
			{
				XML cxml = et.Current;
				TransitionItem item = TransitionItem.createInstance(FieldTypes.ParseTransitionActionType(cxml.GetAttribute("type")));
				_items.Add(item);

				item.time = (float)cxml.GetAttributeInt("time") / (float)FRAME_RATE;
				item.targetId = cxml.GetAttribute("target", string.Empty);
				item.tween = cxml.GetAttributeBool("tween");
				item.label = cxml.GetAttribute("label");
				item.Setup(this);

				if (item.tween)
				{
					item.duration = (float)cxml.GetAttributeInt("duration") / FRAME_RATE;
					if (item.time + item.duration > _maxTime)
						_maxTime = item.time + item.duration;

					string ease = cxml.GetAttribute("ease");
					if (ease != null)
						item.easeType = FieldTypes.ParseEaseType(ease);

					item.repeat = cxml.GetAttributeInt("repeat");
					item.yoyo = cxml.GetAttributeBool("yoyo");
					item.label2 = cxml.GetAttribute("label2");

					string v = cxml.GetAttribute("endValue");
					if (v != null)
					{
						DecodeValue(item, cxml.GetAttribute("startValue", string.Empty), item.startValue);
						DecodeValue(item, v, item.endValue);
					}
					else
					{
						item.tween = false;
						DecodeValue(item, cxml.GetAttribute("startValue", string.Empty), item.value);
					}
				}
				else
				{
					if (item.time > _maxTime)
						_maxTime = item.time;
					DecodeValue(item, cxml.GetAttribute("value", string.Empty), item.value);
				}
			}
		}

		void DecodeValue(TransitionItem item, string str, TransitionValue value)
		{
			string[] arr;
			switch (item.type)
			{
				case TransitionActionType.XY:
				case TransitionActionType.Size:
				case TransitionActionType.Pivot:
				case TransitionActionType.Skew:
					arr = str.Split(',');
					if (arr[0] == "-")
					{
						value.b1 = false;
					}
					else
					{
						value.f1 = float.Parse(arr[0]);
						value.b1 = true;
					}
					if (arr[1] == "-")
					{
						value.b2 = false;
					}
					else
					{
						value.f2 = float.Parse(arr[1]);
						value.b2 = true;
					}
					break;

				case TransitionActionType.Alpha:
					value.f1 = float.Parse(str);
					break;

				case TransitionActionType.Rotation:
					value.f1 = int.Parse(str);
					break;

				case TransitionActionType.Scale:
					arr = str.Split(',');
					value.f1 = float.Parse(arr[0]);
					value.f2 = float.Parse(arr[1]);
					break;

				case TransitionActionType.Color:
					value.AsColor = ToolSet.ConvertFromHtmlColor(str);
					break;

				case TransitionActionType.Animation:
					arr = str.Split(',');
					if (arr[0] == "-")
						((TransitionItem_Animation)item).frame = -1;
					else
						((TransitionItem_Animation)item).frame = int.Parse(arr[0]);
					((TransitionItem_Animation)item).playing = arr[1] == "p";
					break;

				case TransitionActionType.Visible:
					((TransitionItem_Visible)item).visible = str == "true";
					break;

				case TransitionActionType.Sound:
					arr = str.Split(',');
					((TransitionItem_Sound)item).sound = arr[0];
					if (arr.Length > 1)
					{
						int intv = int.Parse(arr[1]);
						if (intv == 100 || intv == 0)
							((TransitionItem_Sound)item).volume = 1;
						else
							((TransitionItem_Sound)item).volume = (float)intv / 100f;
					}
					else
						((TransitionItem_Sound)item).volume = 1;
					break;

				case TransitionActionType.Transition:
					arr = str.Split(',');
					((TransitionItem_Transition)item).transName = arr[0];
					if (arr.Length > 1)
						((TransitionItem_Transition)item).transRepeat = int.Parse(arr[1]);
					else
						((TransitionItem_Transition)item).transRepeat = 1;
					break;

				case TransitionActionType.Shake:
					arr = str.Split(',');
					((TransitionItem_Shake)item).shakeAmplitude = float.Parse(arr[0]);
					((TransitionItem_Shake)item).shakePeriod = float.Parse(arr[1]);
					break;

				case TransitionActionType.ColorFilter:
					arr = str.Split(',');
					value.f1 = float.Parse(arr[0]);
					value.f2 = float.Parse(arr[1]);
					value.f3 = float.Parse(arr[2]);
					value.f4 = float.Parse(arr[3]);
					break;
			}
		}
	}

	public class TransitionItem
	{
		public float time;
		public string targetId;
		public TransitionActionType type;
		public float duration;
		public TransitionValue value;
		public TransitionValue startValue;
		public TransitionValue endValue;
		public Ease easeType;
		public int repeat;
		public bool yoyo;
		public bool tween;
		public string label;
		public string label2;

		//hooks
		public TransitionHook hook;
		public TransitionHook hook2;

		//running properties
		public Tween tweener;
		public bool completed;
		public GObject target;
		public uint displayLockToken;

		//cached delegates
		public TweenCallback delayedCallDelegate;
		public TweenCallback tweenStartDelegate;
		public TweenCallback tweenUpdateDelegate;
		public TweenCallback tweenCompleteDelegate;

		public static TransitionItem createInstance(TransitionActionType type)
		{
			TransitionItem inst;
			switch (type)
			{
				case TransitionActionType.XY:
				case TransitionActionType.Size:
				case TransitionActionType.Scale:
				case TransitionActionType.Pivot:
				case TransitionActionType.Skew:
					inst = new TransitionItem_Vector2();
					break;

				case TransitionActionType.Alpha:
				case TransitionActionType.Rotation:
					inst = new TransitionItem_Float();
					break;

				case TransitionActionType.Color:
					inst = new TransitionItem_Color();
					break;

				case TransitionActionType.ColorFilter:
					inst = new TransitionItem_ColorFilter();
					break;

				case TransitionActionType.Animation:
					inst = new TransitionItem_Animation();
					break;

				case TransitionActionType.Shake:
					inst = new TransitionItem_Shake();
					break;

				case TransitionActionType.Sound:
					inst = new TransitionItem_Sound();
					break;

				case TransitionActionType.Transition:
					inst = new TransitionItem_Transition();
					break;

				case TransitionActionType.Visible:
					inst = new TransitionItem_Visible();
					break;

				default:
					inst = new TransitionItem();
					break;
			}

			inst.type = type;
			return inst;
		}

		public TransitionItem()
		{
		}

		virtual public void Setup(Transition owner)
		{
			easeType = Ease.OutQuad;
			delayedCallDelegate = () => { owner.OnDelayedCall(this); };

			if (tween)
			{
				tweenStartDelegate = () => { if (hook != null) hook(); };
				tweenUpdateDelegate = () => { owner.ApplyValue(this, value); };
				tweenCompleteDelegate = () => { owner.OnTweenComplete(this); };
			}
		}

		public void Copy(TransitionItem source)
		{
			this.time = source.time;
			this.targetId = source.targetId;
			this.type = source.type;
			this.duration = source.duration;
			if (this.value != null)
				this.value.Copy(source.value);
			if (this.startValue != null)
				this.startValue.Copy(source.startValue);
			if (this.endValue != null)
				this.endValue.Copy(source.endValue);
			this.easeType = source.easeType;
			this.repeat = source.repeat;
			this.yoyo = source.yoyo;
			this.tween = source.tween;
			this.label = source.label;
			this.label2 = source.label2;
		}
	}

	public class TransitionItem_Float : TransitionItem
	{
		public DOGetter<float> getter;
		public DOSetter<float> setter;

		public TransitionItem_Float()
		{
			value = new TransitionValue();
			startValue = new TransitionValue();
			endValue = new TransitionValue();

			getter = () => { return value.f1; };
			setter = (x) => { value.f1 = x; };
		}
	}

	public class TransitionItem_Vector2 : TransitionItem
	{
		public DOGetter<Vector2> getter;
		public DOSetter<Vector2> setter;

		public TransitionItem_Vector2()
		{
			value = new TransitionValue();
			startValue = new TransitionValue();
			endValue = new TransitionValue();

			getter = () => { return value.AsVec2; };
			setter = (x) => { value.AsVec2 = x; };
		}
	}

	public class TransitionItem_Color : TransitionItem
	{
		public DOGetter<Color> getter;
		public DOSetter<Color> setter;

		public TransitionItem_Color()
		{
			value = new TransitionValue();
			startValue = new TransitionValue();
			endValue = new TransitionValue();

			getter = () => { return value.AsColor; };
			setter = (x) => { value.AsColor = x; };
		}
	}

	public class TransitionItem_ColorFilter : TransitionItem
	{
		public DOGetter<Vector4> getter;
		public DOSetter<Vector4> setter;

		bool filterCreated;

		public TransitionItem_ColorFilter()
		{
			value = new TransitionValue();
			startValue = new TransitionValue();
			endValue = new TransitionValue();

			getter = () => { return value.AsVec4; };
			setter = (x) => { value.AsVec4 = x; };
		}

		public void SetFilter()
		{
			ColorFilter cf = target.filter as ColorFilter;
			if (cf == null)
			{
				cf = new ColorFilter();
				target.filter = cf;
				filterCreated = true;
			}
			else
			{
				cf.Reset();
			}
			cf.AdjustBrightness(value.f1);
			cf.AdjustContrast(value.f2);
			cf.AdjustSaturation(value.f3);
			cf.AdjustHue(value.f4);
		}

		public void ClearFilter()
		{
			if (filterCreated)
			{
				target.filter = null;
				filterCreated = false;
			}
		}
	}

	public class TransitionItem_Visible : TransitionItem
	{
		public bool visible;
	}

	public class TransitionItem_Animation : TransitionItem
	{
		public int frame;
		public bool playing;
	}

	public class TransitionItem_Sound : TransitionItem
	{
		public string sound;
		public float volume;
		public AudioClip audioClip;

		public override void Setup(Transition owner)
		{
			base.Setup(owner);
		}

		public void Play()
		{
			if (audioClip == null)
			{
				if (UIConfig.soundLoader == null || sound.StartsWith(UIPackage.URL_PREFIX))
					audioClip = UIPackage.GetItemAssetByURL(sound) as AudioClip;
				else
					audioClip = UIConfig.soundLoader(sound);
			}

			if (audioClip != null)
				Stage.inst.PlayOneShotSound(audioClip, volume);
		}
	}

	public class TransitionItem_Transition : TransitionItem
	{
		public string transName;
		public int transRepeat;

		public PlayCompleteCallback playCompleteDelegate;

		override public void Setup(Transition owner)
		{
			base.Setup(owner);

			playCompleteDelegate = () => { owner.OnInnerActionComplete(this); };
		}
	}

	public class TransitionItem_Shake : TransitionItem
	{
		public float shakeAmplitude;
		public float shakePeriod;

		float offsetX;
		float offsetY;
		float elapsed;

		TimerCallback timerDelegate;
		Transition owner;

		override public void Setup(Transition owner)
		{
			base.Setup(owner);

			timerDelegate = Run;
			this.owner = owner;
		}

		public void Start()
		{
			offsetX = offsetY = 0;
			elapsed = shakePeriod;
			Timers.inst.AddUpdate(timerDelegate);
		}

		public void Stop(bool resetPosition)
		{
			if (Timers.inst.Exists(timerDelegate))
			{
				Timers.inst.Remove(timerDelegate);

				if (resetPosition)
				{
					target._gearLocked = true;
					target.SetXY(target.x - offsetX, target.y - offsetY);
					target._gearLocked = false;
				}
			}
		}

		void Run(object param)
		{
			float r = Mathf.Ceil(shakeAmplitude * elapsed / shakePeriod);
			Vector2 vr = UnityEngine.Random.insideUnitCircle * r;
			vr.x = vr.x > 0 ? Mathf.Ceil(vr.x) : Mathf.Floor(vr.x);
			vr.y = vr.y > 0 ? Mathf.Ceil(vr.y) : Mathf.Floor(vr.y);

			target._gearLocked = true;
			target.SetXY(target.x - offsetX + vr.x, target.y - offsetY + vr.y);
			target._gearLocked = false;

			offsetX = vr.x;
			offsetY = vr.y;
			elapsed -= Time.deltaTime;
			if (elapsed <= 0)
			{
				target._gearLocked = true;
				target.SetXY(target.x - offsetX, target.y - offsetY);
				target._gearLocked = false;

				Timers.inst.Remove(timerDelegate);

				owner.OnInnerActionComplete(this);
			}
		}
	}

	public class TransitionValue
	{
		public float f1;
		public float f2;
		public float f3;
		public float f4;
		public bool b1;
		public bool b2;

		public TransitionValue()
		{
			b1 = true;
			b2 = true;
		}

		public void Copy(TransitionValue source)
		{
			this.f1 = source.f1;
			this.f2 = source.f2;
			this.f3 = source.f3;
			this.f4 = source.f4;
			this.b1 = source.b1;
			this.b2 = source.b2;
		}

		public Vector2 AsVec2
		{
			get { return new Vector2(f1, f2); }
			set
			{
				f1 = value.x;
				f2 = value.y;
			}
		}

		public Vector4 AsVec4
		{
			get { return new Vector4(f1, f2, f3, f4); }
			set
			{
				f1 = value.x;
				f2 = value.y;
				f3 = value.z;
				f4 = value.w;
			}
		}

		public Color AsColor
		{
			get { return new Color(f1, f2, f3, f4); }
			set
			{
				f1 = value.r;
				f2 = value.g;
				f3 = value.b;
				f4 = value.a;
			}
		}
	}
}
