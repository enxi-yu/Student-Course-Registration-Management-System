(function () {
  const pageMeta = {
    dashboard: { title: "首页", description: "查看个人信息和学业概览" },
    courses: { title: "选课中心", description: "浏览并选择本学期课程" },
    schedule: { title: "我的课表", description: "查看本学期课程安排" },
    grades: { title: "成绩查询", description: "查看已修课程成绩和 GPA" },
    evaluation: { title: "课程评价", description: "对已修课程进行评价" },
    profile: { title: "个人资料", description: "查看个人资料，维护联系方式和账号安全" },
    detail: { title: "课程详情", description: "查看课程详细信息" }
  };

  const state = {
    student: null,
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
    if (text.indexOf("当前账号不是学生") >= 0) {
      return "当前账号不是学生，无权访问学生端";
    }

    if (text.indexOf("请先登录") >= 0) {
      return "请先登录";
    }

    return text || "请先登录";
  }

  function updateStudentSummary(student) {
    const summary = document.getElementById("student-summary");
    if (!student) {
      summary.textContent = "未登录";
      return;
    }

    summary.textContent = `${student.realName || "-"} / ${student.studentNo || "-"}`;
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
    const heading = document.getElementById("content-heading");
    if (heading) heading.hidden = page === "dashboard";
  }

  function renderAccessMessage(message) {
    state.isAuthorized = false;
    updateStudentSummary(null);
    setMessage("", "");
    setPageMeta("dashboard");
    setActiveNav("dashboard");

    const title = normalizeAccessMessage(message);
    document.getElementById("page-root").innerHTML = `
      <section class="panel" style="text-align:center; padding:48px 24px;">
        <div style="font-size:64px; margin-bottom:16px;">🔒</div>
        <h3 class="panel-title" style="font-size:20px;">${title}</h3>
        <p style="color:var(--muted); margin-bottom:0;">请返回统一登录页面，使用学生账号登录后进入学生端。</p>
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
        await window.studentPages.dashboard.render(container);
        return;
      }

      if (page === "courses") {
        await window.studentPages.courses.render(container, state.pageOptions);
        return;
      }

      if (page === "schedule") {
        await window.studentPages.schedule.render(container, state.pageOptions);
        return;
      }

      if (page === "grades") {
        await window.studentPages.grades.render(container);
        return;
      }

      if (page === "evaluation") {
        await window.studentPages.evaluation.render(container);
        return;
      }

      if (page === "profile") {
        await window.studentPages.profile.render(container);
        return;
      }

      if (page === "detail") {
        await window.studentPages.detail.render(container, state.pageOptions);
        return;
      }

      throw new Error("页面不存在");
    } catch (error) {
      setMessage(error.message, "error");
    }
  }

  async function initializeStudentPage() {
    document.getElementById("page-root").innerHTML = `
      <section class="panel" style="text-align:center; padding:48px 24px;">
        <div style="font-size:40px; margin-bottom:12px;">⏳</div>
        <h3 class="panel-title">正在进入学生端...</h3>
        <p style="color:var(--muted);">正在验证身份并加载个人信息，请稍候。</p>
      </section>
    `;

    try {
      const student = await window.nativeApi.request("student.getCurrentStudent", {});
      state.student = student;
      state.isAuthorized = true;
      updateStudentSummary(student);
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

  window.openStudentPage = openPage;
  initializeStudentPage();
})();
