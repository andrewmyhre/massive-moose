$(document).ready(function () {
    var lc = LC.init(document.getElementById('root'),
    {
        imageURLPrefix: "/content/img",
        imageSize: { width: 1600, height: 800 },
        backgroundColor: "#FFED7428",
        toolbarPosition: 'top'
    });

    $('[data-action=upload]').click(function (e) {
        e.preventDefault();

        $('.finish-and-upload').html('Uploading...');

        //$('.finish-and-upload').html('<code>' + JSON.stringify(lc.getSnapshot()) + '</code>');

        $.post('http://local.api.massivemoose.com/literally/receive/0/0',
            JSON.stringify(lc.getSnapshot()),
            function(data){
                $('.finish-and-upload')
                    .html('<form class="finish-and-upload"><input type="submit" data-action="upload" value="Finish and Upload"></form>');
            },
            'json');

        // this is all standard Imgur API; only LC-specific thing is the image
        // data argument;
        /*$.ajax({
            url: 'https://api.imgur.com/3/image',
            type: 'POST',
            headers: {
                // Your application gets an imgurClientId from Imgur
                Authorization: 'Client-ID ' + imgurClientId,
                Accept: 'application/json'
            },
            data: {
                // convert the image data to base64
                image:  lc.canvasForExport().toDataURL().split(',')[1],
                type: 'base64'
            },
            success: function(result) {
                var url = 'https://imgur.com/gallery/' + result.data.id;
                $('.finish-and-upload').html("<a href='" + url + "'>" + url + "</a>");
            },
        });*/
    });
});