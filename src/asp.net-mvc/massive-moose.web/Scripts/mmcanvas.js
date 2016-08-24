$(document).ready(function(){
    $('#drawSpace').show();
    var _toolsWaitHandle=0;
        _lc = LC.init(document.getElementById('drawSpace'),
        {
            imageURLPrefix: "/content/img",
            imageSize: { width: 1600, height: 800 },
            //backgroundColor: "#ED7428",
            backgroundColor:"transparent",
            toolbarPosition: 'top'
        });

        $('.lc-options').append('<div class="session-options"><button id="save-button">Finish</button><button id="cancel-button">Cancel</button></div>');

        //$('.lc-picker').toggle( "slide" );
        //$('.horz-toolbar').toggle( "slide", {direction:'up'} );

        var unsubscribeOnDrawStart = _lc.on('drawStart', function(arguments) {
            if ($('.lc-picker').is(":visible"))
            {
                $('.lc-picker').toggle( "slide", null, 100 );
                $('.horz-toolbar').toggle( "slide", {direction:'up'}, 100 );
            }
        // do stuff
        });
        var unsubscribeOnDrawStart = _lc.on('drawEnd', function(arguments) {
            if (_toolsWaitHandle != 0)
            {
                clearTimeout(_toolsWaitHandle);
            }
            _toolsWaitHandle = setTimeout(toggleTools, 2000);
        // do stuff
        });
        //unsubscribe();

        function toggleTools()
        {
            $('.lc-picker').toggle( "slide" );
            $('.horz-toolbar').toggle( "slide", {direction:'up'} );
            if (_toolsWaitHandle != 0)
            {
                clearTimeout(_toolsWaitHandle);
            }
        }
});