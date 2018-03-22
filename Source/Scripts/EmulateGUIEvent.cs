using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
    public enum EmulateGUIEventType
    {
        KeyDown,
        KeyUp,
        scrollWheel
    }

    public struct EmulateGUIEvent
    {
        public EmulateGUIEventType type;
        public KeyCode keyCode;
        public EventModifiers modifiers;
        public char character;
        public Vector2 delta;

        public EmulateGUIEvent(EmulateGUIEventType t, KeyCode kc, EventModifiers em, char c, Vector2 dlt)
        {
            type = t;
            keyCode = kc;
            modifiers = em;
            character = c;
            delta = dlt;
        }
    }
}