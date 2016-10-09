var utils = {}; 

utils.distanceBetween = function (point1, point2) {
    return Math.sqrt(Math.pow(point2.x - point1.x, 2) + Math.pow(point2.y - point1.y, 2));
}
utils.angleBetween = function (point1, point2) {
    return Math.atan2(point2.x - point1.x, point2.y - point1.y);
}
utils.toHslaString = function (hsla) {
    return 'hsla(' + hsla.h + ',' + (hsla.s * 100) + '%,' + (hsla.l * 100) + '%,' + hsla.a + ')'
}
utils.fromHslaString = function (input) {
    input = input.substr(input.indexOf('(') + 1);
    input = input.substring(0, input.length - 1);
    var s = input.split(',');
    var col = {
        h: parseInt(s[0].trim()),
        s: parseFloat(s[1].substr(0, s[1].indexOf('%')).trim()) / 100,
        l: parseFloat(s[2].substr(0, s[2].indexOf('%')).trim()) / 100,
        a: parseFloat(s[3].trim())
    };
}

utils.fromRgbString = function (input) {
    input = input.substr(input.indexOf('(') + 1);
    input = input.substring(0, input.length - 1);
    var s = input.split(',');


    var col = {
        r: parseInt(s[0].trim()),
        g: parseInt(s[1].trim()),
        b: parseInt(s[2].trim()),
        a: 255
    };
    return utils.rgbToHsl(col.r, col.g, col.b);
}

utils.rgbToHsl = function (r, g, b) {
    r /= 255, g /= 255, b /= 255;
    var max = Math.max(r, g, b), min = Math.min(r, g, b);
    var h, s, l = (max + min) / 2;

    if (max == min) {
        h = s = 0; // achromatic
    } else {
        var d = max - min;
        s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
        switch (max) {
            case r: h = (g - b) / d + (g < b ? 6 : 0); break;
            case g: h = (b - r) / d + 2; break;
            case b: h = (r - g) / d + 4; break;
        }
        h *= 60;
    }

    return { h: h, s: s, l: l, a: 1 };
}

