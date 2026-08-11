(() => {
  const accountInput = document.getElementById("account");
  const passwordInput = document.getElementById("password");
  const form = document.getElementById("loginForm");
  const message = document.getElementById("loginMessage");
  const roleHint = document.getElementById("roleHint");
  const loginButton = document.getElementById("loginButton");

  const roleConfig = {
    student: { label: "学生", api: "http://localhost:5101/api/auth/login", target: "http://localhost:5101/student.html" },
    teacher: { label: "教师", api: "http://localhost:5102/api/auth/login", target: "http://localhost:5102/teacher.html" },
    admin: { label: "管理员", api: "http://localhost:5103/api/auth/login", target: "http://localhost:5103/admin.html" }
  };

  const roleOf = value => ({ S: "student", T: "teacher", A: "admin" })[(value || "").trim().charAt(0).toUpperCase()] || "";
  const setMessage = (text, success = false) => { message.textContent = text; message.style.color = success ? "#15803d" : "#d64545"; };
  const setLoading = loading => { loginButton.disabled = loading; loginButton.querySelector(".btn-text").classList.toggle("d-none", loading); loginButton.querySelector(".btn-loading").classList.toggle("d-none", !loading); };

  accountInput.addEventListener("input", () => {
    const role = roleOf(accountInput.value);
    roleHint.textContent = role ? `身份识别：${roleConfig[role].label}` : "身份识别：仅支持 S / T / A 开头的账号";
    message.textContent = "";
  });

  form.addEventListener("submit", async event => {
    event.preventDefault();
    const username = accountInput.value.trim();
    const password = passwordInput.value;
    const role = roleOf(username);
    if (!role || !password) { setMessage("请输入以 S、T 或 A 开头的账号和密码"); return; }

    setLoading(true);
    try {
      const response = await fetch(roleConfig[role].api, {
        method: "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password })
      });
      const result = await response.json().catch(() => ({}));
      if (!response.ok) throw new Error(result.message || "账号或密码错误");
      window.location.assign(roleConfig[role].target);
    } catch (error) {
      setMessage(error.message || "登录服务不可用，请确认已启动全部服务");
    } finally {
      setLoading(false);
    }
  });
})();
