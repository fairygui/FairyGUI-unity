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
		public delegate GLoader GLoaderCreator();
		public delegate GComponent GComponentCreator();
		internal static Dictionary<string, GComponentCreator> packageItemExtensions = new Dictionary<string, GComponentCreator>();
		internal static GLoaderCreator loaderConstructor;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="type"></param>
		public static void SetPackageItemExtension(string url, System.Type type)
		{
			SetPackageItemExtension(url, () =>
			{
				GComponent g = Activator.CreateInstance(type) as GComponent;
				if (g == null)
					throw new Exception("Unable to create instance of '" + type.FullName + "'");
				return g;
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="creator"></param>
		public static void SetPackageItemExtension(string url, GComponentCreator creator)
		{
			packageItemExtensions[url.Substring(5)] = creator;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		public static void SetLoaderExtension(System.Type type)
		{
			SetLoaderExtension(() => (GLoader)Activator.CreateInstance(type));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="creator"></param>
		public static void SetLoaderExtension(GLoaderCreator creator)
		{
			loaderConstructor = creator;
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
						GComponentCreator creator;
						if (packageItemExtensions.TryGetValue(pi.owner.id + pi.id, out creator))
							return creator();

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
						return loaderConstructor();
					return new GLoader();
			}
			return null;
		}
	}
}
