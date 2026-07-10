(() => {
  const accountInput = document.getElementById("account");
  const passwordInput = document.getElementById("password");
  const form = document.getElementById("loginForm");
  const message = document.getElementById("loginMessage");
  const roleHint = document.getElementById("roleHint");
  const loginButton = document.getElementById("loginButton");
  const togglePassword = document.getElementById("togglePassword");

  const roleConfig = {
    student: { label: "学生", color: "#1456d6", target: "/student.html" },
    teacher: { label: "教师", color: "#0f8f6b", target: "/teacher.html" },
    admin: { label: "管理员", color: "#c56a00", target: "/admin.html" }
  };

  function normalizeAccount(value) {
    return (value || "").trim();
  }

  function getRoleByAccount(account) {
    const prefix = normalizeAccount(account).charAt(0).toUpperCase();
    if (prefix === "S") return "student";
    if (prefix === "T") return "teacher";
    if (prefix === "A") return "admin";
    return "";
  }

  function setMessage(text, type = "error") {
    message.textContent = text;
    message.style.color = type === "success" ? "#15803d" : "#d64545";
  }

  function clearMessage() {
    message.textContent = "";
  }

  function updateRoleHint() {
    const account = normalizeAccount(accountInput.value);
    const role = getRoleByAccount(account);

    if (!account) {
      roleHint.textContent = "身份识别：等待输入";
      roleHint.style.color = "";
      roleHint.style.background = "";
      return;
    }

    if (!role) {
      roleHint.textContent = "身份识别：仅支持 S / T / A 开头";
      roleHint.style.color = "#b45309";
      roleHint.style.background = "#fff7ed";
      return;
    }

    const config = roleConfig[role];
    roleHint.textContent = `身份识别：${config.label}`;
    roleHint.style.color = config.color;
    roleHint.style.background = `${config.color}12`;
  }

  function setLoading(loading) {
    loginButton.disabled = loading;
    loginButton.querySelector(".btn-text").classList.toggle("d-none", loading);
    loginButton.querySelector(".btn-loading").classList.toggle("d-none", !loading);
  }

  function buildTargetUrl(role, account) {
    const config = roleConfig[role];
    const params = new URLSearchParams({
      role,
      account
    });
    return `${config.target}?${params.toString()}`;
  }

  accountInput.addEventListener("input", () => {
    clearMessage();
    updateRoleHint();
  });

  togglePassword.addEventListener("click", () => {
    const isPassword = passwordInput.type === "password";
    passwordInput.type = isPassword ? "text" : "password";
    togglePassword.textContent = isPassword ? "隐藏" : "显示";
  });

  form.addEventListener("submit", (event) => {
    event.preventDefault();
    clearMessage();

    const account = normalizeAccount(accountInput.value);
    const password = passwordInput.value;
    const role = getRoleByAccount(account);

    if (!account) {
      setMessage("请输入账号。");
      accountInput.focus();
      return;
    }

    if (!role) {
      setMessage("账号格式不正确，请使用 S、T 或 A 开头的账号。");
      accountInput.focus();
      return;
    }

    if (!password) {
      setMessage("请输入密码。");
      passwordInput.focus();
      return;
    }

    const config = roleConfig[role];
    setLoading(true);
    setMessage(`正在识别为${config.label}并跳转...`, "success");

    window.localStorage.setItem("loginAccount", account);
    window.localStorage.setItem("loginRole", role);

    window.setTimeout(() => {
      window.location.href = buildTargetUrl(role, account);
    }, 450);
  });

  updateRoleHint();
})();
