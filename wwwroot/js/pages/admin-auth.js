async function loginAdmin() {
    const message = document.getElementById('adminLoginMessage');
    message.className = 'inline-message';
    message.textContent = '正在登录...';

    try {
        const admin = await adminFetch('/api/admin/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                username: document.getElementById('adminLoginUsername').value,
                password: document.getElementById('adminLoginPassword').value
            })
        });

        message.className = 'inline-message success';
        message.textContent = '登录成功';
        renderAdminCurrent(admin);
        await loadAdminPermissions();
    } catch (error) {
        message.className = 'inline-message error';
        message.textContent = error.message;
    }
}

async function loadAdminCurrent() {
    try {
        const admin = await adminFetch('/api/admin/current');
        renderAdminCurrent(admin);
    } catch (error) {
        document.getElementById('adminCurrentRole').textContent = '当前角色：未登录管理员';
        document.getElementById('adminCurrentInfo').innerHTML = '<div class="empty-state">请先在“权限管理”页签登录管理员账号</div>';
    }
}

async function loadAdminPermissions() {
    const tbody = document.getElementById('permissionTableBody');
    if (!tbody) {
        return;
    }

    try {
        const rows = await adminFetch('/api/admin/permissions');
        if (!rows.length) {
            tbody.innerHTML = adminEmptyRow(3, '暂无权限数据');
            return;
        }

        tbody.innerHTML = rows.map(item => `
            <tr>
                <td>${adminEscape(item.permissionCode)}</td>
                <td>${adminEscape(item.permissionName)}</td>
                <td>${adminEscape(item.module)}</td>
            </tr>
        `).join('');
    } catch (error) {
        tbody.innerHTML = adminEmptyRow(3, error.message);
    }
}

function renderAdminCurrent(admin) {
    document.getElementById('adminCurrentRole').textContent = `当前角色：系统管理员 ${admin.realName || admin.username || ''}`;
    document.getElementById('adminCurrentInfo').innerHTML = `
        <div class="info-item">
            <div class="info-label">管理员编号</div>
            <div class="info-value">${adminEscape(admin.adminNo || '-')}</div>
        </div>
        <div class="info-item">
            <div class="info-label">登录账号</div>
            <div class="info-value">${adminEscape(admin.username || '-')}</div>
        </div>
        <div class="info-item">
            <div class="info-label">姓名</div>
            <div class="info-value">${adminEscape(admin.realName || '-')}</div>
        </div>
        <div class="info-item">
            <div class="info-label">管理员级别</div>
            <div class="info-value">${Number(admin.adminLevel) === 0 ? '超级管理员' : '普通管理员'}</div>
        </div>
    `;
}
