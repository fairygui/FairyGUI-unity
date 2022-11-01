using System;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

public class EmojiMain : MonoBehaviour
{
    GComponent _mainView;
    GList _list;
    GTextInput _input1;
    GTextInput _input2;
    GComponent _emojiSelectUI1;
    GComponent _emojiSelectUI2;

    class Message
    {
        public string sender;
        public string senderIcon;
        public string msg;
        public bool fromMe;
    }
    List<Message> _messages;

    Dictionary<uint, Emoji> _emojies;

    void Awake()
    {
        UIPackage.AddPackage("UI/Emoji");

        UIConfig.verticalScrollBar = "ui://Emoji/ScrollBar_VT";
        UIConfig.defaultScrollBarDisplay = ScrollBarDisplayType.Auto;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _messages = new List<Message>();

        _mainView = this.GetComponent<UIPanel>().ui;

        _list = _mainView.GetChild("list").asList;
        _list.SetVirtual();
        _list.itemProvider = GetListItemResource;
        _list.itemRenderer = RenderListItem;

        _input1 = _mainView.GetChild("input1").asTextInput;
        _input1.onKeyDown.Add(__inputKeyDown1);

        _input2 = _mainView.GetChild("input2").asTextInput;
        _input2.onKeyDown.Add(__inputKeyDown2);

        //作为demo，这里只添加了部分表情素材
        _emojies = new Dictionary<uint, Emoji>();
        for (uint i = 0x1f600; i < 0x1f637; i++)
        {
            string url = UIPackage.GetItemURL("Emoji", Convert.ToString(i, 16));
            if (url != null)
                _emojies.Add(i, new Emoji(url));
        }
        _input2.emojies = _emojies;

        _mainView.GetChild("btnSend1").onClick.Add(__clickSendBtn1);
        _mainView.GetChild("btnSend2").onClick.Add(__clickSendBtn2);

        _mainView.GetChild("btnEmoji1").onClick.Add(__clickEmojiBtn1);
        _mainView.GetChild("btnEmoji2").onClick.Add(__clickEmojiBtn2);

        _emojiSelectUI1 = UIPackage.CreateObject("Emoji", "EmojiSelectUI").asCom;
        _emojiSelectUI1.fairyBatching = true;
        _emojiSelectUI1.GetChild("list").asList.onClickItem.Add(__clickEmoji1);

        _emojiSelectUI2 = UIPackage.CreateObject("Emoji", "EmojiSelectUI_ios").asCom;
        _emojiSelectUI2.fairyBatching = true;
        _emojiSelectUI2.GetChild("list").asList.onClickItem.Add(__clickEmoji2);
    }

    void AddMsg(string sender, string senderIcon, string msg, bool fromMe)
    {
        bool isScrollBottom = _list.scrollPane.isBottomMost;

        Message newMessage = new Message();
        newMessage.sender = sender;
        newMessage.senderIcon = senderIcon;
        newMessage.msg = msg;
        newMessage.fromMe = fromMe;
        _messages.Add(newMessage);

        if (newMessage.fromMe)
        {
            if (_messages.Count == 1 || UnityEngine.Random.Range(0f, 1f) < 0.5f)
            {
                Message replyMessage = new Message();
                replyMessage.sender = "FairyGUI";
                replyMessage.senderIcon = "r1";
                replyMessage.msg = "Today is a good day. \U0001f600";
                replyMessage.fromMe = false;
                _messages.Add(replyMessage);
            }
        }

        if (_messages.Count > 100)
            _messages.RemoveRange(0, _messages.Count - 100);

        _list.numItems = _messages.Count;

        if (isScrollBottom)
            _list.scrollPane.ScrollBottom();
    }

    string GetListItemResource(int index)
    {
        Message msg = _messages[index];
        if (msg.fromMe)
            return "ui://Emoji/chatRight";
        else
            return "ui://Emoji/chatLeft";
    }

    void RenderListItem(int index, GObject obj)
    {
        GButton item = (GButton)obj;
        Message msg = _messages[index];
        if (!msg.fromMe)
            item.GetChild("name").text = msg.sender;
        item.icon = UIPackage.GetItemURL("Emoji", msg.senderIcon);

        //Recaculate the text width
        GRichTextField tf = item.GetChild("msg").asRichTextField;
        tf.emojies = _emojies;
        tf.text = EmojiParser.inst.Parse(msg.msg);
    }

    void __clickSendBtn1(EventContext context)
    {
        string msg = _input1.text;
        if (msg.Length == 0)
            return;

        AddMsg("Unity", "r0", msg, true);
        _input1.text = "";
    }

    void __clickSendBtn2(EventContext context)
    {
        string msg = _input2.text;
        if (msg.Length == 0)
            return;

        AddMsg("Unity", "r0", msg, true);
        _input2.text = "";
    }

    void __clickEmojiBtn1(EventContext context)
    {
        GRoot.inst.ShowPopup(_emojiSelectUI1, (GObject)context.sender, PopupDirection.Up);
    }

    void __clickEmojiBtn2(EventContext context)
    {
        GRoot.inst.ShowPopup(_emojiSelectUI2, (GObject)context.sender, PopupDirection.Up);
    }

    void __clickEmoji1(EventContext context)
    {
        GButton item = (GButton)context.data;
        _input1.ReplaceSelection("[:" + item.text + "]");
    }

    void __clickEmoji2(EventContext context)
    {
        GButton item = (GButton)context.data;
        _input2.ReplaceSelection(Char.ConvertFromUtf32(Convert.ToInt32(UIPackage.GetItemByURL(item.icon).name, 16)));
    }

    void __inputKeyDown1(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Return)
            _mainView.GetChild("btnSend1").onClick.Call();
    }

    void __inputKeyDown2(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Return)
            _mainView.GetChild("btnSend2").onClick.Call();
    }

    void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
        {
            Application.Quit();
        }
    }
}