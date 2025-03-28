
// ----------------------------------------------------------
// PLCCheckbox
// ----------------------------------------------------------

function fcb_selectCheckbox(cbx) {
    var $span = $(cbx).closest('span');
    var group = $span.attr('controlgroup');
    var isheader = $span.attr('isheadercontrol');

    if (isheader == 'T') {
        $("span[controlgroup='" + group + "'][isheadercontrol='F']").each(function () {
            $(this).find("input[type=checkbox][id$='ucb_cbSelect']")[0].checked = cbx.checked;
        });
    }
    else {
        var $cbx_head = $("span[controlgroup='" + group + "'][isheadercontrol='T'] > input[type=checkbox][id$='ucb_cbSelect']")[0];
        if ($cbx_head) {
            var chk = true;
            $("span[controlgroup='" + group + "'][isheadercontrol='F']").each(function () {
                if (!$(this).find("input[type=checkbox][id$='ucb_cbSelect']")[0].checked) { chk = false; }
            });
            $cbx_head.checked = chk;
        }
    }
};

// #region OOP helper
var OOP = {
    version: 0.0,
    inherit: function (subClass, superClass) {
        subClass.prototype = Object.create(superClass.prototype);
        Object.defineProperty(subClass.prototype, 'constructor', {
            value: subClass,
            enumerable: false,
            writable: true
        });
    }
};
// #endregion OOP helper

// #region DataSource
(function ($, document) {
    "use strict";

    function DataSource(opts) {
        this.version = 0.0;

        this.opts = opts;
    }

    DataSource.prototype._createRequest = function (req, opts, defaults) {
        var request = {};

        if (typeof req === "object") {
            request = $.extend({}, request, req);
        } else {
            request.url = req;
        }

        request.data = $.extend({}, opts.data, request.data);

        request = $.extend({}, defaults, request, {
            success: opts.success,
            error: opts.error
        });

        return request;
    };

    DataSource.prototype.save = function (opts) {
        var save = this.opts.save;
        var dataMap = this.opts.dataMap;

        if (typeof save === "function") {
            save(opts);
        } else {
            var post = DataSource.defaults.post;
            var request = this._createRequest(save, opts, post);

            if (!!dataMap) {
                var newData = dataMap(request.data, "save");
                if (typeof newData !== "undefined") {
                    request.data = newData;
                }
            }

            $.ajax(request);
        }
    };

    DataSource.prototype.read = function (opts) {
        var read = this.opts.read;
        var dataMap = this.opts.dataMap;

        if (typeof read === "function") {
            read(opts);
        } else {
            var get = DataSource.defaults.get;
            var request = this._createRequest(read, opts, get);

            if (!!dataMap) {
                var newData = dataMap(request.data, "read");
                if (typeof newData !== "undefined") {
                    request.data = newData;
                }
            }

            $.ajax(request);
        }
    };

    DataSource.prototype.delete = function (opts) {
        var del = this.opts.delete;
        var dataMap = this.opts.dataMap;

        if (typeof del === "function") {
            del(opts);
        } else {
            var post = DataSource.defaults.post;
            var request = this._createRequest(del, opts, post);

            if (!!dataMap) {
                var newData = dataMap(request.data, "delete");
                if (typeof newData !== "undefined") {
                    request.data = newData;
                }
            }

            $.ajax(request);
        }
    };

    DataSource.defaults = {
        get: { type: "POST", dataType: "json" },
        post: { type: "POST", dataType: "json" }
    };

    window.DataSource = DataSource;

})(window.jQuery, document);
// #endregion DataSource

