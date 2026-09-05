(function () {
  let dashboardElements;

  function numberValue(value) {
    return Number.isFinite(Number(value)) ? Number(value) : 0;
  }

  async function loadDashboard(container, semester, displaySemester) {
    const data = await window.nativeApi.request("teacher.getDashboard", { semester });
    const pendingScoreCount = numberValue(data.pendingScoreCount);
    const courseCount = numberValue(data.courseCount);
    const classCount = numberValue(data.classCount);
    const studentCount = numberValue(data.studentCount);

    window.sharedUi.updateDashboard(dashboardElements, {
      metrics: [
        { label: "课程数量", value: courseCount, note: `${displaySemester} 课程数`, tone: "blue" },
        { label: "教学班", value: classCount, note: `${displaySemester} 教学班`, tone: "cyan" },
        { label: "选课学生", value: studentCount, note: `${displaySemester} 已选人数`, tone: "green" },
        { label: "待录成绩", value: pendingScoreCount, note: pendingScoreCount > 0 ? "建议优先处理" : "暂无待处理", tone: "amber" }
      ],
      profile: [
        { label: "教师姓名", value: data.teacherName }, { label: "教师工号", value: data.teacherNo },
        { label: "职称", value: data.title }, { label: "所属院系", value: data.department }
      ]
    });

  }

  async function render(container) {
    const currentSemester = window.academicSemester.getCurrent().canonical;

    const filterHtml = '<section class="panel dashboard-filter-panel"><div class="toolbar"><div class="field"><label>学年学期</label><div id="teacher-semester-picker"></div></div><div class="toolbar-actions"><button class="primary-button" type="button" id="load-dashboard-button">刷新统计</button></div></div></section>';
    dashboardElements = window.sharedUi.renderDashboardShell(container, {
      prefix: "dashboard", heroTitle: "教师工作台", filterHtml,
      heroActions: [{ label: "查看我的课程", target: "courses", style: "primary-button" }, { label: "提交开课申请", target: "applications", style: "secondary-button" }],
      quickActions: [{ label: "我的课程", target: "courses" }, { label: "我的课表", target: "schedule" }, { label: "开课申请", target: "applications" }, { label: "课程评价", target: "evaluations" }],
      onNavigate: target => window.openTeacherPage(target)
    });

    let picker;
    const refresh = async () => {
      window.sharedUi.updateDashboard(dashboardElements, { metrics: [{ label: "首页数据", value: "…", note: "正在加载", tone: "blue" }], profile: [] });
      try { await loadDashboard(container, picker.value(), picker.label()); }
      catch (error) { window.sharedUi.mountState(dashboardElements.metrics, "error", `首页数据加载失败：${error.message}`, refresh); }
    };
    picker = window.academicSemester.mountPicker("teacher-semester-picker", {
      minimumStartYear: 2024,
      selected: currentSemester
    });
    document.getElementById("load-dashboard-button").addEventListener("click", refresh);
    await refresh();
  }

  window.teacherPages = window.teacherPages || {};
  window.teacherPages.dashboard = {
    render
  };
})();
