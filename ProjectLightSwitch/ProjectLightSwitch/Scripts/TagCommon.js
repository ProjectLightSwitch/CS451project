




// ------------------------------------------------------------------------------
// -------------------------------- TagOptions ----------------------------------
// ------------------------------------------------------------------------------
 
var TagOptions =
{
    selectedTagSubmissionName: "SelectedTags[(0)]",
    ajax_navigateToTagUrl: "/Tags/Navigate",
    ajax_searchUrl: "/Tags/Search",
    enabled: true,
    editUrl: null,
    pathSeparator: ' > '
};

// ------------------------------------------------------------------------------
// -------------------------------- TagAdapter ----------------------------------
// ------------------------------------------------------------------------------

function TagAdapter()
{
    this._ajaxNavigateUrlUrl = "/Tags/Navigate";
    this._ajaxSearchUrlUrl = "/Tags/Search";
}

TagAdapter.prototype.getCategories = function (callback) {
    this.getChildTags(InvisibleRootId, true, allback);
}

TagAdapter.prototype.getChildTags = function (parentId, childrenOnly, callback) {
    $.ajax({
        cache: false,
        data: { 'id': parentId, 'childrenOnly': childrenOnly },
        url: this._ajaxNavigateUrlUrl,
        type: "GET"
    }).success(function (response) {
        callback(response);
    });
}

TagAdapter.prototype.searchTags = function (searchTerm, callback) {
    $.ajax({
        cache: false,
        data: { 'term': searchTerm },
        url: this._ajaxSearchUrlUrl,
        type: "GET"
    }).success(function (response) {
        callback(response);
    });
}

// ------------------------------------------------------------------------------ 
// -------------------------------- TagSelector --------------------------
// ------------------------------------------------------------------------------

function TagSelector()
{
    this._selectedTags = [];
    this._selectionListeners = {};
}

TagSelector.prototype.onTagSelected = function (path)
{
    var result = {
        id: path.length > 0 ? path[path.length - 1].id : null,
        'path': path
    };

    this._selectedTags.push(result);

    for (var i = 0; i < this._selectionListeners["all"].length; i++) {
        this._selectionListeners["all"][i].tagSelected(result);
    }
    for (var i = 0; i < this._selectionListeners[path[0].id].length; i++) {
        this._selectionListeners[path[0].id][i].tagSelected(result);
    }
}

TagSelector.prototype.onTagDeselected = function (tagId)
{
    //find and remove the selected tag
    var catId;
    for (var i = 0; i < this._selectedTags.length; i++) {
        if (this._selectedTags[i].id == tagId) {
            catId = this._selectedTags[i].path[0].id;
            this._selectedTags.splice(i, 1);
            break;
        }
    }

    for (var i = 0; i < this._selectionListeners["all"].length; i++) {
        this._selectionListeners["all"][i].tagDeselected(tagId);
    }

    for (var i = 0; i < this._selectionListeners[catId].length; i++) {
        this._selectionListeners[catId][i].tagDeselected(tagId);
    }
}


// Listeners must implement the following:
// -- function tagDeselected((int)id) { }
// -- function tagSelected({ 'id':(int), 'path':(Tag[]) })

// subscriptionFor: The category tag ID, from which to receive notifications or "all" to receive updates for all categories
// (Don't worry about removal for now)
TagSelector.prototype.addListener = function (handler, subscriptionFor) {
    if (typeof (subscriptionFor) == 'undefined' || subscriptionFor == null) {
        subscriptionFor = 'all';
    }

    if (handler instanceof Object) {
        if (typeof (this._selectionListeners[subscriptionFor]) == 'undefined') {
            this._selectionListeners[subscriptionFor] = [handler];
        } else {
            this._selectionListeners[subscriptionFor].push = handler;
        }
    }
}

// ------------------------------------------------------------------------------
// -------------------------------- SelectedTags --------------------------------
// ------------------------------------------------------------------------------

function SelectedTags(container, tagClosedCallback) {
    this._container = container;

    /* function(tagId) */
    this._tagClosedCallback = tagClosedCallback;
}

// Listeners for TagSearcher
SelectedTags.prototype.tagSelected = function (results) {
    this.addTag(results.path);
}

SelectedTags.prototype.tagDeselected = function (tagId) {
    this._container.find('[data-id="' + tagId + '"]').remove();
}
// END Listeners

SelectedTags.prototype.addTag = function (path)
{
    var label = [path[0].label];
    if (path.length > 3) {
        label.push("...");
    }
    // Path should always be > 1
    if (path.length > 2) {
        label.push(path[path.length - 2].label);
    }
    if (path.length > 1) {
        label.push(path[path.length - 1].label);
    }
    label = label.join(' > ');
    var id = path[path.length - 1].id;
    this._container.append(
        $('<li>').text(label + ' ').attr('data-id', id).append(
            $('<a>').css('color', 'red').text('X').click(function (tagId, evt) {
                this._tagClosedCallback(tagId),
                $(evt.target).parent().remove();
            }.bind(this, id))
        )
    );
}