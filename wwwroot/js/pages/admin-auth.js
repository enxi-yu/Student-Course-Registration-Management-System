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
        document.getElementById('adminCurrentRole').textContent = '未登录';
    }
}

// 普通管理员（level != 0）隐藏的菜单项
const RESTRICTED_TABS = ['students', 'teachers', 'batches', 'logs'];

function applyAdminMenuVisibility() {
    const isSuper = window.ADMIN_LEVEL === 0;
    let firstVisibleTab = null;

    document.querySelectorAll('.nav-item[data-tab]').forEach(btn => {
        const tab = btn.getAttribute('data-tab');
        const restricted = RESTRICTED_TABS.indexOf(tab) >= 0;
        const visible = !restricted || isSuper;
        btn.style.display = visible ? '' : 'none';
        if (visible && firstVisibleTab == null) firstVisibleTab = tab;
    });

    document.querySelectorAll('.nav-collapsible[data-hide-when-empty]').forEach(group => {
        const hasVisibleItem = Array.from(group.querySelectorAll('.nav-item[data-tab]'))
            .some(btn => btn.style.display !== 'none');
        group.style.display = hasVisibleItem ? '' : 'none';
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
    document.getElementById('adminCurrentRole').textContent =
        `${admin.realName || admin.username || '-'} / ${admin.adminNo || '-'}`;
}
