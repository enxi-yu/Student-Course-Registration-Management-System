let adminStudents = [];
let studentFormController;
function ensureStudentForm() {
    if (!studentFormController) studentFormController = window.sharedUi.createForm({
        root: '#students-tab', submitButton: '#saveStudentButton', busyText: '保存中...',
        fields: [
            { name: 'username', label: '登录账号', selector: '#studentUsername', required: true },
            { name: 'password', label: '初始密码', selector: '#studentPassword', required: () => !document.getElementById('studentUserId').value, validate: value => value && (value.length < 6 || value.length > 20) ? '密码长度必须为6-20位' : '' },
            { name: 'realName', label: '姓名', selector: '#studentRealName', required: true },
            { name: 'studentNo', label: '学号', selector: '#studentNo', required: true },
            { name: 'major', label: '专业', selector: '#studentMajor', required: true },
            { name: 'grade', label: '年级', selector: '#studentGrade', required: true },
            { name: 'phone', label: '联系电话', selector: '#studentPhone', validate: value => value && !/^1\d{10}$/.test(value) ? '请输入11位手机号码' : '' },
            { name: 'email', label: '电子邮箱', selector: '#studentEmail' }
        ]
    });
    return studentFormController;
}

async function loadStudents() {
    const tbody = document.getElementById('studentTableBody');
    if (!tbody) return;

    const keyword = document.getElementById('studentKeyword').value;
    const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : '';
    window.sharedUi.setTableState(tbody, 9, '正在加载学生数据...');

    try {
        adminStudents = await adminFetch('/api/admin/students' + query);
        window.sharedUi.renderTableRows(tbody, adminStudents, item => `
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
        `, 9, '暂无学生数据');
    } catch (error) {
        window.sharedUi.setTableError(tbody, 9, `学生数据加载失败：${error.message}`, loadStudents);
    }
}

async function saveStudent() {
    if (ensureStudentForm().isSubmitting()) return;
    const userId = document.getElementById('studentUserId').value;
    try {
        await ensureStudentForm().submit(async values => {
            const payload = { ...readStudentForm(), ...values };
            await adminFetch(userId ? `/api/admin/students/${encodeURIComponent(userId)}` : '/api/admin/students', { method: userId ? 'PUT' : 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
        });
        alert('学生信息已保存');
        clearStudentForm();
        await loadStudents();
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
    if (!await adminDialog.danger('确定要禁用该学生账号吗？', '禁用学生账号')) return;
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
    const password = await adminDialog.prompt('请输入新的学生登录密码（6-20位）：', { title: '重置学生密码', inputType: 'password', maxLength: 20 });
    if (!password) return;
    if (password.length < 6 || password.length > 20) return alert('密码长度必须为6-20位');

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
