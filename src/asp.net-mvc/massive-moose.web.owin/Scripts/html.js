$(document).ready(function () {
    if (!_baseApiUrl) { _baseApiUrl  = 'http://local.api.massivemoose.com';}
    setTimeout(updateWall,10);
    var _brickInUse = null;
    var _lc = null;
    var _wall = null;
    var _toolsWaitHandle = 0;

    $('#tblWall tr td').click(function (bv) {
        openSession($(this).attr('data-viewx'),
            $(this).attr('data-viewy'),
            $(this).attr('data-addressx'),
            $(this).attr('data-addressy'));
    });

    $('#vp_increase')
        .click(function () {
            _viewportScaleWhenDrawing += 0.1;
            viewport = document.querySelector("meta[name=viewport]");
            viewport.setAttribute('content', 'width=device-width, initial-scale=' + _viewportScaleWhenDrawing);
            $('#vp-info').html('vp: ' + parseFloat(_viewportScaleWhenDrawing).toFixed(2));
        });
    $('#vp_decrease')
        .click(function () {
            _viewportScaleWhenDrawing -= 0.1;
            viewport = document.querySelector("meta[name=viewport]");
            viewport.setAttribute('content', 'width=device-width, initial-scale=' + _viewportScaleWhenDrawing);
            $('#vp-info').html('vp: ' + parseFloat(_viewportScaleWhenDrawing).toFixed(2));
        });
    $('#vp-info').html('vp: ' + parseFloat(_viewportScaleWhenDrawing).toFixed(2));

    function openCanvas(brick) {
        $('#wall').hide();
        $('#save-etc').show();
        $('#drawSpace').show();
        _lc = LC.init(document.getElementById('drawSpace'),
        {
            imageURLPrefix: "/content/img",
            imageSize: { width: 1600, height: 800 },
            //backgroundColor: "#ED7428",
            backgroundColor:"transparent",
            toolbarPosition: 'top'
        });

        var fullscreenButton = $('<button id="fullscreen-button">Fullscreen</button>');
        fullscreenButton.click(function() {
            if (screenfull.enabled) {
                screenfull.request();
            }
        })
        var uploadButton = $('<button id="save-button">Finish</button>');
        uploadButton.click(ClickUpload);
        var cancelButton = $('<button id="cancel-button">Cancel</button>');
        cancelButton.click(ClickCancel);

        $('.lc-options').append($('<div class="session-options"></div>').append(fullscreenButton).append(uploadButton).append(cancelButton));

        var unsubscribeOnDrawStart = _lc.on('drawStart', function (arguments) {
            if ($('.lc-picker').is(":visible")) {
                $('.lc-picker').toggle("slide", null, 100);
                $('.horz-toolbar').toggle("slide", { direction: 'up' }, 100);
            }
            // do stuff
        });
        var unsubscribeOnDrawStart = _lc.on('drawEnd', function (arguments) {
            if (_toolsWaitHandle != 0) {
                clearTimeout(_toolsWaitHandle);
            }
            _toolsWaitHandle = setTimeout(toggleTools, 2000);
            // do stuff
        });
        //unsubscribe();
        function toggleTools() {
            $('.lc-picker').toggle("slide");
            $('.horz-toolbar').toggle("slide", { direction: 'up' });
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
            $('body').css({ 'min-width': 0, 'min-height':0 });
            _lc.setZoom((window.innerWidth - 100) / adjustedWidth);
            //var dx = (1600 - window.innerWidth) / 2, dy = (800 - window.innerHeight) / 2;
            //_lc.setPan(dx, dy);
        }
        $('html, body').animate({
            scrollTop: 0, scrollLeft: 0
        }, 500);
        //_lc.setColor('background', "#ED7428");
    }

    function updateWall(scrollToId) {
        if (!_brickInUse) {
            $('#drawSpace').hide();
            $('#save-etc').hide();
            viewport = document.querySelector("meta[name=viewport]");
            viewport.setAttribute('content', 'width=device-width, initial-scale=' + _viewportScale);
            $.getJSON(_baseApiUrl + '/v2/wall/0/0',
                null,
                function(data) {
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
                                        '/v1/image/t/' +
                                        brick.X +
                                        '/' +
                                        brick.Y +
                                        '?d=' +
                                        brick.D +
                                        '")'
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

    function openSession(x, y,addressX,addressY) {
        $.post(_baseApiUrl+'/v1/image/begin/' + addressX + '/' + addressY)
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

        $('.finish-and-upload').hide();
        $('.cancel').hide();
        $('#messages').html('Uploading...');

        $.post(_baseApiUrl + '/literally/receive/' + _brickInUse.sessionToken,
                JSON.stringify(_lc.getSnapshot()))
            .done(function() {
                $('.finish-and-upload').show();
                $('.cancel').show();
                $('#messages').html('');

                var brickView = $(_brickInUse.element);
                if (brickView) {
                    brickView.css({
                        'backgroundImage': 'url("' +
                            _baseApiUrl +
                            '/v1/image/t/' +
                            _brickInUse.X +
                            '/' +
                            _brickInUse.Y +
                            '?r=' +
                            Math.floor((Math.random() * 10000) + 1) +
                            '")'
                    });
                }

                _brickInUse = null;
                _lc = null;
                $('#drawingSpace').hide();
                $('#wall').show();
                $('#messages').html('');
                updateWall(brickView[0].id);
            })
    }

    function ClickCancel(e) {
        e.preventDefault();

        $.post(_baseApiUrl + '/v1/image/release/' + _brickInUse.X + '/' + _brickInUse.Y + '/' + _brickInUse.sessionToken, null)
        .done(function() {
            _brickInUse = null;
            _lc = null;
            $('#drawingSpace').hide();
            $('#wall').show();
            updateWall();
        });

        return false;
    }
});
