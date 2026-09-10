async function loadAdminCurrent() {
    try {
        const admin = await adminFetch('/api/admin/current');
        window.ADMIN_LEVEL = Number(admin.adminLevel);
        window.ADMIN_DEPARTMENT = admin.department || '';
        applyAdminMenuVisibility();
        if (typeof applyCourseDepartmentScope === 'function') applyCourseDepartmentScope();
        if (typeof applyBatchAccess === 'function') applyBatchAccess();
        renderAdminCurrent(admin);
    } catch (error) {
        // 未登录
        window.ADMIN_LEVEL = undefined;
        window.ADMIN_DEPARTMENT = '';
        applyAdminMenuVisibility();
        document.getElementById('adminCurrentRole').textContent = '未登录';
    }
}

// 职责划分：超级管理员(level=0)拥有全部功能，教务管理员(level=1)管教学与选课，权限表见 admin-shell.js
function applyAdminMenuVisibility() {
    const loggedIn = window.ADMIN_LEVEL !== undefined;
    let firstVisibleTab = null;

    document.querySelectorAll('.nav-item[data-tab]').forEach(btn => {
        const tab = btn.getAttribute('data-tab');
        const visible = loggedIn && window.isAdminTabAllowed(tab);
        btn.style.display = visible ? '' : 'none';
        if (visible && firstVisibleTab == null) firstVisibleTab = tab;
    });

    // 所有导航分组统一处理：组内没有可见项就整组隐藏（含分组标题）
    document.querySelectorAll('.nav-collapsible').forEach(group => {
        const hasVisibleItem = Array.from(group.querySelectorAll('.nav-item[data-tab]'))
            .some(btn => btn.style.display !== 'none');
        group.style.display = hasVisibleItem ? '' : 'none';
    });

    // 面板同步隐藏
    document.querySelectorAll('.tab-content').forEach(el => {
        const tab = el.id.replace(/-tab$/, '');
        el.style.display = loggedIn && window.isAdminTabAllowed(tab) ? '' : 'none';
    });

    // 当前激活tab被隐藏则切到第一个可见tab
    const activeNav = document.querySelector('.nav-item.active');
    if (activeNav && activeNav.style.display === 'none' && firstVisibleTab != null) {
        // 交给switchTab
        if (typeof window.switchTab === 'function') window.switchTab(firstVisibleTab);
    }
}

function renderAdminCurrent(admin) {
    const department = Number(admin.adminLevel) === 0 ? '全校' : (admin.department || '未配置学院');
    document.getElementById('adminCurrentRole').textContent =
        `${admin.realName || admin.username || '-'} / ${admin.adminNo || '-'} / ${department}`;
}
