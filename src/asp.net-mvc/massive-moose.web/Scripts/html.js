﻿$(document).ready(function () {
    if (!_baseApiUrl) { _baseApiUrl  = 'http://local.api.massivemoose.com';}
    setTimeout(updateWall,10);
    var _brickInUse = null;
    var _lc = null;
    var _wall = null;

    $('[data-action=upload]').click(function (e) {
        e.preventDefault();

        $('.finish-and-upload').hide();
        $('.cancel').hide();
        $('#messages').html('Uploading...');

        $.post(_baseApiUrl+'/literally/receive/' + _brickInUse.sessionToken,
                JSON.stringify(_lc.getSnapshot()))
            .done(function() {
                $('.finish-and-upload').show();
                $('.cancel').show();
                $('#messages').html('');

                var brickView = $(_brickInUse.element);
                if (brickView) {
                    brickView.css({ 'backgroundImage': 'url("' + _baseApiUrl + '/v1/image/t/' + _brickInUse.AddressX + '/' + _brickInUse.AddressY + '?r=' + Math.floor((Math.random() * 10000) + 1) + '")' });
                }

                _brickInUse = null;
                _lc = null;
                $('#drawingSpace').hide();
                $('#wall').show();
                $('#messages').html('');
                updateWall();
        });
        return false;
    });

    $('[data-action=cancel]')
        .click(function (e) {
            e.preventDefault();

            _brickInUse = null;
            _lc = null;
            $('#drawingSpace').hide();
            $('#wall').show();
            updateWall();

            return false;
        });

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
        
        if (brick.snapshotJson) {
            _lc.loadSnapshot(JSON.parse(brick.snapshotJson));
        }

        if (window.innerWidth < 1600) {
            _lc.setZoom((window.innerWidth - 100) / 1600);
            var dx = (1600 - window.innerWidth) / 2, dy = (800 - window.innerHeight) / 2;
            _lc.setPan(dx, dy);
        }
        //_lc.setColor('background', "#ED7428");
    }

    function updateWall() {
        $('#drawSpace').hide();
        $('#save-etc').hide();
        $.getJSON(_baseApiUrl+'/v2/wall/0/0',null,
            function (data) {
                _wall = data;
                $('#wall').html('');
                var tbl = $('<table id="tblWall"></table>');
                for (var y = 0; y < data.length; y++) {
                    var row = $('<tr class="' + (y % 2 == 1 ? 'row_offset' : 'row') + '"></tr>');
                    for (var x = 0; x < data[y].length; x++) {
                        var brickView = $('<td id="' + x.toString() + y.toString() + '" class="brick"></td>');
                        var brick = data[x][y];
                        brick.element = brickView;
                        if (brick && brick.Id != 0) {
                            brickView.css({ 'backgroundImage': 'url("' + _baseApiUrl + '/v1/image/t/' + brick.AddressX + '/' + brick.AddressY+'")' });
                            //brickView.append('<img src="'+_baseApiUrl+'/v1/image/t/' + brick.AddressX + '/' + brick.AddressY + '" />');
                        }
                        $(brickView).attr('data-addressX', brick.AddressX);
                        $(brickView).attr('data-addressY', brick.AddressY);
                        $(brickView).attr('data-viewX', x);
                        $(brickView).attr('data-viewY', y);

                        brickView.click(function (bv) {
                            openSession($(this).attr('data-viewx'), $(this).attr('data-viewy'), $(this).attr('data-addressx'), $(this).attr('data-addressy'));
                        });
                        row.append(brickView);
                    }
                    tbl.append(row);
                }
                $('#wall').append(tbl);
            });

        //setTimeout(updateWall, 10000);
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
});
