Lua支持
====

## 准备工作

1. **如果你使用的是ToLua，在Scripting Define Symbols里加上** `FAIRYGUI_TOLUA`。XLUA则不需要。
2. 如果是ToLua，将以下语句添加到CustomSettings.cs适当的位置，然后重新生成绑定文件。

  ```csharp

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
    _GT(typeof(Timers)),
    _GT(typeof(GTween)),
    _GT(typeof(GTweener)),
    _GT(typeof(EaseType)),
    _GT(typeof(TweenValue)),
    _GT(typeof(UIObjectFactory)),
  ```

3. 将FairyGUI.lua放入你的lua文件存放目录。

## 监听事件

1. 普通方法的侦听和删除侦听

  ```csharp

    require 'FairyGUI'
    
    function OnClick()
    	print('you click')
    end
    
    --也可以带上事件参数
    function OnClick(context)
    	print('you click'..context.sender)
    end
    
    UIPackage.AddPackage('Demo')
    local view = UIPackage.CreateObject('Demo', 'DemoMain')
    GRoot.inst:AddChild(view)
    
    view.onClick:Add(OnClick)
    --view.onClick:Remove(OnClick)
    --view.onClick:Set(OnClick)

  ```
	
2. **ToLua支持带self的回调**

  ```csharp

    function TestClass:OnClick()
    	print('you click')
    end
    
    function TestClass:OnClick(context)
    	print('you click'..context.sender)
    end
    
    self.view.onClick:Add(TestClass.OnClick, self)
    self.view.onClick:Remove(TestClass.OnClick, self)

  ```

## 使用Window类

FairyGUI提供的Window类，一般需要开发者自己扩展，例如覆盖OnShown,OnHide等。在Lua里，编写Window扩展的方法是：

```csharp
    WindowBase = fgui.window_class()
    
    --构建函数
    function WindowBase:ctor()
    end
    
    --可覆盖的函数(可选，不是说必须）
    function WindowBase:OnInit()
        self.contentPane = UIPackage.CreateObject("Basics", "WindowA");
    end
    
    function WindowBase:OnShown()
    end
    
    function WindowBase:OnHide()
    end
    
    function WindowBase:DoShowAnimation()
        self:OnShown();
    end
    
    function WindowBase:DoHideAnimation()
        self:HideImmediately();
    end

    --创建并显示窗口
    local win = WindowBase.New();
    win:Show();
    
    也可以继续继承以上得到的Window类，例如:
    MyWindow = fgui.window_class(WindowBase)
    
    在继承类里调用父类方法的:
    function MyWindow:OnInit()
    	WindowBase.OnInit(self)
    end
```

## 自定义扩展

FairyGUI在C#里可以使用`UIObjectFactory.SetPackageItemExtension`进行自定义扩展。在Lua里，同样可以这样做。方法如下：

1. 定义扩展类。注意基础类型，不要搞错。例如按钮是GButton，一般的组件则是GComponent。

  ```csharp

    MyButton = fgui.extension_class(GButton)
    
    --注意这里不是构造函数，是当组件已经构建完毕后调用的

    function MyButton:ctor()
    	print(self:GetChild('n1'))
    end
    
    --添加自定义的方法和字段
    function MyButton:Test()
    	print('test')
    end
    
    local get = tolua.initget(MyButton)
    local set = tolua.initset(MyButton)
    get.myProp = function(self)
    	return self._myProp
    end
    
    set.myProp = function(self, value)
    	self._myProp = value
    	self:GetChild('n1').text = value
    end
  
  ```

2. 注册扩展类。要在创建任何对象前注册好。

  ```csharp
    fgui.register_extension("ui://包名/我的按钮", MyButton)
  ```

3. 完成以上两步后，任何“我的按钮”这个资源创建出来的对象都可以使用MyButton访问了。例如：

  ```csharp

    local myButton = someComponent:GetChild("myButton") --这个myButton的资源是“我的按钮”
    myButton:Test()
    myButton.myProp = 'hello'
    
    local myButton2 = UIPackage.CreateObject("包名","我的按钮")
    myButton2:Test()
    myButton2.myProp = 'world'

  ```