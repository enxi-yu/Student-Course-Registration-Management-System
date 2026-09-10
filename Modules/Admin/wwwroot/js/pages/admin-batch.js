let adminBatches = [];
let batchClassOptions = [];
let batchMajors = [];
let batchGrades = [];
let batchFormController;
let batchPage = 1;

function applyBatchAccess() {
    const formCard = document.querySelector('#batches-tab > .form-card');
    if (formCard) formCard.style.display = Number(window.ADMIN_LEVEL) === 0 ? '' : 'none';
}

function ensureBatchForm() {
    if (!batchFormController) batchFormController = window.sharedUi.createForm({
        root: '#batches-tab', submitButton: '#saveBatchButton', busyText: '保存中...',
        fields: [
            { name: 'batchName', label: '批次名称', selector: '#batchName', required: true },
            { name: 'startTime', label: '开始时间', selector: '#batchStartTime', required: true },
            { name: 'endTime', label: '结束时间', selector: '#batchEndTime', required: true, validate: (value, values) => (value && values.startTime && new Date(value) <= new Date(values.startTime)) ? '结束时间必须晚于开始时间' : '' },
            { name: 'semester', label: '所属学期', selector: '#batchSemester', required: true, requiredMessage: '请先选择所属学期' },
            { name: 'classIds', label: '开放课程', read: () => batchChecked('batchCourseOptions').map(Number), required: true, requiredMessage: '请至少选择一门开放课程' },
            { name: 'majors', label: '面向专业', read: () => batchChecked('batchMajorOptions'), required: true, requiredMessage: '请至少选择一个面向专业' },
            { name: 'grades', label: '面向年级', read: () => batchChecked('batchGradeOptions'), required: true, requiredMessage: '请至少选择一个面向年级' }
        ]
    });
    return batchFormController;
}

function batchChecked(containerId) {
    return Array.from(document.querySelectorAll(`#${containerId} input[type="checkbox"]:checked`)).map(x => x.value);
}

function updateBatchMulti(id, emptyText) {
    const root = document.getElementById(id);
    const values = Array.from(root.querySelectorAll('.batch-multi-option input[type="checkbox"]:checked')).map(x => x.dataset.label || x.value);
    root.querySelector('.batch-multi-trigger').textContent = values.length ? `已选择 ${values.length} 项：${values.slice(0, 2).join('、')}${values.length > 2 ? '…' : ''}` : emptyText;
}

// 全选框基于当前筛选的情况显示
function updateBatchCheckAll(root) {
    const checkAll = root.querySelector('.batch-multi-checkall input');
    if (!checkAll) return;
    const options = Array.from(root.querySelectorAll('.batch-multi-option'));
    const visible = options.filter(o => o.style.display !== 'none');
    const total = visible.length;
    const selected = visible.filter(o => o.querySelector('input').checked).length;
    checkAll.indeterminate = selected > 0 && selected < total;
    checkAll.checked = total > 0 && selected === total;
}

function renderBatchOptions(containerId, items, selected, valueKey, labelBuilder) {
    const box = document.getElementById(containerId);
    const selectedSet = new Set((selected || []).map(String));
    box.innerHTML = items.map(item => {
        const value = String(typeof item === 'object' ? item[valueKey] : item);
        const labels = labelBuilder(item);
        return `<label class="batch-multi-option" data-search="${adminEscape(labels.search.toLowerCase())}"><input type="checkbox" value="${adminEscape(value)}" data-label="${adminEscape(labels.title)}" ${selectedSet.has(value) ? 'checked' : ''}><span>${adminEscape(labels.title)}${labels.detail ? `<small>${adminEscape(labels.detail)}</small>` : ''}</span></label>`;
    }).join('');
}

function wireBatchMulti(id, emptyText) {
    const root = document.getElementById(id);
    const menu = root.querySelector('.batch-multi-menu');
    menu.querySelectorAll('.batch-multi-checkall').forEach(x => x.remove());
    const checkAll = document.createElement('label');
    checkAll.className = 'batch-multi-checkall';
    checkAll.innerHTML = '<input type="checkbox"><span>全选</span>';
    menu.insertBefore(checkAll, root.querySelector('.batch-multi-options'));
    const refresh = () => { updateBatchCheckAll(root); updateBatchMulti(id, emptyText); };
    root.querySelector('.batch-multi-trigger').onclick = event => { event.stopPropagation(); document.querySelectorAll('.batch-multi.open').forEach(x => { if (x !== root) x.classList.remove('open'); }); root.classList.toggle('open'); };
    root.querySelectorAll('.batch-multi-option input').forEach(x => x.onchange = refresh);
    // 全选点击作用于全部项
    checkAll.querySelector('input').onchange = () => {
        const check = checkAll.querySelector('input').checked;
        root.querySelectorAll('.batch-multi-option input[type="checkbox"]').forEach(x => { x.checked = check; });
        refresh();
    };
    // 搜索仅做显示过滤，删除输入后勾选状态保留
    const search = root.querySelector('.batch-multi-search');
    if (search) search.oninput = () => { root.querySelectorAll('.batch-multi-option').forEach(x => x.style.display = x.dataset.search.includes(search.value.trim().toLowerCase()) ? '' : 'none'); updateBatchCheckAll(root); };
    refresh();
}