// #region DBPanel
(function (document) {
    "use strict";

    function DBPanel(dbpServerId, opts) {
        this.version = 0.0;

        var id = dbpServerId;
        this.id = id;

        var dbp = document.querySelector("[id$='" + id + "']");
        this.dbp = dbp;

        var panelList = _plcdbpanellist[id];
        this.fields = panelList.fields;

        this.opts = opts || {};
        opts = this.opts;

        var ds = opts.dataSource;
        this.dataSource = ds;

        if (!!ds) {
            this.load();
        }
    }

    DBPanel.prototype.focusFirstField = function () {
        var dbp = this.dbp;
        var inputs = ["input:not([type='hidden'])", "select", "textarea"];
        var selector = inputs.join(":enabled:not([readonly]), ") + ":enabled:not([readonly])";

        var nodeList = dbp.querySelectorAll(selector);
        for (var i = 0; i < nodeList.length; i++) {
            var input = nodeList[i];
            var style = input.style;

            if (style.visibility != "hidden"
                && style.display != "none") {
                input.focus();
                input.select();
                break;
            }
        }
    };

    DBPanel.prototype.editMode = function () {
        var dbp = this.dbp;
        dbp.querySelectorAll("[browse]").forEach(function (input) {
            input.disabled = false;
            input.removeAttribute("browse");

            if ((input instanceof HTMLInputElement && input.type === "image")
                || input instanceof HTMLSpanElement) {
                input.style.filter = "";
                input.style.opacity = "";
            }
        });

        this.focusFirstField();

        this._mode = "EDIT";
    };

    DBPanel.prototype.browseMode = function () {
        var dbp = this.dbp;
        var inputs = ["input", "select", "textarea", "button", "span"];
        var selector = inputs.join(":enabled, ") + ":enabled";

        var prevSibling = dbp.previousElementSibling;
        if (!!prevSibling && prevSibling.id == "expandModal") {
            var closeExpand = prevSibling.querySelector(".ui-dialog-titlebar-close");
            closeExpand.click();
        }

        dbp.querySelectorAll(selector).forEach(function (input) {
            input.disabled = true;
            input.setAttribute("browse", "browse");

            if ((input instanceof HTMLInputElement && input.type === "image")
                || input instanceof HTMLSpanElement) {
                input.style.filter = "grayscale(1)";
                input.style.opacity = ".75";
            }
        });

        this._mode = "BROWSE";
    };

    DBPanel.prototype.getMode = function () {
        return this._mode;
    };

    DBPanel.prototype.isEditMode = function () {
        return this.getMode() === "EDIT";
    };

    DBPanel.prototype.isBrowseMode = function () {
        return this.getMode() === "BROWSE";
    };

    DBPanel.prototype.save = function (callback, params) {
        if (!this.validate())
            return;

        var json = this.toJSON();
        var ds = this.dataSource;

        var data = {
            params: params,
            fields: json
        };

        if (ds) {
            ds.save({
                data: data,
                success: success,
                error: function () { Alert("Error saving."); }
            });
        }

        var self = this;
        function success(response) {
            self.browseMode();
            if (!!callback) callback(response);
        }
    };

    DBPanel.prototype.load = function (callback, params) {
        var fields = $.extend({}, this.fields);
        var ds = this.dataSource;

        var data = {
            params: params,
            fields: fields
        };

        if (ds) {
            ds.read({
                data: data,
                success: success,
                error: function () { Alert("Error loading."); }
            });
        }

        var self = this;
        function success(response) {
            self.fromJSON(response);
            if (!!callback) callback(response);
        }
    };

    DBPanel.prototype.cancel = function (callback, params) {
        this.load(callback, params);
        this.browseMode();
    };

    DBPanel.prototype.delete = function (callback, params) {
        var json = this.toJSON();
        var ds = this.dataSource;

        var data = {
            params: params,
            fields: json
        };

        if (ds) {
            ds.delete({
                data: data,
                success: success,
                error: function () { Alert("Error deleting."); }
            });
        }

        var self = this;
        function success(response) {
            self.clear();
            if (!!callback) callback(response);
        }
    };

    DBPanel.prototype.clear = function () {
        var fields = this.fields;
        for (var field in fields) {
            this.setFieldValue(field, "");
        }
    }

    DBPanel.prototype.reset = function () {
        var fields = this.fields;
        for (var field in fields) {
            var $field = $("#" + fields[field].clientid);
            if ($field.is(":input:visible:enabled:not([readonly])")
                || $field.find(":input:visible").is(":enabled:not([readonly])")) {
                this.setFieldValue(field, "");
            }
        }
    }

    DBPanel.prototype.validate = function () {
        var fields = this.fields;
        var emptyMandatoryFields = "";

        for (var field in fields) {
            var pf = fields[field];

            // skip hidden fields
            if (pf.hidefield == "T")
                continue;

            var fieldValue = this.getFieldValue(field).trim();
            if (pf.required == "T" && fieldValue == "") {
                emptyMandatoryFields += " - " + pf.prompt + "<br/>";
            }
        }

        if (emptyMandatoryFields != "") {
            var msg = "The following field(s) are required: <br/>" + emptyMandatoryFields;
            _msgDlg.alert({ title: "Validate", content: msg });
            return false;
        }

        return true;
    };

    // Requires PLCDBPanel.js
    DBPanel.prototype.getFieldValue = function (field) {
        var panelId = this.id;
        return GetDBPanelField(panelId, field);
    };

    // Requires PLCDBPanel.js
    DBPanel.prototype.setFieldValue = function (field, value) {
        var panelId = this.id;
        SetDBPanelField(panelId, field, value);
    };

    DBPanel.prototype.toJSON = function () {
        var fields = this.fields;
        var json = {};
        for (var field in fields) {
            json[field] = this.getFieldValue(field);
        }
        return json;
    };

    DBPanel.prototype.fromJSON = function (json) {
        var fields = this.fields;
        for (var field in fields) {
            var fieldName = field.split('.')[1];
            var fieldValue = json[fieldName];

            if (fieldValue) {
                var mask = fields[field].editmask.toLowerCase();
                var format = null;
                if (mask.match(/yyyy/))
                    format = mask.replace("mm/", "MM/").replace("hh", "HH");
                else if (mask.match(/hh:mm/))
                    format = mask.replace("hh", "HH");

                if (format)
                    fieldValue = new Date(fieldValue).format(format);
            }

            this.setFieldValue(field, fieldValue);
        }
    };

    DBPanel.prototype.initForDialog = function (dialogEl) {
        var dbp = this.dbp;

        // #region Calendar
        // create calendar container to move out calendar popup from the dialog
        var dbpCalendar = document.querySelector("dbpanel-calendar");
        if (!dbpCalendar) {
            dbpCalendar = document.createElement("div");
            dbpCalendar.classList.add("dbpanel-calendar");
            document.body.appendChild(dbpCalendar);
        }

        // bind click event to calendar icon
        dbp.querySelectorAll("[id*='imgCalendar']").forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                // execute event once
                if (e.currentTarget.dataset.initForDialog) return;
                e.currentTarget.dataset.initForDialog = true;

                // get offset from top left corner of the page
                var el = dialogEl || dbp;
                var rect = el.getBoundingClientRect();
                var top = rect.top + window.pageYOffset;
                var left = rect.left + window.pageXOffset;

                // reposition popup and add to calendar container
                dbp.querySelectorAll(".ajax__calendar").forEach(function (calendar) {
                    calendar.style.position = "absolute";
                    calendar.style.top = top + "px";
                    calendar.style.left = left + "px";
                    calendar.querySelector(".ajax__calendar_container").style.zIndex = 2000;

                    dbpCalendar.appendChild(calendar);
                });
            });
        });
        // #endregion Calendar
    };

    window.DBPanel = DBPanel;

})(document);
// #endregion DBPanel

