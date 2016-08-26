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

            _brickInUse = null;
            _lc = null;
            _wall = null;
            _toolsWaitHandle = 0;

            setTimeout(updateWall,10);

            var bricks = document.getElementsByClassName('brick');
            for (var ei = 0; ei < bricks.length; ei++) {
                var b = document.getElementById(bricks[ei].id);
                b.onclick = function(e) {
                    //console.log(e);
                    //console.log(b.getAttribute('data-addressx') + ',' + b.getAttribute('data-addressy'));
                    openSession(e.currentTarget.attributes.getNamedItem('data-viewx').value,
                        e.currentTarget.attributes.getNamedItem('data-viewy').value,
                        e.currentTarget.attributes.getNamedItem('data-addressx').value,
                        e.currentTarget.attributes.getNamedItem('data-addressy').value);
                };
                
            }
//            $('#tblWall tr td').click(function (bv) {
//                openSession($(this).attr('data-viewx'),
//                    $(this).attr('data-viewy'),
//                    $(this).attr('data-addressx'),
//                    $(this).attr('data-addressy'));
//            });

            function openCanvas(brick) {
                document.getElementById('wall').style.display = 'none';
                document.getElementById('drawSpace').style.display = 'block';
                document.getElementById('tools-wrapper').style.display = 'block';
                document.getElementById('save-button').disabled = '';
                document.getElementById('cancel-button').disabled = '';

                // configure literally canvas
                _lc = LC.init(document.getElementById('drawSpace'),
                {
                    imageURLPrefix: "/content/img",
                    imageSize: { width: 1600, height: 800 },
                    //backgroundColor: "#ED7428",
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
                      el: document.getElementById('tool-eraser'),
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
                    _toolsWaitHandle = setTimeout(showTools, 2000);
                    // do stuff
                });
                //unsubscribe();
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
                    //$('body').css({ 'min-width': 0, 'min-height': 0 });
                    _lc.setZoom((window.innerWidth - 100) / adjustedWidth);
                }
            }

            function updateWall(scrollToId) {
                if (!_brickInUse) {
                    viewport = document.querySelector("meta[name=viewport]");
                    viewport.setAttribute('content', 'width=device-width, initial-scale=' + _viewportScale);
                    $.getJSON(_baseApiUrl + '/v2/wall/' + _inviteCode + '/0/0',
                        null,
                        function (data) {
                            document.getElementById('wall').style.display = 'block';
                            document.getElementById('drawSpace').style.display = 'none';
                            for (var y = 0; y < data.length; y++) {

                                for (var x = 0; x < data[y].length; x++) {
                                    var brickView = $('#c' + (y * 12 + x));
                                    var brick = data[x][y];
                                    brick.element = brickView;
                                    if (brick && brick.G != ''
                                        && (_wall && _wall[x][y] && (_wall[x][y].D != data.D))) {
                                        brickView.css({
                                            'backgroundImage': 'url("' +
                                                _baseApiUrl +
                                                '/v1/image/t'
                                                + '/' + _inviteCode
                                                + '/' + brick.X
                                                + '/' + brick.Y
                                                + '/?d=' + brick.D
                                                + '")'
                                        });
                                    }
                                    $(brickView).attr('data-addressX', brick.X);
                                    $(brickView).attr('data-addressY', brick.Y);
                                    $(brickView).attr('data-viewX', x);
                                    $(brickView).attr('data-viewY', y);
                                }
                            }
                            _wall = data;
                            $('body').css({ 'min-width': '1600px', 'min-height': '900px' });
                            viewport = document.querySelector("meta[name=viewport]");
                            viewport.setAttribute('content', 'width=device-width, initial-scale=' + _viewportScale);

                            if (scrollToId) {
                                document.getElementById(scrollToId).scrollIntoView();
                            }
                            setTimeout(updateWall, 10000);
                        });
                } else {

                }


            }

            function openSession(x, y, addressX, addressY) {
                if (!addressX || !addressY)
                    return;

                $.post(_baseApiUrl + '/v1/' + _inviteCode + '/draw/' + addressX + '/' + addressY)
                    .done(function (data) {
                        _brickInUse = _wall[x][y];
                        _brickInUse.sessionToken = data.sessionToken;
                        _brickInUse.snapshotJson = data.snapshotJson;
                        openCanvas(_brickInUse);
                    })
                .fail(function () {
                    alert('someone is currently drawing on that space')
                    updateWall();
                });
            }

            function ClickUpload(e) {
                e.preventDefault();
                document.getElementById('save-button').disabled = 'disabled';
                document.getElementById('cancel-button').disabled = 'disabled';

                $.post(_baseApiUrl + '/literally/draw/' + _brickInUse.sessionToken,
                        JSON.stringify(_lc.getSnapshot()))
                    .done(function () {

                        var brickView = $(_brickInUse.element);
                        if (brickView) {
                            brickView.css({
                                'backgroundImage': 'url("' +
                                    _baseApiUrl +
                                    '/v1/image/t' +
                                    '/'+_inviteCode +
                                    '/' + _brickInUse.X +
                                    '/' + _brickInUse.Y +
                                    '/t?r=' +
                                    Math.floor((Math.random() * 10000) + 1) +
                                    '")'
                            });
                        }

                        _brickInUse = null;
                        _lc = null;
                        document.getElementById('drawSpace').style.display = 'none';
                        document.getElementById('tools-wrapper').style.display = 'none';
                        document.getElementById('wall').style.display = 'block';

                        // wait a second before updating to give Azure a chance to propagate the thumbnail image
                        setTimeout(function () {
                                updateWall(brickView[0].id);
                            },
                            5000);
                    })
            }

            function ClickCancel(e) {
                e.preventDefault();
                document.getElementById('save-button').disabled = 'disabled';
                document.getElementById('cancel-button').disabled = 'disabled';

                $.post(_baseApiUrl + '/v1/release/' + _brickInUse.sessionToken, null)
                .done(function () {
                    _brickInUse = null;
                    _lc = null;
                    document.getElementById('drawSpace').style.display = 'none';
                    document.getElementById('tools-wrapper').style.display = 'none';
                    document.getElementById('wall').style.display = 'block';
                    updateWall();
                });

                return false;
            }

        },
        
    }
})();