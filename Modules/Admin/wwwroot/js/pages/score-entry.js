(function () {
  function escapeHtml(value) {
    return String(value ?? "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function calculate(score) {
    if (score >= 90) return { gradeLevel: "A", gpa: 4.0 };
    if (score >= 80) return { gradeLevel: "B", gpa: 3.0 };
    if (score >= 70) return { gradeLevel: "C", gpa: 2.0 };
    if (score >= 60) return { gradeLevel: "D", gpa: 1.0 };
    return { gradeLevel: "F", gpa: 0 };
  }

  function row(score) {
    const total = score.totalScore === null || score.totalScore === undefined ? "" : score.totalScore;
    const hasScore = total !== "";

    return `
      <tr data-student-no="${escapeHtml(score.studentNo)}" data-has-score="${hasScore ? "1" : "0"}">
        <td>${escapeHtml(score.studentNo)}</td>
        <td>${escapeHtml(score.studentName)}</td>
        <td>
          <input class="score-input" type="number" min="0" max="100" step="0.1" value="${total}">
        </td>
        <td class="grade-cell">${escapeHtml(score.gradeLevel || "-")}</td>
        <td class="gpa-cell">${score.gpa === null || score.gpa === undefined ? "-" : score.gpa}</td>
        <td class="credit-cell">${score.creditObtained || 0}</td>
        <td>
          <input class="remark-input" type="text" value="${escapeHtml(score.updateRemark || "")}" placeholder="${hasScore ? "修改原因" : "可选"}">
        </td>
        <td>
          <button class="primary-button js-save-score" type="button">保存</button>
        </td>
      </tr>
    `;
  }

  function updatePreview(tableRow) {
    const input = tableRow.querySelector(".score-input");
    const value = Number(input.value);
    if (input.value === "" || Number.isNaN(value)) {
      tableRow.querySelector(".grade-cell").textContent = "-";
      tableRow.querySelector(".gpa-cell").textContent = "-";
      return;
    }

    const preview = calculate(value);
    tableRow.querySelector(".grade-cell").textContent = preview.gradeLevel;
    tableRow.querySelector(".gpa-cell").textContent = preview.gpa.toFixed(1);
    input.dataset.dirty = "1";
  }

  function collectRow(tableRow, classId) {
    const scoreInput = tableRow.querySelector(".score-input");
    const remarkInput = tableRow.querySelector(".remark-input");
    const totalScore = Number(scoreInput.value);

    if (scoreInput.value === "" || Number.isNaN(totalScore) || totalScore < 0 || totalScore > 100) {
      throw new Error("总评成绩必须在 0 到 100 之间");
    }

    if (tableRow.dataset.hasScore === "1" && !remarkInput.value.trim()) {
      throw new Error("修改已有成绩时必须填写修改备注");
    }

    return {
      classId,
      studentNo: tableRow.dataset.studentNo,
      totalScore,
      updateRemark: remarkInput.value.trim()
    };
  }

  async function refresh(container, classId) {
    const scores = await window.nativeApi.request("score.getScoreSheet", { classId });
    const body = document.getElementById("scores-table-body");

    if (!scores || scores.length === 0) {
      body.innerHTML = `
        <tr>
          <td colspan="8"><div class="empty-state">暂无学生选课</div></td>
        </tr>
      `;
      return;
    }

    body.innerHTML = scores.map(row).join("");

    body.querySelectorAll(".score-input").forEach((input) => {
      input.addEventListener("input", () => updatePreview(input.closest("tr")));
    });

    body.querySelectorAll(".js-save-score").forEach((button) => {
      button.addEventListener("click", async () => {
        const tableRow = button.closest("tr");
        try {
          const payload = collectRow(tableRow, classId);
          await window.nativeApi.request("score.saveScore", payload);
          await refresh(container, classId);
        } catch (error) {
          alert(error.message);
        }
      });
    });
  }

  async function render(container, options) {
    const classId = Number(options && options.classId);
    const className = options && options.className ? options.className : `教学班 ${classId || "-"}`;

    if (!classId) {
      container.innerHTML = `
        <section class="panel">
          <h3 class="panel-title">成绩录入</h3>
          <div class="empty-state">请先从“我的课程”页面选择一个教学班。</div>
        </section>
      `;
      return;
    }

    container.innerHTML = `
      <section class="panel">
        <div class="toolbar">
          <div>
            <h3 class="panel-title">${escapeHtml(className)}</h3>
            <p class="metric-note">教学班编号：${classId}</p>
          </div>
          <button class="primary-button" type="button" id="batch-save-scores">批量保存</button>
        </div>
      </section>

      <section class="table-panel">
        <table class="data-table">
          <thead>
            <tr>
              <th>学号</th>
              <th>姓名</th>
              <th>总评成绩</th>
              <th>等级</th>
              <th>GPA</th>
              <th>获得学分</th>
              <th>修改备注</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody id="scores-table-body">
            <tr>
              <td colspan="8"><div class="empty-state">正在加载成绩单...</div></td>
            </tr>
          </tbody>
        </table>
      </section>
    `;

    document.getElementById("batch-save-scores").addEventListener("click", async () => {
      try {
        const rows = Array.from(document.querySelectorAll("#scores-table-body tr"))
          .filter((tableRow) => tableRow.querySelector(".score-input")?.dataset.dirty === "1")
          .map((tableRow) => collectRow(tableRow, classId));

        if (rows.length === 0) {
          alert("没有需要保存的成绩");
          return;
        }

        const result = await window.nativeApi.request("score.batchSaveScores", { classId, rows });
        alert(`批量保存成功：${result.savedCount} 条`);
        await refresh(container, classId);
      } catch (error) {
        alert(error.message);
      }
    });

    await refresh(container, classId);
  }

  window.teacherPages = window.teacherPages || {};
  window.teacherPages.scores = {
    render
  };
})();
