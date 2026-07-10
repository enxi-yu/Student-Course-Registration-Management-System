(function () {
  function escapeHtml(value) {
    return String(value ?? "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function uniqueSemesters(items) {
    return Array.from(new Set((items || [])
      .map((item) => item.semester)
      .filter((semester) => semester && String(semester).trim())))
      .sort((left, right) => String(right).localeCompare(String(left)));
  }

  function semesterOptions(semesters, selected) {
    if (!semesters.length) {
      return '<option value="">暂无学期数据</option>';
    }

    return semesters.map((semester) => {
      const value = escapeHtml(semester);
      const isSelected = semester === selected ? " selected" : "";
      return `<option value="${value}"${isSelected}>${value}</option>`;
    }).join("");
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

  async function getAllCourses() {
    return await window.nativeApi.request("teacher.getMyCourses", { semester: "" });
  }

  async function loadCourses(container) {
    const semesterSelect = document.getElementById("semester-filter");
    const semester = semesterSelect ? semesterSelect.value : "";
    const courses = await window.nativeApi.request("teacher.getMyCourses", { semester });
    const body = document.getElementById("courses-table-body");
    const summary = document.getElementById("courses-summary");

    if (summary) {
      summary.textContent = semester ? `当前学期：${semester}` : "当前学期：未选择";
    }

    if (!courses || courses.length === 0) {
      body.innerHTML = `
        <tr>
          <td colspan="8">
            <div class="empty-state">当前学期暂无课程数据</div>
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
        <div class="empty-state">正在读取课程学期...</div>
      </section>
    `;

    const allCourses = await getAllCourses();
    const semesters = uniqueSemesters(allCourses);
    const selectedSemester = semesters[0] || "";

    container.innerHTML = `
      <section class="panel">
        <div class="toolbar">
          <div class="field">
            <label for="semester-filter">学期筛选</label>
            <select id="semester-filter" ${semesters.length ? "" : "disabled"}>
              ${semesterOptions(semesters, selectedSemester)}
            </select>
          </div>
          <div class="toolbar-actions">
            <span class="term-badge" id="courses-summary">当前学期：${escapeHtml(selectedSemester || "未选择")}</span>
            <button class="primary-button" type="button" id="load-courses-button">查询课程</button>
          </div>
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
    document.getElementById("semester-filter").addEventListener("change", () => loadCourses(container));
    await loadCourses(container);
  }

  window.teacherPages = window.teacherPages || {};
  window.teacherPages.courses = {
    render
  };
})();