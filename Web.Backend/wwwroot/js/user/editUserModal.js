$(document).ready(function (){
	const forms = document.querySelectorAll('.needs-validation')
	Array.prototype.slice.call(forms)
		.forEach(function (form) {
			form.addEventListener('submit', function (event) {
				if (!form.checkValidity()) {
					event.preventDefault()
					event.stopPropagation()
				}
				form.classList.add('was-validated')
			}, false)
		})

	$("#btnSubmit").click(function (event){
		if(event){
			const userId = event.target.dataset.userId
			const firstname = $("#firstname").val();
			const lastname = $("#lastname").val();
			const username = $("#username").val();
			const email = $("#email").val();
			const phoneNumber = $("#phoneNumber").val();
			const roleIds =$("#selectRole :selected").map(function(i, el) {
							return $(el).val();
							}).get()
			const id = userId;
			$.ajax({
				type: "PUT",
				url: `/User/update/${userId}`,
				data:{id, firstname, lastname , username, email, phoneNumber, roleIds},
				success: function (res){
					$("#modalEdit").removeClass("show")
					$(".modal-backdrop").removeClass("show")
					window.location.replace("User")
					alert("Chỉnh sửa nhân viên thành công");
				},
				error: function (err) {
					alert("Đã xảy ra lỗi trông quá trình chỉnh sửa");
				}
			})
		}
		
	})
})
