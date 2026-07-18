async function loadLogs() {
    const tbody = document.getElementById('logTableBody');
    if (!tbody) return;

    const params = new URLSearchParams();
    const keyword = document.getElementById('logKeyword').value;
    const operationType = document.getElementById('logOperationType').value;
    const startTime = document.getElementById('logStartTime').value;
    const endTime = document.getElementById('logEndTime').value;

    if (keyword) params.set('keyword', keyword);
    if (operationType) params.set('operationType', operationType);
    if (startTime) params.set('startTime', startTime);
    if (endTime) params.set('endTime', endTime);

    const query = params.toString() ? '?' + params.toString() : '';

    try {
        const rows = await adminFetch('/api/admin/logs' + query);
        if (!rows.length) {
            tbody.innerHTML = adminEmptyRow(7, '暂无系统日志');
            return;
        }

        tbody.innerHTML = rows.map(item => `
            <tr>
                <td>${adminEscape(item.logTime)}</td>
                <td>${adminEscape(item.username || item.userId || '-')}</td>
                <td>${adminEscape(item.operationType)}</td>
                <td>${adminEscape(item.operationDesc)}</td>
                <td>${adminEscape(item.targetId || '-')}</td>
                <td>${renderLogResult(item.resultStatus, item.errorMessage)}</td>
                <td>${adminEscape(item.ipAddress || '-')}</td>
            </tr>
        `).join('');
    } catch (error) {
        tbody.innerHTML = adminEmptyRow(7, error.message);
    }
}

function clearLogFilters() {
    document.getElementById('logKeyword').value = '';
    document.getElementById('logOperationType').value = '';
    document.getElementById('logStartTime').value = '';
    document.getElementById('logEndTime').value = '';
    loadLogs();
}

function renderLogResult(status, errorMessage) {
    const text = adminEscape(status || '-');
    if (status === '成功') {
        return `<span class="status-badge active">${text}</span>`;
    }

    return `<span class="status-badge disabled">${text}</span>${errorMessage ? '<br>' + adminEscape(errorMessage) : ''}`;
}
