const pageMeta = {
  dashboard: { title: "首页", subtitle: "教师端后台管理" },
  courses: { title: "我的课程", subtitle: "查看当前教师开设的课程和教学班" },
  schedule: { title: "我的课表", subtitle: "按周查看授课安排" },
  students: { title: "选课名单", subtitle: "查看教学班学生名单并导出" },
  scores: { title: "成绩录入", subtitle: "录入和维护学生课程成绩" },
  applications: { title: "开课申请", subtitle: "提交新课程或新教学班申请" },
  password: { title: "修改密码", subtitle: "维护教师账号安全" }
};

const renderers = {
  dashboard: renderDashboard,
  courses: () => renderTablePage("我的课程", [
    ["数据库系统", "数据库系统-01", "64", "48 / 60", "周一 3-4节"],
    ["数据结构", "数据结构-02", "48", "55 / 60", "周三 1-2节"],
    ["软件工程", "软件工程-01", "40", "37 / 50", "周五 5-6节"]
  ]),
  schedule: renderSchedule,
  students: () => renderTablePage("选课名单", [
    ["2024001", "李明", "计算机科学与技术", "数据库系统-01", "已选课"],
    ["2024002", "王雨", "软件工程", "数据库系统-01", "已选课"],
    ["2024003", "陈晨", "人工智能", "数据库系统-01", "已选课"]
  ]),
  scores: renderScores,
  applications: renderApplication,
  password: renderPassword
};

function setActivePage(page) {
  const meta = pageMeta[page] || pageMeta.dashboard;
  document.getElementById("view-title").textContent = meta.title;
  document.getElementById("view-subtitle").textContent = meta.subtitle;

  document.querySelectorAll(".nav-item").forEach((button) => {
    button.classList.toggle("active", button.dataset.page === page);
  });

  const renderer = renderers[page] || renderDashboard;
  document.getElementById("page-root").innerHTML = renderer();
  bindPageActions(page);
}

function renderDashboard() {
  return `
    <section class="dashboard-grid">
      ${metricCard("本学期课程数", "3", "已发布课程")}
      ${metricCard("教学班数量", "5", "含合班授课")}
      ${metricCard("总选课人数", "184", "当前已选人数")}
      ${metricCard("待录入成绩人数", "42", "期末成绩待处理")}
    </section>

    <section class="panel">
      <h3 class="panel-title">系统通信</h3>
      <div class="quick-actions">
        <button class="primary-button" type="button" id="ping-button">测试通信</button>
        <button class="secondary-button" type="button" id="db-button">测试 Oracle 连接</button>
      </div>
      <div class="status-line" id="system-status">等待测试。</div>
    </section>

    <section class="table-panel">
      <table class="data-table">
        <thead>
          <tr>
            <th>待办事项</th>
            <th>课程/教学班</th>
            <th>状态</th>
            <th>更新时间</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>录入期末成绩</td>
            <td>数据库系统-01</td>
            <td>进行中</td>
            <td>今天</td>
          </tr>
          <tr>
            <td>核对选课名单</td>
            <td>数据结构-02</td>
            <td>待处理</td>
            <td>本周</td>
          </tr>
          <tr>
            <td>提交开课申请</td>
            <td>数据仓库技术</td>
            <td>草稿</td>
            <td>本月</td>
          </tr>
        </tbody>
      </table>
    </section>
  `;
}

function metricCard(label, value, note) {
  return `
    <article class="metric-card">
      <div class="metric-label">${label}</div>
      <div class="metric-value">${value}</div>
      <div class="metric-note">${note}</div>
    </article>
  `;
}

function renderTablePage(title, rows) {
  const body = rows.map((row) => `
    <tr>${row.map((item) => `<td>${item}</td>`).join("")}</tr>
  `).join("");

  return `
    <div class="placeholder-grid">
      <section class="table-panel">
        <table class="data-table">
          <thead>
            <tr>
              <th>编号</th>
              <th>名称</th>
              <th>学时/学号</th>
              <th>人数/班级</th>
              <th>状态/时间</th>
            </tr>
          </thead>
          <tbody>${body}</tbody>
        </table>
      </section>
      <aside class="panel">
        <h3 class="panel-title">${title}</h3>
        <p>第一阶段仅展示界面框架，真实数据将在后续通过 C# Service 和 Repository 读取 Oracle。</p>
      </aside>
    </div>
  `;
}

function renderSchedule() {
  return `
    <section class="table-panel">
      <table class="data-table">
        <thead>
          <tr>
            <th>节次</th>
            <th>周一</th>
            <th>周二</th>
            <th>周三</th>
            <th>周四</th>
            <th>周五</th>
          </tr>
        </thead>
        <tbody>
          <tr><td>1-2</td><td></td><td></td><td>数据结构<br>明德楼 202</td><td></td><td></td></tr>
          <tr><td>3-4</td><td>数据库系统<br>致远楼 305</td><td></td><td></td><td></td><td></td></tr>
          <tr><td>5-6</td><td></td><td></td><td></td><td></td><td>软件工程<br>线上</td></tr>
          <tr><td>7-8</td><td></td><td></td><td></td><td></td><td></td></tr>
        </tbody>
      </table>
    </section>
  `;
}

function renderScores() {
  return `
    <section class="panel">
      <h3 class="panel-title">成绩录入</h3>
      <div class="empty-state">成绩录入业务将在下一阶段接入 C# 成绩计算服务。</div>
    </section>
  `;
}

function renderApplication() {
  return `
    <section class="panel">
      <h3 class="panel-title">开课申请</h3>
      <div class="form-grid">
        <div class="field">
          <label>课程名称</label>
          <input type="text" placeholder="例如：数据库系统">
        </div>
        <div class="field">
          <label>课程学分</label>
          <input type="text" placeholder="例如：3.0">
        </div>
        <div class="field">
          <label>总学时</label>
          <input type="text" placeholder="例如：48">
        </div>
        <div class="field">
          <label>教材</label>
          <input type="text" placeholder="例如：数据库系统概论">
        </div>
        <div class="field" style="grid-column: 1 / -1;">
          <label>教学计划</label>
          <textarea placeholder="后续阶段提交到 C# Service，再写入 Oracle。"></textarea>
        </div>
      </div>
    </section>
  `;
}

function renderPassword() {
  return `
    <section class="panel">
      <h3 class="panel-title">修改密码</h3>
      <div class="form-grid">
        <div class="field">
          <label>原密码</label>
          <input type="password">
        </div>
        <div class="field">
          <label>新密码</label>
          <input type="password">
        </div>
      </div>
    </section>
  `;
}

function bindPageActions(page) {
  if (page !== "dashboard") {
    return;
  }

  const status = document.getElementById("system-status");
  const pingButton = document.getElementById("ping-button");
  const dbButton = document.getElementById("db-button");

  pingButton.addEventListener("click", async () => {
    setStatus(status, "正在测试 C# 通信...", "");

    try {
      const result = await window.nativeApi.request("system.ping", {});
      setStatus(status, `C# 通信成功：${result.message}`, "success");
    } catch (error) {
      setStatus(status, `C# 通信失败：${error.message}`, "error");
    }
  });

  dbButton.addEventListener("click", async () => {
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

document.querySelectorAll(".nav-item").forEach((button) => {
  button.addEventListener("click", () => {
    setActivePage(button.dataset.page);
  });
});

document.querySelector(".logout-button").addEventListener("click", () => {
  alert("第一阶段暂未接入登录状态。");
});

setActivePage("dashboard");
