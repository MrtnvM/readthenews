$("div.news-buttons > a:has(i.fa-heart-o)").click(function (e) {
    e.preventDefault();
    var link = this;
    $.get(this.href, function (data) {
            console.log(data);
            if (data.result === true) {
                $("i", link).removeClass("fa-heart-o").addClass("fa-heart");
                $("i", link).addClass("favorite-news-i");
            }
        },
        "json"
    );
    
    console.log("works!");
});

$("div.news-buttons > a:has(i.fa-remove)").click(function (e) {
    e.preventDefault();
    var link = this;
    $.get(link.href, function (data) {
        console.log(data);
        if (data.result === true) {
            $()
        }
    });
});