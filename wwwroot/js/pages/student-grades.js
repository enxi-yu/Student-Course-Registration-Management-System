(function () {
  function formatNumber(value, digits, emptyText) {
    if (value === null || value === undefined || value === "") {
      return emptyText || "-";
    }

    const number = Number(value);
    return Number.isFinite(number) ? number.toFixed(digits) : (emptyText || "-");
  }

  function escapeHtml(value) {
    return String(value === null || value === undefined ? "" : value)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

function renderGpaCards(gpa) {
    return `
    <div class="gpa-grid">
    <div class="gpa-card">
        <div class="gpa-label">总平均绩点</div>
        <div class="gpa-value" style="color: #2563eb;">${formatNumber(gpa.avgGpa, 2)}</div>
    </div>
    <div class="gpa-card">
        <div class="gpa-label">总已修学分</div>
        <div class="gpa-value">${formatNumber(gpa.totalCreditsFinished, 1, "0.0")}</div>
    </div>
    <div class="gpa-card">
        <div class="gpa-label">必修学分</div>
        <div class="gpa-value" style="color: #16a34a;">${formatNumber(gpa.mandatoryCreditsFinished, 1, "0.0")}</div>
    </div>
    <div class="gpa-card">
        <div class="gpa-label">选修/公选学分</div>
        <div class="gpa-value" style="color: #ea580c;">${formatNumber(gpa.electiveCreditsFinished, 1, "0.0")}</div>
    </div>
    </div>
`;
}

  function groupCoursesBySemester(courses) {
    const groups = new Map();

    (courses || []).forEach((item) => {
      const semester = item.semester || "未标注学期";
      if (!groups.has(semester)) {
        groups.set(semester, []);
      }

      groups.get(semester).push(item);
    });

    return Array.from(groups.entries()).map(([semester, items]) => ({ semester, items }));
  }

  function findSemesterSummary(gpa, semester) {
    return (gpa.semesterGpas || []).find((item) => item.semester === semester) || {};
  }

  function renderSemesterTable(group, gpa, index) {
    const summary = findSemesterSummary(gpa, group.semester);
    const rows = group.items.map((item, rowIndex) => {
      const isGraded = item.totalScore != null;

      let passBadge = "";
      if (!isGraded) {
          passBadge = `<span class="pass-badge" style="background: #f1f5f9; color: #94a3b8;">-</span>`;
      } else {
          passBadge = `<span class="pass-badge ${item.isPassed ? "passed" : "pending"}" ${!item.isPassed ? 'style="background: #fee2e2; color: #ef4444;"' : ''}>${item.isPassed ? "是" : "否"}</span>`;
      }

      return `
      <tr>
        <td>${rowIndex + 1}</td>
        <td>${escapeHtml(item.courseCode || "-")}</td>
        <td>${escapeHtml(item.courseName || "-")}</td>
        <td>${formatNumber(item.credit, 1)}</td>
        <td>${item.gpa != null ? formatNumber(item.gpa, 2) : "-"}</td>
        <td>${item.totalScore != null ? formatNumber(item.totalScore, 1) : escapeHtml(item.gradeLevel || "未录入")}</td>
        <td>${passBadge}</td>
      </tr>
    `;
    }).join("");

    return `
      <section class="panel semester-grade-panel">
        <div class="semester-grade-header">
          <h3 class="panel-title">${escapeHtml(group.semester)}</h3>
          <div class="semester-grade-summary">
            <span>本学期平均绩点：${formatNumber(summary.avgGpa, 2)}</span>
            <span>学分：${formatNumber(summary.credits, 1, "0.0")}</span>
            <span>课程数：${summary.courses || group.items.length}</span>
          </div>
        </div>
        <div class="table-panel">
          <table class="data-table grade-table">
            <thead>
              <tr>
                <th>序号</th>
                <th>课程编码</th>
                <th>课程名称</th>
                <th>学分</th>
                <th>绩点</th>
                <th>成绩</th>
                <th>是否通过</th>
              </tr>
            </thead>
            <tbody>${rows}</tbody>
          </table>
        </div>
      </section>
    `;
  }

  function renderSemesterGrades(gpa, courses) {
    const groups = groupCoursesBySemester(courses);
    if (groups.length === 0) {
      return `<section class="panel"><div class="empty-state">暂无成绩记录。</div></section>`;
    }

    return groups.map((group, index) => renderSemesterTable(group, gpa, index)).join("");
  }

  async function render(container) {
    container.innerHTML = `
      <section class="panel">
        <h3 class="panel-title">成绩查询</h3>
        <div class="empty-state">正在加载成绩数据...</div>
      </section>
    `;

    let gpa = {};
    let courses = [];
    try {
      [gpa, courses] = await Promise.all([
        window.nativeApi.request("student.getGpa", {}),
        window.nativeApi.request("student.getGrades", {})
      ]);
    } catch (error) {
      container.innerHTML = `
        <section class="panel">
          <h3 class="panel-title">成绩查询</h3>
          <div class="message error">加载成绩数据失败：${escapeHtml(error.message)}</div>
        </section>
      `;
      return;
    }

    container.innerHTML = `
      <section class="panel">
        <h3 class="panel-title">GPA 总览</h3>
        ${renderGpaCards(gpa || {})}
      </section>
      ${renderSemesterGrades(gpa || {}, courses || [])}
    `;
  }

  window.studentPages = window.studentPages || {};
  window.studentPages.grades = {
    render
  };
})();
