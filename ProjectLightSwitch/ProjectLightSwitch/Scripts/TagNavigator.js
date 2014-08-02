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

    // Set defaults
    this.options = {
        selectedTagSubmissionName: "SelectedTags[{0}]",
        ajax_getChildrenUrl: "/Tags/Children",
        ajax_navigateToTagUrl: "/Tags/Navigate",
        editUrl: "/Tags/Edit/{0}",
        editable: true,
        pathSeparator: ' > '
    };
    if (options != null) {
        var providedOptionKeys = Object.keys(options);
        for (var providedOptionKey in providedOptionKeys) {
            this.options[providedOptionKey] = options[providedOptionKey];
        }
    }

    // PRIVATES
    this._cssContainerClassName = "TagNavigator";
    this._selectedClassName = "sel",
    this._hiddenInputContainerName = "selTagInputs";
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
    this.selectedBreadcrumbsContainer = $('<ul>').addClass(this._selectedBreadcrumbsUlClassName);

    this.container.append(
        this.hierarchicalTagContainer).append(
        $('<br>').attr('clear', 'left')).append(
        this.selectedBreadcrumbsContainer).append(
        // See: http://www.quirksmode.org/css/clearing.html for possible workaround
        $('<br>').attr('clear', 'left')
    );

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
            var list = getUrlPath();
            var num = parseInt(list[list.length - 1]);
            if (!isNaN(num)) {
                tagId = num;
            }
            else {
                tagId = InvisibleRootId;
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
            'name': tagnav.options.selectedTagSubmissionName.replace('{0}', index),
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
        tagNavigator = this;
        $('<li />').attr(this._tagIdAttr, tagId).text(path.join(this.options.pathSeparator)).append($('<a>').attr('href', '#').click(function () {
            tagNavigator.endTagSelected(tagId, false); return false;
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
    var ul = $('<ul>').attr(this._depthAttr, depth).attr(this._parentIdAttr, data.parent.id).attr(this._parentNameAttr, data.parent.name);
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
        if (tagnav.options.editable) {
            li.append('<span class="editSpan">(<a href="' + tagnav.options.editUrl.replace('{0}', child.id) + '">Edit</a>)</span>')
        }

        if (child.type == TagType.NavigationalTag || child.type == TagType.Category) {
            li.addClass(child.type == TagType.Category ? 'tagnav_cat' : 'tagnav_nav').addClass('tagnav_tag');
            li.append(
                $('<a>').attr('href', '#').click(
                    function () {
                        tagnav.tagSelected(child.id, label, child.type, depth);
                        return false;
                    }
                ).text(label));
        }
        else if (child.type == TagType.SelectableTag) {
            var isChecked = tagnav.selectedEndTags[child.id] === true;
            li.addClass('tagnav_sel');
            li.append(
                $('<label>').attr({ 'for': 'tagnav_' + child.id }).text(label).after(
                $('<input>').click(function () {
                    tagnav.endTagSelected(child.id, $(this)[0].checked);
                }
                ).attr({ 'name': 'tagnav_' + child.id, 'type': 'checkbox', 'checked': isChecked })
            ))
        }
        li.appendTo(ul);
    });

    // Ensure only parent is selected
    var parentElement = ul.prev().find('[' + tagnav.options._parentIdAttr + '=' + data.parent.id + ']');
    parentElement.addClass(this._selectedTagClassName);
    ul.prev().children().not(parentElement).removeClass(this._selectedTagClassName);

    ul.appendTo(tagnav.hierarchicalTagContainer).fadeIn('slow');
}

