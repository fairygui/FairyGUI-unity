using DG.Tweening;
using LuaInterface;
using System;
using UnityEngine;

public static class TweenUtils
{
	public static Tweener TweenFloat(float start, float end, float duration, LuaFunction OnUpdate)
	{
		return DOTween.To(() => start, x =>
		{
			try
			{
				OnUpdate.Call(x);
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}
		, end, duration).SetEase(Ease.Linear).OnComplete(() =>
		{
			OnUpdate.Dispose();
			OnUpdate = null;
		});
	}

	public static Tweener TweenVector2(Vector2 start, Vector2 end, float duration, LuaFunction OnUpdate)
	{
		return DOTween.To(() => start, x =>
		{
			try
			{
				OnUpdate.Call(x);
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}, end, duration).OnComplete(() =>
		{
			OnUpdate.Dispose();
			OnUpdate = null;
		});
	}

	public static Tweener TweenVector3(Vector3 start, Vector3 end, float duration, LuaFunction OnUpdate)
	{
		return DOTween.To(() => start, x => OnUpdate.Call(x), end, duration).OnComplete(() =>
		{
			OnUpdate.Dispose();
			OnUpdate = null;
		});
	}

	public static void SetEase(Tweener tweener, Ease ease)
	{
		tweener.SetEase(ease);
	}

	public static void OnComplete(Tweener tweener, LuaFunction func)
	{
		tweener.OnComplete(() =>
		{
			try
			{
				func.Call();
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
			func.Dispose();
			func = null;
		});
	}

	public static void OnComplete(Tweener tweener, LuaFunction func, object self)
	{
		tweener.OnComplete(() =>
		{
			try
			{
				func.Call(self);
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
			func.Dispose();
			func = null;
			if (self is LuaTable)
			{
				((LuaTable)self).Dispose();
				self = null;
			}
		});
	}

	public static void SetDelay(Tweener tweener, float delay)
	{
		tweener.SetDelay(delay);
	}

	public static void SetLoops(Tweener tweener, int loops)
	{
		tweener.SetLoops(loops);
	}

	public static void SetLoops(Tweener tweener, int loops, bool yoyo)
	{
		tweener.SetLoops(loops, yoyo ? LoopType.Yoyo : LoopType.Restart);
	}

	public static void SetTarget(Tweener tweener, object target)
	{
		tweener.SetTarget(target);
	}
}
