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
        initialize: function(containerEl, configuration) {
            this.cfg = configuration;
            this.wallETag = '0';
            this.sendBuffer = configuration.sendBuffer;
            this._baseApiUrl = configuration.baseApiUrl;
            this._inviteCode = configuration.inviteCode;
            this._refreshTime = this.cfg.refreshTime;
            this.sendBuffer.addListener('sendsucceeded', this.onSendSucceeded, this);
            this.sendBuffer.addListener('sendfailed', this.onSendFailed, this);
            this.sendBuffer.addListener('additem', this.onBrickUpdated, this);
            this.xhr = new XMLHttpRequest();
            this.updateXhr = new XMLHttpRequest();

            this.onSessionClosed = this.cfg.onSessionClosed;

            this.brickWidth = 1600 / 12;
            this.brickHeight = 900 / 12;

            this.bindToElement(containerEl);
            this.updateWall();
        },
        bindToElement: function(containerEl) {
            this.containerEl = containerEl;

            var layers = [];

            this.dv_background = document.createElement('div');
            this.dv_background.style['background'] = 'url("/Content/backgrounds/white-brick.jpg")';
            this.dv_background.style['background-size'] ='50% 50%';
            this.dv_background.style['background-repeat'] = 'repeat';
            layers.push(this.dv_background);

            this.cv_tiles = document.createElement('canvas');
            this.cv_tiles_ctx = this.cv_tiles.getContext('2d');
            layers.push(this.cv_tiles);

            this.cv_ui = document.createElement('canvas');
            this.cv_ui_ctx = this.cv_tiles.getContext('2d');
            layers.push(this.cv_ui);

            for (var i = 0; i < layers.length; i++) {
                var cv = layers[i];
                cv.wall = this;
                cv.width = 1600;
                cv.height = 900;
                cv.style.position = 'absolute';
                cv.style.top = '0';
                cv.style.left = '0';
                cv.style.width = '1600px';
                cv.style.height = '900px';
                cv.style['z-index'] = (i + 1);

                this.containerEl.appendChild(cv);
            }

            this.cv_ui.addEventListener('mouseover', this.ui_mouseOver);
            this.cv_ui.addEventListener('mousemove', this.ui_mouseOver);
            this.cv_ui.addEventListener('click', this.ui_click);

            this.dialogContainer = document.createElement('div');
            this.dialogContainer.style.position = 'absolute';
            this.dialogContainer.style.top = '0px';
            this.dialogContainer.style.left = '0px';
            this.dialogContainer.style.width = '100%';
            this.dialogContainer.style.height = '100%';
            this.dialogContainer.style.background = 'transparent';
            this.dialogContainer.style['z-index'] = '100';
            this.dialogContainer.style.display = 'none';

            this.dialog = document.createElement('div');
            this.dialog.style.position = 'absolute';
            this.dialog.style.border = '2px solid blue';
            this.dialog.style['corner-radius'] = '3px';
            this.dialog.style['text-align'] = 'center';
            this.dialog.style['z-index'] = '101';
            this.dialog.style['font-size'] = '2em';
            this.dialog.style.padding = '1em';
            this.dialog.style['background-color'] = "#fff";
            this.dialogContainer.appendChild(this.dialog);
            this.containerEl.appendChild(this.dialogContainer);
        },
        ui_mouseOver: function(e) {
            var x = Math.floor(e.offsetX / e.target.wall.brickWidth);
            var y = Math.floor(e.offsetY / e.target.wall.brickHeight);
            e.target.wall.selectedBrick = {
                addressX: x - 6,
                addressY: y - 6,
                viewX: x,
                viewY: y
            };
            var ctx = e.target.getContext('2d');
            ctx.clearRect(0, 0, e.target.width, e.target.height);
            ctx.beginPath();
            ctx.rect(x * e.target.wall.brickWidth, y * e.target.wall.brickHeight, e.target.wall.brickWidth, e.target.wall.brickHeight);
            ctx.stroke();
        },
        ui_click: function (e) {
            e.target.wall.openSession(e.target.wall.selectedBrick.viewX,
                e.target.wall.selectedBrick.viewY,
                e.target.wall.selectedBrick.addressX,
                e.target.wall.selectedBrick.addressY);
        },
            onBrickUpdated: function(buffer, type, data) {
                var brickImage = new Image();
                var wall = this;
                var brickx = data.sessionData.data.viewX, bricky = data.sessionData.data.viewY;
                brickImage.onload = function() {
                    wall.cv_tiles_ctx.drawImage(this,
                        brickx * wall.brickWidth,
                        bricky * wall.brickHeight,
                        wall.brickWidth,
                        wall.brickHeight);
                };
                brickImage.src = data.imageData;
            },
        onSendSucceeded: function(buffer, type, data) {
        },
        onSendFailed: function(buffer, type, data) {
        },
        checkWallStaleness: function (wall) {
            var $this = wall;

            if ($this.updateXhr.readyState == 0 || $this.updateXhr.readyState == 4) {
                $this.updateXhr.open('HEAD', $this._baseApiUrl + '/v2/wall/' + $this._inviteCode + '/img/' + $this.wallETag);
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
        updateWall: function(updatedBrickElement) {
            var wall = this;
            var img = new Image();
            img.src = this._baseApiUrl + '/v2/wall/' + this._inviteCode + '/img?'+Date.now();
            img.onload = function() {
                wall.cv_tiles_ctx.clearRect(0, 0, wall.cv_tiles.width, wall.cv_tiles.height);
                wall.cv_tiles_ctx.drawImage(this, 0, 0, wall.cv_tiles.width, wall.cv_tiles.height);
                setTimeout(wall.checkWallStaleness, 10, wall);
            };
        },
            showProgress:function(message) {
                this.dialog.innerHTML = '<div>' + message + '</div><div><img src="/content/progress.gif" /></div>';
                this.dialogContainer.style.display = 'block';
                var r = this.dialog.getClientRects();
                this.dialog.style.top = ((window.innerHeight / 2)-(r[0].height / 2)) + 'px';
                this.dialog.style.left = ((window.innerWidth / 2)-(r[0].width / 2)) + 'px';
            },
            hideProgress:function() {
                this.dialogContainer.style.display = 'none';
            },
        openSession: function (x, y, addressX, addressY) {
            $this = this;

            this.showProgress('Loading...');
            this.xhr.open('POST', this._baseApiUrl + '/v1/' + this._inviteCode + '/draw/' + addressX + '/' + addressY);
            this.xhr.setRequestHeader('Content-Type', 'application/json');
            this.xhr.onload = function () {
                if (this.status === 200) {
                    var data = JSON.parse(this.responseText);
                    $this._brickInUse =
                    {
                        addressX: $this.selectedBrick.addressX,
                        addressY: $this.selectedBrick.addressY,
                        viewX: $this.selectedBrick.viewX,
                        viewY: $this.selectedBrick.viewY,
                        sessionToken: data.sessionToken,
                        snapshotJson:data.snapshotJson
                    };
                    $this.hideProgress();
                    $this.openCanvas($this._brickInUse);
                } else if (this.status == 409) {
                    alert('someone is currently drawing on that space');
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
        cancelSession: function () {
            if (this.onCloseProgress) {
                this.onCloseProgress();
            }
            if (!this._brickInUse) {
                return;
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
                    if ($this.onSessionClosed) {
                        $this.onSessionClosed();
                    }
                } else {
                    alert('there was a problem saving.. err... sorry... try again? :/');
                }
            }

        }
    }
})();