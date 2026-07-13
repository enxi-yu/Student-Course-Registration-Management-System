(function () {
  const pageMeta = {
    dashboard: { title: "首页" },
    courses: { title: "我的课程", description: "查看当前教师自己的课程和教学班" },
    schedule: { title: "我的课表", description: "查看当前教师本学期课程安排" },
    students: { title: "选课名单", description: "查看教学班学生名单和选课信息" },
    scores: { title: "成绩录入", description: "录入并维护学生课程成绩" },
    applications: { title: "开课申请", description: "提交和查看教师开课申请" },
    password: { title: "修改密码", description: "修改当前账号登录密码" }
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
      return "请先登录";
    }

    return text || "请先登录";
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
    document.querySelectorAll(".nav-item").forEach((button) => {
      button.classList.toggle("active", button.dataset.page === page);
    });
  }

  function setPageMeta(page) {
    const meta = pageMeta[page] || pageMeta.dashboard;
    document.getElementById("view-title").textContent = meta.title;
    document.getElementById("view-description").textContent = meta.description;
    document.getElementById("view-subtitle").textContent = meta.description;
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
        <p>教师端不提供独立登录入口。请通过公共登录入口登录后进入教师端。</p>
        <div class="dev-panel">
          <button class="primary-button" type="button" id="dev-teacher-button">开发测试：使用教师 Mock Session</button>
          <button class="ghost-button" type="button" id="dev-student-button">开发测试：使用非教师 Mock Session</button>
        </div>
      </section>
    `;

    document.getElementById("dev-teacher-button").addEventListener("click", async () => {
      await window.nativeApi.request("app.useMockTeacherSession", {});
      await initializeTeacherPage();
    });

    document.getElementById("dev-student-button").addEventListener("click", async () => {
      await window.nativeApi.request("app.useMockStudentSession", {});
      await initializeTeacherPage();
    });
  }

  function renderThirdPhase(container, page) {
    const meta = pageMeta[page] || { title: "功能", description: "功能正在完善中" };
    container.innerHTML = `
      <section class="panel">
        <h3 class="panel-title">${meta.title}</h3>
        <div class="empty-state">${meta.description}</div>
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

      if (page === "password") {
        await window.teacherPages.password.render(container);
        return;
      }

      renderThirdPhase(container, page);
    } catch (error) {
      setMessage(error.message, "error");
    }
  }

  async function initializeTeacherPage() {
    document.getElementById("page-root").innerHTML = `
      <section class="panel">
        <h3 class="panel-title">正在进入教师端...</h3>
        <div class="empty-state">正在读取当前 UserSession</div>
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
      renderAccessMessage("请先登录");
    } catch (error) {
      setMessage(error.message, "error");
    }
  });

  window.openTeacherPage = openPage;
  initializeTeacherPage();
})();
