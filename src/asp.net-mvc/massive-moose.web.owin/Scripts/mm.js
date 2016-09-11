var MassiveMoose = (function () {
    return {
        cfg: { baseApiUrl: '', drawZoom: 0.8 , viewportScale:1.0, viewportScaleWhenDrawing:0.7, inviteCode:''},
        initialize: function(configuration) {
            cfg = configuration;
            wallETag = '0';
            xhrWaitHandle = null;
            var xhr = new XMLHttpRequest();
            xhr.onreadystatechange = function(evt) {

            }
            
            viewport = document.querySelector("meta[name=viewport]");
            document.getElementById('diagnostics').innerHTML = '<div>'+viewport.getAttribute('content')+'</div>';
            document.getElementById('diagnostics').innerHTML += '<div>inner:'+window.innerWidth+','+window.innerHeight+'</div>';
//            xhr.addEventListener("progress", updateProgress);
//            xhr.addEventListener("load", transferComplete);
//            xhr.addEventListener("error", transferFailed);
//            xhr.addEventListener("abort", transferCanceled);
//            xhr.addEventListener("loadstart", transferStarted);

            function updateProgress(evt) {

            }
            function transferComplete(evt) {

                document.getElementById('progress').style.display = 'none';
            }
            function transferFailed(evt) {

                document.getElementById('progress').style.display = 'none';
            }
            function transferCanceled(evt) {

                document.getElementById('progress').style.display = 'none';
            }
            function transferStarted(evt) {

            }

            function setViewScale() {
                var vs = cfg.viewPortScale;
                if (cfg.firstTime) {
                    vs=1.0;
                }
                else if (_brickInUse) {
                    vs=cfg.viewportScaleWhenDrawing;
                }
                viewport = document.querySelector("meta[name=viewport]");
                viewport.setAttribute('content', 'width=device-width, initial-scale=' + vs);

            }

            var fontState ={
                    isItalic: false,
                    isBold: false,
                    fontName: 'Helvetica',
                    fontSizeIndex: 4
                };


            SANS_SERIF_FONTS = [['Arial', 'Arial,"Helvetica Neue",Helvetica,sans-serif'], ['Arial Black', '"Arial Black","Arial Bold",Gadget,sans-serif'], ['Arial Narrow', '"Arial Narrow",Arial,sans-serif'], ['Gill Sans', '"Gill Sans","Gill Sans MT",Calibri,sans-serif'], ['Helvetica', '"Helvetica Neue",Helvetica,Arial,sans-serif'], ['Impact', 'Impact,Haettenschweiler,"Franklin Gothic Bold",Charcoal,"Helvetica Inserat","Bitstream Vera Sans Bold","Arial Black",sans-serif'], ['Tahoma', 'Tahoma,Verdana,Segoe,sans-serif'], ['Trebuchet MS', '"Trebuchet MS","Lucida Grande","Lucida Sans Unicode","Lucida Sans",Tahoma,sans-serif'], ['Verdana', 'Verdana,Geneva,sans-serif']].map(function (_arg) {
                var name, value;
                name = _arg[0], value = _arg[1];
                return {
                    name: name,
                    value: value
                };
            });

            SERIF_FONTS = [['Baskerville', 'Baskerville,"Baskerville Old Face","Hoefler Text",Garamond,"Times New Roman",serif'], ['Garamond', 'Garamond,Baskerville,"Baskerville Old Face","Hoefler Text","Times New Roman",serif'], ['Georgia', 'Georgia,Times,"Times New Roman",serif'], ['Hoefler Text', '"Hoefler Text","Baskerville Old Face",Garamond,"Times New Roman",serif'], ['Lucida Bright', '"Lucida Bright",Georgia,serif'], ['Palatino', 'Palatino,"Palatino Linotype","Palatino LT STD","Book Antiqua",Georgia,serif'], ['Times New Roman', 'TimesNewRoman,"Times New Roman",Times,Baskerville,Georgia,serif']].map(function (_arg) {
                var name, value;
                name = _arg[0], value = _arg[1];
                return {
                    name: name,
                    value: value
                };
            });

            MONOSPACE_FONTS = [['Consolas/Monaco', 'Consolas,monaco,"Lucida Console",monospace'], ['Courier New', '"Courier New",Courier,"Lucida Sans Typewriter","Lucida Typewriter",monospace'], ['Lucida Sans Typewriter', '"Lucida Sans Typewriter","Lucida Console",monaco,"Bitstream Vera Sans Mono",monospace']].map(function (_arg) {
                var name, value;
                name = _arg[0], value = _arg[1];
                return {
                    name: name,
                    value: value
                };
            });

            OTHER_FONTS = [['Copperplate', 'Copperplate,"Copperplate Gothic Light",fantasy'], ['Papyrus', 'Papyrus,fantasy'], ['Script', '"Brush Script MT",cursive']].map(function (_arg) {
                var name, value;
                name = _arg[0], value = _arg[1];
                return {
                    name: name,
                    value: value
                };
            });

            ALL_FONTS = [['Sans Serif', SANS_SERIF_FONTS], ['Serif', SERIF_FONTS], ['Monospace', MONOSPACE_FONTS], ['Other', OTHER_FONTS]];

            FONT_NAME_TO_VALUE = {};

            for (_i = 0, _len = SANS_SERIF_FONTS.length; _i < _len; _i++) {
                _ref = SANS_SERIF_FONTS[_i], name = _ref.name, value = _ref.value;
                FONT_NAME_TO_VALUE[name] = value;
            }

            for (_j = 0, _len1 = SERIF_FONTS.length; _j < _len1; _j++) {
                _ref1 = SERIF_FONTS[_j], name = _ref1.name, value = _ref1.value;
                FONT_NAME_TO_VALUE[name] = value;
            }

            for (_k = 0, _len2 = MONOSPACE_FONTS.length; _k < _len2; _k++) {
                _ref2 = MONOSPACE_FONTS[_k], name = _ref2.name, value = _ref2.value;
                FONT_NAME_TO_VALUE[name] = value;
            }

            for (_l = 0, _len3 = OTHER_FONTS.length; _l < _len3; _l++) {
                _ref3 = OTHER_FONTS[_l], name = _ref3.name, value = _ref3.value;
                FONT_NAME_TO_VALUE[name] = value;
            }


            function getFontSizes() {
                return [9, 10, 12, 14, 18, 24, 36, 48, 64, 72, 96, 144, 288];
            }

            function updateFontSettings(newState) {
                var fontSize, items, k;
                if (newState == null) {
                    newState = {};
                }
                for (k in fontState) {
                    if (!(k in newState)) {
                        newState[k] = fontState[k];
                    }
                }
                fontSize = getFontSizes()[newState.fontSizeIndex];
                items = [];
                if (newState.isItalic) {
                    items.push('italic');
                }
                if (newState.isBold) {
                    items.push('bold');
                }
                items.push("" + fontSize + "px");
                items.push(FONT_NAME_TO_VALUE[newState.fontName]);
                fontState = newState;
                _lc.tool.font = items.join(' ');
                return _lc.trigger('setFont', items.join(' '));
            }

            document.getElementById('font-face').onchange = function (e) {
                var newState= {
                    fontName:e.target.value
                }
                updateFontSettings(newState);
            };

            document.getElementById('font-size').onchange = function(e) {
                var newState = {
                    fontSizeIndex: e.target.value
                }
                updateFontSettings(newState);
            }

            document.getElementById('font-italic').onchange = function() {
                var newState = {
                    isItalic: e.target.isChecked
                }
                updateFontSettings(newState);
            };

            document.getElementById('font-bold').onchange = function (e) {
                var newState = {
                    isBold: e.target.isChecked
                }
                updateFontSettings(newState);
            };

            function showTextOptions(){
                document.getElementById('textTools').style.display='inline-block';
                document.getElementById('drawTools').style.display = 'none';
                updateFontSettings(null);
            }
            function showDrawingOptions(){
                document.getElementById('drawTools').style.display='inline-block';
                document.getElementById('textTools').style.display='none';
            }

            _viewportScaleWhenDrawing = cfg.viewPortScaleWhenDrawing;
            _viewportScale = cfg.viewPortScale;
            _drawZoom = cfg.drawZoom;
            _baseApiUrl = cfg.baseApiUrl;
            _inviteCode = cfg.inviteCode;

            _brickInUse = null;
            _lc = null;
            _wall = null;
            _toolsWaitHandle = 0;

            setTimeout(checkWallStaleness, 10);

            document.getElementById('toolbar-size').onclick = function() {
                var toolsInner = document.getElementById('tools-inner');
                if (toolsInner.style.display == '' || toolsInner.style.display=='inline-block') {
                    toolsInner.style.display = 'none';
                    document.getElementById('toolbar-size').getElementsByTagName('span')[0].className = 'glyphicon glyphicon-plus';
                } else {
                    toolsInner.style.display = 'inline-block';
                    document.getElementById('toolbar-size').getElementsByTagName('span')[0].className = 'glyphicon glyphicon-minus';
                }
            }

            document.getElementById('tools-wrapper').style.top = '0px';
            document.getElementById('tools-wrapper').style.bottom = '';
            document.getElementById('toolbar-dock').onclick = function () {
                if (document.getElementById('tools-wrapper').style.top == '' &&
                    document.getElementById('tools-wrapper').style.bottom == '') {
                    document.getElementById('tools-wrapper').style.bottom = '0px';
                    document.getElementById('toolbar-dock').getElementsByTagName('span')[0].className = 'glyphicon glyphicon-chevron-up';
                } else if (document.getElementById('tools-wrapper').style.top == '0px') {
                    document.getElementById('tools-wrapper').style.bottom = '0px';
                    document.getElementById('tools-wrapper').style.top = '';
                    document.getElementById('toolbar-dock').getElementsByTagName('span')[0].className = 'glyphicon glyphicon-chevron-up';
                } else {
                    document.getElementById('tools-wrapper').style.top = '0px';
                    document.getElementById('tools-wrapper').style.bottom = '';
                    document.getElementById('toolbar-dock').getElementsByTagName('span')[0].className = 'glyphicon glyphicon-chevron-down';
                }
            }

            if (document.getElementById('help')) {
                document.getElementById('moreHelp1')
                    .onclick = function() {
                        xhr.open('GET', '/Home/Help');
                        xhr.setRequestHeader('Content-Type', 'text/html');
                        xhr.onload = function () {
                            document.getElementById('help-scroller').style.overflowY = 'scroll';
                            document.getElementById('help-full').innerHTML = xhr.responseText;
                            document.getElementById('help-full-container').style.display = 'block';
                            document.getElementById('help-question').style.display = 'none';
                        }
                        xhr.send();
                    };
                document.getElementById('noHelpThanks1').onclick = function () {
                    SetDontHelpMe();
                    document.getElementById('help').style.display = 'none';
                };
                document.getElementById('help-close').onclick = function () {
                    SetDontHelpMe();
                    document.getElementById('help').style.display = 'none';
                };
                document.getElementById('moreHelp2')
                    .onclick = function () {
                        xhr.open('GET', '/Home/Help');
                        xhr.setRequestHeader('Content-Type', 'text/html');
                        xhr.onload = function () {
                            document.getElementById('help-scroller').style.overflowY = 'scroll';
                            document.getElementById('help-full').innerHTML = xhr.responseText;
                            document.getElementById('help-full-container').style.display = 'block';
                            document.getElementById('help-question').style.display = 'none';
                        }
                        xhr.send();
                    };
                document.getElementById('noHelpThanks2').onclick = function () {
                    SetDontHelpMe();
                    document.getElementById('help').style.display = 'none';
                };
                document.getElementById('noHelpThanks3').onclick = function () {
                    SetDontHelpMe();
                    document.getElementById('help').style.display = 'none';
                };
                document.getElementById('help-close').onclick = function () {
                    SetDontHelpMe();
                    document.getElementById('help').style.display = 'none';
                };

                function SetDontHelpMe() {
                    xhr.open('POST', '/Home/DontHelpMe');
                    xhr.send();
                }
            }

            var bricks = document.getElementsByClassName('brick');
            for (var ei = 0; ei < bricks.length; ei++) {
                var b = document.getElementById(bricks[ei].id);
                b.onclick = function(e) {
                    openSession(e.currentTarget.attributes.getNamedItem('data-viewx').value,
                        e.currentTarget.attributes.getNamedItem('data-viewy').value,
                        e.currentTarget.attributes.getNamedItem('data-addressx').value,
                        e.currentTarget.attributes.getNamedItem('data-addressy').value);
                };
                
            }

            function openCanvas(brick) {
                document.getElementById('wall').style.display = 'none';
                document.getElementById('drawSpace').style.display = 'block';
                document.getElementById('tools-wrapper').style.display = 'block';
                document.getElementById('save-button').disabled = '';
                document.getElementById('cancel-button').disabled = '';
                document.getElementById('alert').style.display = 'none';

                // configure literally canvas
                _lc = LC.init(document.getElementById('drawSpace'),
                {
                    imageURLPrefix: "/content/img",
                    imageSize: { width: 1600, height: 800 },
                    backgroundColor: "transparent",
                    toolbarPosition: 'top'
                });
                var tools = [
                    {
                    name: 'pencil',
                    el: document.getElementById('tool-pencil'),
                    tool: new LC.tools.Pencil(_lc)
                  },
                  {
                    name: 'eraser',
                    el: document.getElementById('tool-eraser'),
                    tool: new LC.tools.Eraser(_lc)
                  },
                  {
                      name: 'line',
                      el: document.getElementById('tool-line'),
                      tool: new LC.tools.Line(_lc)
                  },
                  {
                      name: 'rectangle',
                      el: document.getElementById('tool-rectangle'),
                      tool: new LC.tools.Rectangle(_lc)
                  },
                  {
                      name: 'ellipse',
                      el: document.getElementById('tool-ellipse'),
                      tool: new LC.tools.Ellipse(_lc)
                  },
                  {
                      name: 'polygon',
                      el: document.getElementById('tool-polygon'),
                      tool: new LC.tools.Polygon(_lc)
                  },
                  {
                      name: 'text',
                      el: document.getElementById('tool-text'),
                      tool: new LC.tools.Text(_lc)
                  },
                  {
                      name: 'pan',
                      el: document.getElementById('tool-pan'),
                      tool: new LC.tools.Pan(_lc)
                  }
                ];

                var activateTool = function (t) {
                    _lc.setTool(t.tool);
                    if (t.tool.name == 'Text') {
                        showTextOptions();
                    } else {
                        showDrawingOptions();
                    }

                    tools.forEach(function(t2) {
                    if (t == t2) {
                        t2.el.style.backgroundColor = 'yellow';
                    } else {
                        t2.el.style.backgroundColor = 'transparent';
                    }
                    });
                }

                var strokeWidths = [
                    {
                        el: document.getElementById('stroke-1'),
                        strokeWidth:1
                    },
                    {
                        el: document.getElementById('stroke-2'),
                        strokeWidth: 2
                    },
                    {
                        el: document.getElementById('stroke-3'),
                        strokeWidth: 5
                    },
                    {
                        el: document.getElementById('stroke-4'),
                        strokeWidth: 10
                    },
                    {
                        el: document.getElementById('stroke-5'),
                        strokeWidth: 20
                    },
                    {
                        el: document.getElementById('stroke-6'),
                        strokeWidth: 30
                    }
                ];
                strokeWidths.forEach(function(sw) {
                    sw.el.onclick = function(e) {
                        _lc.tool.strokeWidth = sw.strokeWidth;
                    }
                    sw.el.style.cursor = "pointer";
                });

                var zoomIn = document.getElementById('zoom-in');
                zoomIn.style.cursor = 'pointer';
                zoomIn.onclick =function (e) {
                        _lc.zoom(_lc.config.zoomStep);
                };

                var zoomOut = document.getElementById('zoom-out');
                zoomOut.style.cursor = 'pointer';
                zoomOut.onclick =function (e) {
                        _lc.zoom(-_lc.config.zoomStep);
                };

                var undo = document.getElementById('undo');
                undo.style.cursor = 'pointer';
                undo.onclick = function(e) {
                    _lc.undo();
                }
                var redo = document.getElementById('redo');
                redo.style.cursor = 'pointer';
                redo.onclick = function (e) {
                    _lc.redo();
                }


                tools.forEach(function(t) {
                  t.el.style.cursor = "pointer";
                  t.el.onclick = function(e) {
                      e.preventDefault();
                    activateTool(t);
                  };
                });
                activateTool(tools[0]);
                updateFontSettings(null);
                //finish configuring

                var uploadButton = document.getElementById('save-button').onclick = ClickUpload;
                var cancelButton = document.getElementById('cancel-button').onclick = ClickCancel;

                var unsubscribeOnDrawStart = _lc.on('drawStart', function (arguments) {
                    if (document.getElementById('tools-wrapper').style.display != 'none') {
                        document.getElementById('tools-wrapper').style.display = 'none';
                    }
                    hideColorPickers();
                    // do stuff
                });
                var unsubscribeOnDrawEnd = _lc.on('drawEnd', function (arguments) {
                    if (_toolsWaitHandle != 0) {
                        clearTimeout(_toolsWaitHandle);
                    }
                    _toolsWaitHandle = setTimeout(showTools, cfg.toolbarShowDelay);
                    // do stuff
                });

                function showTools() {
                    if (document.getElementById('tools-wrapper').style.display == 'none') {
                        document.getElementById('tools-wrapper').style.display = 'block';
                    }
                    if (_toolsWaitHandle != 0) {
                        clearTimeout(_toolsWaitHandle);
                    }
                }

                if (brick.snapshotJson) {
                    _lc.loadSnapshot(JSON.parse(brick.snapshotJson));
                }

                var zoomAmount = _drawZoom;
                viewport = document.querySelector("meta[name=viewport]");
                viewport.setAttribute('content', 'width=device-width, initial-scale=' + _viewportScaleWhenDrawing);

                var adjustedWidth = 1600 * zoomAmount;
                if (window.innerWidth < adjustedWidth) {
                    document.body.style.minWidth = '0px';
                    document.body.style.minHeight = '0px';

                    _lc.setZoom((window.innerWidth - 100) / adjustedWidth);
                }
            }

            function checkWallStaleness() {
                if (xhr.readyState == 0 || xhr.readyState == 4) {
                    xhr.open('HEAD', _baseApiUrl + '/v2/wall/' + _inviteCode + '/0/0/' + wallETag);
                    xhr.setRequestHeader('If-None-Match', wallETag);
                    xhr.onload = function() {
                        if (xhr.status == 200) {
                            wallETag = xhr.getResponseHeader('ETag');
                            if (wallETag) {
                                wallETag = wallETag.replace("\"", "").replace("\"", "")
                            }
                            updateWall();
                        }
                        setTimeout(checkWallStaleness, 10000);
                    }
                    xhr.send();
                } else {
                    setTimeout(checkWallStaleness, 10000);
                }
            }

            function updateWall(updatedBrickElement) {
                if (!_brickInUse) {
                    document.getElementById('progress').style.display = 'none';
                    setViewScale();
                    xhr.open('GET',_baseApiUrl + '/v2/wall/' + _inviteCode + '/0/0');
                    xhr.setRequestHeader('Content-Type', 'application/json');
                    xhr.onload = function() {
                        if (xhr.status === 200) {
                            var data = JSON.parse(xhr.responseText);
                            document.getElementById('wall').style.display = 'block';
                            document.getElementById('drawSpace').style.display = 'none';
                            for (var y = 0; y < data.length; y++) {

                                for (var x = 0; x < data[y].length; x++) {
                                    var brickView = document.getElementById('c' + (y * 12 + x));
                                    brickView.setAttribute('data-viewX', x);
                                    brickView.setAttribute('data-viewY', y);
                                    var brick = data[x][y];
                                    brick.element = brickView;

                                    if (!brick) {
                                        brickView.className = 'brick free';
                                        continue;
                                    }

                                    if (brick.c == 1
                                        && (_wall && _wall[x][y])) {
                                        if (brickView.getAttribute('data-updated') == '0') {
                                            console.log('new brick ' + x + ', ' + y);
                                        }

                                        // TODO: only update source image if the brick.D value is different to the data-updated attribute on the element
                                        if (brick.d != brickView.getAttribute('data-updated')) {
                                            brickView.style.backgroundImage = 'url("' +
                                                _baseApiUrl +
                                                '/v1/image/t' +
                                                '/' +
                                                _inviteCode +
                                                '/' +
                                                brick.x +
                                                '/' +
                                                brick.y +
                                                '?d=' +
                                                brick.d +
                                                '")';
                                        }
                                    }
                                    if (brick.u != 1) {
                                        brickView.innerHTML = '';
                                        brickView.className = 'brick free';
                                    } else {
                                        brickView
                                            .innerHTML = '<span class="bc iu glyphicon glyphicon-ban-circle"></span>';
                                        brickView.className = 'brick inuse';
                                    }
                                    brickView.setAttribute('data-inuse', brick.u);
                                    brickView.setAttribute('data-hascontent', brick.c);
                                    brickView.setAttribute('data-addressx', brick.x);
                                    brickView.setAttribute('data-addressY', brick.y);
                                    brickView.setAttribute('data-updated', brick.d);
                                }
                            }
                            _wall = data;
                            //document.body.style.minWidth = '1600px';
                            //document.body.style.minHeight = '900px';
                            setViewScale();
                            if (updatedBrickElement)
                                updatedBrickElement.scrollIntoView();
                            setTimeout(checkWallStaleness, 10000);
                        }
                    };
                    xhr.send();
                } else {

                }


            }

            function openSession(x, y, addressX, addressY) {
                if (!addressX || !addressY)
                    return;

                xhr.open('POST', _baseApiUrl + '/v1/' + _inviteCode + '/draw/' + addressX + '/' + addressY);
                xhr.setRequestHeader('Content-Type', 'application/json');
                xhr.onload = function() {
                    if (xhr.status === 200) {
                        var data = JSON.parse(xhr.responseText);
                        _brickInUse = _wall[x][y];
                        _brickInUse.sessionToken = data.sessionToken;
                        _brickInUse.snapshotJson = data.snapshotJson;
                        openCanvas(_brickInUse);
                    } else if (xhr.status == 409) {
                        alert('someone is currently drawing on that space')
                        updateWall();
                    } else {
                        alert('something went terribly, terribly wrong. call your lawyers.');
                    }
                };
                xhr.send();
            }

            function ClickUpload(e) {
                e.preventDefault();
                hideColorPickers();
                document.getElementById('save-button').disabled = 'disabled';
                document.getElementById('cancel-button').disabled = 'disabled';
                document.getElementById('progress').style.display = 'inline-block';
                xhr.open('POST', _baseApiUrl + '/v1/save/'+_brickInUse.sessionToken, true);
                xhr.setRequestHeader('Content-Type', 'application/json');
                xhr.onload = function () {
                    if (xhr.status === 200) {
                        var brickElement = _brickInUse.element;
                        if (brickElement) {
                            brickElement.style.backgroundImage = 'url("' +
                                    _baseApiUrl +
                                    '/v1/image/t' +
                                    '/' + _inviteCode +
                                    '/' + _brickInUse.x +
                                    '/' + _brickInUse.y +
                                    '?r=' +
                                    Math.floor((Math.random() * 10000) + 1) +
                                    '")';
                            brickElement.scrollIntoView();
                            setViewScale();
                        }

                        _brickInUse = null;
                        _lc = null;
                        document.getElementById('drawSpace').style.display = 'none';
                        document.getElementById('tools-wrapper').style.display = 'none';
                        document.getElementById('wall').style.display = 'block';
                        document.getElementById('progress').style.display = 'block';

                        // wait a second before updating to give Azure a chance to propagate the thumbnail image
                        setTimeout(function () {
                            updateWall(brickElement);
                        }, 5000);
                    } else {
                        document.getElementById('save-button').disabled = '';
                        document.getElementById('cancel-button').disabled = '';
                        document.getElementById('alert-message').innerHtml = 'Sorry! There was a problem saving the image... try again? :/';
                        document.getElementById('alert').style.display = 'block';
                        document.getElementById('progress').style.display = 'block';
                    }
                };
                var imageSize = { width: 1600, height: 800 };
                var imageBounds = {
                    x: 0, y: 0, width: imageSize.width, height: imageSize.height
                };
                xhr.send('{"snapshotJson":"' + escape(JSON.stringify(_lc.getSnapshot())) + '","imageData":"' + _lc.getImage({ rect: imageBounds }).toDataURL() + '"}');
            }

            function hideColorPickers() {
                var colorPickers = document.getElementsByClassName('cp-app');
                var eli;
                for (eli = 0; eli < colorPickers.length; eli++) {
                    colorPickers[eli].style.display = 'none';
                }
            }

            function ClickCancel(e) {
                e.preventDefault();
                hideColorPickers();
                document.getElementById('save-button').disabled = 'disabled';
                document.getElementById('cancel-button').disabled = 'disabled';
                document.getElementById('progress').style.display = 'inline-block';

                xhr.open('POST', _baseApiUrl + '/v1/release/' + _brickInUse.sessionToken);
                xhr.setRequestHeader('Content-Type', 'application/json');
                xhr.onload = function() {
                    if (xhr.status === 200) {
                        _brickInUse = null;
                        _lc = null;
                        document.getElementById('drawSpace').style.display = 'none';
                        document.getElementById('tools-wrapper').style.display = 'none';
                        document.getElementById('wall').style.display = 'block';
                        updateWall();
                    } else {
                        alert('there was a problem saving.. err... sorry... try again? :/');
                        document.getElementById('save-button').disabled = '';
                        document.getElementById('cancel-button').disabled = '';
                    }
                };
                xhr.send();

                return false;
            }

        },
        
    }
})();