// #region Dialog
(function ($, document) {
    "use strict";

    function Dialog(div, opts) {
        this.version = 0.0;

        var self = this;
        this.opts = $.extend(true, {}, Dialog.defaults, opts);
        opts = this.opts;

        var uiOpts = opts.ui;
        uiOpts.close = function () {
            if (opts.updatePanel) {
                // Append to original parent to avoid duplicate ids on partial postback
                $(this).parent().appendTo(self.$parent).css({ top: 0, left: 0 });
            }

            self.dispatch("close");
        };
        uiOpts.open = function () {
            let $dialog = $(this);

            $(".ui-dialog-titlebar-close", $(this).parent()).hide();
            $(".ui-widget-overlay").css("position", "fixed");

            // Fix position of flexbox dropdown
            $(".popup-ffb").css("position", "absolute").css("top", "0px").css("left", "0px");

            //updated to use appendTo option
            //$(this).parent().appendTo("form");

            if (opts.updatePanel && self.$parent.find($dialog.parent()).length > 0) {
                $dialog.parent().appendTo(uiOpts.appendTo);
            }

            $dialog.parent().find(".ui-dialog-buttonset :button:visible:first").focus();

            self.dispatch("open");
        };
        uiOpts.dragStart = function () {
            $(".popup-ffb .ffb:visible").hide();

            self.dispatch("dragStart");
        };

        this.$dialog = typeof div === "string" ? $("#" + div) : $(div);
        this.$parent = this.$dialog.parent();
        this._dialog = this.$dialog.dialog(uiOpts);

        var $dlgButtons = this.$dialog.parent().find(".ui-dialog-buttonset");
        this.$buttons = $dlgButtons.find(":button");

        // create the spacer after the button
        var $btnSpacer = $dlgButtons.find(":button:contains(" + opts.spacer + ")");
        if ($btnSpacer.length) {
            var spacer = document.createElement("div");
            spacer.className = "dialog-spacer";
            spacer.style.width = "100%";
            $btnSpacer.after(spacer);

            $dlgButtons.css("display", "flex");
            $dlgButtons.css("width", "100%");
        }

        // contain tabbing
        if (opts.containTabbing) {
            var $container = this.$dialog.closest(".ui-dialog");
            $container[0].addEventListener('keydown', function (e) {
                if (e.keyCode === 9) {
                    var isForward = !e.shiftKey;
                    var $tabbables = $container.find(":tabbable:visible:enabled");
                    var edgeEl = (isForward ? $tabbables.last() : $tabbables.first())[0];
                    var nextEl = (isForward ? $tabbables.first() : $tabbables.last())[0];

                    if (edgeEl === document.activeElement) {
                        e.preventDefault();
                        nextEl.focus();
                    }
                }
            }, true);

            if ($container.find("[tabindex]:first").length) {
                $container.find("[tabindex]").removeAttr("tabindex");
            }
        }
    }

    Dialog.prototype.openCenter = function () {
        this.open();
        this.recenter();
    };

    Dialog.prototype.recenter = function () {
        var $dialog = this.$dialog;

        $dialog.dialog("option", "position", { my: "center", at: "center", of: window });
    };

    Dialog.prototype.open = function () {
        var $dialog = this.$dialog;

        $dialog.dialog("open");
    };

    Dialog.prototype.close = function () {
        var $dialog = this.$dialog;

        $dialog.dialog("close");
    };

    Dialog.prototype.dispatch = function (event, args) {
        var handler = this.opts.events[event];
        if (handler) {
            var $dialog = this.$dialog;
            handler.call($dialog, this, args);
        }
    }

    Dialog.defaults = {
        ui: {
            title: document.title,
            resizable: false,
            draggable: true,
            closeOnEscape: false,
            width: "auto",
            minWidth: 300,
            modal: true,
            autoOpen: false,
            appendTo: "form"
        },
        events: {},
        updatePanel: false,
        containTabbing: true
    };

    window.Dialog = Dialog;
    
})(window.jQuery, document);

