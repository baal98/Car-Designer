$(document).ready(function () {

    // This initialises carousels on the container elements specified, in this case, carousel1.
    $("#carousel1").CloudCarousel(
        {
            reflHeight: 40,
            reflGap: 2,
            titleBox: $('#da-vinci-title'),
            altBox: $('#da-vinci-alt'),
            buttonLeft: $('#slider-left-but'),
            buttonRight: $('#slider-right-but'),
            yRadius: 30,
            xPos: 480,
            yPos: 32,
            speed: 0.15,
            autoRotate: "yes",
            autoRotateDelay: 1500
        }
    );
});