import { createNewObject } from './commonFunctionality.js';

export function loadImageFromFileInput(input) {
    if (input.files && input.files[0]) {
        const file = input.files[0];
        const fileExtension = file.name.split('.').pop().toLowerCase();

        const reader = new FileReader();
        reader.onload = (e) => {
            loadImageFromSrc(e.target.result);
        };
        reader.readAsDataURL(file);
    }
}

// Когато файлът се зарежда от потребителския компютър, неговото съдържание често се конвертира в Base64 формат и няма информация за оригиналното му име в този формат. Ако искате да запазите оригиналното име на файла, трябва да го вземете в момента на зареждане на файла и да го предадете като аргумент(filename) на функцията createNewObject.Обикновено това се случва, когато потребителят избере файл чрез HTML елемент от тип < input type = "file" >. Този код слуша за събитието change на input елемента и когато потребителят избере файл, той се прочита като Data URL (Base64) и се използва за създаване на новия обект. Името на файла се предава като втори аргумент на функцията createNewObject.

document.querySelector('input[type="file"]').addEventListener('change', function (event) {
    const file = event.target.files[0];
    const filename = file.name;
    const reader = new FileReader();

    reader.onload = function (loadEvent) {
        const src = loadEvent.target.result; // Base64 representation of the file
        createNewObject(src, filename);
    }

    reader.readAsDataURL(file); // Reads the file as Data URL (Base64)
});



// Добавяне на обекти чрез кликане на бутоните с event listener.
document.querySelectorAll('#objects button[data-src]').forEach((button) => {
    button.addEventListener('click', () => {
        const src = button.getAttribute('data-src');
        const filename = src.split('/').pop();
        createNewObject(src, filename);
    });
});



//document.querySelectorAll('#objects button[data-src]').forEach((button) => {
//    button.addEventListener('click', () => {
//        const src = button.getAttribute('data-src');
//        createNewObject(src); //src е пътят до изображението на обекта, което се взима от атрибута data-src на бутона, който е натиснат
//    });
//});
