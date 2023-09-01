document.addEventListener('DOMContentLoaded', (event) => {
    const form = document.getElementById('deleteForm');

    form.addEventListener('submit', function (e) {
        e.preventDefault();

        Swal.fire({
            title: 'Are you sure?',
            text: "You won't be able to revert this! Remember, the UNDO button won't bring back your project.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, delete it!',
            cancelButtonText: 'No, cancel!',
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                fetch(form.action, {
                    method: 'POST',
                    body: new FormData(form),
                    headers: {
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    }
                }).then(function (response) {
                    if (response.ok) {
                        Swal.fire(
                            'Deleted!',
                            'Your project has been deleted.',
                            'success'
                        ).then(() => {
                            window.location.href = '@Url.Action("Index", "CanvasMvc")';
                        });
                    } else {
                        Swal.fire(
                            'Error!',
                            'An error occurred while deleting the project.',
                            'error'
                        );
                    }
                }).catch(function () {
                    Swal.fire(
                        'Error!',
                        'An error occurred while deleting the project.',
                        'error'
                    );
                });
            }
        });
    });
});