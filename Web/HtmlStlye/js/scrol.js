$(document).ready(function () {
    //alert(sessionStorage.getItem(0));
    if (sessionStorage.getItem(0) != null) {
        $(".btn").hide();
        $(".showName").show();
        $(".showName1").show();
        var el = document.getElementById("userN");
        el.innerHTML = "用户: " + sessionStorage.getItem(0) + " 你好! ";
    }
    else {
        $(".showName").hide();
        $(".showName1").hide();
        $(".btn").show();
    }
    $(function () {
        $("#zhuxiao").click(function () {
            sessionStorage.clear();
            window.location.href = "/index.html";
        })
    })
})
function ScrollImgLeft() {
    var speed = 30
    var scroll_begin = document.getElementById("scroll_begin");
    var scroll_end = document.getElementById("scroll_end");
    var scroll_div = document.getElementById("scroll_div");
    scroll_end.innerHTML = scroll_begin.innerHTML
    function Marquee() {
        if (scroll_end.offsetWidth - scroll_div.scrollLeft <= 0)
            scroll_div.scrollLeft -= scroll_begin.offsetWidth
        else
            scroll_div.scrollLeft++
    }
    var MyMar = setInterval(Marquee, speed)
    scroll_div.onmouseover = function () { clearInterval(MyMar) }
    scroll_div.onmouseout = function () { MyMar = setInterval(Marquee, speed) }
}
