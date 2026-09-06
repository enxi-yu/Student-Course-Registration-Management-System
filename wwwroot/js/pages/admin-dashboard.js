(function () {
    let dashboardElements;
    async function refreshDashboard() {
        const data = await adminFetch('/api/admin/dashboard');
        const isSuperAdmin = Number(data.adminLevel) === 0;
        window.sharedUi.updateDashboard(dashboardElements, {
            metrics: [
                { label: '课程总数', value: data.courseCount, note: '系统已维护课程', tone: 'blue', target: 'courses' },
                { label: '教学班', value: data.classCount, note: '当前教学班总数', tone: 'cyan', target: 'classes' },
                { label: '启用用户', value: data.activeUserCount, note: '当前可登录用户', tone: 'green', target: 'students' },
                { label: '待审申请', value: data.pendingApplicationCount, note: Number(data.pendingApplicationCount) > 0 ? '建议优先处理' : '暂无待处理', tone: 'amber', target: 'applications' }
            ],
            profile: [
                { label: '管理员姓名', value: data.realName }, { label: '管理员编号', value: data.adminNo },
                { label: '登录账号', value: data.username }, { label: '管理级别', value: isSuperAdmin ? '超级管理员' : '普通管理员' }
            ]
        });
    }

    window.loadAdminDashboard = async function () {
        const root = document.getElementById('admin-dashboard-root');
        if (!root) return;
        dashboardElements = window.sharedUi.renderDashboardShell(root, {
            prefix: 'admin-dashboard',
            heroTitle: '管理控制台',
            heroActions: [{ label: '进入课程管理', target: 'courses', style: 'primary-button' }, { label: '处理开课申请', target: 'applications', style: 'secondary-button' }],
            quickActions: [{ label: '课程管理', target: 'courses' }, { label: '排课管理', target: 'scheduling' }, { label: '开课审批', target: 'applications' }, { label: '学生管理', target: 'students' }],
            onNavigate: target => switchTab(target)
        });
        window.sharedUi.updateDashboard(dashboardElements, { metrics: [{ label: '首页数据', value: '…', note: '正在加载', tone: 'blue' }], profile: [] });
        try { await refreshDashboard(); }
        catch (error) { window.sharedUi.mountState(dashboardElements.metrics, 'error', `首页数据加载失败：${error.message}`, window.loadAdminDashboard); }
    };
})();
