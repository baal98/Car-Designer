$(document).ready(function () {
    checkForNewMessages();

    setInterval(checkForNewMessages, 30000);
});

function checkForNewMessages() {
    $.get('/Chat/GetNewMessagesCount', function (data) {
        if (data > 0) {
            $('#notificationIcon').removeClass('hidden');
            $('#messageCount').text(data);
        } else {
            $('#notificationIcon').addClass('hidden');
            $('#messageCount').text('');
        }
    });
}
