const SELECTION_DEFAULT_SEMESTER = '2025-2026-2';
let selectionCurrent = null;

function loadSelectionPage() {
    const sem = document.getElementById('selectionSemester');
    if (sem && !sem.value) sem.value = SELECTION_DEFAULT_SEMESTER;
    exitSelectionMode(true);
}

// 搜索学生
async function searchStudentsForSelection() {
    const keyword = document.getElementById('selectionKeyword').value.trim();

    const tbody = document.getElementById('selectionStudentTableBody');
    try {
        const rows = await adminFetch('/api/admin/students?' + new URLSearchParams({ keyword }).toString());
        if (!rows.length) { tbody.innerHTML = adminEmptyRow(5, '未找到学生'); return; }
        tbody.innerHTML = rows.map(s => `
            <tr>
                <td>${adminEscape(s.studentNo || s.username)}</td>
                <td>${adminEscape(s.realName)}</td>
                <td>${adminEscape(s.major || '-')}</td>
                <td>${adminEscape(s.grade || '-')}</td>
                <td><button class="btn btn-sm btn-primary" onclick="enterSelectionMode('${adminEscape(s.studentNo || s.username)}', '${adminEscape(s.realName)}')">进入代选</button></td>
            </tr>`).join('');
    } catch (e) {
        tbody.innerHTML = adminEmptyRow(5, e.message);
    }
}

// 从搜索结果进入代选模式
function enterSelectionMode(studentNo, studentName) {
    selectionCurrent = { studentNo };
    document.getElementById('selStudentName').textContent = `${studentNo} ${studentName || ''}`;
    document.getElementById('selectionSearchCard').style.display = 'none';
    document.getElementById('selectionModeCard').style.display = 'block';
    document.getElementById('selectionWorkArea').style.display = 'block';
    loadSelectableClasses();
    loadStudentEnrollments();
}

function exitSelectionMode(silent) {
    selectionCurrent = null;
    const search = document.getElementById('selectionSearchCard');
    const mode = document.getElementById('selectionModeCard');
    const work = document.getElementById('selectionWorkArea');
    if (search) search.style.display = 'block';
    if (mode) mode.style.display = 'none';
    if (work) work.style.display = 'none';
    if (!silent) {
        document.getElementById('selectionKeyword').value = '';
        document.getElementById('selectionStudentTableBody').innerHTML = '';
    }
}

// 加载可选教学班列表
async function loadSelectableClasses() {
    const tbody = document.getElementById('selectionClassTableBody');
    if (!tbody) return;
    if (!selectionCurrent) { tbody.innerHTML = adminEmptyRow(6, '请先进入代选模式'); return; }

    const semester = document.getElementById('selectionSemester').value.trim();
    const keyword = document.getElementById('selectionClassKeyword').value.trim();
    const params = new URLSearchParams();
    params.set('semester', semester);
    if (keyword) params.set('keyword', keyword);

    try {
        const rows = await adminFetch('/api/admin/selection/classes?' + params.toString());
        if (!rows.length) { tbody.innerHTML = adminEmptyRow(6, `学期 ${semester} 暂无可选教学班`); return; }
        tbody.innerHTML = rows.map(item => {
            const full = Number(item.selectedCount) >= Number(item.capacity);
            return `
            <tr>
                <td>${adminEscape(item.classId)}</td>
                <td>${adminEscape(item.courseName)}<br><span class="muted-text">${adminEscape(item.courseType || '')} / ${adminEscape(item.credit)}学分</span></td>
                <td>${adminEscape(item.teacherName || '-')}${item.teacherNo ? '<br>' + adminEscape(item.teacherNo) : ''}</td>
                <td>${adminEscape(item.scheduleSummary || '-')}</td>
                <td>${adminEscape(item.selectedCount)}/${adminEscape(item.capacity)}${full ? ' <span class="status-badge rejected">满</span>' : ''}</td>
                <td><button class="btn btn-sm btn-primary" onclick="selectClass(${item.classId})">代选</button></td>
            </tr>`;
        }).join('');
    } catch (e) {
        tbody.innerHTML = adminEmptyRow(6, e.message);
    }
}

// 加载已选课程
async function loadStudentEnrollments() {
    const tbody = document.getElementById('enrollmentTableBody');
    if (!tbody) return;
    if (!selectionCurrent) { tbody.innerHTML = adminEmptyRow(5, '请先进入代选模式'); return; }

    const semester = document.getElementById('selectionSemester').value.trim();
    const params = new URLSearchParams({ semester });
    try {
        const rows = await adminFetch('/api/admin/selection/students/' + encodeURIComponent(selectionCurrent.studentNo) + '/enrollments?' + params.toString());
        if (!rows.length) { tbody.innerHTML = adminEmptyRow(5, `学期 ${semester} 暂无已选课程`); return; }
        tbody.innerHTML = rows.map(item => `
            <tr>
                <td>${adminEscape(item.classId)}</td>
                <td>${adminEscape(item.courseName)}<br><span class="muted-text">${adminEscape(item.courseType || '')} / ${adminEscape(item.credit)}学分</span></td>
                <td>${adminEscape(item.teacherName || '-')}</td>
                <td>${adminEscape(item.scheduleSummary || '-')}<br><span class="muted-text">${item.batchName ? adminEscape(item.batchName) : '无批次'}</span></td>
                <td><button class="btn btn-sm btn-delete" onclick="dropClass(${item.classId})">代退</button></td>
            </tr>`).join('');
    } catch (e) {
        tbody.innerHTML = adminEmptyRow(5, e.message);
    }
}

// 代选课
async function selectClass(classId, force) {
    if (!selectionCurrent) { alert('请先进入代选模式'); return; }
    const semester = document.getElementById('selectionSemester').value.trim();
    const confirmText = force
        ? `确认超员扩容后代 ${selectionCurrent.studentNo} 在学期 ${semester} 选教学班 ${classId}？`
        : `确认代 ${selectionCurrent.studentNo} 在学期 ${semester} 选教学班 ${classId}？`;
    if (!confirm(confirmText)) return;

    const params = new URLSearchParams();
    if (force) params.set('force', 'true');
    const url = `/api/admin/selection/students/${encodeURIComponent(selectionCurrent.studentNo)}/classes/${classId}` + (params.toString() ? '?' + params.toString() : '');
    try {
        const result = await adminFetch(url, { method: 'POST' });
        if (result.Success) {
            alert(result.Message);
            loadSelectableClasses();
            loadStudentEnrollments();
            return;
        }
        if (result.RequireCapacityConfirm) {
            const ok = confirm(`${result.Message}\n\n点击确定则把容量 +1 并继续代选`);
            if (ok) selectClass(classId, true);
            return;
        }
        let msg = result.Message;
        if (result.ConflictCourses && result.ConflictCourses.length) {
            msg += '\n冲突课程：' + result.ConflictCourses.join('、');
        }
        alert(msg);
    } catch (e) {
        alert('代选失败：' + e.message);
    }
}

// 代退课
async function dropClass(classId) {
    if (!selectionCurrent) { alert('请先进入代选模式'); return; }
    const semester = document.getElementById('selectionSemester').value.trim();
    if (!confirm(`确认代 ${selectionCurrent.studentNo} 在学期 ${semester} 退教学班 ${classId}？`)) return;
    try {
        const result = await adminFetch(`/api/admin/selection/students/${encodeURIComponent(selectionCurrent.studentNo)}/classes/${classId}`, { method: 'DELETE' });
        alert(result.Message);
        if (result.Success) { loadSelectableClasses(); loadStudentEnrollments(); }
    } catch (e) {
        alert('代退失败：' + e.message);
    }
}
