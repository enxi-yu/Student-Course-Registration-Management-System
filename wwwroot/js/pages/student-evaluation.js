(function () {
  function escapeHtml(value) {
    return String(value === null || value === undefined ? "" : value)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function formatDate(dateStr) {
    if (!dateStr) return "-";
    var d = new Date(dateStr);
    return d.getFullYear() + "-" +
           String(d.getMonth() + 1).padStart(2, "0") + "-" +
           String(d.getDate()).padStart(2, "0") + " " +
           String(d.getHours()).padStart(2, "0") + ":" +
           String(d.getMinutes()).padStart(2, "0");
  }

  // Render star rating display (read-only, 5 stars)
  function renderStars(rating) {
    var stars = "";
    for (var i = 1; i <= 5; i++) {
      if (i <= rating) {
        stars += '<span class="eval-star filled">&#9733;</span>';
      } else {
        stars += '<span class="eval-star">&#9734;</span>';
      }
    }
    return stars;
  }

  // Render clickable star input
  function renderStarInput(currentRating, classId) {
    var stars = "";
    for (var i = 1; i <= 5; i++) {
      if (i <= currentRating) {
        stars += '<span class="eval-star eval-star-input filled" data-rating="' + i + '" data-class="' + classId + '">&#9733;</span>';
      } else {
        stars += '<span class="eval-star eval-star-input" data-rating="' + i + '" data-class="' + classId + '">&#9734;</span>';
      }
    }
    return stars;
  }

  // Render evaluated course card
  function renderEvaluatedCard(item) {
    return (
      '<div class="eval-card evaluated">' +
        '<div class="eval-card-header">' +
          '<div class="eval-course-name">' + escapeHtml(item.courseName) + '</div>' +
          '<div class="eval-status done">已评价</div>' +
        '</div>' +
        '<div class="eval-card-info">' +
          '<span>' + escapeHtml(item.courseCode) + '</span>' +
          '<span class="eval-dot">·</span>' +
          '<span>' + escapeHtml(item.teacherName) + '</span>' +
          '<span class="eval-dot">·</span>' +
          '<span>' + escapeHtml(item.semester) + '</span>' +
          '<span class="eval-dot">·</span>' +
          '<span>' + item.credit + ' 学分</span>' +
        '</div>' +
        '<div class="eval-result">' +
          '<div class="eval-rating-display">' + renderStars(item.rating) + '</div>' +
          (item.comment ? '<div class="eval-comment-display">' + escapeHtml(item.comment) + '</div>' : '') +
          '<div class="eval-date">' + formatDate(item.evaluationDate) + '</div>' +
        '</div>' +
      '</div>'
    );
  }

  // Render pending evaluation card (not yet evaluated, with input)
  function renderPendingCard(item) {
    return (
      '<div class="eval-card pending">' +
        '<div class="eval-card-header">' +
          '<div class="eval-course-name">' + escapeHtml(item.courseName) + '</div>' +
          '<div class="eval-status pending-status">待评价</div>' +
        '</div>' +
        '<div class="eval-card-info">' +
          '<span>' + escapeHtml(item.courseCode) + '</span>' +
          '<span class="eval-dot">·</span>' +
          '<span>' + escapeHtml(item.teacherName) + '</span>' +
          '<span class="eval-dot">·</span>' +
          '<span>' + escapeHtml(item.semester) + '</span>' +
          '<span class="eval-dot">·</span>' +
          '<span>' + item.credit + ' 学分</span>' +
        '</div>' +
        '<div class="eval-form" data-class="' + item.classId + '">' +
          '<div class="eval-form-row">' +
            '<label class="eval-label">评分</label>' +
            '<div class="eval-star-group" id="star-group-' + item.classId + '">' +
              renderStarInput(0, item.classId) +
            '</div>' +
          '</div>' +
          '<div class="eval-form-row">' +
            '<label class="eval-label">评语</label>' +
            '<textarea class="eval-textarea" id="eval-comment-' + item.classId + '" placeholder="请输入你的课程评价..." maxlength="500" rows="3"></textarea>' +
          '</div>' +
          '<div class="eval-form-row">' +
            '<button class="eval-submit-btn" type="button" data-class="' + item.classId + '">提交评价</button>' +
          '</div>' +
        '</div>' +
      '</div>'
    );
  }

  // Render evaluation history section
  function renderHistory(historyItems) {
    if (!historyItems || historyItems.length === 0) {
      return '<section class="panel"><h3 class="panel-title">历史评价</h3><div class="empty-state">暂无历史评价记录</div></section>';
    }

    var items = historyItems.map(function(item) {
      return (
        '<div class="eval-card history-item">' +
          '<div class="eval-card-header">' +
            '<div class="eval-course-name">' + escapeHtml(item.courseName) + '</div>' +
            '<div class="eval-date">' + formatDate(item.evaluationDate) + '</div>' +
          '</div>' +
          '<div class="eval-card-info">' +
            '<span>' + escapeHtml(item.courseCode) + '</span>' +
            '<span class="eval-dot">·</span>' +
            '<span>' + escapeHtml(item.teacherName) + '</span>' +
            '<span class="eval-dot">·</span>' +
            '<span>' + escapeHtml(item.semester) + '</span>' +
          '</div>' +
          '<div class="eval-result">' +
            '<div class="eval-rating-display">' + renderStars(item.rating) + '</div>' +
            (item.comment ? '<div class="eval-comment-display">' + escapeHtml(item.comment) + '</div>' : '') +
          '</div>' +
        '</div>'
      );
    }).join("");

    return (
      '<section class="panel">' +
        '<h3 class="panel-title">历史评价</h3>' +
        items +
      '</section>'
    );
  }

  function bindStarEvents(container) {
    var starInputs = container.querySelectorAll(".eval-star-input");
    starInputs.forEach(function(star) {
      star.addEventListener("click", function() {
        var rating = parseInt(this.dataset.rating);
        var classId = parseInt(this.dataset.class);
        var group = document.getElementById("star-group-" + classId);
        if (!group) return;
        group.dataset.rating = rating;
        group.querySelectorAll(".eval-star-input").forEach(function(s, idx) {
          if (idx < rating) {
            s.classList.add("filled");
            s.innerHTML = "&#9733;";
          } else {
            s.classList.remove("filled");
            s.innerHTML = "&#9734;";
          }
        });
      });

      star.addEventListener("mouseenter", function() {
        var rating = parseInt(this.dataset.rating);
        var classId = parseInt(this.dataset.class);
        var group = document.getElementById("star-group-" + classId);
        if (!group) return;
        group.querySelectorAll(".eval-star-input").forEach(function(s, idx) {
          if (idx < rating) {
            s.classList.add("hover");
          }
        });
      });

      star.addEventListener("mouseleave", function() {
        var classId = parseInt(this.dataset.class);
        var group = document.getElementById("star-group-" + classId);
        if (!group) return;
        group.querySelectorAll(".eval-star-input").forEach(function(s) {
          s.classList.remove("hover");
        });
      });
    });
  }

  function bindSubmitEvents(container) {
    var buttons = container.querySelectorAll(".eval-submit-btn");
    buttons.forEach(function(btn) {
      btn.addEventListener("click", async function() {
        var classId = parseInt(this.dataset.class);
        var group = document.getElementById("star-group-" + classId);
        var rating = parseInt(group ? group.dataset.rating || 0 : 0);
        var textarea = document.getElementById("eval-comment-" + classId);
        var comment = textarea ? textarea.value.trim() : "";

        if (rating < 1 || rating > 5) {
          window.setStudentMessage("请先选择评分（1-5星）", "error");
          return;
        }

        btn.disabled = true;
        btn.textContent = "提交中...";

        try {
          await window.nativeApi.request("student.submitEvaluation", {
            classId: classId,
            rating: rating,
            comment: comment
          });
          window.setStudentMessage("评价提交成功！", "success");
          // Reload the page
          render(container);
        } catch (error) {
          btn.disabled = false;
          btn.textContent = "提交评价";
          window.setStudentMessage("提交失败：" + error.message, "error");
        }
      });
    });
  }

  // Expose setMessage to global for the module's use
  window.setStudentMessage = function(msg, type) {
    var root = document.getElementById("message-root");
    if (!msg) {
      root.innerHTML = "";
      return;
    }
    root.innerHTML = '<div class="message ' + (type || "") + '">' + escapeHtml(msg) + '</div>';
  };

  async function render(container) {
    // Clear message
    if (document.getElementById("message-root")) {
      document.getElementById("message-root").innerHTML = "";
    }

    container.innerHTML = (
      '<section class="panel">' +
        '<h3 class="panel-title">课程评价</h3>' +
        '<div class="empty-state">正在加载评价数据...</div>' +
      '</section>'
    );

    try {
      var evaluations = await window.nativeApi.request("student.getEvaluations", {});

      if (!evaluations || evaluations.length === 0) {
        container.innerHTML = (
          '<section class="panel">' +
            '<h3 class="panel-title">课程评价</h3>' +
            '<div class="empty-state">暂无需要评价的课程</div>' +
          '</section>'
        );
        return;
      }

      var pending = evaluations.filter(function(e) { return !e.hasEvaluated; });
      var done = evaluations.filter(function(e) { return e.hasEvaluated; });

      var html = "";

      // Pending evaluations section
      if (pending.length > 0) {
        html += '<section class="panel"><h3 class="panel-title">待评价课程</h3>';
        html += pending.map(renderPendingCard).join("");
        html += '</section>';
      }

      // Completed evaluations section
      if (done.length > 0) {
        html += '<section class="panel"><h3 class="panel-title">已评价课程</h3>';
        html += done.map(renderEvaluatedCard).join("");
        html += '</section>';
      }

      container.innerHTML = html;

      // Bind events
      bindStarEvents(container);
      bindSubmitEvents(container);
    } catch (error) {
      container.innerHTML = (
        '<section class="panel">' +
          '<h3 class="panel-title">课程评价</h3>' +
          '<div class="message error">加载评价数据失败：' + escapeHtml(error.message) + '</div>' +
        '</section>'
      );
    }
  }

  window.studentPages = window.studentPages || {};
  window.studentPages.evaluation = { render: render };
})();
