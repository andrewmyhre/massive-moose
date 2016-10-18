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
            this.debug = opts.debug || false;
            this.isDrawing = false;
            this.lastPoint = null;
            this.mouseOut = false;
            //this.scale = 1.0;
            this.scaleAtPinchStart = 1.0;
            this.offsetAtPinchStart = { x: 0, y: 0 };
            this.offset = { x: 0, y: 0 };
            this.zoomEnabled = true;
            this.quality = opts.quality || 2;
            this.transform = new Transform();

            this.diagnostics = new Map();

            this.popup = null;

            this.isPinching = false;

            this.canvas = document.createElement('canvas');
            this.canvas.id = 'canvas';
            this.ctx = this.canvas.getContext('2d');
            this.ctx.lineJoin = this.ctx.lineCap = 'round';

            this.buffer = document.createElement('canvas');
            this.buffer.id = 'buffer';
            this.bufferCtx = this.buffer.getContext('2d');
            this.bufferCtx.lineJoin = this.bufferCtx.lineCap = 'round';
            this.bufferSize = 10;

            this.cv_raster = document.createElement('canvas');
            this.cv_raster_ctx = this.cv_raster.getContext('2d');
            this.cv_raster.id = 'raster';
            this.cv_raster_ctx.lineJoin = this.cv_raster_ctx.lineCap = 'round';

            this.cv_imported = document.createElement('canvas');
            this.cv_imported_ctx = this.cv_imported.getContext('2d');
            this.cv_imported.id = 'imported';
            this.cv_imported_ctx.lineJoin = this.cv_imported_ctx.lineCap = 'round';

            this.raster_data = null;

            this.canvas.moose = this;
            this.currentShape = null;
            this.shapes = [];
            this.shapeHistory = [];
            this.historyIndex = -1;

            this.cv_raster.width = this.buffer.width = this.width = opts.width;
            this.cv_raster.height = this.buffer.height = this.height = opts.height;
            this.position = { x: 0, y: 0 };

            this.buffer.moose = this;

            this.toolSize = 20;
            this.foreColor = { h: 100, s: 1, l: 0.5, a: 1 };

            if (this.debug) {
                this.debugElement = this.createDebugElement();
            }

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

            if (this.zoomEnabled) {
                this.bindHammerTime(this);
            }
        },
        getScale: function () {
            return this.transform.m[0];
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

            this.canvas.style.position = 'absolute';
            this.canvas.style.top = '200px';
            this.canvas.style.left = '0';
            this.canvas.style['z-index'] = '1';
            this.canvas.style['border'] = '1px solid blue';

            this.cv_imported.style.position = 'absolute';
            this.cv_imported.style.top = '0';
            this.cv_imported.style.left = '0';
            this.cv_imported.style['z-index'] = '1';
            this.cv_imported.style['border'] = '1px solid lime';
            this.cv_imported.width = 1600;
            this.cv_imported.height = 900;

            this.cv_raster.style.position = 'absolute';
            this.cv_raster.style.top = '0';
            this.cv_raster.style.left = '0';
            this.cv_raster.style['z-index'] = '2';
            this.cv_raster.style['border'] = '1px solid lime';

            this.buffer.style.position = 'absolute';
            this.buffer.style.top = '0';
            this.buffer.style.left = '0';
            this.buffer.style['z-index'] = '3';
            this.buffer.style['border'] = '1px solid blue';


            this.canvas.width = 1600 * this.quality;
            this.canvas.height = 900 * this.quality;

            var m = this.transform.m;
            this.ctx.transform(this.quality, 0, 0, this.quality, 0, 0);
            this.bufferCtx.transform(m[0], m[1], m[2], m[3], m[4], m[5]);
            this.cv_raster_ctx.transform(m[0], m[1], m[2], m[3], m[4], m[5]);

            if (this.debug) {
                this.containerEl.appendChild(this.canvas);
                this.containerEl.appendChild(this.cv_imported);
            }
            this.containerEl.appendChild(this.cv_raster);
            this.containerEl.appendChild(this.buffer);

            this.containerEl.style['background-color'] = "#aaaaaa"
            this.containerEl.style['overflow'] = 'hidden';
            this.containerEl.style['position'] = 'absolute';
            this.containerEl.style['top'] = '0px';
            this.containerEl.style['left'] = '0px';
            this.containerEl.style['width'] = this.width;
            this.containerEl.style['height'] = this.height;


            if (this.debugElement)
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
            this.raster_data = null;
            this.setDocumentViewportScale(1 / s);

            this.transform = new Transform();
            //this.transform.scale(2, 2);
            this.sessionData = data;
            this.containerEl.style.display = 'block';
            this.enableToolbar();

            this.ctx.clearRect(0, 0, this.width, this.height);
            this.bufferCtx.clearRect(0, 0, this.buffer.width, this.buffer.height);
            this.cv_raster_ctx.clearRect(0, 0, this.cv_raster.width, this.cv_raster.height);
            this.cv_imported_ctx.clearRect(0, 0, this.cv_imported.width, this.cv_imported.height);

            this.setToolbarPosition('top');
            this.toolbar.children[0].className = this.isFullscreen() ? 'toolbar toolbar-sm' : 'toolbar toolbar-bg';

            for (var i = 0; i < this.toolbarItems.length; i++) {
                if (this.toolbarItems[i].resetToDefaults) {
                    this.toolbarItems[i].resetToDefaults();
                }
            }

            this.shapeHistory = [];
            this.shapes = [];
            this.historyIndex = -1;
            this.importedShapes = [];

            this.isDrawing = false;
            this.zoom(1, 1, 0, 0);
            window.scrollTo(0, 0);
        },
        importDrawingData: function (data) {
            var t = this.selectedTool, ts = this.toolSize, fc = this.foreColor;
            var snapshot = JSON.parse(data);

            if (snapshot && snapshot.length) {
                this.importedShapes = snapshot;
                for (var i = 0; i < snapshot.length; i++) {
                    try {
                        this.drawShapeToCanvas(snapshot[i], this.cv_imported_ctx);
                    } catch (ex) {
                        console.log(ex.message);
                        this.writeDebug(ex.message);
                    }
                }
                this.redraw();
            }
            this.selectedTool = t;
            this.toolSize = ts;
            this.foreColor = fc;
        },
        close: function () {
            this.isDrawing = false;
            this.raster_data = null;
            this.containerEl.style.display = 'none';
            this.shapes = [];
            if (this.isFullscreen()) {

                this.exitFullscreen();
            }
        },
        onSave: function () {
            this.disableToolbar();
            this.flush();
            this.redraw();
            if (this.onExportImage) {
                try {
                    // copy all history up to historyIndex to this.shapes
                    this.shapes = this.importedShapes.concat(this.shapeHistory.slice(0, this.historyIndex + 1));
                    this.onExportImage(this.sessionData,
                        this.canvas.toDataURL('image/png'),
                        JSON.stringify(this.shapes));
                    return;
                } catch (ex) {
                    this.writeDebug(ex.message);
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
                    this.writeDebug(ex.message);
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
                    el.className = 'btn btn-default btn-brush';
                    el.addEventListener('click', function (e) {
                        if (onSelected)
                            onSelected($this);
                        return true;
                    });
                    return el;
                },
                onPointerStart: function (moose, pt, targetContext) {
                    this.lastPoint = pt;
                    return this.onPointerDrag(moose, pt, targetContext);
                },
                onPointerDrag: function (moose, pt, targetContext) {
                    var dist = utils.distanceBetween(this.lastPoint, pt);
                    var angle = utils.angleBetween(this.lastPoint, pt);
                    var ctx = targetContext || moose.bufferCtx;

                    var fc = moose.foreColor;

                    var toolSize = pt.toolSize || moose.toolSize;
                    for (var i = 0; i <= dist; i += toolSize / 4) {

                        x = this.lastPoint.x + (Math.sin(angle) * i);
                        y = this.lastPoint.y + (Math.cos(angle) * i);

                        var radgrad = ctx.createRadialGradient(x, y, toolSize / 4, x, y, toolSize / 2);

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
                        if (dist == 0) break;
                    }
                    this.lastPoint = pt;
                    return pt;
                },
                onPointerStop: function (moose) {
                    this.lastPoint = null;
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
                    el.className = 'btn btn-default btn-brush';
                    el.addEventListener('click', function (e) {
                        if (onSelected)
                            onSelected($this);
                        return true;
                    });
                    return el;
                },
                onPointerStart: function (moose, pt, targetContext) {
                    this.lastPoint = pt;

                    this.sizeVariation = pt.sizeVariation || (Math.random() * 1) + 0.5;
                    this.sizeChangeWait = pt.sizeChangeWait || utils.getRandomInt(2, 5);
                    this.blotWait = pt.blotWait || utils.getRandomInt(100, 300);
                    this.actualInkSize = pt.actualInkSize || 1;

                    return this.onPointerDrag(moose, pt, targetContext)
                },
                onPointerDrag: function (moose, pt, targetContext) {
                    var dist = utils.distanceBetween(this.lastPoint, pt);
                    var angle = utils.angleBetween(this.lastPoint, pt);
                    var ctx = targetContext || moose.bufferCtx

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

                    for (var i = 0; i <= dist; i += this.actualInkSize / 4) {

                        x = this.lastPoint.x + (Math.sin(angle) * i);
                        y = this.lastPoint.y + (Math.cos(angle) * i);

                        var radgrad = ctx
                            .createRadialGradient(x, y, this.actualInkSize / 4, x, y, this.actualInkSize / 2);

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
                        if (dist == 0) break;
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
                }
            }
        ],
        toolbarItems: [
            {
                name: 'toggleToolbarSize',
                enabled: function () { return true; },
                showWhenCollapsed: true,
                collapsed: false,
                initialize: function (moose) {
                    var $this = this;
                    var el = document.createElement('button');
                    el.style.float = 'left';
                    el.className = 'btn btn-info';
                    el.innerHTML = '<span class="glyphicon glyphicon-chevron-left" aria-hidden="true"></span>';
                    el.addEventListener('click',
                        function (e) {

                            e = e || window.event // cross-browser event

                            if (e.stopPropagation) {
                                // W3C standard variant
                                e.stopPropagation()
                            } else {
                                // IE variant
                                e.cancelBubble = true
                            }

                            for (var i = 0; i < moose.toolbarItems.length; i++) {
                                var ti = moose.toolbarItems[i];
                                if (!ti.enabled()) continue;
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
                            return true;
                        });

                    this.el = el;
                    return el;
                }
            },
            {
                name: 'moveToolbar',
                enabled: function () { return true; },
                showWhenCollapsed: true,
                initialize: function (moose, toolbarElement) {
                    var $this = this;
                    $this.toolbarElement = toolbarElement;
                    var el = document.createElement('button');
                    el.className = 'btn btn-default';
                    el.innerHTML = '<span class="glyphicon glyphicon-triangle-bottom"></span>';
                    el.addEventListener('click',
                        function (e) {
                            if (moose.toolbarPosition == 'top') {
                                $this.el.innerHTML = '<span class="glyphicon glyphicon-triangle-top"></span>';
                                moose.setToolbarPosition('bottom');
                            } else {
                                $this.el.innerHTML = '<span class="glyphicon glyphicon-triangle-bottom"></span>';
                                moose.setToolbarPosition('top');
                            }
                            return true;
                        });
                    this.el = el;
                    return el;
                }
            },
        {
            name: 'save',
            enabled: function () { return true; },
            initialize: function (moose) {
                var el = document.createElement('button');
                el.className = 'btn btn-primary';
                el.innerHTML = '<span class="glyphicon glyphicon-floppy-disk"></span>';
                el.addEventListener('click', function (e) {
                    moose.onSave();
                    return true;
                });
                this.el = el;
                return el;
            }
        },
        {
            name: 'cancel',
            enabled: function () { return true; },
            initialize: function (moose) {
                var el = document.createElement('button');
                el.className = 'btn btn-danger';
                el.innerHTML = '<span class="glyphicon glyphicon-remove"></span>';
                this.el = el;
                el.addEventListener('click', function (e) {
                    moose.onCancel();
                    return true;
                });
                return el;
            }
        },
            {
                name: 'undo',
                enabled: function () { return true; },
                initialize: function (moose) {
                    this.el = document.createElement('button');
                    this.el.className = 'btn btn-default';
                    this.el.innerHTML = '<span class="glyphicon glyphicon-arrow-left"></span>';
                    this.el.addEventListener('click',
                        function (e) {
                            moose.undo();
                            return true;
                        });
                    return this.el;
                }
            },
            {
                name: 'redo',
                enabled: function () { return true; },
                initialize: function (moose) {
                    this.el = document.createElement('button');
                    this.el.className = 'btn btn-default';
                    this.el.innerHTML = '<span class="glyphicon glyphicon-arrow-right"></span>';
                    this.el.addEventListener('click',
                        function () {
                            moose.redo();
                            return true;
                        });
                    return this.el;
                }
            },
        {
            name: 'fullscreen',
            enabled: function () { return true; },
            initialize: function (moose) {
                var el = document.createElement('button');
                el.className = 'btn btn-default';
                this.el = el;
                el.innerHTML = '<span class="glyphicon glyphicon-fullscreen"></span>';
                el.addEventListener('click', function (e) {
                    if (moose.isFullscreen()) {
                        moose.exitFullscreen();
                    } else {
                        moose.enterFullscreen();
                    }
                    moose.toolbar.fitOnScreen();
                    return true;
                });
                return el;
            }
        },
            {
                name: 'selectTool',
                enabled: function () { return true; },
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
                    this.popup.style.maxWidth = '100%';
                    //this.popup.style.height = '200px';
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
                                        el.innerHTML = tool.iconHtml;
                                        moose.closePopups();
                                    });
                            $popup.appendChild(toolSelector);

                        }
                        $popup.style.display = 'block';
                        var r = this.getClientRects();
                        var pr = $popup.getClientRects();
                        $popup.style.left = r[0].left + 'px';
                        var spaceBelow = window.innerHeight - (r[0].top + r[0].height + 50);
                        var spaceAbove = r[0].top;
                        if (spaceBelow < spaceAbove) {
                            $popup.style.top = (r[0].top - pr[0].height) + 'px';

                        } else {
                            $popup.style.top = r[0].bottom + 'px';
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
            enabled: function () { return true; },
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
                        cel.addEventListener('click', function (e) {
                            moose.setForeColor(utils.fromRgbString(this.style.backgroundColor));
                            $this.el.style.backgroundColor = this.style.backgroundColor;
                            $this.picker.style.display = 'none';
                            $this.opened = false;
                            return true;
                        });
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
            enabled: function () { return true; },
            showWhenCollapsed: false,
            resetToDefaults: function () {
                this.moose.toolSize = 10;
                this.el.innerHTML = 'Size:' + this.moose.toolSize;
            },
            updatePreview: function () {
                var ctx = this.el.previewCanvas.getContext('2d');
                ctx.beginPath();
                ctx.clearRect(0, 0, 200, 200);
                ctx.ellipse(100, 100, this.moose.toolSize / 2, this.moose.toolSize / 2, 45 * Math.PI / 180, 0, 2 * Math.PI);
                ctx.fillStyle = utils.toHslaString(this.moose.foreColor);
                ctx.fill();

            },
            initialize: function (moose) {
                this.moose = moose;
                var el = document.createElement('button');
                el.parent = this;
                el.className = 'btn btn-default';
                el.innerHTML = 'Size:' + moose.toolSize;
                el.style['margin-right'] = '1em';

                el.addEventListener('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    if (!el.popup) {
                        el.popup = document.createElement('div');
                        el.popup.style.backgroundColor = '#FFF';
                        el.popup.style.padding = "10px";
                        el.popup.style.border = "2px solid blue";
                        el.popup.style.position = 'absolute';
                        el.popup.style['z-index'] = 104;
                        var input = document.createElement('input');
                        input.type = 'range';
                        input.name = 'toolSize';
                        input.value = moose.toolSize;
                        input.min = 1;
                        input.max = 200;
                        input.attributes['step'] = '1';
                        input.defaultValue = moose.toolSize;
                        input.oninput = input.onchange = function (e) {
                            moose.toolSize = input.value;
                            el.innerHTML = 'Size:' + moose.toolSize;
                            el.parent.updatePreview();

                        };
                        input.style.width = '500px';
                        input.style.setProperty("display", "inline-block", "important");
                        input.style['position'] = 'relative';
                        input.style['top'] = '5px';
                        input.style['margin'] = '1em 0 1em 0';
                        el.popup.appendChild(input);

                        var preview = document.createElement('canvas');
                        preview.width = 200;
                        preview.height = 200;
                        preview.style.position = 'relative';
                        preview.style.display = 'block';
                        preview.style.margin = 'auto';
                        preview.className = 'tool-size-preview';
                        el.previewCanvas = preview;
                        el.popup.appendChild(preview);

                        moose.containerEl.appendChild(el.popup);


                    } else {
                        if (moose.closePopups() == el.popup) {
                            return;
                        }
                    }

                    el.popup.style.display = 'block';
                    var pr = el.popup.getClientRects();
                    el.popup.style.left = ((window.innerWidth / 2) - (pr[0].width / 2)) + 'px';
                    el.popup.style.top = ((window.innerHeight / 2) - (pr[0].height / 2)) + 'px';

                    el.parent.updatePreview();
                    moose.setPopup(el.popup);
                }, true);

                this.el = el;

                return el;
            }
        },
        {
            name: 'toggle canvas',
            enabled: function () {
                return this.moose.debug;
            },
            initialize: function (moose) {
                var el = document.createElement('button');
                this.moose = moose;
                if (!moose.debug) return null;
                el.className = 'btn btn-default';
                el.innerHTML = 'canvas';
                el.addEventListener('click', function (e) {
                    if (document.getElementById('canvas').style.display == 'none') {
                        document.getElementById('canvas').style.display = 'inline-block';
                    } else {
                        document.getElementById('canvas').style.display = 'none';
                    }
                    return true;
                });
                this.el = el;
                return el;
            }
        },
        {
            name: 'toggle buffer',
            enabled: function () { return this.moose.debug; },
            initialize: function (moose) {
                var el = document.createElement('button');
                this.moose = moose;
                if (!moose.debug) return null;
                el.className = 'btn btn-default';
                el.innerHTML = 'buffer';
                el.addEventListener('click', function (e) {
                    if (document.getElementById('buffer').style.display == 'none') {
                        document.getElementById('buffer').style.display = 'inline-block';
                    } else {
                        document.getElementById('buffer').style.display = 'none';
                    }
                    return true;
                });
                this.el = el;
                return el;
            }
        },
        {
            name: 'toggle raster',
            enabled: function () { return this.moose.debug; },
            initialize: function (moose) {
                var el = document.createElement('button');
                this.moose = moose;
                if (!moose.debug) return null;
                el.className = 'btn btn-default';
                el.innerHTML = 'raster';
                el.addEventListener('click', function (e) {
                    if (document.getElementById('raster').style.display == 'none') {
                        document.getElementById('raster').style.display = 'inline-block';
                    } else {
                        document.getElementById('raster').style.display = 'none';
                    }
                    return true;
                });
                this.el = el;
                return el;
            }
        },
        {
            name: 'toggle imported',
            enabled: function () { return this.moose.debug; },
            initialize: function (moose) {
                var el = document.createElement('button');
                this.moose = moose;
                if (!moose.debug) return null;
                el.className = 'btn btn-default';
                el.innerHTML = 'imported';
                el.addEventListener('click', function (e) {
                    if (document.getElementById('imported').style.display == 'none') {
                        document.getElementById('imported').style.display = 'inline-block';
                    } else {
                        document.getElementById('imported').style.display = 'none';
                    }
                    return true;
                });
                this.el = el;
                return el;
            }
        }
        ],
        createToolbar: function () {
            var t = document.createElement('div');

            t.id = 'toolbar-container';
            t.className = 'toolbar-container';
            t.style.setProperty('top', '0px');
            t.style.setProperty('left', '0px');
            t.style['z-index'] = 10;
            t.moose = this;

            var tb = document.createElement('div');
            tb.id = 'toolbar';
            tb.className = this.isFullscreen() ? 'toolbar toolbar-sm' : 'toolbar toolbar-bg';

            for (var i = 0; i < this.toolbarItems.length; i++) {
                var ti = this.toolbarItems[i];
                ti.moose = this;
                var el = ti.initialize(this, t);
                if (!ti || !ti.enabled()) continue;
                el.className += ' toolbar-item btn-toolbar';
                el.toolbarItem = ti;
                el.addEventListener('click',
                    function (e) {
                        e.preventDefault();
                        console.log('clicked ' + this.toolbarItem.name);
                        return false;
                    });
                tb.appendChild(el);
            }
            t.appendChild(tb);

            // toolbar drag element
            var dr = document.createElement('div');
            dr.attributes['id'] = 'toolbar-drag';
            dr.className = 'toolbar-drag';
            dr.style['z-index'] = 2;
            dr.top = '0';
            dr.left = '0';
            t.appendChild(dr);

            return t;
        },
        setPopup: function (popup) {
            this.popup = popup;
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
                if (!ti.enabled()) continue;

                ti.el.disabled = true;
            }
            this.toolbar.children[1].style.display = 'inline-block';
        },
        enableToolbar: function () {
            for (var i = 0; i < this.toolbarItems.length; i++) {
                var ti = this.toolbarItems[i];
                if (!ti.enabled()) continue;

                ti.el.disabled = false;
            }
            this.toolbar.children[1].style.display = 'none';
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
            this.toolbar.children[0].className = 'toolbar toolbar-bg';
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
            this.toolbar.children[0].className = 'toolbar toolbar-sm';
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
            if (this.historyIndex <= -1)
                return;

            var t0 = performance.now();
            if (this.shapes.length > 0) {
                this.shapes.pop();
                this.redrawBuffer();
            }

            var redraw = false;
            if (this.shapes.length == 0 && this.historyIndex > 0) {
                // copy previous x shapes from history into the buffer
                for (var i = 0; i < this.bufferSize; i++) {
                    if (this.historyIndex - this.bufferSize + i < 0) continue;
                    this.shapes.push(this.shapeHistory[this.historyIndex - this.bufferSize + i]);
                }
                redraw = true;
            }
            this.historyIndex--;
            if (redraw) this.redraw();
            this.debugInfo();
            var t1 = performance.now();
            this.diagnostics.set('undo', t1 - t0);
        },
        redo: function () {
            if (this.historyIndex >= this.shapeHistory.length - 1) return;
            var t0 = performance.now();
            this.shapes.push(this.shapeHistory[++this.historyIndex]);
            this.redrawBuffer();
            if (this.shapes.length >= this.bufferSize) {
                this.flush();
            }
            this.debugInfo();
            var t1 = performance.now();
            this.diagnostics.set('redo', t1 - t0);
        },
        addHistory: function (shape) {
            var t0 = performance.now();
            var redraw = false;
            if (this.historyIndex < (this.shapeHistory.length - 1)) {
                this.shapeHistory = this.shapeHistory.slice(0, this.historyIndex + 1);
                this.shapes = [];
                redraw = true;
            }
            this.shapeHistory.push(shape);
            this.historyIndex = this.shapeHistory.length - 1;
            if (redraw) {
                this.redraw();
            }
            this.debugInfo();
            var t1 = performance.now();
            this.diagnostics.set('add history', t1 - t0);
        },
        debugInfo: function (e) {
            var d = '';
            if (e) { d += 's:' + Math.round(e.screenX) + ',' + Math.round(e.screenY) + ' c:' + Math.round(e.clientX) + ',' + Math.round(e.clientY) + '<br/>'; }
            //d += 'scale:' + this.getScale() + ' p:' + (Math.round(this.position.x)) + ',' + Math.round(this.position.y) + ' o:' + window.pageXOffset + ',' + window.pageYOffset + '<br/>';
            if (this.transform) {
                //d += this.transform.toString() + '<br/>';
            }
            //d += 'shapes:' + this.shapes.length;
            d += 'b:' + this.shapes.length + ' h:' + this.shapeHistory.length + '(' + this.historyIndex + ')';

            this.diagnostics.forEach(function (value, key) {
                d += ', ' + key + ':' + Math.round(value) + 'ms';
            });

            if (this.debug) {
                this.writeDebug(d);
            }
        },
        clientToCanvas: function (point) {
            var m = this.transform.m;
            var p = {
                x: (point.x / m[0]) - (m[4] / m[0]),
                y: (point.y / m[3]) - (m[5] / m[3])
            };

            return p;
        },
        startDrawingShape: function (point, context) {
            this.isDrawing = true;
            var ctx = context || this.bufferCtx;
            point = this.clientToCanvas(point);
            this.lastPoint = point;
            var tool = this.selectedTool || this.tools[0];
            this.currentShape = {
                foreColor: this.foreColor,
                toolSize: this.toolSize,
                points: [],
                toolName: tool.name
            }
            var ptData = tool.onPointerStart(this, point, ctx);
            this.lastPoint = ptData;
            this.currentShape.points.push(ptData);
        },
        drawMove: function (pt, tool, context) {
            var t = tool || this.selectedTool;
            var ctx = context || this.bufferCtx;
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
        drawStop: function (pt, tool, context) {
            this.selectedTool.onPointerStop(this);

            var s = this.currentShape;
            this.saveShape(s, this.buffer);
            this.addHistory(s);
            s = null;

            if (this.shapes.length >= this.bufferSize) {
                this.flush();
            }
            this.debugInfo();
            //this.rasterize();
        },
        flush: function () {
            var t0 = performance.now();
            var m = this.transform.m;
            var x = -m[4] / m[0], y = -m[5] / m[3], w = this.width / m[0], h = this.height / m[3];
            this.ctx.drawImage(this.buffer, x, y, w, h);
            this.bufferCtx.clearRect(0, 0, this.width, this.height);
            this.updateRaster();
            this.shapes = [];
            var t1 = performance.now();
            this.diagnostics.set('flush', (t1 - t0));
            this.debugInfo();
        },
        updateRaster: function () {
            this.cv_raster_ctx.clearRect(0, 0, this.width, this.height);
            var m = this.transform.m;
            this.cv_raster_ctx.drawImage(this.cv_imported, 0, 0, this.width, this.height);
            this.cv_raster_ctx.drawImage(this.canvas, 0, 0, this.width, this.height);
        },
        zoom: function (newScale, oldScale, clientX, clientY) {
            var t0 = performance.now();
            this.flush();

            if (newScale < 1) newScale = 1;
            if (newScale > 32) newScale = 32;

            var scaler = this.getScale() > newScale ? oldScale : newScale;
            var can = this.clientToCanvas({ x: clientX, y: clientY });
            this.transform.scale(newScale / oldScale, newScale / oldScale);
            var can_post = this.clientToCanvas({ x: clientX, y: clientY });
            var delta = { x: can_post.x - can.x, y: can_post.y - can.y };
            this.transform.translate(delta.x, delta.y);

            var m = this.transform.m;
            this.bufferCtx.setTransform(m[0], m[1], m[2], m[3], m[4], m[5]);
            this.cv_raster_ctx.setTransform(m[0], m[1], m[2], m[3], m[4], m[5]);
            //this.ctx.setTransform(m[0], m[1], m[2], m[3], m[4], m[5]);
            this.position.x = m[4];
            this.position.y = m[5];

            this.updateRaster();
            var t1 = performance.now();
            this.diagnostics.set('zoom', t1 - t0);
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
            tool.onPointerStart(this, lastPoint, context);
            if (shape.points.length <= 0) return;

            for (var p = 1; p < shape.points.length; p++) {
                var pt = shape.points[p];
                tool.onPointerDrag(this, pt, context);
                lastPoint = pt;
            }
            tool.onPointerStop(this, pt, context);
        },
        saveShape: function (shape) {
            this.shapes.push(shape);
            this.currentShape = null;
        },
        drawShapesToCanvas: function () {
            if (!this.shapes) this.shapes = [];
            for (var s = 0; s < this.shapes.length; s++) {
                this.saveShape(this.shapes[s], this.bufferCtx);
            }
        },
        drawRasterized: function () {
            if (this.raster_data == null) return;
            var moose = this;
            if (!this.rasterizedImage) {
                this.rasterizedImage = new Image;
                this.rasterizedImage.onload = function () {
                    moose.bufferCtx.clearRect(-1000, -1000, moose.width + 2000, moose.height + 2000);
                    moose.bufferCtx.drawImage(moose.rasterizedImage, 0, 0);
                };
            } else {
                moose.bufferCtx.clearRect(-1000, -1000, moose.width + 2000, moose.height + 2000);
                moose.bufferCtx.drawImage(moose.rasterizedImage, 0, 0);
            }
            this.rasterizedImage.src = this.raster_data;
        },
        rasterize: function () {
            this.raster_data = this.buffer.toDataURL('image/png');
        },
        redrawBuffer: function () {
            var t0 = performance.now();
            var toolSize = this.toolSize;
            var foreColor = this.foreColor;
            this.bufferCtx.clearRect(0, 0, this.width, this.height);
            for (var s = 0; s < this.shapes.length; s++) {
                this.drawShapeToCanvas(this.shapes[s], this.bufferCtx);
            }
            this.toolSize = toolSize;
            this.foreColor = foreColor;
            var t1 = performance.now();
            this.diagnostics.set('redraw buffer', t1 - t0);
        },
        redraw: function () {
            var t0 = performance.now();
            var toolSize = this.toolSize;
            var foreColor = this.foreColor;
            this.ctx.clearRect(0, 0, this.width, this.height);
            var redrawCount = (this.historyIndex + 1) - this.shapes.length; // number of items in history up to head minus total items in buffer
            this.ctx.drawImage(this.cv_imported, 0, 0, this.cv_imported.width, this.cv_imported.height);
            for (var s = 0; s < redrawCount; s++) {
                this.drawShapeToCanvas(this.shapeHistory[s], this.ctx);
            }
            this.updateRaster();
            this.redrawBuffer();
            this.toolSize = toolSize;
            this.foreColor = foreColor;
            var t1 = performance.now();
            this.diagnostics.set('redraw', t1 - t0);
        },
        drawDot: function (x, y, color) {
            this.bufferCtx.fillStyle = color;
            this.bufferCtx.beginPath();
            this.bufferCtx.arc(x, y, 6, 0, Math.PI / 2, true);
            this.bufferCtx.fill();
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
        writeDebug: function (log) {
            this.debugElement.innerHTML = log;
        },
        setForeColor: function (col) {
            this.foreColor = col;
        },
        createDebugElement: function () {
            var d = document.createElement('div');
            d.className = 'debug-panel';
            return d;
        },
        bindEvents: function () {
            var moose = this;

            moose.toolbar.move = function (e) {
                if (moose.toolbar.dragging) {

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

                    // we allow the user to move the mouse a few pixels before we decide the toolbar is actually being dragged
                    // this allows the user to click buttons
                    if (!moose.toolbar.dragActivated) {
                        var d = utils.distanceBetween(moose.toolbar.dragInitialPosition, pos);
                        if (d > 5) {
                            moose.toolbar.dragActivated = true;
                        }
                    }

                    moose.toolbar.lastDragPosition = newPos;

                    if (!moose.toolbar.dragActivated) {
                        console.log('dragging, not activated:'+pos.x+','+pos.y);
                        return;
                    }

                    e.stopPropagation();
                    e.preventDefault();

                    console.log('dragging, activated:' + pos.x + ',' + pos.y);
                    moose.disableToolbar();
                    moose.closePopups();

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
                moose.toolbar.dragActivated = false;
                moose.toolbar.dragging = true;
                moose.toolbar.dragInitialPosition = pos;
                moose.toolbar.dragPosition = { x: pos.x - r[0].left, y: pos.y - r[0].top };
                moose.toolbar.lastDragPosition = moose.toolbar.dragPosition;
                console.log('toolbar startmove: '+pos.x+','+pos.y);
                return false;
            };
            moose.toolbar.endmove = function (e) {
                moose.enableToolbar();
                moose.toolbar.dragging = false;
                moose.toolbar.dragActivated = false;
                console.log('toolbar endmove');
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
            document.body.addEventListener('mousemove', moose.toolbar.move, true);

            this.toolbar.addEventListener('touchstart', moose.toolbar.startmove);
            this.toolbar.addEventListener('touchmove', moose.toolbar.move);
            document.body.addEventListener('touchmove', moose.toolbar.move, true);
            document.body.addEventListener('touchend', moose.toolbar.endmove, true);

            window.addEventListener('mouseup', moose.toolbar.endmove, true);
            this.buffer.addEventListener('mouseup', moose.toolbar.endmove, true);


            this.toolbar.addEventListener('click', function (e) {
                if (moose.popup) {
                    moose.popup.style.display = 'none';
                    moose.popup = null;
                }
            }, false);

            this.buffer.addEventListener('mousedown',
                function (e) {
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
                });

            this.buffer.addEventListener('mousemove', function (e) {
                //var moose = this.moose;
                var currentPoint = { x: e.clientX, y: e.clientY };

                if (this.moose.mouseOut) {
                    this.moose.mouseOut = false;
                    this.moose.isDrawing = true;
                    this.moose.startDrawingShape(currentPoint);
                }
                if (this.moose.isDrawing) {
                    e.preventDefault();

                    if (this.moose.mouseOut) {
                        this.moose.mouseOut = false;
                    }

                    this.moose.drawMove(currentPoint);
                }
            });

            this.buffer.addEventListener('mouseup', function (e) {
                var moose = this.moose;
                var currentPoint = { x: e.clientX, y: e.clientY };
                if (moose.isDrawing) {
                    e.preventDefault();

                    moose.isDrawing = false;
                    moose.drawStop(currentPoint);
                }
            });
            this.buffer.addEventListener('mouseout', function (e) {
                var moose = this.moose;
                if (moose.isDrawing) {
                    e.preventDefault();

                    moose.mouseOut = true;
                    moose.isDrawing = false;
                    moose.selectedTool.onPointerStop(moose.lastPoint);
                }
            });

            this.buffer.addEventListener('touchmove',
                function (e) {
                    var moose = this.moose;
                    if (moose.isDrawing) {
                        e.preventDefault();
                        var touches = e.changedTouches;
                        if (touches.length === 1) {
                            var currentPoint = { x: touches[0].clientX, y: touches[0].clientY };
                            moose.drawMove(currentPoint);
                        }
                    }
                });
            this.buffer.addEventListener('touchend',
                function (e) {
                    var moose = this.moose;
                    if (moose.isDrawing) {
                        e.preventDefault();
                        var touches = e.changedTouches;
                        moose.writeDebug('touch end:' + touches.length);

                        moose.isDrawing = false;
                        moose.drawStop(moose);
                    }
                });

            this.buffer.addEventListener('touchstart',
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
                        moose.startDrawingShape(point);
                    }
                });

            var Shape;
            Shape = function () {
                return this;
            };
        },
        bindHammerTime: function (moose) {
            moose.hammertime = new Hammer(this.buffer);
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
                        if (moose.isDrawing) {
                            moose.lastPoint = null;
                            moose.isDrawing = false;
                            moose.writeDebug('stop drawing');
                        }
                        moose.pinchDeltaLastFrame = { x: 0, y: 0 };
                        moose.scaleAtPinchStart = moose.getScale()
                        moose.offsetAtPinchStart = moose.offset;
                    });
            moose.hammertime.on('pinchmove',
                    function (ev) {
                        try {
                            var moose = ev.target.moose;
                            if (moose.isDrawing) {
                                moose.lastPoint = null;
                                moose.isDrawing = false;
                                moose.writeDebug('stop drawing');
                            }

                            var newScale = moose.scaleAtPinchStart * (ev.scale);
                            var pt = { x: ev.center.x + (window.pageXOffset), y: ev.center.y + (window.pageYOffset) };
                            moose.zoom(newScale, moose.getScale(), pt.x, pt.y);
                            var dx = ev.deltaX - moose.pinchDeltaLastFrame.x,
                                dy = ev.deltaY - moose.pinchDeltaLastFrame.y;
                            moose.transform.translate(dx / newScale, dy / newScale);
                            moose.pinchDeltaLastFrame = { x: ev.deltaX, y: ev.deltaY };
                        } catch (ex) {
                            moose.writeDebug(ex.message);
                        }
                    });
            moose.hammertime.on('pinchend',
                    function (ev) {
                        var moose = ev.target.moose;
                    });
            moose.hammertime.on("pan",
                    function (ev) {
                        var moose = ev.target.moose;
                    });
        }

    }
});
