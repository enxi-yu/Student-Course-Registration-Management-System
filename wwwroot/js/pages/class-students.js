(function () {
  function escapeHtml(value) {
    return String(value ?? "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function row(student) {
    return `
      <tr>
        <td>${escapeHtml(student.studentNo)}</td>
        <td>${escapeHtml(student.studentName)}</td>
        <td>${escapeHtml(student.major)}</td>
        <td>${escapeHtml(student.grade)}</td>
        <td>${escapeHtml(student.selectTime || "-")}</td>
      </tr>
    `;
  }

  async function loadStudents(container, classId) {
    const body = document.getElementById("students-table-body");
    body.innerHTML = `
      <tr>
        <td colspan="5"><div class="empty-state">正在加载学生名单...</div></td>
      </tr>
    `;

    const students = await window.nativeApi.request("teacher.getClassStudents", { classId });
    if (!students || students.length === 0) {
      body.innerHTML = `
        <tr>
          <td colspan="5"><div class="empty-state">暂无学生选课</div></td>
        </tr>
      `;
      return;
    }

    body.innerHTML = students.map(row).join("");
  }

  async function render(container, options) {
    const classId = Number(options && options.classId);
    const className = options && options.className ? options.className : `教学班 ${classId || "-"}`;

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
            <h3 class="panel-title">${escapeHtml(className)}</h3>
            <p class="metric-note">教学班编号：${classId}</p>
          </div>
          <button class="primary-button" type="button" id="export-students-button">导出 CSV</button>
        </div>
      </section>

      <section class="table-panel">
        <table class="data-table">
          <thead>
            <tr>
              <th>学号</th>
              <th>姓名</th>
              <th>专业</th>
              <th>年级</th>
              <th>选课时间</th>
            </tr>
          </thead>
          <tbody id="students-table-body"></tbody>
        </table>
      </section>
    `;

    document.getElementById("export-students-button").addEventListener("click", async () => {
      try {
        const result = await window.nativeApi.request("teacher.exportClassStudentsCsv", { classId });
        alert(result.path ? `导出成功：${result.path}` : result.message);
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
