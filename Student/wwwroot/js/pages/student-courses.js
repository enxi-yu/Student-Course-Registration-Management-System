(function () {
  function renderCourseCard(course) {
    const selectedClass = course.isSelected ? " selected" : "";
    const badgeClass = course.remaining > 0 ? "available" : "full";
    const badgeText = course.remaining > 0 ? `剩余 ${course.remaining} 人` : "已满";

    return `
      <div class="course-card${selectedClass}" data-class-id="${course.classId}">
        <div class="course-card-info">
          <h3>${course.courseName} <small>${course.className}</small></h3>
          <div class="course-card-meta">
            <span>教师：${course.teacherName || "-"}</span>
            <span>学分：${course.credit.toFixed(1)}</span>
            <span>类型：${course.courseType || "-"}</span>
            <span>时间：${course.scheduleSummary || "暂无"}</span>
          </div>
        </div>
        <div class="course-card-actions">
          <span class="capacity-badge ${badgeClass}">${course.selectedCount}/${course.capacity} ${badgeText}</span>
          ${course.isSelected
            ? `<button class="danger-button drop-btn" data-class-id="${course.classId}">退课</button>`
            : `<button class="primary-button select-btn" data-class-id="${course.classId}" ${course.remaining <= 0 ? "disabled" : ""}>选课</button>`
          }
        </div>
      </div>
    `;
  }

  async function render(container) {
    container.innerHTML = `
      <section class="panel">
        <h3 class="panel-title">选课中心</h3>
        <div class="empty-state">正在加载可选课程...</div>
      </section>
    `;

    let courses;
    try {
      courses = await window.nativeApi.request("student.getAvailableCourses", {});
    } catch (error) {
      container.innerHTML = `
        <section class="panel">
          <h3 class="panel-title">选课中心</h3>
          <div class="message error">加载课程列表失败：${error.message}</div>
        </section>
      `;
      return;
    }

    if (!courses || courses.length === 0) {
      container.innerHTML = `
        <section class="panel">
          <h3 class="panel-title">选课中心</h3>
          <div class="empty-state">本学期暂无可选课程。</div>
        </section>
      `;
      return;
    }

    const courseCards = courses.map((c) => renderCourseCard(c)).join("");

    container.innerHTML = `
      <section class="panel">
        <h3 class="panel-title">选课中心</h3>
        <p style="color: var(--muted); font-size: 14px; margin-bottom: 16px;">
          共 ${courses.length} 门课程可选。
        </p>
      </section>
      <div class="course-list">${courseCards}</div>
    `;

    // 绑定选课按钮
    container.querySelectorAll(".select-btn").forEach((btn) => {
      btn.addEventListener("click", async () => {
        const classId = parseInt(btn.dataset.classId);
        if (isNaN(classId)) return;

        btn.disabled = true;
        btn.textContent = "选课中...";

        try {
          const result = await window.nativeApi.request("student.selectCourse", { classId });
          if (result && result.success) {
            alert(result.message || "选课成功！");
            await render(container);
          } else {
            alert(result.message || "选课失败。");
            btn.disabled = false;
            btn.textContent = "选课";
          }
        } catch (error) {
          alert("选课失败：" + error.message);
          btn.disabled = false;
          btn.textContent = "选课";
        }
      });
    });

    // 绑定退课按钮
    container.querySelectorAll(".drop-btn").forEach((btn) => {
      btn.addEventListener("click", async () => {
        const classId = parseInt(btn.dataset.classId);
        if (isNaN(classId)) return;
        if (!confirm("确认要退选该课程吗？")) return;

        btn.disabled = true;
        btn.textContent = "退课中...";

        try {
          const result = await window.nativeApi.request("student.dropCourse", { classId });
          if (result && result.success) {
            alert(result.message || "退课成功！");
            await render(container);
          } else {
            alert(result.message || "退课失败。");
            btn.disabled = false;
            btn.textContent = "退课";
          }
        } catch (error) {
          alert("退课失败：" + error.message);
          btn.disabled = false;
          btn.textContent = "退课";
        }
      });
    });
  }

  window.studentPages = window.studentPages || {};
  window.studentPages.courses = {
    render
  };
})();
