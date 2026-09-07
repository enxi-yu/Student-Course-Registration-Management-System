(function () {
  var selectedIds = {};       // 进入 batch 时的数据库基线（后端真实已选）
  var pendingIds = {};        // 当前编辑会话的最终目标状态（尚未写库）
  var allCourses = [];
  var scheduleData = {};
  var baseSchedule = [];
  var batches = [];
  var currentBatch = null;

  function escape(v) { return String(v || "").replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;"); }

  // ---- 查询某个教学班对象 ----
  function getCourse(classId) {
    for (var i = 0; i < allCourses.length; i++) {
      if (allCourses[i].classId === Number(classId)) return allCourses[i];
    }
    return null;
  }

  // ---- 统一判断是否允许临时选课（列表 checkbox 与 detail 共用）----
  function canSelectTemporarily(course) {
    if (!course) return false;
    if (pendingIds[course.classId]) return true;   // 已经临时选择
    if (selectedIds[course.classId]) return true;  // 原本拥有该名额，允许恢复
    return course.remaining > 0;                   // 否则需要有剩余容量
  }

  // ---- 临时选择（只改 pending，不写库）----
  function select(classId) {
    var cid = Number(classId);
    var course = getCourse(cid);
    if (!course) return { ok: false, message: "课程不存在" };
    if (!canSelectTemporarily(course)) return { ok: false, message: "该课程已满，无法选课。" };
    for (var key in pendingIds) {
      if (!pendingIds[key]) continue;
      var otherId = Number(key);
      if (otherId === cid) continue;
      var other = getCourse(otherId);
      if (other && other.courseId === course.courseId) {
        return { ok: false, message: "同一门课程只能选择一个教学班，请先取消已选择的其他教学班。" };
      }
    }
    pendingIds[cid] = true;
    return { ok: true };
  }

  // ---- 临时退课（只改 pending，不写库）----
  function drop(classId) {
    delete pendingIds[Number(classId)];
    return { ok: true };
  }

  function hasUnsavedChanges() {
    var a = Object.keys(selectedIds).sort();
    var b = Object.keys(pendingIds).sort();
    if (a.length !== b.length) return true;
    for (var i = 0; i < a.length; i++) {
      if (a[i] !== b[i]) return true;
    }
    return false;
  }

  function clearSession() {
    currentBatch = null;
    selectedIds = {};
    pendingIds = {};
    allCourses = [];
    scheduleData = {};
    baseSchedule = [];
  }

  // ---- 唯一 selection session（detail 通过它读写，而不是自己维护第二份状态）----
  window.studentCourseSelectionState = {
    hasSession: function () { return !!currentBatch; },
    getCurrentBatchId: function () { return currentBatch ? currentBatch.batchId : null; },
    getCurrentBatch: function () { return currentBatch; },
    getCourse: function (classId) { return getCourse(classId); },
    getCourseState: function (classId) { return !!pendingIds[Number(classId)]; },
    canSelect: function (classId) { return canSelectTemporarily(getCourse(classId)); },
    select: function (classId) { return select(classId); },
    drop: function (classId) { return drop(classId); },
    hasUnsavedChanges: function () { return hasUnsavedChanges(); }
  };

  // ---- 与“我的课表”一致的实时课表预览（基于 pendingIds）----
  function buildMiniSchedule() {
    var conflictIds = {};
    findConflicts().forEach(function (p) { conflictIds[p[0]] = true; conflictIds[p[1]] = true; });
    return window.sharedUi.timetable(selectedPreviewSchedule(), { conflictIds: conflictIds, cardClass: 'selection-preview-card' });
  }

  function buildCourseCard(c) {
    var checked = pendingIds[c.classId] ? " checked" : "";
    var badgeClass = c.remaining > 0 ? "available" : "full";
    var badgeText = c.remaining > 0 ? "剩余 " + c.remaining + " 人" : "已满";
    var disabled = !canSelectTemporarily(c) ? " disabled" : "";

    return '<div class="course-card">'
      + '<div class="course-card-check">'
        + '<input type="checkbox" class="course-checkbox" data-cid="' + c.classId + '"' + checked + disabled + '>'
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

  // ---- 渲染当前 batch 的课程列表 + 预览 ----
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

      // 返回批次（带未保存确认）
      containerRef.querySelector('.back-batches').addEventListener('click', async function () {
        if (hasUnsavedChanges()) {
          var ok = await window.sharedUi.confirm("当前有未保存的选课修改，返回后将丢失这些修改，确定返回吗？");
          if (!ok) return;
        }
        clearSession();
        renderBatchList();
      });

      // 刷新课程（带未保存确认）
      var refreshBtn = containerRef.querySelector(".refresh-btn");
      if (refreshBtn) refreshBtn.addEventListener("click", async function () {
        if (refreshBtn.disabled) return;
        if (hasUnsavedChanges()) {
          var ok = await window.sharedUi.confirm("当前有未保存的选课修改，刷新后将丢失这些修改，确定继续吗？");
          if (!ok) return;
        }
        window.sharedUi.setBusy(refreshBtn, true, "刷新中...");
        try {
          await loadCourses();
          refresh();
        } catch (e) {
          showLoadError(e);
        } finally { if (refreshBtn.isConnected) window.sharedUi.setBusy(refreshBtn, false); }
      });

      // checkbox 事件（只改 session，不写库）
      containerRef.querySelectorAll(".course-checkbox").forEach(function (cb) {
        cb.addEventListener("change", function () {
          var cid = parseInt(cb.dataset.cid);
          if (cb.checked) {
            var res = select(cid);
            if (!res.ok) {
              cb.checked = false;
              window.sharedUi.alert(res.message);
              return;
            }
          } else {
            drop(cid);
          }
          refresh();
        });
      });

      // 点击课程名进入详情（同 session，不传 pendingIds）
      containerRef.querySelectorAll(".click-detail").forEach(function (el) {
        el.addEventListener("click", function () {
          var cid = parseInt(el.dataset.cid);
          if (!isNaN(cid) && window.openStudentPage) window.openStudentPage("detail", { classId: cid, batchId: currentBatch.batchId, returnPage: "courses" });
        });
      });

      // 保存选课（原子接口）
      var saveBtn = containerRef.querySelector(".save-btn");
      if (saveBtn) {
        saveBtn.addEventListener("click", async function () {
          if (saveBtn.disabled) return;
          if (!hasUnsavedChanges()) {
            window.sharedUi.alert("未做任何修改，无需保存。");
            return;
          }
          window.sharedUi.setBusy(saveBtn, true, "正在保存...");
          var classIds = Object.keys(pendingIds).map(Number);
          try {
            var result = await window.nativeApi.request("student.saveCourseSelection", { batchId: currentBatch.batchId, classIds: classIds });
            if (!result || result.success !== true) {
              throw new Error(result && result.message ? result.message : "保存失败，请重试。");
            }
            window.sharedUi.alert(result.message || "保存成功！");
            await loadCourses();
            refresh();
          } catch (e) {
            window.sharedUi.alert("保存失败：" + e.message);
            // 失败不重载、不清空 pending，保持用户临时修改
          } finally { if (saveBtn.isConnected) window.sharedUi.setBusy(saveBtn, false); }
        });
      }
    });
  }

  // ---- 加载课程列表（失败抛出，由调用方区分 error/empty）----
  async function loadCourses() {
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
  }

  function showLoadError(e) {
    containerRef.innerHTML = '<section class="panel"><div class="message error">课程数据加载失败：' + escape(e.message) + '</div>'
      + '<button class="secondary-button retry-btn">重试</button>'
      + '<button class="secondary-button back-batches">返回批次</button></section>';
    var retryBtn = containerRef.querySelector(".retry-btn");
    if (retryBtn) retryBtn.addEventListener("click", function () { enterBatch(currentBatch); });
    var backBtn = containerRef.querySelector(".back-batches");
    if (backBtn) backBtn.addEventListener("click", function () { clearSession(); renderBatchList(); });
  }

  // ---- 进入某个批次并加载课程 ----
  async function enterBatch(batch) {
    currentBatch = batch;
    containerRef.innerHTML = '<section class="panel"><div class="empty-state">正在加载课程...</div></section>';
    try {
      await loadCourses();
      refresh();
    } catch (e) {
      showLoadError(e);
    }
  }

  function renderBatchList() {
    clearSession();
    var visible = batches.filter(function (b) { return Number(b.status) !== 2; });
    containerRef.innerHTML = '<section class="panel"><h3 class="panel-title">选课批次</h3><p class="page-description">请选择可参与的选课批次，进入后查看该批次开放的课程。</p><div class="selection-batch-list">'
      + (visible.length ? visible.map(function (b) {
          var active = Number(b.status) === 1;
          return '<button type="button" class="selection-batch-card' + (active ? ' active' : '') + '" data-batch="' + b.batchId + '"' + (active ? '' : ' disabled') + '><span><strong>' + escape(b.batchName) + '</strong><small>' + escape(b.startTime) + ' 至 ' + escape(b.endTime) + '</small></span><span class="status-badge">' + escape(b.statusText) + '</span></button>';
        }).join('') : '<div class="empty-state">当前没有面向你的选课批次。</div>') + '</div></section>';
    containerRef.querySelectorAll('.selection-batch-card.active').forEach(function (button) {
      button.addEventListener('click', function () {
        var target = batches.find(function (b) { return Number(b.batchId) === Number(button.dataset.batch); });
        if (target) enterBatch(target);
      });
    });
  }

  async function render(container, options) {
    containerRef = container;
    var requestedBatchId = options && options.batchId;

    // 从 detail 返回同一个 batch：只重渲染，保持当前 session 不重新加载
    if (requestedBatchId && currentBatch && Number(currentBatch.batchId) === Number(requestedBatchId)) {
      refresh();
      return;
    }

    container.innerHTML = '<section class="panel"><div class="empty-state">正在加载选课批次...</div></section>';
    try {
      batches = await window.nativeApi.request("student.getSelectionBatches", {});
    } catch (e) {
      batches = [];
      containerRef.innerHTML = '<section class="panel"><div class="message error">选课批次加载失败：' + escape(e.message) + '</div><button class="secondary-button retry-batches">重试</button></section>';
      var rb = containerRef.querySelector(".retry-batches");
      if (rb) rb.addEventListener("click", function () { render(container, options); });
      return;
    }

    if (requestedBatchId) {
      var target = batches.find(function (b) { return Number(b.batchId) === Number(requestedBatchId); });
      if (target) { await enterBatch(target); return; }
    }
    renderBatchList();
  }

  window.studentPages = window.studentPages || {};
  window.studentPages.courses = { render: render };
})();
