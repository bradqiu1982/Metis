var MyVideoRecord = function () {
        var virec = null;
        var videoblob = null;
        var countdowntime = 180;
        var functioncalltime = 0;
        var timerInterval = null;

    function detectIE() {
          var ua = window.navigator.userAgent;

          // Test values; Uncomment to check result …
          // IE 10
          // ua = 'Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)';
          // IE 11
          // ua = 'Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko';
          // Edge 12 (Spartan)
          // ua = 'Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.71 Safari/537.36 Edge/12.0';
          // Edge 13
          // ua = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586';

          var msie = ua.indexOf('MSIE ');
          if (msie > 0) {
            // IE 10 or older => return version number
            return parseInt(ua.substring(msie + 5, ua.indexOf('.', msie)), 10);
          }
          var trident = ua.indexOf('Trident/');
          if (trident > 0) {
            // IE 11 => return version number
            var rv = ua.indexOf('rv:');
            return parseInt(ua.substring(rv + 3, ua.indexOf('.', rv)), 10);
          }
          var edge = ua.indexOf('Edge/');
          if (edge > 0) {
            // Edge (IE 12+) => return version number
            return parseInt(ua.substring(edge + 5, ua.indexOf('.', edge)), 10);
          }
          // other browser
          return false;
    }

    var videoinit = function () {
        $('body').on('click', '.maineditor-video', function () {

            if (detectIE() != false)
            {
                alert('IE does not support video function!')
                return false;
            }

            $("#m-video-play").addClass('hidden');
            $("#m-video-record").removeClass('hidden');

            $(".mc-end").addClass('hidden');
            $(".mc-play").addClass('hidden');
            $(".mc-start").removeClass('hidden');
            $(".m-video-loading").addClass('hidden');

            $('#modal-video').modal({ backdrop: 'static' });

            if (virec === null) {
                virec = new VideoRecorderJS.init(
                {
                    resize: 0.8, // recorded video dimentions are 0.4 times smaller than the original
                    webpquality: 0.5, // chrome and opera support webp imags, this is about the aulity of a frame
                    framerate: 30,  // recording frame rate
                    videotagid: "m-video-record",
                    videoWidth: "640",
                    videoHeight: "480",
                    log: true,
                    workerPath: "../../Scripts/VideoRecorderJs-master/dist/recorderWorker.js"
                },
                function () {
                    //success callback. this will fire if browsers supports
                },
                function (err) {
                    //onerror callback, this will fire for mediaErrors
                    if (err.name == "BROWSER_NOT_SUPPORTED") {
                        //handler code goes here
                    } else if (err.name == "PermissionDeniedError") {
                        //handler code goes here
                    } else if (err.name == "NotFoundError") {
                        //handler code goes here
                    } else {
                        throw 'Unidentified Error.....';
                    }

                }
                );
            }
        })

        $('body').on('click', '.video-close', function () {
            $('#modal-video').modal('hide');
            if (virec) {
                //virec.clearRecording();
                stopCountDown();
                virec = null;
                $('#m-video-play').attr('src', '');
                $('#m-video-record').attr('src', '');
            }
        })

        $('body').on('click', '.m-video-op', function () {
            $('.m-video-op').parent().addClass('hidden');
            if ($(this).attr('data-val') == 'start') {
                //start record
                virec.startCapture();
                stopCountDown();
                startCountDown();
                $('.mc-end').removeClass('hidden');
            }
            else if ($(this).attr('data-val') == 'end') {

                $('#m-video-record').addClass('hidden');
                $('#m-video-play').removeClass('hidden');

                //stop record
                virec.stopCapture(oncaptureFinish);
                stopCountDown();

                $('.mc-play').removeClass('hidden');
                //$('#m-play').removeClass('hidden');
                //$('#m-pause').addClass('hidden');
            }

        })


        function oncaptureFinish(result) {
            result.forEach(function (item) {
                if (item.type == "video") {
                    videoblob = item.blob;
                    var videobase64 = window.URL.createObjectURL(videoblob);
                    document.getElementById('m-video-play').src = videobase64;
                }
            });
        }

        function setCountDownTime(time) {
            $('#countdown').html(time);
            return time;
        }


        function startCountDown() {
            if (timerInterval == null) {
                functioncalltime = countdowntime;
                var value = setCountDownTime(functioncalltime);
                timerInterval = setInterval(function () {
                    var value = setCountDownTime(--functioncalltime);
                    if (value == 0) {
                        clearInterval(timerInterval);
                        virec.stopCapture(oncaptureFinish);
                    }
                }, 1000);
            }
        }

        function stopCountDown() {
            if (timerInterval) {
                clearInterval(timerInterval);
                timerInterval = null;
            }
        }
    };

    return {
        Init: function () {
            videoinit();
        },
        GetViRec: function ()
        {
            return virec;
        },
        SetViRec: function (v)
        {
            virec = v;
        },
        GetVideoBlob: function ()
        {
            return videoblob;
        },
        StopTheCountDown: function () {
            if (timerInterval) {
                clearInterval(timerInterval);
                timerInterval = null;
            }
        }

    };
}();