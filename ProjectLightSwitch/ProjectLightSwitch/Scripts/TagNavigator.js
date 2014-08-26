function TagNavigator(containerSelector, options) {
    this.tagAdapter = new TagAdapter();
    this.tagSelector = new TagSelector();

    // JSONTagModel[]
    this.fullPath = null;
    
    // Override defaults
    for (option in options)
    {
        if (options.hasOwnProperty(option)) {
            TagOptions[option] = options[option];
        }
    }

    this._containerSelector = containerSelector;
    this._tagDepthContainer = null;
    this._childBrowsers = [];
    
    // "PRIVATES"
    options = options || {};
    this._enabled = getDefault(options.enabled, true);
    this._editUrlFormat = getDefault(options.editUrlFormat, null);
    this._containerClassName = "tagnav";
    this._tagContainerClassName = "tags";
    this._tagDepthContainerClassName = "depth_container"

    // Attributes
    this._tagIdAttr = "data-tagid";
    this._depthAttr = "data-depth";
    this._parentIdAttr = "data-parentId";
    this._parentNameAttr = "data-parentName";

    $(document).ready(this.init.bind(this));
}

TagNavigator.prototype.init = function ()
{
    this._container = $(this._containerSelector);

    // Container wasn't found
    if (!this._container.length) {
        return;
    }

    // The overall container
    this._container.children().remove();
    this._container.addClass(this._containerClassName);

    // Add the category container
    this._tagNavigationContainer = $('<div>').addClass(this._tagContainerClassName);
    this._container.append(
        this._tagNavigationContainer).append(
        $('<br>').css('clear', 'left'));
    
    // Is navigation to new tags allowed or just being displayed as information
    // Autocomplete now separate

    //if (this.options.enabled) {
    //    var autocomplete = $('<input>').addClass(this._searchTextClassName);
    //    this.wireAutocomplete(autocomplete);
    //    this._container.append(autocomplete);
    //}
    // See: http://www.quirksmode.org/css/clearing.html for possible workaround
    //this._container.append($('<br>').css('clear', 'left'));

    // Hook into form submission
    $(document).ready(function () {
        this._container.closest('form').submit(this._injectHiddenFields.bind(this));
    }.bind(this));

    this.navigateToTag(this.findTagId());
};

// Searches URL anchor, id querystring value and navigates to child
// returns root children if not found
TagNavigator.prototype.findTagId = function ()
{
    var tagId = getUrlHash();
    if (tagId == null) {
        tagId = getUrlVars()['id'];
        if (tagId == null) {
            tagId = InvisibleRootId;

            var list = getUrlPath();
            if (list.length > 1)
            {
                var num = parseInt(list[list.length - 1]);
                if (!isNaN(num) && num >= InvisibleRootId) {
                    tagId = num;
                }
            }
        }
    }
    return tagId;
}

/* path: [{id, type, text},{id, type, text},...] */
//TagNavigator.prototype.selectTag = function (path)
//{
//    if (!(path instanceof Array) || path.length == 0) {
//        return;
//    }

//    this.tagSelector.onTagSelected(path);
//}



// This assumes the tagnav is placed inside a form
TagNavigator.prototype._injectHiddenFields = function ()
{
    var selectedPaths = this.tagSelector.getSelectedPaths();
    for (var i = 0; i < selectedPaths.length; i++)
    {
        var tagId = selectedPaths[i][selectedPaths[i].length - 1].id;
        $('<input>').attr({
            'name': TagOptions.selectedTagSubmissionName.replace('%s', i),
            'value': tagId,
            'type': 'hidden'
        }).appendTo(this._container);
    }
}

// INTERFACE METHODS

TagNavigator.prototype.selectTag = function (parentId, tagInfo)
{
    var selectionPath = [];
    for (var i = 0; i < this.fullPath.length; i++) {
        selectionPath.push(this.fullPath[i]);

        // BUG
        if (this.fullPath[i].id == parentId) {
            break;
        }
    }
    selectionPath.push(tagInfo);
    this.tagSelector.onTagSelected(selectionPath);
}

TagNavigator.prototype.deselectTag = function (tagId) {
    this.tagSelector.onTagDeselected(tagId);
}

TagNavigator.prototype.isTagSelected = function (tagId)
{
    return this.tagSelector.isTagSelected(tagId);
}

TagNavigator.prototype.navigateToTag = function(id)
{

    // Remove previous child browsers as selection listeners and DOM elements
    var childBrowserLen = this._childBrowsers.length;
    for (var i = 0; i < childBrowserLen; i++) {
        this.tagSelector.removeListener(this._childBrowsers[i], 'all');
    }
    this._childBrowsers = [];

    this.tagAdapter.getDescendantTags(id, false, function (response) {
        this._tagNavigationContainer.children().remove();
        this.fullPath = [];

        var len = response.results.length;
        for (var i = 0; i < len; i++)
        {
            var result = response.results[i];
            this.fullPath.push(result.parent);
            var div = $('<div>').addClass(this._tagDepthContainerClassName).appendTo(this._tagNavigationContainer);
            var selNavTag = (i < len - 1) ? response.results[i + 1].parent.id : null;
            var childNav = new TagChildrenNavigator(div, this, result, null, { isSelNavigating: false, cssPrefix: 'search', selNavTagId: selNavTag, editUrlFormat: this._editUrlFormat, enabled: this._enabled });
            this.tagSelector.addListener(childNav, 'all');
        }
    }.bind(this));
}

// END INTERFACE METHODS







