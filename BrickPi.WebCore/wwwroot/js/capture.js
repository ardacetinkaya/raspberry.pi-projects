var data = {
    action: 'None',
    source: '/images/Image.jpg'
}

$(window).keydown(function (e) {
    key = (e.keyCode) ? e.keyCode : e.which;
    $('.key.c' + key).addClass('keydown');
    Action(key);
});

$(window).keyup(function (e) {
    key = (e.keyCode) ? e.keyCode : e.which;
    $('.key.c' + key).removeClass('keydown');

});
$(".key").click(function () {
    var controlId = $(this).attr("id");
    switch (controlId) {
        case 'capturestop':
            data.action = 'capturestop';
            $('#capturestart').css("pointer-events", "all");
            $('#capturestop').css("pointer-events", "none");
            break;
        case 'capturestart':
            data.action = 'capturestart';
            $('#capturestart').css("pointer-events", "none");
            $('#capturestop').css("pointer-events", "all");
            break;
        default:
            Action(controlId);

    }
});

function Action(actionName) {
    var direction = {
        Direction: actionName,
        Speed: 50
    }
    $.ajax({
            url: '/api/Movement/',
            type: 'POST',
            dataType: 'text',
            data: JSON.stringify(direction),
            contentType: 'application/json'
        })
        .done(function (data, textStatus, jqXHR) {})
        .error(function (request, status, error) {
            console.log(error)
        })
        .always(function (data) {});
}

Vue.component('capture', {
    template: '<span><img v-bind:src="source" width="100%" height="auto" /><br />Last Update: {{ last }}</span>',
    props: ["last"],
    data: function () {
        return data
    },
    created: function () {
        $.ajax({
                url: '/api/Movement/',
                type: 'GET',
                dataType: 'text',
                contentType: 'application/json'
            })
            .done(function (data, textStatus, jqXHR) {
                if (data === 'True') {
                    $('#capturestart').css("pointer-events", "none");
                    $('#capturestop').css("pointer-events", "all");
                } else {
                    $('#capturestart').css("pointer-events", "all");
                    $('#capturestop').css("pointer-events", "none");
                }
            })
            .error(function (request, status, error) {
                console.log(error);
            })
            .always(function (data) {});
        this.timer = setInterval(this.captureProcess, 3000);
    },
    methods: {
        captureProcess: function () {
            console.log(data.action);
            if (data.action === 'capturestart')
                Action(data.action);

            data.source = "/images/Image.jpg?" + new Date().getTime();
            this.last = new Date().toLocaleString();

        },
        cancelAutoUpdate: function () {
            clearInterval(this.timer)
        }

    },
    beforeDestroy() {
        clearInterval(this.timer)
    }
});

var captureComponent = new Vue({
    el: '#imageCapture',
    data: function data() {
        return {
            date: new Date().toLocaleString()
        }
    }
});