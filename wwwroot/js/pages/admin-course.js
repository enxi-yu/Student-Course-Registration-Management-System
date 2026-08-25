

function getTypeBadge(type) {
    const badges = {
        '必修': '<span class="type-badge required">必修</span>',
        '选修': '<span class="type-badge elective">选修</span>',
        '公选': '<span class="type-badge public">公选</span>'
    };
    return badges[type] || type;
}

async function loadCourses() {
    const tbody = document.getElementById('courseTableBody');
    try {
        const keyword = document.getElementById('courseKeyword').value.trim();
        const coursetype = document.getElementById('courseTypeFilter').value;
        const params = new URLSearchParams();
        if (keyword) params.set('keyword', keyword);
        if (coursetype) params.set('coursetype', coursetype);
        const qs = params.toString();

        const data = await adminFetch('/api/admin/courses' + (qs ? '?' + qs : ''));
        tbody.innerHTML = '';
        if (data.length === 0) {
            tbody.innerHTML = adminEmptyRow(8, '暂无课程数据');
            return;
        }
        data.forEach(c => {
            tbody.innerHTML += `
                <tr>
                    <td>${c.courseId}</td>
                    <td>${c.courseName}</td>
                    <td>${getTypeBadge(c.courseType)}</td>
                    <td>${c.credit}</td>
                    <td>${c.totalHours}</td>
                    <td>${c.department || '-'}</td>
                    <td>${c.courseDesc || '-'}</td>
                    <td>
                        <button class="btn btn-sm btn-edit" onclick="openEditModal(${c.courseId})">编辑</button>
                        <button class="btn btn-sm btn-delete" onclick="deleteCourse(${c.courseId})">删除</button>
                    </td>
                </tr>
            `;
        });
    } catch (err) {
        tbody.innerHTML = adminEmptyRow(8, (err && err.message) || 'Unauthorized: 未登录');
    }
}

function publishCourse() {
    const name = document.getElementById('courseName').value;
    const type = document.getElementById('courseType').value;
    const credit = document.getElementById('credit').value;
    const totalHours = document.getElementById('totalHours').value;
    const department = document.getElementById('department').value;
    const courseDesc = document.getElementById('courseDesc').value;

    if (!name) {
        return alert('请输入课程名称！');
    }
    if (!type) {
        return alert('请选择课程类型！');
    }
    if (!credit) {
        return alert('请输入学分！');
    }
    if (!totalHours) {
        return alert('请输入总学时！');
    }

    fetch('/api/admin/courses', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            courseName: name,
            courseType: type,
            credit: parseFloat(credit) || 0,
            totalHours: parseInt(totalHours) || 0,
            department: department,
            courseDesc: courseDesc
        })
    })
    .then(res => {
        if (res.ok) {
            alert('课程发布成功！');
            loadCourses();
            clearForm();
        } else {
            return res.text().then(msg => alert('发布失败：' + msg));
        }
    })
    .catch(err => alert('网络错误：' + err));
}

function clearForm() {
    document.getElementById('courseName').value = '';
    document.getElementById('courseType').value = '';
    document.getElementById('credit').value = '';
    document.getElementById('totalHours').value = '';
    document.getElementById('department').value = '';
    document.getElementById('courseDesc').value = '';
}

function openEditModal(id) {
    adminFetch('/api/admin/courses/' + id)
        .then(data => {
            document.getElementById('editCourseId').value = data.courseId;
            document.getElementById('editCourseIdDisplay').value = data.courseId;
            document.getElementById('editCourseName').value = data.courseName;
            document.getElementById('editCourseType').value = data.courseType;
            document.getElementById('editCredit').value = data.credit;
            document.getElementById('editTotalHours').value = data.totalHours;
            document.getElementById('editDepartment').value = data.department || '';
            document.getElementById('editCourseDesc').value = data.courseDesc || '';
            document.getElementById('editModal').style.display = 'flex';
        })
        .catch(err => alert('加载课程信息失败：' + ((err && err.message) || '未登录')));
}

function closeModal() {
    document.getElementById('editModal').style.display = 'none';
}

function updateCourse() {
    const id = document.getElementById('editCourseId').value;
    const name = document.getElementById('editCourseName').value;
    const type = document.getElementById('editCourseType').value;
    const credit = document.getElementById('editCredit').value;
    const totalHours = document.getElementById('editTotalHours').value;
    const department = document.getElementById('editDepartment').value;
    const courseDesc = document.getElementById('editCourseDesc').value;

    if (!name) {
        return alert('请输入课程名称！');
    }
    if (!type) {
        return alert('请选择课程类型！');
    }
    if (!credit) {
        return alert('请输入学分！');
    }
    if (!totalHours) {
        return alert('请输入总学时！');
    }

    fetch('/api/admin/courses/' + id, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            courseName: name,
            courseType: type,
            credit: parseFloat(credit) || 0,
            totalHours: parseInt(totalHours) || 0,
            department: department,
            courseDesc: courseDesc
        })
    })
    .then(res => {
        if (res.ok) {
            alert('课程更新成功！');
            closeModal();
            loadCourses();
        } else {
            return res.text().then(msg => alert('更新失败：' + msg));
        }
    })
    .catch(err => alert('网络错误：' + err));
}

function deleteCourse(id) {
    if (!confirm('确定要删除这门课程吗？此操作不可撤销！')) {
        return;
    }

    fetch('/api/admin/courses/' + id, {
        method: 'DELETE'
    })
    .then(res => {
        if (res.ok) {
            alert('课程删除成功！');
            loadCourses();
        } else {
            return res.text().then(msg => alert('删除失败：' + msg));
        }
    })
    .catch(err => alert('网络错误：' + err));
}