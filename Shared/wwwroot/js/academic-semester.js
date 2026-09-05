(function () {
  function getCurrent(date) {
    const current = date instanceof Date ? date : new Date();
    const year = current.getFullYear();
    const month = current.getMonth() + 1;
    const firstTerm = month >= 8 || month === 1;
    const startYear = month >= 8 ? year : year - 1;
    const term = firstTerm ? 1 : 2;

    return {
      startYear,
      endYear: startYear + 1,
      term,
      canonical: `${startYear}-${startYear + 1}-${term}`,
      legacy: term === 1 ? `${startYear}-fall` : `${startYear + 1}-spring`
    };
  }

  function resolve(availableSemesters, date) {
    const current = getCurrent(date);
    const available = (availableSemesters || []).map(value => String(value || "").trim()).filter(Boolean);
    const match = available.find(value => value.toLowerCase() === current.canonical.toLowerCase())
      || available.find(value => value.toLowerCase() === current.legacy.toLowerCase());

    return {
      display: format(current.canonical),
      query: match || current.canonical,
      legacy: current.legacy
    };
  }

  function format(semester) {
    const value = String(semester || "").trim();
    let match = value.match(/^(\d{4})-(\d{4})-([12])$/);
    if (match) {
      return `${match[1]}-${match[2]}学年 ${match[3] === "1" ? "第一学期" : "第二学期"}`;
    }

    match = value.match(/^(\d{4})-(spring|fall)$/i);
    if (match) {
      const year = Number(match[1]);
      return match[2].toLowerCase() === "spring"
        ? `${year - 1}-${year}学年 第二学期`
        : `${year}-${year + 1}学年 第一学期`;
    }

    return value || "未选择";
  }

  function academicYears(minimumStartYear, date) {
    const minimum = Number(minimumStartYear) || 2024;
    const maximum = getCurrent(date).startYear + 1;
    const years = [];
    for (let year = maximum; year >= minimum; year -= 1) years.push(year);
    return years;
  }

  function mountPicker(containerId, options) {
    const settings = options || {};
    const container = document.getElementById(containerId);
    if (!container) return null;

    const years = academicYears(settings.minimumStartYear || 2024);
    const initial = String(settings.selected || getCurrent().canonical).match(/^(\d{4})-(\d{4})-([12])$/);
    let selectedYear = initial ? Number(initial[1]) : getCurrent().startYear;
    let selectedTerm = initial ? Number(initial[3]) : getCurrent().term;
    let allSelected = Boolean(settings.allowAll && settings.selected === "");

    container.innerHTML = `
      <div class="semester-picker">
        <button class="semester-picker-trigger" type="button" aria-expanded="false">
          <span class="semester-picker-value"></span><span class="semester-picker-arrow" aria-hidden="true">⌄</span>
        </button>
        <div class="semester-picker-panel" hidden>
          ${settings.allowAll ? '<button class="semester-picker-all" type="button" data-semester-all>全部学期</button>' : ''}
          <div class="semester-year-grid">
            ${years.map(year => `<button type="button" data-semester-year="${year}">${year}-${year + 1}</button>`).join("")}
          </div>
          <div class="semester-term-grid">
            <button type="button" data-semester-term="1">第一学期</button>
            <button type="button" data-semester-term="2">第二学期</button>
          </div>
        </div>
      </div>`;

    const trigger = container.querySelector(".semester-picker-trigger");
    const panel = container.querySelector(".semester-picker-panel");
    const valueElement = container.querySelector(".semester-picker-value");

    function code() { return allSelected ? "" : `${selectedYear}-${selectedYear + 1}-${selectedTerm}`; }
    function update() {
      valueElement.textContent = allSelected ? "全部学期" : format(code());
      container.querySelectorAll("[data-semester-year]").forEach(button => button.classList.toggle("selected", !allSelected && Number(button.dataset.semesterYear) === selectedYear));
      container.querySelectorAll("[data-semester-term]").forEach(button => button.classList.toggle("selected", !allSelected && Number(button.dataset.semesterTerm) === selectedTerm));
      const allButton = container.querySelector("[data-semester-all]");
      if (allButton) allButton.classList.toggle("selected", allSelected);
    }

    trigger.addEventListener("click", () => {
      panel.hidden = !panel.hidden;
      trigger.setAttribute("aria-expanded", String(!panel.hidden));
    });
    container.querySelectorAll("[data-semester-year]").forEach(button => button.addEventListener("click", () => {
      allSelected = false;
      selectedYear = Number(button.dataset.semesterYear);
      update();
    }));
    container.querySelectorAll("[data-semester-term]").forEach(button => button.addEventListener("click", () => {
      allSelected = false;
      selectedTerm = Number(button.dataset.semesterTerm);
      update();
      panel.hidden = true;
      trigger.setAttribute("aria-expanded", "false");
      if (typeof settings.onChange === "function") settings.onChange(code(), format(code()));
    }));
    const allButton = container.querySelector("[data-semester-all]");
    if (allButton) allButton.addEventListener("click", () => {
      allSelected = true;
      update();
      panel.hidden = true;
      trigger.setAttribute("aria-expanded", "false");
      if (typeof settings.onChange === "function") settings.onChange("", "全部学期");
    });

    update();
    return { value: code, label: () => allSelected ? "全部学期" : format(code()) };
  }

  window.academicSemester = { getCurrent, resolve, format, academicYears, mountPicker };
})();
