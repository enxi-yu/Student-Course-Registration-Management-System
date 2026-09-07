(function () {
    let dashboardElements;

    // 首页入口：超级管理员(level=0)拥有全部功能、入口覆盖各管理模块；
    // 教务管理员(level=1)仅教学管理（课程/排课/开课审批/评价/容量）与选课管理（批次/代选课程）
    function renderShell(isSuperAdmin) {
        const actions = isSuperAdmin === undefined
            ? { hero: [], quick: [] }
            : isSuperAdmin
                ? {
                    hero: [{ label: '进入课程管理', target: 'courses', style: 'primary-button' }, { label: '学生账号管理', target: 'students', style: 'secondary-button' }],
                    quick: [
                        { label: '排课管理', target: 'scheduling' },
                        { label: '开课审批', target: 'applications' },
                        { label: '教师管理', target: 'teachers' },
                        { label: '选课批次', target: 'batches' },
                        { label: '容量调整', target: 'classes' },
                        { label: '系统日志', target: 'logs' }
                    ]
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
        const isSuperAdmin = Number(data.adminLevel) === 0;
        renderShell(isSuperAdmin);
        window.sharedUi.updateDashboard(dashboardElements, {
            metrics: [
                { label: '课程总数', value: data.courseCount, note: '系统已维护课程', tone: 'blue' },
                { label: '教学班', value: data.classCount, note: '当前教学班总数', tone: 'cyan' },
                { label: '启用用户', value: data.activeUserCount, note: '当前可登录用户', tone: 'green' },
                { label: '待审申请', value: data.pendingApplicationCount, note: Number(data.pendingApplicationCount) > 0 ? '建议优先处理' : '暂无待处理', tone: 'amber' }
            ],
            profile: [
                { label: '管理员姓名', value: data.realName }, { label: '管理员编号', value: data.adminNo },
                { label: '登录账号', value: data.username }, { label: '管理级别', value: isSuperAdmin ? '超级管理员' : '教务管理员' }
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
