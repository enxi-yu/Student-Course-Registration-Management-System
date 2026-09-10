let logPage = 1;
let logHasMore = false;

function initializeLogPage() {
    logPage = 1;
    logHasMore = false;
    document.getElementById('logResults').hidden = true;
    document.getElementById('logTableBody').innerHTML = '';
}

function logQuery(page, pageSize) {
    const params = new URLSearchParams();
    const keyword = document.getElementById('logKeyword').value.trim();
    const operationType = document.getElementById('logOperationType').value.trim();
    const startTime = document.getElementById('logStartTime').value;
    const endTime = document.getElementById('logEndTime').value;
    if (keyword) params.set('keyword', keyword);
    if (operationType) params.set('operationType', operationType);
    if (startTime) params.set('startTime', startTime);
    if (endTime) params.set('endTime', endTime);
    params.set('page', page);
    params.set('pageSize', pageSize);
    return '?' + params.toString();
}

async function loadLogs(resetPage = true) {
    const tbody = document.getElementById('logTableBody');
    if (!tbody) return;
    if (resetPage) logPage = 1;
    window.sharedUi.setTableState(tbody, 7, '正在查询系统日志...');

    try {
        const data = await adminFetch('/api/admin/logs' + logQuery(logPage, 20));
        document.getElementById('logResults').hidden = false;
        const rows = data.items || [];
        logHasMore = Boolean(data.hasMore);
        window.sharedUi.renderTableRows(tbody, rows, item => `
                <tr>
                    <td>${adminEscape(item.logTime)}</td>
                    <td>${adminEscape(item.username || item.adminNo || '-')}</td>
                    <td>${adminEscape(item.operationType)}</td>
                    <td>${adminEscape(item.operationDesc)}</td>
                    <td>${adminEscape(item.targetId || '-')}</td>
                    <td>${renderLogResult(item.resultStatus, item.errorMessage)}</td>
                    <td>${adminEscape(item.ipAddress || '-')}</td>
                </tr>
            `, 7, '暂无系统日志');
        window.sharedUi.pagination('logPagination',{page:logPage,pageSize:20,hasMore:logHasMore,onChange:page=>{logPage=page;loadLogs(false);}});
    } catch (error) {
        window.sharedUi.setTableError(tbody,7,error.message,()=>loadLogs(false));
    }
}

function clearLogFilters() {
    document.getElementById('logKeyword').value = '';
    document.getElementById('logOperationType').value = '';
    document.getElementById('logStartTime').value = '';
    document.getElementById('logEndTime').value = '';
    initializeLogPage();
}

async function exportLogs() {
    try {
        const response = await fetch('/api/admin/logs/export' + logQuery(1, 20));
        if (!response.ok) {
            const data = await response.json().catch(() => ({}));
            throw new Error(data.message || '导出失败');
        }
        const blob = await response.blob();
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = `系统日志_${new Date().toISOString().slice(0, 10)}.xlsx`;
        link.click();
        URL.revokeObjectURL(link.href);
    } catch (error) {
        alert('导出失败：' + error.message);
    }
}

function renderLogResult(status, errorMessage) {
    const text = adminEscape(status || '-');
    if (status === '成功') {
        return `<span class="status-badge active">${text}</span>`;
    }

    return `<span class="status-badge disabled">${text}</span>${errorMessage ? '<br>' + adminEscape(errorMessage) : ''}`;
}
