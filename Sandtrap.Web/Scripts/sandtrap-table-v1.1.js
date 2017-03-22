
; (function ($, window, undefined) {

    // Constants
    var rowDeleted = 'rowDeleted.table';
    var rowActivated = 'rowActivated.table';

    //Defaults
    var defaults = {
        rounding: undefined
    };

    // Constructor
    function table(element, options) {
        // Assign the DOM element
        this.element = $(element);
        this.options = $.extend({}, defaults, options);
        this.defaults = defaults;
        // Initialise the select
        this.initialise();
    }

    table.prototype.initialise = function () {

        var self = this;
        // Declare the main UI components
        this.form = $(this.element).closest('form');
        var tableBodies = $(this.element).children('tbody');
        this.body = tableBodies.first();
        this.hiddenBody = tableBodies.last();
        this.footer = $(this.element).children('tfoot').children('tr');
        this.addButton = this.footer.find('.add-button');
        // Set properties
        this.hasNumeric = false;
        this.hasSelect = false;
        this.hasDatepicker = false;
        this.fieldName = $(this.element).attr('id').replace('_', '.');
        var data = $(this.element).data();
        this.isDirtyProperty = data['isdirtyproperty'];
        this.isActiveProperty = data['isactiveproperty'];

        // Ensure referenced plugins are present
        if (this.hiddenBody.find('.numeric-input').length > 0) {
            if ($.fn.numeric) {
                // Attach plugins
                this.body.find('.numeric-input').numeric({ rounding: this.options.rounding });
                this.hasNumeric = true;
            } else {
                // TODO: Load the plugin (from where?) or combine all into one file or bundle?
                //console.error('The sandtrap-numeric plugin not loaded');
                //return;
            }
        }
        if (this.hiddenBody.find('.select-input').length > 0) {
            if ($.fn.selectlist) {
                // Attach plugins
                //this.body.find('.select-input').children('input[type="text"]').selectlist();
                this.body.find('.select-input').selectlist();
                this.hasSelect = true;
            } else {
                // TODO: Load the plugin (from where?) or combine all into one file or bundle?
                //console.error('The sandtrap-select plugin not loaded');
                //return;
            }
        }
        if (this.hiddenBody.find('.datepicker-input').length > 0) {
            if ($.fn.datepicker) {
                // Attach plugins
                this.body.find('.datepicker-input').datepicker();
                this.hasDatepicker = true;
            } else {
                // TODO: Load the plugin (from where?) or combine all into one file or bundle?
                //console.error('The sandtrap-datepicker plugin not loaded');
                //return;
            }
        }

        // **************Events****************

        // Add new row to the table
        this.addButton.click(function (e) {
            self.addRow();
        });

        // Delete new rows from the table or archive/activate existing rows
        this.body.on('click', '.table-button', function () {
            var row = $(this).closest('tr');
            self.deleteRow(row);
        });

        // Track changes
        this.body.on('change', 'input, textarea, select', function () {
            if (self.isDirtyProperty === undefined) {
                return;
            }
            var row = $(this).closest('tr');
            var isDirtyInput = $(this).closest('tr').find('.' + self.isDirtyProperty);
            isDirtyInput.val(self.isDirty(row));
        })

        // Update totals
        this.body.on('change', '.numeric-input', function () {
            self.updateTotals($(this));
        });

        // Keyboard events
        this.element.on('keydown', 'input', function (e) {
            // Add row on CTRL+SHIFT+PLUS, delete row on Ctrl+SHIFT+MINUS
            //if (e.ctrlKey && e.shiftKey) {
            //    if (e.which === 107) {
            //        // TODO: This causes the first item to be selected in
            //        //a select control!!
            //        self.addRow()
            //    } else if (e.which === 109) {
            //        var row = $(this).closest('tr');
            //        self.deleteRow(row);
            //    };
            //}
        });

        // Update on submit
        this.form.submit(function () {
            self.hiddenBody.remove(); // Prevent the 'new row' being posted
            // Mark any archived rows as dirty
            if (self.isDirtyProperty && self.isActiveProperty) {
                var rows = self.body.find('.archived');
                $.each(rows, function (index, item) {
                    $(this).find('.' + self.isDirtyProperty).val('True');
                })
            }
        })

    }

    // **************Methods****************

    // Update column totals
    table.prototype.updateTotals = function (element) {
        var total = 0;
        var colIndex = $(element).closest('td').index();
        // Check if the column is totalled
        var footer = this.footer.children('td').eq(colIndex).children('.footer-total');
        if (footer.length === 0) {
            return;
        }
        // Calculate the totals of active inputs in the column
        $.each(this.body.children('.edit-row'), function (index, row) {
            if (!$(row).hasClass('archived')) {
                total += Number($(row).children('td').eq(colIndex).find('.numeric-input').val());
            }
        });
        // Update the footer
        // TODO: Add function or data-val attribute for formating the correct decimal places
        //total = $(element).getFormattedValueFor(total);
        //this.footer.children('td').eq(colIndex).text(total);
        footer.text(total);
    }

    // Adds a new row to the table
    table.prototype.addRow = function () {
        // Clone the new row
        // TODO: Why doesn't this work to attach the plugins? - var clone = this.newRow.clone(true, true);
        var clone = this.hiddenBody.clone();
        // Update the indexers
        var index = $.now();
        clone.html($(clone).html().replace(/#/g, index));
        // Mark it as new
        var newRows = clone.find('tr');
        newRows.first().data('isnew', true);
        // Add the new rows
        this.body.append(newRows);
        // Attach plugins
        if (this.hasNumeric) {
            clone.find('.numeric-input').numeric({ rounding: this.options.rounding });
        }
        if (this.hasSelect) {
            clone.find('.select-input').selectlist();
        }
        if (this.hasDatepicker) {
            clone.find('.datepicker-input').datepicker();
        }
        // Add validation
        if ($.validator) {
            // TODO: Get the unobtrusive validation - this.form.data(unobtrusiveValidation);
            // and add the rules for the new elements to save reparsing the whole form
            // https://xhalent.wordpress.com/2011/01/24/applying-unobtrusive-validation-to-dynamic-content/
            if ($.validator) {
                this.form.data('validator', null);
                $.validator.unobtrusive.parse(this.form)
            }
        }
        // Set focus to the first input
        newRows.find('.table-control').first().focus();
    }

    // Delete new rows from the table or archive/activate existing rows
    table.prototype.deleteRow = function (row) {
        row = $(row);
        if (row.data('isnew')) {
            // Its a new row so remove it (it does not need to be posted back because it never existed)
            row.next('.validation-row').remove();
            row.remove();
        } else {
            var inputs = row.find('.table-control');
            var isActiveInput = undefined;
            var index = $('.edit-row').index(row);
            if (this.isActiveProperty !== undefined) {
                isActiveInput = row.find('.' + this.isActiveProperty);
            }
            if (row.hasClass('archived')) {
                // Its an existing row so mark it as active
                row.removeClass('archived');
                // Allow user interaction
                $.each(inputs, function (index, item) {
                    $(this).siblings('.table-text').remove();
                    $(this).show();
                });
                isActiveInput.val('True');
                // Raise event
                $(this.element).trigger({
                    type: rowActivated,
                    index: row.index() // TODO: which index taking into account validation rows?
                });
            } else {
                // Archive the row
                row.addClass('archived');
                // Prevent user interaction
                $.each(inputs, function (index, item) {
                    var value = $(this).val();
                    if ($(this).is('select')) {
                        value = $(this).children(':selected').text();
                    } else if ($(this).is(':checkbox')) {
                        value = $(this).is(':checked') ? 'Yes' : 'No';
                    }
                    var div = $('<div></div>').addClass('table-text').text(value);
                    $(this).after(div);
                    $(this).hide();
                });
                isActiveInput.val('False');
                // Raise event
                $(this.element).trigger({
                    type: rowDeleted,
                    index: row.index() // TODO: which index taking into account validation rows?
                });
            }
        }
        // Update totals
        row.find('.numeric-input').each(function () {
            //self.upda
            self.updateTotals($(this));
        });
    }

    // Returns a value indicating if row values have changed from their original values
    table.prototype.isDirty = function (row) {
        var isDirty = false;
        var inputs = $(row).find('.table-control');
        $.each(inputs, function () {
            var input = $(this);
            if (input.is(':checkbox') && input.prop('checked') !== input.prop('defaultChecked')) {
                isDirty = true;
                return false;
            } else if (input.is('select')) {
                var selected = input.children(':selected');
                if (!selected.prop('defaultSelected')) {
                    isDirty = true;
                    return false;
                }
            } else if (input.val() !== input.prop('defaultValue')) {
                isDirty = true;
                return false;
            }
        });
        return isDirty;
    }

    // Table definition
    $.fn.table = function (options) {
        return this.each(function () {
            if (!$.data(this, 'table')) {
                $.data(this, 'table', new table(this, options));
            }
        });
    }

}(jQuery, window));