utils.getRandomInt = function (min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

var math = {};
math.scalePositionScalar = function (val, viewportSize, oldScale, newScale) {
    var newSize, oldSize;
    oldSize = viewportSize * oldScale;
    newSize = viewportSize * newScale;
    return val + (oldSize - newSize) / 2;
};

var Draw = (function () {
    return {
        initialize: function (arg1, arg2) {
            var containerEl, opts;
            opts = null;
            containerEl = null;
            if (arg1 instanceof HTMLElement) {
                containerEl = arg1;
                opts = arg2;
            } else {
                opts = arg1;
            }

            this.onExportImage = opts.onExportImage;
            this.onCanceled = opts.onCanceled;

            this.viewport = document.querySelector("meta[name=viewport]");

            this.viewportScale = 1;
            this.opts = opts || {};
            this.isDrawing = false;
            this.lastPoint = null;
            this.mouseOut = false;
            //this.scale = 1.0;
            this.scaleAtPinchStart = 1.0;
            this.offsetAtPinchStart = { x: 0, y: 0 };
            this.offset = { x: 0, y: 0 };
            this.zoomEnabled = true;
            this.transform = new Transform();

            this.popup = null;

            this.isPinching = false;

            this.canvas = document.createElement('canvas');
            this.ctx = this.canvas.getContext('2d');
            this.ctx.lineJoin = this.ctx.lineCap = 'round';

            this.buffer = document.createElement('canvas');
            this.bufferCtx = this.buffer.getContext('2d');
            this.bufferCtx.lineJoin = this.bufferCtx.lineCap = 'round';


            this.canvas.style['background-color'] = 'white';
            this.canvas.moose = this;
            this.currentShape = null;
            this.shapes = [];
            this.shapeHistory = [];
            this.historyIndex = 0;

            this.canvas.width = this.buffer.width = this.width = opts.width;
            this.canvas.height = this.buffer.height = this.height = opts.height;
            this.position = { x: 0, y: 0 };

            this.buffer.moose = this;
            this.buffer.lineJoin = this.buffer.lineCap = 'round';

            this.toolSize = 10;
            this.foreColor = { h: 100, s: 1, l: 0.5, a: 1 };

            this.debugElement = this.createDebugElement();

            this.buildPalette();

            if (containerEl) {
                this.bindToElement(containerEl);
                this.toolbar = this.createToolbar();
                this.containerEl.appendChild(this.toolbar);
            }

            this.bindEvents();

            var scalex = 1600 / screen.availWidth;
            var scaley = 900 / screen.availHeight;
            this.scale = scalex;
            if (scalex * screen.availHeight > 900) {
                this.scale = scaley;
            }
            if (this.scale < 1) {
                this.scale = 1;
            }
            this.canvas.width = 1600;
            this.canvas.height = 900;
            //this.ctx.scale(1 / this.scale, 1 / this.scale);

            if (this.zoomEnabled) {
                this.bindHammerTime(this);
            }
        },
        getScale: function() {
            return this.transform.m[0];
        },
        setScale:function(scale) {
            this.transform.scale(scale);
            var m = this.transform.m;
            this.ctx.transform(m[0], m[1], m[2], m[3], m[4], m[5]);
        },
        setDocumentViewportScale: function (scale) {
            this.viewport.setAttribute('content', 'width=device-width, initial-scale=' + scale);
        },
        bindToElement: function (containerEl) {
            var ref1, repaintAll;
            if (this.containerEl) {
                console.warn("Trying to bind to a DOM element more than once is unsupported.");
                return;
            }
            this.containerEl = containerEl;
            this.containerEl.appendChild(this.canvas);
            this.containerEl.style['background-color'] = "#aaaaaa"
            this.containerEl.style['overflow'] = 'hidden';
            this.containerEl.style['position'] = 'absolute';
            this.containerEl.style['top'] = '0px';
            this.containerEl.style['left'] = '0px';
            this.containerEl.style['width'] = this.width;
            this.containerEl.style['height'] = this.height;
            this.containerEl.appendChild(this.debugElement);

            this.isBound = true;
        },
        startDrawing: function (data) {
            var scalex = 1600 / screen.availWidth;
            var scaley = 900 / screen.availHeight;
            var s = scalex;
            if (s * screen.availHeight > 900) {
                s = scaley;
            }
            if (s < 1) {
                s = 1;
            }
            this.setDocumentViewportScale(1 / s);

            this.sessionData = data;
            this.containerEl.style.display = 'block';
            this.enableToolbar();

            this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
            this.bufferCtx.clearRect(0, 0, this.canvas.width, this.canvas.height);

            this.setToolbarPosition('top');
            this.toolbar.className = this.isFullscreen() ? 'toolbar-small' : 'toolbar-big';

            for (var i = 0; i < this.toolbarItems.length; i++) {
                if (this.toolbarItems[i].resetToDefaults) {
                    this.toolbarItems[i].resetToDefaults();
                }
            }

            this.isDrawing = false;
            window.scrollTo(0, 0);

        },
        importDrawingData: function (data) {
            var t = this.selectedTool, ts = this.toolSize, fc = this.foreColor;
            var snapshot = JSON.parse(data);

            if (snapshot && snapshot.length) {
                for (var i = 0; i < snapshot.length; i++) {
                    try {
                        this.shapes.push(snapshot[i]);
                        this.saveShape(snapshot[i], this.ctx);
                        this.saveShape(snapshot[i], this.bufferCtx);
                    } catch (ex) {
                        this.debug(ex.message);
                    }
                }
            }
            this.selectedTool = t;
            this.toolSize = ts;
            this.foreColor = fc;
        },
        close: function () {
            this.isDrawing = false;
            this.containerEl.style.display = 'none';
            this.shapes = [];
            if (this.isFullscreen()) {

                this.exitFullscreen();
            }
        },
        onSave: function () {
            this.disableToolbar();
            if (this.onExportImage) {
                try {
                    this.onExportImage(this.sessionData,
                        this.buffer.toDataURL('image/png'),
                        JSON.stringify(this.shapes));
                    this.close();
                    return;
                } catch (ex) {
                    this.debug(ex.message);
                }
            }
            this.enableToolbar();
        },
        onCancel: function () {
            this.disableToolbar();
            if (this.onCanceled) {
                try {
                    this.onCanceled(this.sessionData);
                    this.close();
                    return;
                } catch (ex) {
                    this.debug(ex.message);
                }
            }
            this.enableToolbar();
        },
        tools: [
            {
                name: 'sprayPaint1',
                iconHtml: '<img src="/content/tool_paint.png" />',
                getToolbarElement: function (isSelected, onSelected) {
                    var $this = this;
                    var el = document.createElement('button');
                    this.el = el;
                    el.innerHTML = this.iconHtml;
                    if (isSelected) {
                        el.style.border = '1px solid red';
                    } else {
                        el.style.border = '1px solid #888';
                    }
                    el.onclick = function (e) {
                        if (onSelected)
                            onSelected($this);
                    };
                    return el;
                },
                onPointerStart: function (moose, pt) {
                    this.lastPoint = pt;
                },
                onPointerDrag: function (moose, pt, targetContext) {
                    var dist = utils.distanceBetween(this.lastPoint, pt);
                    var angle = utils.angleBetween(this.lastPoint, pt);
                    var ctx = targetContext || moose.ctx;

                    var fc = moose.foreColor;

                    var toolSize = pt.toolSize || moose.toolSize;
                    for (var i = 0; i < dist; i += toolSize / 4) {

                        x = this.lastPoint.x + (Math.sin(angle) * i);
                        y = this.lastPoint.y + (Math.cos(angle) * i);

                        var radgrad = ctx
                            .createRadialGradient(x, y, toolSize / 2, x, y, toolSize);

                        var centerColor = { h: fc.h, s: fc.s, l: fc.l, a: fc.a };
                        var midColor = { h: fc.h, s: fc.s, l: fc.l, a: fc.a };
                        var edgeColor = { h: fc.h, s: fc.s, l: fc.l, a: fc.a };
                        centerColor.a = 1;
                        midColor.a = 0.5;
                        edgeColor.a = 0;

                        radgrad.addColorStop(0, utils.toHslaString(centerColor));
                        radgrad.addColorStop(0.5, utils.toHslaString(midColor));
                        radgrad.addColorStop(1, utils.toHslaString(edgeColor));

                        ctx.fillStyle = radgrad;
                        ctx.fillRect(x - toolSize,
                            y - toolSize,
                            toolSize * 2,
                            toolSize * 2);
                    }
                    this.lastPoint = pt;
                    return pt;
                },
                onPointerStop: function (moose) {
                    this.lastPoint = null;
                    moose.addHistory(moose.currentShape);
                    moose.saveShape(moose.currentShape, moose.bufferCtx);
                }
            },
            {
                name: 'ink',
                iconHtml: '<img src="/content/tool_ink.png" />',
                getToolbarElement: function (isSelected, onSelected) {
                    var $this = this;
                    var el = document.createElement('button');
                    this.el = el;
                    el.innerHTML = this.iconHtml;
                    if (isSelected) {
                        el.style.border = '1px solid red';
                    } else {
                        el.style.border = '1px solid #888';
                    }
                    el.onclick = function (e) {
                        if (onSelected)
                            onSelected($this);
                    };
                    return el;
                },
                onPointerStart: function (moose, pt) {
                    this.lastPoint = pt;

                    this.sizeVariation = pt.sizeVariation || (Math.random() * 1) + 0.5;
                    this.sizeChangeWait = pt.sizeChangeWait || utils.getRandomInt(2, 5);
                    this.blotWait = pt.blotWait || utils.getRandomInt(100, 300);
                    this.actualInkSize = pt.actualInkSize || 1;

                },
                onPointerDrag: function (moose, pt, targetContext) {
                    var dist = utils.distanceBetween(this.lastPoint, pt);
                    var angle = utils.angleBetween(this.lastPoint, pt);
                    var ctx = targetContext || moose.ctx

                    var fc = moose.foreColor;
                    this.sizeChangeWait--;
                    this.blotWait--;

                    if (this.sizeChangeWait <= 0) {
                        this.sizeVariation = pt.sizeVariation || Math.random() + 0.5;
                        this.sizeChangeWait = pt.sizeChangeWait || utils.getRandomInt(3, 8);
                    }

                    if (this.blotWait <= 0) {
                        this.sizeVariation = pt.sizeVariation || this.sizeVariation * (2 + Math.random() * 2);
                        this.blotWait = this.blotWait || utils.getRandomInt(100, 300);

                    }

                    var toolSize = moose.toolSize * this.sizeVariation;
                    if (pt.actualInkSize)
                        this.actualInkSize = pt.actualInkSize;
                    else
                        this.actualInkSize += (toolSize - this.actualInkSize) / (this.sizeChangeWait / 2);

                    for (var i = 0; i < dist; i += this.actualInkSize / 4) {

                        x = this.lastPoint.x + (Math.sin(angle) * i);
                        y = this.lastPoint.y + (Math.cos(angle) * i);

                        var radgrad = ctx
                            .createRadialGradient(x, y, this.actualInkSize / 2, x, y, this.actualInkSize);

                        var centerColor = { h: fc.h, s: fc.s, l: fc.l, a: fc.a };
                        var midColor = { h: fc.h, s: fc.s, l: fc.l, a: fc.a };
                        var edgeColor = { h: fc.h, s: fc.s, l: fc.l, a: fc.a };
                        centerColor.a = 1;
                        midColor.a = 0.8;
                        edgeColor.a = 0;

                        radgrad.addColorStop(0, utils.toHslaString(centerColor));
                        radgrad.addColorStop(0.85, utils.toHslaString(midColor));
                        radgrad.addColorStop(1, utils.toHslaString(edgeColor));

                        ctx.fillStyle = radgrad;
                        ctx.fillRect(x - this.actualInkSize,
                            y - this.actualInkSize,
                            this.actualInkSize * 2,
                            this.actualInkSize * 2);
                    }

                    var pointData =
                    {
                        x: pt.x,
                        y: pt.y,
                        sizeChangeWait: this.sizeChangeWait,
                        blotWait: this.blotWait,
                        sizeVariation: this.sizeVariation,
                        actualInkSize: this.actualInkSize
                    };
                    this.lastPoint = pt;
                    return pointData;
                },
                onPointerStop: function (moose) {
                    this.lastPoint = null;
                    moose.addHistory(moose.currentShape);
                    moose.saveShape(moose.currentShape, moose.bufferCtx);
                }
            }
        ],
        toolbarItems: [
            {
                name: 'toggleToolbarSize',
                enabled: true,
                showWhenCollapsed: true,
                collapsed: false,
                initialize: function (moose) {
                    var $this = this;
                    var el = document.createElement('button');
                    el.style.height = '100%';
                    el.style.float = 'left';
                    el.className = 'btn btn-info';
                    el.innerHTML = '<span class="glyphicon glyphicon-chevron-left" aria-hidden="true"></span>';
                    el.onclick = function (e) {

                        for (var i = 0; i < moose.toolbarItems.length; i++) {
                            var ti = moose.toolbarItems[i];
                            if (!ti.enabled) continue;
                            if ($this.collapsed) {
                                ti.el.style.setProperty("display", "inline-block", "important");
                            } else if (!ti.showWhenCollapsed) {
                                ti.el.style.display = 'none';
                            }
                        }
                        $this.collapsed = !$this.collapsed;
                        if ($this.collapsed) {
                            $this.el
                                .innerHTML =
                                '<span class="glyphicon glyphicon-chevron-right" aria-hidden="true"></span>';
                        } else {
                            $this.el
                                .innerHTML =
                                '<span class="glyphicon glyphicon-chevron-left" aria-hidden="true"></span>';
                        }
                        moose.toolbar.fitOnScreen();
                    }

                    this.el = el;
                    return el;
                }
            },
            {
                name: 'moveToolbar',
                enabled: false,
                showWhenCollapsed: true,
                initialize: function (moose, toolbarElement) {
                    var $this = this;
                    $this.toolbarElement = toolbarElement;
                    var el = document.createElement('button');
                    el.className = 'btn btn-default';
                    el.innerHTML = '<span class="glyphicon glyphicon-triangle-bottom"></span>';
                    el.onclick = function (e) {
                        if (moose.toolbarPosition == 'top') {
                            $this.el.innerHTML = '<span class="glyphicon glyphicon-triangle-top"></span>';
                            moose.setToolbarPosition('bottom');
                        } else {
                            $this.el.innerHTML = '<span class="glyphicon glyphicon-triangle-bottom"></span>';
                            moose.setToolbarPosition('top');
                        }
                    }
                    this.el = el;
                    return el;
                }
            },
        {
            name: 'save',
            enabled: true,
            initialize: function (moose) {
                var el = document.createElement('button');
                el.className = 'btn btn-primary';
                el.innerHTML = '<span class="glyphicon glyphicon-floppy-disk"></span>';
                el.onclick = function (e) {
                    moose.onSave();
                };
                this.el = el;
                return el;
            }
        },
        {
            name: 'cancel',
            enabled: true,
            initialize: function (moose) {
                var el = document.createElement('button');
                el.className = 'btn btn-danger';
                el.innerHTML = '<span class="glyphicon glyphicon-remove"></span>';
                this.el = el;
                el.onclick = function (e) {
                    moose.onCancel();
                };
                return el;
            }
        },
            {
                name: 'undo',
                enabled: true,
                initialize: function (moose) {
                    this.el = document.createElement('button');
                    this.el.className = 'btn btn-default';
                    this.el.innerHTML = '<span class="glyphicon glyphicon-arrow-left"></span>';
                    this.el.onclick = function () {
                        moose.undo();
                    }
                    return this.el;
                }
            },
            {
                name: 'redo',
                enabled: true,
                initialize: function (moose) {
                    this.el = document.createElement('button');
                    this.el.className = 'btn btn-default';
                    this.el.innerHTML = '<span class="glyphicon glyphicon-arrow-right"></span>';
                    this.el.onclick = function () {
                        moose.redo();
                    }
                    return this.el;
                }
            },
        {
            name: 'fullscreen',
            enabled: true,
            initialize: function (moose) {
                var el = document.createElement('button');
                el.className = 'btn btn-default';
                this.el = el;
                el.innerHTML = '<span class="glyphicon glyphicon-fullscreen"></span>';
                el.onclick = function (e) {
                    if (moose.isFullscreen()) {
                        moose.exitFullscreen();
                    } else {
                        moose.enterFullscreen();
                    }
                    moose.toolbar.fitOnScreen();
                };
                return el;
            }
        },
            {
                name: 'selectTool',
                enabled: true,
                showWhenCollapsed: false,
                selectedTool: null,
                resetToDefaults: function () {
                    this.moose.selectedTool = this.moose.tools[0];
                    this.el.innerHTML = this.moose.tools[0].iconHtml;
                },
                initialize: function (moose) {
                    var $this = this;
                    this.moose = moose;
                    var el = document.createElement('button');
                    el.className = 'btn btn-default';
                    this.el = el;
                    if (!this.popup) {
                        this.popup = document.createElement('div');
                    }
                    el.innerHTML = moose.tools[0].iconHtml;
                    this.popup.style.position = 'absolute';
                    this.popup.style['z-index'] = 103;
                    this.popup.style.width = '200px';
                    this.popup.style.height = '200px';
                    this.popup.style.top = '50px';
                    this.popup.style.left = '50px';
                    this.popup.style.backgroundColor = 'white';
                    this.popup.style.border = '2px solid black';
                    this.popup.style.padding = '0.5em';
                    $popup = this.popup;
                    el.addEventListener('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                        if (moose.closePopups() == $popup) {
                            return;
                        }
                        $popup.innerHTML = '';
                        for (var i = 0; i < moose.tools.length; i++) {
                            var toolSelector = moose.tools[i].getToolbarElement(
                                    moose.tools[i] == moose.selectedTool,
                                    function (tool) {
                                        moose.selectedTool = tool;
                                        $popup.style.display = 'none';
                                        el.innerHTML = tool.iconHtml;
                                    });
                            $popup.appendChild(toolSelector);

                        }
                        $popup.style.display = 'block';
                        var r = this.getClientRects();
                        var pr = $popup.getClientRects();
                        $popup.style.left = r[0].left + 'px';
                        if (moose.toolbarPosition == 'top') {
                            $popup.style.top = r[0].bottom + 'px';
                        } else {
                            $popup.style.top = (r[0].top - pr[0].height) + 'px';
                        }
                        moose.setPopup($popup);
                    }, true);
                    moose.containerEl.appendChild(this.popup);
                    $popup.style.display = 'none';
                    return el;
                }
            },
        {
            name: 'pickColor',
            enabled: true,
            showWhenCollapsed: false,
            opened: false,
            resetToDefaults: function () {
                this.moose.foreColor = { h: 0, s: 0, l: 0, a: 1 };
                this.el.style.backgroundColor = utils.toHslaString(this.moose.foreColor);
            },
            initialize: function (moose) {
                var $this = this;
                this.moose = moose;
                var el = document.createElement('button');
                el.className = 'btn btn-default';
                var fc = moose.foreColor;
                el.style.backgroundColor = utils.toHslaString(fc);
                el.addEventListener('click',
                    function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                        moose.closePopups();
                        if (!$this.opened) {
                            $this.picker.style.display = 'block';
                            $this.picker.style.position = 'absolute';
                            $this.picker.style.top = '0px';
                            $this.picker.style.left = '0px';
                            if (!moose.isFullscreen()) {
                                $this.picker.style.width = window.innerWidth + 'px';
                                $this.picker.style.height = window.innerHeight + 'px';
                            } else {
                                $this.picker.style.width = window.innerWidth + 'px';
                                $this.picker.style.height = window.innerHeight + 'px';
                            }
                            $this.picker.style['z-index'] = 102;
                            $this.opened = true;
                        } else {
                            $this.picker.style.display = 'none';
                            $this.opened = false;
                        }
                    },
                    false);
                el.innerHTML = '&nbsp';
                this.el = el;

                var colorPicker = document.createElement('div');
                colorPicker.style.display = 'none';
                colorPicker.style.position = 'absolute';
                colorPicker.style.top = '0';
                colorPicker.style.left = '1';
                colorPicker.style.height = screen.availHeight + 'px';
                colorPicker.style['z-index'] = 1;
                var step = 50;
                for (var py = 0; py < 10; py++) {
                    var row = document.createElement('div');
                    row.style.width = screen.availWidth + 'px';
                    row.style.height = '10%';
                    for (var px = 0; px < 10; px++) {
                        var cel = document.createElement('button');
                        cel.style.width = '10%';
                        cel.style.height = '100%';
                        cel.style.margin = 'auto auto auto auto';
                        cel.style.border = '0';
                        var col = null;
                        if (moose.pallette[px] && moose.pallette[px][py])
                            col = moose.pallette[px][py];
                        else
                            col = { h: 200, s: 1, l: 1, a: 1 };
                        cel.style.backgroundColor = utils.toHslaString(col);
                        cel.style.display = 'inline-block';
                        row.appendChild(cel);
                        cel.onclick = function (e) {
                            moose.setForeColor(utils.fromRgbString(this.style.backgroundColor));
                            $this.el.style.backgroundColor = this.style.backgroundColor;
                            $this.picker.style.display = 'none';
                            $this.opened = false;
                        };
                    }
                    colorPicker.appendChild(row);
                }

                $this.picker = colorPicker;
                moose.containerEl.appendChild($this.picker);

                return el;
            }
        },
        {
            name: 'toolSize',
            enabled: true,
            showWhenCollapsed: false,
            resetToDefaults: function () {
                this.moose.toolSize = 10;
                this.el.innerHTML = 'Size:' + this.moose.toolSize;
            },
            initialize: function (moose) {
                this.moose = moose;
                var el = document.createElement('button');
                el.className = 'btn btn-default';
                el.innerHTML = 'Size:' + moose.toolSize;
                el.style['margin-right'] = '1em';

                el.addEventListener('click', function (e) {
                    console.log('tool size click');
                    e.preventDefault();
                    e.stopPropagation();
                    if (!el.popup) {
                        el.popup = document.createElement('div');
                        el.popup.style.backgroundColor = '#FFF';
                        el.popup.style.padding = "10px";
                        el.popup.style.border = "2px solid blue";
                        el.popup.style.position = 'absolute';
                        el.popup.style.top = '50px';
                        el.popup.style.left = '50px';
                        el.popup.style['z-index'] = 104;
                        var input = document.createElement('input');
                        input.type = 'range';
                        input.name = 'toolSize';
                        input.value = moose.toolSize;
                        input.min = 3;
                        input.max = 200;
                        input.attributes['step'] = '1';
                        input.defaultValue = moose.toolSize;
                        input.oninput = input.onchange = function (e) {
                            moose.toolSize = input.value;
                            el.innerHTML = 'Size:' + moose.toolSize;
                        };
                        input.style.width = '500px';
                        input.style.setProperty("display", "inline-block", "important");
                        input.style['position'] = 'relative';
                        input.style['top'] = '5px';
                        el.popup.appendChild(input);
                        moose.containerEl.appendChild(el.popup);
                    } else {
                        if (moose.closePopups() == el.popup) {
                            return;
                        }
                    }

                    var r = this.getClientRects();
                    el.popup.style.left = r[0].left + 'px';
                    if (moose.toolbarPosition == 'bottom') {
                        el.popup.style.top = (r[0].top - 98) + 'px';
                    } else {
                        el.popup.style.top = (r[0].top + 128) + 'px';
                    }


                    el.popup.style.display = 'block';
                    moose.setPopup(el.popup);
                }, true);

                this.el = el;

                return el;
            }
        },
        {
            name: 'zoom-in',
            enabled: this.zoomEnabled,
            initialize: function (moose) {
                var el = document.createElement('button');
                el.className = 'btn btn-default';
                el.innerHTML = 'zoom in';
                el.onclick = function (e) {
                    var newScale = moose.getScale() * 2;
                    moose.zoom(newScale, moose.getScale(), 0.5, 0.5);
                };
                this.el = el;
                return el;
            }
        },
        {
            name: 'zoom-out',
            enabled: this.zoomEnabled,
            initialize: function (moose) {
                var el = document.createElement('button');
                el.className = 'btn btn-default';
                el.innerHTML = 'zoom out';
                el.onclick = function (e) {
                    var newScale = moose.getScale() / 2;
                    if (newScale == moose.getScale()) return;
                    moose.zoom(newScale, moose.getScale(), 0.5, 0.5);
                };
                this.el = el;
                return el;
            }
        }
        ],
        setPopup: function (popup) {
            this.popup = popup;
            console.log(this.popup);
        },
        closePopups: function () {
            if (this.popup) {
                var p = this.popup;
                this.popup.style.display = 'none';
                this.popup = null;
                return p;
            }
        },
        setToolbarPosition: function (position) {
            if (position == 'top') {
                this.toolbar.style.setProperty('top', '0px');
                this.toolbar.style.setProperty('bottom', '')
                this.toolbarPosition = 'top';
            } else if (position == 'bottom') {
                this.toolbar.style.setProperty('top', '');
                this.toolbar.style.setProperty('bottom', '0px')
                this.toolbarPosition = 'bottom';
            }
        },
        disableToolbar: function () {
            for (var i = 0; i < this.toolbarItems.length; i++) {
                var ti = this.toolbarItems[i];
                if (!ti.enabled) continue;

                ti.el.disabled = true;
            }
        },
        enableToolbar: function () {
            for (var i = 0; i < this.toolbarItems.length; i++) {
                var ti = this.toolbarItems[i];
                if (!ti.enabled) continue;

                ti.el.disabled = false;
            }
        },
        toggleFullscreen: function () {
            if (this.isFullscreen()) {
                this.exitFullscreen();
            } else {
                this.enterFullscreen();
            }
        },
        exitFullscreen: function () {
            if (document.exitFullscreen) {
                document.exitFullscreen();
            } else if (document.webkitExitFullscreen) {
                document.webkitExitFullscreen();
            } else if (document.mozCancelFullScreen) {
                document.mozCancelFullScreen();
            } else if (document.msExitFullscreen) {
                document.msExitFullscreen();
            }
            this.toolbar.className = 'toolbar-big';
        },
        enterFullscreen: function () {
            if (this.containerEl.requestFullscreen) {
                this.containerEl.requestFullscreen();
            } else if (this.containerEl.webkitRequestFullscreen) {
                this.containerEl.webkitRequestFullscreen();
            } else if (this.containerEl.mozRequestFullScreen) {
                this.containerEl.mozRequestFullScreen();
            } else if (this.containerEl.msRequestFullscreen) {
                this.containerEl.msRequestFullscreen();
            }
            this.toolbar.className = 'toolbar-small';
        },
        isFullscreen: function () {
            if (
                document.fullscreenEnabled ||
                    document.webkitFullscreenEnabled ||
                    document.mozFullScreenEnabled ||
                    document.msFullscreenEnabled
            ) {
                var isFullscreen =
                	document.fullscreenElement ||
                	document.webkitFullscreenElement ||
                	document.mozFullScreenElement ||
                	document.msFullscreenElement;
                if (!isFullscreen || isFullscreen == 'undefined')
                    return false;
                return true;

            }
            return false;
        },
        undo: function () {
            if (this.shapes.length == 0) return;
            this.shapes.pop();
            this.historyIndex--;
            this.redraw();
        },
        redo: function () {
            if (this.historyIndex == this.shapeHistory.length) return;
            this.shapes.push(this.shapeHistory[this.historyIndex++]);
            this.redraw();
        },
        addHistory: function (shape) {
            if (this.historyIndex < this.shapeHistory.length) {
                this.shapeHistory = this.shapeHistory.slice(this.historyIndex - 1);
            }
            this.shapeHistory.push(shape);
            this.historyIndex = this.shapeHistory.length;
        },
        debugInfo: function (e) {
            var d = '';
            if (e) { d += 's:' + Math.round(e.screenX) + ',' + Math.round(e.screenY) + ' c:' + Math.round(e.clientX) + ',' + Math.round(e.clientY) + '<br/>'; }
            //d += 'scale:' + this.getScale() + ' p:' + (Math.round(this.position.x)) + ',' + Math.round(this.position.y) + ' o:' + window.pageXOffset + ',' + window.pageYOffset + '<br/>';
            if (this.transform) {
                d += this.transform.toString() + '<br/>';
            }
            this.debug(d);
        },
        clientToCanvas: function (point) {
            var m = this.transform.m;
            var p = {
                x: (point.x / m[0]) - (m[4] / m[0]),
                y: (point.y / m[3]) - (m[5] / m[3])
            };

            return p;
        },
        startDrawingShape: function (point) {
            this.isDrawing = true;
            point = this.clientToCanvas(point);
            this.lastPoint = point;
            var tool = this.selectedTool || this.tools[0];
            this.currentShape = {
                foreColor: this.foreColor,
                toolSize: this.toolSize,
                points: [],
                toolName: tool.name
            }
            tool.onPointerStart(this, point);
        },
        drawMove: function (pt, tool, context) {
            var t = tool || this.selectedTool;
            var ctx = context || this.ctx;
            pt = this.clientToCanvas(pt);
            var ptData = t.onPointerDrag(this, pt);
            this.lastPoint = ptData;
            if (!this.currentShape) {
                this.currentShape = {
                    foreColor: this.foreColor,
                    toolSize: this.toolSize,
                    points: [],
                    toolName: this.selectedTool.name
                }
            }
            this.currentShape.points.push(ptData);
        },
        drawStop: function () {
            this.selectedTool.onPointerStop(this);
        },
        drawShapeToCanvas: function (shape, context) {
            if (!this.shapes) this.shapes = [];
            if (!shape) return;
            var lastPoint = shape.points[0];
            var tool = this.tools[0];
            if (shape.toolName == 'ink') {
                tool = this.tools[1];
            } else {
                tool = this.tools[0];
            }
            this.foreColor = shape.foreColor;
            this.toolSize = shape.toolSize;
            tool.onPointerStart(this, lastPoint);
            if (shape.points.length <= 0) return;

            for (var p = 1; p < shape.points.length; p++) {
                var pt = shape.points[p];
                tool.onPointerDrag(this, pt, context);
                lastPoint = pt;
            }
        },
        saveShape: function (shape, context) {
            this.drawShapeToCanvas(shape, context);
            this.shapes.push(shape);
            this.currentShape = null;
        },
        drawShapesToCanvas: function () {
            if (!this.shapes) this.shapes = [];
            for (var s = 0; s < this.shapes.length; s++) {
                this.saveShape(this.shapes[s]);
            }
        },
        buildPalette: function () {
            var p = [];
            for (var h = 5; h <= 255; h += 25) {
                var pr = [];
                for (var l = 0.1; l <= 1.0; l += 0.1) {
                    pr.push({ h: h, s: 1, l: l, a: 1 });
                }
                p.push(pr);
            }
            this.pallette = p;
        },
        debug: function (log) {
            this.debugElement.innerHTML = log;
        },
        setForeColor: function (col) {
            this.foreColor = col;
        },
        createDebugElement: function () {
            var d = document.createElement('div');
            d.style.position = 'absolute';
            d.style.top = '0px';
            d.style.left = '0px';
            d.style.height = '0px';
            d.style['z-index'] = 2;
            return d;
        },
        createToolbar: function () {
            var t = document.createElement('div');
            t.attributes['id'] = 'toolbar';
            t.style.setProperty('top', '0px');
            t.style.setProperty('left', '0px');
            t.style.backgroundColor = '#fff';
            t.style['z-index'] = 1;
            t.className = this.isFullscreen() ? 'toolbar-small' : 'toolbar-big';

            for (var i = 0; i < this.toolbarItems.length; i++) {
                var ti = this.toolbarItems[i];
                if (!ti || !ti.enabled) continue;
                var el = ti.initialize(this, t);
                el.className += ' toolbar-item';
                t.appendChild(el);
            }

            return t;
        },
        bindEvents: function () {
            var moose = this;

            moose.toolbar.move = function (e) {
                if (moose.toolbar.dragging) {
moose.debug('toolbar dragging');
                    moose.disableToolbar();
                    e.stopPropagation();
                    e.preventDefault();

                    var touches = e.changedTouches;
                    var pos = { x: e.clientX, y: e.clientY };
                    if (touches) {
                        if (touches.length == 1) {
                            pos = { x: touches[0].pageX, y: touches[0].pageY };
                        }
                    }

                    var newPos = {
                        x: pos.x - moose.toolbar.dragPosition.x,
                        y: pos.y - moose.toolbar.dragPosition.y
                    };

                    moose.toolbar.fitOnScreen();

                    moose.toolbar.position = newPos;
                    moose.toolbar.style.left = moose.toolbar.position.x + 'px';
                    moose.toolbar.style.top = moose.toolbar.position.y + 'px';
                }
            };
            moose.toolbar.startmove = function (e) {
                var touches = e.changedTouches;
                var pos = { x: e.clientX, y: e.clientY };
                if (touches) {
                    if (touches.length == 1) {
                        pos = { x: touches[0].pageX, y: touches[0].pageY };
                    }
                }
                var r = moose.toolbar.getClientRects();
                moose.toolbar.position = { x: r[0].left, y: r[0].top };
                moose.toolbar.dragging = true;
                moose.toolbar.dragPosition = { x: pos.x - r[0].left, y: pos.y - r[0].top };
            };
            moose.toolbar.endmove = function (e) {
                if (moose.toolbar.dragging) {
                    moose.toolbar.dragging = false;
                    moose.enableToolbar();
                }
            };
            moose.toolbar.fitOnScreen = function () {
                var r = moose.toolbar.getClientRects();
                var pos = { x: r[0].left, y: r[0].height };
                if (pos.x + r[0].width > screen.availWidth) {
                    pos.x = screen.availWidth - r[0].width;
                }
                if (pos.y + r[0].height > screen.availHeight) {
                    pos.y = screen.availHeight - r[0].height;
                }
                if (pos.y < 0) {
                    pos.y = 0;
                }
                if (pos.x < 0) {
                    pos.x = 0;
                }

                moose.toolbar.position = pos;
                moose.toolbar.style.left = moose.toolbar.position.x + 'px';
                moose.toolbar.style.top = moose.toolbar.position.y + 'px';
            };
            this.toolbar.addEventListener('mousedown', moose.toolbar.startmove, true);
            document.addEventListener('mousemove', moose.toolbar.move, true);
            window.addEventListener('mouseup', moose.toolbar.endmove, true);
            this.toolbar.addEventListener('touchstart', moose.toolbar.startmove, true);
            document.addEventListener('touchmove', moose.toolbar.move, true);
            document.addEventListener('touchend', moose.toolbar.endmove, true);


            this.toolbar.addEventListener('click', function (e) {
                console.log('toolbar click');
                if (moose.popup) {
                    moose.popup.style.display = 'none';
                    moose.popup = null;
                }
            }, false);

            this.toolbar.addEventListener('mouseup',
                function (e) {
                    moose.toolbar.dragging = false;
                }, true);
            this.toolbar.addEventListener('mousemove',
                function (e) {

                }, true);
            this.canvas.onmousedown = function (e) {
                if (moose.popup) {
                    moose.popup.style.display = 'none';
                    moose.popup = null;
                }

                var point = { x: e.clientX, y: e.clientY };
                if (e.shiftKey) {
                    moose.zoom(moose.getScale() * 1.2, moose.getScale(), point.x, point.y);
                } else if (e.ctrlKey) {
                    moose.zoom(moose.getScale() * 0.8, moose.getScale(), point.x, point.y);
                } else {
                    moose.startDrawingShape(point);
                }
            }
            this.canvas.addEventListener('mousemove',function (e) {
                //var moose = this.moose;
                this.moose.debugInfo();
                var currentPoint = { x: e.clientX, y: e.clientY };

                if (this.moose.mouseOut) {
                    this.moose.mouseOut = false;
                    this.moose.isDrawing = true;
                    this.moose.startDrawingShape(currentPoint);
                }
                if (this.moose.isDrawing) {
                    e.preventDefault();

                    if (this.moose.mouseOut) {
                        this.moose.lastPoint =
                        this.moose.mouseOut = false;
                    }

                    this.moose.drawMove(currentPoint);
                }
            });

            this.canvas.addEventListener('mouseup',function (e) {
                var moose = this.moose;
                if (moose.isDrawing) {
                    e.preventDefault();

                    moose.isDrawing = false;
                    moose.drawStop(moose);
                }
            });
            this.canvas.addEventListener('mouseout',function (e) {
                var moose = this.moose;
                if (moose.isDrawing) {
                    e.preventDefault();

                    moose.mouseOut = true;
                    moose.isDrawing = false;
                    moose.selectedTool.onPointerStop(moose);
                }
            });
            this.canvas.addEventListener('touchmove',
                function (e) {
                    var moose = this.moose;
moose.debug('touchmove');
                    if (moose.isDrawing) {
                        e.preventDefault();
                        var touches = e.changedTouches;
                        if (touches.length === 1) {
                            try {
                            var currentPoint = { x: touches[0].clientX, y: touches[0].clientY };
                            moose.debug(currentPoint.x+','+currentPoint.y);
                            currentPoint = moose.clientToCanvas(currentPoint);
                            moose.drawMove(currentPoint);
                            } catch (ex)
                            {
                                moose.debug(ex.message);
                            }
                        }
                    }
                });
            this.canvas.addEventListener('touchend',
                function (e) {
                    var moose = this.moose;
moose.debug('touchend');
                    if (moose.isDrawing) {
                        e.preventDefault();

                        moose.isDrawing = false;
                        moose.selectedTool.onPointerStop(moose);
                    }
                });
            this.canvas.addEventListener('touchstart',
                function (e) {
                    var moose = this.moose;
                    if (moose.popup) {
                        moose.popup.style.display = 'none';
                        moose.popup = null;
                    }

                    if (e.target.tagName.toLowerCase() !== 'canvas') {
                        return;
                    }

                    var moose = this.moose;

                    var touches = e.changedTouches;
                    if (e.touches.length === 1) {
                        e.preventDefault();
                        moose.isDrawing = true;
                        var point = { x: (touches[0].clientX), y: (touches[0].clientY) };
                        point = moose.clientToCanvas(point);
                        moose.lastPoint = point;
                        moose.startDrawingShape(point);
                        document.addEventListener('touchmove', moose.touchMoveListener);
                        document.addEventListener('touchend', moose.touchEndListener);
                        document.addEventListener('touchcancel', moose.touchEndListener);
                    }
                });

            var Shape;
            Shape = function () {
                return this;
            };
        },
        redraw: function () {
            this.ctx.clearRect(0, 0, this.width, this.height);
            for (var s = 0; s < this.shapes.length; s++) {
                this.drawShapeToCanvas(this.shapes[s], this.ctx);
            }
        },
        drawDot: function (x, y, color) {
            this.ctx.fillStyle = color;
            this.ctx.beginPath();
            this.ctx.arc(x, y, 6, 0, Math.PI / 2, true);
            this.ctx.fill();
        },
        zoom: function (newScale, oldScale, clientX, clientY) {
            if (newScale < 1) newScale = 1;
            if (newScale > 32) newScale = 32;

            var scaler = this.getScale() > newScale ? oldScale : newScale;
            var can = this.clientToCanvas({ x: clientX, y: clientY });
            this.transform.scale(newScale / oldScale, newScale / oldScale);
            var can_post = this.clientToCanvas({ x: clientX, y: clientY });
            var delta = { x: can_post.x - can.x, y: can_post.y - can.y };
            this.transform.translate(delta.x, delta.x);
            var m = this.transform.m;
            this.ctx.setTransform(m[0], m[1], m[2], m[3], m[4], m[5]);
            this.position.x = m[4];
            this.position.y = m[5];

            this.redraw();
            //this.drawDot(can.x, can.y, "red");
            //this.drawDot(can_post.x, can_post.y, "blue");
        },
        bindHammerTime: function (moose) {
            moose.hammertime = new Hammer(this.canvas);
            moose.hammertime.moose = moose;
            moose.hammertime.on('pan',
                    function (ev) {
                        var moose = ev.target.moose;
                    });
            moose.hammertime.get('pinch').set({ enable: true, direction: Hammer.DIRECTION_ALL });
            moose.hammertime.get('pan').set({ direction: Hammer.DIRECTION_ALL });
            moose.hammertime.on('pinchstart',
                    function (ev) {
                        var moose = ev.target.moose;
                        moose.scaleAtPinchStart = moose.getScale()
                        moose.offsetAtPinchStart = moose.offset;
                    });
            moose.hammertime.on('pinchmove',
                    function (ev) {
                        try {
                            var moose = ev.target.moose;
                            var newScale = moose.scaleAtPinchStart * (ev.scale);
                            var pt = { x: ev.center.x + (window.pageXOffset), y: ev.center.y + (window.pageYOffset) };
                            moose.debug(pt.x + ',' + pt.y);
                            moose.zoom(newScale, moose.getScale(), pt.x / window.innerWidth, pt.y / window.innerHeight);
                        } catch (ex) {
                            moose.debug(ex.message);
                        }
                    });
            moose.hammertime.on('pinchend',
                    function (ev) {
                        var moose = ev.target.moose;
                    });
            moose.hammertime.on("panleft panright tap press",
                    function (ev) {
                        var moose = ev.target.moose;
                    });
        }

    }
});
