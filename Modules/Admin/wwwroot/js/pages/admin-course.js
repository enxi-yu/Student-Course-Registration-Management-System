let courseFormController;
function ensureCourseForm() {
    if (!courseFormController) courseFormController = window.sharedUi.createForm({
        root: '#courses-tab', submitButton: '#publishCourseButton', busyText: '新增中...',
        fields: [
            { name: 'courseName', label: '课程名称', selector: '#courseName', required: true },
            { name: 'courseType', label: '课程类型', selector: '#courseType', required: true, oneOf: ['必修', '选修', '公选'] },
            { name: 'credit', label: '学分', selector: '#credit', required: true, type: 'number', min: 0.5, invalidMessage: '学分必须大于 0' },
            { name: 'department', label: '开课学院', selector: '#department', required: true },
            { name: 'courseDesc', label: '课程描述', selector: '#courseDesc' }
        ]
    });
    return courseFormController;
}

function applyCourseDepartmentScope() {
    const input = document.getElementById('department');
    if (!input) return;
    const scoped = Number(window.ADMIN_LEVEL) === 1;
    input.readOnly = scoped;
    if (scoped) input.value = window.ADMIN_DEPARTMENT || '';
    input.title = scoped ? '普通教务管理员只能维护所属学院的课程' : '';
}

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
    window.sharedUi.setTableState(tbody, 6, '正在加载课程数据...');
    try {
        const keyword = document.getElementById('courseKeyword').value.trim();
        const coursetype = document.getElementById('courseTypeFilter').value;
        const params = new URLSearchParams();
        if (keyword) params.set('keyword', keyword);
        if (coursetype) params.set('coursetype', coursetype);
        const qs = params.toString();

        const data = await adminFetch('/api/admin/courses' + (qs ? '?' + qs : ''));
        window.sharedUi.renderTableRows(tbody, data, c => `
                <tr>
                    <td>${adminEscape(c.courseName)}</td>
                    <td>${getTypeBadge(c.courseType)}</td>
                    <td>${adminEscape(c.credit)}</td>
                    <td>${adminEscape(c.department || '-')}</td>
                    <td>${adminEscape(c.courseDesc || '-')}</td>
                    <td>
                        <button class="btn btn-sm btn-edit" onclick="openEditModal(${c.courseId})">编辑</button>
                        <button class="btn btn-sm btn-delete" onclick="deleteCourse(${c.courseId})">删除</button>
                    </td>
                </tr>
            `, 6, '暂无课程数据');
    } catch (err) {
        window.sharedUi.setTableError(tbody, 6, `课程数据加载失败：${(err && err.message) || '服务暂不可用'}`, loadCourses);
    }
}

async function publishCourse() {
    if (ensureCourseForm().isSubmitting()) return;
    try {
        await ensureCourseForm().submit(async values => {
            await adminFetch('/api/admin/courses', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ courseName: values.courseName, courseType: values.courseType, credit: values.credit, totalHours: 0, department: values.department, courseDesc: values.courseDesc }) });
        }, '新增中...');
        alert('课程新增成功！');
        clearForm();
        await loadCourses();
    } catch (error) { alert('新增失败：' + error.message); }
}

function clearForm() {
    document.getElementById('courseName').value = '';
    document.getElementById('courseType').value = '';
    document.getElementById('credit').value = '';
    document.getElementById('department').value = Number(window.ADMIN_LEVEL) === 1 ? (window.ADMIN_DEPARTMENT || '') : '';
    document.getElementById('courseDesc').value = '';
}

async function openEditModal(id) {
    try {
        const data = await adminFetch('/api/admin/courses/' + id);
        const scoped = Number(window.ADMIN_LEVEL) === 1;
        const dialog = window.sharedUi.formDialog({title:'编辑课程',description:scoped?'只能修改本学院课程，开课学院不可变更。':'修改课程基础信息。',submitText:'保存修改',fields:[
            {name:'courseName',label:'课程名称',required:true,value:data.courseName},
            {name:'courseType',label:'课程类型',kind:'select',required:true,value:data.courseType,options:['必修','选修','公选']},
            {name:'credit',label:'学分',type:'number',required:true,min:0.5,step:0.5,value:data.credit,validate:value=>Number(value)>0?'':'学分必须大于 0'},
            {name:'department',label:'开课学院',required:true,value:scoped?(window.ADMIN_DEPARTMENT||''):(data.department||'')},
            {name:'courseDesc',label:'课程描述',kind:'textarea',wide:true,value:data.courseDesc||''}
        ],onSubmit:values=>adminFetch('/api/admin/courses/'+id,{method:'PUT',headers:{'Content-Type':'application/json'},body:JSON.stringify({...values,credit:Number(values.credit),totalHours:0})}),onSuccess:async()=>{alert('课程更新成功！');await loadCourses();}});
        if (scoped && dialog.form.elements.department) dialog.form.elements.department.readOnly = true;
    } catch (error) { alert('加载课程信息失败：'+error.message); }
}

async function deleteCourse(id) {
    if (!await adminDialog.danger('确定要删除这门课程吗？此操作不可撤销！', '删除课程')) {
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
