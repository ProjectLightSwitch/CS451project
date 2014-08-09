
function TagAdapter()
{
    this._ajaxNavigateUrlUrl = "/Tags/Navigate";
}

TagAdapter.prototype.getCategories = function(callback, context)
{
    this.getChildTags(InvisibleRootId, callback, context);
}

TagAdapter.prototype.getChildTags = function(parentId, callback, context)
{
    $.ajax({
        cache: false,
        data: { 'id': parentId, 'childrenOnly': true },
        url: this._ajaxNavigateUrlUrl,
        type: "GET"
    }).success(function (response) {
        callback.call(context, response);
    });
}

// --------------

function TagSearcher(container, selectionChangedCallback)
{
    this.container = container;
    this.adapter = new TagAdapter();
    this.selectedTags = [];
    this.selectionChangedCallback = selectionChangedCallback;

    this.init();
}


/*
    path: [{id, label},{id, label},...]
*/
TagSearcher.prototype.addTagSelection = function(path)
{
    if(typeof path != 'array' || path.length == 0) {
        return;
    }

    var endTag = path[path.length - 1];
    for(var tag in this.selectedTags) {
        if(endTag.id == tag[tag.length - 1].id) {
            return
        }
    }
    this.selectedTags.push(path);
    if(typeof this.selectionChangedCallback != 'function') {
        this.selectionChangedCallback(this.selectedTags);
    }
}

/*
    path: [{id, label},{id, label},...]
*/
TagSearcher.prototype.removeTagSelection = function(tagId)
{
    for(var i = 0; i < this.selectedTags.length; i++) {
        if(this.selectedTags[i][tag.length - 1].id == tagId) {
            this.selectedTags.splice(i, 1);
            if(typeof this.selectionChangedCallback != 'function') {
                this.selectionChangedCallback(this.selectedTags);
            }
        }
    }
}

TagSearcher.prototype.init = function()
{
    this.container.children().remove();
    this.adapter.getCategories(function (response) {
        if (response.results.length == 0) {
            return;
        }

        response.results = response.results[0];
        var children = response.results.children;
        for (var i = 0; i < children.length; i++)
        {
            var labelInfo = this.getTagLabel(children[i], response.reqLangId != response.defLangId);
            this.container.append($('<h3>' + labelInfo.label + '</h3>').css('font-style', labelInfo.success ? 'normal' : 'italic'));

            var div = $('<div data-id="' + children[i].id + '"></div>');
            this.container.append(div);

            // TODO: Lazy load category children 
            // (this way prevents graphical glitch when resizing during first expansion)
            new CategoryBrowser(div, this, children[i].id);
        }

        this.container.accordion({
            collapsible: true,
            heightStyle: "content",
            active: false
        });


        // Lazy load
        // ---------
        //this.container.accordion({
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

    }, this);
}

TagSearcher.prototype.getTagLabel = function(tagInfo, translationAttempted)
{
    var transSuccess = true;
    var label = translationAttempted ? tagInfo.text : tagInfo.eng;

    if (label == null) {
        label = tagInfo.eng;
        transSuccess = false;
    }
    return {'success':transSuccess, 'label':label};
}

// ------------------------------

function CategoryBrowser(container, searcher, categoryId)
{
    this.path = [];
    this.selectedTagIds = {};
    this.container = container;
    this.searcher = searcher;
    this.categoryId = categoryId;

    this.navigateToTag(categoryId);
}

CategoryBrowser.prototype.back = function(e)
{
    if(this.path.length <= 1) {
        return;
    }
    this.path.pop();
    this.loadChildren(this.path[this.path.length - 1].id);
    e.preventDefault();
}

CategoryBrowser.prototype.tagSelectionChanged = function (evt)
{
    if (this.checked) {
        var fullPath = evt.data.context.path.slice(0);
        fullPath.push({ label: evt.data.label, id: evt.data.id });
        evt.data.context.searcher.addTagSelection(fullPath);
    } else {
        evt.data.context.searcher.removeTagSelection(evt.data.id);
    }
}

CategoryBrowser.prototype.navigateToTag = function (arg)
{
    var id;
    if (typeof arg == 'number') {
        id = arg;
    } else {
        id = arg.data.id;
    }

    if (this.path.length == 0 || this.path[this.path.length - 1] != id) {
        this.path.push({ 'id': id });
        this.loadChildren(id);
    }
}

CategoryBrowser.prototype.loadChildren = function (tagId)
{
    this.searcher.adapter.getChildTags(tagId, function (response) {
        this.container.children().remove();

        // Update label in path
        var translationAttempted = response.reqLangId != response.defLangId;
        var parentLabelInfo = this.searcher.getTagLabel(response.results[0].parent, translationAttempted);
        this.path[this.path.length - 1].label = parentLabelInfo;
        
        var encodedLabel = $('<div/>').html(parentLabelInfo.label).text();

        if (tagId != this.categoryId) {
            $('<div>').addClass('search_item').addClass('search_back').on('click', this.back.bind(this)).append(
                $('<a>').attr('href', '#').html('&lt; Back').css('text-decoration', 'none')
            ).append(
                $('<span>').addClass('float-right').addClass('b').html(encodedLabel)
            ).appendTo(this.container);
        }

        if (response.results.length == 0 || response.results[0].children.length == 0) {
            $('<div><i>No Results Found</i></div>').appendTo(this.container);
            return;
        }

        var children = response.results[0].children;
        for (var i = 0; i < children.length; i++)
        {
            var labelInfo = this.searcher.getTagLabel(children[i]);

            var div = $('<div>').addClass('search_item').attr('data-id', children[i].id).appendTo(this.container);

            if(children[i].type == TagType.NavigationalTag)
            {
                div.text(labelInfo.label).addClass('search_navTag');
                div.on('click', { 'id': children[i].id }, this.navigateToTag.bind(this));
                div.append($('<span class="decorator">&gt;</span>'));
            }
            else if (children[i].type == TagType.SelectableTag)
            {
                div.addClass('search_selTag');
                $('<label>').text(labelInfo.label).append(
                    $('<input/>').addClass('decorator').attr({ 'type': 'checkbox' }).on(
                        'change', { context:this, id:children[i].id, label:labelInfo.label }, this.tagSelectionChanged
                )).appendTo(div);
            }
        }
    }, this);
}

// --------------
function TagNavigator(container, options) {
    if (typeof container == 'string') {
        container = $('#' + container);
    }
    if (!container || !container.length) {
        throw "Tag navigation container not found.";
    }
    this.container = container;
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
    this.container.children().remove();
    this.container.addClass(this._cssContainerClassName);

    this.hierarchicalTagContainer = $('<div>').addClass(this._hierarchicalContainerDivClassName);
    this.container.append(
        this.hierarchicalTagContainer).append(
        $('<br>').css('clear', 'left'));
    
    if (this.options.enabled) {
        var autocomplete = $('<input>').addClass(this._searchTextClassName);
        this.wireAutocomplete(autocomplete);
        this.container.append(autocomplete);

        this.selectedBreadcrumbsContainer = $('<ul>').addClass(this._selectedBreadcrumbsUlClassName);
        this.container.append(this.selectedBreadcrumbsContainer);
    }
    // See: http://www.quirksmode.org/css/clearing.html for possible workaround
    this.container.append($('<br>').css('clear', 'left'));
        
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
        ? this.container.find(selector)
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

