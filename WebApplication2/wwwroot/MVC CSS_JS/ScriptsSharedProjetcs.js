function editProject(projectId) {
    window.location.href = `/Original_WorkVersion/index.html?projectId=${projectId}`;
}

$(document).ready(function () {
    // Скриване на спинера при зареждане на страницата
    $('#spinner').hide();

    // Функция, която показва спинера при клик върху връзките
    $(document).on('click', 'a', function () {
        $('#spinner').show();
    });

    // Функция, която скрива спинера при приключване на AJAX заявка
    $(document).ajaxStop(function () {
        $('#spinner').hide();
    });
});

$(window).on('load', function () {
    $('#spinner').hide();
});


$(document).ready(function () {
    // Listen for image click events
    $(".card-img-top").on('click', function () {
        var imageUrl = $(this).attr("src");
        // Set the modal image src to the image url
        $("#modalImage").attr("src", imageUrl);
        // Show the modal
        $("#imagePreviewModal").modal('show');
    });

    // Listen for clicks outside the modal or on the image to close the modal
    $("#imagePreviewModal").on('click', function (e) {
        if (e.target.id == "modalImage") {
            $("#imagePreviewModal").modal('hide');
        }
    });
});