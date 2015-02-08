$("div.news-buttons > a:has(i.fa-heart-o)").click(function (e) {
    e.preventDefault();
    var link = this;
    $.get(this.href);
    $("i", link).removeClass("fa-heart-o").addClass("fa-heart");
    $("i", link).addClass("favorite-news-i");
    $(link).css("background-color", "#7DD7C4");
    $("div.ibox[data-news-id=" + $(link).attr("data-link-news-id") + "]").fadeToggle();
});

$("div.news-buttons > a:has(i.fa-remove)").click(function (e) {
    e.preventDefault();
    var link = this;
    $.get(link.href);
    $(link).css("background-color", "red");
    $("div.ibox[data-news-id=" + $(link).attr("data-link-news-id") + "]").fadeOut();
});

$("div.news-buttons > a:has(i.fa-clock-o)").click(function (e) {
    e.preventDefault();
    var link = this;
    $.get(this.href);
    $(link).css("background-color", "#7DD7C4").css("color", "#444");
    $("div.ibox[data-news-id=" + $(link).attr("data-link-news-id") + "]").fadeToggle();
});

$("#subscribe-link").click(function (e) {
    e.preventDefault();
    $.get(this.href);
    $(this).fadeOut();
});

$(function () {
    $(".ibox").each(function () {
        var myElement = this;
        var hammertime = new Hammer(myElement);
        hammertime.on('swipe', function (ev) {
            console.log(ev);
            $(myElement).fadeOut();
            $.get($("div.news-buttons > a:has(i.fa-remove)", myElement).prop("href"));
        });
    });    
});