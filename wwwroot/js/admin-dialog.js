(function () {
  window.adminDialog = {
    alert: function (message, title) { return window.sharedUi.alert(message, title); },
    confirm: function (message, title) { return window.sharedUi.confirm(message, title); },
    danger: function (message, title) { return window.sharedUi.danger(message, title); },
    prompt: function (message, options) { return window.sharedUi.prompt(message, options); }
  };
})();
