(function () {
    const weekdays = ['', '周一', '周二', '周三', '周四', '周五', '周六', '周日'];
    let optionsLoaded = false;
    let editingClassId = null;
    let searchBound = false;
    let semesterPicker = null;
    let semesterFilterPicker = null;
    let scheduleFormController = null;

    function currentSemester() {
        return window.academicSemester.getCurrent().canonical;
    }

    function timeRow(time) {
        const value = time || { weekday: 1, startPeriod: 1, endPeriod: 2, startWeek: 1, endWeek: 16, classroom: '' };
        return `<div class="schedule-time-row">
            <div class="field"><label>星期</label><select class="schedule-weekday">${weekdays.slice(1).map((day, index) => `<option value="${index + 1}"${index + 1 === Number(value.weekday) ? ' selected' : ''}>${day}</option>`).join('')}</select></div>
            <div class="field"><label>开始节次</label><input class="schedule-start-period" type="number" min="1" max="12" value="${Number(value.startPeriod)}"></div>
            <div class="field"><label>结束节次</label><input class="schedule-end-period" type="number" min="1" max="12" value="${Number(value.endPeriod)}"></div>
            <div class="field"><label>开始周</label><input class="schedule-start-week" type="number" min="1" max="30" value="${Number(value.startWeek)}"></div>
            <div class="field"><label>结束周</label><input class="schedule-end-week" type="number" min="1" max="30" value="${Number(value.endWeek)}"></div>
            <div class="field"><label>教室</label><input class="schedule-classroom" type="text" placeholder="例如：教学楼A101" value="${adminEscape(value.classroom || '')}"></div>
            <button class="btn btn-delete" type="button" onclick="removeScheduleTimeRow(this)">删除</button>
        </div>`;
    }

    window.addScheduleTimeRow = function () {
        document.getElementById('scheduleTimes').insertAdjacentHTML('beforeend', timeRow());
    };

    window.removeScheduleTimeRow = function (button) {
        const rows = document.querySelectorAll('.schedule-time-row');
        if (rows.length === 1) return alert('至少保留一条上课时间');
        button.closest('.schedule-time-row').remove();
    };

    const searchItems = { course: [], teacher: [] };

    function choiceText(type, item) {
        return type === 'course' ? `${item.courseName}（${item.courseId}）` : `${item.teacherName}（${item.teacherNo}）`;
    }

    function selectChoice(type, item) {
        document.getElementById(type === 'course' ? 'scheduleCourse' : 'scheduleTeacher').value = type === 'course' ? item.courseId : item.teacherNo;
        document.getElementById(type === 'course' ? 'scheduleCourseSearch' : 'scheduleTeacherSearch').value = choiceText(type, item);
        document.getElementById(type === 'course' ? 'scheduleCourseResults' : 'scheduleTeacherResults').hidden = true;
    }

    async function searchOptions(type, keyword) {
        const results = document.getElementById(type === 'course' ? 'scheduleCourseResults' : 'scheduleTeacherResults');
        if (!keyword) { results.hidden = true; results.innerHTML = ''; return; }
        const endpoint = type === 'course' ? 'courses' : 'teachers';
        const data = await adminFetch(`/api/admin/scheduling/${endpoint}?keyword=${encodeURIComponent(keyword || '')}&page=1&pageSize=50`);
        searchItems[type] = data.items || [];
        results.innerHTML = searchItems[type].length ? searchItems[type].map((item, index) => `<button type="button" data-schedule-choice="${type}" data-choice-index="${index}"><strong>${adminEscape(choiceText(type, item))}</strong><span>${adminEscape(type === 'course' ? item.department || '未设置院系' : [item.department, item.title].filter(Boolean).join(' · ') || '未设置院系和职称')}</span></button>`).join('') : '<div class="search-choice-empty">没有找到匹配结果</div>';
        results.hidden = false;
    }

    function bindSearch() {
        if (searchBound) return;
        ['course', 'teacher'].forEach(type => {
            const input = document.getElementById(type === 'course' ? 'scheduleCourseSearch' : 'scheduleTeacherSearch');
            let timer;
            input.addEventListener('input', () => {
                clearTimeout(timer);
                document.getElementById(type === 'course' ? 'scheduleCourse' : 'scheduleTeacher').value = '';
                timer = setTimeout(() => searchOptions(type, input.value.trim()).catch(error => alert('搜索失败：' + error.message)), 280);
            });
            input.addEventListener('search', () => searchOptions(type, input.value.trim()));
        });
        document.addEventListener('click', event => {
            const button = event.target.closest('[data-schedule-choice]');
            if (button) selectChoice(button.dataset.scheduleChoice, searchItems[button.dataset.scheduleChoice][Number(button.dataset.choiceIndex)]);
            if (!event.target.closest('.search-choice')) document.querySelectorAll('.search-choice-results').forEach(element => { element.hidden = true; });
        });
        searchBound = true;
    }

    async function loadOptions() {
        if (optionsLoaded) return;
        mountSemesterPicker(currentSemester());
        semesterFilterPicker = window.academicSemester.mountPicker('scheduleSemesterFilter', {
            minimumStartYear: 2024,
            selected: '',
            allowAll: true
        });
        bindSearch();
        optionsLoaded = true;
    }

    function mountSemesterPicker(selected) {
        semesterPicker = window.academicSemester.mountPicker('scheduleSemesterPicker', {
            minimumStartYear: 2024,
            selected: selected || currentSemester(),
            onChange: value => { document.getElementById('scheduleSemester').value = value; }
        });
        document.getElementById('scheduleSemester').value = semesterPicker.value();
    }

    window.resetSchedulingForm = function () {
        editingClassId = null;
        document.getElementById('scheduleCourse').value = '';
        document.getElementById('scheduleTeacher').value = '';
        document.getElementById('scheduleCourseSearch').value = '';
        document.getElementById('scheduleTeacherSearch').value = '';
        mountSemesterPicker(currentSemester());
        document.getElementById('scheduleClassName').value = '';
        document.getElementById('scheduleCapacity').value = '40';
        document.getElementById('scheduleTimes').innerHTML = timeRow();
    };

    function readTimes() {
        return Array.from(document.querySelectorAll('.schedule-time-row')).map(row => ({
            weekday: Number(row.querySelector('.schedule-weekday').value),
            startPeriod: Number(row.querySelector('.schedule-start-period').value),
            endPeriod: Number(row.querySelector('.schedule-end-period').value),
            startWeek: Number(row.querySelector('.schedule-start-week').value),
            endWeek: Number(row.querySelector('.schedule-end-week').value),
            classroom: row.querySelector('.schedule-classroom').value.trim()
        }));
    }

    function ensureScheduleForm() {
        if (!scheduleFormController) scheduleFormController = window.sharedUi.createForm({
            root: '#scheduling-tab', submitButton: '#saveScheduleButton', busyText: '保存中...',
            fields: [
                { name: 'courseId', label: '课程', selector: '#scheduleCourse', required: true, type: 'number' },
                { name: 'teacherNo', label: '任课教师', selector: '#scheduleTeacher', required: true },
                { name: 'semester', label: '学年学期', selector: '#scheduleSemester', required: true },
                { name: 'className', label: '教学班名称', selector: '#scheduleClassName', required: true },
                { name: 'capacity', label: '课程容量', selector: '#scheduleCapacity', required: true, type: 'number', min: 1 },
                { name: 'times', label: '上课时间', read: readTimes, required: true, validate: times => {
                    if (times.some(x => !Number.isInteger(x.weekday) || x.weekday < 1 || x.weekday > 7)) return '请选择正确的星期';
                    if (times.some(x => !Number.isInteger(x.startPeriod) || !Number.isInteger(x.endPeriod) || x.startPeriod < 1 || x.endPeriod > 12)) return '节次必须为1-12之间的整数';
                    if (times.some(x => !Number.isInteger(x.startWeek) || !Number.isInteger(x.endWeek) || x.startWeek < 1 || x.endWeek > 30)) return '周次必须为1-30之间的整数';
                    if (times.some(x => x.startPeriod > x.endPeriod)) return '结束节次不能早于开始节次';
                    if (times.some(x => x.startWeek > x.endWeek)) return '结束周不能早于开始周';
                    return '';
                } }
            ]
        });
        return scheduleFormController;
    }

    window.saveSchedule = async function () {
        if (ensureScheduleForm().isSubmitting()) return;
        try {
            await ensureScheduleForm().submit(async payload => {
                const url = editingClassId ? `/api/admin/scheduling/${editingClassId}` : '/api/admin/scheduling';
                await adminFetch(url, { method: editingClassId ? 'PUT' : 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
            });
            alert(editingClassId ? '排课修改成功，课程总学时已自动更新' : '排课保存成功，课程总学时已自动更新');
            resetSchedulingForm();
            await loadSchedules();
        } catch (error) {
            alert('排课失败：' + error.message);
        }
    };

    window.editSchedule = async function (classId) {
        try {
            await loadOptions();
            const item = await adminFetch(`/api/admin/scheduling/${classId}`);
            editingClassId = classId;
            selectChoice('course', { courseId: item.courseId, courseName: item.courseName });
            selectChoice('teacher', { teacherNo: item.teacherNo, teacherName: item.teacherName });
            mountSemesterPicker(item.semester);
            document.getElementById('scheduleClassName').value = item.className;
            document.getElementById('scheduleCapacity').value = String(item.capacity);
            document.getElementById('scheduleTimes').innerHTML = item.times.map(timeRow).join('') || timeRow();
            document.getElementById('scheduleCourseSearch').scrollIntoView({ behavior: 'smooth', block: 'center' });
        } catch (error) {
            alert('读取排课失败：' + error.message);
        }
    };

    window.loadSchedules = async function () {
        const body = document.getElementById('schedulingTableBody');
        if (!body) return;
        window.sharedUi.setTableState(body, 8, '正在加载排课数据...');
        try {
            await loadOptions();
            if (!document.querySelector('.schedule-time-row')) resetSchedulingForm();
            const semester = semesterFilterPicker ? semesterFilterPicker.value() : '';
            const keywordElement = document.getElementById('scheduleKeyword');
            const keyword = keywordElement ? keywordElement.value.trim() : '';
            const params = new URLSearchParams();
            if (semester) params.set('semester', semester);
            if (keyword) params.set('keyword', keyword);
            const queryString = params.toString();
            const rows = await adminFetch('/api/admin/scheduling' + (queryString ? `?${queryString}` : ''));
            window.sharedUi.renderTableRows(body, rows, item => `<tr>
                <td>${adminEscape(item.className)}<br><span class="muted-text">#${item.classId}</span></td>
                <td>${adminEscape(item.courseName)}<br><span class="muted-text">#${item.courseId}</span></td>
                <td>${adminEscape(window.academicSemester.format(item.semester))}</td>
                <td>${adminEscape(item.teacherName)}<br><span class="muted-text">${adminEscape(item.teacherNo)}</span></td>
                <td>${adminEscape(item.scheduleText)}</td><td>${item.selectedCount}/${item.capacity}</td><td>${item.totalHours}</td>
                <td><button class="btn btn-sm btn-edit" type="button" onclick="editSchedule(${item.classId})">编辑</button> <button class="btn btn-sm btn-delete" type="button" onclick="deleteSchedule(${item.classId})">删除排课</button></td>
            </tr>`, 8, '当前筛选条件下暂无排课');
        } catch (error) {
            window.sharedUi.setTableError(body, 8, `排课数据加载失败：${error.message}`, loadSchedules);
        }
    };

    window.deleteSchedule = async function (classId) {
        if (!await adminDialog.danger('确定删除该教学班及其全部上课时间吗？已有学生选课时系统会阻止删除。', '删除排课')) return;
        try {
            await adminFetch(`/api/admin/scheduling/${classId}`, { method: 'DELETE' });
            alert('排课已删除，课程总学时已自动更新');
            await loadSchedules();
        } catch (error) { alert('删除失败：' + error.message); }
    };
})();
