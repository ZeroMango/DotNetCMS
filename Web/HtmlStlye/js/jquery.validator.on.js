
 $(document).ready(function () {
 
  //登录
 
 //表单验证用户名、密码、确认密码
    $('#txtStudentname').focus(function () {
        $("#name").html("");
        var emailTXT = $('#txtStudentname').val();
        if (emailTXT == "") {
            $("#name").html("请输入用户名").css({ "color": "#333" });;
            return;
        }

    });
    $('#txtStudentname').blur(function () {
        $("#name").html("");
        var name = $('#txtStudentname').val();
        if (name == "$('#txtStudentname').val()") {
            $("#name").html("用户名不能为空").css({ "color": "red" });;
            return;
        }
        if (name.length < 2 || name.length > 10) {
            $("#name").html("用户名长度必须2到10位").css({ "color": "red" });;
            return;
        }
  })
 


    $('#txtpassword').focus(function () {
        $("#password").html("");
        var txtpassword = $('#txtpassword').val();
        if (txtpassword == "") {
            $("#password").html("请输入密码").css({ "color": "#333" });;
            return;
        }

    });
 
  $('#txtpassword').blur(function () {
        $("#password").html("");
        var txtpassword = $('#txtpassword').val();
        if (txtpassword == "") {
            $("#password").html("登陆密码不能为空");
            return;
        }
        //var patt = /^(?![a-z]+$)(?!\d+$)[a-z0-9]{6,20}$/i;
        //var patt = /(?=.*\d.*)(?=.*\D.*).{6,20}/;
        if (txtpassword.length<6) {
            $("#password").html("登录密码至少6位").css({ "color": "red" });
            return;
        }

    });
  
  
  
  
   $('#txtCurword').focus(function () {
        $("#curword").html("");
        var txtCurword = $('#txtCurword').val();
        if (txtCurword == "") {
            $("#curword").html("请输入确认密码").css({ "color": "#333" });
            return;
        }

    });
    $('#txtCurword').blur(function () {
        $("#curword").html("");
        var txtCurword = $('#txtCurword').val();
        if (txtCurword == "$('#txtCurword').val()") {
            $("#curword").html("确认密码不能为空");
            return;
        }

    });
    //密码验证	
    $('#txtCurword').blur(function () {
        var texpwd = $("#txtpassword");
        var texpwd2 = $("#txtCurword");
        if (texpwd.val() != texpwd2.val()) {
            $("#curword").html("密码输入不一致,请重新输入").css({ "color": "red" });
        }
        else {
            $("#curword").html("");
        }

    })


  
   //提交验证
    $(".login_submit").click(function () {
       
 
        var emailTXT = $('#txtStudentname').val();


        var txtpassword = $('#txtpassword').val();
        if ( (emailTXT == "") && (txtpassword == "")) {

            $("#name").html("用户名不能为空");


            $("#password").html("密码不能为空");
            $(".tips").css("color", "red");
            return;
        }
      
       

        $("#name").html("");
        var name = $('#txtStudentname').val();
        if (name == "") {
            $("#name").html("请输入用户名").css({ "color": "#333" });
            return;
        }



        $("#password").html("");
        var txtpassword = $('#txtpassword').val();
        if (txtpassword == "") {
            $("#password").html("请输入密码").css({ "color": "#333" });
            return;
        }

        //var patt = /^(?!\D+$)(?![^a-zA-Z]+$)[a-zA-Z0-9]{6,20}$/; ///(?=.*\d.*)(?=.*\D.*).{6,20}/;
        if (txtpassword.length<6) {
            $("#password").html("登录密码至少6位").css({ "color": "red" });
            return;
        }

      
     

    })
  
/*重置按钮*/
 
   $(".re_submit").click(function () {       
       $("#txtStudentname").val(""); 
	   $("#txtpassword").val(""); 
	   $("#txtCurword").val("");      

    })
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
  })
 
 
 
 