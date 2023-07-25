#if FAIRYGUI_TMPRO
namespace FairyGUI
{
    public class TextFieldDisplay : DisplayObject
    {
        public TextFieldDisplay(TextField textField)
        {
            _flags |= Flags.TouchDisabled;

            CreateGameObject("TextFieldDisplay");
            graphics = new NGraphics(gameObject);
            graphics.meshFactory = textField;

            gOwner = textField.gOwner;
        }
    }
}
#endif
