using System;
using System.Collections.Generic;
using System.Reflection;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class UIObjectFactory
	{
		internal static Dictionary<string, ConstructorInfo> packageItemExtensions = new Dictionary<string, ConstructorInfo>();
		internal static ConstructorInfo loaderConstructor;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="type"></param>
		public static void SetPackageItemExtension(string url, System.Type type)
		{
			packageItemExtensions[url.Substring(5)] = type.GetConstructor(System.Type.EmptyTypes);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		public static void SetLoaderExtension(System.Type type)
		{
			loaderConstructor = type.GetConstructor(System.Type.EmptyTypes);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pi"></param>
		/// <returns></returns>
		public static GObject NewObject(PackageItem pi)
		{
			Stats.LatestObjectCreation++;

			switch (pi.type)
			{
				case PackageItemType.Image:
					return new GImage();

				case PackageItemType.MovieClip:
					return new GMovieClip();

				case PackageItemType.Component:
					{
						ConstructorInfo extentionConstructor;
						if (packageItemExtensions.TryGetValue(pi.owner.id + pi.id, out extentionConstructor))
						{
							GComponent g = (GComponent)extentionConstructor.Invoke(null);
							if (g == null)
								throw new Exception("Unable to create instance of '" + extentionConstructor.Name + "'");

							return g;
						}

						XML xml = pi.componentData;
						string extention = xml.GetAttribute("extention");
						if (extention != null)
						{
							switch (extention)
							{
								case "Button":
									return new GButton();

								case "Label":
									return new GLabel();

								case "ProgressBar":
									return new GProgressBar();

								case "Slider":
									return new GSlider();

								case "ScrollBar":
									return new GScrollBar();

								case "ComboBox":
									return new GComboBox();

								default:
									return new GComponent();
							}
						}
						else
							return new GComponent();
					}
			}
			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static GObject NewObject(string type)
		{
			Stats.LatestObjectCreation++;

			switch (type)
			{
				case "image":
					return new GImage();

				case "movieclip":
					return new GMovieClip();

				case "component":
					return new GComponent();

				case "text":
					return new GTextField();

				case "richtext":
					return new GRichTextField();

				case "inputtext":
					return new GTextInput();

				case "group":
					return new GGroup();

				case "list":
					return new GList();

				case "graph":
					return new GGraph();

				case "loader":
					if (loaderConstructor != null)
						return (GLoader)loaderConstructor.Invoke(null);
					else
						return new GLoader();
			}
			return null;
		}
	}
}
