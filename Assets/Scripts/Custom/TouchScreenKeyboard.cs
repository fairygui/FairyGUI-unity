using UnityEngine;

namespace FairyGUI
{
    public partial class TouchScreenKeyboard
    {
        public void Open(string text, int keyboardType, bool autocorrection, bool multiline, bool secure, bool alert,
            bool hideInput, string textPlaceholder, int characterLimit)
        {
            if (_keyboard != null)
                return;
            UnityEngine.TouchScreenKeyboard.hideInput = hideInput;
            this._keyboard = UnityEngine.TouchScreenKeyboard.Open(text, (TouchScreenKeyboardType)keyboardType,
                autocorrection, multiline, secure, alert,
                textPlaceholder, characterLimit);
        }
    }
}