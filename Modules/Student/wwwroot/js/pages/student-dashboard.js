(function () {
  let dashboardElements;
  function numberValue(value) {
    return Number.isFinite(Number(value)) ? Number(value) : 0;
  }

  function formatNumber(value, digits) {
    const number = Number(value);
    return Number.isFinite(number) ? number.toFixed(digits) : "-";
  }

  async function loadDashboard(semester, displaySemester) {
    const data = await window.nativeApi.request("student.getDashboard", { semester });
    const profile = data.profile || {};
    const gpa = data.gpaSummary || {};

    window.sharedUi.updateDashboard(dashboardElements, {
      metrics: [
        { label: "课程数量", value: numberValue(data.currentSemesterCourseCount), note: `${displaySemester} 已选课程数`, tone: "blue" },
        { label: "学分统计", value: formatNumber(data.currentSemesterCredit, 1), note: `${displaySemester} 课程总学分`, tone: "cyan" },
        { label: "平均绩点", value: formatNumber(gpa.avgGpa, 2), note: "累计 GPA", tone: "green" },
        { label: "已修学分", value: formatNumber(gpa.totalCreditsFinished, 1), note: "累计完成学分", tone: "amber" }
      ],
      profile: [
        { label: "学生姓名", value: profile.realName }, { label: "学生学号", value: profile.studentNo },
        { label: "所属专业", value: profile.major }, { label: "所在年级", value: profile.grade }
      ]
    });

  }

  async function render(container) {
    const currentSemester = window.academicSemester.getCurrent().canonical;
    const filterHtml = '<section class="panel dashboard-filter-panel"><div class="toolbar"><div class="field"><label>学年学期</label><div id="student-semester-picker"></div></div><div class="toolbar-actions"><button class="primary-button" type="button" id="student-dashboard-refresh">刷新统计</button></div></div></section>';
    dashboardElements = window.sharedUi.renderDashboardShell(container, {
      prefix: "dashboard", heroTitle: "学生学习中心", filterHtml,
      heroActions: [{ label: "进入选课中心", target: "courses", style: "primary-button" }, { label: "查看我的课表", target: "schedule", style: "secondary-button" }],
      quickActions: [{ label: "选课中心", target: "courses" }, { label: "我的课表", target: "schedule" }, { label: "成绩查询", target: "grades" }, { label: "课程评价", target: "evaluation" }],
      onNavigate: target => window.openStudentPage(target)
    });

    let picker;
    const refresh = async () => {
      window.sharedUi.updateDashboard(dashboardElements, { metrics: [{ label: "首页数据", value: "…", note: "正在加载", tone: "blue" }], profile: [] });
      try { await loadDashboard(picker.value(), picker.label()); }
      catch (error) { window.sharedUi.mountState(dashboardElements.metrics, "error", `首页数据加载失败：${error.message}`, refresh); }
    };
    picker = window.academicSemester.mountPicker("student-semester-picker", {
      minimumStartYear: 2024,
      selected: currentSemester
    });
    document.getElementById("student-dashboard-refresh").addEventListener("click", refresh);
    await refresh();
  }

  window.studentPages = window.studentPages || {};
  window.studentPages.dashboard = { render };
})();