// #region DBPanel Dialog
(function (Dialog, $, document) {
    "use strict";

    function DBPanelDialog(div, opts) {
        //opts.updatePanel = false;
        Dialog.call(this, div, opts);
        this.version = 0.0;

        var self = this;
        opts = this.opts;

        var $dialog = this.$dialog;
        var dbPanel = opts.dbPanel;

        var dbpID = (dbPanel || {}).id || Math.random().toString(36).slice(-6) + "_dbp_";
        var btnIds = {
            Add: dbpID + "_add",
            Edit: dbpID + "_edit",
            Save: dbpID + "_save",
            Cancel: dbpID + "_cancel",
            Delete: dbpID + "_delete",
            Close: dbpID + "_close"
        };
        this.btnIds = btnIds;

        var uiButtons = $dialog.dialog("option", "buttons");
        var buttons = opts.buttons || [];

        var btnOpts = {
            "Add": function () {
                var dbp = self.opts.dbPanel;

                self.editMode();
                dbp.clear();

                self.dispatch("add", dbp);
            },
            "Edit": function () {
                var dbp = self.opts.dbPanel;

                self.editMode();

                self.dispatch("edit", dbp);
            },
            "Save": function () {
                var dbp = self.opts.dbPanel;

                if (dbp) {
                    dbp.save(saved);
                } else {
                    saved();
                }

                function saved() {
                    self.browseMode();
                    self.dispatch("save", dbp);
                }
            },
            "Cancel": function () {
                var dbp = self.opts.dbPanel;

                self.browseMode();
                if (dbp) {
                    dbp.cancel();
                }

                self.dispatch("cancel", dbp);
            },
            "Delete": function () {
                var dbp = self.opts.dbPanel;

                if (dbp) {
                    dbp.delete(deleted);
                } else {
                    deleted();
                }

                function deleted() {
                    self.dispatch("delete", dbp);
                }
            }
        };

        for (var button in uiButtons) {
            btnOpts[button] = uiButtons[button];
        }

        btnOpts["Close"] = function () {
            self.close();
        };

        $dialog.dialog("option", "buttons", btnOpts);

        // set button id and hide button not in buttons option
        var $dlgButtons = $dialog.parent().find(".ui-dialog-buttonset");
        for (var btn in btnIds) {
            var $btn = $dlgButtons.find(":button:contains(" + btn + ")");
            $btn.attr("id", btnIds[btn]);

            if ((/^(add|edit|delete)$/i).test(btn)
                && !buttons[btn.toLowerCase()]
                && !buttons[btn]) {
                $btn.hide();
            }
        }

        // separate standard and custom buttons
        $dlgButtons.css("display", "flex");
        $dlgButtons.css("width", "100%");

        // create spacer for buttons
        var spacer = document.createElement("div");
        spacer.className = "dbp-dialog-spacer";
        spacer.style.width = "100%";
        $("#" + btnIds.Delete).after(spacer);

        this.$buttons = $dlgButtons.find(":button");

        if (!!dbPanel) {
            dbPanel.initForDialog($dialog[0]);
        }

        this.browseMode();
    }

    OOP.inherit(DBPanelDialog, Dialog);

    DBPanelDialog.prototype.editMode = function () {
        var btnIds = this.btnIds;
        this.$buttons.button("option", "disabled", true);
        $("#" + btnIds.Save).button("option", "disabled", false);
        $("#" + btnIds.Cancel).button("option", "disabled", false);

        var dbp = this.opts.dbPanel;
        if (dbp) {
            dbp.editMode();
        }
    };

    DBPanelDialog.prototype.browseMode = function () {
        var btnIds = this.btnIds;
        this.$buttons.button("option", "disabled", false);
        $("#" + btnIds.Save).button("option", "disabled", true);
        $("#" + btnIds.Cancel).button("option", "disabled", true);

        var dbp = this.opts.dbPanel;
        if (dbp) {
            dbp.browseMode();
        }
    };

    window.DBPanelDialog = DBPanelDialog;

})(window.Dialog, window.jQuery, document);
// #endregion DBPanel Dialog

