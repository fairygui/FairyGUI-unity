namespace FairyGUI
{
	public interface IMobileInputAdapter
	{
		int keyboardType { get; set; }
		bool done { get; }
		string GetInput();
		void OpenKeyboard(string text, bool autocorrection, bool multiline, bool secure, bool alert, string textPlaceholder);
		void CloseKeyboard();
	}
}
