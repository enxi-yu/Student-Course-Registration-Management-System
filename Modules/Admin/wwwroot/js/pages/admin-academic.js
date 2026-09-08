let adminAcademics = [];
let academicFormController;
function ensureAcademicForm() {
    if (!academicFormController) academicFormController = window.sharedUi.createForm({
        root: '#academics-tab', submitButton: '#saveAcademicButton', busyText: '保存中...',
        fields: [
            { name: 'username', label: '登录账号', selector: '#academicUsername', required: true },
            { name: 'adminNo', label: '管理员编号', selector: '#academicAdminNo', required: true },
            { name: 'password', label: '初始密码', selector: '#academicPassword', required: () => !document.getElementById('academicUserId').value, validate: value => value && (value.length < 6 || value.length > 20) ? '密码长度必须为6-20位' : '' },
            { name: 'realName', label: '姓名', selector: '#academicRealName', required: true },
            { name: 'phone', label: '联系电话', selector: '#academicPhone', validate: value => value && !/^1\d{10}$/.test(value) ? '请输入11位手机号码' : '' },
            { name: 'email', label: '电子邮箱', selector: '#academicEmail' }
        ]
    });
    return academicFormController;
}

async function loadAcademics() {
    const tbody = document.getElementById('academicTableBody');
    if (!tbody) return;

    const keyword = document.getElementById('academicKeyword').value;
    const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : '';
    window.sharedUi.setTableState(tbody, 6, '正在加载教务管理员数据...');

    try {
        adminAcademics = await adminFetch('/api/admin/academics' + query);
        window.sharedUi.renderTableRows(tbody, adminAcademics, item => `
            <tr>
                <td>${adminEscape(item.adminNo)}</td>
                <td>${adminEscape(item.realName)}</td>
                <td>${adminEscape(item.username)}</td>
                <td>${adminStatusBadge(item.status)}</td>
                <td>${adminEscape(item.phone || '-')}${item.email ? '<br>' + adminEscape(item.email) : ''}</td>
                <td>
                    <div class="row-actions">
                        <button class="btn btn-sm btn-edit" onclick="editAcademic(${item.userId})">编辑</button>
                        <button class="btn btn-sm btn-secondary" onclick="resetAcademicPassword(${item.userId})">重置密码</button>
                        ${Number(item.status) === 1
                            ? `<button class="btn btn-sm btn-delete" onclick="disableAcademic(${item.userId})">禁用</button>`
                            : `<button class="btn btn-sm btn-edit" onclick="enableAcademic(${item.userId})">启用</button>`}
                    </div>
                </td>
            </tr>
        `, 6, '暂无教务管理员数据');
    } catch (error) {
        window.sharedUi.setTableError(tbody, 6, `教务管理员数据加载失败：${error.message}`, loadAcademics);
    }
}

async function saveAcademic() {
    if (ensureAcademicForm().isSubmitting()) return;
    const userId = document.getElementById('academicUserId').value;
    try {
        await ensureAcademicForm().submit(async values => {
            const payload = { ...readAcademicForm(), ...values };
            await adminFetch(userId ? `/api/admin/academics/${encodeURIComponent(userId)}` : '/api/admin/academics', { method: userId ? 'PUT' : 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
        });
        alert('教务管理员信息已保存');
        clearAcademicForm();
        await loadAcademics();
    } catch (error) {
        alert('保存失败：' + error.message);
    }
}

function editAcademic(userId) {
    const item = adminAcademics.find(row => Number(row.userId) === Number(userId));
    if (!item) return;

    document.getElementById('academicFormTitle').textContent = '编辑教务管理员账号';
    document.getElementById('academicUserId').value = item.userId;
    document.getElementById('academicUsername').value = item.username || '';
    document.getElementById('academicAdminNo').value = item.adminNo || '';
    document.getElementById('academicRealName').value = item.realName || '';
    document.getElementById('academicPassword').value = '';
    document.getElementById('academicPhone').value = item.phone || '';
    document.getElementById('academicEmail').value = item.email || '';
    document.getElementById('academicStatus').value = String(item.status);
    document.getElementById('academicUsername').focus();
}

function clearAcademicForm() {
    document.getElementById('academicFormTitle').textContent = '新增教务管理员账号';
    document.getElementById('academicUserId').value = '';
    document.getElementById('academicUsername').value = '';
    document.getElementById('academicAdminNo').value = '';
    document.getElementById('academicRealName').value = '';
    document.getElementById('academicPassword').value = '';
    document.getElementById('academicPhone').value = '';
    document.getElementById('academicEmail').value = '';
    document.getElementById('academicStatus').value = '1';
}

async function disableAcademic(userId) {
    if (!await adminDialog.danger('确定要禁用该教务管理员账号吗？禁用后其无法登录管理后台。', '禁用教务管理员')) return;
    await changeAcademicStatus(userId, false);
}

async function enableAcademic(userId) {
    await changeAcademicStatus(userId, true);
}

async function changeAcademicStatus(userId, enabled) {
    try {
        await adminFetch(`/api/admin/academics/${encodeURIComponent(userId)}/${enabled ? 'enable' : 'disable'}`, { method: 'PUT' });
        loadAcademics();
    } catch (error) {
        alert('操作失败：' + error.message);
    }
}

async function resetAcademicPassword(userId) {
    const password = await adminDialog.prompt('请输入教务管理员新的登录密码（6-20位）：', { title: '重置教务管理员密码', inputType: 'password', maxLength: 20 });
    if (!password) return;
    if (password.length < 6 || password.length > 20) return alert('密码长度必须为6-20位');

    try {
        await adminFetch(`/api/admin/academics/${encodeURIComponent(userId)}/password`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ password })
        });
        alert('密码已重置');
    } catch (error) {
        alert('重置失败：' + error.message);
    }
}

function readAcademicForm() {
    return {
        username: document.getElementById('academicUsername').value,
        adminNo: document.getElementById('academicAdminNo').value,
        password: document.getElementById('academicPassword').value,
        realName: document.getElementById('academicRealName').value,
        phone: document.getElementById('academicPhone').value,
        email: document.getElementById('academicEmail').value,
        status: Number(document.getElementById('academicStatus').value)
    };
}
