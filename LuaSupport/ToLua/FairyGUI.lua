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
PlayState = FairyGUI.PlayState
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

--[[
FairyGUI自带的定时器，有需要可以使用。例如：
每秒回调，无限次数
fgui.add_timer(1,0,callback)
可以带self参数
fgui.add_timer(1,0,callback,self)
可以自定义参数
fgui.add_timer(1,0,callback,self,data)

！！警告，定时器回调函数不要与UI事件回调函数共用
]]
function fgui.add_timer(interval, repeatCount, func, obj, param)
	local callbackParam
	if param~=nil then
		if obj==nil then
			callbackParam=param
		else
			callbackParam=obj
			func = function(p)
				func(p, param)
			end
		end
	end

	local delegate = fgui_internal.GetDelegate(func, obj, true, FairyGUI.TimerCallback)
	FairyGUI.Timers.inst:Add(interval, repeatCount, delegate, callbackParam)
end

function fgui.remove_timer(func, obj)
	local delegate = fgui_internal.GetDelegate(func, obj, false)
	if delegate~=nil then
		FairyGUI.Timers.inst:Remove(delegate)
	end
end

---以下是内部使用的代码---
fgui_internal = {}

--这里建立一个c# delegate到lua函数的映射，是为了支持self参数，和方便地进行remove操作
fgui_internal.EventDelegates = {}
setmetatable(fgui_internal.EventDelegates, {__mode = "k"})
function fgui_internal.GetDelegate(func, obj, createIfNone, delegateType)
	local mapping
	if obj~=nil then
		mapping = obj.EventDelegates
		if mapping==nil then
			mapping = {}
			setmetatable(mapping, {__mode = "k"})
			obj.EventDelegates = mapping
		end
	else
		mapping = fgui_internal.EventDelegates
	end

	local delegate = mapping[func]
	if createIfNone and delegate==nil then
		local realFunc
		if obj~=nil then
			realFunc = function(context)
				func(obj,context)
			end
		else
			realFunc = func
		end
		delegateType = delegateType or FairyGUI.EventCallback1
		delegate = delegateType(realFunc)

		mapping[func] = delegate
	end

	return delegate
end

--将EventListener.Add和EventListener.Remove重新进行定义，以适应lua的使用习惯
local EventListener_mt = getmetatable(EventListener)
local oldListenerAdd = rawget(EventListener_mt, 'Add')
local oldListenerRemove = rawget(EventListener_mt, 'Remove')
local oldListenerSet = rawget(EventListener_mt, 'Set')
local oldListenerAddCapture = rawget(EventListener_mt, 'AddCapture')
local oldListenerRemoveCapture = rawget(EventListener_mt, 'RemoveCapture')

local function ListenerAdd(listener, func, obj)
	local delegate = fgui_internal.GetDelegate(func, obj, true)
	oldListenerAdd(listener, delegate)
end

local function ListenerRemove(listener, func, obj)
	local delegate = fgui_internal.GetDelegate(func, obj, false)
	if delegate ~= nil then 
		oldListenerRemove(listener, delegate)
	end
end

local function ListenerSet(listener, func, obj)
	if func==nil then
		oldListenerSet(listener, nil)
	else
		local delegate = fgui_internal.GetDelegate(func, obj, true)
		oldListenerSet(listener, delegate)
	end
end

local function ListenerAddCapture(listener, func, obj)
	local delegate = fgui_internal.GetDelegate(func, obj, true)
	oldListenerAddCapture(listener, delegate)
end

local function ListenerRemoveCapture(listener, func, obj)
	local delegate = fgui_internal.GetDelegate(func, obj, false)
	if delegate ~= nil then 
		oldListenerRemoveCapture(listener, delegate)
	end
end

rawset(EventListener_mt, 'Add', ListenerAdd)
rawset(EventListener_mt, 'Remove', ListenerRemove)
rawset(EventListener_mt, 'Set', ListenerSet)
rawset(EventListener_mt, 'AddCapture', ListenerAddCapture)
rawset(EventListener_mt, 'RemoveCapture', ListenerRemoveCapture)

--将EventDispatcher.AddEventListener和EventDispatcher.RemoveEventListener重新进行定义，以适应lua的使用习惯
local EventDispatcher_mt = getmetatable(EventDispatcher)
local oldAddEventListener = rawget(EventDispatcher_mt, 'AddEventListener')
local oldRemoveEventListener = rawget(EventDispatcher_mt, 'RemoveEventListener')

local function AddEventListener(dispatcher, name, func, obj)
	local delegate = fgui_internal.GetDelegate(func, obj, true)
	oldAddEventListener(dispatcher, name, delegate)
end

local function RemoveEventListener(dispatcher, name, func, obj)
	local delegate = fgui_internal.GetDelegate(func, obj, false)
	if delegate ~= nil then 
		oldRemoveEventListener(dispatcher, name, delegate)
	end
end

rawset(EventDispatcher_mt, 'AddEventListener', AddEventListener)
rawset(EventDispatcher_mt, 'RemoveEventListener', RemoveEventListener)
