$("div.news-buttons > a:has(i.fa-heart-o)").click(function (e) {
    e.preventDefault();
    var link = this;
    $.get(this.href, function (data) {
            if (data.result === true) {
                $("i", link).removeClass("fa-heart-o").addClass("fa-heart");
                $("i", link).addClass("favorite-news-i");
                $(link).css("background-color", "#7DD7C4");
                $("div.ibox[data-news-id=" + $(link).attr("data-link-news-id") + "]").fadeToggle();
            }
        },
        "json"
    );
});

$("div.news-buttons > a:has(i.fa-remove)").click(function (e) {
    e.preventDefault();
    var link = this;
    $.get(link.href, function (data) {
        if (data.result === true) {
            $(link).css("background-color", "red");
            $("div.ibox[data-news-id=" + $(link).attr("data-link-news-id") + "]").fadeOut();
        }
    });
});

$("div.news-buttons > a:has(i.fa-clock-o)").click(function (e) {
    e.preventDefault();
    var link = this;
    $.get(this.href, function (data) {
            if (data.result === true) {
                $("i", link).css("background-color", "#7DD7C4").css("color", "#444");
                $("div.ibox[data-news-id=" + $(link).attr("data-link-news-id") + "]").fadeToggle();
            }
        },
        "json"
    );
});