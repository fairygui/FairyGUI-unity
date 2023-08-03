using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI
{
    /// <summary>
    /// Gear is a connection between object and controller.
    /// </summary>
    public class GearFontSize : GearBase
    {
        Dictionary<string, int> _storage;
        int _default;
        GTextField _textField;

        public GearFontSize(GObject owner)
            : base(owner)
        {
        }

        protected override void Init()
        {
            if (_owner is GLabel)
                _textField = ((GLabel)_owner).GetTextField();
            else if (_owner is GButton)
                _textField = ((GButton)_owner).GetTextField();
            else
                _textField = (GTextField)_owner;

            _default = _textField.textFormat.size;
            _storage = new Dictionary<string, int>();
        }

        override protected void AddStatus(string pageId, ByteBuffer buffer)
        {
            if (pageId == null)
                _default = buffer.ReadInt();
            else
                _storage[pageId] = buffer.ReadInt();
        }

        override public void Apply()
        {
            if (_textField == null)
                return;

            _owner._gearLocked = true;

            int cv;
            if (!_storage.TryGetValue(_controller.selectedPageId, out cv))
                cv = _default;

            TextFormat tf = _textField.textFormat;
            tf.size = cv;
            _textField.textFormat = tf;

            _owner._gearLocked = false;
        }

        override public void UpdateState()
        {
            if (_textField != null)
                _storage[_controller.selectedPageId] = _textField.textFormat.size;
        }
    }
}
