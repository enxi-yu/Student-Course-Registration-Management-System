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

// 加载开课申请列表
function loadApplications() {
    fetch('/api/admin/applications')
        .then(response => response.json())
        .then(data => {
            const tbody = document.getElementById('applicationTableBody');
            tbody.innerHTML = '';
            if (data.length === 0) {
                tbody.innerHTML = '<tr><td colspan="11" class="empty-state">暂无开课申请记录</td></tr>';
                return;
            }

            data.forEach(app => {
                const statusInfo = getStatusInfo(app.status);
                const typeClass = getTypeClass(app.courseType);
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>${app.applyId}</td>
                    <td>${app.teacherNo}</td>
                    <td>${app.courseName}</td>
                    <td><span class="type-badge ${typeClass}">${app.courseType}</span></td>
                    <td>${app.credit}</td>
                    <td>${app.totalHours}</td>
                    <td>${app.department}</td>
                    <td>${app.textbook || '-'}</td>
                    <td>${app.applyTime || '-'}</td>
                    <td><span class="status-badge ${statusInfo.class}">${statusInfo.text}</span></td>
                    <td><button class="btn btn-sm btn-edit" onclick="viewDetail('${app.applyId}')">查看详情</button></td>
                `;
                tbody.appendChild(row);
            });
        })
        .catch(error => {
            console.error('加载开课申请失败:', error);
        });
}

// 查看申请详情（调用详情API，填充弹窗）
function viewDetail(applyId) {
    fetch('/api/admin/applications/' + applyId)
        .then(response => response.json())
        .then(data => {
            document.getElementById('detailApplyId').value = data.applyId;
            
            document.getElementById('detailContent').innerHTML = `
                <p><strong>申请编号：</strong>${data.applyId}</p>
                <p><strong>教师工号：</strong>${data.teacherNo}</p>
                <p><strong>课程名称：</strong>${data.courseName}</p>
                <p><strong>课程类型：</strong>${data.courseType}</p>
                <p><strong>学分：</strong>${data.credit}</p>
                <p><strong>总学时：</strong>${data.totalHours}</p>
                <p><strong>开设院系：</strong>${data.department || '-'}</p>
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
            console.error('获取申请详情失败:', error);
            alert('获取申请详情失败: ' + error.message);
        });
}

// 关闭详情弹窗
function closeDetailModal() {
    document.getElementById('detailModal').style.display = 'none';
}

// 打开审批弹窗（从详情弹窗跳转）
function openApproveModal() {
    const applyId = document.getElementById('detailApplyId').value;
    document.getElementById('approveApplyId').value = applyId;
    document.getElementById('approveStatus').value = '';
    document.getElementById('approveComment').value = '';
    document.getElementById('detailModal').style.display = 'none';
    document.getElementById('approveModal').style.display = 'flex';
}

// 关闭审批弹窗
function closeApproveModal() {
    document.getElementById('approveModal').style.display = 'none';
}

// 提交审批（调用审批API）
function submitApproval() {
    const applyId = document.getElementById('approveApplyId').value;
    const status = document.getElementById('approveStatus').value;
    const comment = document.getElementById('approveComment').value;
    if (!status) {
        alert('请选择审批结果');
        return;
    }
    fetch('/api/admin/applications/' + applyId + '/approve', {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            Status: status,
            Comment: comment
        })
    })
    .then(response => {
        if (response.ok) {
            alert('审批成功');
            closeApproveModal();
            loadApplications();
        } else {
            response.text().then(text => alert('审批失败: ' + text));
        }
    })
    .catch(error => {
        console.error('审批失败:', error);
        alert('审批失败: ' + error.message);
    });
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