async function loadBatchLookups() {
    if (batchClassOptions.length) return;
    const [classes, audiences] = await Promise.all([adminFetch('/api/admin/classes'), adminFetch('/api/admin/lookups/student-audiences')]);
    batchClassOptions = classes;
    batchMajors = audiences.majors || [];
    batchGrades = audiences.grades || [];
    wireBatchSemester();
    fillBatchSelectors([], [], [], '');
}

function batchCourseLabel(x) {
    return { title: `${x.courseName} · ${x.className}`, detail: `${x.teacherName || '未分配教师'} · ${x.semester}`, search: `${x.courseName} ${x.className} ${x.teacherName || ''} ${x.semester}` };
}

function batchSemesterList() {
    return [...new Set(batchClassOptions.map(x => x.semester).filter(Boolean))];
}

let batchSemester = '';

// 未选学期时不渲染课程并提示先选学期；选中/切换学期仅加载该学期教学班
function fillBatchSelectors(classIds, majors, grades, semester) {
    const courses = semester ? batchClassOptions.filter(x => x.semester === semester) : [];
    renderBatchOptions('batchCourseOptions', courses, semester ? (classIds || []) : [], 'classId', batchCourseLabel);
    renderBatchOptions('batchMajorOptions', batchMajors, majors || [], '', x => ({ title: x, detail: '', search: x }));
    renderBatchOptions('batchGradeOptions', batchGrades, grades || [], '', x => ({ title: x, detail: '', search: x }));
    wireBatchMulti('batchCourseMulti', semester ? '请选择开放课程' : '请先选择所属学期');
    wireBatchMulti('batchMajorMulti', '请选择面向专业');
    wireBatchMulti('batchGradeMulti', '请选择面向年级');
}

function fillBatchSemesterSelect(select, value) {
    select.innerHTML = '<option value="">请选择学期</option>' + batchSemesterList().map(s => `<option value="${adminEscape(s)}">${adminEscape(s)}</option>`).join('');
    select.value = value || '';
}

function wireBatchSemester() {
    const select = document.getElementById('batchSemester');
    if (!select) return;
    fillBatchSemesterSelect(select, '');
    select.onchange = async () => {
        if (batchChecked('batchCourseOptions').length && !(await window.sharedUi.confirm('切换学期将清空已勾选的开放课程，确定切换吗？'))) { select.value = batchSemester; return; }
        batchSemester = select.value;
        fillBatchSelectors([], batchChecked('batchMajorOptions'), batchChecked('batchGradeOptions'), batchSemester);
    };
}

async function loadBatches(resetPage = true) {
    const tbody = document.getElementById('batchTableBody'); if (!tbody) return;
    if (resetPage) batchPage = 1;
    window.sharedUi.setTableState(tbody, 6, '正在加载选课批次...');
    try {
        applyBatchAccess();
        if (Number(window.ADMIN_LEVEL) === 0) await loadBatchLookups();
        adminBatches = await adminFetch('/api/admin/batches');
        batchPage = window.sharedUi.renderPagedTable({body:tbody,rows:adminBatches,page:batchPage,pageSize:10,colspan:6,emptyText:'暂无选课批次',pagination:'batchPagination',onPageChange:page=>{batchPage=page;loadBatches(false);},row:item=>`<tr><td>${adminEscape(item.batchId)}</td><td>${adminEscape(item.batchName)}</td><td>${adminEscape(item.startTime)}</td><td>${adminEscape(item.endTime)}</td><td>${adminBatchBadge(item.status,item.statusText)}</td><td>${Number(window.ADMIN_LEVEL) === 0 ? `<button class="btn btn-sm btn-edit table-action" onclick="editBatch(${item.batchId})">编辑</button>${Number(item.status) === 1 ? ` <button class="btn btn-sm btn-delete table-action" onclick="endBatch(${item.batchId})">结束</button>` : ''}` : '<span class="muted">仅查看</span>'}</td></tr>`});
    } catch (error) { window.sharedUi.setTableError(tbody,6,error.message,()=>loadBatches(false)); }
}

