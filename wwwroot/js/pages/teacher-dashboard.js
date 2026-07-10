(function () {
  function escapeHtml(value) {
    return String(value ?? "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function numberValue(value) {
    return Number.isFinite(Number(value)) ? Number(value) : 0;
  }

  function metricCard(label, value, note, tone) {
    return `
      <article class="metric-card metric-card-${tone || "blue"}">
        <div class="metric-label">${escapeHtml(label)}</div>
        <div class="metric-value">${numberValue(value)}</div>
        <div class="metric-note">${escapeHtml(note)}</div>
      </article>
    `;
  }

  function profileItem(label, value) {
    return `
      <div class="profile-item">
        <div class="profile-label">${escapeHtml(label)}</div>
        <div class="profile-value">${escapeHtml(value || "-")}</div>
      </div>
    `;
  }

  function actionButton(label, page, style) {
    return `<button class="${style || "secondary-button"} dashboard-action" type="button" data-dashboard-page="${page}">${escapeHtml(label)}</button>`;
  }

  function todoItem(title, detail, state) {
    return `
      <li class="todo-item">
        <span class="todo-dot ${state || ""}"></span>
        <div>
          <div class="todo-title">${escapeHtml(title)}</div>
          <div class="todo-detail">${escapeHtml(detail)}</div>
        </div>
      </li>
    `;
  }

  function formatToday() {
    return new Intl.DateTimeFormat("zh-CN", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      weekday: "long"
    }).format(new Date());
  }

  function bindDashboardActions(container) {
    container.querySelectorAll("[data-dashboard-page]").forEach((button) => {
      button.addEventListener("click", () => {
        if (typeof window.openTeacherPage === "function") {
          window.openTeacherPage(button.dataset.dashboardPage);
        }
      });
    });
  }

  async function render(container) {
    container.innerHTML = `
      <section class="panel dashboard-loading">
        <h3 class="panel-title">正在加载教师首页</h3>
        <div class="empty-state">正在读取教师信息和本学期统计...</div>
      </section>
    `;

    const data = await window.nativeApi.request("teacher.getDashboard", {});
    const pendingScoreCount = numberValue(data.pendingScoreCount);
    const courseCount = numberValue(data.courseCount);
    const classCount = numberValue(data.classCount);
    const studentCount = numberValue(data.studentCount);

    container.innerHTML = `
      <section class="dashboard-hero">
        <div>
          <div class="hero-kicker">${escapeHtml(formatToday())}</div>
          <h3>欢迎回来，${escapeHtml(data.teacherName || "老师")}</h3>
          <p>${escapeHtml(data.department || "-")} · ${escapeHtml(data.title || "教师")} · 工号 ${escapeHtml(data.teacherNo || "-")}</p>
        </div>
        <div class="hero-actions">
          ${actionButton("查看我的课程", "courses", "primary-button")}
          ${actionButton("提交开课申请", "applications", "secondary-button")}
        </div>
      </section>

      <section class="dashboard-grid">
        ${metricCard("本学期课程", courseCount, "当前教师负责的课程数", "blue")}
        ${metricCard("教学班", classCount, "可进入名单与成绩管理", "cyan")}
        ${metricCard("选课学生", studentCount, "所有教学班已选人数", "green")}
        ${metricCard("待录成绩", pendingScoreCount, pendingScoreCount > 0 ? "建议优先处理" : "暂无待处理", "amber")}
      </section>

      <section class="dashboard-layout">
        <article class="panel dashboard-profile">
          <div class="panel-heading-row">
            <h3 class="panel-title">个人信息</h3>
            <span class="status-badge status-approved">已登录</span>
          </div>
          <div class="profile-grid profile-grid-compact">
            ${profileItem("教师姓名", data.teacherName)}
            ${profileItem("教师工号", data.teacherNo)}
            ${profileItem("职称", data.title)}
            ${profileItem("所属院系", data.department)}
          </div>
        </article>

        <article class="panel quick-panel">
          <h3 class="panel-title">快捷操作</h3>
          <div class="quick-action-grid">
            ${actionButton("我的课程", "courses", "ghost-button")}
            ${actionButton("选课名单", "students", "ghost-button")}
            ${actionButton("成绩录入", "scores", "ghost-button")}
            ${actionButton("开课申请", "applications", "ghost-button")}
          </div>
        </article>
      </section>
    `;

    bindDashboardActions(container);
  }

  window.teacherPages = window.teacherPages || {};
  window.teacherPages.dashboard = {
    render
  };
})();