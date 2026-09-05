(function () {
  const fields = [
    { label: "姓名", key: "teacherName" }, { label: "教师工号", key: "teacherNo" },
    { label: "登录账号", key: "username" }, { label: "职称", key: "title" },
    { label: "所属院系", key: "department" }, { label: "手机号", key: "phone", editable: true, type: "tel" },
    { label: "邮箱", key: "email", editable: true, type: "email" }
  ];
  const render = container => window.sharedUi.renderProfilePage(container, {
    fields,
    load: () => window.nativeApi.request("teacher.getCurrentTeacher", {}),
    update: payload => window.nativeApi.request("teacher.updateProfile", payload),
    changePassword: payload => window.nativeApi.request("account.changePassword", payload),
    logout: () => window.nativeApi.request("app.logout", {})
  });
  window.teacherPages = window.teacherPages || {}; window.teacherPages.password = { render };
})();
