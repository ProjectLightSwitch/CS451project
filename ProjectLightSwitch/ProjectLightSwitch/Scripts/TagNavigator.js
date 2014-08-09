
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

function TagSearcher(container)
{
    this.container = container;
    this.adapter = new TagAdapter();

    this.init();
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
            if (children[i].text == null) {
                children[i].text = children[i].eng;
                if (response.reqLangId != response.defLangId) {
                    children[i].text = '<i>' + children[i].text + '</i>';
                }
            }
            this.container.append('<h3>' + children[i].text + '</h3>');
            this.container.append('<div data-id="' + children[i].id + '"></div>');
        }

        this.container.accordion({
            beforeActivate: function (event, ui) {
                if (ui.newPanel.attr('loaded') != '1') {
                    ui.newPanel.attr('loaded', '1');
                    new CategoryBrowser(ui.newPanel, this.adapter, ui.newPanel.attr('data-id'));
                }
            }
        });

    }, this);
}

function CategoryBrowser(container, adapter, categoryId)
{
    this.path = [];
    this.selectedTagIds = {};
    this.container = container;
    this.adapter = adapter;
    this.categoryId = categoryId;

    this.navigateToTag(categoryId);
}

CategoryBrowser.prototype.back = function()
{
    if(this.path.length <= 1) {
        return;
    }

    this.path.pop();
    this.loadChildren(this.path[this.path.length - 1]);
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
        this.path.push(id);
        this.loadChildren(id);
    }
}

CategoryBrowser.prototype.loadChildren = function (tagId)
{
    this.adapter.getChildTags(tagId, function (response) {
        this.container.children().remove();

        if (tagId != this.categoryId) {
            $('<a>').click(this.back.bind(this)).text('Back').appendTo(this.container);
        }

        if (response.results.length == 0) {
            return;
        }

        var children = response.results[0].children;
        
        for (var i = 0; i < children.length; i++)
        {
            var div = $('<div>').attr('data-id', children[i].id).text(children[i]['eng']).appendTo(this.container);
            div.on('click', { 'id': children[i].id }, this.navigateToTag.bind(this));
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

