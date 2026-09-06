let adminTeachers = [];
let teacherFormController;
function ensureTeacherForm() {
    if (!teacherFormController) teacherFormController = window.sharedUi.createForm({
        root: '#teachers-tab', submitButton: '#saveTeacherButton', busyText: '保存中...',
        fields: [
            { name: 'username', label: '登录账号', selector: '#teacherUsername', required: true },
            { name: 'password', label: '初始密码', selector: '#teacherPassword', required: () => !document.getElementById('teacherUserId').value, validate: value => value && (value.length < 6 || value.length > 20) ? '密码长度必须为6-20位' : '' },
            { name: 'realName', label: '姓名', selector: '#teacherRealName', required: true },
            { name: 'teacherNo', label: '工号', selector: '#teacherNo', required: true },
            { name: 'title', label: '职称', selector: '#teacherTitle', required: true },
            { name: 'department', label: '所属院系', selector: '#teacherDepartment', required: true },
            { name: 'phone', label: '联系电话', selector: '#teacherPhone', validate: value => value && !/^1\d{10}$/.test(value) ? '请输入11位手机号码' : '' },
            { name: 'email', label: '电子邮箱', selector: '#teacherEmail' }
        ]
    });
    return teacherFormController;
}

async function loadTeachers() {
    const tbody = document.getElementById('teacherTableBody');
    if (!tbody) return;

    const keyword = document.getElementById('teacherKeyword').value;
    const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : '';
    window.sharedUi.setTableState(tbody, 7, '正在加载教师数据...');

    try {
        adminTeachers = await adminFetch('/api/admin/teachers' + query);
        window.sharedUi.renderTableRows(tbody, adminTeachers, item => `
            <tr>
                <td>${adminEscape(item.teacherNo)}</td>
                <td>${adminEscape(item.realName)}</td>
                <td>${adminEscape(item.title)}</td>
                <td>${adminEscape(item.department)}</td>
                <td>${adminStatusBadge(item.status)}</td>
                <td>${adminEscape(item.phone || '-')}${item.email ? '<br>' + adminEscape(item.email) : ''}</td>
                <td>
                    <div class="row-actions">
                        <button class="btn btn-sm btn-edit" onclick="editTeacher(${item.userId})">编辑</button>
                        <button class="btn btn-sm btn-secondary" onclick="resetTeacherPassword(${item.userId})">重置密码</button>
                        ${Number(item.status) === 1
                            ? `<button class="btn btn-sm btn-delete" onclick="disableTeacher(${item.userId})">禁用</button>`
                            : `<button class="btn btn-sm btn-edit" onclick="enableTeacher(${item.userId})">启用</button>`}
                    </div>
                </td>
            </tr>
        `, 7, '暂无教师数据');
    } catch (error) {
        window.sharedUi.setTableError(tbody, 7, `教师数据加载失败：${error.message}`, loadTeachers);
    }
}

async function saveTeacher() {
    if (ensureTeacherForm().isSubmitting()) return;
    const userId = document.getElementById('teacherUserId').value;
    try {
        await ensureTeacherForm().submit(async values => {
            const payload = { ...readTeacherForm(), ...values };
            await adminFetch(userId ? `/api/admin/teachers/${encodeURIComponent(userId)}` : '/api/admin/teachers', { method: userId ? 'PUT' : 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
        });
        alert('教师信息已保存');
        clearTeacherForm();
        await loadTeachers();
    } catch (error) {
        alert('保存失败：' + error.message);
    }
}

function editTeacher(userId) {
    const item = adminTeachers.find(row => Number(row.userId) === Number(userId));
    if (!item) return;

    document.getElementById('teacherFormTitle').textContent = '编辑教师账号';
    document.getElementById('teacherUserId').value = item.userId;
    document.getElementById('teacherUsername').value = item.username || '';
    document.getElementById('teacherPassword').value = '';
    document.getElementById('teacherRealName').value = item.realName || '';
    document.getElementById('teacherNo').value = item.teacherNo || '';
    document.getElementById('teacherTitle').value = item.title || '';
    document.getElementById('teacherDepartment').value = item.department || '';
    document.getElementById('teacherPhone').value = item.phone || '';
    document.getElementById('teacherEmail').value = item.email || '';
    document.getElementById('teacherStatus').value = String(item.status);
    document.getElementById('teacherUsername').focus();
}

function clearTeacherForm() {
    document.getElementById('teacherFormTitle').textContent = '新增教师账号';
    document.getElementById('teacherUserId').value = '';
    document.getElementById('teacherUsername').value = '';
    document.getElementById('teacherPassword').value = '';
    document.getElementById('teacherRealName').value = '';
    document.getElementById('teacherNo').value = '';
    document.getElementById('teacherTitle').value = '';
    document.getElementById('teacherDepartment').value = '';
    document.getElementById('teacherPhone').value = '';
    document.getElementById('teacherEmail').value = '';
    document.getElementById('teacherStatus').value = '1';
}

async function disableTeacher(userId) {
    if (!await adminDialog.danger('确定要禁用该教师账号吗？', '禁用教师账号')) return;
    await changeTeacherStatus(userId, false);
}

async function enableTeacher(userId) {
    await changeTeacherStatus(userId, true);
}

async function changeTeacherStatus(userId, enabled) {
    try {
        await adminFetch(`/api/admin/teachers/${encodeURIComponent(userId)}/${enabled ? 'enable' : 'disable'}`, { method: 'PUT' });
        loadTeachers();
    } catch (error) {
        alert('操作失败：' + error.message);
    }
}

async function resetTeacherPassword(userId) {
    const password = await adminDialog.prompt('请输入新的教师登录密码（6-20位）：', { title: '重置教师密码', inputType: 'password', maxLength: 20 });
    if (!password) return;
    if (password.length < 6 || password.length > 20) return alert('密码长度必须为6-20位');

    try {
        await adminFetch(`/api/admin/teachers/${encodeURIComponent(userId)}/password`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ password })
        });
        alert('密码已重置');
    } catch (error) {
        alert('重置失败：' + error.message);
    }
}

function readTeacherForm() {
    return {
        username: document.getElementById('teacherUsername').value,
        password: document.getElementById('teacherPassword').value,
        realName: document.getElementById('teacherRealName').value,
        phone: document.getElementById('teacherPhone').value,
        email: document.getElementById('teacherEmail').value,
        status: Number(document.getElementById('teacherStatus').value),
        teacherNo: document.getElementById('teacherNo').value,
        title: document.getElementById('teacherTitle').value,
        department: document.getElementById('teacherDepartment').value
    };
}
