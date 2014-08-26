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

function StoryWizard(formSelector)
{
    this._formSelector = formSelector;
    $(document).ready(this._init.bind(this));
}

StoryWizard.prototype._init = function ()
{
    this._form = $(this._formSelector);
    this._form.children('section').hide();
    this._form.children('section').first().show();
    this.currentSection = 0;

    this._validator = this._form.validate(
    {
        //onkeyup: function (el) { $(el).valid(); },
        onfocusout: function (el) { $(el).valid(); },
        errorPlacement: function (error, el) {
            if ($(el).attr('type') === 'radio') {
                var parent = $(el).parent();
                if(parent.children().not('[type="radio"],label').length == 0)
                {
                    error.insertAfter(parent);
                }
                return;
            } 
            error.insertAfter(el);
        }
    });

    this._form.find('.next-section').bind('click', function (evt) {
        evt.preventDefault();
        this.navigateNext();
    }.bind(this));
    this._form.find('.prev-section').bind('click', function (evt) {
        evt.preventDefault();
        this.navigatePrevious();
    }.bind(this));
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
    //var result =  this._form.children('section:visible').find('input').valid();

    var inputs = this._form.children('section:visible').find('input');
    var len = inputs.length;

    var result = true;
    for (var i = 0; i < len; i++) {
        if (!this._validator.element($(inputs[i]))) {
            result = false;
        }
    }
    return result;
}