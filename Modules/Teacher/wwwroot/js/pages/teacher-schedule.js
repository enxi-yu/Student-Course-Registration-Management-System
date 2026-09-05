(function () {
  const escapeHtml = window.sharedUi.escapeHtml;

  function listRow(item) {
    return `
      <tr>
        <td>${escapeHtml(item.courseName)}</td>
        <td>${escapeHtml(item.className)}</td>
        <td>${escapeHtml(window.academicSemester.format(item.semester))}</td>
        <td>${escapeHtml(window.sharedUi.weekdayText(item.weekday))}</td>
        <td>${escapeHtml(window.sharedUi.periodText(item))}</td>
        <td>${escapeHtml(item.weekRange || "-")}</td>
        <td>${escapeHtml(item.classroom || "-")}</td>
        <td>${item.credit}</td>
        <td>${item.totalHours}</td>
      </tr>
    `;
  }

  function renderGrid(schedule) {
    return window.sharedUi.timetable(schedule);
  }

  async function loadSchedule(semesterPicker) {
    const semester = semesterPicker.value();
    const summary = document.getElementById("schedule-summary");

    if (summary) {
      summary.textContent = `当前学期：${semesterPicker.label()}`;
    }

    const root = document.getElementById("schedule-root");
    window.sharedUi.mountState(root, "loading", "正在加载课表数据...");
    let schedule;
    try {
      schedule = await window.nativeApi.request("teacher.getMySchedule", { semester });
    } catch (error) {
      window.sharedUi.mountState(root, "error", `加载课表失败：${error.message}`, () => loadSchedule(semesterPicker));
      return;
    }

    if (!schedule || schedule.length === 0) {
      window.sharedUi.mountState(root, "empty", "当前学期暂无课表安排");
      return;
    }

    root.innerHTML = renderGrid(schedule) + window.sharedUi.dataTable({ columns: ["课程名称", "教学班", "学期", "星期", "节次", "周次", "教室", "学分", "学时"], rows: schedule, row: listRow });
  }

  async function render(container) {
    const selectedSemester = window.academicSemester.getCurrent().canonical;

    const fields = '<div class="field"><label>学年学期</label><div id="schedule-semester-picker"></div></div>';
    const actions = `<span class="term-badge" id="schedule-summary">当前学期：${escapeHtml(window.academicSemester.format(selectedSemester))}</span><button class="primary-button" type="button" id="load-schedule-button">查询课表</button>`;
    container.innerHTML = window.sharedUi.filterBar(fields, actions) + '<div id="schedule-root"></div>';

    const semesterPicker = window.academicSemester.mountPicker("schedule-semester-picker", {
      minimumStartYear: 2024,
      selected: selectedSemester
    });
    document.getElementById("load-schedule-button").addEventListener("click", () => loadSchedule(semesterPicker));
    await loadSchedule(semesterPicker);
  }

  window.teacherPages = window.teacherPages || {};
  window.teacherPages.schedule = {
    render
  };
})();
