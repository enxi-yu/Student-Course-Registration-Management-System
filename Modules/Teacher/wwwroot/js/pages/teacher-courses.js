(function () {
  const escapeHtml = window.sharedUi.escapeHtml;

  function row(course) {
    return `
      <tr>
        <td>
          <strong>${escapeHtml(course.courseName)}</strong>
          <div class="metric-note">${escapeHtml(course.description || "暂无简介")}</div>
        </td>
        <td>${escapeHtml(course.className)}</td>
        <td>${escapeHtml(window.academicSemester.format(course.semester))}</td>
        <td>${course.credit}</td>
        <td>${course.capacity}</td>
        <td>${course.selectedCount}</td>
        <td>
          <div class="row-actions">
            <button class="secondary-button js-view-students" type="button" data-class-id="${course.classId}" data-class-name="${escapeHtml(course.className)}" data-course-name="${escapeHtml(course.courseName)}">查看学生</button>
            <button class="primary-button js-score-entry" type="button" data-class-id="${course.classId}" data-class-name="${escapeHtml(course.className)}" data-course-name="${escapeHtml(course.courseName)}">录入成绩</button>
          </div>
        </td>
      </tr>
    `;
  }

  async function loadCourses(container, semesterPicker) {
    const semester = semesterPicker.value();
    const courses = await window.nativeApi.request("teacher.getMyCourses", { semester });
    const body = document.getElementById("courses-table-body");
    const summary = document.getElementById("courses-summary");

    if (summary) {
      summary.textContent = `当前学期：${semesterPicker.label()}`;
    }

    if (!courses || courses.length === 0) {
      body.innerHTML = window.sharedUi.emptyRow(7, "当前学期暂无课程数据");
      return;
    }

    window.sharedUi.renderTableRows(body, courses, row, 7, "暂无课程");
    container.querySelectorAll(".js-view-students").forEach((button) => {
      button.addEventListener("click", () => {
        window.openTeacherPage("students", {
          classId: Number(button.dataset.classId),
          className: button.dataset.className,
          courseName: button.dataset.courseName
        });
      });
    });

    container.querySelectorAll(".js-score-entry").forEach((button) => {
      button.addEventListener("click", () => {
        window.openTeacherPage("scores", {
          classId: Number(button.dataset.classId),
          className: button.dataset.className,
          courseName: button.dataset.courseName
        });
      });
    });
  }

  async function render(container) {
    const selectedSemester = window.academicSemester.getCurrent().canonical;

    const filterFields = `
          <div class="field">
            <label>学年学期</label>
            <div id="courses-semester-picker"></div>
          </div>`;
    const filterActions = `
            <span class="term-badge" id="courses-summary">当前学期：${escapeHtml(window.academicSemester.format(selectedSemester))}</span>
            <button class="primary-button" type="button" id="load-courses-button">查询课程</button>`;
    container.innerHTML = window.sharedUi.filterBar(filterFields, filterActions) + window.sharedUi.dataTable({
      columns: ["课程名称", "教学班名称", "学期", "学分", "容量", "已选人数", "操作"],
      bodyId: "courses-table-body",
      emptyText: "正在加载课程数据..."
    });

    const semesterPicker = window.academicSemester.mountPicker("courses-semester-picker", {
      minimumStartYear: 2024,
      selected: selectedSemester
    });
    document.getElementById("load-courses-button").addEventListener("click", () => loadCourses(container, semesterPicker));
    await loadCourses(container, semesterPicker);
  }

  window.teacherPages = window.teacherPages || {};
  window.teacherPages.courses = {
    render
  };
})();
