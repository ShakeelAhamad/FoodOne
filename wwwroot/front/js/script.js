// ============ Navigation ============
$(".nav-blacklayer").hide();
const hamburger = document.querySelector(".hamburger");
const navMenu = document.querySelector(".nav-menu");
hamburger.addEventListener("click", mobileMenu);
function mobileMenu() {
    hamburger.classList.toggle("active");
    navMenu.classList.toggle("active");
    $(".nav-blacklayer").show();
}
const navLink = document.querySelectorAll(".nav-link");
navLink.forEach(n => n.addEventListener("click", closeMenu));
function closeMenu() {
    hamburger.classList.remove("active");
    navMenu.classList.remove("active");
    $(".nav-blacklayer").hide();
}
$(document).ready(function () {
    $('.close-nav').click(function () {
        hamburger.classList.remove("active");
        navMenu.classList.remove("active");
        $(".nav-blacklayer").hide();
    });
    $('.nav-blacklayer').click(function () {
        hamburger.classList.remove("active");
        navMenu.classList.remove("active");
        $(".nav-blacklayer").hide();
    });
});
// User Drop Down
$(".drop-down .selected a").click(function () {
    $(".drop-down .options ul").toggle();
});
//SELECT OPTIONS AND HIDE OPTION AFTER SELECTION
$(".drop-down .options ul li a").click(function () {
    var text = $(this).html();
    $(".drop-down .selected a span").html(text);
    $(".drop-down .options ul").hide();
});
//HIDE OPTIONS IF CLICKED ANYWHERE ELSE ON PAGE
$(document).bind('click', function (e) {
    var $clicked = $(e.target);
    if (!$clicked.parents().hasClass("drop-down"))
        $(".drop-down .options ul").hide();
});




