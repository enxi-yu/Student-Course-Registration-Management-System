(function () {
  var selectedIds = {};       // 当前已选 classId -> true (后端)
  var pendingIds = {};        // 本地暂存 classId -> true (还没提交)
  var allCourses = [];
  var scheduleData = {};
  var baseSchedule = [];
  var batches = [];
  var currentBatch = null;

  var WEEKDAY = ["", "周一", "周二", "周三", "周四", "周五", "周六", "周日"];
  var PERIODS = [[1,2], [3,4], [5,6], [7,8], [9,10], [11,12]];

  function escape(v) { return String(v || "").replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;"); }

  // ---- 与“我的课表”一致的实时课表预览 ----
  function buildMiniSchedule() {
    var conflictIds = {};
    findConflicts().forEach(function (p) { conflictIds[p[0]] = true; conflictIds[p[1]] = true; });
    return window.sharedUi.timetable(selectedPreviewSchedule(), { conflictIds: conflictIds, cardClass: 'selection-preview-card' });
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
    var grouped = {};
    selectedPreviewSchedule().forEach(function (s) { (grouped[s.classId] = grouped[s.classId] || []).push(s); });
    var cids = Object.keys(grouped).map(Number);
    for (var i = 0; i < cids.length; i++) {
      var a = grouped[cids[i]];
      if (!a) continue;
      for (var j = i + 1; j < cids.length; j++) {
        var b = grouped[cids[j]];
        if (!b) continue;
        for (var ai = 0; ai < a.length; ai++) {
          for (var bi = 0; bi < b.length; bi++) {
            if (a[ai].weekday === b[bi].weekday
              && a[ai].startPeriod <= b[bi].endPeriod
              && a[ai].endPeriod >= b[bi].startPeriod
              && weeksOverlap(a[ai].weekRange, b[bi].weekRange)) {
              conflicts.push([cids[i], cids[j]]);
            }
          }
        }
      }
    }
    return conflicts;
  }

  function selectedPreviewSchedule() {
    var batchIds = {};
    allCourses.forEach(function (c) { batchIds[c.classId] = true; });
    var result = baseSchedule.filter(function (s) { return !batchIds[s.classId] || pendingIds[s.classId]; });
    var known = {};
    result.forEach(function (s) { known[s.classId] = true; });
    Object.keys(pendingIds).forEach(function (id) {
      if (!known[id]) result = result.concat(scheduleData[Number(id)] || []);
    });
    return result;
  }

  function weeksOverlap(a, b) {
    var an = String(a || '').match(/\d+/g) || ['1','99'];
    var bn = String(b || '').match(/\d+/g) || ['1','99'];
    var amin = Math.min.apply(null, an.map(Number)), amax = Math.max.apply(null, an.map(Number));
    var bmin = Math.min.apply(null, bn.map(Number)), bmax = Math.max.apply(null, bn.map(Number));
    return amin <= bmax && bmin <= amax;
  }

  // ---- 刷新界面 ----
  var containerRef;
  function refresh() {
    loadScheduleCache().then(function () {
      var conflicts = findConflicts();
      var conflictIds = {};
      conflicts.forEach(function (p) { conflictIds[p[0]] = true; conflictIds[p[1]] = true; });

      containerRef.innerHTML = ''
        + '<div class="batch-context"><button class="secondary-button back-batches">返回批次</button><div><strong>' + escape(currentBatch.batchName) + '</strong><span>' + escape(currentBatch.startTime) + ' 至 ' + escape(currentBatch.endTime) + '</span></div></div>'
        + '<section class="panel"><h3 class="panel-title">我的课表预览</h3>'
          + buildMiniSchedule()
          + (conflicts.length > 0 ? '<div class="message error">存在上课周次和节次重叠，请调整课程后再保存。</div>' : '')
        + '</section>'
        + '<section class="panel"><div style="display:flex; justify-content:space-between; align-items:center;">'
          + '<h3 class="panel-title" style="margin:0;">可选课程</h3>'
          + '<div style="display:flex; gap:8px;"><button class="secondary-button refresh-btn">刷新课程</button><button class="primary-button save-btn">保存选课</button></div>'
        + '</div></section>'
        + '<div class="course-list">'
          + (allCourses.length ? allCourses.map(buildCourseCard).join("") : '<section class="panel"><div class="empty-state">当前没有开放的选课批次，或本学期暂无可选课程。</div></section>')
        + '</div>';

      containerRef.querySelector('.back-batches').addEventListener('click', function () {
        currentBatch = null;
        showBatchList();
      });

      var refreshBtn = containerRef.querySelector(".refresh-btn");
      if (refreshBtn) refreshBtn.addEventListener("click", async function () {
        if (refreshBtn.disabled) return;
        window.sharedUi.setBusy(refreshBtn, true, "刷新中...");
        try { await loadCourses(); refresh(); }
        finally { if (refreshBtn.isConnected) window.sharedUi.setBusy(refreshBtn, false); }
      });

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
        saveBtn.disabled = conflicts.length > 0;
        saveBtn.addEventListener("click", async function () {
          if (saveBtn.disabled) return;
          window.sharedUi.setBusy(saveBtn, true, "正在保存...");

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
            window.sharedUi.setBusy(saveBtn, false);
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
              var r = await window.nativeApi.request("student.selectCourse", { classId: toSelect[i], batchId: currentBatch.batchId });
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
      allCourses = await window.nativeApi.request("student.getAvailableCourses", { batchId: currentBatch.batchId });
      selectedIds = {};
      pendingIds = {};
      allCourses.forEach(function (c) {
        if (c.isSelected) {
          selectedIds[c.classId] = true;
          pendingIds[c.classId] = true;
        }
      });
      var semester = allCourses.length ? allCourses[0].semester : window.academicSemester.getCurrent().canonical;
      baseSchedule = await window.nativeApi.request("student.getSchedule", { semester: semester });
    } catch (e) {
      allCourses = [];
      selectedIds = {};
      pendingIds = {};
      baseSchedule = [];
    }
  }

  function showBatchList() {
    containerRef.innerHTML = '<section class="panel"><h3 class="panel-title">选课批次</h3><p class="page-description">请选择可参与的选课批次，进入后查看该批次开放的课程。</p><div class="selection-batch-list">'
      + (batches.filter(function (b) { return Number(b.status) !== 2; }).length ? batches.filter(function (b) { return Number(b.status) !== 2; }).map(function (b) {
          var active = Number(b.status) === 1;
          return '<button type="button" class="selection-batch-card' + (active ? ' active' : '') + '" data-batch="' + b.batchId + '"' + (active ? '' : ' disabled') + '><span><strong>' + escape(b.batchName) + '</strong><small>' + escape(b.startTime) + ' 至 ' + escape(b.endTime) + '</small></span><span class="status-badge">' + escape(b.statusText) + '</span></button>';
        }).join('') : '<div class="empty-state">当前没有面向你的选课批次。</div>') + '</div></section>';
    containerRef.querySelectorAll('.selection-batch-card.active').forEach(function (button) {
      button.addEventListener('click', async function () {
        currentBatch = batches.find(function (b) { return Number(b.batchId) === Number(button.dataset.batch); });
        await loadCourses();
        refresh();
      });
    });
  }

  async function render(container) {
    containerRef = container;
    container.innerHTML = '<section class="panel"><div class="empty-state">正在加载选课批次...</div></section>';
    try { batches = await window.nativeApi.request("student.getSelectionBatches", {}); } catch (e) { batches = []; }
    currentBatch = null;
    showBatchList();
  }

  window.studentPages = window.studentPages || {};
  window.studentPages.courses = { render: render };
})();
