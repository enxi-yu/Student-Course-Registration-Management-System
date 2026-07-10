(function () {
  const COURSE_TYPES = ["公选", "选修", "必修"];

  function escapeHtml(value) {
    return String(value ?? "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function statusClass(status) {
    if (status === "通过" || status === "已开课") return "status-approved";
    if (status === "驳回") return "status-rejected";
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
        <td>${escapeHtml(application.department || "-")}</td>
        <td>${escapeHtml(application.textbook || "-")}</td>
        <td>${escapeHtml(application.courseSummary || "-")}</td>
        <td><span class="status-badge ${statusClass(application.status)}">${escapeHtml(application.status || "待审核")}</span></td>
        <td>${escapeHtml(application.applyTime || "-")}</td>
        <td>${escapeHtml(application.approveTime || "-")}</td>
        <td>${escapeHtml(application.approveComment || "-")}</td>
      </tr>
    `;
  }

  async function loadApplications() {
    const applications = await window.nativeApi.request("teacher.getCourseApplications", {});
    const body = document.getElementById("applications-table-body");

    if (!applications || applications.length === 0) {
      body.innerHTML = `
        <tr>
          <td colspan="11"><div class="empty-state">暂无开课申请记录</div></td>
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
    const courseName = data.get("courseName").trim();
    const courseType = data.get("courseType").trim();
    const department = data.get("department").trim();

    if (!courseName) {
      throw new Error("课程名称不能为空");
    }

    if (Number.isNaN(credit) || credit <= 0) {
      throw new Error("学分必须大于 0");
    }

    if (!Number.isInteger(totalHours) || totalHours <= 0) {
      throw new Error("总学时必须为正整数");
    }

    if (!courseType) {
      throw new Error("课程类型不能为空");
    }

    if (!COURSE_TYPES.includes(courseType)) {
      throw new Error("课程类型只能选择公选、选修或必修");
    }

    if (!department) {
      throw new Error("面向学院不能为空");
    }

    return {
      courseName,
      credit,
      totalHours,
      textbook: data.get("textbook").trim(),
      courseSummary: data.get("courseSummary").trim(),
      courseType,
      department
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
            <input name="courseName" type="text" maxlength="100" placeholder="例如：数据库系统实践">
          </div>
          <div class="field">
            <label>课程类型</label>
            <select name="courseType" required>
              <option value="">请选择课程类型</option>
              <option value="公选">公选</option>
              <option value="选修">选修</option>
              <option value="必修">必修</option>
            </select>
          </div>
          <div class="field">
            <label>学分</label>
            <input name="credit" type="number" min="0.5" step="0.5" placeholder="2.0">
          </div>
          <div class="field">
            <label>总学时</label>
            <input name="totalHours" type="number" min="1" step="1" placeholder="32">
          </div>
          <div class="field">
            <label>面向学院</label>
            <input name="department" type="text" maxlength="20" placeholder="例如：软件学院">
          </div>
          <div class="field">
            <label>参考教材</label>
            <input name="textbook" type="text" maxlength="200" placeholder="可选，例如：数据库系统概论">
          </div>
          <div class="field field-wide">
            <label>课程描述</label>
            <textarea name="courseSummary" placeholder="填写课程目标、主要内容和考核方式"></textarea>
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
              <th>课程类型</th>
              <th>学分</th>
              <th>总学时</th>
              <th>面向学院</th>
              <th>参考教材</th>
              <th>课程描述</th>
              <th>审批状态</th>
              <th>申请时间</th>
              <th>审批时间</th>
              <th>审批意见</th>
            </tr>
          </thead>
          <tbody id="applications-table-body">
            <tr>
              <td colspan="11"><div class="empty-state">正在加载申请记录...</div></td>
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