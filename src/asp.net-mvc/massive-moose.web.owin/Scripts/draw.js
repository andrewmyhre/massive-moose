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
            initialize: function(arg1, arg2) {
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

                this.viewportScale = 1;
                this.opts = opts || {};
                this.isDrawing = false;
                this.lastPoint = null;
                this.mouseOut = true;
                this.scale = 1.0;
                this.scaleAtPinchStart = 1.0;
                this.offsetAtPinchStart = { x: 0, y: 0 };
                this.offset = { x: 0, y: 0 };
                this.zoomEnabled = false;
                document.body.style.backgroundColor = '#ff0000';

                this.isPinching = false;

                this.canvas = document.createElement('canvas');
                this.buffer = document.createElement('canvas');
                this.canvas.style['background-color'] = 'white';
                this.canvas.moose = this;
                this.ctx = this.canvas.getContext('2d');
                this.ctx.lineJoin = this.ctx.lineCap = 'round';
                this.currentShape = null;
                this.shapes = [];

                this.canvas.width = this.buffer.width = this.width = opts.width;
                this.canvas.height = this.buffer.height = this.height = opts.height;
                this.position = { x: 0, y: 0 };

                this.buffer.moose = this;
                this.buffer.lineJoin = this.buffer.lineCap = 'round';

                this.toolSize = 10;
                this.foreColor = { h: 100, s: 1, l: 0.5, a: 1 };

                this.debugElement = this.createDebugElement();

                if (containerEl) {
                    this.bindToElement(containerEl);
                    this.containerEl.appendChild(this.createToolbar());
                }

                this.bindEvents();

                if (this.zoomEnabled) {
                    this.bindHammerTime(this);
                }
            },
            bindToElement: function(containerEl) {
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
            startDrawing: function(data) {
                var viewport = document.querySelector("meta[name=viewport]");
                var scalex = 1600 / screen.availWidth;
                var scaley = 900 / screen.availHeight;
                this.scale = scalex;
                console.log(this.scale);
                if (scalex * screen.availHeight > 900) {
                    this.scale = scaley;
                    console.log(this.scale);
                }
                if (this.scale < 1) {
                    this.scale = 1;
                    console.log(this.scale);
                }
                this.canvas.width = 1600;
                this.canvas.height = 900;
                this.ctx.scale(1 / this.scale, 1 / this.scale);
                this.sessionData = data;
                this.containerEl.style.display = 'block';
                this.enableToolbar();
                this.selectedTool = this.tools[0];

                if (data && data.data && data.data.snapshotJson) {
                    var snapshot = JSON.parse(data.data.snapshotJson);

                    if (snapshot && snapshot.length) {
                        for (var i = 0; i < snapshot.length; i++) {
                            try {
                                this.shapes.push(snapshot[i]);
                            } catch (ex) {

                            }
                        }
                    }
                }
                this.redraw();
                this.isDrawing = false;
            },
            close: function () {
                this.isDrawing = false;
                this.containerEl.style.display = 'none';
                this.shapes = [];
            },
            onSave: function() {
                this.disableToolbar();
                if (this.onExportImage) {
                    try {
                        this.updateBuffer();
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
            onCancel: function() {
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
                    iconHtml:'<img src="/content/tool_paint.png" />',
                    getToolbarElement: function(isSelected, onSelected) {
                        var $this = this;
                        var el = document.createElement('button');
                        this.el = el;
                        el.innerHTML = this.iconHtml;
                        if (isSelected) {
                            el.style.border = '1px solid red';
                        } else {
                            el.style.border = '1px solid #888';
                        }
                        el.onclick = function(e) {
                            if (onSelected)
                                onSelected($this);
                        };
                        return el;
                    },
                    onPointerStart: function(moose, pt) {
                        moose.isDrawing = true;
                        moose.lastPoint = pt;
                    },
                    onPointerDrag: function(moose, pt, data) {
                        var dist = utils.distanceBetween(moose.lastPoint, pt);
                        var angle = utils.angleBetween(moose.lastPoint, pt);

                        var fc = moose.foreColor;


                        for (var i = 0; i < dist; i += moose.toolSize / 4) {

                            x = moose.lastPoint.x + (Math.sin(angle) * i);
                            y = moose.lastPoint.y + (Math.cos(angle) * i);

                            var radgrad = moose.ctx
                                .createRadialGradient(x, y, moose.toolSize / 2, x, y, moose.toolSize);

                            var centerColor = { h: fc.h, s: fc.s, l: fc.l, a: fc.a };
                            var midColor = { h: fc.h, s: fc.s, l: fc.l, a: fc.a };
                            var edgeColor = { h: fc.h, s: fc.s, l: fc.l, a: fc.a };
                            centerColor.a = 1;
                            midColor.a = 0.5;
                            edgeColor.a = 0;

                            radgrad.addColorStop(0, utils.toHslaString(centerColor));
                            radgrad.addColorStop(0.5, utils.toHslaString(midColor));
                            radgrad.addColorStop(1, utils.toHslaString(edgeColor));

                            moose.ctx.fillStyle = radgrad;
                            moose.ctx.fillRect(x - moose.toolSize,
                                y - moose.toolSize,
                                moose.toolSize * 2,
                                moose.toolSize * 2);
                        }
                        moose.lastPoint = pt;
                        if (!moose.currentShape) {
                            moose.currentShape = {
                                foreColor: fc,
                                toolSize: moose.toolSize,
                                points: []
                            }
                        }
                        moose.currentShape.points.push(pt);

                    },
                    onPointerStop: function(moose) {
                        if (!moose.shapes) moose.shapes = [];
                        moose.shapes.push(moose.currentShape);
                        moose.currentShape = null;
                        console.log(moose.shapes[moose.shapes.length - 1]);
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
                        moose.isDrawing = true;
                        moose.lastPoint = pt;

                        this.sizeVariation = pt.sizeVariation || (Math.random() * 1) + 0.5;
                        this.sizeChangeWait = pt.sizeChangeWait || utils.getRandomInt(2, 5);
                        this.blotWait = pt.blotWait || utils.getRandomInt(100, 300);
                        this.actualInkSize = pt.actualInkSize || 1;

                    },
                    onPointerDrag: function (moose, pt) {
                        var dist = utils.distanceBetween(moose.lastPoint, pt);
                        var angle = utils.angleBetween(moose.lastPoint, pt);

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
                            console.log('blot: ' + this.sizeVariation);
                        }

                        var toolSize = moose.toolSize * this.sizeVariation;
                        if (pt.actualInkSize)
                            this.actualInkSize = pt.actualInkSize;
                        else 
                            this.actualInkSize += (toolSize - this.actualInkSize) / (this.sizeChangeWait / 2);

                        for (var i = 0; i < dist; i += this.actualInkSize / 4) {

                            x = moose.lastPoint.x + (Math.sin(angle) * i);
                            y = moose.lastPoint.y + (Math.cos(angle) * i);

                            var radgrad = moose.ctx
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


                            moose.ctx.fillStyle = radgrad;
                            moose.ctx.fillRect(x - this.actualInkSize,
                                y - this.actualInkSize,
                                this.actualInkSize * 2,
                                this.actualInkSize * 2);
                        }

                        var pointData=
                        {
                            x: pt.x,
                            y: pt.y,
                            sizeChangeWait: this.sizeChangeWait,
                            blotWait: this.blotWait,
                            sizeVariation: this.sizeVariation,
                            actualInkSize: this.actualInkSize
                        };

                        moose.lastPoint = pt;
                        if (!moose.currentShape) {
                            moose.currentShape = {
                                foreColor: fc,
                                toolSize: moose.toolSize,
                                points: [],
                                toolName: moose.selectedTool.name
                            }
                        }
                        moose.currentShape.points.push(pointData);

                    },
                    onPointerStop: function (moose) {
                        if (!moose.shapes) moose.shapes = [];
                        moose.shapes.push(moose.currentShape);
                        moose.currentShape = null;
                        console.log(moose.shapes[moose.shapes.length - 1]);
                    }
                }
            ],
            toolbarItems: [
                {
                    name: 'toggleToolbarSize',
                    enabled: true,
                    showWhenCollapsed: true,
                    collapsed: false,
                    initialize: function(moose) {
                        var $this = this;
                        var el = document.createElement('button');
                        el.innerHTML = '<span class="glyphicon glyphicon-chevron-left" aria-hidden="true"></span>';
                        el.onclick = function(e) {

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
                        }

                        this.el = el;
                        return el;
                    }
                },
                {
                    name: 'moveToolbar',
                    enabled: true,
                    showWhenCollapsed: true,
                    position: 'top',
                    initialize: function(moose, toolbarElement) {
                        var $this = this;
                        $this.toolbarElement = toolbarElement;
                        var el = document.createElement('button');
                        el.innerHTML = '<span class="glyphicon glyphicon-triangle-bottom"></span>';
                        el.onclick = function(e) {
                            if ($this.position == 'top') {
                                $this.toolbarElement.style.setProperty('top', '');
                                $this.toolbarElement.style.setProperty('bottom', '0px')
                                $this.position = 'bottom';
                                $this.el.innerHTML = '<span class="glyphicon glyphicon-triangle-top"></span>';
                            } else if ($this.position = 'bottom') {
                                $this.toolbarElement.style.setProperty('top', '0px');
                                $this.toolbarElement.style.setProperty('bottom', '')
                                $this.position = 'top';
                                $this.el.innerHTML = '<span class="glyphicon glyphicon-triangle-bottom"></span>';
                            }
                        }
                        this.el = el;
                        return el;
                    }
                },
                {
                    name: 'selectTool',
                    enabled: true,
                    showWhenCollapsed: false,
                    selectedTool: null,
                    initialize: function(moose) {
                        var $this = this;
                        var el = document.createElement('button');
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
                        el.onclick = function(e) {
                            $popup.innerHTML = '';
                            for (var i = 0; i < moose.tools.length; i++) {
                                var toolSelector = moose.tools[i].getToolbarElement(
                                        moose.tools[i] == moose.selectedTool,
                                        function (tool) {
                                            moose.selectedTool = tool;
                                            console.log('selected tool ' + tool.name);
                                            $popup.style.display = 'none';
                                            el.innerHTML = tool.iconHtml;
                                    });
                                $popup.appendChild(toolSelector);
                                
                            }
                            $popup.style.display = 'block';
                        };
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
                initialize: function (moose) {
                    var $this = this;
                    var el = document.createElement('button');
                    var fc = moose.foreColor;
                    el.style.backgroundColor = utils.toHslaString(fc);
                    el.onclick = function (e) {
                        if (!$this.opened) {
                            $this.picker.style.display = 'block';
                            $this.picker.style.position = 'absolute';
                            $this.picker.style.top = '0px';
                            $this.picker.style.left = '0px';
                            $this.picker.style.width = (screen.availWidth / moose.viewportScale) + 'px';
                            $this.picker.style.height = (screen.availHeight / moose.viewportScale) + 'px';
                            $this.picker.style['z-index'] = 102;
                            $this.opened = true;
                        } else {
                            $this.picker.style.display = 'none';
                            $this.opened = false;
                        }
                    }
                    el.innerHTML = '&nbsp';
                    this.el = el;

                    var colorPicker = document.createElement('div');
                    colorPicker.style.display = 'none';
                    colorPicker.style.position = 'absolute';
                    colorPicker.style.top = '0';
                    colorPicker.style.left = '1';
                    colorPicker.style['z-index'] = 1;
                    var step = 50;
                    for (var py = 0; py < 10; py++) {
                        var row = document.createElement('div');
                        row.style.width = '100%';
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
                initialize: function (moose) {
                    var container = document.createElement('span');
                    var lbl = document.createElement('label');
                    lbl.attributes['for'] = 'toolSize';
                    lbl.innerHTML = 'Size:' + moose.toolSize;
                    lbl.style['margin-right'] = '1em';
                    lbl.style['width'] = '50px';

                    var el = document.createElement('input');
                    el.type = 'range';
                    el.name = 'toolSize';
                    el.value = moose.toolSize;
                    el.attributes['min'] = '0';
                    el.attributes['max'] = '200';
                    el.attributes['step'] = '1';
                    el.defaultValue = moose.toolSize;
                    el.oninput = el.onchange = function (e) {
                        moose.toolSize = el.value;
                        lbl.innerHTML = 'Size:' + moose.toolSize;
                    };
                    el.style.width = '200px';
                    el.style.setProperty("display", "inline-block", "important");
                    el.style['position'] = 'relative';
                    el.style['top'] = '5px';

                    container.appendChild(lbl);
                    container.appendChild(el);
                    this.el = container;

                    return container;
                }
            },
            {
                name: 'fullscreen',
                enabled: true,
                initialize: function (moose) {
                    var el = document.createElement('button');
                    this.el = el;
                    el.innerHTML = '<span class="glyphicon glyphicon-fullscreen"></span>';
                    el.onclick = function (e) {
                        if (
                              document.fullscreenEnabled ||
                              document.webkitFullscreenEnabled ||
                              document.mozFullScreenEnabled ||
                              document.msFullscreenEnabled
                          ) {

                            if (
                                  document.fullscreenElement ||
                                  document.webkitFullscreenElement ||
                                  document.mozFullScreenElement ||
                                  document.msFullscreenElement
                              ) {
                                if (document.exitFullscreen) {
                                    document.exitFullscreen();
                                } else if (document.webkitExitFullscreen) {
                                    document.webkitExitFullscreen();
                                } else if (document.mozCancelFullScreen) {
                                    document.mozCancelFullScreen();
                                } else if (document.msExitFullscreen) {
                                    document.msExitFullscreen();
                                }
                            } else {
                                if (moose.containerEl.requestFullscreen) {
                                    moose.containerEl.requestFullscreen();
                                } else if (moose.containerEl.webkitRequestFullscreen) {
                                    moose.containerEl.webkitRequestFullscreen();
                                } else if (moose.containerEl.mozRequestFullScreen) {
                                    moose.containerEl.mozRequestFullScreen();
                                } else if (moose.containerEl.msRequestFullscreen) {
                                    moose.containerEl.msRequestFullscreen();
                                }
                            }
                        }
                    };
                    return el;
                }
            },
            {
                name: 'cancel',
                enabled: true,
                initialize: function (moose) {
                    var el = document.createElement('button');
                    el.innerHTML = '<span class="glyphicon glyphicon-remove"></span>';
                    this.el = el;
                    el.onclick = function (e) {
                        moose.onCancel();
                    };
                    return el;
                }
            },
            {
                name: 'save',
                enabled: true,
                initialize: function (moose) {
                    var el = document.createElement('button');
                    el.innerHTML = '<span class="glyphicon glyphicon-floppy-disk"></span>';
                    el.onclick = function (e) {
                        moose.onSave();
                    };
                    this.el = el;
                    return el;
                }
            },
            {
                name: 'zoom-in',
                enabled: this.zoomEnabled,
                initialize: function (moose) {
                    var el = document.createElement('button');
                    el.innerHTML = 'zoom in';
                    el.onclick = function (e) {
                        var newScale = moose.scale * 2;
                        moose.zoom(newScale, moose.scale, 0.5, 0.5);
                        moose.scale = newScale;
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
                    el.innerHTML = 'zoom out';
                    el.onclick = function (e) {
                        var newScale = moose.scale / 2;
                        if (newScale == moose.scale) return;
                        moose.zoom(newScale, moose.scale, 0.5, 0.5);
                        moose.scale = newScale;
                    };
                    this.el = el;
                    return el;
                }
            }
        ],
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
        zoom: function (newScale, oldScale, centerX, centerY) {
            if (newScale < 1) newScale = 1;
            if (newScale > 32) newScale = 32;

            var oldScale;
            oldScale = this.scale;

            var actualWidth = this.width * this.scale;
            var actualHeight = this.height * this.scale;
            var newActualWidth = this.width * newScale;
            var newActualHeight = this.height * newScale;

            var canvasPointX = this.position.x + this.width * centerX * this.scale;
            var canvasPointY = this.position.y + this.height * centerY * this.scale;
            var newCanvasPointX = canvasPointX * (newScale / oldScale);
            var newCanvasPointY = canvasPointY * (newScale / oldScale);
            var deltaX = newCanvasPointX - canvasPointX;
            var deltaY = newCanvasPointY - canvasPointY;

            if (this.position.x + deltaX < 0)
                deltaX = this.position.x;
            else if (this.position.x + deltaX + window.innerWidth > newActualWidth)
                deltaX = newActualWidth - window.innerWidth - this.position.x;

            if (this.position.y + deltaY < 0)
                deltaY = this.position.Y;
            else if (this.position.y + deltaY + window.innerHeight > newActualHeight)
                deltaY = newActualHeight - window.innerHeight - this.position.y;

            this.debug('scale:' + newScale
                + ', t(x,y):' + canvasPointX + ',' + canvasPointY
                + ', t\'(x,y):' + newCanvasPointX + ',' + newCanvasPointY
                + ', d(x,y):' + deltaX + ',' + deltaY);

            this.scale = newScale;

            //this.keepPanInImageBounds();
            this.ctx.scale(newScale / oldScale, newScale / oldScale);
            this.ctx.translate(-deltaX, -deltaY);
            this.position.x += deltaX;
            this.position.y += deltaY;
            //this.debug(newScale);

            //this.debug('pos:'+x+','+y);
            this.redraw();
        },
        drawShapesToCanvas: function (canvas) {
            var ctx = canvas.getContext('2d');

            if (!this.shapes) this.shapes = [];
            for (var s = 0; s < this.shapes.length; s++) {
                var shape = this.shapes[s];
                if (!shape) continue;
                var lastPoint = shape.points[0];
                var tool = this.tools[0];
                if (shape.toolName == 'ink') {
                    tool = this.tools[1];
                } else {
                    tool = this.tools[0];
                }
                this.foreColor = shape.foreColor;
                tool.onPointerStart(this, lastPoint);
                if (shape.points.length <= 0) continue;

                for (var p = 1; p < shape.points.length; p++) {
                    var pt = shape.points[p];
                    tool.onPointerDrag(this, pt);
                    lastPoint = pt;
                }
            }
        },
        updateBuffer: function () {
            var bufferCtx = this.buffer.getContext('2d');
            bufferCtx.clearRect(0, 0, this.width, this.height);
            this.drawShapesToCanvas(this.buffer);
        },
        redraw: function () {
            this.ctx.clearRect(this.position.x, this.position.y, this.width, this.height);
            if (!this.shapes) this.shapes = [];
            this.drawShapesToCanvas(this.canvas);

            var bufferCtx = this.buffer.getContext('2d');
            bufferCtx.clearRect(0, 0, this.width, this.height);
            this.drawShapesToCanvas(this.buffer);
        },
        pallette: [
            [
                { h: 0, s: 1, l: 0, a: 1 }, { h: 0, s: 1, l: 0.1, a: 1 }, { h: 0, s: 1, l: 0.2, a: 1 },
                { h: 0, s: 1, l: 0.3, a: 1 }, { h: 0, s: 1, l: 0.4, a: 1 }, { h: 0, s: 1, l: 0.5, a: 1 },
                { h: 0, s: 1, l: 0.6, a: 1 }, { h: 0, s: 1, l: 0.7, a: 1 }, { h: 0, s: 1, l: 0.8, a: 1 },
                { h: 0, s: 1, l: 0.9, a: 1 },
            ],
            [
                { h: 36, s: 1, l: 0, a: 1 }, { h: 36, s: 1, l: 0.1, a: 1 }, { h: 36, s: 1, l: 0.2, a: 1 },
                { h: 36, s: 1, l: 0.3, a: 1 }, { h: 36, s: 1, l: 0.4, a: 1 }, { h: 36, s: 1, l: 0.5, a: 1 },
                { h: 36, s: 1, l: 0.6, a: 1 }, { h: 36, s: 1, l: 0.7, a: 1 }, { h: 36, s: 1, l: 0.8, a: 1 },
                { h: 36, s: 1, l: 0.9, a: 1 },
            ],
            [
                { h: 72, s: 1, l: 0, a: 1 }, { h: 72, s: 1, l: 0.1, a: 1 }, { h: 72, s: 1, l: 0.2, a: 1 },
                { h: 72, s: 1, l: 0.3, a: 1 }, { h: 72, s: 1, l: 0.4, a: 1 }, { h: 72, s: 1, l: 0.5, a: 1 },
                { h: 72, s: 1, l: 0.6, a: 1 }, { h: 72, s: 1, l: 0.7, a: 1 }, { h: 72, s: 1, l: 0.8, a: 1 },
                { h: 72, s: 1, l: 0.9, a: 1 },
            ],
            [
                { h: 108, s: 1, l: 0, a: 1 }, { h: 108, s: 1, l: 0.1, a: 1 }, { h: 108, s: 1, l: 0.2, a: 1 },
                { h: 108, s: 1, l: 0.3, a: 1 }, { h: 108, s: 1, l: 0.4, a: 1 }, { h: 108, s: 1, l: 0.5, a: 1 },
                { h: 108, s: 1, l: 0.6, a: 1 }, { h: 108, s: 1, l: 0.7, a: 1 }, { h: 108, s: 1, l: 0.8, a: 1 },
                { h: 108, s: 1, l: 0.9, a: 1 },
            ],
            [
                { h: 144, s: 1, l: 0, a: 1 }, { h: 144, s: 1, l: 0.1, a: 1 }, { h: 144, s: 1, l: 0.2, a: 1 },
                { h: 144, s: 1, l: 0.3, a: 1 }, { h: 144, s: 1, l: 0.4, a: 1 }, { h: 144, s: 1, l: 0.5, a: 1 },
                { h: 144, s: 1, l: 0.6, a: 1 }, { h: 144, s: 1, l: 0.7, a: 1 }, { h: 144, s: 1, l: 0.8, a: 1 },
                { h: 144, s: 1, l: 0.9, a: 1 },
            ],
            [
                { h: 180, s: 1, l: 0, a: 1 }, { h: 180, s: 1, l: 0.1, a: 1 }, { h: 180, s: 1, l: 0.2, a: 1 },
                { h: 180, s: 1, l: 0.3, a: 1 }, { h: 180, s: 1, l: 0.4, a: 1 }, { h: 180, s: 1, l: 0.5, a: 1 },
                { h: 180, s: 1, l: 0.6, a: 1 }, { h: 180, s: 1, l: 0.7, a: 1 }, { h: 180, s: 1, l: 0.8, a: 1 },
                { h: 180, s: 1, l: 0.9, a: 1 },
            ],
            [
                { h: 216, s: 1, l: 0, a: 1 }, { h: 216, s: 1, l: 0.1, a: 1 }, { h: 216, s: 1, l: 0.2, a: 1 },
                { h: 216, s: 1, l: 0.3, a: 1 }, { h: 216, s: 1, l: 0.4, a: 1 }, { h: 216, s: 1, l: 0.5, a: 1 },
                { h: 216, s: 1, l: 0.6, a: 1 }, { h: 216, s: 1, l: 0.7, a: 1 }, { h: 216, s: 1, l: 0.8, a: 1 },
                { h: 216, s: 1, l: 0.9, a: 1 },
            ],
            [
                { h: 252, s: 1, l: 0, a: 1 }, { h: 252, s: 1, l: 0.1, a: 1 }, { h: 252, s: 1, l: 0.2, a: 1 },
                { h: 252, s: 1, l: 0.3, a: 1 }, { h: 252, s: 1, l: 0.4, a: 1 }, { h: 252, s: 1, l: 0.5, a: 1 },
                { h: 252, s: 1, l: 0.6, a: 1 }, { h: 252, s: 1, l: 0.7, a: 1 }, { h: 252, s: 1, l: 0.8, a: 1 },
                { h: 252, s: 1, l: 0.9, a: 1 },
            ],
            [
                { h: 288, s: 1, l: 0, a: 1 }, { h: 288, s: 1, l: 0.1, a: 1 }, { h: 288, s: 1, l: 0.2, a: 1 },
                { h: 288, s: 1, l: 0.3, a: 1 }, { h: 288, s: 1, l: 0.4, a: 1 }, { h: 288, s: 1, l: 0.5, a: 1 },
                { h: 288, s: 1, l: 0.6, a: 1 }, { h: 288, s: 1, l: 0.7, a: 1 }, { h: 288, s: 1, l: 0.8, a: 1 },
                { h: 288, s: 1, l: 0.9, a: 1 },
            ],
            [
                { h: 324, s: 1, l: 0, a: 1 }, { h: 324, s: 1, l: 0.1, a: 1 }, { h: 324, s: 1, l: 0.2, a: 1 },
                { h: 324, s: 1, l: 0.3, a: 1 }, { h: 324, s: 1, l: 0.4, a: 1 }, { h: 324, s: 1, l: 0.5, a: 1 },
                { h: 324, s: 1, l: 0.6, a: 1 }, { h: 324, s: 1, l: 0.7, a: 1 }, { h: 324, s: 1, l: 0.8, a: 1 },
                { h: 324, s: 1, l: 0.9, a: 1 },
            ],
            [
                { h: 360, s: 1, l: 0, a: 1 }, { h: 360, s: 1, l: 0.1, a: 1 }, { h: 360, s: 1, l: 0.2, a: 1 },
                { h: 360, s: 1, l: 0.3, a: 1 }, { h: 360, s: 1, l: 0.4, a: 1 }, { h: 360, s: 1, l: 0.5, a: 1 },
                { h: 360, s: 1, l: 0.6, a: 1 }, { h: 360, s: 1, l: 0.7, a: 1 }, { h: 360, s: 1, l: 0.8, a: 1 },
                { h: 360, s: 1, l: 0.9, a: 1 },
            ]
        ],
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
            t.className = 'toolbar';
            t.attributes['id'] = 'toolbar';
            t.style.setProperty('top', '0px');
            t.style.setProperty('left', '0px');
            t.style.backgroundColor = '#fff';
            //t.style['font-size'] = '5em';
            t.style['z-index'] = 1;

            for (var i = 0; i < this.toolbarItems.length; i++) {
                var ti = this.toolbarItems[i];
                if (!ti || !ti.enabled) continue;
                var el = ti.initialize(this, t);
                el.className += 'toolbar-item';
                t.appendChild(el);
            }

            return t;
        },
        startDrawingShape: function(point) {
            this.isDrawing = true;
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
        bindEvents: function () {
            this.canvas.onmousedown = function (e) {
                var moose = this.moose;
                var point = { x: e.clientX, y: e.clientY };
                if (e.shiftKey) {
                    moose.zoom(moose.scale * 1.2, moose.scale, point.x / window.innerWidth, point.y / window.innerHeight);
                } else if (e.ctrlKey) {
                    moose.zoom(moose.scale * 0.8, moose.scale, point.x / window.innerWidth, point.y / window.innerHeight);
                } else {
                    moose.startDrawingShape(point);
                }
            }
            this.canvas.onmousemove = function (e) {
                var moose = this.moose;
                if (moose.mouseOut) {
                    moose.lastPoint = { x: e.clientX /= moose.scale, y: e.clientY /= moose.scale };
                    moose.mouseOut = false;
                }
                if (!moose.isDrawing) return;
                var currentPoint = { x: e.clientX, y: e.clientY };
                currentPoint.x *= moose.scale;
                currentPoint.y *= moose.scale;
                moose.selectedTool.onPointerDrag(moose, currentPoint);
            }

            this.canvas.onmouseup = function () {
                var moose = this.moose;
                moose.isDrawing = false;
                moose.selectedTool.onPointerStop(moose);
            };
            this.canvas.onmouseout = function () {
                var moose = this.moose;
                moose.mouseOut = true;
            };
            this.canvas.addEventListener('touchmove',
                function (e) {
                    e.preventDefault();

                    var moose = this.moose;
                    var touches = e.changedTouches;
                    if (touches.length === 1) {
                        var currentPoint = { x: touches[0].pageX * moose.scale + moose.position.x, y: touches[0].pageY * moose.scale + moose.position.y };
                        //moose.debug('touch move');
                        //alert('touch move at ' + touches[0].pageX + ',' + touches[0].pageY);
                        moose.selectedTool.onPointerDrag(moose, currentPoint);
                    }
                });
            this.canvas.addEventListener('touchend',
                function (e) {
                    e.preventDefault();
                    var moose = this.moose;
                    moose.isDrawing = false;
                    moose.selectedTool.onPointerStop(moose);
                });
            this.canvas.addEventListener('touchstart',
                function (e) {
                    if (e.target.tagName.toLowerCase() !== 'canvas') {
                        return;
                    }

                    var moose = this.moose;
                    e.preventDefault();
                    var touches = e.changedTouches;
                    if (e.touches.length === 1) {
                        moose.isDrawing = true;
                        moose.lastPoint = { x: touches[0].pageX * moose.scale + moose.position.x, y: touches[0].pageY * moose.scale + moose.position.y };
                        moose.startDrawingShape(point);
                        document.addEventListener('touchmove', touchMoveListener);
                        document.addEventListener('touchend', touchEndListener);
                        return document.addEventListener('touchcancel', touchEndListener);
                    }
                });

            var Shape;
            Shape = function () {
                return this;
            };
            Shape.prototype.helloWorld = function () {
                console.log('hello world');
            };

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
                        moose.scaleAtPinchStart = moose.scale
                        moose.offsetAtPinchStart = moose.offset;
                    });
            moose.hammertime.on('pinchmove',
                    function (ev) {
                        try {
                            var moose = ev.target.moose;
                            var newScale = moose.scaleAtPinchStart * (ev.scale);
                            moose.zoom(newScale, moose.scale, ev.center.x, ev.center.y);
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