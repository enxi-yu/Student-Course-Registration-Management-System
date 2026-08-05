(function () {
    const loaders = {
        courses: () => typeof loadCourses === 'function' && loadCourses(),
        applications: () => typeof loadApplications === 'function' && loadApplications(),
        students: () => typeof loadStudents === 'function' && loadStudents(),
        teachers: () => typeof loadTeachers === 'function' && loadTeachers(),
        batches: () => typeof loadBatches === 'function' && loadBatches(),
        classes: () => typeof loadAdminClasses === 'function' && loadAdminClasses(),
        logs: () => typeof loadLogs === 'function' && loadLogs(),
        permissions: () => {
            if (typeof loadAdminCurrent === 'function') loadAdminCurrent();
            if (typeof loadAdminPermissions === 'function') loadAdminPermissions();
        }
    };

    window.switchTab = function (tabName) {
        document.querySelectorAll('.tab-content').forEach(tab => tab.classList.remove('active'));
        document.querySelectorAll('.nav-item').forEach(item => item.classList.remove('active'));

        const tab = document.getElementById(tabName + '-tab');
        if (tab) {
            tab.classList.add('active');
        }

        const nav = document.querySelector(`.nav-item[data-tab="${tabName}"]`);
        if (nav) {
            nav.classList.add('active');
        }

        if (loaders[tabName]) {
            loaders[tabName]();
        }
    };

    window.adminEscape = function (value) {
        if (value === null || value === undefined) {
            return '';
        }

        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    };

    window.adminFetch = async function (url, options) {
        const response = await fetch(url, options || {});
        const contentType = response.headers.get('content-type') || '';
        const data = contentType.includes('application/json') ? await response.json() : await response.text();

        if (!response.ok) {
            const message = data && data.message ? data.message : String(data || response.statusText);
            throw new Error(message);
        }

        return data;
    };

    window.adminStatusBadge = function (status) {
        return Number(status) === 1
            ? '<span class="status-badge active">激活</span>'
            : '<span class="status-badge disabled">禁用</span>';
    };

    window.adminBatchBadge = function (status, text) {
        const numberStatus = Number(status);
        if (numberStatus === 0) {
            return `<span class="status-badge not-started">${adminEscape(text || '未开始')}</span>`;
        }

        if (numberStatus === 1) {
            return `<span class="status-badge ongoing">${adminEscape(text || '进行中')}</span>`;
        }

        return `<span class="status-badge ended">${adminEscape(text || '已结束')}</span>`;
    };

    window.adminEmptyRow = function (colspan, text) {
        return `<tr><td colspan="${colspan}" class="empty-state">${adminEscape(text)}</td></tr>`;
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
    });
})();
