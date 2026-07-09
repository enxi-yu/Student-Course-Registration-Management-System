(function () {
  function escapeHtml(value) {
    return String(value ?? "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function statusClass(status) {
    if (status === "已通过") return "status-approved";
    if (status === "已驳回") return "status-rejected";
    return "status-pending";
  }


  function showNotice(type, message) {
    const notice = document.getElementById("application-notice");
    if (!notice) {
      return;
    }

    notice.className = `form-notice ${type}`;
    notice.textContent = message;
    notice.hidden = false;

    window.clearTimeout(showNotice.timer);
    showNotice.timer = window.setTimeout(() => {
      notice.hidden = true;
    }, 3600);
  }
  function row(application) {
    return `
      <tr>
        <td>${escapeHtml(application.courseName)}</td>
        <td>${escapeHtml(application.courseType || "-")}</td>
        <td>${application.credit}</td>
        <td>${application.totalHours}</td>
        <td>${escapeHtml(application.targetMajor || "-")}</td>
        <td>${escapeHtml(application.targetGrade || "-")}</td>
        <td><span class="status-badge ${statusClass(application.status)}">${escapeHtml(application.status || "待审核")}</span></td>
        <td>${escapeHtml(application.applyTime || "-")}</td>
        <td>${escapeHtml(application.reviewRemark || "-")}</td>
      </tr>
    `;
  }

  async function loadApplications() {
    const applications = await window.nativeApi.request("teacher.getCourseApplications", {});
    const body = document.getElementById("applications-table-body");

    if (!applications || applications.length === 0) {
      body.innerHTML = `
        <tr>
          <td colspan="9"><div class="empty-state">暂无开课申请记录</div></td>
        </tr>
      `;
      return;
    }

    body.innerHTML = applications.map(row).join("");
  }

  function readForm() {
    const form = document.getElementById("application-form");
    const data = new FormData(form);
    const credit = Number(data.get("credit"));
    const totalHours = Number(data.get("totalHours"));

    if (!data.get("courseName").trim()) {
      throw new Error("课程名称不能为空");
    }

    if (Number.isNaN(credit) || credit <= 0) {
      throw new Error("学分必须大于 0");
    }

    if (!Number.isInteger(totalHours) || totalHours <= 0) {
      throw new Error("学时必须为正整数");
    }

    return {
      courseName: data.get("courseName").trim(),
      courseType: data.get("courseType").trim(),
      credit,
      totalHours,
      targetMajor: data.get("targetMajor").trim(),
      targetGrade: data.get("targetGrade").trim(),
      description: data.get("description").trim()
    };
  }

  async function render(container) {
    container.innerHTML = `
      <section class="panel">
        <h3 class="panel-title">提交开课申请</h3>
        <div id="application-notice" class="form-notice" hidden></div>
        <form id="application-form" class="application-form">
          <div class="field">
            <label>课程名称</label>
            <input name="courseName" type="text" placeholder="例如：数据库系统实践">
          </div>
          <div class="field">
            <label>课程类型</label>
            <input name="courseType" type="text" placeholder="例如：专业选修">
          </div>
          <div class="field">
            <label>学分</label>
            <input name="credit" type="number" min="0.5" step="0.5" placeholder="2.0">
          </div>
          <div class="field">
            <label>学时</label>
            <input name="totalHours" type="number" min="1" step="1" placeholder="32">
          </div>
          <div class="field">
            <label>面向专业</label>
            <input name="targetMajor" type="text" placeholder="计算机科学与技术">
          </div>
          <div class="field">
            <label>面向年级</label>
            <input name="targetGrade" type="text" placeholder="2024级">
          </div>
          <div class="field field-wide">
            <label>课程大纲描述</label>
            <textarea name="description" placeholder="填写课程目标、主要内容和考核方式"></textarea>
          </div>
          <div class="field-actions">
            <button class="primary-button" type="submit">提交申请</button>
          </div>
        </form>
      </section>

      <section class="table-panel">
        <table class="data-table">
          <thead>
            <tr>
              <th>课程名称</th>
              <th>类型</th>
              <th>学分</th>
              <th>学时</th>
              <th>专业</th>
              <th>年级</th>
              <th>状态</th>
              <th>申请时间</th>
              <th>审核意见</th>
            </tr>
          </thead>
          <tbody id="applications-table-body">
            <tr>
              <td colspan="9"><div class="empty-state">正在加载申请记录...</div></td>
            </tr>
          </tbody>
        </table>
      </section>
    `;

    document.getElementById("application-form").addEventListener("submit", async (event) => {
      event.preventDefault();

      try {
        const payload = readForm();
        await window.nativeApi.request("teacher.submitCourseApplication", payload);
        event.target.reset();
        await loadApplications();
        showNotice("success", "开课申请已提交");
      } catch (error) {
        showNotice("error", error.message || "提交失败，请稍后重试。");
      }
    });

    await loadApplications();
  }

  window.teacherPages = window.teacherPages || {};
  window.teacherPages.applications = {
    render
  };
})();