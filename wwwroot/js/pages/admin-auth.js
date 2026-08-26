async function loginAdmin() {
    const message = document.getElementById('adminLoginMessage');
    message.className = 'inline-message';
    message.textContent = '正在登录...';

    try {
        const admin = await adminFetch('/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                username: document.getElementById('adminLoginUsername').value,
                password: document.getElementById('adminLoginPassword').value
            })
        });

        message.className = 'inline-message success';
        message.textContent = '登录成功';
        window.ADMIN_LEVEL = Number(admin.adminLevel);
        applyAdminMenuVisibility();
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
        window.ADMIN_LEVEL = Number(admin.adminLevel);
        applyAdminMenuVisibility();
        renderAdminCurrent(admin);
    } catch (error) {
        // 未登录
        window.ADMIN_LEVEL = undefined;
        applyAdminMenuVisibility();
        document.getElementById('adminCurrentRole').textContent = '当前角色：未登录管理员';
        document.getElementById('adminCurrentInfo').innerHTML = '<div class="empty-state">请先在"权限管理"页签登录管理员账号</div>';
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

// 普通管理员（level != 0）隐藏的菜单项
const RESTRICTED_TABS = ['students', 'teachers', 'batches', 'logs'];

function applyAdminMenuVisibility() {
    const isSuper = window.ADMIN_LEVEL === 0;
    let firstVisibleTab = null;

    document.querySelectorAll('.nav-item').forEach(btn => {
        const tab = btn.getAttribute('data-tab');
        const restricted = RESTRICTED_TABS.indexOf(tab) >= 0;
        const visible = !restricted || isSuper;
        btn.style.display = visible ? '' : 'none';
        if (visible && firstVisibleTab == null) firstVisibleTab = tab;
    });

    // 面板同步隐藏
    RESTRICTED_TABS.forEach(tab => {
        const el = document.getElementById(tab + '-tab');
        if (el) el.style.display = isSuper ? '' : 'none';
    });

    // 当前激活tab被隐藏则切到第一个可见tab
    const activeNav = document.querySelector('.nav-item.active');
    if (activeNav && activeNav.style.display === 'none' && firstVisibleTab != null) {
        // 交给switchTab
        if (typeof window.switchTab === 'function') window.switchTab(firstVisibleTab);
    }
}

function renderAdminCurrent(admin) {
    const lv = Number(admin.adminLevel);
    document.getElementById('adminCurrentRole').textContent =
        (lv === 0 ? '当前角色：超级管理员 ' : '当前角色：普通管理员 ') + (admin.realName || admin.username || '');
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
            <div class="info-value">${lv === 0 ? '超级管理员' : '普通管理员'}</div>
        </div>
    `;
}
