
var SendBuffer = (function() {
    return {
        initialize: function (opts) {
            this.buffer = [];
            this.sending = [];
            this.failed = [];
            this.sent = [];
            this.destinationUrl = opts.destinationUrl;
            this.listeners = [];
            this.sleepTime = 300;
            this.uploadTimer = setTimeout(this.tryUpload, this.sleepTime, this);
            this.inviteCode = opts.inviteCode;
        },
        tryUpload: function (sendBuffer) {
            while (sendBuffer.buffer.length > 0) {
                var data = sendBuffer.buffer.pop();
                console.log('sending ' + data);
                sendBuffer.sending.push(data);
                var url = sendBuffer.destinationUrl + '/v1/';
                if (sendBuffer.inviteCode) {
                    url += inviteCode + '/';
                }
                url += 'save/' + data.sessionData.data.sessionToken;
                try {
                    if (!data.xhr) {
                        data.xhr = new XMLHttpRequest();
                    }
                    data.xhr.open('POST', url);
                    data.xhr.setRequestHeader('Content-Type', 'application/json');
                    data.xhr.data = data;
                    data.xhr.onload = function () {
                        if (this.status === 200) {
                            var i = sendBuffer.sending.indexOf(this.data);
                            if (i > -1) {
                                sendBuffer.sending.splice(i, 1);
                            }
                            sendBuffer.sent.push(this.data);
                            sendBuffer.fireEvent('sendsucceeded', this.data);
                        } else {
                            var i = sendBuffer.sending.indexOf(this.data);
                            if (i > -1) {
                                sendBuffer.sending.splice(i, 1);
                            }
                            sendBuffer.failed.push(this.data);
                            sendBuffer.fireEvent('sendfailed', this.data);
                        }
                    };
                    data.xhr.send('{"snapshotJson":"' + escape(data.jsonData) + '","imageData":"' + data.imageData + '"}');

                } catch (ex) {
                    console.log(ex.message);
                    sendBuffer.sending.remove(data);
                    sendBuffer.failed.push(data);
                }
            }

            sendBuffer.uploadTimer = setTimeout(sendBuffer.tryUpload, sendBuffer.sleepTime, sendBuffer);
        },
        push:function(o) {
            this.buffer.push(o);

            this.fireEvent('additem', o);
        },
        fireEvent:function(type,data,context) {
            var listeners, handlers, i, n, handler, scope;

            if (!(listeners = this.listeners)) {
                return;
            }
            if (!(handlers = listeners[type])) {
                return;
            }
            for (i = 0, n = handlers.length; i < n; i++) {
                handler = handlers[i];
                if (typeof (context) !== "undefined" && context !== handler.context) continue;
                if (handler.method.call(
                    handler.scope, this, type, data
                ) === false) {
                    return false;
                }
            }
            return true;
        },
        addListener: function (type,method, scope, context) {
            var listeners, handlers;
            var scope;

            if (!(listeners = this.listeners)) {
                listeners = this.listeners = {};
            }
            if (!(handlers = listeners[type])) {
                handlers = listeners[type] = [];
            }
            scope = (scope ? scope : window);
            handlers.push({
                method: method,
                scope: scope,
                context: (context ? context : scope)
            });
        }
    }
});