async function saveBatch() {
    if (Number(window.ADMIN_LEVEL) !== 0) return alert('普通教务管理员只能查看选课批次');
    if (ensureBatchForm().isSubmitting()) return;
    const batchId = document.getElementById('batchId').value;
    try {
        await ensureBatchForm().submit(async values => {
            const payload = { batchName: values.batchName, startTime: values.startTime, endTime: values.endTime };
            const batch = await adminFetch(batchId ? `/api/admin/batches/${batchId}` : '/api/admin/batches', { method:batchId?'PUT':'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify(payload) });
            await adminFetch(`/api/admin/batches/${batch.batchId}/offerings`, { method:'PUT', headers:{'Content-Type':'application/json'}, body:JSON.stringify({ offerings:values.classIds.map(classId => ({classId,majors:values.majors,grades:values.grades})) }) });
        });
        alert('选课批次已保存'); clearBatchForm(); await loadBatches();
    } catch (error) { alert('保存失败：'+error.message); }
}

// 弹窗内使用复选框多选组件
function dialogBatchList(dialog, name, config) {
    const input = dialog.form.elements[name];
    if (!input || !input.parentNode) return;
    const wrap = document.createElement('div');
    wrap.className = 'batch-multi';
    wrap.id = config.wrapId;
    const searchHtml = config.searchPlaceholder ? `<input type="search" class="batch-multi-search" placeholder="${config.searchPlaceholder}">` : '';
    wrap.innerHTML = `<button type="button" class="batch-multi-trigger">${config.emptyText}</button><div class="batch-multi-menu">${searchHtml}<div class="batch-multi-options" id="${config.optionsId}"></div></div>`;
    input.replaceWith(wrap);
    renderBatchOptions(config.optionsId, config.items, config.selected, config.valueKey, config.labelBuilder);
    wireBatchMulti(wrap.id, config.emptyText);
}

async function editBatch(batchId) {
    if (Number(window.ADMIN_LEVEL) !== 0) return alert('普通教务管理员只能查看选课批次');
    const item = adminBatches.find(x => Number(x.batchId) === Number(batchId)); if (!item) return;
    try {
        const offerings = await adminFetch(`/api/admin/batches/${batchId}/offerings`);
        const selected = offerings.filter(x => x.selected);
        const selectedMajors = [...new Set(selected.flatMap(x => x.majors || []))];
        const selectedGrades = [...new Set(selected.flatMap(x => x.grades || []))];
        const classSemesters = [...new Set(selected.map(x => x.semester).filter(Boolean))];
        const initSemester = classSemesters.length === 1 ? classSemesters[0] : '';
        if (classSemesters.length > 1) await window.sharedUi.alert('该批次已开放的课程跨了多个学期，请选择目标学期后重新保存，其它学期的课程将被移除。');
        const dialog = window.sharedUi.formDialog({
            title: '编辑选课批次', description: '先选择所属学期，再在该学期的教学班中勾选开放课程。', submitText: '保存修改',
            fields: [
                { name: 'batchName', label: '批次名称', required: true, value: item.batchName || '' },
                { name: 'semester', label: '所属学期', kind: 'select', required: true, value: initSemester, options: batchSemesterList() },
                { name: 'startTime', label: '开始时间', type: 'datetime-local', required: true, value: toDateTimeLocal(item.startTime) },
                { name: 'endTime', label: '结束时间', type: 'datetime-local', required: true, value: toDateTimeLocal(item.endTime) },
                { name: 'classIds', label: '开放课程', kind: 'multiselect', wide: true, required: true, value: [], options: [] },
                { name: 'majors', label: '面向专业', kind: 'multiselect', required: true, value: [], options: [] },
                { name: 'grades', label: '面向年级', kind: 'multiselect', required: true, value: [], options: [] }
            ]
        });
        const dialogForm = dialog.form, submitButton = dialogForm.querySelector('[type="submit"]');
        const setBusy = busy => { submitButton.disabled = busy; submitButton.textContent = busy ? '保存中...' : '保存修改'; };
        const dialogClassCfg = { wrapId: 'dlgClassMulti', optionsId: 'dlgClassOptions', emptyText: '请选择开放课程', searchPlaceholder: '搜索课程、教学班或教师', items: initSemester ? batchClassOptions.filter(x => x.semester === initSemester) : [], selected: selected.filter(x => x.semester === initSemester).map(x => x.classId), valueKey: 'classId', labelBuilder: batchCourseLabel };
        dialogBatchList(dialog, 'classIds', dialogClassCfg);
        dialogBatchList(dialog, 'majors', { wrapId: 'dlgMajorMulti', optionsId: 'dlgMajorOptions', emptyText: '请选择', items: batchMajors, selected: selectedMajors, valueKey: '', labelBuilder: x => ({ title: x, detail: '', search: x }) });
        dialogBatchList(dialog, 'grades', { wrapId: 'dlgGradeMulti', optionsId: 'dlgGradeOptions', emptyText: '请选择', items: batchGrades, selected: selectedGrades, valueKey: '', labelBuilder: x => ({ title: x, detail: '', search: x }) });
        let dialogSemester = initSemester;
        const semesterSelect = dialogForm.elements['semester'];
        semesterSelect.addEventListener('change', async () => {
            if (batchChecked('dlgClassOptions').length && !(await window.sharedUi.confirm('切换学期将清空已勾选的开放课程，确定切换吗？'))) { semesterSelect.value = dialogSemester; return; }
            dialogSemester = semesterSelect.value;
            dialogClassCfg.items = dialogSemester ? batchClassOptions.filter(x => x.semester === dialogSemester) : [];
            dialogClassCfg.selected = [];
            renderBatchOptions(dialogClassCfg.optionsId, dialogClassCfg.items, [], dialogClassCfg.valueKey, dialogClassCfg.labelBuilder);
            wireBatchMulti(dialogClassCfg.wrapId, dialogClassCfg.emptyText);
        });
        dialogForm.onsubmit = async event => {
            event.preventDefault();
            if (submitButton.disabled) return;
            const start = dialogForm.elements['startTime']?.value || '', end = dialogForm.elements['endTime']?.value || '';
            if (start && end && new Date(end).getTime() <= new Date(start).getTime()) { window.sharedUi.alert('结束时间必须晚于开始时间'); return; }
            const classIds = batchChecked('dlgClassOptions').map(Number);
            if (!classIds.length) { window.sharedUi.alert('请至少选择一门开放课程'); return; }
            const majors = batchChecked('dlgMajorOptions'), grades = batchChecked('dlgGradeOptions');
            if (!majors.length) { window.sharedUi.alert('请至少选择一个面向专业'); return; }
            if (!grades.length) { window.sharedUi.alert('请至少选择一个面向年级'); return; }
            setBusy(true);
            try {
                const batch = await adminFetch(`/api/admin/batches/${batchId}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ batchName: dialogForm.elements['batchName'].value.trim(), startTime: start, endTime: end }) });
                await adminFetch(`/api/admin/batches/${batch.batchId}/offerings`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ offerings: classIds.map(classId => ({ classId, majors, grades })) }) });
                dialog.close();
                await window.sharedUi.alert('选课批次已保存');
                await loadBatches();
            } catch (error) { 
                setBusy(false); window.sharedUi.alert('保存失败：' + window.sharedUi.errorText(error)); 
            }
        };
    } catch (error) { 
        alert('读取批次配置失败：' + error.message); 
    }
}

async function endBatch(batchId) {
    if (Number(window.ADMIN_LEVEL) !== 0) return alert('普通教务管理员只能查看选课批次');
    if (!await window.sharedUi.confirm('确定立即结束这个选课批次吗？结束后学生将不能继续选课。')) return;
    try {
        await adminFetch(`/api/admin/batches/${batchId}/end`, {method:'PUT'});
        await loadBatches(false);
        await window.sharedUi.alert('批次已结束');
    } catch(error){ alert('结束失败：'+window.sharedUi.errorText(error)); }
}

function clearBatchForm() {
    document.getElementById('batchFormTitle').textContent='新增选课批次'; document.getElementById('batchId').value=''; document.getElementById('batchName').value=''; document.getElementById('batchStartTime').value=''; document.getElementById('batchEndTime').value='';
    const semesterSelect = document.getElementById('batchSemester'); if (semesterSelect) { batchSemester=''; semesterSelect.value=''; }
    fillBatchSelectors([], [], [], '');
}
function toDateTimeLocal(value){return value?String(value).replace(' ','T').slice(0,16):'';}
document.addEventListener('click', event => { if (!event.target.closest('.batch-multi')) document.querySelectorAll('.batch-multi.open').forEach(x=>x.classList.remove('open')); });
