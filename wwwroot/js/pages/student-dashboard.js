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

  function renderTodaySchedule(courses) {
    if (!courses || courses.length === 0) {
      return `<div class="empty-state">今日无课程安排</div>`;
    }

    const rows = courses.map((item) => `
      <tr>
        <td>${item.courseName}</td>
        <td>${item.teacherName || "-"}</td>
        <td>${item.classroom || "-"}</td>
        <td>第${item.startPeriod}-${item.endPeriod}节</td>
      </tr>
    `).join("");

    return `
      <table class="data-table">
        <thead>
          <tr>
            <th>课程名称</th>
            <th>授课教师</th>
            <th>教室</th>
            <th>节次</th>
          </tr>
        </thead>
        <tbody>${rows}</tbody>
      </table>
    `;
  }

  async function render(container) {
    container.innerHTML = `
      <section class="panel">
        <h3 class="panel-title">个人信息</h3>
        <div class="empty-state">正在加载学生首页数据...</div>
      </section>
    `;

    const data = await window.nativeApi.request("student.getDashboard", {});
    const profile = data.profile || {};
    const gpa = data.gpaSummary || {};

    container.innerHTML = `
      <section class="panel">
        <h3 class="panel-title">个人信息</h3>
        <div class="profile-grid">
          ${profileItem("姓名", profile.realName)}
          ${profileItem("学号", profile.studentNo)}
          ${profileItem("专业", profile.major)}
          ${profileItem("年级", profile.grade)}
          ${profileItem("电话", profile.phone)}
          ${profileItem("邮箱", profile.email)}
        </div>
      </section>

      <section class="dashboard-grid">
        ${metricCard("本学期课程", data.currentSemesterCourseCount, "已选课程数")}
        ${metricCard("本学期学分", data.currentSemesterCredit.toFixed(1), "总学分")}
        ${metricCard("平均绩点", gpa.avgGpa ? gpa.avgGpa.toFixed(2) : "-", "GPA")}
        ${metricCard("已修学分", gpa.totalCreditsFinished ? gpa.totalCreditsFinished.toFixed(1) : "0.0", "累计完成")}
      </section>

      <section class="panel">
        <h3 class="panel-title">今日课程</h3>
        ${renderTodaySchedule(data.todayCourses)}
      </section>

      <section class="panel">
        <h3 class="panel-title">系统通信</h3>
        <div class="quick-actions">
          <button class="primary-button" type="button" id="ping-button">测试通信</button>
          <button class="secondary-button" type="button" id="db-button">测试 Oracle 连接</button>
        </div>
        <div class="status-line" id="system-status">等待测试。</div>
      </section>
    `;

    const status = document.getElementById("system-status");
    document.getElementById("ping-button").addEventListener("click", async () => {
      setStatus(status, "正在测试 C# 通信...", "");
      try {
        const result = await window.nativeApi.request("system.ping", {});
        setStatus(status, `C# 通信成功：${result.message}`, "success");
      } catch (error) {
        setStatus(status, `C# 通信失败：${error.message}`, "error");
      }
    });

    document.getElementById("db-button").addEventListener("click", async () => {
      setStatus(status, "正在连接 Oracle...", "");
      try {
        const result = await window.nativeApi.request("system.testDbConnection", {});
        setStatus(status, `${result.message}，主机：${result.serverHost}，用户：${result.currentUser}`, "success");
      } catch (error) {
        setStatus(status, `Oracle 连接失败：${error.message}`, "error");
      }
    });
  }

  function setStatus(element, message, state) {
    element.textContent = message;
    element.className = `status-line ${state || ""}`.trim();
  }

  window.studentPages = window.studentPages || {};
  window.studentPages.dashboard = {
    render
  };
})();
