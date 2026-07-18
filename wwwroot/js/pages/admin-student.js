let adminStudents = [];

async function loadStudents() {
    const tbody = document.getElementById('studentTableBody');
    if (!tbody) return;

    const keyword = document.getElementById('studentKeyword').value;
    const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : '';

    try {
        adminStudents = await adminFetch('/api/admin/students' + query);
        if (!adminStudents.length) {
            tbody.innerHTML = adminEmptyRow(9, '暂无学生数据');
            return;
        }

        tbody.innerHTML = adminStudents.map(item => `
            <tr>
                <td>${adminEscape(item.studentNo)}</td>
                <td>${adminEscape(item.realName)}</td>
                <td>${adminEscape(item.major)}</td>
                <td>${adminEscape(item.grade)}</td>
                <td>${adminEscape(item.avgGpa)}</td>
                <td>${adminEscape(item.creditFinished)}</td>
                <td>${adminStatusBadge(item.status)}</td>
                <td>${adminEscape(item.phone || '-')}${item.email ? '<br>' + adminEscape(item.email) : ''}</td>
                <td>
                    <div class="row-actions">
                        <button class="btn btn-sm btn-edit" onclick="editStudent(${item.userId})">编辑</button>
                        <button class="btn btn-sm btn-secondary" onclick="resetStudentPassword(${item.userId})">重置密码</button>
                        ${Number(item.status) === 1
                            ? `<button class="btn btn-sm btn-delete" onclick="disableStudent(${item.userId})">禁用</button>`
                            : `<button class="btn btn-sm btn-edit" onclick="enableStudent(${item.userId})">启用</button>`}
                    </div>
                </td>
            </tr>
        `).join('');
    } catch (error) {
        tbody.innerHTML = adminEmptyRow(9, error.message);
    }
}

async function saveStudent() {
    const userId = document.getElementById('studentUserId').value;
    const payload = readStudentForm();

    try {
        await adminFetch(userId ? `/api/admin/students/${encodeURIComponent(userId)}` : '/api/admin/students', {
            method: userId ? 'PUT' : 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        alert('学生信息已保存');
        clearStudentForm();
        loadStudents();
    } catch (error) {
        alert('保存失败：' + error.message);
    }
}

function editStudent(userId) {
    const item = adminStudents.find(row => Number(row.userId) === Number(userId));
    if (!item) return;

    document.getElementById('studentFormTitle').textContent = '编辑学生账号';
    document.getElementById('studentUserId').value = item.userId;
    document.getElementById('studentUsername').value = item.username || '';
    document.getElementById('studentPassword').value = '';
    document.getElementById('studentRealName').value = item.realName || '';
    document.getElementById('studentNo').value = item.studentNo || '';
    document.getElementById('studentMajor').value = item.major || '';
    document.getElementById('studentGrade').value = item.grade || '';
    document.getElementById('studentPhone').value = item.phone || '';
    document.getElementById('studentEmail').value = item.email || '';
    document.getElementById('studentStatus').value = String(item.status);
    document.getElementById('studentAvgGpa').value = item.avgGpa || 0;
    document.getElementById('studentCreditFinished').value = item.creditFinished || 0;
    document.getElementById('studentUsername').focus();
}

function clearStudentForm() {
    document.getElementById('studentFormTitle').textContent = '新增学生账号';
    document.getElementById('studentUserId').value = '';
    document.getElementById('studentUsername').value = '';
    document.getElementById('studentPassword').value = '';
    document.getElementById('studentRealName').value = '';
    document.getElementById('studentNo').value = '';
    document.getElementById('studentMajor').value = '';
    document.getElementById('studentGrade').value = '';
    document.getElementById('studentPhone').value = '';
    document.getElementById('studentEmail').value = '';
    document.getElementById('studentStatus').value = '1';
    document.getElementById('studentAvgGpa').value = '0';
    document.getElementById('studentCreditFinished').value = '0';
}

async function disableStudent(userId) {
    if (!confirm('确定要禁用该学生账号吗？')) return;
    await changeStudentStatus(userId, false);
}

async function enableStudent(userId) {
    await changeStudentStatus(userId, true);
}

async function changeStudentStatus(userId, enabled) {
    try {
        await adminFetch(`/api/admin/students/${encodeURIComponent(userId)}/${enabled ? 'enable' : 'disable'}`, { method: 'PUT' });
        loadStudents();
    } catch (error) {
        alert('操作失败：' + error.message);
    }
}

async function resetStudentPassword(userId) {
    const password = prompt('请输入新的学生登录密码：');
    if (!password) return;

    try {
        await adminFetch(`/api/admin/students/${encodeURIComponent(userId)}/password`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ password })
        });
        alert('密码已重置');
    } catch (error) {
        alert('重置失败：' + error.message);
    }
}

function readStudentForm() {
    return {
        username: document.getElementById('studentUsername').value,
        password: document.getElementById('studentPassword').value,
        realName: document.getElementById('studentRealName').value,
        phone: document.getElementById('studentPhone').value,
        email: document.getElementById('studentEmail').value,
        status: Number(document.getElementById('studentStatus').value),
        studentNo: document.getElementById('studentNo').value,
        major: document.getElementById('studentMajor').value,
        grade: document.getElementById('studentGrade').value,
        avgGpa: Number(document.getElementById('studentAvgGpa').value || 0),
        creditFinished: Number(document.getElementById('studentCreditFinished').value || 0)
    };
}
