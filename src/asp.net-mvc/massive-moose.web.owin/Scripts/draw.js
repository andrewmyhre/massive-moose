function distanceBetween(point1, point2) {
    return Math.sqrt(Math.pow(point2.x - point1.x, 2) + Math.pow(point2.y - point1.y, 2));
}
function angleBetween(point1, point2) {
    return Math.atan2(point2.x - point1.x, point2.y - point1.y);
}

var MassiveMoose = (function () {
    return {
        tools: [{
            onPointerDrag: function (moose, pt) {
                var dist = distanceBetween(moose.lastPoint, pt);
                var angle = angleBetween(moose.lastPoint, pt);

                var fc = moose.foreColor;

                for (var i = 0; i < dist; i += moose.lineWidth / 4) {

                    x = moose.lastPoint.x + (Math.sin(angle) * i);
                    y = moose.lastPoint.y + (Math.cos(angle) * i);

                    var radgrad = moose.ctx.createRadialGradient(x, y, moose.lineWidth / 2, x, y, moose.lineWidth);

                    var centerColor = 'rgba(' + fc.r + ',' + fc.g + ',' + fc.b + ',1)';
                    var midColor = 'rgba(' + fc.r + ',' + fc.g + ',' + fc.b + ',0.5)';
                    var edgeColor = 'rgba(' + fc.r + ',' + fc.g + ',' + fc.b + ',0)';

                    radgrad.addColorStop(0, centerColor);
                    radgrad.addColorStop(0.5, midColor);
                    radgrad.addColorStop(1, edgeColor);

                    moose.ctx.fillStyle = radgrad;
                    moose.ctx.fillRect(x - moose.lineWidth, y - moose.lineWidth, moose.lineWidth * 2, moose.lineWidth * 2);
                }
                moose.lastPoint = pt;
            }
        }],
        debug: function (log) {
            this.debugElement.innerHTML = log;
        },
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
            this.opts = opts || {};
            this.isDrawing = false;
            this.lastPoint = null;
            this.mouseOut = true;

            this.canvas = document.createElement('canvas');
            this.canvas.style['background-color'] = 'white';
            this.canvas.moose = this;
            this.ctx = this.canvas.getContext('2d');
            this.ctx.lineJoin = this.ctx.lineCap = 'round';

            this.debugElement = document.createElement('div');

            this.canvas.width = this.width = 300;
            this.canvas.height = this.height = 160;

            this.lineWidth = 10;
            this.foreColor = { r: 0, g: 0, b: 180 };

            if (containerEl) {
                this.bindToElement(containerEl);
            }


            this.canvas.onmousedown = function (e) {
                var moose = this.moose;
                moose.isDrawing = true;
                moose.lastPoint = { x: e.clientX, y: e.clientY };
            }
            this.canvas.onmousemove = function (e) {
                var moose = this.moose;
                if (moose.mouseOut) {
                    moose.lastPoint = { x: e.clientX, y: e.clientY };
                    moose.mouseOut = false;
                }
                if (!moose.isDrawing) return;
                var currentPoint = { x: e.clientX, y: e.clientY };
                moose.tools[0].onPointerDrag(moose, currentPoint);
            }

            this.canvas.onmouseup = function () {
                var moose = this.moose;
                moose.isDrawing = false;
            };
            this.canvas.onmouseout = function () {
                var moose = this.moose;
                moose.mouseOut = true;
            };
            this.canvas.addEventListener('touchmove', function (e) {
                e.preventDefault();

                var moose = this.moose;
                var touches = e.changedTouches;
                if (touches.length === 1) {
                    var currentPoint = { x: touches[0].pageX, y: touches[0].pageY };
                    moose.debug('touch move');
                    //alert('touch move at ' + touches[0].pageX + ',' + touches[0].pageY);
                    moose.tools[0].onPointerDrag(moose, currentPoint);
                }
            });
            this.canvas.addEventListener('touchend', function (e) {
                e.preventDefault();
                var moose = this.moose;
                moose.isDrawing = false;
            });
            this.canvas.addEventListener('touchstart', function (e) {
                if (e.target.tagName.toLowerCase() !== 'canvas') {
                    return;
                }

                var moose = this.moose;
                e.preventDefault();
                var touches = e.changedTouches;
                if (e.touches.length === 1) {
                    moose.isDrawing = true;
                    moose.lastPoint = { x: e.touches[0].pageX, y: e.touches[0].pageY };
                    document.addEventListener('touchmove', touchMoveListener);
                    document.addEventListener('touchend', touchEndListener);
                    return document.addEventListener('touchcancel', touchEndListener);
                }
            });
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
            this.containerEl.parentElement.appendChild(this.debugElement);
            this.isBound = true;
        }
    }
});