function loadCourses() {
    fetch('/api/admin/courses')
        .then(res => res.json())
        .then(data => {
            const tbody = document.getElementById('courseTableBody');
            tbody.innerHTML = '';
            data.forEach(c => {
                tbody.innerHTML += `<tr><td>${c.c_name}</td><td>${c.c_credit}</td></tr>`;
            });
        });
}
function publishCourse() {
    const name = document.getElementById('cName').value;
    const credit = document.getElementById('cCredit').value;
    fetch('/api/admin/courses', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ c_name: name, c_credit: parseInt(credit) })
    }).then(res => { if(res.ok) { alert('🎉 发布成功'); loadCourses(); } });
}