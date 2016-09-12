using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using System;

public class EmojiProMain : MonoBehaviour
{
	GComponent _mainView;
	GList _list;
	GTextInput _input;

	string _itemURL1;
	string _itemURL2;

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
		UIPackage.AddPackage("UI/EmojiPro");

		UIConfig.verticalScrollBar = UIPackage.GetItemURL("EmojiPro", "ScrollBar_VT");
		UIConfig.defaultScrollBarDisplay = ScrollBarDisplayType.Auto;
	}

	void Start()
	{
		Application.targetFrameRate = 60;
		Stage.inst.onKeyDown.Add(OnKeyDown);

		_itemURL1 = UIPackage.GetItemURL("EmojiPro", "chatLeft");
		_itemURL2 = UIPackage.GetItemURL("EmojiPro", "chatRight");

		_messages = new List<Message>();

		_mainView = this.GetComponent<UIPanel>().ui;

		_list = _mainView.GetChild("list").asList;
		_list.defaultItem = _itemURL1;
		_list.SetVirtual();
		_list.itemProvider = GetListItemResource;
		_list.itemRenderer = RenderListItem;

		_input = _mainView.GetChild("input").asTextInput;
		_input.onKeyDown.Add(__inputKeyDown);

		//作为demo，这里只添加了部分表情素材
		_emojies = new Dictionary<uint, Emoji>();
		for (uint i = 0x1f600; i < 0x1f637; i++)
		{
			string url = UIPackage.GetItemURL("EmojiPro", Convert.ToString(i, 16));
			if (url != null)
				_emojies.Add(i, new Emoji(url));
		}
		_input.emojies = _emojies;

		_mainView.GetChild("btnSend").onClick.Add(__clickSendBtn);
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
			return _itemURL2;
		else
			return _itemURL1;
	}

	void RenderListItem(int index, GObject obj)
	{
		GButton item = (GButton)obj;
		Message msg = _messages[index];
		if (!msg.fromMe)
			item.GetChild("name").text = msg.sender;
		item.icon = UIPackage.GetItemURL("EmojiPro", msg.senderIcon);

		//Recaculate the text width
		GRichTextField tf = item.GetChild("msg").asRichTextField;
		tf.emojies = _emojies;
		tf.width = tf.initWidth;
		tf.text = msg.msg;
		tf.width = tf.textWidth;
	}

	void __clickSendBtn()
	{
		string msg = _input.text;
		if (msg.Length == 0)
			return;

		AddMsg("Unity", "r0", msg, true);
		_input.text = "";
	}

	void __inputKeyDown(EventContext context)
	{
		if (context.inputEvent.keyCode == KeyCode.Return)
			__clickSendBtn();
	}

	void OnKeyDown(EventContext context)
	{
		if (context.inputEvent.keyCode == KeyCode.Escape)
		{
			Application.Quit();
		}
	}
}