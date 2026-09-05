(function () {
  const fields = [
    { label: "姓名", key: "realName" }, { label: "学号", key: "studentNo" },
    { label: "登录账号", key: "username" }, { label: "专业", key: "major" },
    { label: "年级", key: "grade" }, { label: "手机号", key: "phone", editable: true, type: "tel" },
    { label: "邮箱", key: "email", editable: true, type: "email" }
  ];
  const render = container => window.sharedUi.renderProfilePage(container, {
    fields,
    load: () => window.nativeApi.request("student.getCurrentStudent", {}),
    update: payload => window.nativeApi.request("student.updateProfile", payload),
    changePassword: payload => window.nativeApi.request("student.changePassword", payload),
    logout: () => window.nativeApi.request("app.logout", {})
  });
  window.studentPages = window.studentPages || {}; window.studentPages.profile = { render };
})();
