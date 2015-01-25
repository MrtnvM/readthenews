$("div.news-buttons > a:has(i.fa-heart-o)").click(function (e) {
    e.preventDefault();
    var link = this;
    $.get(this.href, function (data) {
        console.log(data);
            if (data.result === true) {
                $("i", link).removeClass("fa-heart-o").addClass("fa-heart");
                $("i", link).css("color", "rgb(249, 58, 116)");
                $(link).css("background-color", "#7DD7C4");
            }
        },
        "json"
    );
    
    console.log("works!");
});