async function loadAdminClasses() {
    const tbody = document.getElementById('classTableBody');
    if (!tbody) return;

    const keyword = document.getElementById('classKeyword').value;
    const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : '';

    try {
        const rows = await adminFetch('/api/admin/classes' + query);
        if (!rows.length) {
            tbody.innerHTML = adminEmptyRow(8, '暂无教学班数据');
            return;
        }

        tbody.innerHTML = rows.map(item => `
            <tr>
                <td>${adminEscape(item.classId)}</td>
                <td>${adminEscape(item.className)}</td>
                <td>${adminEscape(item.courseName)}<br><span class="muted-text">#${adminEscape(item.courseId)}</span></td>
                <td>${adminEscape(item.semester)}</td>
                <td>${adminEscape(item.teacherName || '-')}${item.teacherNo ? '<br>' + adminEscape(item.teacherNo) : ''}</td>
                <td>${adminEscape(item.capacity)}</td>
                <td>${adminEscape(item.selectedCount)}</td>
                <td><button class="btn btn-sm btn-edit" onclick="adjustCapacity(${item.classId}, ${item.capacity}, ${item.selectedCount})">调整容量</button></td>
            </tr>
        `).join('');
    } catch (error) {
        tbody.innerHTML = adminEmptyRow(8, error.message);
    }
}

async function adjustCapacity(classId, capacity, selectedCount) {
    const value = prompt(`当前容量 ${capacity}，已选 ${selectedCount} 人。请输入新的课程容量：`, String(capacity));
    if (value === null) return;

    const newCapacity = Number(value);
    if (!Number.isInteger(newCapacity) || newCapacity < 0) {
        alert('容量必须是非负整数');
        return;
    }

    if (newCapacity < Number(selectedCount)) {
        alert('新容量不能小于已选人数');
        return;
    }

    const remark = prompt('请输入容量调整说明（可选）：') || '';

    try {
        await adminFetch(`/api/admin/classes/${encodeURIComponent(classId)}/capacity`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ capacity: newCapacity, remark })
        });
        alert('容量已调整');
        loadAdminClasses();
    } catch (error) {
        alert('调整失败：' + error.message);
    }
}
