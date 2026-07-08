function startBatch() {
    const startTime = document.getElementById('startTime').value;
    const endTime = document.getElementById('endTime').value;
    fetch('/api/admin/batches', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ start_time: startTime, end_time: endTime })
    }).then(res => { if(res.ok) alert('🚀 选课批次开启成功！'); });
}