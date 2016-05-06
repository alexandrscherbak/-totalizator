function showLoginPopup() {
    $.ajax({
        type: "GET",
        url: "/Account/LoginPopup",
        success: function (data) {
            $(".modal-backdrop").remove();
            var popupWrapper = $("#PopupWrapper");
            popupWrapper.empty();
            popupWrapper.html(data);
            var popup = $(".modal", popupWrapper);
            $(".modal", popupWrapper).modal();
        }
    });
}