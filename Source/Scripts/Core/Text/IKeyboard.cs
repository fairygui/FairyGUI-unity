namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public interface IKeyboard
	{
		/// <summary>
		/// 
		/// </summary>
		bool done { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		string GetInput();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="text"></param>
		/// <param name="autocorrection"></param>
		/// <param name="multiline"></param>
		/// <param name="secure"></param>
		/// <param name="alert"></param>
		/// <param name="textPlaceholder"></param>
		/// <param name="keyboardType"></param>
		void Open(string text, bool autocorrection, bool multiline, bool secure, bool alert, string textPlaceholder, int keyboardType);

		/// <summary>
		/// 
		/// </summary>
		void Close();
	}
}
