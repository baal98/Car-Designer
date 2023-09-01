// Импортиране на необходимите функции от други файлове
import { loadImageFromFileInput } from "./backgroundLogic.js";
import {
    createRuler,
    createNumber,
    createMiddleNumber,
} from "./rullers_functions.js";

// Експортиране на функцията loadProject
//export { loadProject };

document.addEventListener("DOMContentLoaded", () => {
    const canvas = document.getElementById("canvas");

    createMiddleNumber(); // създаване на линеен рулер с междинни числа
    createNumber(); // създаване на линеен рулер с числа
    createRuler(); // създаване на линеен рулер

    // Извикване на функцията loadProject с projectId
    if (window.projectId) {
        loadProject(window.projectId);
    }
});




