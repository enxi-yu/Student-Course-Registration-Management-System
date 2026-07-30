(function () {
  function escapeHtml(v) {
    return String(v ?? "").replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;");
  }

  function weekdayLabel(d) {
    return ["", "周一", "周二", "周三", "周四", "周五", "周六", "周日"][d] || "";
  }

  var currentClassId = 0;

  async function render(container, classId) {
    currentClassId = classId;
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

    var scheduleHtml = (detail.schedule || []).length === 0
      ? "<p>暂无上课时间安排</p>"
      : `<table class="data-table"><thead><tr><th>星期</th><th>节次</th><th>教室</th><th>周次</th></tr></thead><tbody>`
        + detail.schedule.map(function (s) {
          return `<tr><td>${weekdayLabel(s.weekday)}</td><td>${s.startPeriod}-${s.endPeriod}节</td><td>${escapeHtml(s.classroom)}</td><td>第${escapeHtml(s.weekRange)}周</td></tr>`;
        }).join("")
        + "</tbody></table>";

    var badgeClass = detail.remaining > 0 ? "available" : "full";
    var badgeText = detail.remaining > 0 ? "剩余 " + detail.remaining + " 人" : "已满";

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
          <button class="primary-button select-btn" ${detail.remaining <= 0 ? "disabled" : ""}>选课</button>
          <button class="danger-button drop-btn">退课</button>
          <button class="secondary-button back-btn">返回</button>
        </div>
        <div id="detail-msg"></div>
      </section>
    `;

    container.querySelectorAll(".back-btn").forEach(function (b) {
      b.addEventListener("click", back);
    });

    container.querySelector(".select-btn").addEventListener("click", async function () {
      var btn = container.querySelector(".select-btn");
      btn.disabled = true;
      btn.textContent = "选课中...";
      try {
        var r = await window.nativeApi.request("student.selectCourse", { classId: currentClassId });
        if (r.success) {
          document.getElementById("detail-msg").innerHTML = `<div class="message success">选课成功！</div>`;
          render(container, currentClassId);
        } else {
          document.getElementById("detail-msg").innerHTML = `<div class="message error">${escapeHtml(r.message)}</div>`;
          btn.disabled = false;
          btn.textContent = "选课";
        }
      } catch (e) {
        document.getElementById("detail-msg").innerHTML = `<div class="message error">${escapeHtml(e.message)}</div>`;
        btn.disabled = false;
        btn.textContent = "选课";
      }
    });

    container.querySelector(".drop-btn").addEventListener("click", async function () {
      if (!confirm("确认退选该课程？")) return;
      var btn = container.querySelector(".drop-btn");
      btn.disabled = true;
      btn.textContent = "退课中...";
      try {
        var r = await window.nativeApi.request("student.dropCourse", { classId: currentClassId });
        if (r.success) {
          document.getElementById("detail-msg").innerHTML = `<div class="message success">退课成功！</div>`;
          render(container, currentClassId);
        } else {
          document.getElementById("detail-msg").innerHTML = `<div class="message error">${escapeHtml(r.message)}</div>`;
          btn.disabled = false;
          btn.textContent = "退课";
        }
      } catch (e) {
        document.getElementById("detail-msg").innerHTML = `<div class="message error">${escapeHtml(e.message)}</div>`;
        btn.disabled = false;
        btn.textContent = "退课";
      }
    });
  }

  function back() {
    if (window.openStudentPage) {
      window.openStudentPage("courses");
    }
  }

  window.studentPages = window.studentPages || {};
  window.studentPages.detail = {
    render: render
  };
})();
