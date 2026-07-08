(function () {
  function metricCard(label, value, note) {
    return `
      <article class="metric-card">
        <div class="metric-label">${label}</div>
        <div class="metric-value">${value}</div>
        <div class="metric-note">${note}</div>
      </article>
    `;
  }

  function profileItem(label, value) {
    return `
      <div class="profile-item">
        <div class="profile-label">${label}</div>
        <div class="profile-value">${value || "-"}</div>
      </div>
    `;
  }

  async function render(container) {
    container.innerHTML = `
      <section class="panel">
        <h3 class="panel-title">教师信息</h3>
        <div class="empty-state">正在加载教师首页数据...</div>
      </section>
    `;

    const data = await window.nativeApi.request("teacher.getDashboard", {});

    container.innerHTML = `
      <section class="panel">
        <h3 class="panel-title">教师信息</h3>
        <div class="profile-grid">
          ${profileItem("教师姓名", data.teacherName)}
          ${profileItem("工号", data.teacherNo)}
          ${profileItem("职称", data.title)}
          ${profileItem("所属院系", data.department)}
        </div>
      </section>

      <section class="dashboard-grid">
        ${metricCard("本学期课程数", data.courseCount, "distinct course_id")}
        ${metricCard("教学班数量", data.classCount, "当前教师负责")}
        ${metricCard("总选课人数", data.studentCount, "所有教学班已选")}
        ${metricCard("待录入成绩人数", data.pendingScoreCount, "尚无有效成绩")}
      </section>
    `;
  }

  window.teacherPages = window.teacherPages || {};
  window.teacherPages.dashboard = {
    render
  };
})();
