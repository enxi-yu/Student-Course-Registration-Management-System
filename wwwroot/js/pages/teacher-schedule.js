(function () {
  const weekdayNames = ["", "周一", "周二", "周三", "周四", "周五", "周六", "周日"];
  const periodRows = [
    { start: 1, end: 2, label: "1-2节" },
    { start: 3, end: 4, label: "3-4节" },
    { start: 5, end: 6, label: "5-6节" },
    { start: 7, end: 8, label: "7-8节" },
    { start: 9, end: 10, label: "9-10节" },
    { start: 11, end: 12, label: "11-12节" }
  ];

  function escapeHtml(value) {
    return String(value ?? "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function periodText(item) {
    if (!item.startPeriod || !item.endPeriod) {
      return "-";
    }

    return `${item.startPeriod}-${item.endPeriod}节`;
  }

  function weekdayText(value) {
    return weekdayNames[Number(value)] || `周${value}`;
  }

  function scheduleCard(item) {
    return `
      <div class="schedule-card">
        <strong>${escapeHtml(item.courseName)}</strong>
        <span>${escapeHtml(item.className)}</span>
        <span>${escapeHtml(item.weekRange || "全周")} · ${escapeHtml(item.classroom || "未安排教室")}</span>
      </div>
    `;
  }

  function listRow(item) {
    return `
      <tr>
        <td>${escapeHtml(item.courseName)}</td>
        <td>${escapeHtml(item.className)}</td>
        <td>${escapeHtml(item.semester)}</td>
        <td>${escapeHtml(weekdayText(item.weekday))}</td>
        <td>${escapeHtml(periodText(item))}</td>
        <td>${escapeHtml(item.weekRange || "-")}</td>
        <td>${escapeHtml(item.classroom || "-")}</td>
        <td>${item.credit}</td>
        <td>${item.totalHours}</td>
      </tr>
    `;
  }

  function renderGrid(schedule) {
    const bySlot = new Map();
    schedule.forEach((item) => {
      const key = `${item.weekday}-${item.startPeriod}-${item.endPeriod}`;
      if (!bySlot.has(key)) {
        bySlot.set(key, []);
      }

      bySlot.get(key).push(item);
    });

    const rows = periodRows.map((period) => {
      const cells = [1, 2, 3, 4, 5, 6, 7].map((weekday) => {
        const key = `${weekday}-${period.start}-${period.end}`;
        const items = bySlot.get(key) || [];
        return `<td>${items.map(scheduleCard).join("")}</td>`;
      }).join("");

      return `<tr><th>${period.label}</th>${cells}</tr>`;
    }).join("");

    return `
      <section class="table-panel schedule-grid-panel">
        <table class="data-table schedule-grid">
          <thead>
            <tr>
              <th>节次</th>
              <th>周一</th>
              <th>周二</th>
              <th>周三</th>
              <th>周四</th>
              <th>周五</th>
              <th>周六</th>
              <th>周日</th>
            </tr>
          </thead>
          <tbody>${rows}</tbody>
        </table>
      </section>
    `;
  }

  async function loadSchedule(container) {
    const semesterInput = document.getElementById("schedule-semester-filter");
    const semester = semesterInput ? semesterInput.value.trim() : "";
    const schedule = await window.nativeApi.request("teacher.getMySchedule", { semester });

    if (!schedule || schedule.length === 0) {
      document.getElementById("schedule-root").innerHTML = `
        <section class="panel">
          <div class="empty-state">暂无课表数据</div>
        </section>
      `;
      return;
    }

    document.getElementById("schedule-root").innerHTML = `
      ${renderGrid(schedule)}
      <section class="table-panel">
        <table class="data-table">
          <thead>
            <tr>
              <th>课程名称</th>
              <th>教学班</th>
              <th>学期</th>
              <th>星期</th>
              <th>节次</th>
              <th>周次</th>
              <th>教室</th>
              <th>学分</th>
              <th>学时</th>
            </tr>
          </thead>
          <tbody>${schedule.map(listRow).join("")}</tbody>
        </table>
      </section>
    `;
  }

  async function render(container) {
    container.innerHTML = `
      <section class="panel">
        <div class="toolbar">
          <div class="field">
            <label for="schedule-semester-filter">学期筛选</label>
            <input id="schedule-semester-filter" type="text" placeholder="例如：2025-2026-2">
          </div>
          <button class="primary-button" type="button" id="load-schedule-button">查询课表</button>
        </div>
      </section>
      <div id="schedule-root">
        <section class="panel">
          <div class="empty-state">正在加载课表数据...</div>
        </section>
      </div>
    `;

    document.getElementById("load-schedule-button").addEventListener("click", () => loadSchedule(container));
    await loadSchedule(container);
  }

  window.teacherPages = window.teacherPages || {};
  window.teacherPages.schedule = {
    render
  };
})();
