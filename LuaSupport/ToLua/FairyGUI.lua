EventContext = FairyGUI.EventContext
EventListener = FairyGUI.EventListener
EventDispatcher = FairyGUI.EventDispatcher
InputEvent = FairyGUI.InputEvent
NTexture = FairyGUI.NTexture
Container = FairyGUI.Container
Image = FairyGUI.Image
Stage = FairyGUI.Stage
Controller = FairyGUI.Controller
GObject = FairyGUI.GObject
GGraph = FairyGUI.GGraph
GGroup = FairyGUI.GGroup
GImage = FairyGUI.GImage
GLoader = FairyGUI.GLoader
GMovieClip = FairyGUI.GMovieClip
TextFormat = FairyGUI.TextFormat
GTextField = FairyGUI.GTextField
GRichTextField = FairyGUI.GRichTextField
GTextInput = FairyGUI.GTextInput
GComponent = FairyGUI.GComponent
GList = FairyGUI.GList
GRoot = FairyGUI.GRoot
GLabel = FairyGUI.GLabel
GButton = FairyGUI.GButton
GComboBox = FairyGUI.GComboBox
GProgressBar = FairyGUI.GProgressBar
GSlider = FairyGUI.GSlider
PopupMenu = FairyGUI.PopupMenu
ScrollPane = FairyGUI.ScrollPane
Transition = FairyGUI.Transition
UIPackage = FairyGUI.UIPackage
Window = FairyGUI.Window
GObjectPool = FairyGUI.GObjectPool
Relations = FairyGUI.Relations
RelationType = FairyGUI.RelationType
UIPanel = FairyGUI.UIPanel
UIPainter = FairyGUI.UIPainter
TypingEffect = FairyGUI.TypingEffect
GTween = FairyGUI.GTween
GTweener = FairyGUI.GTweener
EaseType = FairyGUI.EaseType

fgui = {}

--[[
用于继承FairyGUI的Window类，同时派生的Window类可以继续被继承。可以重写的方法有（与Window类里的同名方法含义完全相同）
OnInit、DoHideAnimation、DoShowAnimation、OnShown、OnHide。

例子：
MyWinClass = fgui.window_class()

function MyWinClass:ctor()
    print('MyWinClass-ctor')
    self.contentPane = UIPackage.CreateObject("Basics", "WindowA")
end

function MyWinClass:OnShown()
    print('MyWinClass-onShown')
end

local win = MyWinClass.New()
win:Show()

]]
function fgui.window_class(base)
    local o = {}

    local base = base or FairyGUI.Window
    setmetatable(o, base)

    o.__index = o
    o.base = base

    o.New = function(...)
        local t = {}
        setmetatable(t, o)

        local ins = FairyGUI.Window.New()
        tolua.setpeer(ins, t)
        ins:SetLuaPeer(t)
        if t.ctor then t.ctor(ins, ...) end

        return ins
    end

    return o
end

--[[
注册组件扩展，用于继承FairyGUI原来的组件类。

例子：

MyButton = fgui.extension_class(GButton)
fgui.register_extension("ui://包名/我的按钮", MyButton)

function MyButton:ctor() --当组件构建完成时此方法被调用
    print(self:GetChild("n1"))
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

local myButton = someComponent:GetChild("myButton") --这个myButton的资源是“我的按钮”
myButton:Test()
myButton.myProp = 'hello'

local myButton2 = UIPackage.CreateObject("包名","我的按钮")
myButton2:Test()
myButton2.myProp = 'world'
]]

function fgui.register_extension(url, extension)
    FairyGUI.UIObjectFactory.SetExtension(url, typeof(extension.base),
                                          extension.Extend)
end

function fgui.extension_class(base)
    local o = {}
    o.__index = o

    o.base = base or GComponent

    o.Extend = function(ins)
        local t = {}
        setmetatable(t, o)
        tolua.setpeer(ins, t)
        return t
    end

    return o
end
