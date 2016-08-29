var MassiveMoose = (function () {
    return {
        cfg: { baseApiUrl: '', drawZoom: 0.8 , viewportScale:1.0, viewportScaleWhenDrawing:0.7, inviteCode:''},
        initialize: function(configuration) {
            cfg = configuration;

            _viewportScaleWhenDrawing = cfg.viewPortScaleWhenDrawing;
            _viewportScale = cfg.viewPortScale;
            _drawZoom = cfg.drawZoom;
            _baseApiUrl = cfg.baseApiUrl;
            _inviteCode = cfg.inviteCode;

            dimensions();

            _brickInUse = null;
            _lc = null;
            _wall = null;
            _toolsWaitHandle = 0;

            setTimeout(updateWall, 10);

            if (document.getElementById('help')) {
                document.getElementById('moreHelp')
                    .onclick = function() {
                        var xhr = new XMLHttpRequest();
                        xhr.open('GET', '/Home/Help');
                        xhr.setRequestHeader('Content-Type', 'text/html');
                        xhr.onload = function () {
                            document.getElementById('help-scroller').style.maxHeight = '90%';
                            document.getElementById('help-scroller').style.overflowY = 'scroll';
                            document.getElementById('help-full').innerHTML = xhr.responseText;
                            document.getElementById('help-full').style.display = 'block';
                            document.getElementById('help-question').style.display = 'none';
                        }
                        xhr.send();
                    };
                document.getElementById('help-close').onclick = function() {
                    SetDontHelpMe();
                    document.getElementById('help').style.display = 'none';
                };
                document.getElementById('noHelpThanks').onclick = function() {
                    SetDontHelpMe();
                    document.getElementById('help').style.display = 'none';
                };

                function SetDontHelpMe() {
                    var xhr = new XMLHttpRequest();
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
                //finish configuring

                var uploadButton = document.getElementById('save-button').onclick = ClickUpload;
                var cancelButton = document.getElementById('cancel-button').onclick = ClickCancel;

                var unsubscribeOnDrawStart = _lc.on('drawStart', function (arguments) {
                    if (document.getElementById('tools-wrapper').style.display != 'none') {
                        document.getElementById('tools-wrapper').style.display = 'none';
                    }
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

            function updateWall(updatedBrickElement) {
                if (!_brickInUse) {
                    viewport = document.querySelector("meta[name=viewport]");
                    viewport.setAttribute('content', 'width=device-width, initial-scale=' + _viewportScale);
                    var xhr = new XMLHttpRequest();
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
                                    var brick = data[x][y];
                                    brick.element = brickView;
                                    if (brick && brick.G != ''
                                        && (_wall && _wall[x][y] && (_wall[x][y].D != data.D))) {
                                        brickView.style.backgroundImage = 'url("' +
                                                _baseApiUrl +
                                                '/v1/image/t'
                                                + '/' + _inviteCode
                                                + '/' + brick.X
                                                + '/' + brick.Y
                                                + '/?d=' + brick.D
                                                + '")';
                                    }
                                    brickView.setAttribute('data-addressx', brick.X);
                                    brickView.setAttribute('data-addressY', brick.Y);
                                    brickView.setAttribute('data-viewX', x);
                                    brickView.setAttribute('data-viewY', y);
                                }
                            }
                            _wall = data;
                            document.body.style.minWidth = '1600px';
                            document.body.style.minHeight = '900px';
                            viewport = document.querySelector("meta[name=viewport]");
                            viewport.setAttribute('content', 'width=device-width, initial-scale=' + _viewportScale);
                            dimensions();

                            if (updatedBrickElement) {
                                updatedBrickElement.scrollIntoView();
                            }
                            setTimeout(updateWall, 10000);
                        }
                    };
                    xhr.send();
                } else {

                }


            }

            function openSession(x, y, addressX, addressY) {
                if (!addressX || !addressY)
                    return;

                var xhr = new XMLHttpRequest();
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
                var xhr = new XMLHttpRequest();
                //xhr.open('POST', _baseApiUrl + '/literally/draw/' + _brickInUse.sessionToken);
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
                                    '/' + _brickInUse.X +
                                    '/' + _brickInUse.Y +
                                    '/t?r=' +
                                    Math.floor((Math.random() * 10000) + 1) +
                                    '")';
                        }

                        _brickInUse = null;
                        _lc = null;
                        document.getElementById('drawSpace').style.display = 'none';
                        document.getElementById('tools-wrapper').style.display = 'none';
                        document.getElementById('wall').style.display = 'block';

                        // wait a second before updating to give Azure a chance to propagate the thumbnail image
                        setTimeout(function () {
                            updateWall(brickElement);
                        }, 5000);
                    } else {
                        document.getElementById('save-button').disabled = '';
                        document.getElementById('cancel-button').disabled = '';
                        document.getElementById('alert-message').innerHtml = 'Sorry! There was a problem saving the image... try again? :/';
                        document.getElementById('alert').style.display = 'block';
                    }
                };
                var imageSize = { width: 1600, height: 900 };
                var imageBounds = {
                    x: 0, y: 0, width: imageSize.width, height: imageSize.height
                };
                //xhr.send(JSON.stringify(_lc.getSnapshot()));
                //formData.append('SnapshotJson', escape(JSON.stringify(_lc.getSnapshot())));
                //formData.append('ImageData', _lc.getImage({ rect: imageBounds }).toDataURL());
                //xhr.send("snapshotJson="+escape(JSON.stringify(_lc.getSnapshot()))+"&imageData="+_lc.getImage({ rect: imageBounds }).toDataURL());
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

                var xhr = new XMLHttpRequest();
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

            function dimensions() {
                var viewportwidth;
                var viewportheight;

                // the more standards compliant browsers (mozilla/netscape/opera/IE7) use window.innerWidth and window.innerHeight

                if (typeof window.innerWidth != 'undefined') {
                    viewportwidth = window.innerWidth,
                    viewportheight = window.innerHeight
                }

                    // IE6 in standards compliant mode (i.e. with a valid doctype as the first line in the document)

                else if (typeof document.documentElement != 'undefined'
                    && typeof document.documentElement.clientWidth !=
                    'undefined' && document.documentElement.clientWidth != 0) {
                    viewportwidth = document.documentElement.clientWidth,
                    viewportheight = document.documentElement.clientHeight
                }

                    // older versions of IE

                else {
                    viewportwidth = document.getElementsByTagName('body')[0].clientWidth,
                    viewportheight = document.getElementsByTagName('body')[0].clientHeight
                }
                //alert('Viewport: ' + viewportwidth + 'x' + viewportheight);
                //alert('availWidth:'+window.screen.availWidth+'x'+window.screen.availHeight);
                var width = window.innerWidth
                    || document.documentElement.clientWidth
                    || document.body.clientWidth;

                var height = window.innerHeight
                    || document.documentElement.clientHeight
                    || document.body.clientHeight;
                //alert('body: ' + width + 'x' + height);

                if (document.getElementById('help')) {
                    document.getElementById('help').style.left = (window.screen.availWidth * 0.1) + 'px';
                    document.getElementById('help').style.top = (window.screen.availHeight * 0.1) + 'px';
                    document.getElementById('help').style.width = (window.screen.availWidth * 0.8) + 'px';
                    document.getElementById('help').style.height = (window.screen.availHeight * 0.8) + 'px';
                }
            }

        },
        
    }
})();