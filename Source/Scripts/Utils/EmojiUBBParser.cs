using FairyGUI;
using FairyGUI.Utils;

public class EmojiUBBParser : UBBParser
{
    static EmojiUBBParser _instance = null;
    public static EmojiUBBParser Instance
    {
        get
        {
            if (_instance == null)
                _instance = new EmojiUBBParser();
            return _instance;
        }
    }

    public EmojiUBBParser()
    {
  
    }
    public string UrlPackageName
    {
        get; set;
    }

    public void SetEmojiHandlersr(string ss)
    {
        this.handlers[ss] = OnTag_Emoji;
    }

    protected string OnTag_Emoji(string tagName, bool end, string attr)
    {
        //var tarName = tagName.Substring(1).ToLower();
        var tarName = tagName;

        var url = UIPackage.GetItemURLLTR(UrlPackageName, tarName);
        return "<img src='" + url + "'/>";
    }
    public new string Parse(string text)
    {
        text = LuaFramework.Util.ToDBC(text);
        return base.Parse(text);
    }
}