(function () {
  const COURSE_TYPES = ["公选", "选修", "必修"];
  let applicationForm;
  let applicationPage = 1;

  const escapeHtml = window.sharedUi.escapeHtml;

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

  async function loadApplications(resetPage = false) {
    const body = document.getElementById("applications-table-body");
    if (resetPage) applicationPage = 1;
    window.sharedUi.setTableState(body, 10, "正在加载申请记录...");
    try {
      const applications = await window.nativeApi.request("teacher.getCourseApplications", {});
      applicationPage = window.sharedUi.renderPagedTable({ body, rows: applications, row, page: applicationPage, pageSize: 10, colspan: 10, emptyText: "暂无开课申请记录", pagination: "teacherApplicationPagination", onPageChange: page => { applicationPage = page; loadApplications(false); } });
    } catch (error) {
      window.sharedUi.setTableError(body, 10, `加载申请记录失败：${error.message}`, loadApplications);
    }
  }

  async function render(container) {
    container.innerHTML = `
      <section class="panel">
        <h3 class="panel-title">提交开课申请</h3>
        <div id="application-notice" class="form-notice" hidden></div>
        <form id="application-form" class="application-form">
          ${window.sharedUi.formControl({ label: "课程名称", name: "courseName", required: true, maxLength: 100, placeholder: "例如：数据库系统实践" })}
          ${window.sharedUi.formControl({ label: "课程类型", name: "courseType", kind: "select", required: true, emptyText: "请选择课程类型", options: COURSE_TYPES })}
          ${window.sharedUi.formControl({ label: "学分", name: "credit", type: "number", required: true, min: 0.5, step: 0.5, placeholder: "2.0" })}
          ${window.sharedUi.formControl({ label: "面向学院", name: "department", required: true, maxLength: 20, placeholder: "例如：软件学院" })}
          ${window.sharedUi.formControl({ label: "参考教材", name: "textbook", maxLength: 200, placeholder: "可选，例如：数据库系统概论" })}
          ${window.sharedUi.formControl({ label: "课程描述", name: "courseSummary", kind: "textarea", wide: true, placeholder: "填写课程目标、主要内容和考核方式" })}
          <div class="field-actions">
            <button class="primary-button" type="submit">提交申请</button>
          </div>
        </form>
      </section>

      <section class="table-panel"><div class="table-scroll">
        <table class="data-table">
          <thead>
            <tr>
              <th>课程名称</th>
              <th>课程类型</th>
              <th>学分</th>
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
              <td colspan="10"><div class="empty-state">正在加载申请记录...</div></td>
            </tr>
          </tbody>
        </table></div><div id="teacherApplicationPagination"></div>
      </section>
    `;

    applicationForm = window.sharedUi.createForm({
      root: "#application-form", submitButton: "#application-form button[type=submit]", busyText: "提交中...",
      fields: [
        { name: "courseName", label: "课程名称", required: true },
        { name: "courseType", label: "课程类型", required: true, oneOf: COURSE_TYPES },
        { name: "credit", label: "学分", required: true, type: "number", min: 0.5, invalidMessage: "学分必须大于 0" },
        { name: "department", label: "面向学院", required: true },
        { name: "textbook", label: "参考教材" }, { name: "courseSummary", label: "课程描述" }
      ]
    });
    document.getElementById("application-form").addEventListener("submit", async (event) => {
      event.preventDefault();
      if (applicationForm.isSubmitting()) return;

      try {
        await applicationForm.submit(async values => {
          const payload = {
            courseName: values.courseName, credit: values.credit, totalHours: 0,
            textbook: values.textbook, courseSummary: values.courseSummary,
            courseType: values.courseType, department: values.department
          };
          await window.nativeApi.request("teacher.submitCourseApplication", payload);
        }, "提交中...");
        event.target.reset();
        await loadApplications(true);
        showNotice("success", "开课申请已提交");
      } catch (error) {
        showNotice("error", window.sharedUi.errorText(error, "提交失败，请稍后重试。"));
      }
    });

    await loadApplications();
  }

  window.teacherPages = window.teacherPages || {};
  window.teacherPages.applications = {
    render
  };
})();
