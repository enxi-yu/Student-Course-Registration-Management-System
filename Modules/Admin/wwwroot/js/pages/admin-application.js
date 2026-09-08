// 切换选项卡（课程管理/开课申请管理）
function switchTab(tabName) {
    document.querySelectorAll('.tab-content').forEach(tab => {
        tab.classList.remove('active');
    });
    document.querySelectorAll('.nav-item').forEach(item => {
        item.classList.remove('active');
    });
    document.getElementById(tabName + '-tab').classList.add('active');

    const buttons = document.querySelectorAll('.nav-item');
    if (tabName === 'courses') {
        buttons[0].classList.add('active');
        loadCourses();
    } 
    else if (tabName === 'applications') {
        buttons[1].classList.add('active');
        loadApplications();
    }
}

let applicationPage = 1;

// 加载开课申请列表
async function loadApplications(resetPage = true) {
    const tbody = document.getElementById('applicationTableBody');
    if (resetPage) applicationPage = 1;
    window.sharedUi.setTableState(tbody, 9, '正在加载开课申请...');
    try {
        const keyword = document.getElementById('applicationKeyword').value.trim();
        const status = document.getElementById('applicationStatusFilter').value;
        const params = new URLSearchParams();
        if (keyword) params.set('keyword', keyword);
        if (status) params.set('status', status);
        const qs = params.toString();

        const data = await adminFetch('/api/admin/applications' + (qs ? '?' + qs : ''));
        applicationPage = window.sharedUi.renderPagedTable({ body: tbody, rows: data, page: applicationPage, pageSize: 15, colspan: 9, emptyText: '暂无开课申请记录', pagination: 'applicationPagination', onPageChange: page => { applicationPage = page; loadApplications(false); }, row: app => {
            const statusInfo = getStatusInfo(app.status);
            const typeClass = getTypeClass(app.courseType);
            return `
              <tr>
                <td>${app.applyId}</td>
                <td>${app.teacherNo}</td>
                <td>${app.courseName}</td>
                <td><span class="type-badge ${typeClass}">${app.courseType}</span></td>
                <td>${app.credit}</td>
                <td>${app.textbook || '-'}</td>
                <td>${app.applyTime || '-'}</td>
                <td>${window.sharedUi.statusBadge(statusInfo.text, statusInfo.class)}</td>
                <td><button class="btn btn-sm btn-edit table-action" onclick="viewDetail('${app.applyId}')">查看详情</button></td>
              </tr>
            `;
        }});
    } catch (error) {
        window.sharedUi.setTableError(tbody, 9, window.sharedUi.errorText(error, '加载失败'), () => loadApplications(false));
    }
}

// 查看申请详情（调用详情API，填充弹窗）
function viewDetail(applyId) {
    adminFetch('/api/admin/applications/' + applyId)
        .then(data => {
            document.getElementById('detailApplyId').value = data.applyId;
            
            document.getElementById('detailContent').innerHTML = `
                <p><strong>申请编号：</strong>${data.applyId}</p>
                <p><strong>教师工号：</strong>${data.teacherNo}</p>
                <p><strong>课程名称：</strong>${data.courseName}</p>
                <p><strong>课程类型：</strong>${data.courseType}</p>
                <p><strong>学分：</strong>${data.credit}</p>
                <p><strong>教材：</strong>${data.textbook || '-'}</p>
                <p><strong>申请时间：</strong>${data.applyTime || '-'}</p>
                <p><strong>状态：</strong>${getStatusInfo(data.status).text}</p>
                <p><strong>审批时间：</strong>${data.approveTime || '-'}</p>
                <p><strong>审批意见：</strong>${data.approveComment || '-'}</p>
                <p><strong>课程简介：</strong>${data.courseSummary || '-'}</p>
            `;

            const btnApprove = document.getElementById('btnApprove');
            btnApprove.style.display = (data.status === '待审核' || !data.status) ? 'inline-block' : 'none';
            document.getElementById('detailModal').style.display = 'flex';
        })
        .catch(error => {
            alert('获取申请详情失败：' + window.sharedUi.errorText(error, '未登录'));
        });
}

// 关闭详情弹窗
function closeDetailModal() {
    document.getElementById('detailModal').style.display = 'none';
}

// 打开审批弹窗（从详情弹窗跳转）
function openApproveModal() {
    const applyId = document.getElementById('detailApplyId').value;
    document.getElementById('detailModal').style.display = 'none';
    window.sharedUi.formDialog({title:'审批开课申请',description:'请选择审批结果，审批意见可选。',submitText:'提交审批',busyText:'提交中...',fields:[{name:'status',label:'审批结果',kind:'select',required:true,options:['通过','驳回']},{name:'comment',label:'审批意见',kind:'textarea',wide:true,value:''}],onSubmit:values=>adminFetch('/api/admin/applications/'+encodeURIComponent(applyId)+'/approve',{method:'PUT',headers:{'Content-Type':'application/json'},body:JSON.stringify(values)}),onSuccess:async values=>{alert(values.status==='驳回'?'申请已驳回':'申请已通过');await loadApplications();}});
}

// 获取状态信息（类名和显示文本）
function getStatusInfo(status) {
    if (!status) 
        return { class: 'pending', text: '待审核' };
    const lowerStatus = status.toLowerCase();
    if (lowerStatus.includes('通过')) 
        return { class: 'approved', text: '已通过' };
    if (lowerStatus.includes('驳回') || lowerStatus.includes('拒绝')) 
        return { class: 'rejected', text: '已驳回' };
    if (lowerStatus.includes('已开课')) 
        return { class: 'ongoing', text: '已开课' };
    return { class: 'pending', text: '待审核' };
}

// 根据课程类型获取对应的CSS类名
function getTypeClass(type) {
    if (!type) 
        return 'elective';
    const lowerType = type.toLowerCase();
    if (lowerType.includes('必修')) 
        return 'required';
    if (lowerType.includes('公选')) 
        return 'public';
    return 'elective';
}

// 页面加载完成后初始化
document.addEventListener('DOMContentLoaded', function() {
    loadApplications();
});
