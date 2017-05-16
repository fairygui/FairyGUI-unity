using System;
using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class UIObjectFactory
	{
		public delegate GComponent GComponentCreator();
		public delegate GLoader GLoaderCreator();

		static Dictionary<string, GComponentCreator> packageItemExtensions = new Dictionary<string, GComponentCreator>();
		static GLoaderCreator loaderCreator;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="type"></param>
		public static void SetPackageItemExtension(string url, System.Type type)
		{
			SetPackageItemExtension(url, () => { return (GComponent)Activator.CreateInstance(type); });
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="creator"></param>
		public static void SetPackageItemExtension(string url, GComponentCreator creator)
		{
			if (url == null)
				throw new Exception("Invaild url: " + url);

			PackageItem pi = UIPackage.GetItemByURL(url);
			if (pi != null)
				pi.extensionCreator = creator;

			packageItemExtensions[url] = creator;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		public static void SetLoaderExtension(System.Type type)
		{
			loaderCreator = () => { return (GLoader)Activator.CreateInstance(type); };
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="creator"></param>
		public static void SetLoaderExtension(GLoaderCreator creator)
		{
			loaderCreator = creator;
		}

		internal static void ResolvePackageItemExtension(PackageItem pi)
		{
			if (!packageItemExtensions.TryGetValue(UIPackage.URL_PREFIX + pi.owner.id + pi.id, out pi.extensionCreator)
				&& !packageItemExtensions.TryGetValue(UIPackage.URL_PREFIX + pi.owner.name + "/" + pi.name, out pi.extensionCreator))
				pi.extensionCreator = null;
		}

		internal static void Clear()
		{
			packageItemExtensions.Clear();
			loaderCreator = null;
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
						if (pi.extensionCreator != null)
							return pi.extensionCreator();

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
					if (loaderCreator != null)
						return loaderCreator();
					else
						return new GLoader();
			}
			return null;
		}
	}
}
