一、安装

1、将以下语句添加到Assets\uLua\Editor\WrapFile.cs适当的位置，然后调用Lua的菜单Gen Lua Wrap Files，重新生成绑定文件。

_GT(typeof(EventContext)),
_GT(typeof(EventDispatcher)),
_GT(typeof(EventListener)),
_GT(typeof(InputEvent)),
_GT(typeof(DisplayObject)),
_GT(typeof(Container)),
_GT(typeof(Stage)),
_GT(typeof(FairyGUI.Controller)),
_GT(typeof(GObject)),
_GT(typeof(GGraph)),
_GT(typeof(GGroup)),
_GT(typeof(GImage)),
_GT(typeof(GLoader)),
_GT(typeof(PlayState)),
_GT(typeof(GMovieClip)),
_GT(typeof(TextFormat)),
_GT(typeof(GTextField)),
_GT(typeof(GRichTextField)),
_GT(typeof(GTextInput)),
_GT(typeof(GComponent)),
_GT(typeof(GList)),
_GT(typeof(GRoot)),
_GT(typeof(GLabel)),
_GT(typeof(GButton)),
_GT(typeof(GComboBox)),
_GT(typeof(GProgressBar)),
_GT(typeof(GSlider)),
_GT(typeof(PopupMenu)),
_GT(typeof(ScrollPane)),
_GT(typeof(Transition)),
_GT(typeof(UIPackage)),
_GT(typeof(Window)),
_GT(typeof(GObjectPool)),
_GT(typeof(Relations)),
_GT(typeof(RelationType)),

2、拷贝FairyGUI.lua到你的lua文件存放目录。

二、使用

1、普通方法的侦听和删除侦听
require 'FairyGUI'

function OnClick() --也可以带上事件参数，OnClick(context)
	print('you click')
end

UIPackage.AddPackage('Demo')
local view = UIPackage.CreateObject('Demo', 'DemoMain')
GRoot.inst:AddChild(view)

view.onClick:Add(OnClick)
--view.onClick:Remove(OnClick)
	
2、类方法的侦听和删除侦听
require 'FairyGUI'

TestClass = class('TestClass', {})
function TestClass:ctor()
	UIPackage.AddPackage('Demo')
	self.view = UIPackage.CreateObject('Demo', 'DemoMain')
	GRoot.inst:AddChild(self.view)

	self.view.onClick:Add(TestClass.OnClick, self)
	--self.view.onClick:Remove(TestClass.OnClick, self)
end

function TestClass:OnClick() --也可以带上事件参数，TestClass:OnClick(context)
	print('you click')
end

TestClass.New()

3、如果要使用Tween，你可以直接使用GObject.TweenXXXX系列函数，免除了Wrap DOTween的麻烦。
