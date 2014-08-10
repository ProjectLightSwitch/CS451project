
function TagAdapter()
{
    this._ajaxNavigateUrlUrl = "/Tags/Navigate";
}

TagAdapter.prototype.getCategories = function(callback)
{
    this.getChildTags(InvisibleRootId, callback);
}

TagAdapter.prototype.getChildTags = function(parentId, callback)
{
    $.ajax({
        cache: false,
        data: { 'id': parentId, 'childrenOnly': true },
        url: this._ajaxNavigateUrlUrl,
        type: "GET"
    }).success(function (response) {
        callback(response);
    });
}

// --------------

function TagSearcher(container)
{
    this.adapter = new TagAdapter();

    this._container = container;
    this._selectedTags = [];
    this._categoryBrowsers = {};
    this._selectionListeners = {};

    this._init();
}

// "PRIVATES"

// There should only be one instance anyway
TagSearcher.prototype._init = function () {
    this._container.children().remove();
    this.adapter.getCategories(function (response) {
        if (response.results.length == 0) {
            return;
        }

        response.results = response.results[0];
        var children = response.results.children;
        for (var i = 0; i < children.length; i++) {
            var labelInfo = this._getTagLabel(children[i], response.reqLangId != response.defLangId);
            this._container.append($('<h3>' + labelInfo.label + '</h3>').css('font-style', labelInfo.success ? 'normal' : 'italic'));

            var div = $('<div data-id="' + children[i].id + '"></div>').appendTo(this._container);

            // TODO: Lazy load category children 
            // (this way prevents graphical glitch when resizing during first expansion)
            this._categoryBrowsers[children[i].id] = new CategoryBrowser(div, this, children[i].id);
        }

        this._container.accordion({
            collapsible: true,
            heightStyle: "content",
            active: false
        });


        // Lazy load
        // ---------
        //this._container.accordion({
        //    beforeActivate: function (event, ui) {
        //        if (ui.newPanel.attr('loaded') != '1') {
        //            event.stopPropagation();
        //            event.preventDefault();
        //            ui.newPanel.attr('loaded', '1');
        //            new CategoryBrowser(ui.newPanel, this.adapter, parseInt(ui.newPanel.attr('data-id')));
        //        }
        //    }.bind(this),
        //    heightStyle: "content",
        //    active: false
        //});

    }.bind(this));
}

TagSearcher.prototype._onTagSelected = function (path)
{
    var result = {
        id: path.length > 0 ? path[path.length - 1].id : null,
        'path': path
    };

    for (var i = 0; i < this._selectionListeners["all"].length; i++) {
        this._selectionListeners["all"][i].tagSelected(result);
    }
    for (var i = 0; i < this._selectionListeners[path[0].id].length; i++) {
        this._selectionListeners[path[0].id][i].tagSelected(result);
    }
}

TagSearcher.prototype._onTagDeselected = function (catId, tagId)
{
    for (var i = 0; i < this._selectionListeners["all"].length; i++) {
        this._selectionListeners["all"][i].tagDeselected(tagId);
    }

    for (var i = 0; i < this._selectionListeners[catId].length; i++) {
        this._selectionListeners[catId][i].tagDeselected(tagId);
    }
}

TagSearcher.prototype._getTagLabel = function(tagInfo, translationAttempted) {
    var transSuccess = true;
    var label = translationAttempted ? tagInfo.text : tagInfo.eng;

    if (label == null) {
        label = tagInfo.eng;
        transSuccess = false;
    }
    return { 'success': transSuccess, 'label': label };
}

// PUBLIC

TagSearcher.prototype.getSelectedTagPaths = function () {
    return this._selectedTags;
}

/*
    path: [{id, label},{id, label},...]
*/
TagSearcher.prototype.selectTag = function (path)
{
    if (!(path instanceof Array) || path.length == 0) {
        return;
    }

    // Check for duplicates
    var endTag = path[path.length - 1];
    for (var i = 0; i < this._selectedTags.length; i++) {
        if (endTag.id == this._selectedTags[i][this._selectedTags[i].length - 1].id) {
            return
        }
    }

    this._selectedTags.push(path);

    this._onTagSelected(path);
}

