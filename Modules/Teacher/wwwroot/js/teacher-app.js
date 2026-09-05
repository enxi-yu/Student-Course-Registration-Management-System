(function () {
  const pageMeta = {
    dashboard: { title: "首页" },
    courses: { title: "我的课程", description: "查看当前教师自己的课程和教学班" },
    schedule: { title: "我的课表", description: "查看本学期授课安排和上课地点。" },
    students: { title: "选课名单", description: "查看当前教学班的选课学生信息，并支持导出名单。" },
    scores: { title: "成绩录入", description: "录入和维护学生课程成绩，系统自动生成成绩等级。" },
    applications: { title: "开课申请", description: "提交和查看教师开课申请" },
    evaluations: { title: "课程评价", description: "查看本人教学班的匿名评价汇总与文字反馈。" },
    password: { title: "个人资料", description: "查看个人资料，维护联系方式和账号安全。" }
  };

  const state = {
    teacher: null,
    isAuthorized: false,
    pageOptions: {}
  };

  function setMessage(message, type) {
    const root = document.getElementById("message-root");
    if (!message) {
      root.innerHTML = "";
      return;
    }

    root.innerHTML = `<div class="message ${type || ""}">${message}</div>`;
  }

  function normalizeAccessMessage(message) {
    const text = message || "";
    if (text.indexOf("当前账号不是教师") >= 0) {
      return "当前账号不是教师，无权访问教师端";
    }

    if (text.indexOf("请先登录") >= 0) {
      return "请先登录后查看教师端信息。";
    }

    return text || "请先登录后查看教师端信息。";
  }

  function updateTeacherSummary(teacher) {
    const summary = document.getElementById("teacher-summary");
    if (!teacher) {
      summary.textContent = "未登录";
      return;
    }

    summary.textContent = `${teacher.teacherName || "-"} / ${teacher.teacherNo || "-"}`;
  }

  function setActiveNav(page) {
    const activePage = page === "students" || page === "scores" ? "courses" : page;
    document.querySelectorAll(".nav-item").forEach((button) => {
      button.classList.toggle("active", button.dataset.page === activePage);
    });
  }

  function setPageMeta(page) {
    const meta = pageMeta[page] || pageMeta.dashboard;
    document.getElementById("view-title").textContent = meta.title;
    document.getElementById("view-description").textContent = meta.description;
    const heading = document.getElementById("content-heading");
    if (heading) heading.hidden = page === "dashboard";
  }

  function renderAccessMessage(message) {
    state.isAuthorized = false;
    updateTeacherSummary(null);
    setMessage("", "");
    setPageMeta("dashboard");
    setActiveNav("dashboard");

    const title = normalizeAccessMessage(message);
    document.getElementById("page-root").innerHTML = `
      <section class="panel">
        <h3 class="panel-title">${title}</h3>
        <p class="empty-state">请返回统一登录页面，使用教师账号登录后进入教师端。</p>
      </section>
    `;
  }

  async function openPage(page, options) {
    if (!state.isAuthorized) {
      renderAccessMessage("请先登录");
      return;
    }

    state.pageOptions = options || {};
    setMessage("", "");
    setActiveNav(page);
    setPageMeta(page);

    const container = document.getElementById("page-root");
    try {
      if (page === "dashboard") {
        await window.teacherPages.dashboard.render(container);
        return;
      }

      if (page === "courses") {
        await window.teacherPages.courses.render(container);
        return;
      }

      if (page === "schedule") {
        await window.teacherPages.schedule.render(container);
        return;
      }

      if (page === "students") {
        await window.teacherPages.students.render(container, state.pageOptions);
        return;
      }

      if (page === "scores") {
        await window.teacherPages.scores.render(container, state.pageOptions);
        return;
      }

      if (page === "applications") {
        await window.teacherPages.applications.render(container);
        return;
      }

      if (page === "evaluations") {
        await window.teacherPages.evaluations.render(container);
        return;
      }

      if (page === "password") {
        await window.teacherPages.password.render(container);
        return;
      }

      throw new Error("页面不存在");
    } catch (error) {
      setMessage(error.message, "error");
    }
  }

  async function initializeTeacherPage() {
    document.getElementById("page-root").innerHTML = `
      <section class="panel">
        <h3 class="panel-title">正在进入教师端...</h3>
        <div class="empty-state">正在读取当前登录状态</div>
      </section>
    `;

    try {
      const teacher = await window.nativeApi.request("teacher.getCurrentTeacher", {});
      state.teacher = teacher;
      state.isAuthorized = true;
      updateTeacherSummary(teacher);
      await openPage("dashboard");
    } catch (error) {
      renderAccessMessage(normalizeAccessMessage(error.message));
    }
  }

  document.querySelectorAll(".nav-item").forEach((button) => {
    button.addEventListener("click", () => openPage(button.dataset.page));
  });

  document.getElementById("logout-button").addEventListener("click", async () => {
    try {
      await window.nativeApi.request("app.logout", {});
      window.location.replace("http://localhost:5100/Login");
      renderAccessMessage("请先登录");
    } catch (error) {
      setMessage(error.message, "error");
    }
  });

  window.openTeacherPage = openPage;
  initializeTeacherPage();
})();
