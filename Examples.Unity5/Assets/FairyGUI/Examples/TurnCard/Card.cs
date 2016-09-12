using System;
using System.Collections.Generic;
using FairyGUI;
using DG.Tweening;
using UnityEngine;

public class Card : GButton
{
	GObject _back;
	GObject _front;

	public override void ConstructFromXML(FairyGUI.Utils.XML xml)
	{
		base.ConstructFromXML(xml);

		_back = GetChild("n0");
		_front = GetChild("icon");
		_front.visible = false;
	}

	public bool opened
	{
		get
		{
			return _front.visible;
		}

		set
		{
			if (DOTween.IsTweening(this))
				DOTween.Kill(this);

			_front.visible = value;
			_back.visible = !value;
		}
	}

	public void SetPerspective()
	{
		_front.displayObject.perspective = true;
		_back.displayObject.perspective = true;
	}

	public void Turn()
	{
		if (DOTween.IsTweening(this))
			return;

		bool toOpen = !_front.visible;
		DOTween.To(() => 0, x =>
		{
			if (toOpen)
			{
				_back.rotationY = x;
				_front.rotationY = -180 + x;
				if (x > 90)
				{
					_front.visible = true;
					_back.visible = false;
				}
			}
			else
			{
				_back.rotationY = -180 + x;
				_front.rotationY = x;
				if (x > 90)
				{
					_front.visible = false;
					_back.visible = true;
				}
			}
		}, 180, 0.8f).SetTarget(this).SetEase(Ease.OutQuad);
	}
}