/**
 * JavaScript code to detect available availability of a
 * particular font in a browser using JavaScript and CSS.
 *
 * Author : Lalit Patel
 * Website: http://www.lalit.org/lab/javascript-css-font-detect/
 * License: Apache Software License 2.0
 *          http://www.apache.org/licenses/LICENSE-2.0
 * Version: 0.15 (21 Sep 2009)
 *          Changed comparision font to default from sans-default-default,
 *          as in FF3.0 font of child element didn't fallback
 *          to parent element if the font is missing.
 * Version: 0.2 (04 Mar 2012)
 *          Comparing font against all the 3 generic font families ie,
 *          'monospace', 'sans-serif' and 'sans'. If it doesn't match all 3
 *          then that font is 100% not available in the system
 * Version: 0.3 (24 Mar 2012)
 *          Replaced sans with serif in the list of baseFonts
 */

/**
 * Usage: d = new Detector();
 *        d.detect('font name');
 */
var Detector = function () {
    // a font will be compared against all the three default fonts.
    // and if it doesn't match all 3 then that font is not available.
    var baseFonts = ['monospace', 'sans-serif', 'serif'];

    //we use m or w because these two characters take up the maximum width.
    // And we use a LLi so that the same matching fonts can get separated
    var testString = "mmmmmmmmmmlli";

    //we test using 72px font size, we may use any size. I guess larger the better.
    var testSize = '72px';

    var h = document.getElementsByTagName("body")[0];

    // create a SPAN in the document to get the width of the text we use to test
    var s = document.createElement("span");
    s.style.fontSize = testSize;
    s.innerHTML = testString;
    var defaultWidth = {};
    var defaultHeight = {};
    for (var index in baseFonts) {
        //get the default width for the three base fonts
        s.style.fontFamily = baseFonts[index];
        h.appendChild(s);
        defaultWidth[baseFonts[index]] = s.offsetWidth; //width for the default font
        defaultHeight[baseFonts[index]] = s.offsetHeight; //height for the defualt font
        h.removeChild(s);
    }

    function detect(font) {
        var detected = false;
        for (var index in baseFonts) {
            s.style.fontFamily = font + ',' + baseFonts[index]; // name of the font along with the base font for fallback.
            h.appendChild(s);
            var matched = (s.offsetWidth != defaultWidth[baseFonts[index]] || s.offsetHeight != defaultHeight[baseFonts[index]]);
            h.removeChild(s);
            detected = detected || matched;
        }
        return detected;
    }

    this.detect = detect;
};
