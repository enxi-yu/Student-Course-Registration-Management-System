(function () {
    let dashboardElements;

    // 系统管理员(level=0)与教务管理员(level=1)的首页入口不同
    function renderShell(isSystemAdmin) {
        const actions = isSystemAdmin === undefined
            ? { hero: [], quick: [] }
            : isSystemAdmin
                ? {
                    hero: [{ label: '学生账号管理', target: 'students', style: 'primary-button' }, { label: '教师账号管理', target: 'teachers', style: 'secondary-button' }],
                    quick: [{ label: '学生管理', target: 'students' }, { label: '教师管理', target: 'teachers' }, { label: '系统日志', target: 'logs' }]
                }
                : {
                    hero: [{ label: '进入课程管理', target: 'courses', style: 'primary-button' }, { label: '处理开课申请', target: 'applications', style: 'secondary-button' }],
                    quick: [{ label: '课程管理', target: 'courses' }, { label: '排课管理', target: 'scheduling' }, { label: '开课审批', target: 'applications' }, { label: '选课批次', target: 'batches' }]
                };
        dashboardElements = window.sharedUi.renderDashboardShell(document.getElementById('admin-dashboard-root'), {
            prefix: 'admin-dashboard',
            heroTitle: '管理控制台',
            heroActions: actions.hero,
            quickActions: actions.quick,
            onNavigate: target => switchTab(target)
        });
    }

    async function refreshDashboard() {
        const data = await adminFetch('/api/admin/dashboard');
        const isSystemAdmin = Number(data.adminLevel) === 0;
        renderShell(isSystemAdmin);
        window.sharedUi.updateDashboard(dashboardElements, {
            metrics: [
                { label: '课程总数', value: data.courseCount, note: '系统已维护课程', tone: 'blue' },
                { label: '教学班', value: data.classCount, note: '当前教学班总数', tone: 'cyan' },
                { label: '启用用户', value: data.activeUserCount, note: '当前可登录用户', tone: 'green' },
                { label: '待审申请', value: data.pendingApplicationCount, note: Number(data.pendingApplicationCount) > 0 ? '建议优先处理' : '暂无待处理', tone: 'amber' }
            ],
            profile: [
                { label: '管理员姓名', value: data.realName }, { label: '管理员编号', value: data.adminNo },
                { label: '登录账号', value: data.username }, { label: '管理级别', value: isSystemAdmin ? '系统管理员' : '教务管理员' }
            ]
        });
    }

    window.loadAdminDashboard = async function () {
        const root = document.getElementById('admin-dashboard-root');
        if (!root) return;
        renderShell(undefined);
        window.sharedUi.updateDashboard(dashboardElements, { metrics: [{ label: '首页数据', value: '…', note: '正在加载', tone: 'blue' }], profile: [] });
        try { await refreshDashboard(); }
        catch (error) { window.sharedUi.mountState(dashboardElements.metrics, 'error', `首页数据加载失败：${error.message}`, window.loadAdminDashboard); }
    };
})();