/*
    path: [{id, label},{id, label},...]
*/
TagSearcher.prototype.deselectTag = function (tagId)
{
    for (var i = 0; i < this._selectedTags.length; i++)
    {
        var path = this._selectedTags[i];
        
        if (path[path.length - 1].id == tagId)
        {
            var catId = path[0].id;
            this._selectedTags.splice(i, 1);
            
            // clear checkbox
            var categoryBrowser = this._categoryBrowsers[path[0].id];
            if (typeof(categoryBrowser.tagSelectionToggled) == 'function') {
                categoryBrowser.tagSelectionToggled(tagId, false);
            }

            this._onTagDeselected(catId, tagId);
        }
    }
}

// subscriptionFor: The category tag ID, from which to receive notifications or "all" to receive updates for all categories
// (Don't worry about removal for now)
TagSearcher.prototype.addListener = function (handler, subscriptionFor)
{
    if (typeof (subscriptionFor) == 'undefined' || subscriptionFor == null) {
        subscriptionFor = 'all';
    }

    if (handler instanceof Object) {
        if (typeof(this._selectionListeners[subscriptionFor]) == 'undefined') {
            this._selectionListeners[subscriptionFor] = [ handler ];
        } else {
            this._selectionListeners[subscriptionFor].push = handler;
        }
    }
}

// ------------------------------

function CategoryBrowser(container, searcher, categoryId) {
    this._path = [];
    this._selectedTagIds = {};
    this._container = container;
    this._searcher = searcher;
    this._categoryId = categoryId;

    this._searcher.addListener(this, categoryId);

    this._loadChildren(categoryId, true);
}

CategoryBrowser.prototype._back = function (e) {
    if (this._path.length <= 1) {
        return;
    }
    this._path.pop();
    this._loadChildren(this._path[this._path.length - 1].id, true);
    e.preventDefault();
}

// Listeners for TagSearcher
CategoryBrowser.prototype.tagSelected = function (results) {
    this._selectedTagIds[results.id] = true;
    var cb = this._container.find('[data-id="' + results.id + '"] input[type="checkbox"]');
    cb.prop('checked', true);
}

CategoryBrowser.prototype.tagDeselected = function (tagId) {
    delete this._selectedTagIds[tagId];
    var cb = this._container.find('[data-id="' + tagId + '"] input[type="checkbox"]');
    cb.prop('checked', false);
}
// END Listeners

// Tag selection changed because user (un)checked a tag
CategoryBrowser.prototype._tagCheckStateChanged = function (evt) {
    if (this.checked) {
        var fullPath = evt.data.context._path.slice(0);
        fullPath.push({ label: evt.data.label, id: evt.data.id });
        evt.data.context._searcher.selectTag(fullPath);
    } else {
        evt.data.context._searcher.deselectTag(evt.data.id);
    }
}

CategoryBrowser.prototype._loadChildren = function (tagId, overrideDuplicationCheck)
{
    // Don't add path if it was set before coming here
    if (this._path.length == 0 || this._path[this._path.length - 1].id != tagId) {
        // Label gets loaded after data is returned
        this._path.push({ 'id': tagId, 'label': '' });
    }

    this._searcher.adapter.getChildTags(tagId, function (response) {
        this._container.children().remove();

        // Update label in path
        var translationAttempted = response.reqLangId != response.defLangId;
        var parentLabelInfo = this._searcher._getTagLabel(response.results[0].parent, translationAttempted);
        this._path[this._path.length - 1].label = parentLabelInfo.label;

        var encodedLabel = $('<div>').html(parentLabelInfo.label).text();

        if (tagId != this._categoryId) {
            $('<div>').addClass('search_item').addClass('search_back').on('click', this._back.bind(this)).append(
                $('<a href="#" style="text-decoration:none">&lt; Back</a>')
            ).append(
                $('<span class="float-right b">' + encodedLabel + '</span>')
            ).appendTo(this._container);
        }

        if (response.results.length == 0 || response.results[0].children.length == 0) {
            $('<div class="search_item"><i>No Results Found</i></div>').appendTo(this._container);
            return;
        }

        var children = response.results[0].children;
        for (var i = 0; i < children.length; i++) {
            var labelInfo = this._searcher._getTagLabel(children[i]);

            var div = $('<div>').addClass('search_item').attr('data-id', children[i].id).appendTo(this._container);

            if (children[i].type == TagType.NavigationalTag) {
                div.text(labelInfo.label).addClass('search_navTag');
                div.on('click', this._loadChildren.bind(this, children[i].id, false));
                div.append($('<span class="decorator">&gt;</span>'));
            }
            else if (children[i].type == TagType.SelectableTag) {
                div.addClass('search_selTag');
                $('<label>').text(labelInfo.label).append(
                    $('<input>').addClass('decorator').attr({ 'type': 'checkbox', 'checked': this._selectedTagIds[children[i].id] == true }).on(
                        'change', { context: this, id: children[i].id, label: labelInfo.label }, this._tagCheckStateChanged
                )).appendTo(div);
            }
        }
    }.bind(this));
}

