
//留言显示
$(function () {
    $.ajax({
        url: '/ModelValue/List',
        dataType: 'json',
        type: 'POST',
        data: 'iTableID=38&rows=3',
        success: function (data) {
            if (data.rows.length > 0) {
                var rows = data.rows;
                var html = [];
                $(rows).each(function () {
                    var reply = this.reply;
                    if (reply == undefined)
                        reply = '';
                    html.push('<li>');
                    html.push('<div class="center_fk_d">');
                    html.push('<p class="center_fk_d_p">访客:<span>'+this.UserName+'</span></p>');
                    html.push('<p class="center_fk_d_pa">时间:<span>'+this.s_AddTime+'</span></p>');
                    html.push('</div>');
                    html.push('<div class="center_fa_da">');
                    html.push('<p class="center_fa_da_p">留言标题:<span class="ft_w">'+this.MessageTitle+'</span></p>');
                    html.push('<div class="center_fa_da_p">留言内容：<span class="ft_w">' + this.remark + '</span></div>');
                    html.push('<p class="color_f">站长回复:<span>' + reply + '</span></p>');
                    html.push('</div>');
                    html.push('</li>');
                });
                $('#messageList').append(html.join(''));
            }
        }
    })
})
 
//提交留言
function Addmessage() {
    if($('#UserName').val().length<=0)
    {
        alert('每项必填不能为空!');
        return;
    }
    if($('#MessageTitle').val().length<=0)
    {
        alert('每项必填不能为空!');
        return;
    }
    if($('#tel').val().length<=0)
    {
        alert('每项必填不能为空!');
        return;
    }
    if($('#qq').val().length<=0)
    {
        alert('每项必填不能为空!');
        return;
    }
    if($('#email').val().length<=0)
    {
        alert('每项必填不能为空!');
        return;
    }
    if($('#remark').val().length<=0)
    {
        alert('每项必填不能为空!');
        return;
    }
    if (!validStr('phone', $('#tel').val())) {
        alert('手机格式输入有误!');
        return;
    }
    if (!validStr('email', $('#email').val())) {
        alert('邮箱格式输入有误!');
        return;
    }
        var param = $('#messageForm').serialize();
        var reply = "";
        var addtime = new Date().toLocaleDateString();
        $.ajax({
            url: '/ModelValue/Add?iTableID=38',
            type: 'POST',
            data: param + '&addtime=' + addtime,
            success: function (data) {
                debugger
                if (data == "yes") {
                    alert('留言成功!');
                    location.reload();
                }
            }
        })
    }
