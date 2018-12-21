EventContext  = FairyGUI.EventContext
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
用于继承FairyGUI的Window类，例如
WindowBase = fgui.window_class()
同时派生的Window类可以被继续被继承，例如
MyWindow = fgui.window_class(WindowBase)
可以重写的方法有（与Window类里的同名方法含义完全相同）
OnInit
DoHideAnimation
DoShowAnimation
OnShown
OnHide
]]
function fgui.window_class(base)
    local o = {}

    local base = base or FairyGUI.LuaWindow
    setmetatable(o, base)

    o.__index = o
    o.base = base

    o.New = function(...)
        local t = {}
        setmetatable(t, o)

        local ins = FairyGUI.LuaWindow.New()
        tolua.setpeer(ins, t)
        ins:ConnectLua(t)
        ins.EventDelegates = {}
        if t.ctor then
            t.ctor(ins,...)
        end

        return ins
    end

    return o
end

--[[
注册组件扩展，例如
fgui.register_extension(UIPackage.GetItemURL("包名","组件名"), my_extension)
my_extension的定义方式见fgui.extension_class
]]
function fgui.register_extension(url, extension)
	local base = extension.base
	if base==GComponent then base=FairyGUI.GLuaComponent
		elseif base==GLabel then base=FairyGUI.GLuaLabel
			elseif base==GButton then base=FairyGUI.GLuaButton
				elseif base==GSlider then base=FairyGUI.GLuaSlider
					elseif base==GProgressBar then base=FairyGUI.GLuaProgressBar
						elseif base==GComboBox then base=FairyGUI.GLuaComboBox
						else
							print("invalid extension base: "..base)
							return
						end
	FairyGUI.LuaUIHelper.SetExtension(url, typeof(base), extension.Extend)
end

--[[
用于继承FairyGUI原来的组件类，例如
MyComponent = fgui.extension_class(GComponent)
function MyComponent:ctor() --当组件构建完成时此方法被调用
	print(self:GetChild("n1"))
end
]]
function fgui.extension_class(base)
	local o = {}
	o.__index = o

	o.base = base or GComponent

	o.Extend = function(ins)
		local t = {}
	    setmetatable(t, o)
	    tolua.setpeer(ins,t)
	    ins.EventDelegates = {}
	    if t.ctor then
	    	t.ctor(ins)
	   	end
	    
	    return t
	end

	return o
end
