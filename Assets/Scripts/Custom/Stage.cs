namespace FairyGUI
{
    public partial class Stage
    {
        /// <summary>
        /// 打开键盘
        /// </summary>
        /// <param name="text"></param>
        /// <param name="keyboardType"></param>
        /// <param name="autocorrection"></param>
        /// <param name="multiline"></param>
        /// <param name="secure"></param>
        /// <param name="alert"></param>
        /// <param name="hideInput"></param>
        /// <param name="textPlaceholder"></param>
        /// <param name="characterLimit"></param>
        public void OpenKeyboard(string text, int keyboardType, bool autocorrection, bool multiline, bool secure,
            bool alert, bool hideInput, string textPlaceholder, int characterLimit)
        {
            if (_keyboard != null)
            {
                _keyboard.Open(text, keyboardType, autocorrection, multiline, secure, alert, hideInput, textPlaceholder,
                    characterLimit);
            }
        }
    }
}