
function TagSearcher(container) {
    if (!container || !container.length) {
        throw "Container not found.";
    }

    this.tagAdapter = new TagAdapter();
    this.tagSelector = new TagSelector();

    // JSONTagModel[]
    //this.fullPaths = {};
    //this._childBrowsers = {};

    /* {uniqueId:{browser:(TagChildrenNavigator), path:(JSONTagModel[])}  */
    this._categoryBrowsers = {};

    this._container = container;
    this._tagDepthContainer = null;

    // "PRIVATES"
    this._containerClassName = "tagsearch";
    this._tagContainerClassName = "tags";
    this._tagDepthContainerClassName = "depth_container"

    // Attributes
    this._tagIdAttr = "data-tagid";
    this._depthAttr = "data-depth";
    this._parentIdAttr = "data-parentId";
    this._parentNameAttr = "data-parentName";

    this.init();
}

TagSearcher.prototype.init = function () {
    // The overall container
    this._container.children().remove();
    this._container.addClass(this._containerClassName);
    

    // Load all category children for now
    // Future work: implement lazy loading by category for performance
    this.tagAdapter.getDescendantTags(InvisibleRootId, true, function (response)
    {
        if (response == null || response.results == null || response.results.length == 0)
        {
            return null;
        }

        var children = response.results[0].children;
        var len = children.length;
        for(var i = 0; i < len; i++)
        {
            var catHeader = $('<h3>').text(children[i].text).appendTo(this._container);
            $('<div>').addClass(this._tagDepthContainerClassName).attr('id', 'searchCat_' + children[i].id).appendTo(this._container);

            this.tagAdapter.getDescendantTags(children[i].id, true, function (categoryResponse)
            {
                var catChildren = categoryResponse.results[0];
                var catNav = new TagChildrenNavigator(
                    $('#searchCat_' + catChildren.parent.id),
                    this,
                    catChildren,
                    catChildren.parent.id,
                    {
                        enabled: true,
                        isSelfNavigating: true,
                        cssPrefix: 'search'
                    });
                this.tagSelector.addListener(catNav, catChildren.parent.id);

                var currentPath = [];
                for(var j = 0; j < categoryResponse.results.length; j++) {
                    currentPath.push(categoryResponse.results[j].parent);
                }
                this._categoryBrowsers[catChildren.parent.id] = { 'path': currentPath, 'browser': catNav };
            }.bind(this));
        }

        this._container.accordion({ heightStyle: "content", collapsible: true, active: false });
    }.bind(this));

    // Hook into form submission
    $(document).ready(function () {
        this._container.closest('form').submit(this._injectHiddenFields.bind(this));
    }.bind(this));
};

// Searches URL anchor, id querystring value and navigates to child
// returns root children if not found
TagSearcher.prototype.findTagId = function () {
    var tagId = getUrlHash();
    if (tagId == null)
    {
        tagId = getUrlVars()['id'];
        if (tagId == null)
        {
            tagId = InvisibleRootId;

            var list = getUrlPath();
            if (list.length > 1) {
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
TagSearcher.prototype.selectTag = function (path) {
    if (!(path instanceof Array) || path.length == 0) {
        return;
    }
    this.tagSelector.onTagSelected(path);
}

/* path: [{id, label},{id, label},...] */
TagSearcher.prototype.deselectTag = function (tagId) {
    this.tagSelector.onTagDeselected(tagId);
}

// This assumes the tagnav is placed inside a form
TagSearcher.prototype._injectHiddenFields = function () {
    var selectedPaths = this.tagSelector.getSelectedPaths();
    for (var i = 0; i < selectedPaths.length; i++) {
        var tagId = selectedPaths[i][selectedPaths[i].length - 1].id;
        $('<input>').attr({
            'name': TagOptions.selectedTagSubmissionName.replace('%s', i),
            'value': tagId,
            'type': 'hidden'
        }).appendTo(this._container);
    }
}

// INTERFACE METHODS

TagSearcher.prototype.selectTag = function (parentId, tagInfo)
{
    var tempPath = null;
    for (var key in this._categoryBrowsers)
    {
        var path = this._categoryBrowsers[key].path;
        if (path[path.length - 1].id == parentId) {
            tempPath = path.slice(0);
            tempPath.push(tagInfo);
        }
    }

    this.tagSelector.onTagSelected(tempPath);
}

TagSearcher.prototype.deselectTag = function (tagId) {
    this.tagSelector.onTagDeselected(tagId);
}

TagSearcher.prototype.isTagSelected = function (tagId) {
    return this.tagSelector.isTagSelected(tagId);
}

TagSearcher.prototype.navigateToParent = function (id)
{
    for (var key in this._categoryBrowsers)
    {
        var path = this._categoryBrowsers[key].path;
        if (path.length - 2 >= 0 && path[path.length - 1].id == id)
        {
            this.navigateToTag(path[path.length - 2].id);
        }
    }
}

TagSearcher.prototype.navigateToTag = function (id)
{
    this.tagAdapter.getDescendantTags(id, false, function (response)
    {
        var results = response.results;

        var path = [];
        for (var i = 0; i < results.length; i++)
        {
            path.push(results[i].parent);
        }
        var leaves = results[results.length - 1];
        var catId = results[1].parent.id;

        // Housecleaning on old browser
        var oldBrowser = this._categoryBrowsers[catId].browser;
        var container = oldBrowser.container;
        oldBrowser.container = null;
        this.tagSelector.removeListener(oldBrowser, catId);

        // Recycle the container
        var browser = new TagChildrenNavigator(container, this, leaves, catId, { isSelfNavigating: true, cssPrefix: 'search', enabled: true })
        this._categoryBrowsers[catId] = { 'path':path, 'browser': browser };
    }.bind(this));
}

// END INTERFACE METHODS







