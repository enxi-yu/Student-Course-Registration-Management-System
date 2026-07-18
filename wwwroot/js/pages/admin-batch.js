let adminBatches = [];

async function loadBatches() {
    const tbody = document.getElementById('batchTableBody');
    if (!tbody) return;

    try {
        adminBatches = await adminFetch('/api/admin/batches');
        if (!adminBatches.length) {
            tbody.innerHTML = adminEmptyRow(6, '暂无选课批次');
            return;
        }

        tbody.innerHTML = adminBatches.map(item => `
            <tr>
                <td>${adminEscape(item.batchId)}</td>
                <td>${adminEscape(item.batchName)}</td>
                <td>${adminEscape(item.startTime)}</td>
                <td>${adminEscape(item.endTime)}</td>
                <td>${adminBatchBadge(item.status, item.statusText)}</td>
                <td><button class="btn btn-sm btn-edit" onclick="editBatch(${item.batchId})">编辑</button></td>
            </tr>
        `).join('');
    } catch (error) {
        tbody.innerHTML = adminEmptyRow(6, error.message);
    }
}

async function saveBatch() {
    const batchId = document.getElementById('batchId').value;
    const statusValue = document.getElementById('batchStatus').value;
    const payload = {
        batchName: document.getElementById('batchName').value,
        startTime: document.getElementById('batchStartTime').value,
        endTime: document.getElementById('batchEndTime').value,
        status: statusValue === '' ? null : Number(statusValue)
    };

    try {
        await adminFetch(batchId ? `/api/admin/batches/${encodeURIComponent(batchId)}` : '/api/admin/batches', {
            method: batchId ? 'PUT' : 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        alert('选课批次已保存');
        clearBatchForm();
        loadBatches();
    } catch (error) {
        alert('保存失败：' + error.message);
    }
}

function editBatch(batchId) {
    const item = adminBatches.find(row => Number(row.batchId) === Number(batchId));
    if (!item) return;

    document.getElementById('batchFormTitle').textContent = '编辑选课批次';
    document.getElementById('batchId').value = item.batchId;
    document.getElementById('batchName').value = item.batchName || '';
    document.getElementById('batchStartTime').value = toDateTimeLocal(item.startTime);
    document.getElementById('batchEndTime').value = toDateTimeLocal(item.endTime);
    document.getElementById('batchStatus').value = String(item.status);
    document.getElementById('batchName').focus();
}

function clearBatchForm() {
    document.getElementById('batchFormTitle').textContent = '新增选课批次';
    document.getElementById('batchId').value = '';
    document.getElementById('batchName').value = '';
    document.getElementById('batchStartTime').value = '';
    document.getElementById('batchEndTime').value = '';
    document.getElementById('batchStatus').value = '';
}

function toDateTimeLocal(value) {
    if (!value) return '';
    return String(value).replace(' ', 'T').slice(0, 16);
}
