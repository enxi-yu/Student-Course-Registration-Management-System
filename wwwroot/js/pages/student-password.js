(function () {
  function escapeHtml(value) {
    return String(value || "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function setFormMessage(message, type) {
    const root = document.getElementById("password-form-message");
    if (!root) return;
    root.innerHTML = message ? `<div class="message ${type || ""}">${escapeHtml(message)}</div>` : "";
  }

  function setProfileMessage(message, type) {
    const root = document.getElementById("profile-form-message");
    if (!root) return;
    root.innerHTML = message ? `<div class="message ${type || ""}">${escapeHtml(message)}</div>` : "";
  }

  function infoItem(label, value) {
    return `
      <div class="profile-item">
        <div class="profile-label">${escapeHtml(label)}</div>
        <div class="profile-value">${escapeHtml(value || "-")}</div>
      </div>
    `;
  }

  async function handleProfileSubmit(event) {
    event.preventDefault();
    const form = event.currentTarget;
    const submit = form.querySelector("button[type='submit']");
    const payload = {
      phone: form.phone.value.trim(),
      email: form.email.value.trim()
    };

    submit.disabled = true;
    submit.textContent = "正在保存...";
    setProfileMessage("", "");

    try {
      await window.nativeApi.request("student.updateProfile", payload);
      setProfileMessage("联系方式保存成功。", "success");
    } catch (error) {
      setProfileMessage(error.message || "联系方式保存失败。", "error");
    } finally {
      submit.disabled = false;
      submit.textContent = "保存联系方式";
    }
  }

  async function handleSubmit(event) {
    event.preventDefault();
    const form = event.currentTarget;
    const submit = form.querySelector("button[type='submit']");
    const payload = {
      oldPassword: form.oldPassword.value,
      newPassword: form.newPassword.value,
      confirmPassword: form.confirmPassword.value
    };

    if (!payload.oldPassword || !payload.newPassword || !payload.confirmPassword) {
      setFormMessage("请完整填写原密码、新密码和确认密码。", "error");
      return;
    }

    if (payload.newPassword !== payload.confirmPassword) {
      setFormMessage("两次输入的新密码不一致。", "error");
      return;
    }

    submit.disabled = true;
    submit.textContent = "正在保存...";
    setFormMessage("", "");

    try {
      const result = await window.nativeApi.request("student.changePassword", payload);
      form.reset();
      setFormMessage(result.message || "密码修改成功。", "success");
    } catch (error) {
      setFormMessage(error.message || "密码修改失败。", "error");
    } finally {
      submit.disabled = false;
      submit.textContent = "保存新密码";
    }
  }

  async function render(container) {
    const profile = await window.nativeApi.request("student.getCurrentStudent", {});

    container.innerHTML = `
      <section class="panel">
        <h3 class="panel-title">基本信息</h3>
        <div class="profile-grid">
          ${infoItem("姓名", profile.realName)}
          ${infoItem("学号", profile.studentNo)}
          ${infoItem("专业", profile.major)}
          ${infoItem("年级", profile.grade)}
        </div>
      </section>

      <section class="panel profile-form-panel">
        <h3 class="panel-title">联系方式维护</h3>
        <p class="form-hint">学生可维护手机号和邮箱；学号、姓名、专业、年级由教务数据维护。</p>
        <div id="profile-form-message"></div>
        <form id="profile-form" class="profile-form">
          <label class="form-field">
            <span>手机号</span>
            <input name="phone" type="tel" maxlength="11" value="${escapeHtml(profile.phone || "")}" placeholder="请输入 11 位手机号">
          </label>
          <label class="form-field">
            <span>邮箱</span>
            <input name="email" type="email" maxlength="50" value="${escapeHtml(profile.email || "")}" placeholder="请输入邮箱">
          </label>
          <div class="form-actions">
            <button class="secondary-button" type="reset">重置</button>
            <button class="primary-button" type="submit">保存联系方式</button>
          </div>
        </form>
      </section>

      <section class="panel profile-form-panel">
        <h3 class="panel-title">修改密码</h3>
        <p class="form-hint">新密码长度为 6 到 20 位。保存后请使用新密码登录。</p>
        <div id="password-form-message"></div>
        <form id="password-form" class="profile-form">
          <label class="form-field">
            <span>原密码</span>
            <input name="oldPassword" type="password" autocomplete="current-password" required>
          </label>
          <label class="form-field">
            <span>新密码</span>
            <input name="newPassword" type="password" autocomplete="new-password" minlength="6" maxlength="20" required>
          </label>
          <label class="form-field">
            <span>确认新密码</span>
            <input name="confirmPassword" type="password" autocomplete="new-password" minlength="6" maxlength="20" required>
          </label>
          <div class="form-actions">
            <button class="primary-button" type="submit">保存新密码</button>
          </div>
        </form>
      </section>
    `;

    document.getElementById("profile-form").addEventListener("submit", handleProfileSubmit);
    document.getElementById("password-form").addEventListener("submit", handleSubmit);
  }

  window.studentPages = window.studentPages || {};
  window.studentPages.password = {
    render
  };
})();
