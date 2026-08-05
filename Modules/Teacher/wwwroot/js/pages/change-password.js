(function () {
  function showNotice(type, message) {
    const notice = document.getElementById("password-notice");
    notice.className = `form-notice ${type}`;
    notice.textContent = message;
    notice.hidden = false;
  }

  function readForm() {
    const form = document.getElementById("password-form");
    const data = new FormData(form);
    const oldPassword = String(data.get("oldPassword") || "").trim();
    const newPassword = String(data.get("newPassword") || "").trim();
    const confirmPassword = String(data.get("confirmPassword") || "").trim();

    if (!oldPassword) {
      throw new Error("原密码不能为空。");
    }

    if (!newPassword) {
      throw new Error("新密码不能为空。");
    }

    if (newPassword.length < 6) {
      throw new Error("新密码长度不能少于 6 位。");
    }

    if (newPassword !== confirmPassword) {
      throw new Error("两次输入的新密码不一致。");
    }

    if (oldPassword === newPassword) {
      throw new Error("新密码不能和原密码相同。");
    }

    return {
      oldPassword,
      newPassword,
      confirmPassword
    };
  }

  async function render(container) {
    container.innerHTML = `
      <section class="panel">
        <h3 class="panel-title">修改密码</h3>
        <div id="password-notice" class="form-notice" hidden></div>
        <form id="password-form" class="application-form">
          <div class="field">
            <label>原密码</label>
            <input name="oldPassword" type="password" autocomplete="current-password">
          </div>
          <div class="field">
            <label>新密码</label>
            <input name="newPassword" type="password" autocomplete="new-password">
          </div>
          <div class="field">
            <label>确认新密码</label>
            <input name="confirmPassword" type="password" autocomplete="new-password">
          </div>
          <div class="field-actions">
            <button class="primary-button" type="submit">保存修改</button>
          </div>
        </form>
      </section>
    `;

    document.getElementById("password-form").addEventListener("submit", async (event) => {
      event.preventDefault();

      try {
        const payload = readForm();
        await window.nativeApi.request("account.changePassword", payload);
        event.target.reset();
        showNotice("success", "密码修改成功。");
      } catch (error) {
        showNotice("error", error.message || "密码修改失败，请稍后重试。");
      }
    });
  }

  window.teacherPages = window.teacherPages || {};
  window.teacherPages.password = {
    render
  };
})();
