$(".messages").animate({ scrollTop: "99999" }, "fast");

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/chat")
    .build();

connection.start()
    .then(function () {
        console.log("connected");
    })
    .catch(function (err) {
        console.error(err.toString());
    });

connection.on("NewMessage",
    function (message) {
        var chatClass = (message.username != '@ViewBag.CurrentUsername') ? 'chat chat-left' : 'chat';
        var chatInfo = getChatInfo(message, chatClass);
        $("#messagesList").append(chatInfo);
        $(".messages").animate({ scrollTop: "99999" }, "fast");
    });

$('#sendButton').click(function () {
    var message = $('#messageInput').val();

    var minLength = 2;
    var maxLength = 300;

    if (message === null || message === '') {
        alert('Cannot Publish Empty Message.');
        return;
    }
    if (message.length < minLength || message.length > maxLength) {
        alert(`Message should have minimum length ${minLength} and maximum length ${maxLength}.`);
        return;
    }

    console.log("Sending message: " + message);
    connection.invoke('Send', message);
    $('#messageInput').val('');
});

// Добавяне на слушател за клавиша Enter
$('#messageInput').on('keydown', function (e) {
    if (e.key === 'Enter' || e.keyCode === 13) {
        $('#sendButton').click();
    }
});

connection.start()
    .then(function () {
        console.log("Connected!");
    })
    .catch(function (err) {
        return console.error(err.toString());
    });

function escapeHtml(unsafe) {
    return unsafe
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

function getChatInfo(message, chatClass) {
    return `
                        <div class="${chatClass}">
                            <div class="chat-avatar">
                                <a class="avatar avatar-online" data-toggle="tooltip" href="#" data-placement="right" title="" data-original-title="${message.username}">
                                    <img src="${message.userImageUrl}" alt="${message.username}">
                                    <i></i>
                                </a>
                            </div>
                            <div class="chat-body">
                                <div class="chat-content">
                                    <p><strong>${message.username}: </strong></p>
                                    <p>${message.text}</p>
                                    <time class="chat-time" datetime="${moment().format('YYYY-MM-DDTHH:mm:ss')}">${moment().format('DD.MM.yyyy HH:mm:ss')}</time>
                                </div>
                            </div>
                        </div>`;
}

// Добавяне на обработчик за клик събитието на бутона за обновяване
$('#refreshButton').click(function () {
    $.get('/Chat/GetChatMessages', function (data) {
        console.log(data);
        $('#historyMessagesList').empty();
        data.$values.forEach(function (message) {
            var chatClass = (message.username != '@ViewBag.CurrentUsername') ? "chat chat-left" : "chat";
            var newMessage = `
                <div class="${chatClass}">
                    <div class="chat-avatar">
                        <a class="avatar avatar-online" data-toggle="tooltip" href="#" data-placement="right" title="" data-original-title="${message.username}">
                            <img src="${message.userImageUrl}" alt="${message.username}">
                            <i></i>
                        </a>
                    </div>
                    <div class="chat-body">
                        <div class="chat-content">
                            <p><strong>${message.username}: </strong>${message.text}</p>
                            <time class="chat-time" datetime="${message.createdOn}">${message.createdOn}</time>
                        </div>
                    </div>
                </div>
            `;
            $('#historyMessagesList').append(newMessage);
        });
    });
});