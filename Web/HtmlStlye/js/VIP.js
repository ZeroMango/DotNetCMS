//VIP 
$(function () {
    var sUserName = $('#sUserName').val();
    var VIPCode = $('#VIPCode').val();
    
    $.ajax({
        url: '/User/QueryList',
        dataType: 'json',
        type: 'POST',
        data: '?sUserName=' + sUserName + '&VIPCode=' + VIPCode + '',
        success: function (data) {
            VIP_bind(data);
        }
    });

    //------------- 绑定最新注册数据----------------
    function VIP_bind(data) {
        var vip_ul = $('.vip_ul');
        var html = '';
        vip_ul.html('');
        $.each(data.memberlist, function () {
            html += '<li>' + this.s_UserName + '</li>';
        });
        vip_ul.html(html);
    }
})

//查询会员

function QueryMember()
{
    var sUserName = $('#sUserName').val();
    var VIPCode = $('#VIPCode').val();
    if (sUserName == '' && VIPCode == '') return alert('请至少填写姓名或会员号一种');
    $.ajax({
        url: '/User/QueryList',
        dataType: 'json',
        type: 'POST',
        data: 'sUserName=' + sUserName + '&VIPCode=' + VIPCode + '',
        success: function (data) {
            VIPMember_Bind(data);
        }
    });
}

//------------绑定查询会员HTML---------------------
function VIPMember_Bind(data) {
    var vip_tb = $('.vip_tb');
    var html = '';
    vip_tb.html('');
    if (data.List.length==0) {
        html += '<tr class="vip_tb_r"><td colspan="3" style="text-align:center">无此相关数据</td></tr>';
    }
    else {
        //绑定会员查询
        $.each(data.List, function () {
            html += '<tr class="vip_tb_r">';
            html += '<td>' + this.s_VIPCode + '</td>';
            html += '<td>' + this.s_UserName + '</td>';
            if (this.sAdderss == null || this.sAdderss == "")
                html += '<td style="text-align:center">----</td>';
            else
                html += '<td>' + this.sAdderss + '</td>';
            html += '</tr>';
        });
    }
    vip_tb.html(html);
    
}