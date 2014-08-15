function getcssClassName(prefix, type) {
    if (type == "SelectableTag") {
        return prefix + 'sel';
    } else if (type == "NavigationalTag") {
        return prefix + 'nav';
    } else if (type == "Category") {
        return prefix + 'cat';
    } else if (type == "PendingSelectableTag") {
        return prefix + 'pend';
    }
    return null;
}


var TagChildrenViewerOptions =
{
    showBackButton: false,
    containerClass: 'tagDepthContainer',
    containerType: '<ul>',
    childClassPrefix:'tag_',
    childType: '<li>'
    
}

function TagChildrenViewer(options)
{
    this._listener


    this.options = TagChildrenViewerOptions;




}

// Tag data type = {(int)id,(string)text,(int/TagType)type}
// data = { parent:(Tag), children:[(Tag), (Tag), ...] }
function DisplayChildren(data)
{
    var container = $(this.options.containerType).addClass(this.options.containerClass);


    InvisibleRootId


    for (var i = 0; i < data.children.length; i++)
    {
        var tagClass = getcssClassName(this.options.childClassPrefix, data);
        
        //var classname = 
        $(this.options.childType).addClass(tagClass).text(data.children[i]).appendTo(container);


    }
    return container;
}