// #region Message Dialog
(function (Dialog, $, document) {
    "use strict";

    function MessageDialog(opts) {
        var div = document.createElement("div");
        div.className = "msg-dialog";
        document.body.appendChild(div);

        Dialog.call(this, div, opts);
        this.version = 0.0;

        var self = this;
        opts = this.opts;

        var $dialog = this.$dialog;

        var msgID = Math.random().toString(36).slice(-6) + "_msg_";
        var btnIds = {
            OK: msgID + "_ok",
            Cancel: msgID + "_cancel"
        };
        this.btnIds = btnIds;

        var uiButtons = $dialog.dialog("option", "buttons");

        var btnOpts = {
            "OK": function () {
                // close first before dispatching event
                // to avoid jquery error: https://bugs.jqueryui.com/ticket/15182/
                // when a first dialog is going to be closed in the event
                self.close();
                self.dispatch("ok");
            },
            "Cancel": function () {
                self.close();
                self.dispatch("cancel");
            }
        };

        for (var button in uiButtons) {
            btnOpts[button] = uiButtons[button];
        }

        $dialog.dialog("option", "buttons", btnOpts);

        // set button id and hide button not in buttons option
        var $dlgButtons = $dialog.parent().find(".ui-dialog-buttonset");
        for (var btn in btnIds) {
            var $btn = $dlgButtons.find(":button:contains(" + btn + ")");
            $btn.attr("id", btnIds[btn]);
        }

        // separate standard and custom buttons
        $dlgButtons.css("display", "flex");
        $dlgButtons.css("width", "100%");

        // create spacer for buttons
        var spacer = document.createElement("div");
        spacer.className = "msg-dialog-spacer";
        spacer.style.width = "100%";
        $("#" + btnIds.OK).before(spacer);

        this.$buttons = $dlgButtons.find(":button");
    }

    OOP.inherit(MessageDialog, Dialog);

    MessageDialog.prototype.alert = function (opts) {
        var btnIds = this.btnIds;
        this.$buttons.show();
        $("#" + btnIds.Cancel).hide();

        var content;
        var position = { my: "center", at: "center", of: window };
        if (typeof opts === "string") {
            content = opts;
        } else {
            content = opts.content || "";
            delete opts.content;

            this.opts.events = opts.events || {};
            delete opts.events;

            if (opts.position)
                position = opts.position;

            this.$dialog.dialog("option", opts);
        }

        this.$dialog.html(content);

        this.open();
        this.$dialog.dialog("option", "position", position);
    };

    MessageDialog.prototype.confirm = function (opts) {
        var btnIds = this.btnIds;
        this.$buttons.show();

        var content = opts.content || "";
        delete opts.content;

        this.opts.events = opts.events || {};
        delete opts.events;

        if (!opts.position)
            opts.position = { my: "center", at: "center", of: window };

        this.$dialog.dialog("option", opts);
        this.$dialog.html(content);

        this.open();
        this.$dialog.dialog("option", "position", opts.position);
    };

    MessageDialog.prototype.prompt = function () {
        console.error("Not implemented");
    };

    // global instance
    var _msgDlg = new MessageDialog({
        ui: {
            minWidth: 300,
            maxWidth: 1000,
            minHeight: 200,
            maxHeight: 600
        }
    });
    _msgDlg.$dialog.css({
        minWidth: 300,
        maxWidth: 1000,
        minHeight: 200,
        maxHeight: 600,
        display: "flex",
        alignItems: "center",
        flexWrap: "wrap"
    });
    window._msgDlg = _msgDlg;

    window.MessageDialog = MessageDialog;

})(window.Dialog, window.jQuery, document);
// #endregion Message Dialog
// #endregion Dialog