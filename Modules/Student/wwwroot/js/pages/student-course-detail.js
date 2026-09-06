(function () {
  const escapeHtml = window.sharedUi.escapeHtml;

  function weekdayLabel(d) {
    return ["", "周一", "周二", "周三", "周四", "周五", "周六", "周日"][d] || "";
  }

  var currentOptions = null;

  async function render(container, options) {
    currentOptions = options || {};
    var currentClassId = Number(currentOptions.classId || 0);
    if (!currentClassId) {
      container.innerHTML = `<section class="panel"><div class="empty-state">课程不存在</div>
        <button class="secondary-button back-btn">返回</button></section>`;
      container.querySelector(".back-btn").addEventListener("click", back);
      return;
    }

    container.innerHTML = `<section class="panel"><div class="empty-state">正在加载课程详情...</div></section>`;

    var detail;
    try {
      detail = await window.nativeApi.request("student.getCourseDetail", { classId: currentClassId });
    } catch (e) {
      container.innerHTML = `<section class="panel"><div class="message error">加载失败：${escapeHtml(e.message)}</div>
        <button class="secondary-button back-btn">返回</button></section>`;
      container.querySelector(".back-btn").addEventListener("click", back);
      return;
    }

    if (!detail) {
      container.innerHTML = `<section class="panel"><div class="empty-state">课程不存在</div>
        <button class="secondary-button back-btn">返回</button></section>`;
      container.querySelector(".back-btn").addEventListener("click", back);
      return;
    }

    var scheduleHtml = window.sharedUi.dataTable({
      columns: ["星期", "节次", "教室", "周次"], rows: detail.schedule || [], emptyText: "暂无上课时间安排",
      row: function (s) { return `<tr><td>${weekdayLabel(s.weekday)}</td><td>${s.startPeriod}-${s.endPeriod}节</td><td>${escapeHtml(s.classroom)}</td><td>第${escapeHtml(s.weekRange)}周</td></tr>`; }
    });

    var badgeClass = detail.remaining > 0 ? "available" : "full";
    var badgeText = detail.remaining > 0 ? "剩余 " + detail.remaining + " 人" : "已满";

    var hasBatch = Number(currentOptions.batchId || 0) > 0;
    var isSelected = !!detail.isSelected;
    var canDrop = !!detail.canDrop;

    var actionHtml = "";
    var actionHintHtml = "";

    if (isSelected) {
      if (canDrop) {
        actionHtml = '<button class="danger-button drop-btn">退课</button>';
      } else {
        actionHtml = '<button class="danger-button drop-btn" disabled>退课</button>';
        actionHintHtml = `<div class="message" style="margin-top:12px;margin-bottom:0;background:var(--blue-050);border:1px solid var(--blue-100);color:var(--blue-800);">当前不在该课程所属选课批次的退课时间内。</div>`;
      }
    } else if (hasBatch) {
      actionHtml = `<button class="primary-button select-btn"${detail.remaining <= 0 ? " disabled" : ""}>选课</button>`;
    } else {
      actionHtml = '<button class="primary-button select-btn" disabled>选课</button>';
      actionHintHtml = `<div class="message" style="margin-top:12px;margin-bottom:0;background:var(--blue-050);border:1px solid var(--blue-100);color:var(--blue-800);">请从选课中心进入课程详情进行选课。</div>`;
    }

    container.innerHTML = `
      <section class="panel">
        <div style="display:flex; justify-content:space-between; align-items:flex-start;">
          <div>
            <h3 class="panel-title" style="margin-bottom:4px;">${escapeHtml(detail.courseName)}</h3>
            <p style="color:var(--muted); margin:0;">${escapeHtml(detail.className)}</p>
          </div>
          <button class="secondary-button back-btn">← 返回</button>
        </div>
      </section>

      <section class="panel">
        <h3 class="panel-title">基本信息</h3>
        <div class="profile-grid">
          <div class="profile-item"><div class="profile-label">课程类型</div><div class="profile-value">${escapeHtml(detail.courseType)}</div></div>
          <div class="profile-item"><div class="profile-label">学分</div><div class="profile-value">${detail.credit.toFixed(1)}</div></div>
          <div class="profile-item"><div class="profile-label">总学时</div><div class="profile-value">${detail.totalHours}</div></div>
          <div class="profile-item"><div class="profile-label">授课教师</div><div class="profile-value">${escapeHtml(detail.teacherName)}</div></div>
          <div class="profile-item"><div class="profile-label">开课院系</div><div class="profile-value">${escapeHtml(detail.department)}</div></div>
          <div class="profile-item"><div class="profile-label">容量</div><div class="profile-value">${detail.selectedCount}/${detail.capacity} <span class="capacity-badge ${badgeClass}" style="margin-left:8px;">${badgeText}</span></div></div>
        </div>
        ${detail.description ? `<div style="margin-top:16px;"><div class="profile-label">课程描述</div><p style="margin:6px 0 0; color:var(--muted);">${escapeHtml(detail.description)}</p></div>` : ""}
      </section>

      <section class="panel">
        <h3 class="panel-title">上课时间</h3>
        ${scheduleHtml}
      </section>

      <section class="panel">
        <div style="display:flex; gap:10px;">
          ${actionHtml}
          <button class="secondary-button back-btn">返回</button>
        </div>
        ${actionHintHtml}
        <div id="detail-msg"></div>
      </section>
    `;

    container.querySelectorAll(".back-btn").forEach(function (b) {
      b.addEventListener("click", back);
    });

    var selectBtn = container.querySelector(".select-btn");
    if (selectBtn) {
      selectBtn.addEventListener("click", async function () {
        if (selectBtn.disabled) return;
        if (!hasBatch) {
          document.getElementById("detail-msg").innerHTML = `<div class="message error">请从选课中心进入课程详情进行选课。</div>`;
          return;
        }
        window.sharedUi.setBusy(selectBtn, true, "选课中...");
        try {
          var r = await window.nativeApi.request("student.selectCourse", { classId: currentClassId, batchId: currentOptions.batchId });
          if (r.success) {
            document.getElementById("detail-msg").innerHTML = `<div class="message success">选课成功！</div>`;
            render(container, currentOptions);
          } else {
            document.getElementById("detail-msg").innerHTML = `<div class="message error">${escapeHtml(r.message)}</div>`;
          }
        } catch (e) {
          document.getElementById("detail-msg").innerHTML = `<div class="message error">${escapeHtml(e.message)}</div>`;
        } finally { if (selectBtn.isConnected) window.sharedUi.setBusy(selectBtn, false); }
      });
    }

    var dropBtn = container.querySelector(".drop-btn");
    if (dropBtn) {
      dropBtn.addEventListener("click", async function () {
        if (dropBtn.disabled) return;
        if (!await window.sharedUi.confirm("确认退选该课程？")) return;
        window.sharedUi.setBusy(dropBtn, true, "退课中...");
        try {
          var r = await window.nativeApi.request("student.dropCourse", { classId: currentClassId });
          if (r.success) {
            document.getElementById("detail-msg").innerHTML = `<div class="message success">退课成功！</div>`;
            render(container, currentOptions);
          } else {
            document.getElementById("detail-msg").innerHTML = `<div class="message error">${escapeHtml(r.message)}</div>`;
          }
        } catch (e) {
          document.getElementById("detail-msg").innerHTML = `<div class="message error">${escapeHtml(e.message)}</div>`;
        } finally { if (dropBtn.isConnected) window.sharedUi.setBusy(dropBtn, false); }
      });
    }
  }

  function back() {
    if (!window.openStudentPage) return;
    var returnPage = currentOptions && currentOptions.returnPage;
    if (returnPage === "courses") {
      window.openStudentPage("courses", {
        batchId: currentOptions.batchId,
        pendingIds: currentOptions.pendingIds
      });
      return;
    }
    if (returnPage === "schedule") {
      window.openStudentPage("schedule", { semester: currentOptions.semester });
      return;
    }
    window.openStudentPage("courses");
  }

  window.studentPages = window.studentPages || {};
  window.studentPages.detail = {
    render: render
  };
})();
