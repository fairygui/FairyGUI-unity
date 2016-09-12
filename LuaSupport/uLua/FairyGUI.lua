luanet.load_assembly('Assembly-CSharp')

EventContext  = FairyGUI.EventContext
EventListener = FairyGUI.EventListener
EventDispatcher = FairyGUI.EventDispatcher
InputEvent = FairyGUI.InputEvent
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

--这里建立一个c# delegate到lua函数的映射，是为了支持self参数，和方便地进行remove操作
local EventDelegates = {}
setmetatable(EventDelegates, {__mode = "k"})
local function GetDelegate(func, obj, createIfNone)
	local mapping
	if obj~=nil then
		mapping = obj.EventDelegates
		if mapping==nil then
			mapping = {}
			setmetatable(mapping, {__mode = "k"})
			obj.EventDelegates = mapping
		end
	else
		mapping = EventDelegates
	end

	local delegate = mapping[func]
	if createIfNone and delegate==nil then
		if obj~=nil then
			delegate = DelegateFactory.FairyGUI_EventCallback1(function(context)
				func(obj,context)
			end)
		else
			delegate = DelegateFactory.FairyGUI_EventCallback1(func)
		end
		mapping[func] = delegate
	end

	return delegate
end

--将EventListener.Add和EventListener.Remove重新进行定义，以适应lua的使用习惯
local EventListener_mt = getmetatable(EventListener)
local oldListenerAdd = rawget(EventListener_mt, 'Add')
local oldListenerRemove = rawget(EventListener_mt, 'Remove')
local oldListenerAddCapture = rawget(EventListener_mt, 'AddCapture')
local oldListenerRemoveCapture = rawget(EventListener_mt, 'RemoveCapture')

local function ListenerAdd(listener, func, obj)
	local delegate = GetDelegate(func, obj, true)
	oldListenerAdd(listener, delegate)
end

local function ListenerRemove(listener, func, obj)
	local delegate = GetDelegate(func, obj, false)
	if delegate ~= nil then 
		oldListenerRemove(listener, delegate)
	end
end

local function ListenerAddCapture(listener, func, obj)
	local delegate = GetDelegate(func, obj, true)
	oldListenerAddCapture(listener, delegate)
end

local function ListenerRemoveCapture(listener, func, obj)
	local delegate = GetDelegate(func, obj, false)
	if delegate ~= nil then 
		oldListenerRemoveCapture(listener, delegate)
	end
end

rawset(EventListener_mt, 'Add', ListenerAdd)
rawset(EventListener_mt, 'Remove', ListenerRemove)
rawset(EventListener_mt, 'AddCapture', ListenerAddCapture)
rawset(EventListener_mt, 'RemoveCapture', ListenerRemoveCapture)

--将EventDispatcher.AddEventListener和EventDispatcher.RemoveEventListener重新进行定义，以适应lua的使用习惯
local EventDispatcher_mt = getmetatable(EventDispatcher)
local oldAddEventListener = rawget(EventDispatcher_mt, 'AddEventListener')
local oldRemoveEventListener = rawget(EventDispatcher_mt, 'RemoveEventListener')

local function AddEventListener(dispatcher, name, func, obj)
	local delegate = GetDelegate(func, obj, true)
	oldAddEventListener(dispatcher, name, delegate)
end

local function RemoveEventListener(dispatcher, name, func, obj)
	local delegate = GetDelegate(func, obj, false)
	if delegate ~= nil then 
		oldRemoveEventListener(dispatcher, name, delegate)
	end
end

rawset(EventDispatcher_mt, 'AddEventListener', AddEventListener)
rawset(EventDispatcher_mt, 'RemoveEventListener', RemoveEventListener)