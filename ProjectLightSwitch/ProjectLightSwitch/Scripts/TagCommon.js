




// ------------------------------------------------------------------------------
// -------------------------------- TagOptions ----------------------------------
// ------------------------------------------------------------------------------
 
var TagOptions =
{
    selectedTagSubmissionName: "SelectedTags[%s]",
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
}

TagAdapter.prototype.getCategories = function (callback) {
    this.getChildTags(InvisibleRootId, true, callback);
}

TagAdapter.prototype.getDescendantTags = function (parentId, childrenOnly, callback) {
    $.ajax({
        cache: false,
        data: { 'id': parentId, 'childrenOnly': childrenOnly },
        url: TagOptions.ajax_navigateToTagUrl,
        type: "GET"
    }).success(function (response) {
        callback(response);
    });
}

TagAdapter.prototype.searchTags = function (searchTerm, callback) {
    $.ajax({
        cache: false,
        data: { 'term': searchTerm },
        url: TagOptions.ajax_searchUrl,
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
    // selected tags stored in order of selection
    // JSONTagModel[][] ([{id,type,text},...])
    this._selectedPaths = [];
    this._selectionListeners = {};
}

TagSelector.prototype.getSelectedPaths = function ()
{
    return this._selectedPaths;
}

TagSelector.prototype.isTagSelected = function(tagId)
{
    for (var i = 0; i < this._selectedPaths.length; i++) {
        var path = this._selectedPaths[i];
        if (path[path.length - 1].id == tagId) {
            return true;
        }
    }
    return false;
}

// (JSONTagModel)tagInfo = The path  ({id,type,text})
TagSelector.prototype.onTagSelected = function (path)
{

    // no duplicates
    if (this.isTagSelected(path[path.length - 1].id)) {
        return;
    }

    this._selectedPaths.push(path);

    for (var i = 0; i < this._selectionListeners['all'].length; i++) {
        this._selectionListeners['all'][i].tagSelected(path);
    }

    if (typeof (this._selectionListeners[path[0].id]) != 'undefined') {
        for (var i = 0; i < this._selectionListeners[path[0].id].length; i++) {
            this._selectionListeners[path[0].id][i].tagSelected(path);
        }
    }
}

// (int)tagId
TagSelector.prototype.onTagDeselected = function (tagId)
{
    //find and remove the selected tag
    var catId;
    for (var i = 0; i < this._selectedPaths.length; i++) {
        var path = this._selectedPaths[i];
        if (path[path.length - 1].id == tagId) {
            catId = path[0].id;
            this._selectedPaths.splice(i, 1);
            break;
        }
    }

    for (var i = 0; i < this._selectionListeners["all"].length; i++) {
        this._selectionListeners["all"][i].tagDeselected(tagId);
    }

    if (typeof (this._selectionListeners[catId]) != 'undefined') {
        for (var i = 0; i < this._selectionListeners[catId].length; i++) {
            this._selectionListeners[catId][i].tagDeselected(tagId);
        }
    }
}

// Listeners must implement the following:
// -- function tagDeselected((int)id) { }
// -- function tagSelected({ 'id':(int), 'path':(Tag[]) })

// subscriptionFor: The category tag ID, from which to receive notifications or "all" to receive updates for all categories
// (Don't worry about removal for now)
TagSelector.prototype.addListener = function (handler, subscriptionFor)
{
    if (typeof (subscriptionFor) == 'undefined' || subscriptionFor == null) {
        subscriptionFor = 'all';
    }

    if (handler instanceof Object) {
        if (typeof (this._selectionListeners[subscriptionFor]) == 'undefined') {
            this._selectionListeners[subscriptionFor] = [handler];
        } else {
            this._selectionListeners[subscriptionFor].push(handler);
        }
    }
}

TagSelector.prototype.removeListener = function (handler, subscriptionFor) 
{
    var subscriptionList = this._selectionListeners[subscriptionFor];
    for(var i = 0; i < subscriptionList.length; i++)
    {
        if(subscriptionList[i] == handler)
        {
            this._selectionListeners[subscriptionFor].splice(i,1);
            // Assume each object only subscribed once
            return
        }
    }
}

// -----------------------------------------------------------------------------------
// -------------------------- CATEGORY BROWSER ---------------------------------------
// -----------------------------------------------------------------------------------

// Example settings
var TagChildrenNavigatorDefaultOptions = {
    isSelfNavigating: true,
    cssPrefix: 'search',
    selNavTagId: null
};

function TagChildrenNavigator(
    container /*DOM element to place the data*/,
    navigator /*Parent*/,
    tagData /*Tag parent and list of children*/,
    identifier,
    options /*TagChildrenNavigatorOptions*/)
{
    this.identifier = identifier;
    this.parentId = tagData.parent.id;

    // Expect a div, add a ul as a child
    container.children().remove();
    this.container = container;
    this._navigator = navigator;

    // Set option defaults
    this._cssPrefix = options.cssPrefix || 'TagChildrenNavigator_';
    this._isSelfNavigating = options.isSelfNavigating || false;
    this._selNavTagId = options.selNavTagId || null;
    
    this._loadTags(tagData);
}

TagChildrenNavigator.prototype.navigateBack = function (e)
{
    this._navigator.navigateToParent(this._parentId);
    e.preventDefault();
}

// Listeners for TagSearcher
TagChildrenNavigator.prototype.tagSelected = function (path) {
    var endTag = path[path.length - 1];
    var cb = this.container.find('[data-id="' + endTag.id + '"] input[type="checkbox"]').prop('checked', true);
}

TagChildrenNavigator.prototype.tagDeselected = function (tagId) {
    var cb = this.container.find('[data-id="' + tagId + '"] input[type="checkbox"]').prop('checked', false);
}
// END Listeners

// Tag selection changed because user (un)checked a tag
TagChildrenNavigator.prototype._tagCheckStateChanged = function (evt)
{
    if (this.checked) {
        evt.data.context._navigator.selectTag(
            evt.data.context.parentId,
            evt.data.tagInfo);
    } else {
        evt.data.context._navigator.deselectTag(
            evt.data.tagInfo.id);
    }
}

TagChildrenNavigator.prototype._loadTags = function (childrenInfo)
{
    this.container.children().remove();

    var ul = $('<ul>').appendTo(this.container);
    // Update label in path
    if (this._isSelfNavigating) {
        var innerNav = $('<li>').addClass(this._cssPrefix + 'item').addClass(this._cssPrefix + 'back').appendTo(ul);
        if (childrenInfo.parent.type != TagType.Category)
        {
            innerNav.click(function (id, e) { 
                this.navigateToParent(id);
                e.preventDefault();
            }.bind(this._navigator, childrenInfo.parent.id)
                
            ).append(
                $('<a href="#" style="text-decoration:none">&lt; Back</a>'));
        }
        innerNav.append(
            $('<span>').addClass('float-right').addClass('b').text(childrenInfo.parent.text)
        );
    }
    
    if (childrenInfo == null || childrenInfo.children.length == 0) {
        $('<li class="' + this._cssPrefix + 'item no_results"><i>No Results Found</i></li>').appendTo(ul);
        return;
    }

    var children = childrenInfo.children;
    var childrenLen = childrenInfo.children.length;
    for (var i = 0; i < childrenLen; i++)
    {
        var tagContainer = $('<li>').addClass(this._cssPrefix + 'tag').attr('data-id', children[i].id).appendTo(ul);

        if (this._selNavTagId == children[i].id)
        {
            tagContainer.addClass('sel');
        }

        if (children[i].type == TagType.Category) {
            tagContainer.text(children[i].text).addClass(this._cssPrefix + 'catTag');
            tagContainer.on('click', this._navigator.navigateToTag.bind(this._navigator, children[i].id));
            tagContainer.append($('<span class="decorator">&gt;</span>'));
        }
        else if (children[i].type == TagType.NavigationalTag) {
            tagContainer.text(children[i].text).addClass(this._cssPrefix + 'navTag');
            tagContainer.on('click', this._navigator.navigateToTag.bind(this._navigator, children[i].id));
            tagContainer.append($('<span class="decorator">&gt;</span>'));
        }
        else if (children[i].type == TagType.SelectableTag) {
            tagContainer.addClass(this._cssPrefix + 'selTag');
            var isChecked = this._navigator.isTagSelected(children[i].id);
            $('<label>').text(children[i].text).append(
                $('<input>').addClass('decorator').attr({ 'type': 'checkbox'}).prop('checked', isChecked).on(
                    'change', { context: this, tagInfo: children[i] }, this._tagCheckStateChanged
            )).appendTo(tagContainer);
        }
    }
}

// -----------------------------------------------------------------------------------
// -------------------------- SELECTED TAG BREADCRUMBS -------------------------------
// -----------------------------------------------------------------------------------

var SelectedTagBreadCrumbsOptions =
{
    cssPrefix : '',
    separator : ' > ',
    closable : true
}

function SelectedTagBreadCrumbs(container, navigator, options)
{
    this._tagSelector = navigator.tagSelector;
    this._navigator = navigator;
    
    this._cssPrefix = options.cssPrefix || '';
    this._closable = options.closable || false;
    this._separator = options.separator || ' > ';
    
    this.init(container);
}

SelectedTagBreadCrumbs.prototype.init = function(container)
{
    container.children().remove();
    this._container = $('<ul>');
    container.append(this._container);
    this._container.addClass(this._cssPrefix + "tabBreadcrumbs")
    this._tagSelector.addListener(this, 'all');
}

SelectedTagBreadCrumbs.prototype.tagSelected = function (path) 
{
    var title = [];
    for (var i = 1; i < path.length; i++) {
        title.push(path[i].text);
    }

    var label = [title[title.length - 2], title[title.length - 1]].join(this._separator);
    title = title.join(this._separator);

    var tagId = path[path.length - 1].id;

    var crumb = $('<li>').attr('data-id', tagId).addClass(this._cssPrefix + "tag").append(
            $('<a>').attr({ 'href': '#', 'title': title }).text(label)).appendTo(this._container);

    if (this._closable) {
        crumb.append($('<a>').attr('href','#').addClass('close').text('X').bind('click',
            function (id) {
                this._tagSelector.onTagDeselected(id);
                return false;
            }.bind(this, tagId)
        ));
    }
}

SelectedTagBreadCrumbs.prototype.tagDeselected = function (tagId) 
{
    this._container.find('[data-id="' + tagId + '"]').remove();
}