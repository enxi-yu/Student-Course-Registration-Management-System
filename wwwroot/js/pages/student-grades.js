(function () {
  function renderGpaCards(gpa) {
    return `
      <div class="gpa-grid">
        <div class="gpa-card">
          <div class="gpa-label">平均绩点</div>
          <div class="gpa-value">${gpa.avgGpa ? gpa.avgGpa.toFixed(2) : "-"}</div>
        </div>
        <div class="gpa-card">
          <div class="gpa-label">已修学分</div>
          <div class="gpa-value">${gpa.totalCreditsFinished ? gpa.totalCreditsFinished.toFixed(1) : "0.0"}</div>
        </div>
        <div class="gpa-card">
          <div class="gpa-label">已修课程数</div>
          <div class="gpa-value">${gpa.totalCourses || 0}</div>
        </div>
      </div>
    `;
  }

  function renderGradesTable(courses) {
    if (!courses || courses.length === 0) {
      return `<div class="empty-state">暂无成绩记录。</div>`;
    }

    const rows = courses.map((item) => `
      <tr>
        <td>${item.courseName}</td>
        <td>${item.semester || "-"}</td>
        <td>${item.credit ? item.credit.toFixed(1) : "-"}</td>
        <td>${item.totalScore != null ? item.totalScore.toFixed(1) : "未录入"}</td>
        <td>${item.gradeLevel || "-"}</td>
        <td>${item.gpa != null ? item.gpa.toFixed(2) : "-"}</td>
      </tr>
    `).join("");

    return `
      <div class="table-panel">
        <table class="data-table">
          <thead>
            <tr>
              <th>课程名称</th>
              <th>学期</th>
              <th>学分</th>
              <th>总评成绩</th>
              <th>等级</th>
              <th>绩点</th>
            </tr>
          </thead>
          <tbody>${rows}</tbody>
        </table>
      </div>
    `;
  }

  async function render(container) {
    container.innerHTML = `
      <section class="panel">
        <h3 class="panel-title">成绩查询</h3>
        <div class="empty-state">正在加载成绩数据...</div>
      </section>
    `;

    let [gpa, courses] = [[], []];
    try {
      [gpa, courses] = await Promise.all([
        window.nativeApi.request("student.getGpa", {}),
        window.nativeApi.request("student.getGrades", {})
      ]);
    } catch (error) {
      container.innerHTML = `
        <section class="panel">
          <h3 class="panel-title">成绩查询</h3>
          <div class="message error">加载成绩数据失败：${error.message}</div>
        </section>
      `;
      return;
    }

    container.innerHTML = `
      <section class="panel">
        <h3 class="panel-title">GPA 总览</h3>
        ${renderGpaCards(gpa)}
      </section>

      <section class="panel">
        <h3 class="panel-title">课程成绩</h3>
        ${renderGradesTable(courses)}
      </section>
    `;
  }

  window.studentPages = window.studentPages || {};
  window.studentPages.grades = {
    render
  };
})();
