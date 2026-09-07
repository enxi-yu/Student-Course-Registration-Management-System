async function loadAdminClasses() {
    const tbody = document.getElementById('classTableBody');
    if (!tbody) return;

    const keyword = document.getElementById('classKeyword').value;
    const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : '';
    window.sharedUi.setTableState(tbody, 8, '正在加载教学班数据...');

    try {
        const rows = await adminFetch('/api/admin/classes' + query);
        window.sharedUi.renderTableRows(tbody, rows, item => `
            <tr>
                <td>${adminEscape(item.classId)}</td>
                <td>${adminEscape(item.className)}</td>
                <td>${adminEscape(item.courseName)}<br><span class="muted-text">#${adminEscape(item.courseId)}</span></td>
                <td>${adminEscape(item.semester)}</td>
                <td>${adminEscape(item.teacherName || '-')}${item.teacherNo ? '<br>' + adminEscape(item.teacherNo) : ''}</td>
                <td>${adminEscape(item.capacity)}</td>
                <td>${adminEscape(item.selectedCount)}</td>
                <td><button class="btn btn-sm btn-edit" onclick="adjustCapacity(${item.classId}, ${item.capacity}, ${item.selectedCount})">调整容量</button></td>
            </tr>
        `, 8, '暂无教学班数据');
    } catch (error) {
        window.sharedUi.setTableError(tbody, 8, `教学班数据加载失败：${error.message}`, loadAdminClasses);
    }
}

function adjustCapacity(classId, capacity, selectedCount) {
    window.sharedUi.formDialog({title:'调整课程容量',summary:`当前容量：${capacity} 人　已选人数：${selectedCount} 人`,submitText:'保存调整',fields:[
        {name:'capacity',label:'新容量',type:'number',required:true,min:selectedCount,step:1,value:capacity,validate:value=>Number.isInteger(Number(value))&&Number(value)>=Number(selectedCount)?'':`新容量不能小于已选人数 ${selectedCount}`},
        {name:'remark',label:'调整原因',kind:'textarea',wide:true,value:''}
    ],onSubmit:values=>adminFetch(`/api/admin/classes/${encodeURIComponent(classId)}/capacity`,{method:'PUT',headers:{'Content-Type':'application/json'},body:JSON.stringify({capacity:Number(values.capacity),remark:values.remark})}),onSuccess:async()=>{alert('容量已调整');await loadAdminClasses();}});
}
