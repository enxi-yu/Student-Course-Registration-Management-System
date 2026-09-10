(function () {
  const escapeHtml = window.sharedUi.escapeHtml;

  function row(student) {
    return `
      <tr>
        <td>${escapeHtml(student.studentNo)}</td>
        <td>${escapeHtml(student.studentName)}</td>
        <td>${escapeHtml(student.major)}</td>
        <td>${escapeHtml(student.grade)}</td>
      </tr>
    `;
  }

  async function loadStudents(container, classId) {
    const body = document.getElementById("students-table-body");
    window.sharedUi.setTableState(body, 4, "正在加载学生名单...");
    let students;
    try {
      students = await window.nativeApi.request("teacher.getClassStudents", { classId });
    } catch (error) {
      window.sharedUi.setTableError(body, 4, `加载学生名单失败：${error.message}`, () => loadStudents(container, classId));
      return;
    }
    window.sharedUi.renderTableRows(body, students, row, 4, "暂无学生选课");
  }

  async function downloadStudentsExcel(classId) {
    const url = `/api/teacher/classes/${encodeURIComponent(classId)}/students/export`;
    const response = await fetch(url, {
      method: "GET",
      credentials: "include"
    });

    if (!response.ok) {
      const contentType = response.headers.get("content-type") || "";
      const data = contentType.includes("application/json")
        ? await response.json()
        : await response.text();
      const message = data && data.message ? data.message : String(data || "导出名单失败");
      throw new Error(message);
    }

    const blob = await response.blob();
    const objectUrl = window.URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = objectUrl;
    link.download = `class_students_${classId}.xlsx`;
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(objectUrl);
  }

  async function render(container, options) {
    const classId = Number(options && options.classId);
    const className = options && options.className ? options.className : `教学班 ${classId || "-"}`;
    const courseName = options && options.courseName ? options.courseName : "";

    if (!classId) {
      container.innerHTML = `
        <section class="panel">
          <h3 class="panel-title">选课名单</h3>
          <div class="empty-state">请先从“我的课程”页面选择一个教学班。</div>
        </section>
      `;
      return;
    }

    container.innerHTML = `
      <section class="panel">
        <div class="toolbar">
          <div>
            <h3 class="panel-title">${escapeHtml(courseName || "选课名单")}</h3>
            <p class="metric-note">教学班：${escapeHtml(className)} · 教学班编号：${classId}</p>
          </div>
          <div class="toolbar-actions">
            <button class="secondary-button table-action" type="button" id="back-to-courses-button">返回我的课程</button>
            <button class="primary-button table-action" type="button" id="export-students-button">导出名单</button>
          </div>
        </div>
      </section>

      <section class="table-panel"><div class="table-scroll">
        <table class="data-table">
          <thead>
            <tr>
              <th>学号</th>
              <th>姓名</th>
              <th>专业</th>
              <th>年级</th>
            </tr>
          </thead>
          <tbody id="students-table-body"></tbody>
        </table></div>
      </section>
    `;

    document.getElementById("back-to-courses-button").addEventListener("click", () => window.openTeacherPage("courses"));
    document.getElementById("export-students-button").addEventListener("click", async () => {
      try {
        await downloadStudentsExcel(classId);
      } catch (error) {
        alert(error.message);
      }
    });

    await loadStudents(container, classId);
  }

  window.teacherPages = window.teacherPages || {};
  window.teacherPages.students = {
    render
  };
})();