// --------------

function SelectedTags(container, tagClosedCallback)
{
    this._container = container;

    /* function(tagId) */
    this._tagClosedCallback = tagClosedCallback;
}

// Listeners for TagSearcher
SelectedTags.prototype.tagSelected = function(results)
{
    this.addTag(results.path);
}

SelectedTags.prototype.tagDeselected = function(tagId)
{
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

// --------------
function TagNavigator(container, options) {
    if (typeof container == 'string') {
        container = $('#' + container);
    }
    if (!container || !container.length) {
        throw "Tag navigation container not found.";
    }
    this._container = container;
    this.selectedEndTags = {};
    this.hierarchicalTagContainer = null;
    this.selectedBreadcrumbsContainer = null;
    this.selectedTagId = null;

    // Set defaults
    this.options = {
        selectedTagSubmissionName: "SelectedTags[(0)]",
        ajax_navigateToTagUrl: "/Tags/Navigate",
        ajax_searchUrl: "/Tags/Search",
        enabled: true,
        editUrl: null,
        pathSeparator: ' > '
    };
    if (options != null) {
        //var providedOptionKeys = Object.keys(options);
        for (var property in options) {
            if (options.hasOwnProperty(property)) {
                this.options[property] = options[property];
            }
        }
    }

    // PRIVATES
    this._cssContainerClassName = "TagNavigator";
    this._hiddenInputContainerName = "selTagInputs";
    this._searchTextClassName = "tagSearch";
    this._hierarchicalContainerDivClassName = "tags";
    this._selectedBreadcrumbsUlClassName = "selectedTags";
    this._selectedTagClassName = "sel";
    this._tagIdAttr = "data-tagid";
    this._depthAttr = "data-depth";
    this._parentIdAttr = "data-parentId";
    this._parentNameAttr = "data-parentName";

    this.init();
}

TagNavigator.prototype.init = function () {
    this._container.children().remove();
    this._container.addClass(this._cssContainerClassName);

    this.hierarchicalTagContainer = $('<div>').addClass(this._hierarchicalContainerDivClassName);
    this._container.append(
        this.hierarchicalTagContainer).append(
        $('<br>').css('clear', 'left'));
    
    if (this.options.enabled) {
        var autocomplete = $('<input>').addClass(this._searchTextClassName);
        this.wireAutocomplete(autocomplete);
        this._container.append(autocomplete);

        this.selectedBreadcrumbsContainer = $('<ul>').addClass(this._selectedBreadcrumbsUlClassName);
        this._container.append(this.selectedBreadcrumbsContainer);
    }
    // See: http://www.quirksmode.org/css/clearing.html for possible workaround
    this._container.append($('<br>').css('clear', 'left'));
        
    // Hook into form submission
    var tagnav = this;
    $(document).ready(function () {
        tagnav.container.closest('form').submit(function () { tagnav.injectHiddenFields(); });
    });

    // StartNavigation
    this.navigateToTag(this.findTagId());
};

TagNavigator.prototype.findTagId = function () {
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


// This assumes the tagnav is placed inside a form
TagNavigator.prototype.injectHiddenFields = function () {
    var tagnav = this;
    var tagIds = Object.keys(this.selectedEndTags);
    $.each(tagIds, function (index, result) {
        $('<input>').attr({
            'name': tagnav.options.selectedTagSubmissionName.replace('(0)', index),
            'value': result,
            'type': 'hidden'
        }).appendTo(tagnav.container);
    });
}

TagNavigator.prototype.getHierarchicalListItemName = function (tagId) {
    return "tagnav_tag_" + tagId;
}

TagNavigator.prototype.findByName = function (name, parent) {
    var selector = '[name="' + name + '"]';
    return (!parent || !parent.length)
        ? this._container.find(selector)
        : parent.find(selector);
}

TagNavigator.prototype.endTagSelected = function (tagId, status) {
    var container = this.hierarchicalTagContainer.find('[' + this._tagIdAttr + '="' + tagId + '"]');

    depth = parseInt(container.attr(this._depthAttr));
    name = container.find("label").text();

    if (status) {
        // Add to internal list of selected tags
        this.selectedEndTags[tagId] = true;

        // Create breadcrumb
        var path = [];
        var navItems = this.hierarchicalTagContainer.children('ul').slice(1, depth + 1);
        var pna = this._parentNameAttr;
        navItems.each(function (index) {
            path.push($(this).attr(pna));
        });
        path.push(name);

        // Add breadcrumb
        tagnav = this;
        $('<li />').attr(this._tagIdAttr, tagId).text(path.join(this.options.pathSeparator)).append($('<a>').attr('href', '#').click(function () {
            if (tagnav.options.enabled) {
                tagnav.endTagSelected(tagId, false);
            }
            return false;
        }).text('X')).appendTo(this.selectedBreadcrumbsContainer);
    }
    else {

        // Delete from internal list of selected tags
        delete this.selectedEndTags[tagId];

        // Delete list item in breadcrumbs
        this.selectedBreadcrumbsContainer.find('[' + this._tagIdAttr + '="' + tagId + '"]').remove();

        //Uncheck box
        var cb = this.hierarchicalTagContainer.find('[' + this._tagIdAttr + '="' + tagId + '"] input[type="checkbox"]');
        cb.attr("checked", false);
    }
}

TagNavigator.prototype.navigateToTag = function (id) {
    var tagnav = this;
    $.ajax({
        cache: false,
        data: { 'id': id, 'childrenOnly': false },
        url: tagnav.options.ajax_navigateToTagUrl,
        type: "GET"
    }).success(function (response) {
        tagnav.selectedTagId = id;
        $.each(response.results, function (index, result) {
            tagnav.displayNavigationLevel(result, index, response.defLangId != response.reqLangId);
        })
    });
}

TagNavigator.prototype.tagSelected = function (parentId, name, type, depth) {
    var tagnav = this;

    $.ajax({
        cache: false,
        data: { 'id': parentId, 'childrenOnly': true },
        url: tagnav.options.ajax_navigateToTagUrl,
        type: "GET"
    }).success(function (response) {

        var data = null;
        if (response.results.length == 1) {
            data = response.results[0];
        }

        tagnav.selectedTagId = parentId;
        tagnav.displayNavigationLevel(data, depth + 1, response.defLangId != response.reqLangId);
    });
    return false;
}

TagNavigator.prototype.displayNavigationLevel = function (data, depth, isTranslated) {
    // Remove levels to right
    this.hierarchicalTagContainer.children().slice(depth).remove();

    if (data == null || data.length == 0) {
        return;
    }

    //this.hierarchicalTagContainer.find("ul:eq(" + depth + "),ul:eq(" + depth + ") ~ ul").remove();
    // Create new unordered list
    var ul = $('<ul>').attr(this._depthAttr, depth).attr(this._parentIdAttr, data.parent.id).attr(this._parentNameAttr, data.parent.text == null ? data.parent.eng : data.parent.text);
    var tagnav = this;

    var missingTranslation;
    var label;
    $.each(data.children, function (index, child) {
        missingTranslation = false;

        // Try to retrieve a translated version
        label = child.text;

        // Fall back on English if translated text isn't available
        if (label === null) {
            if (isTranslated) {
                missingTranslation = true;
            }
            label = child.eng;
        }

        var li = $('<li>').attr(tagnav._depthAttr, depth).attr(tagnav._tagIdAttr, child.id).addClass('tagnav_tag');;
        if (missingTranslation) {
            li.addClass("noTrans");
        }

        if (child.id == tagnav.selectedTagId) {
            li.addClass(tagnav._selectedTagClassName);
        }

        if (tagnav.options.enabled && tagnav.options.editUrl != null) {
            li.append('<span class="editSpan">(<a class="editTag" href="' + tagnav.options.editUrl.replace('(0)', child.id) + '">Edit</a>)</span>');
        }

        if (child.type == TagType.NavigationalTag || child.type == TagType.Category) {
            li.addClass(child.type == TagType.Category ? 'tagnav_cat' : 'tagnav_nav').addClass('tagnav_tag');
            li.append(
                tagnav.options.enabled 
                ? $('<a>').attr('href', '#').click(function () {
                    tagnav.tagSelected(child.id, label, child.type, depth);
                    return false;
                }).text(label)
                : $('<span>').text(label));
        }
        else if (child.type == TagType.SelectableTag) {
            var isChecked = tagnav.selectedEndTags[child.id] === true;
            li.addClass('tagnav_sel');
            li.append(
                tagnav.options.enabled 
                ? $('<label>').attr({ 'for': 'tagnav_' + child.id }).text(label).after(
                    $('<input>').click(function () {
                        tagnav.endTagSelected(child.id, $(this)[0].checked);
                    }
                    ).attr({ 'name': 'tagnav_' + child.id, 'type': 'checkbox', 'checked': isChecked })
                )
                : $('<span>').text(label))
        }
        li.appendTo(ul);
    });

    // Ensure only parent is selected
    var parentElement = this.hierarchicalTagContainer.children().last().find('[' + tagnav._tagIdAttr + '=' + data.parent.id + ']');
    parentElement.addClass(this._selectedTagClassName);
    parentElement.siblings().removeClass(this._selectedTagClassName);



    ul.appendTo(tagnav.hierarchicalTagContainer).fadeIn('fast');
}

TagNavigator.prototype.wireAutocomplete = function (textbox)
{
    var tagnav = this;
    $(function () {
        textbox.autocomplete({
            source: function (request, response) {
                $.ajax(tagnav.options.ajax_searchUrl, 
                {
                    cache: tagnav.options.editUrl == null,
                    data: { term: request.term },
                    success: function (data)
                    {
                        $.each(data, function(idx, item){
                            data[idx].label = item.label.join(tagnav.options.pathSeparator);
                        });
                        data.sort(function (a, b) { return a.label.localeCompare(b.label); });
                        response(data)
                    }
                })
            },
            minLength: 3,
            select: function (event, ui) {
                event.preventDefault();
                textbox.val(ui.item.label);
                tagnav.navigateToTag(ui.item.value);
            },

            //html: true, // optional (jquery.ui.autocomplete.html.js required)
            focus: function (event, ui) {
                event.preventDefault();
            },
            open: function (event, ui) {
                $(".ui-autocomplete").css("z-index", 1000);
            }
        });
    });

    //textbox.autocomplete({
    //    source: function (request, response) {
    //        // TODO: replace url
    //        $.ajax({
    //            dataType: "json",
    //            type: 'Get',
    //            url: '/Tags/Search',

    //            success: function (data) {
    //                textbox.removeClass('ui-autocomplete-loading');  // hide loading image

    //                response($.map(data, function (item) {
    //                    // your operation on data
    //                }));
    //            },
    //            error: function (data) {
    //                textbox.removeClass('ui-autocomplete-loading');
    //            }
    //        });
    //    },
    //    minLength: 3,
    //    open: function () {

    //    },
    //    close: function () {

    //    },
    //    focus: function (event, ui) {

    //    },
    //    select: function (event, ui) {

    //    }
    //});
}

