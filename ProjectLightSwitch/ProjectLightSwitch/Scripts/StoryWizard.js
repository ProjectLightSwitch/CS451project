/*
Use: 

Place <section> inside <form> tag
Place grouped elements inside <section> tags

Call constructor with form id attribute, and jQuery validator rules object
(Doesn't support custom messages for now)

place next and previous buttons or links with 'next-section' and 'prev-section' classes
the form will only navigate to the next section if the current one is valid
going forward past the last section will submit the form

*/

function StoryWizard(formId, rules)
{
    this._formId = formId;
    this._rules = rules;
    $(document).ready(this._init.bind(this));
}

StoryWizard.prototype._init = function ()
{
    this._form = $('#' + this._formId);
    this._form.children('section').hide();
    this._form.find('section:first-child').show();
    this.currentSection = 0;

    this._form.find('.next-section').bind('click', function (evt) {
        evt.preventDefault();
        this.navigateNext();
    }.bind(this));
    this._form.find('.prev-section').bind('click', function (evt) {
        evt.preventDefault();
        this.navigatePrevious();
    }.bind(this));
    this._form.submit(function (evt) {
        if (!this._validateCurrentSection) {
            evt.preventDefault();
        }
    }.bind(this));

    $.validator.addMethod(
        "regex",
        function (value, element, regexp) {
            //var re = new RegExp(regexp);
            return this.optional(element) || regexp.test(value);
        },
        "Invalid input"
    );

    this._validator = this._form.validate({
        'rules': this._rules,
        'errorPlacement': function (error, element) {
            if (element.attr("type") == "radio") {
                error.insertAfter(element.closest('form').find('[name="' + element.attr('name') + '"]:last'));
            } else {
                error.insertAfter(element);
            }
        }
    });
}

StoryWizard.prototype.navigateToSection = function (idx)
{
    var sections = this._form.children('section');
    if (idx == -1) {
        this.currentSection = 0;
        return;
    }
    else if (idx == sections.length) {
        this.currentSection--;
        this._form.trigger('submit');
    }
    else {
        sections.hide();
        $(sections[idx]).show();
        this.currentSection = idx;
    }
}

StoryWizard.prototype.navigateNext = function () {
    if (this._validateCurrentSection()) {
        this.navigateToSection(this.currentSection + 1);
    }
}

StoryWizard.prototype.navigatePrevious = function () {
    this.navigateToSection(this.currentSection - 1);
}

StoryWizard.prototype._validateCurrentSection = function ()
{
    var inputs = this._form.find('section:visible').find('input');
    var len = inputs.length;

    var result = true;
    for (var i = 0; i < len; i++) {
        if (!this._validator.element($(inputs[i]))) {
            result = false;
        }
        //if (typeof (this._rules[$(inputs[i]).attr('name')]) != 'undefined' && !$(inputs[i]).valid()) {
        //    result = false;
        //}
    }
    return result;
}