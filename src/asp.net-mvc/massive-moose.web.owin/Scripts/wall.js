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
            this.sendBuffer.addListener('sendsucceeded', this.onSendSucceeded, this);
            this.sendBuffer.addListener('sendfailed', this.onSendFailed, this);

            this.bindToElement(containerEl);
            this.updateWall();
        },
        bindToElement: function(containerEl) {
            this.containerEl = containerEl;

            var layers = [];

            this.dv_background = document.createElement('div');
            this.dv_background.style['background'] = 'url("/Content/backgrounds/white-brick.jpg")';
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
        },
        ui_mouseOver: function(e) {
            var tw = 1600 / 12;
            var th = 900 / 12;
            var x = Math.floor(e.clientX / tw);
            var y = Math.floor(e.clientY / th);
            e.target.wall.selectedAddress = {
                x: x - 6,
                y: y - 6
            };
            var ctx = e.target.getContext('2d');
            ctx.clearRect(0, 0, e.target.width, e.target.height);
            ctx.beginPath();
            ctx.rect(x * tw, y * th, tw, th);
            ctx.stroke();
        },
        ui_click: function(e) {
            alert(e.target.wall.selectedAddress.x + ',' + e.target.wall.selectedAddress.y);
        },
        onSendSucceeded: function(buffer, type, data) {
        },
        onSendFailed: function(buffer, type, data) {
        },
        updateWall: function(updatedBrickElement) {
            var wall = this;
            var img = new Image();
            img.src = this._baseApiUrl + '/v2/wall/' + this._inviteCode + '/img';
            img.onload = function() {
                wall.cv_tiles_ctx.clearRect(0, 0, wall.cv_tiles.width, wall.cv_tiles.height);
                wall.cv_tiles_ctx.drawImage(this, 0, 0, wall.cv_tiles.width, wall.cv_tiles.height);
            };
        },
    }
})();