//$(document).ready(function () {
//    // Hide the spinner when the page loads
//    $('#spinner').hide();

//    // Add click event listener to the edit button
//    $('.edit-link').click(function () {
//        $('#spinner').show(); // Show the spinner
//        var projectId = $(this).attr('data-project-id'); // Get the project id from the button's data-project-id attribute

//        // Make a GET request to the Edit action
//        $.ajax({
//            url: '@Url.Action("Edit", "CanvasMvc")',
//            type: 'GET',
//            data: { id: projectId }
//        }).done(function () {
//            // If the request succeeds, redirect to the Edit page
//            window.location.href = '@Url.Action("Edit", "CanvasMvc")?id=' + projectId;
//        }).fail(function (jqXHR) {
//            // If the request fails, show an error message
//            if (jqXHR.status === 404) {
//                Swal.fire(
//                    'Error!',
//                    'The project could not be found.',
//                    'error'
//                ).then(function () {
//                    // After showing the error message, redirect to the Index page
//                    window.location.href = '@Url.Action("Index", "CanvasMvc")';
//                });
//            } else {
//                Swal.fire(
//                    'Error!',
//                    'An unknown error occurred.',
//                    'error'
//                );
//            }
//        }).always(function () {
//            $('#spinner').hide(); // Hide the spinner
//        });
//    });


//    // Add click event listener to the delete button
//    $('.delete-link').click(function () {
//        $('#spinner').show(); // Show the spinner
//        var projectId = $(this).attr('data-project-id'); // Get the project id from the button's data-project-id attribute

//        // Show a confirmation dialog
//        Swal.fire({
//            title: 'Are you sure?',
//            text: "You won't be able to revert this!",
//            icon: 'warning',
//            showCancelButton: true,
//            confirmButtonText: 'Yes, delete it!',
//            cancelButtonText: 'No, cancel!',
//            reverseButtons: true
//        }).then((result) => {
//            if (result.isConfirmed) {
//                // If the user confirms, send a DELETE request to the Delete action
//                $.ajax({
//                    url: '@Url.Action("Delete", "CanvasMvc")',
//                    type: 'POST',
//                    data: {
//                        id: projectId,
//                        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() // Include the anti-forgery token
//                    }
//                }).done(function () {
//                    // If the request succeeds, show a success message and redirect to the Index page
//                    Swal.fire(
//                        'Deleted!',
//                        'Your project has been deleted.',
//                        'success'
//                    ).then(function () {
//                        window.location.href = '@Url.Action("Index", "CanvasMvc")';
//                    });
//                }).fail(function () {
//                    // If the request fails, show an error message
//                    Swal.fire(
//                        'Error!',
//                        'An error occurred while deleting the project.',
//                        'error'
//                    );
//                }).always(function () {
//                    $('#spinner').hide(); // Hide the spinner
//                });
//            } else {
//                $('#spinner').hide(); // Hide the spinner
//            }
//        });
//    });
//});

document.addEventListener('DOMContentLoaded', function () {
    // Hide the spinner when the page loads
    document.getElementById('spinner').style.display = 'none';

    // Add click event listener to the edit buttons
    Array.from(document.getElementsByClassName('edit-link')).forEach(function (editButton) {
        editButton.addEventListener('click', function () {
            document.getElementById('spinner').style.display = 'block'; // Show the spinner
            var projectId = this.getAttribute('data-project-id'); // Get the project id from the button's data-project-id attribute

            // Make a GET request to the Edit action
            fetch('@Url.Action("Edit", "CanvasMvc")' + '?id=' + projectId)
                .then(function (response) {
                    if (response.ok) {
                        // If the request succeeds, redirect to the Edit page
                        window.location.href = '@Url.Action("Edit", "CanvasMvc")' + '?id=' + projectId;
                    } else {
                        // If the request fails, show an error message
                        if (response.status === 404) {
                            Swal.fire('Error!', 'The project could not be found.', 'error')
                                .then(function () {
                                    // After showing the error message, redirect to the Index page
                                    window.location.href = '@Url.Action("Index", "CanvasMvc")';
                                });
                        } else {
                            Swal.fire('Error!', 'An unknown error occurred.', 'error');
                        }
                    }
                })
                .finally(function () {
                    document.getElementById('spinner').style.display = 'none'; // Hide the spinner
                });
        });
    });

    // Add click event listener to the delete buttons
    Array.from(document.getElementsByClassName('delete-link')).forEach(function (deleteButton) {
        deleteButton.addEventListener('click', function () {
            document.getElementById('spinner').style.display = 'block'; // Show the spinner
            var projectId = this.getAttribute('data-project-id'); // Get the project id from the button's data-project-id attribute

            // Show a confirmation dialog
            Swal.fire({
                title: 'Are you sure?',
                html: '<img src="@Url.Content("~/Images/ThinkItFace.png")" /> <br/> <p>You won\'t be able to revert this!</p>',
                icon: null, // Set the icon to null
                showCancelButton: true,
                confirmButtonText: 'Yes, delete it!',
                cancelButtonText: 'No, cancel!',
                reverseButtons: true,
                showClass: {
                    popup: 'animate__animated animate__fadeInDown'
                },
                hideClass: {
                    popup: 'animate__animated animate__fadeOutUp'
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    // If the user confirms, send a DELETE request to the Delete action
                    fetch('@Url.Action("Delete", "CanvasMvc")', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/x-www-form-urlencoded',
                            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                        },
                        body: new URLSearchParams({ id: projectId }).toString()
                    }).then(function (response) {
                        if (response.ok) {
                            // If the request succeeds, show a success message and redirect to the Index page
                            Swal.fire({
                                title: 'Phhh!',
                                html: '<img src="@Url.Content("~/Images/CryingFace.png")" /> <br/> <p>Your project has been deleted!</p>',
                                confirmButtonText: 'Okay',
                                showClass: {
                                    popup: 'animate__animated animate__fadeInDown'
                                },
                                hideClass: {
                                    popup: 'animate__animated animate__fadeOutUp'
                                }
                            }).then(function () {
                                window.location.href = '@Url.Action("Index", "CanvasMvc")';
                            });
                        } else {
                            // If the request fails, show an error message
                            Swal.fire({
                                title: 'Error!',
                                html: '<img src="@Url.Content("~/Images/Ooops.png")" /> <br/> <p>An error occurred while deleting the project.</p>',
                                icon: 'error',
                                confirmButtonText: 'Okay',
                                showClass: {
                                    popup: 'animate__animated animate__fadeInDown'
                                },
                                hideClass: {
                                    popup: 'animate__animated animate__fadeOutUp'
                                }
                            });
                        }
                    }).finally(function () {
                        document.getElementById('spinner').style.display = 'none'; // Hide the spinner
                    });
                } else {
                    document.getElementById('spinner').style.display = 'none'; // Hide the spinner
                }
            });
        });
    });
});