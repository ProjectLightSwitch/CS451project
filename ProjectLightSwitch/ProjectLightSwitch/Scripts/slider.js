$(document).ready(function (){

    $(".nextLink").on("click",function(e){

        var currentActiveImage = $(".image-shown");
        var nextActiveImage = currentActiveImage.next();

        if (nextActiveImage.length == 0)
        {
            nextActiveImage = $(".slider_img img").first();
        }

        currentActiveImage.removeClass("image-shown").addClass("image-hidden").css("z-index", -10);
        nextActiveImage.addClass("image-shown").removeClass("image-hidden").css("z-index", 20);
        $(".slider_img img").not([currentActiveImage, nextActiveImage]).css("z-index", 1);
        
        e.preventDefault()
    });

    $(".previousLink").on("click",function(e){

        var currentActiveImage = $(".image-shown");
        var nextActiveImage = currentActiveImage.prev();

        if(nextActiveImage.length == 0)
        {
            nextActiveImage = $(".slider_img img").last();
        }

        currentActiveImage.removeClass("image-shown").addClass("image-hidden").css("z-index", -10);
        nextActiveImage.addClass("image-shown").removeClass("image-hidden").css("z-index", 20);
        $(".slider_img img").not([currentActiveImage, nextActiveImage]).css("z-index", 1);

        e.preventDefault;
    });
});