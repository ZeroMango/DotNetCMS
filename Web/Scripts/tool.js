$(document).ready(function(){
	  $('#navArea li').mousemove(function(){
		$(this).find('div').css("top",$(this).position().top+50);
		$(this).find('div').css("left",$(this).position().left);
	  	$(this).find('div').slideDown();//you can give it a speed
	  });
	  $('#navArea li').mouseleave(function(){
	  	$(this).find('div').slideUp("fast");
	  });
	  
});