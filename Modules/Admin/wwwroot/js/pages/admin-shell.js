(function () {
    const loginPortalUrl = 'http://localhost:5100/Login';
    const pendingMutations = new Map();
    const pageChrome = [
        {
            key: 'courses',
            title: '课程管理',
            description: '新增、查询和维护课程基础信息。',
            fields: '<div class="field"><label for="courseKeyword">课程查询</label><input type="text" id="courseKeyword" placeholder="课程名称 / 学院"></div>' +
                '<div class="field"><label for="courseTypeFilter">类型</label><select id="courseTypeFilter"><option value="">全部</option><option value="必修">必修</option><option value="选修">选修</option><option value="公选">公选</option></select></div>',
            actions: '<button class="btn btn-primary" type="button" onclick="loadCourses()">查询</button>'
        },
        {
            key: 'scheduling',
            title: '排课管理',
            description: '创建和维护教学班、任课教师及上课时间安排。',
            fields: '<div class="field"><label for="scheduleKeyword">关键词</label><input type="text" id="scheduleKeyword" placeholder="教学班 / 课程 / 教师"></div>' +
                '<div class="field"><label>筛选</label><div id="scheduleSemesterFilter"></div></div>',
            actions: '<button class="btn btn-primary" type="button" onclick="loadSchedules()">查询排课</button>'
        },
        {
            key: 'applications',
            title: '开课申请',
            description: '查询并审批教师提交的开课申请。',
            fields: '<div class="field"><label for="applicationKeyword">申请查询</label><input type="text" id="applicationKeyword" placeholder="申请编号 / 课程名称 / 教师工号"></div>' +
                '<div class="field"><label for="applicationStatusFilter">状态</label><select id="applicationStatusFilter"><option value="">全部</option><option value="待审核">待审核</option><option value="通过">已通过</option><option value="驳回">已驳回</option><option value="已开课">已开课</option></select></div>',
            actions: '<button class="btn btn-primary" type="button" onclick="loadApplications()">查询</button>'
        },
        {
            key: 'students',
            title: '学生管理',
            description: '新增、查询和维护学生资料及账号状态。',
            fields: '<div class="field"><label for="studentKeyword">搜索学生</label><input type="text" id="studentKeyword" placeholder="学号 / 姓名 / 专业"></div>',
            actions: '<button class="btn btn-primary" type="button" onclick="loadStudents()">查询</button>'
        },
        {
            key: 'teachers',
            title: '教师管理',
            description: '新增、查询和维护教师资料及账号状态。',
            fields: '<div class="field"><label for="teacherKeyword">搜索教师</label><input type="text" id="teacherKeyword" placeholder="工号 / 姓名 / 院系"></div>',
            actions: '<button class="btn btn-primary" type="button" onclick="loadTeachers()">查询</button>'
        },
        {
            key: 'academics',
            title: '教务管理',
            description: '新增、查询和维护教务管理员账号。',
            fields: '<div class="field"><label for="academicKeyword">搜索教务管理员</label><input type="text" id="academicKeyword" placeholder="编号 / 姓名 / 登录账号"></div>',
            actions: '<button class="btn btn-primary" type="button" onclick="loadAcademics()">查询</button>'
        },
        {
            key: 'batches',
            title: '选课批次',
            description: '设置选课开放时间、课程范围及面向学生。'
        },
        {
            key: 'classes',
            title: '容量调整',
            description: '查询教学班并调整可选容量。',
            fields: '<div class="field"><label for="classKeyword">搜索教学班</label><input type="text" id="classKeyword" placeholder="课程 / 教学班 / 教师 / 学期"></div>',
            actions: '<button class="btn btn-primary" type="button" onclick="loadAdminClasses()">查询</button>'
        },
        {
            key: 'evaluations',
            title: '评价管理',
            description: '查询课程评价结果与文字反馈，并支持导出。',
            fields: '<div class="field"><label for="evaluationKeyword">关键词</label><input id="evaluationKeyword" placeholder="课程 / 教学班 / 教师"></div>' +
                '<div class="field"><label>学年学期</label><div id="evaluationSemesterPicker"></div></div>',
            actions: '<button class="btn btn-secondary" type="button" onclick="exportEvaluations()">导出 Excel</button>' +
                '<button class="btn btn-primary" type="button" onclick="loadEvaluations()">查询评价</button>'
        },
        {
            key: 'logs',
            title: '系统日志',
            description: '按条件查询和导出系统操作记录。',
            filterClass: 'log-filter-bar',
            fields: '<div class="field"><label for="logKeyword">关键词</label><input type="text" id="logKeyword" placeholder="账号 / 对象 / 描述"></div>' +
                '<div class="field"><label for="logOperationType">操作类型</label><input type="text" id="logOperationType" placeholder="登录 / 新增 / 修改"></div>' +
                '<div class="field"><label for="logStartTime">开始时间</label><input class="datetime-clean-empty" type="datetime-local" id="logStartTime" value="" required></div>' +
                '<div class="field"><label for="logEndTime">结束时间</label><input class="datetime-clean-empty" type="datetime-local" id="logEndTime" value="" required></div>',
            actions: '<button class="btn btn-secondary" type="button" onclick="clearLogFilters()">重置</button>' +
                '<button class="btn btn-secondary" type="button" onclick="exportLogs()">导出日志</button>' +
                '<button class="btn btn-primary" type="button" onclick="loadLogs()">查询日志</button>'
        }
    ];

    function renderAdminPageChrome() {
        if (!window.sharedUi) return;

        pageChrome.forEach(config => {
            const heading = document.getElementById(config.key + 'PageHeading');
            if (heading) {
                heading.innerHTML = window.sharedUi.pageHeading(config.title, config.description);
            }

            const filter = document.getElementById(config.key + 'FilterBar');
            if (filter && config.fields) {
                filter.innerHTML = window.sharedUi.filterBar(config.fields, config.actions, config.filterClass);
            }
        });
    }

    // 脚本位于页面底部，此时挂载点已存在。先生成筛选控件，供其他页面的
    // DOMContentLoaded 初始化逻辑安全读取，避免因脚本注册顺序出现空节点。
    renderAdminPageChrome();

    const loaders = {
        dashboard: () => typeof loadAdminDashboard === 'function' && loadAdminDashboard(),
        courses: () => typeof loadCourses === 'function' && loadCourses(),
        scheduling: () => typeof loadSchedules === 'function' && loadSchedules(),
        applications: () => typeof loadApplications === 'function' && loadApplications(),
        evaluations: () => typeof loadEvaluations === 'function' && loadEvaluations(),
        students: () => typeof loadStudents === 'function' && loadStudents(),
        teachers: () => typeof loadTeachers === 'function' && loadTeachers(),
        academics: () => typeof loadAcademics === 'function' && loadAcademics(),
        batches: () => typeof loadBatches === 'function' && loadBatches(),
        classes: () => typeof loadAdminClasses === 'function' && loadAdminClasses(),
        selection: () => typeof loadSelectionPage === 'function' && loadSelectionPage(),
        logs: () => typeof initializeLogPage === 'function' && initializeLogPage(),
        profile: () => typeof renderAdminProfile === 'function' && renderAdminProfile()
    };

    function setGroupExpanded(group, expanded) {
        const items = group.querySelector('.nav-group-items');
        const toggle = group.querySelector('.nav-group-toggle');
        if (!items || !toggle) return;
        items.hidden = !expanded;
        toggle.setAttribute('aria-expanded', String(expanded));
    }

    window.toggleAdminNavGroup = function (groupId, button) {
        const items = document.getElementById(groupId);
        if (!items) return;
        const group = items.closest('.nav-collapsible');
        const shouldExpand = items.hidden;

        document.querySelectorAll('.nav-collapsible').forEach(item => {
            if (item !== group) setGroupExpanded(item, false);
        });
        setGroupExpanded(group, shouldExpand);
    };

    // 管理员职责划分：
    // 超级管理员(admin_level=0)：拥有全部管理功能 —— 教学管理、选课管理、用户管理（学生/教师账号）、系统日志
    // 教务管理员(admin_level=1)：教学管理（课程/排课/开课审批/评价/容量）、选课管理（批次/代选课程）
    window.ADMIN_TAB_PERMISSIONS = {
        super: ['dashboard', 'courses', 'scheduling', 'applications', 'evaluations', 'classes', 'batches', 'selection', 'students', 'teachers', 'academics', 'logs', 'profile'],
        academic: ['dashboard', 'courses', 'scheduling', 'applications', 'evaluations', 'classes', 'batches', 'selection', 'profile']
    };

    window.isAdminTabAllowed = function (tabName) {
        if (window.ADMIN_LEVEL === undefined) return false;
        const allowed = window.ADMIN_LEVEL === 0 ? window.ADMIN_TAB_PERMISSIONS.super : window.ADMIN_TAB_PERMISSIONS.academic;
        return allowed.indexOf(tabName) >= 0;
    };

    window.switchTab = function (tabName) {
        if (!window.isAdminTabAllowed(tabName)) {
            alert('当前管理员类型无权访问此功能');
            return;
        }

        document.querySelectorAll('.tab-content').forEach(tab => tab.classList.remove('active'));
        document.querySelectorAll('.nav-item').forEach(item => item.classList.remove('active'));

        const tab = document.getElementById(tabName + '-tab');
        if (tab) {
            tab.classList.add('active');
        }

        const nav = document.querySelector(`.nav-item[data-tab="${tabName}"]`);
        if (nav) {
            nav.classList.add('active');
            const group = nav.closest('.nav-collapsible');
            if (group) {
                document.querySelectorAll('.nav-collapsible').forEach(item => setGroupExpanded(item, item === group));
            } else {
                document.querySelectorAll('.nav-collapsible').forEach(item => setGroupExpanded(item, false));
            }
        }

        if (loaders[tabName]) {
            loaders[tabName]();
        }
    };

    window.adminEscape = window.sharedUi.escapeHtml;

    window.adminFetch = function (url, options) {
        const requestOptions = options || {};
        const method = String(requestOptions.method || 'GET').toUpperCase();
        const mutationKey = method === 'GET' ? '' : `${method}:${url}:${requestOptions.body || ''}`;
        if (mutationKey && pendingMutations.has(mutationKey)) return pendingMutations.get(mutationKey);

        const request = (async function () {
            const response = await fetch(url, requestOptions);
            const contentType = response.headers.get('content-type') || '';
            const data = contentType.includes('application/json') ? await response.json() : await response.text();

            if (response.status === 401) {
                window.location.replace(loginPortalUrl);
                throw new Error('登录已失效，请重新登录');
            }

            if (!response.ok) {
                const message = window.sharedUi.errorText(data, String(data || response.statusText));
                throw new Error(message);
            }

            return data;
        })();

        if (mutationKey) {
            pendingMutations.set(mutationKey, request);
            request.finally(() => pendingMutations.delete(mutationKey)).catch(() => {});
        }
        return request;
    };

    window.adminStatusBadge = function (status) {
        return Number(status) === 1
            ? window.sharedUi.statusBadge('激活', 'active')
            : window.sharedUi.statusBadge('禁用', 'disabled');
    };

    window.adminBatchBadge = function (status, text) {
        const numberStatus = Number(status);
        if (numberStatus === 0) {
            return window.sharedUi.statusBadge(text || '未开始', 'not-started');
        }

        if (numberStatus === 1) {
            return window.sharedUi.statusBadge(text || '进行中', 'ongoing');
        }

        return window.sharedUi.statusBadge(text || '已结束', 'ended');
    };

    document.addEventListener('DOMContentLoaded', function () {
        if (typeof loadAdminCurrent === 'function') {
            loadAdminCurrent();
        }

        const active = document.querySelector('.tab-content.active');
        if (active && active.id) {
            const tabName = active.id.replace('-tab', '');
            if (loaders[tabName]) {
                loaders[tabName]();
            }
        }

        const logoutButton = document.getElementById('logout-button');
        if (logoutButton) {
            logoutButton.addEventListener('click', async () => {
                try {
                    await window.adminFetch('/api/auth/logout', { method: 'POST' });
                } catch (e) {
                    // 退出请求失败也继续跳转
                }
                window.location.replace(loginPortalUrl);
            });
        }
    });
})();
