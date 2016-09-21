var Wall = (function () {
    return {
        cfg: {
            baseApiUrl: '',
            drawZoom: 0.8,
            viewportScale: 1.0,
            viewportScaleWhenDrawing: 0.7,
            inviteCode: '',
            refreshTime: 3000
        },
        initialize: function (containerEl, configuration) {
            this.cfg = configuration;
            this.wallETag = '0';
            this.xhrWaitHandle = null;
            this.xhr = new XMLHttpRequest();
            this.updateXhr = new XMLHttpRequest();
            this.xhr.onreadystatechange = function (evt) {

            }
            this.containerEl = containerEl;

            this.onShowProgress = this.cfg.onShowProgress;
            this.onCloseProgress = this.cfg.onCloseProgress;

            window.addEventListener("resize", this.updateHelpDialogDimensions);
            this._viewportScaleWhenDrawing = this.cfg.viewPortScaleWhenDrawing;
            this._viewportScale = this.cfg.viewPortScale;
            this._drawZoom = this.cfg.drawZoom;
            this._baseApiUrl = this.cfg.baseApiUrl;
            this._inviteCode = this.cfg.inviteCode;
            this._refreshTime = this.cfg.refreshTime;

            this._brickInUse = null;
            this._lc = null;
            this._wall = null;
            this._toolsWaitHandle = 0;

            this.updateDialogs();
            this.bindHelp();
            this.bindBricks();

            setTimeout(this.checkWallStaleness, 10, this);

            var wall = this;
            window.onscroll = wall.updateDialogs;
        },
        bindHelp: function () {
            var $this = this;
            if (document.getElementById('help')) {
                document.getElementById('moreHelp1')
                    .onclick = function () {
                        $this.xhr.open('GET', '/Home/Help');
                        $this.xhr.setRequestHeader('Content-Type', 'text/html');
                        $this.xhr.onload = function () {
                            document.getElementById('help-scroller').style.overflowY = 'scroll';
                            document.getElementById('help-full').innerHTML = $this.xhr.responseText;
                            document.getElementById('help-full-container').style.display = 'block';
                            document.getElementById('help-question').style.display = 'none';
                        }
                        $this.xhr.send();
                    };
                document.getElementById('noHelpThanks1').onclick = function () {
                    $this.setDontHelpMe();
                };
                document.getElementById('help-close').onclick = function () {
                    $this.setDontHelpMe();
                };
                document.getElementById('moreHelp2')
                    .onclick = function () {
                        $this.xhr.open('GET', '/Home/Help');
                        $this.xhr.setRequestHeader('Content-Type', 'text/html');
                        $this.xhr.onload = function () {
                            document.getElementById('help-scroller').style.overflowY = 'scroll';
                            document.getElementById('help-full').innerHTML = $this.xhr.responseText;
                            document.getElementById('help-full-container').style.display = 'block';
                            document.getElementById('help-question').style.display = 'none';
                        }
                        $this.xhr.send();
                    };
                document.getElementById('noHelpThanks2').onclick = function () {
                    $this.setDontHelpMe();
                    document.getElementById('help').style.display = 'none';
                };
                document.getElementById('noHelpThanks3').onclick = function () {
                    $this.setDontHelpMe();
                    document.getElementById('help').style.display = 'none';
                };
                document.getElementById('help-close').onclick = function () {
                    $this.setDontHelpMe();
                    document.getElementById('help').style.display = 'none';
                };
            }

        },
        setDontHelpMe: function () {
            this.xhr.open('POST', '/Home/DontHelpMe');
            this.xhr.send();
            document.getElementById('help').style.display = 'none';
        },
        bindBricks: function () {
            var $wall = this;
            var bricks = document.getElementsByClassName('brick');
            for (var ei = 0; ei < bricks.length; ei++) {
                var b = document.getElementById(bricks[ei].id);
                b.onclick = function (e) {
                    $wall.openSession(e.currentTarget.attributes.getNamedItem('data-viewx').value,
                        e.currentTarget.attributes.getNamedItem('data-viewy').value,
                        e.currentTarget.attributes.getNamedItem('data-addressx').value,
                        e.currentTarget.attributes.getNamedItem('data-addressy').value);
                };

            }
        },
        updateDialogs: function () {
            viewport = document.querySelector("meta[name=viewport]");
            var help = document.getElementById('help');
            var diag = document.getElementById('diagnostics');
            var progress_container = document.getElementById('working_container');
            var progress = document.getElementById('working_panel');
            if (help) {

                var ratio = 0.8;
                var paddingX = Math.round(window.innerWidth * (1 - ratio) / 2);
                var paddingY = Math.round(window.innerHeight * (1 - ratio) / 2);
                var width = Math.round(window.innerWidth * ratio);
                var height = Math.round(window.innerHeight * ratio);
                help.style.width = width + 'px';
                help.style.height = height + 'px';
                help.style.left = (window.pageXOffset + paddingX) + 'px';
                help.style.top = (window.pageYOffset + paddingY) + 'px';

                if (width < height) {
                    document.getElementById('rotate-alert').style.display = 'inline-block';
                } else {
                    document.getElementById('rotate-alert').style.display = 'none';
                }
            }

            if (progress_container.style.display != 'none') {
                var r = progress.getClientRects();
                progress.style.left = ((window.scrollX) + ((window.innerWidth - r[0].width) / 2)) + 'px';
                progress.style.top = ((window.scrollY * 0.7) + ((window.innerHeight - r[0].height) / 2)) + 'px';
            }
        },
        updateProgress: function (evt) {

        },
        transferComplete: function (evt) {

        },
        transferFailed: function (evt) {

        },
        transferCanceled: function (evt) {

        },
        transferStarted: function (evt) {

        },
        setViewScale: function () {
            var vs = this.cfg.viewPortScale;
            viewport = document.querySelector("meta[name=viewport]");
            viewport.setAttribute('content', 'width=device-width, initial-scale=1');

        },
        openSession: function (x, y, addressX, addressY) {
            if (!addressX || !addressY)
                return;
            $this = this;

            if (this.onShowProgress) {
                this.onShowProgress('Loading...');
                this.updateDialogs();
            }

            this.xhr.open('POST', this._baseApiUrl + '/v1/' + this._inviteCode + '/draw/' + addressX + '/' + addressY);
            this.xhr.setRequestHeader('Content-Type', 'application/json');
            this.xhr.onload = function () {
                if (this.status === 200) {
                    var data = JSON.parse(this.responseText);
                    $this._brickInUse = $this._wall[x][y];
                    $this._brickInUse.sessionToken = data.sessionToken;
                    $this._brickInUse.snapshotJson = data.snapshotJson;
                    $this.openCanvas($this._brickInUse);
                } else if (this.status == 409) {
                    alert('someone is currently drawing on that space')
                    $this.updateWall();
                } else {
                    alert('something went terribly, terribly wrong. call your lawyers.');
                }
                if (this.onCloseProgress) {
                    this.onCloseProgress();
                }
            };
            this.xhr.send();
        },
        openCanvas: function (brick) {
            if (this.cfg.startDrawing) {
                this.cfg.startDrawing({ data: brick, wallInstance: this });
            }

        },
        checkWallStaleness: function (wall) {
            var $this = wall;

            if ($this.updateXhr.readyState == 0 || $this.updateXhr.readyState == 4) {
                $this.updateXhr.open('HEAD', $this._baseApiUrl + '/v2/wall/' + $this._inviteCode + '/0/0/' + $this.wallETag);
                $this.updateXhr.setRequestHeader('If-None-Match', $this.wallETag);
                $this.updateXhr.onload = function () {
                    if ($this.updateXhr.status == 200) {
                        $this.wallETag = $this.updateXhr.getResponseHeader('ETag');
                        if ($this.wallETag) {
                            $this.wallETag = $this.wallETag.replace("\"", "").replace("\"", "")
                        }
                        $this.updateWall();
                    } else {
                        setTimeout($this.checkWallStaleness, $this._refreshTime, $this);
                    }
                }
                $this.updateXhr.send();
            } else {
                setTimeout($this.checkWallStaleness, $this._refreshTime, $this);
            }
        },
        updateWall: function (updatedBrickElement) {
            if (!this._brickInUse) {
                $this = this;
                this.setViewScale();
                this.updateXhr.open('GET', this._baseApiUrl + '/v2/wall/' + this._inviteCode + '/0/0');
                this.updateXhr.setRequestHeader('Content-Type', 'application/json');
                this.updateXhr.onload = function () {
                    if (this.status === 200) {
                        var data = JSON.parse(this.responseText);
                        $this.containerEl.style.display = 'block';
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

                                var brickImageUrl = $this._baseApiUrl +
                                    '/v1/image/t' +
                                    '/' +
                                    $this._inviteCode +
                                    '/' +
                                    brick.x +
                                    '/' +
                                    brick.y +
                                    '?d=' +
                                    brick.d;

                                if (brick.c == 1 && ($this._wall && $this._wall[x][y])) {
                                    // TODO: only update source image if the brick.D value is different to the data-updated attribute on the element
                                    if (brick.d != brickView.getAttribute('data-updated')) {
                                        brickView.style.backgroundImage = 'url("' + brickImageUrl + '")';
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
                        $this._wall = data;
                        //document.body.style.minWidth = '1600px';
                        //document.body.style.minHeight = '900px';
                        $this.setViewScale();
                        if ($this.updatedBrickElement) {
                            var r = $this.updatedBrickElement.getClientRects;

                            document.scrollTo(r[0].left, r[0].top);
                        }
                        setTimeout($this.checkWallStaleness, $this._refreshTime, $this);
                    }
                };
                this.updateXhr.send();
            } else {

            }


        },
        updateBrickBackground: function (brickElement) {
            if (brickElement) {
                brickElement.style.backgroundImage = 'url("' +
                    this._baseApiUrl +
                    '/v1/image/t' +
                    '/' +
                    this._inviteCode +
                    '/' +
                    this._brickInUse.x +
                    '/' +
                    this._brickInUse.y +
                    '?r=' +
                    Math.floor((Math.random() * 10000) + 1) +
                    '")';
                var r = brickElement.getClientRects();
                window.scrollTo(r[0].left, r[0].top);
                this.setViewScale();
            }
        },
        endSession: function (sessionData, imageData, json) {
            if (this.onCloseProgress) {
                this.onCloseProgress();
            }
            var $this = this;
            var xhr = this.xhr;
            xhr.open('POST', sessionData.wallInstance._baseApiUrl + '/v1/save/' + sessionData.data.sessionToken, true);
            xhr.setRequestHeader('Content-Type', 'application/json');
            xhr.onload = function () {
                if (xhr.status == 200) {
                    $this.updateBrickBackground($this._brickInUse.element);

                    var brick = $this._brickInUse;
                    $this._brickInUse = null;
                    $this.containerEl.style.display = 'block';

                    // wait a second before updating to give Azure a chance to propagate the thumbnail image
                    setTimeout(function () {
                        $this.updateWall(brick.element);
                    },
                        5000);
                }
            };
            xhr.send('{"snapshotJson":"' + escape(json) + '","imageData":"' + imageData + '"}');
        },
        cancelSession: function () {
            if (this.onCloseProgress) {
                this.onCloseProgress();
            }
            var $this = this;
            var xhr = this.xhr;
            xhr.open('POST', this._baseApiUrl + '/v1/release/' + this._brickInUse.sessionToken);
            xhr.setRequestHeader('Content-Type', 'application/json');
            xhr.send();
            xhr.onload = function () {
                if (this.status === 200) {
                    $this._brickInUse = null;
                    document.getElementById('drawSpace').style.display = 'none';
                    $this.updateWall();
                } else {
                    alert('there was a problem saving.. err... sorry... try again? :/');
                }
            }

        }
    }
})();