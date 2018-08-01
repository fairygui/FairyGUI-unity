using System.Collections.Generic;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public enum EaseType
	{
		Linear,
		SineIn,
		SineOut,
		SineInOut,
		QuadIn,
		QuadOut,
		QuadInOut,
		CubicIn,
		CubicOut,
		CubicInOut,
		QuartIn,
		QuartOut,
		QuartInOut,
		QuintIn,
		QuintOut,
		QuintInOut,
		ExpoIn,
		ExpoOut,
		ExpoInOut,
		CircIn,
		CircOut,
		CircInOut,
		ElasticIn,
		ElasticOut,
		ElasticInOut,
		BackIn,
		BackOut,
		BackInOut,
		BounceIn,
		BounceOut,
		BounceInOut,
		Custom
	}

	public static class EaseTypeUtils
	{
		static Dictionary<string, EaseType> EaseTypeMap = new Dictionary<string, EaseType>
		{
			{ "Linear", EaseType.Linear },
			{ "Elastic.In", EaseType.ElasticIn },
			{ "Elastic.Out", EaseType.ElasticInOut },
			{ "Elastic.InOut", EaseType.ElasticInOut },
			{ "Quad.In", EaseType.QuadIn },
			{ "Quad.Out", EaseType.QuadOut },
			{ "Quad.InOut", EaseType.QuadInOut },
			{ "Cube.In", EaseType.CubicIn },
			{ "Cube.Out", EaseType.CubicOut },
			{ "Cube.InOut", EaseType.CubicInOut },
			{ "Quart.In", EaseType.QuartIn },
			{ "Quart.Out", EaseType.QuartOut },
			{ "Quart.InOut", EaseType.QuartInOut },
			{ "Quint.In", EaseType.QuintIn },
			{ "Quint.Out", EaseType.QuintOut },
			{ "Quint.InOut", EaseType.QuintInOut },
			{ "Sine.In", EaseType.SineIn },
			{ "Sine.Out", EaseType.SineOut },
			{ "Sine.InOut", EaseType.SineInOut },
			{ "Bounce.In", EaseType.BounceIn },
			{ "Bounce.Out", EaseType.BounceOut },
			{ "Bounce.InOut", EaseType.BounceInOut },
			{ "Circ.In", EaseType.CircIn },
			{ "Circ.Out", EaseType.CircOut },
			{ "Circ.InOut", EaseType.CircInOut },
			{ "Expo.In", EaseType.ExpoIn },
			{ "Expo.Out", EaseType.ExpoOut },
			{ "Expo.InOut", EaseType.ExpoInOut },
			{ "Back.In", EaseType.BackIn },
			{ "Back.Out", EaseType.BackOut },
			{ "Back.InOut", EaseType.BackInOut }
		};

		public static EaseType ParseEaseType(string value)
		{
			EaseType type;
			if (!EaseTypeMap.TryGetValue(value, out type))
				type = EaseType.ExpoOut;

			return type;
		}
	}
}
