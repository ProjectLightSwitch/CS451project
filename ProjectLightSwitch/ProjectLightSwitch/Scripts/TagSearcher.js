
function TagSearcher(container) {
    this.adapter = new TagAdapter();
    this.tagSelector = new TagSelector();

    //this._events = new TagSelectionEvents();
    this._container = container;
    this._selectedTags = [];
    this._categoryBrowsers = {};

    this._init();
}

// "PRIVATES"

// There should only be one instance anyway
TagSearcher.prototype._init = function ()
{
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

TagSearcher.prototype.addListener = function (handler, subscriptionFor) {
    this._events.addListener(handler, subscriptionFor);
}

TagSearcher.prototype._getTagLabel = function (tagInfo, translationAttempted) {
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

    this._events.onTagSelected(path);
}

/*
    path: [{id, label},{id, label},...]
*/
TagSearcher.prototype.deselectTag = function (tagId)
{
    for (var i = 0; i < this._selectedTags.length; i++)
    {
        var path = this._selectedTags[i];

        if (path[path.length - 1].id == tagId) {
            var catId = path[0].id;
            this._selectedTags.splice(i, 1);

            // clear checkbox
            var categoryBrowser = this._categoryBrowsers[path[0].id];
            if (typeof (categoryBrowser.tagSelectionToggled) == 'function') {
                categoryBrowser.tagSelectionToggled(tagId, false);
            }

            this._events.onTagDeselected(catId, tagId);
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

CategoryBrowser.prototype._loadChildren = function (tagId, overrideDuplicationCheck) {
    // Don't add path if it was set before coming here
    if (this._path.length == 0 || this._path[this._path.length - 1].id != tagId) {
        // Label gets loaded after data is returned
        this._path.push({ 'id': tagId, 'label': '' });
    }

    this._searcher.adapter.getChildTags(tagId, true, function (response) {
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