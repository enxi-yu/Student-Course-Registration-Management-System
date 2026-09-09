let adminBatches = [];
let batchClassOptions = [];
let batchMajors = [];
let batchGrades = [];
let batchFormController;
let batchPage = 1;

function ensureBatchForm() {
    if (!batchFormController) batchFormController = window.sharedUi.createForm({
        root: '#batches-tab', submitButton: '#saveBatchButton', busyText: '保存中...',
        fields: [
            { name: 'batchName', label: '批次名称', selector: '#batchName', required: true },
            { name: 'startTime', label: '开始时间', selector: '#batchStartTime', required: true },
            { name: 'endTime', label: '结束时间', selector: '#batchEndTime', required: true, validate: (value, values) => (value && values.startTime && new Date(value) <= new Date(values.startTime)) ? '结束时间必须晚于开始时间' : '' },
            { name: 'classIds', label: '开放课程', read: () => batchChecked('batchCourseOptions').map(Number), required: true, requiredMessage: '请至少选择一门开放课程' },
            { name: 'majors', label: '面向专业', read: () => batchChecked('batchMajorOptions') },
            { name: 'grades', label: '面向年级', read: () => batchChecked('batchGradeOptions') }
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
    fillBatchSelectors([], [], []);
}

function fillBatchSelectors(classIds, majors, grades) {
    renderBatchOptions('batchCourseOptions', batchClassOptions, classIds, 'classId', x => ({ title:`${x.courseName} · ${x.className}`, detail:`${x.teacherName || '未分配教师'} · ${x.semester}`, search:`${x.courseName} ${x.className} ${x.teacherName || ''} ${x.semester}` }));
    renderBatchOptions('batchMajorOptions', batchMajors, majors, '', x => ({title:x, detail:'', search:x}));
    renderBatchOptions('batchGradeOptions', batchGrades, grades, '', x => ({title:x, detail:'', search:x}));
    wireBatchMulti('batchCourseMulti', '请选择开放课程'); wireBatchMulti('batchMajorMulti', '全部专业'); wireBatchMulti('batchGradeMulti', '全部年级');
}

async function loadBatches(resetPage = true) {
    const tbody = document.getElementById('batchTableBody'); if (!tbody) return;
    if (resetPage) batchPage = 1;
    window.sharedUi.setTableState(tbody, 6, '正在加载选课批次...');
    try {
        await loadBatchLookups();
        adminBatches = await adminFetch('/api/admin/batches');
        batchPage = window.sharedUi.renderPagedTable({body:tbody,rows:adminBatches,page:batchPage,pageSize:10,colspan:6,emptyText:'暂无选课批次',pagination:'batchPagination',onPageChange:page=>{batchPage=page;loadBatches(false);},row:item=>`<tr><td>${adminEscape(item.batchId)}</td><td>${adminEscape(item.batchName)}</td><td>${adminEscape(item.startTime)}</td><td>${adminEscape(item.endTime)}</td><td>${adminBatchBadge(item.status,item.statusText)}</td><td><button class="btn btn-sm btn-edit table-action" onclick="editBatch(${item.batchId})">编辑</button>${Number(item.status) === 1 ? ` <button class="btn btn-sm btn-delete table-action" onclick="endBatch(${item.batchId})">结束</button>` : ''}</td></tr>`});
    } catch (error) { window.sharedUi.setTableError(tbody,6,error.message,()=>loadBatches(false)); }
}

async function saveBatch() {
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
    const item = adminBatches.find(x => Number(x.batchId) === Number(batchId)); if (!item) return;
    try {
        const offerings = await adminFetch(`/api/admin/batches/${batchId}/offerings`);
        const selected = offerings.filter(x => x.selected);
        const selectedMajors = [...new Set(selected.flatMap(x => x.majors || []))];
        const selectedGrades = [...new Set(selected.flatMap(x => x.grades || []))];
        const dialog = window.sharedUi.formDialog({
            title: '编辑选课批次', description: '修改开放时间、课程范围及面向学生。', submitText: '保存修改',
            fields: [
                { name: 'batchName', label: '批次名称', required: true, value: item.batchName || '' },
                { name: 'startTime', label: '开始时间', type: 'datetime-local', required: true, value: toDateTimeLocal(item.startTime) },
                { name: 'endTime', label: '结束时间', type: 'datetime-local', required: true, value: toDateTimeLocal(item.endTime) },
                { name: 'classIds', label: '开放课程', kind: 'multiselect', wide: true, required: true, value: [], options: [] },
                { name: 'majors', label: '面向专业', kind: 'multiselect', value: [], options: [] },
                { name: 'grades', label: '面向年级', kind: 'multiselect', value: [], options: [] }
            ]
        });
        dialogBatchList(dialog, 'classIds', { wrapId: 'dlgClassMulti', optionsId: 'dlgClassOptions', emptyText: '请选择开放课程', searchPlaceholder: '搜索课程、教学班或教师', items: batchClassOptions, selected: selected.map(x => x.classId), valueKey: 'classId', labelBuilder: x => ({ title: `${x.courseName} · ${x.className}`, detail: `${x.teacherName || '未分配教师'} · ${x.semester}`, search: `${x.courseName} ${x.className} ${x.teacherName || ''} ${x.semester}` }) });
        dialogBatchList(dialog, 'majors', { wrapId: 'dlgMajorMulti', optionsId: 'dlgMajorOptions', emptyText: '全部专业', items: batchMajors, selected: selectedMajors, valueKey: '', labelBuilder: x => ({ title: x, detail: '', search: x }) });
        dialogBatchList(dialog, 'grades', { wrapId: 'dlgGradeMulti', optionsId: 'dlgGradeOptions', emptyText: '全部年级', items: batchGrades, selected: selectedGrades, valueKey: '', labelBuilder: x => ({ title: x, detail: '', search: x }) });
        const dialogForm = dialog.form, submitButton = dialogForm.querySelector('[type="submit"]');
        const setBusy = busy => { submitButton.disabled = busy; submitButton.textContent = busy ? '保存中...' : '保存修改'; };
        dialogForm.onsubmit = async event => {
            event.preventDefault();
            if (submitButton.disabled) return;
            const start = dialogForm.elements['startTime']?.value || '', end = dialogForm.elements['endTime']?.value || '';
            if (start && end && new Date(end).getTime() <= new Date(start).getTime()) { window.sharedUi.alert('结束时间必须晚于开始时间'); return; }
            const classIds = batchChecked('dlgClassOptions').map(Number);
            if (!classIds.length) { window.sharedUi.alert('请至少选择一门开放课程'); return; }
            const majors = batchChecked('dlgMajorOptions'), grades = batchChecked('dlgGradeOptions');
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
    if (!await window.sharedUi.confirm('确定立即结束这个选课批次吗？结束后学生将不能继续选课。')) return;
    try {
        await adminFetch(`/api/admin/batches/${batchId}/end`, {method:'PUT'});
        await loadBatches(false);
        await window.sharedUi.alert('批次已结束');
    } catch(error){ alert('结束失败：'+window.sharedUi.errorText(error)); }
}

function clearBatchForm() {
    document.getElementById('batchFormTitle').textContent='新增选课批次'; document.getElementById('batchId').value=''; document.getElementById('batchName').value=''; document.getElementById('batchStartTime').value=''; document.getElementById('batchEndTime').value=''; fillBatchSelectors([],[],[]);
}
function toDateTimeLocal(value){return value?String(value).replace(' ','T').slice(0,16):'';}
document.addEventListener('click', event => { if (!event.target.closest('.batch-multi')) document.querySelectorAll('.batch-multi.open').forEach(x=>x.classList.remove('open')); });
