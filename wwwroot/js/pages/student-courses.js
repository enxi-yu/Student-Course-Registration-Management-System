(function () {
  var selectedIds = {};       // 当前已选 classId -> true (后端)
  var pendingIds = {};        // 本地暂存 classId -> true (还没提交)
  var allCourses = [];
  var scheduleData = {};

  var WEEKDAY = ["", "周一", "周二", "周三", "周四", "周五"];
  var PERIODS = ["1-2", "3-4", "5-6", "7-8", "9-10"];

  function escape(v) { return String(v || "").replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;"); }

  // ---- 顶部迷你课表 ----
  function buildMiniSchedule() {
    // 汇总所有暂存课程的 schedule
    var grid = {};
    for (var p = 0; p < 5; p++) { grid[p] = {}; for (var d = 0; d < 5; d++) grid[p][d] = []; }

    Object.keys(pendingIds).forEach(function (cid) {
      var cidNum = parseInt(cid);
      var items = scheduleData[cidNum];
      if (!items) return;
      items.forEach(function (s) {
        var day = s.weekday - 1;
        if (day < 0 || day > 4) return;
        for (var p = 0; p < 5; p++) {
          var ps = p * 2 + 1, pe = ps + 1;
          if (s.startPeriod <= pe && s.endPeriod >= ps) {
            grid[p][day].push(s);
          }
        }
      });
    });

    var html = '<div class="mini-schedule">';
    html += '<div class="mini-schedule-header">节次</div>';
    WEEKDAY.forEach(function (l, i) { if (i > 0) html += '<div class="mini-schedule-header">' + l + '</div>'; });

    for (var p = 0; p < 5; p++) {
      html += '<div class="mini-schedule-period">' + PERIODS[p] + '</div>';
      for (var d = 0; d < 5; d++) {
        var cells = grid[p][d] || [];
        if (cells.length > 0) {
          var names = cells.map(function (s) { return escape(s.courseName || s.className); }).join("<br>");
          var dup = new Set();
          cells.forEach(function (s) { dup.add(s.classId); });
          var conflict = dup.size > 1 ? " conflict" : "";
          html += '<div class="mini-schedule-cell has-course' + conflict + '">' + names + '</div>';
        } else {
          html += '<div class="mini-schedule-cell"></div>';
        }
      }
    }
    html += '</div>';
    return html;
  }

  // ---- 已选课程列表 ----
  function buildSelectedBar() {
    var selected = allCourses.filter(function (c) { return pendingIds[c.classId]; });
    if (selected.length === 0) {
      return '<p style="color:var(--muted); margin:0;">暂未选择课程，请在下方勾选后保存。</p>';
    }
    var html = '<div class="selected-course-tags">';
    selected.forEach(function (c) {
      html += '<span class="selected-tag">' + escape(c.courseName) + ' (' + escape(c.className) + ')'
        + ' <span class="tag-remove" data-cid="' + c.classId + '">✕</span></span>';
    });
    html += '</div>';
    return html;
  }

  // ---- 课程选择卡片 ----
  function buildCourseCard(c) {
    var checked = pendingIds[c.classId] ? " checked" : "";
    var badgeClass = c.remaining > 0 ? "available" : "full";
    var badgeText = c.remaining > 0 ? "剩余 " + c.remaining + " 人" : "已满";

    return '<div class="course-card">'
      + '<div class="course-card-check">'
        + '<input type="checkbox" class="course-checkbox" data-cid="' + c.classId + '"' + checked + (c.remaining <= 0 && !pendingIds[c.classId] ? " disabled" : "") + '>'
      + '</div>'
      + '<div class="course-card-info click-detail" data-cid="' + c.classId + '">'
        + '<h3>' + escape(c.courseName) + ' <small>' + escape(c.className) + '</small></h3>'
        + '<div class="course-card-meta">'
          + '<span>教师：' + escape(c.teacherName) + '</span>'
          + '<span>学分：' + c.credit.toFixed(1) + '</span>'
          + '<span>类型：' + escape(c.courseType) + '</span>'
          + '<span>时间：' + escape(c.scheduleSummary || "暂无") + '</span>'
        + '</div>'
      + '</div>'
      + '<div class="course-card-badge">'
        + '<span class="capacity-badge ' + badgeClass + '">' + c.selectedCount + '/' + c.capacity + ' ' + badgeText + '</span>'
      + '</div>'
    + '</div>';
  }

  // ---- 加载缓存课程时间 ----
  function loadScheduleCache() {
    var ids = Object.keys(pendingIds).map(Number);
    var need = ids.filter(function (id) { return !scheduleData[id]; });
    if (need.length === 0) return Promise.resolve();

    return Promise.all(need.map(function (id) {
      return window.nativeApi.request("student.getCourseDetail", { classId: id }).then(function (d) {
        if (d && d.schedule) scheduleData[id] = d.schedule;
      }).catch(function () {});
    }));
  }

  // ---- 检查时间冲突 ----
  function findConflicts() {
    var conflicts = [];
    var cids = Object.keys(pendingIds).map(Number);
    for (var i = 0; i < cids.length; i++) {
      var a = scheduleData[cids[i]];
      if (!a) continue;
      for (var j = i + 1; j < cids.length; j++) {
        var b = scheduleData[cids[j]];
        if (!b) continue;
        for (var ai = 0; ai < a.length; ai++) {
          for (var bi = 0; bi < b.length; bi++) {
            if (a[ai].weekday === b[bi].weekday
              && a[ai].startPeriod <= b[bi].endPeriod
              && a[ai].endPeriod >= b[bi].startPeriod) {
              conflicts.push([cids[i], cids[j]]);
            }
          }
        }
      }
    }
    return conflicts;
  }

  // ---- 刷新界面 ----
  var containerRef;
  function refresh() {
    loadScheduleCache().then(function () {
      var conflicts = findConflicts();
      var conflictIds = {};
      conflicts.forEach(function (p) { conflictIds[p[0]] = true; conflictIds[p[1]] = true; });

      containerRef.innerHTML = ''
        + '<section class="panel"><h3 class="panel-title">我的课表预览</h3>'
          + buildMiniSchedule()
          + '<div style="margin-top:12px;">' + buildSelectedBar() + '</div>'
          + (conflicts.length > 0 ? '<div class="message error" style="margin-top:8px;">⚠ 存在时间冲突的课程，请调整后再保存。</div>' : '')
        + '</section>'
        + '<section class="panel"><div style="display:flex; justify-content:space-between; align-items:center;">'
          + '<h3 class="panel-title" style="margin:0;">可选课程</h3>'
          + '<button class="primary-button save-btn">💾 保存选课</button>'
        + '</div></section>'
        + '<div class="course-list">'
          + allCourses.map(buildCourseCard).join("")
        + '</div>';

      // checkbox 事件
      containerRef.querySelectorAll(".course-checkbox").forEach(function (cb) {
        cb.addEventListener("change", function () {
          var cid = parseInt(cb.dataset.cid);
          if (cb.checked) {
            pendingIds[cid] = true;
          } else {
            delete pendingIds[cid];
          }
          refresh();
        });
      });

      // 标签移除
      containerRef.querySelectorAll(".tag-remove").forEach(function (btn) {
        btn.addEventListener("click", function () {
          var cid = parseInt(btn.dataset.cid);
          delete pendingIds[cid];
          refresh();
        });
      });

      // 点击课程名查看详情
      containerRef.querySelectorAll(".click-detail").forEach(function (el) {
        el.addEventListener("click", function () {
          var cid = parseInt(el.dataset.cid);
          if (!isNaN(cid) && window.openStudentPage) window.openStudentPage("detail", { classId: cid });
        });
      });

      // 保存按钮
      var saveBtn = containerRef.querySelector(".save-btn");
      if (saveBtn) {
        saveBtn.addEventListener("click", async function () {
          saveBtn.disabled = true;
          saveBtn.textContent = "正在保存...";

          var toSelect = [];
          var toDrop = [];
          Object.keys(pendingIds).forEach(function (id) {
            if (!selectedIds[id]) toSelect.push(parseInt(id));
          });
          Object.keys(selectedIds).forEach(function (id) {
            if (!pendingIds[id]) toDrop.push(parseInt(id));
          });

          // 无变化，直接提示
          if (toSelect.length === 0 && toDrop.length === 0) {
            alert("未做任何修改，无需保存。");
            saveBtn.disabled = false;
            saveBtn.textContent = "💾 保存选课";
            return;
          }

          var errors = [];
          // 先退课
          for (var i = 0; i < toDrop.length; i++) {
            try {
              var r = await window.nativeApi.request("student.dropCourse", { classId: toDrop[i] });
              if (!r.success) errors.push(r.message);
            } catch (e) { errors.push(e.message); }
          }
          // 再选课
          for (var i = 0; i < toSelect.length; i++) {
            try {
              var r = await window.nativeApi.request("student.selectCourse", { classId: toSelect[i] });
              if (!r.success) errors.push(r.message);
            } catch (e) { errors.push(e.message); }
          }

          if (errors.length > 0) {
            alert("保存失败：" + errors.join("；") + "\n\n请检查后重试。");
          } else {
            alert("保存成功！");
          }

          await loadCourses();
          refresh();
        });
      }
    });
  }

  // ---- 加载课程列表 ----
  async function loadCourses() {
    try {
      allCourses = await window.nativeApi.request("student.getAvailableCourses", {});
      selectedIds = {};
      pendingIds = {};
      allCourses.forEach(function (c) {
        if (c.isSelected) {
          selectedIds[c.classId] = true;
          pendingIds[c.classId] = true;
        }
      });
    } catch (e) {
      allCourses = [];
      selectedIds = {};
      pendingIds = {};
    }
  }

  async function render(container) {
    containerRef = container;
    container.innerHTML = '<section class="panel"><div class="empty-state">正在加载课程列表...</div></section>';
    await loadCourses();
    refresh();
  }

  window.studentPages = window.studentPages || {};
  window.studentPages.courses = { render: render };
})();
