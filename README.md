FairyGUI Unity SDK
====

A flexible UI framework for Unity3D, working with the FREE professional Game UI Editor: FairyGUI Editor.  
Download the editor from here: [www.fairygui.com](http://www.fairygui.com/download)  

FairyGUI UI编辑器 操作简单，使用习惯与Adobe系列软件保持一致，策划和美术设计师可以轻松上手。在编辑器即可组合各种复杂UI组件，以及为UI设计动画效果，无需编写任何代码。可一键导出到Unity，Starling，Egret， LayaAir，Flash等多个主流应用和游戏平台。<br>
下载UI编辑器：[www.fairygui.com](http://www.fairygui.com/download)

与NGUI、UGUI等传统Unity UI引擎相比，FairyGUI使用一种更接近设计师的思维方式去重新定义UI的制作过程，大大减轻了程序员在制作UI需要投入的时间。<br>
在运行效率方面，FairyGUI对DrawCall优化使用了特有的`FairyBatching`技术，相比NGUI、UGUI的传统优化技术更加高效而且容易控制，特别是对动静耦合越来越复杂的UI设计更是应付自如。<br>
在功能方面，FairyGUI对传统UI制作痛点都有很好的内置支持，例如`图文混排`（包括文字和动画混排），`表情输入`（直接支持键盘上的表情），`虚拟列表`、`循环列表`，`像素级点击检测`，`曲面 UI`, `手势`，`粒子和模型穿插UI`，`打字效果`等。<br>
FairyGUI还对所有输入方式进行了完整的封装，无论是鼠标、单点触摸、多点触摸还是VR手柄输入，开发者都可以使用相同的代码处理交互。<br>

Get Started
====
- Examples.Unity4  
如果你第一次探索FairyGUI，强烈建议你从Examples工程开始。这是适用于Unity4.x版本的工程(这里使用的Unity版本是4.6.6)。

- Examples.Unity5  
如果你第一次探索FairyGUI，强烈建议你从Examples工程开始。这是适用于Unity5.x版本的工程(这里使用的Unity版本是5.2.1)。

- Source  
这里是FairyGUI的源码，如果用于新的项目，这里都是必需的文件。另外，你还需要放置DOTween的DLL。  
这里的源码和着色器都是Unity所有版本通用的。

- LuaSupport  
FairyGUI对Lua十分友好，如果你使用Lua开发Unity游戏，这里为你准备了非常便利的支持。


Learn
====

- 下载FairyGUI编辑器。  
- 下载Examples工程，浏览一遍FairyGUI的例子。  
- 解压Examples工程UIProject目录下的UIProject.zip，使用FairyGUI编辑器打开，结合例子浏览一遍UI工程。  
- 观看视频教程：[FairyGUI游戏UI开发基础](http://www.taikr.com/course/446)  
- 阅读文字教程：[FairyGUI教程](http://www.fairygui.com/tutorial)  
- 进群与小伙伴一起交流：434866637  

License
====
MIT 你可以自由使用FairyGUI在你的商业和非商业项目。  
如果觉得FairyGUI好用，请在Unity商店购买支持作者。[Unity AssetStore](http://u3d.as/kX8)


Version History
====
1.8.0
- NEW: Add DisplayObject.cacheAsBitmap.
- NEW: Add new URL format support: ui://packageName/resourceName.
- NEW: Add HTML syntax <p align=''> and UBB syntax [align=] features.
- NEW: Add GList.onRightClickItem TreeView.onRightClickNode
- NEW: Add GObject.MakeFullScreen.
- NEW: Add GLabel color control.
- NEW: StrokeColor is now involved in color control.
- IMPROVE: Improve ScrollPane user experience.
- IMRPOVE: Improve fairy batching performance.
- IMRPOVE: Change GProgressBar.max/value and GSlider.max/value value type from float to double.
- FIXED: A text clipping bug.

1.7.5
- NEW: Add LongPressGesture.holdRangeRadius
- IMRPOVE: Improve game object manage strategy, display object will not hide its game object in hierarchy when it is not in display list now. That makes easier to track objects life cycle.
- IMPROVE: Make text crisp when content is scaled.
- FIXED: GList.scrollToView failed when the list is virtual and loop.
- FIXED: Allow virtual list to enable scrolling in two directions.

1.7.4
- IMPROVE: Add i18n support for gearText.
- FIXED: A bitmap font loading bug.
- FIXED: If font texture changed while TypingEffect is running, TypingEffect will continue to play normally.

1.7.3
- NEW: Add UIConfig.defaultTouchScrollSpeedRatio.
- FIXED: Potential memory leak problem of virutal list.
- FIXED: Inertia some time get disabled of scrollpane.
- IMPROVE: A complicated case of relation system.

1.7.2
- NEW: Add GoWrapper.wrapTarget.
- FIXED: Small displacement of text rendering on Unity 5.4 or later.
- FIXED: Movieclip playing in runtime is a little slower than in editor.

1.7.1
- NEW: Add ListLayoutType.Pagination.
- NEW: Add FillType.ScaleMatchWidth and FillType.ScaleMatchHeight.
- FIXED: Transitions on UIPanel is not stopped after UIPanel is destroyed.

1.7.0
- NEW: Add UIContentScaler.ScaleMode.ConstantPhysicalSize
- NEW: Add GGraph color gear support.
- NEW: Add ColorFilter/Skew transition.
- NEW: Add GList.align and GList.verticalAlign.
- NEW: Add Transition.timeScale.
- NEW: Add GObject.OnDragMove.
- NEW: Add GObject.OnGearStop.
- NEW: Add GComboBox.icons.
- NEW: Add new TextField auto size type: Shrink.
- NEW: Add Image.tileGridIndice to support tile in scale8grids.
- NEW: Add TypingEffect.
- NEW: Fixed ComboBox icon display.
- IMPROVE: Improve UIPanel to support dropping 3d content on it.
- IMPROVE: Improve ScrollPane to support embed scrolling.
- FIXED: BMFont display bug.

1.6.0
- NEW: Add ScrollPane.onPullDownRelease/ScrollPane.onPullUpRelease event.
- NEW: UIPackage.CreateObjectAsync for creating object asynchronously.
- NEW: Support GLabel input options.
- NEW: Add GearText and GearIcon.
- IMPROVE: Optimize speed and GC usage of UI construction.
- IMPROVE: Refactor TextField. Added InputTextField.
- IMPROVE: Add BlendMode.Off, Remove Image(Opaque) Shader.
- FIXED: Fixed wrong caret position bug of input text.
- FIXED: Call InvalidateBatchingState on progress changing of ProgressBar/Slider.
- FIXED: Pixel snapping bug in relation system.

1.5.3
- FIXED: A Bug in multi-language file parsing.
- FIXED: Failed to change scroll positions in onScroll event handler.
- IMPROVE: Tween effects now available for virtual lists.
- IMPROVE: Access input event is allowed in onItemClick event handler.

1.5.2
- IMPROVE: Optimize fairy batching.
- NEW: Add GComboBox.UpdateDropdownList.
- FIXED: Mistake to hide objects when using stencil mask.

1.5.1
- NEW: Add InputEvent.button for pc platforms.
- FIXED: Text clipping.
- FIXED: UIPanel/UIPainter bug of handling GameObject enable/disable.

1.5.0
- NEW: Add IOS emoji input support
- IMPROVE: Virtual list now supports seperate item height and resource.

1.4.4
- NEW: Add GObject.pixelSnapping
- NEW: Add ScrollPane.inertiaDisabled

1.4.3
- NEW: Add gradient text support.
- NEW: Add GList.scrollItemToViewOnClick.
- NEW: Add DynamicFont.customBoldAndItalic.
- FIXED: GTextInput.maxLength.
- IMPROVE: Update DragDropManager to support custom loader.

1.4.2
- FIXED: A UI Scale bug.
- IMPROVE: Gear handling.

1.4.1
- NEW: Add text shadow support.
- NEW: Add anchor support.

1.4.0
- NEW: Add GObject.BlendMode.
- NEW: Add GObject.Filter.
- NEW: Add GObject.Skew.

1.3.3
- NEW: Add gesture support

1.3.2
- NEW: Add free mask support.

1.3.1
- NEW: Add Tween delay to gears.
- FIX: GTextInput bug.
- IMPROVE: Remove white space in shader file name.

1.3.0
- NEW: New UIPainter component. Curve UI now supported.
- NEW: New list feature: page mode.
- NEW: New render order feature for GComponent: GComponent.childrenRenderOrder.
- NEW: Html parsing enhanced.
- NEW: Add Window.bringToFontOnClick and UIConfig.bringWindowToFrontOnClick.
- NEW: Add GTextInput.keyboardType
- REMOVE: GObject.SetPivotByRatio. Use SetPivot instead. 

1.2.3
- NEW: Add UIConfig.allowSoftnessOnTopOrLeftSide
- IMPROVE: UI Shaders.
- IMPROVE: Add GImage/GLoader/GMovieClip.shader/material

1.2.2
- NEW: Add "FairyGUI/UI Config" Component
- FIX: Pixel perfect issues.
- FIX: Change GMovieClip to keep its speed even time scale changed.

1.2.1
- NEW: Sort UIPanel by z order is supported. See Stage.SortWorldSpacePanelsByZOrder.
- IMPROVE: Image FillMethod implementation. FillImage shader is removed.
- IMPROVE: Shader optimization.

1.2.0
- NEW: Added editor extensions: UIPanel/UIScaler/StageCamera.
- NEW: Added FillMethod to image/loader.
- IMPROVE: Refactor shaders.
- IMPROVE: Refactor some classes.
- REMOVE: StageNode. Use UIPanel instead.
- REMOVE: Stage prefab is removed, you don't need to put this into scene now.

1.1.0
- NEW: Added virtual list and loop list support. See GList.SetVirutal/GList.SetVirtualAndLoop
- NEW: Added StageNode. For displaying UI in perspective camera.
- NEW: Added GObject.SetPivotByRatio.
- FIX: GTextField.maxLength now working correctly.
- REMOVE: MiniStage. Use StageNode instead.
- REMOVE: GComponent.onScroll. Use GComponent.scrollPane.onScroll instead.

1.0.3
- NEW: Now you can have different scrollbar styles for each component.
- NEW: Added fixed grip size option for scrollbar.
- NEW: Added tween value behavior for progressbar.
- NEW: Added Transition.playReverse.
- FIX: Scroll event bug.
- FIX: Large moveclip performance.

1.0.2
- NEW: Added Controller.autoRadioGroupDepth (UI Editor function)
- NEW: Added GTextInput.promptText (UI Editor function)
- FIX: Change to create MobileInputAdapter statically to avoid il2cpp problem.

1.0.1
- NEW: Added EventBridge to improve Event system.
- NEW: Added Container.touchChildren. 
- FIX: RenderingOrder will now handle properly when display list changing frequently.
- FIX: Use GRoot.scaleX/Y for content scaling, removed the redundant code.
