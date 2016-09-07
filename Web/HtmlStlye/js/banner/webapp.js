//Ê×Ò³Banner
function Banner() {
    var active = 0, as = document.getElementById('pagenavi').getElementsByTagName('a');
    //ÂÖ²¥ÇÐ»»Ê±¼ä
    var intervalTime = document.getElementById("pagenavi").getAttribute("data-intervaltime");
    if (typeof intervalTime == 'undefined' || intervalTime == 'undefined' || intervalTime == null) {
        intervalTime = 6000;
    } else {
        intervalTime = intervalTime * 1000;
    }
    for (var i = 0; i < as.length; i++) {
        (function () {
            var j = i;
            as[i].onclick = function () {
                t2.slide(j);
                return false;
            }
        })();
    }
    var t1 = new TouchScroll({ id: 'wrapper', 'width': 5, 'opacity': 0.7, color: '#555', minLength: 20 });
    var t2 = new TouchSlider({
        id: 'slider', speed: 600, timeout: intervalTime, before: function (index) {
            as[active].className = '';
            active = index;
            as[active].className = 'active';
        }
    });
}
