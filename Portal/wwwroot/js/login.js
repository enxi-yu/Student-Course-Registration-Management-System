(() => {
  const accountInput = document.getElementById("account");
  const passwordInput = document.getElementById("password");
  const form = document.getElementById("loginForm");
  const message = document.getElementById("loginMessage");
  const roleHint = document.getElementById("roleHint");
  const loginButton = document.getElementById("loginButton");
  let dialog = document.getElementById("loginDialog");
  let dialogMessage = document.getElementById("loginDialogMessage");
  let dialogButton = document.getElementById("loginDialogButton");
  let focusAfterDialog = null;

  // 兼容服务未重启时仍在内存中的旧 Razor 页面，避免新版脚本因缺少弹窗节点而中断。
  if (!dialog || !dialogMessage || !dialogButton) {
    document.body.insertAdjacentHTML("beforeend", `
      <div class="login-dialog-backdrop" id="loginDialog" hidden>
        <section class="login-dialog" role="alertdialog" aria-modal="true" aria-labelledby="loginDialogTitle" aria-describedby="loginDialogMessage">
          <div class="login-dialog-icon" aria-hidden="true">!</div>
          <div class="login-dialog-content">
            <h2 id="loginDialogTitle">登录提示</h2>
            <p id="loginDialogMessage"></p>
          </div>
          <button type="button" class="login-dialog-button" id="loginDialogButton">我知道了</button>
        </section>
      </div>`);
    dialog = document.getElementById("loginDialog");
    dialogMessage = document.getElementById("loginDialogMessage");
    dialogButton = document.getElementById("loginDialogButton");
  }

  const roleConfig = {
    student: { label: "学生", api: "http://localhost:5101/api/auth/login", target: "http://localhost:5101/student.html" },
    teacher: { label: "教师", api: "http://localhost:5102/api/auth/login", target: "http://localhost:5102/teacher.html" },
    admin: { label: "管理员", api: "http://localhost:5103/api/auth/login", target: "http://localhost:5103/admin.html" }
  };

  const roleOf = value => ({ S: "student", T: "teacher", A: "admin" })[(value || "").trim().charAt(0).toUpperCase()] || "";
  const setMessage = text => { message.textContent = text || ""; };
  const setLoading = loading => { loginButton.disabled = loading; loginButton.querySelector(".btn-text").classList.toggle("d-none", loading); loginButton.querySelector(".btn-loading").classList.toggle("d-none", !loading); };

  const closeDialog = () => {
    dialog.hidden = true;
    if (focusAfterDialog) focusAfterDialog.focus();
    focusAfterDialog = null;
  };

  const showDialog = (text, field) => {
    focusAfterDialog = field || loginButton;
    if (field) field.classList.add("input-error");
    dialogMessage.textContent = text;
    dialog.hidden = false;
    dialogButton.focus();
  };

  const userMessage = (error, response) => {
    const text = String((error && error.message) || "");
    if (text.includes("数据库连接失败")) {
      return "数据库连接暂时异常，请稍后重试。";
    }
    if (text === "Failed to fetch") {
      return "暂时无法连接登录服务，请确认对应服务已经启动后重试。";
    }
    if (response && response.status >= 500) return "登录服务暂时不可用，请稍后重试。";
    return text || "登录失败，请稍后重试。";
  };

  dialogButton.addEventListener("click", closeDialog);
  dialog.addEventListener("click", event => { if (event.target === dialog) closeDialog(); });
  document.addEventListener("keydown", event => { if (event.key === "Escape" && !dialog.hidden) closeDialog(); });

  accountInput.addEventListener("input", () => {
    const role = roleOf(accountInput.value);
    roleHint.textContent = role ? `身份识别：${roleConfig[role].label}` : "身份识别：仅支持 S / T / A 开头的账号";
    message.textContent = "";
    accountInput.classList.remove("input-error");
  });
  passwordInput.addEventListener("input", () => passwordInput.classList.remove("input-error"));

  form.addEventListener("submit", async event => {
    event.preventDefault();
    const username = accountInput.value.trim();
    const password = passwordInput.value;
    const role = roleOf(username);
    setMessage("");
    accountInput.classList.remove("input-error");
    passwordInput.classList.remove("input-error");
    if (!username) { showDialog("用户名不能为空，请输入学号或工号。", accountInput); return; }
    if (!role) { showDialog("账号格式不正确，请使用以 S、T 或 A 开头的账号。", accountInput); return; }
    if (!password.trim()) { showDialog("密码不能为空，请输入登录密码。", passwordInput); return; }

    setLoading(true);
    let response;
    try {
      response = await fetch(roleConfig[role].api, {
        method: "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password })
      });
      const result = await response.json().catch(() => ({}));
      if (!response.ok) throw new Error(result.message || "账号或密码错误");
      window.location.assign(roleConfig[role].target);
    } catch (error) {
      const text = userMessage(error, response);
      const field = text.includes("密码") ? passwordInput : (text.includes("用户名") || text.includes("账号") ? accountInput : null);
      showDialog(text, field);
    } finally {
      setLoading(false);
    }
  });
})();
