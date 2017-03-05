using System;
using System.Collections.Generic;
using FairyGUI.Utils;
using DG.Tweening;
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

		void _Play(int times, float delay, PlayCompleteCallback onComplete, bool reverse)
		{
			Stop(true, true);

			if (times == 0)
				times = 1;
			else if (times == -1)
				times = int.MaxValue;
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

			if (item.type == TransitionActionType.ColorFilter && item.filterCreated)
				item.target.filter = null;

			if (item.completed)
				return;

			if (item.tweener != null)
			{
				item.tweener.Kill();
				item.tweener = null;
			}

			if (item.type == TransitionActionType.Transition)
			{
				Transition trans = ((GComponent)item.target).GetTransition(item.value.s);
				if (trans != null)
					trans.Stop(setToComplete, false);
			}
			else if (item.type == TransitionActionType.Shake)
			{
				if (Timers.inst.Exists(item.__Shake))
				{
					Timers.inst.Remove(item.__Shake);
					item.target._gearLocked = true;
					item.target.SetXY(item.target.x - item.startValue.f1, item.target.y - item.startValue.f2);
					item.target._gearLocked = false;
				}
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
			if (!_playing)
				return;

			_playing = false;
			int cnt = _items.Count;
			for (int i = 0; i < cnt; i++)
			{
				TransitionItem item = _items[i];
				if (item.target == null || item.completed)
					continue;

				if (item.tweener != null)
				{
					item.tweener.Kill();
					item.tweener = null;
				}

				if (item.type == TransitionActionType.Transition)
				{
					Transition trans = ((GComponent)item.target).GetTransition(item.value.s);
					if (trans != null)
						trans.Dispose();
				}
				else if (item.type == TransitionActionType.Shake)
				{
					Timers.inst.Remove(item.__Shake);
				}
			}
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
						value.c = (Color)aParams[0];
						break;

					case TransitionActionType.Animation:
						value.i = Convert.ToInt32(aParams[0]);
						if (aParams.Length > 1)
							value.b = Convert.ToBoolean(aParams[1]);
						break;

					case TransitionActionType.Visible:
						value.b = Convert.ToBoolean(aParams[0]);
						break;

					case TransitionActionType.Sound:
						value.s = (string)aParams[0];
						if (aParams.Length > 1)
							value.f1 = Convert.ToSingle(aParams[1]);
						break;

					case TransitionActionType.Transition:
						value.s = (string)aParams[0];
						if (aParams.Length > 1)
							value.i = Convert.ToInt32(aParams[1]);
						break;

					case TransitionActionType.Shake:
						value.f1 = Convert.ToSingle(aParams[0]);
						if (aParams.Length > 1)
							value.f2 = Convert.ToSingle(aParams[1]);
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
		/// <param name="source"></param>
		public void Copy(Transition source)
		{
			Stop();
			_items.Clear();
			int cnt = source._items.Count;
			for (int i = 0; i < cnt; i++)
			{
				_items.Add(source._items[i].Clone());
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
						item.tweener = DOVirtual.DelayedCall(startTime, () =>
						{
							item.tweener = null;
							_totalTasks--;

							StartTween(item, 0);
						}, true);
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
						item.tweener = DOVirtual.DelayedCall(startTime, () =>
						{
							item.tweener = null;
							item.completed = true;
							_totalTasks--;

							ApplyValue(item, item.value);
							if (item.hook != null)
								item.hook();

							CheckAllComplete();
						}, true);
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

						item.tweener = DOTween.To(() => new Vector2(startValue.f1, startValue.f2),
									val =>
									{
										item.value.f1 = val.x;
										item.value.f2 = val.y;
									}, new Vector2(endValue.f1, endValue.f2), item.duration);
					}
					break;

				case TransitionActionType.Scale:
				case TransitionActionType.Skew:
					{
						item.value.f1 = startValue.f1;
						item.value.f2 = startValue.f2;
						item.tweener = DOTween.To(() => new Vector2(startValue.f1, startValue.f2),
							val =>
							{
								item.value.f1 = val.x;
								item.value.f2 = val.y;
							}, new Vector2(endValue.f1, endValue.f2), item.duration);
						break;
					}

				case TransitionActionType.Alpha:
				case TransitionActionType.Rotation:
					{
						item.value.f1 = startValue.f1;
						item.tweener = DOTween.To(() => startValue.f1, v => item.value.f1 = v, endValue.f1, item.duration);
						break;
					}

				case TransitionActionType.Color:
					{
						item.value.c = startValue.c;
						item.tweener = DOTween.To(() => startValue.c, v => item.value.c = v, endValue.c, item.duration);
						break;
					}

				case TransitionActionType.ColorFilter:
					{
						item.value.f1 = startValue.f1;
						item.value.f2 = startValue.f2;
						item.value.f3 = startValue.f3;
						item.value.f4 = startValue.f4;
						item.tweener = DOTween.To(() => new Vector4(startValue.f1, startValue.f2, startValue.f3, startValue.f4),
							v =>
							{
								item.value.f1 = v.x;
								item.value.f2 = v.y;
								item.value.f3 = v.z;
								item.value.f4 = v.w;
							},
							new Vector4(endValue.f1, endValue.f2, endValue.f3, endValue.f4), item.duration);
						break;
					}
			}

			item.tweener.SetEase(item.easeType)
				.SetUpdate(true)
				.OnStart(() => { if (item.hook != null) item.hook(); })
				.OnUpdate(() => { ApplyValue(item, item.value); })
				.OnComplete(() => { tweenComplete(item); });
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

		void tweenComplete(TransitionItem item)
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

		void __playTransComplete(TransitionItem item)
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

								if (item.filterCreated)
								{
									item.filterCreated = false;
									item.target.filter = null;
								}
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

		void ApplyValue(TransitionItem item, TransitionValue value)
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
					item.target.skew = new Vector2(value.f1, value.f2);
					if (invalidateBatchingEveryFrame)
						_owner.InvalidateBatchingState(true);
					break;

				case TransitionActionType.Color:
					((IColorGear)item.target).color = value.c;
					break;

				case TransitionActionType.Animation:
					if (!value.b1)
						value.i = ((IAnimationGear)item.target).frame;
					((IAnimationGear)item.target).frame = value.i;
					((IAnimationGear)item.target).playing = value.b;
					break;

				case TransitionActionType.Visible:
					item.target.visible = value.b;
					break;

				case TransitionActionType.Transition:
					Transition trans = ((GComponent)item.target).GetTransition(value.s);
					if (trans != null)
					{
						if (value.i == 0)
							trans.Stop(false, true);
						else if (trans.playing)
							trans._totalTimes = value.i == -1 ? int.MaxValue : value.i;
						else
						{
							item.completed = false;
							_totalTasks++;
							if (_reversed)
								trans.PlayReverse(value.i, 0, () => { __playTransComplete(item); });
							else
								trans.Play(value.i, 0, () => { __playTransComplete(item); });
							if (_timeScale != 1)
								trans.timeScale = _timeScale;
						}
					}
					break;

				case TransitionActionType.Sound:
					AudioClip sound = UIPackage.GetItemAssetByURL(value.s) as AudioClip;
					if (sound != null)
						Stage.inst.PlayOneShotSound(sound, value.f1);
					break;

				case TransitionActionType.Shake:
					item.startValue.f1 = 0; //offsetX
					item.startValue.f2 = 0; //offsetY
					item.startValue.f3 = item.value.f2;//shakePeriod
					Timers.inst.AddUpdate(item.__Shake, this);
					_totalTasks++;
					item.completed = false;
					break;

				case TransitionActionType.ColorFilter:
					ColorFilter cf = item.target.filter as ColorFilter;
					if (cf == null)
					{
						cf = new ColorFilter();
						item.target.filter = cf;
						item.filterCreated = true;
					}
					else
					{
						cf.Reset();
					}
					cf.AdjustBrightness(value.f1);
					cf.AdjustContrast(value.f2);
					cf.AdjustSaturation(value.f3);
					cf.AdjustHue(value.f4);
					break;
			}

			item.target._gearLocked = false;
		}

		internal void ShakeItem(TransitionItem item)
		{
			float r = Mathf.Ceil(item.value.f1 * item.startValue.f3 / item.value.f2);
			Vector2 vr = UnityEngine.Random.insideUnitCircle * r;
			vr.x = vr.x > 0 ? Mathf.Ceil(vr.x) : Mathf.Floor(vr.x);
			vr.y = vr.y > 0 ? Mathf.Ceil(vr.y) : Mathf.Floor(vr.y);

			item.target._gearLocked = true;
			item.target.SetXY(item.target.x - item.startValue.f1 + vr.x, item.target.y - item.startValue.f2 + vr.y);
			item.target._gearLocked = false;

			item.startValue.f1 = vr.x;
			item.startValue.f2 = vr.y;
			item.startValue.f3 -= Time.deltaTime;
			if (item.startValue.f3 <= 0)
			{
				item.target._gearLocked = true;
				item.target.SetXY(item.target.x - item.startValue.f1, item.target.y - item.startValue.f2);
				item.target._gearLocked = false;

				item.completed = true;
				_totalTasks--;
				Timers.inst.Remove(item.__Shake);

				CheckAllComplete();
			}
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
				TransitionItem item = new TransitionItem();
				_items.Add(item);

				item.time = (float)cxml.GetAttributeInt("time") / (float)FRAME_RATE;
				item.targetId = cxml.GetAttribute("target", string.Empty);
				item.type = FieldTypes.ParseTransitionActionType(cxml.GetAttribute("type"));
				item.tween = cxml.GetAttributeBool("tween");
				item.label = cxml.GetAttribute("label");
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
						DecodeValue(item.type, cxml.GetAttribute("startValue", string.Empty), item.startValue);
						DecodeValue(item.type, v, item.endValue);
					}
					else
					{
						item.tween = false;
						DecodeValue(item.type, cxml.GetAttribute("startValue", string.Empty), item.value);
					}
				}
				else
				{
					if (item.time > _maxTime)
						_maxTime = item.time;
					DecodeValue(item.type, cxml.GetAttribute("value", string.Empty), item.value);
				}
			}
		}

		void DecodeValue(TransitionActionType type, string str, TransitionValue value)
		{
			string[] arr;
			switch (type)
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
					value.c = ToolSet.ConvertFromHtmlColor(str);
					break;

				case TransitionActionType.Animation:
					arr = str.Split(',');
					if (arr[0] == "-")
					{
						value.b1 = false;
					}
					else
					{
						value.i = int.Parse(arr[0]);
						value.b1 = true;
					}
					value.b = arr[1] == "p";
					break;

				case TransitionActionType.Visible:
					value.b = str == "true";
					break;

				case TransitionActionType.Sound:
					arr = str.Split(',');
					value.s = arr[0];
					if (arr.Length > 1)
					{
						int intv = int.Parse(arr[1]);
						if (intv == 100 || intv == 0)
							value.f1 = 1;
						else
							value.f1 = (float)intv / 100f;
					}
					else
						value.f1 = 1;
					break;

				case TransitionActionType.Transition:
					arr = str.Split(',');
					value.s = arr[0];
					if (arr.Length > 1)
						value.i = int.Parse(arr[1]);
					else
						value.i = 1;
					break;

				case TransitionActionType.Shake:
					arr = str.Split(',');
					value.f1 = float.Parse(arr[0]);
					value.f2 = float.Parse(arr[1]);
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
		public bool filterCreated;
		public uint displayLockToken;

		public TransitionItem()
		{
			easeType = Ease.OutQuad;
			value = new TransitionValue();
			startValue = new TransitionValue();
			endValue = new TransitionValue();
		}

		public TransitionItem Clone()
		{
			TransitionItem item = new TransitionItem();
			item.time = this.time;
			item.targetId = this.targetId;
			item.type = this.type;
			item.duration = this.duration;
			item.value.Copy(this.value);
			item.startValue.Copy(this.startValue);
			item.endValue.Copy(this.endValue);
			item.easeType = this.easeType;
			item.repeat = this.repeat;
			item.yoyo = this.yoyo;
			item.tween = this.tween;
			item.label = this.label;
			item.label2 = this.label2;
			return item;
		}

		public void __Shake(object callback)
		{
			((Transition)callback).ShakeItem(this);
		}
	}

	public class TransitionValue
	{
		public float f1;//x, scalex, pivotx,alpha,shakeAmplitude,rotation
		public float f2;//y, scaley, pivoty, shakePeriod
		public float f3;
		public float f4;
		public int i;//frame
		public Color c;//color
		public bool b;//playing
		public string s;//sound,transName

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
			this.i = source.i;
			this.c = source.c;
			this.b = source.b;
			this.s = source.s;
			this.b1 = source.b1;
			this.b2 = source.b2;
		}
	}
}
