using UnityEngine;
using FairyGUI;

public class EmojiMain : MonoBehaviour
{
	GComponent _mainView;
	GList _list;
	GTextInput _input;
	GComponent _emojiSelectUI;

	string _itemURL1;
	string _itemURL2;

	void Awake()
	{
		UIPackage.AddPackage("UI/Emoji");

		UIConfig.verticalScrollBar = UIPackage.GetItemURL("Emoji", "ScrollBar_VT");
		UIConfig.defaultScrollBarDisplay = ScrollBarDisplayType.Auto;
	}

	void Start()
	{
		Application.targetFrameRate = 60;
		Stage.inst.onKeyDown.Add(OnKeyDown);

		_mainView = this.GetComponent<UIPanel>().ui;

		_list = _mainView.GetChild("list").asList;
		_list.RemoveChildrenToPool();
		_input = _mainView.GetChild("input").asTextInput;
		_input.onKeyDown.Add(__inputKeyDown);

		_itemURL1 = UIPackage.GetItemURL("Emoji", "chatLeft");
		_itemURL2 = UIPackage.GetItemURL("Emoji", "chatRight");

		_mainView.GetChild("btnSend").onClick.Add(__clickSendBtn);
		_mainView.GetChild("btnEmoji").onClick.Add(__clickEmojiBtn);

		_emojiSelectUI = UIPackage.CreateObject("Emoji", "EmojiSelectUI").asCom;
		_emojiSelectUI.fairyBatching = true;
		_emojiSelectUI.GetChild("list").asList.onClickItem.Add(__clickEmoji);
	}

	void AddMsg(string sender, string senderIcon, string msg, bool fromMe)
	{
		bool isScrollBottom = _list.scrollPane.isBottomMost;

		GButton item = _list.AddItemFromPool(fromMe ? _itemURL2 : _itemURL1).asButton;
		if(!fromMe)
			item.GetChild("name").text = sender;
		item.icon = UIPackage.GetItemURL("Emoji", senderIcon);

		//Recaculate the text width
		GRichTextField tf = item.GetChild("msg").asRichTextField;
		tf.width = tf.initWidth;
		tf.text = EmojiParser.inst.Parse(msg);
		tf.width = tf.textWidth;

		if (fromMe)
		{
			if (_list.numChildren==1 || Random.Range(0f, 1f) < 0.5f)
			{
				AddMsg("FairyGUI", "r1", "Today is a good day. [:gz]", false);
			}
		}

		if (_list.numChildren > 30)
			_list.RemoveChildrenToPool(0, _list.numChildren - 30);

		if (isScrollBottom)
			_list.scrollPane.ScrollBottom(true);
	}

	void __clickSendBtn()
	{
		string msg = _input.text;
		if (msg.Length == 0)
			return;

		AddMsg("Unity", "r0", msg, true);
		_input.text = "";
	}

	void __clickEmojiBtn(EventContext context)
	{
		GRoot.inst.ShowPopup(_emojiSelectUI, (GObject)context.sender, false);
	}

	void __clickEmoji(EventContext context)
	{
		GButton item = (GButton)context.data;
		_input.ReplaceSelection("[:" + item.text + "]");
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