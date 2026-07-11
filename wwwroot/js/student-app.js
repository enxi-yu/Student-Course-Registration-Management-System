(function () {
  const pageMeta = {
    dashboard: { title: "首页", description: "查看个人信息和学业概览" },
    courses: { title: "选课中心", description: "浏览并选择本学期课程" },
    schedule: { title: "我的课表", description: "查看本学期课程安排" },
    grades: { title: "成绩查询", description: "查看已修课程成绩和 GPA" },
    evaluation: { title: "课程评价", description: "对已修课程进行评价" },
    password: { title: "修改个人信息", description: "维护联系方式和账号安全" }
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
    document.getElementById("view-subtitle").textContent = meta.description;
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
        <p style="color:var(--muted); margin-bottom:24px;">
          当前尚未登录，请通过以下方式之一进入学生端：
        </p>
        <div style="display:flex; flex-direction:column; align-items:center; gap:12px;">
          <div style="background:#f8fafc; border:1px solid var(--line); border-radius:10px; padding:16px 24px; max-width:420px; width:100%;">
            <div style="font-weight:700; margin-bottom:6px;">方式一：开发测试（本地调试用）</div>
            <p style="font-size:13px; color:var(--muted); margin-bottom:10px;">
              点击下方按钮，自动使用预设的学生账号（学号 S2024001）
            </p>
            <button class="primary-button" type="button" id="dev-student-button"
                    style="font-size:15px; padding:10px 28px;">
              👤 使用学生 Mock Session
            </button>
          </div>
          <div style="background:#f8fafc; border:1px solid var(--line); border-radius:10px; padding:16px 24px; max-width:420px; width:100%;">
            <div style="font-weight:700; margin-bottom:6px;">方式二：公共登录入口</div>
            <p style="font-size:13px; color:var(--muted); margin-bottom:10px;">
              通过统一登录页面以真实账号登录后进入学生端
            </p>
            <button class="secondary-button" type="button" id="dev-teacher-button"
                    style="font-size:14px;">
              🔄 切换为教师 Mock Session（测试权限）
            </button>
          </div>
        </div>
      </section>
    `;

    document.getElementById("dev-student-button").addEventListener("click", async () => {
      const btn = document.getElementById("dev-student-button");
      btn.disabled = true;
      btn.textContent = "正在连接服务器...";
      try {
        await window.nativeApi.request("app.useMockStudentSession", {});
        await initializeStudentPage();
      } catch (error) {
        btn.disabled = false;
        btn.textContent = "👤 使用学生 Mock Session";
        setMessage("登录失败：" + error.message + "（请确认已通过 dotnet run 启动后端服务）", "error");
      }
    });

    document.getElementById("dev-teacher-button").addEventListener("click", async () => {
      const btn = document.getElementById("dev-teacher-button");
      btn.disabled = true;
      btn.textContent = "正在切换...";
      try {
        await window.nativeApi.request("app.useMockTeacherSession", {});
        await initializeStudentPage();
      } catch (error) {
        btn.disabled = false;
        btn.textContent = "🔄 切换为教师 Mock Session（测试权限）";
        setMessage("操作失败：" + error.message, "error");
      }
    });
  }

  function renderComingSoon(container, page) {
    const meta = pageMeta[page] || { title: "功能", description: "后续阶段实现" };
    container.innerHTML = `
      <section class="panel">
        <h3 class="panel-title">${meta.title}</h3>
        <div class="empty-state">该功能将在后续阶段实现。</div>
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
        await window.studentPages.courses.render(container);
        return;
      }

      if (page === "schedule") {
        await window.studentPages.schedule.render(container);
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

      if (page === "password") {
        await window.studentPages.password.render(container);
        return;
      }

      renderComingSoon(container, page);
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
      renderAccessMessage("请先登录");
    } catch (error) {
      setMessage(error.message, "error");
    }
  });

  window.openStudentPage = openPage;
  initializeStudentPage();
})();
