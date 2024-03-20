var WebGLTextInput = {
    $canvasEle: null,
    $containerEle: null,
    $inputEle: null,
    $textareaEle: null,
    $callbacks: null,

    WebGLTextInputInit: function (onInputCallback, onBlurCallback) {
        callbacks = { onInputCallback: onInputCallback, onBlurCallback: onBlurCallback };
        canvasEle = document.getElementById('unity-canvas');
    },

    WebGLTextInputSetText: function (text, multiline, color, fontSize, fontFace, maxLength) {
        if (containerEle == null) {
            InitInput(inputEle = document.createElement("input"));
            InitInput(textareaEle = document.createElement("textarea"));

            containerEle = document.createElement("div");
            containerEle.style.cssText = "transform-origin:0 0;-webkit-transform-origin:0 0;-moz-transform-origin:0 0;-o-transform-origin:0 0;";
            containerEle.style.position = "absolute";
            containerEle.style.zIndex = '1E5';
            containerEle.name = "WebGLTextInput";
            canvasEle.parentElement.appendChild(containerEle);
        }

        inputEle.parentElement && (containerEle.removeChild(inputEle));
        textareaEle.parentElement && (containerEle.removeChild(textareaEle));

        var current = multiline ? textareaEle : inputEle;
        containerEle.appendChild(current);
        containerEle.style.display = "";

        current.maxLength = maxLength <= 0 ? 1E5 : maxLength;
        current.value = UTF8ToString(text);
        current.style.color = color;
        current.style.fontSize = fontSize + 'px';
        current.style.fontFamily = fontFace;
        current.focus();
    },

    WebGLTextInputShow: function (x, y, width, height, scaleX, scaleY) {
        containerEle.style.top = y + "px";
        containerEle.style.left = x + "px";
        containerEle.style.width = width + "px";
        containerEle.style.height = height + "px";
        containerEle.style.transform = "scale(" + scaleX + "," + scaleY + ")";
    },

    WebGLTextInputHide: function () {
        containerEle.style.display = "none";
    },


    $InitInput: function (input) {
        var style = input.style;
        style.position = "absolute";
        style.width = "100%";
        style.height = "100%";
        style.overflow = "hidden";
        style.resize = 'none';
        style.backgroundColor = 'transparent';
        style.border = 'none';
        style.outline = 'none';

        var t = callbacks;
        input.addEventListener('input', function () {
            var returnStr = input.value;
            var bufferSize = lengthBytesUTF8(returnStr) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(returnStr, buffer, bufferSize);
            dynCall("vi", t.onInputCallback, [buffer]);
        });

        input.addEventListener('blur', function () {
            if (input.parentElement)
                input.parentElement.style.display = "none";
            dynCall("v", t.onBlurCallback);
        });

        if (input.tagName == "INPUT") {
            input.addEventListener('keydown', function (e) {
                if ((e.which && e.which === 13) || (e.keyCode && e.keyCode === 13)) {
                    e.preventDefault();
                    input.blur();
                }
            });
        }

        var stopEvent = function (e) {
            if (e.type == 'touchmove')
                e.preventDefault();
            e.stopPropagation && e.stopPropagation();
        }

        input.addEventListener('mousemove', stopEvent, { passive: false });
        input.addEventListener('mousedown', stopEvent, { passive: false });
        input.addEventListener('touchmove', stopEvent, { passive: false });
    },
};

autoAddDeps(WebGLTextInput, '$canvasEle');
autoAddDeps(WebGLTextInput, '$containerEle');
autoAddDeps(WebGLTextInput, '$inputEle');
autoAddDeps(WebGLTextInput, '$textareaEle');
autoAddDeps(WebGLTextInput, "$callbacks");
autoAddDeps(WebGLTextInput, '$InitInput');
mergeInto(LibraryManager.library, WebGLTextInput);