(function () {
  const escapeHtml = window.sharedUi.escapeHtml;
  function listRow(item) {
    return `<tr><td>${escapeHtml(item.courseName)}</td><td>${escapeHtml(item.className)}</td><td>${escapeHtml(window.academicSemester.format(item.semester))}</td><td>${escapeHtml(window.sharedUi.weekdayText(item.weekday))}</td><td>${escapeHtml(window.sharedUi.periodText(item))}</td><td>${escapeHtml(item.weekRange || "-")}</td><td>${escapeHtml(item.classroom || "-")}</td><td>${item.credit}</td><td>${item.totalHours}</td></tr>`;
  }
  function renderGrid(schedule) {
    return window.sharedUi.timetable(schedule, { clickable: true, cardClass: "student-schedule-card" });
  }
  function bindDetails(container) {
    container.querySelectorAll(".student-schedule-card").forEach(card => card.addEventListener("click", () => window.openStudentPage("detail", { classId: Number(card.dataset.classId) })));
  }
  async function loadSchedule(container, picker) {
    document.getElementById("student-schedule-summary").textContent = `当前学期：${picker.label()}`;
    const root = document.getElementById("student-schedule-root");
    window.sharedUi.mountState(root, "loading", "正在加载课表数据...");
    try {
      const schedule = await window.nativeApi.request("student.getSchedule", { semester: picker.value() });
      if (!schedule || !schedule.length) { window.sharedUi.mountState(root, "empty", "当前学期暂无课表安排"); return; }
      root.innerHTML = renderGrid(schedule) + window.sharedUi.dataTable({ columns: ["课程名称", "教学班", "学期", "星期", "节次", "周次", "教室", "学分", "学时"], rows: schedule, row: listRow });
      bindDetails(container);
    } catch (error) { window.sharedUi.mountState(root, "error", `加载课表失败：${error.message}`, () => loadSchedule(container, picker)); }
  }
  async function render(container) {
    const selected = window.academicSemester.getCurrent().canonical;
    const fields = '<div class="field"><label>学年学期</label><div id="student-schedule-semester-picker"></div></div>';
    const actions = `<span class="term-badge" id="student-schedule-summary">当前学期：${escapeHtml(window.academicSemester.format(selected))}</span><button class="primary-button" type="button" id="student-load-schedule-button">查询课表</button>`;
    container.innerHTML = window.sharedUi.filterBar(fields, actions) + '<div id="student-schedule-root"></div>';
    window.sharedUi.mountState(document.getElementById("student-schedule-root"), "loading", "正在加载课表数据...");
    const picker = window.academicSemester.mountPicker("student-schedule-semester-picker", { minimumStartYear: 2024, selected });
    document.getElementById("student-load-schedule-button").addEventListener("click", () => loadSchedule(container, picker));
    await loadSchedule(container, picker);
  }
  window.studentPages = window.studentPages || {};
  window.studentPages.schedule = { render };
})();
