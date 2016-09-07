//javascript 很多常用的公共方法






//图片地址不存在，或地址错误时，显示默认图片
function imgUrlError(defaultImgUrl) {
	$("img").each(function() {
		if ($(this).attr("src") == "") {
			$(this).attr("src", defaultImgUrl);
		} else {
			$(this).attr("onerror", "javascript:this.src='"+defaultImgUrl+"'");
		}
	});
}


//Ajax请求惯用方法
function Ajax(postUrl, postData) {
    $.ajax({
        type: "POST",
        async:false,
        url: postUrl, //用与处理ajax的地址及函数
        data: postData,
        dataType: "json",
        success: function (data) {
            return data;
        },
        error: function () {
            return "请检查网络连接...";
        }
    });
}

//验证手机、邮箱、密码、金额
function validStr(validType,validStr)
{
    var valiStr = "";		//验证手机的正则表达式
	switch(validType)
	{
	    case "phone": valiStr = /^1[3|4|5|7|8][0-9]\d{8}$/; break;
		case "email":valiStr = /^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$/; break;
		case "password":valiStr = /^(?![a-z]+$)(?!\d+$)[a-z0-9]{6,20}$/i; break;
		case "money":valiStr = /^((^0\.\d{0,2}$)|(^[1-9]+\d*\.\d{0,2}$)|(^[1-9]\d*$)|(^0$))?$/; break;
		default: break;
	}
	return valiStr.test(validStr);
}

//获取地址栏传递的参数
function urlKey(keyName) {
    var locationHref = location.href;
    var reg = new RegExp("(^|&)" + keyName + "=([^&]*)(&|$)");
    var kayValue = locationHref.substr(locationHref.indexOf("?") + 1).match(reg);
    if (kayValue != null) return unescape(kayValue[2]); return "";
}



$(function(){
	imgUrlError("/HtmlStlye/img/default.png");
})
