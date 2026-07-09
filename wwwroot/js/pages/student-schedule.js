(function () {
  const PERIOD_LABELS = ["1-2", "3-4", "5-6", "7-8", "9-10"];
  const WEEKDAY_LABELS = ["周一", "周二", "周三", "周四", "周五"];

  function buildScheduleGrid(items) {
    // 构建一个 5 行（时段）x 5 列（工作日）的网格
    const grid = {};
    for (let p = 0; p < 5; p++) {
      grid[p] = {};
      for (let d = 0; d < 5; d++) {
        grid[p][d] = null;
      }
    }

    (items || []).forEach((item) => {
      const dayIndex = item.weekday - 1;
      if (dayIndex < 0 || dayIndex >= 5) return;

      // 找到占用的时段
      for (let p = 0; p < 5; p++) {
        const periodStart = p * 2 + 1;
        const periodEnd = periodStart + 1;
        if (item.startPeriod <= periodEnd && item.endPeriod >= periodStart) {
          grid[p][dayIndex] = item;
        }
      }
    });

    let html = '<div class="schedule-grid">';
    html += '<div class="schedule-header">节次</div>';
    WEEKDAY_LABELS.forEach((label) => {
      html += `<div class="schedule-header">${label}</div>`;
    });

    for (let p = 0; p < 5; p++) {
      html += `<div class="schedule-period">${PERIOD_LABELS[p]}</div>`;
      for (let d = 0; d < 5; d++) {
        const item = grid[p][d];
        if (item) {
          html += `
            <div class="schedule-cell has-course">
              <div class="course-name">${item.courseName}</div>
              <div class="course-info">${item.teacherName || ""}</div>
              <div class="course-info">${item.classroom || ""}</div>
            </div>
          `;
        } else {
          html += '<div class="schedule-cell"></div>';
        }
      }
    }

    html += "</div>";
    return html;
  }

  async function render(container) {
    container.innerHTML = `
      <section class="panel">
        <h3 class="panel-title">我的课表</h3>
        <div class="empty-state">正在加载课程表...</div>
      </section>
    `;

    let items;
    try {
      items = await window.nativeApi.request("student.getSchedule", {});
    } catch (error) {
      container.innerHTML = `
        <section class="panel">
          <h3 class="panel-title">我的课表</h3>
          <div class="message error">加载课表失败：${error.message}</div>
        </section>
      `;
      return;
    }

    if (!items || items.length === 0) {
      container.innerHTML = `
        <section class="panel">
          <h3 class="panel-title">我的课表</h3>
          <div class="empty-state">本学期暂无课程安排，请先在选课中心选课。</div>
        </section>
      `;
      return;
    }

    container.innerHTML = `
      <section class="panel">
        <h3 class="panel-title">我的课表</h3>
        <p style="color: var(--muted); font-size: 14px; margin-bottom: 16px;">
          共 ${items.length} 条课程时间安排。
        </p>
        ${buildScheduleGrid(items)}
      </section>
    `;
  }

  window.studentPages = window.studentPages || {};
  window.studentPages.schedule = {
    render
  };
})();
