(function () {
  function escapeHtml(value) {
    return String(value ?? "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function row(course) {
    return `
      <tr>
        <td>
          <strong>${escapeHtml(course.courseName)}</strong>
          <div class="metric-note">${escapeHtml(course.description || "暂无简介")}</div>
        </td>
        <td>${escapeHtml(course.className)}</td>
        <td>${escapeHtml(course.semester)}</td>
        <td>${course.credit}</td>
        <td>${course.totalHours}</td>
        <td>${course.capacity}</td>
        <td>${course.selectedCount}</td>
        <td>
          <div class="row-actions">
            <button class="secondary-button js-view-students" type="button" data-class-id="${course.classId}" data-class-name="${escapeHtml(course.className)}">查看学生</button>
            <button class="primary-button js-score-entry" type="button" data-class-id="${course.classId}" data-class-name="${escapeHtml(course.className)}">录入成绩</button>
          </div>
        </td>
      </tr>
    `;
  }

  async function loadCourses(container) {
    const semesterInput = document.getElementById("semester-filter");
    const semester = semesterInput ? semesterInput.value.trim() : "";
    const courses = await window.nativeApi.request("teacher.getMyCourses", { semester });
    const body = document.getElementById("courses-table-body");

    if (!courses || courses.length === 0) {
      body.innerHTML = `
        <tr>
          <td colspan="8">
            <div class="empty-state">暂无课程数据</div>
          </td>
        </tr>
      `;
      return;
    }

    body.innerHTML = courses.map(row).join("");
    container.querySelectorAll(".js-view-students").forEach((button) => {
      button.addEventListener("click", () => {
        window.openTeacherPage("students", {
          classId: Number(button.dataset.classId),
          className: button.dataset.className
        });
      });
    });

    container.querySelectorAll(".js-score-entry").forEach((button) => {
      button.addEventListener("click", () => {
        window.openTeacherPage("scores", {
          classId: Number(button.dataset.classId),
          className: button.dataset.className
        });
      });
    });
  }

  async function render(container) {
    container.innerHTML = `
      <section class="panel">
        <div class="toolbar">
          <div class="field">
            <label for="semester-filter">学期筛选</label>
            <input id="semester-filter" type="text" placeholder="例如：2025-2026-2">
          </div>
          <button class="primary-button" type="button" id="load-courses-button">查询课程</button>
        </div>
      </section>

      <section class="table-panel">
        <table class="data-table">
          <thead>
            <tr>
              <th>课程名称</th>
              <th>教学班名称</th>
              <th>学期</th>
              <th>学分</th>
              <th>学时</th>
              <th>容量</th>
              <th>已选人数</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody id="courses-table-body">
            <tr>
              <td colspan="8">
                <div class="empty-state">正在加载课程数据...</div>
              </td>
            </tr>
          </tbody>
        </table>
      </section>
    `;

    document.getElementById("load-courses-button").addEventListener("click", () => loadCourses(container));
    await loadCourses(container);
  }

  window.teacherPages = window.teacherPages || {};
  window.teacherPages.courses = {
    render
  };
})();
