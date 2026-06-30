$(document).ready(function (){
	$(".btnLoadEditModal").click(
		function(event){
			if(event){
				const userId = event.target.dataset.userId
				$.ajax({
					contentType : 'text/html; charset=utf-8',
					type:"GET",
					url: `/User/modal-edit-user/${userId}`,
					success: function (res){
						$("#modal").append(res)
					}
				})
			}
		}
	)
	$("#btnLoadAddModal").click(
		function(event){
			$.ajax({
				contentType : 'text/html; charset=utf-8',
				type:"GET",
				url: `/User/modal-add-user`,
				success: function (res){
					$("#modal").append(res)
				}
			})
		}
	)
	$(document).click(function(e) {
		if (!$(e.target).closest('.modal').length) {
			$("#modal").empty();
		}
	});
})
