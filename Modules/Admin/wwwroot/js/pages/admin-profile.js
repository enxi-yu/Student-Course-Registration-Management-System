(function () {
    // 管理员本人资料页
    const roleText = admin => (Number(admin.adminLevel) === 0 ? '超级管理员' : '教务管理员');
    const departmentText = admin => (Number(admin.adminLevel) === 0 ? '全校' : (admin.department || '未配置'));
    const withRole = admin => { if (admin) { admin.roleText = roleText(admin); admin.departmentText = departmentText(admin); } return admin; };
    // 短封装：method 默认 GET；body 传入时自动 JSON 序列化并带 Content-Type
    const call = (url, method, body) => window.adminFetch(url, {
        method: method || 'GET',headers: { 'Content-Type': 'application/json' },
        body: body === undefined ? undefined : JSON.stringify(body)
    });

    async function renderAdminProfile() {
        const root = document.getElementById('adminProfileRoot');
        if (!root) return;
        const heading = document.getElementById('profilePageHeading');
        if (heading && window.sharedUi) 
            heading.innerHTML = window.sharedUi.pageHeading('个人资料', '查看当前账号信息，维护联系方式和登录密码。');
        await window.sharedUi.renderProfilePage(root, {
            fields: [
                { label: '姓名', key: 'realName' },{ label: '管理员编号', key: 'adminNo' },
                { label: '登录账号', key: 'username' },{ label: '角色', key: 'roleText' },
                { label: '管理学院', key: 'departmentText' },
                { label: '手机号', key: 'phone', editable: true, type: 'tel' },{ label: '邮箱', key: 'email', editable: true, type: 'email' }
            ],
            load: () => call('/api/admin/current').then(withRole),
            update: payload => call('/api/admin/profile', 'PUT', payload).then(withRole),
            changePassword: payload => call('/api/admin/password', 'POST', payload),
            logout: () => call('/api/auth/logout', 'POST'),
            loginUrl: 'http://localhost:5100/Login'
        });
    }
    window.renderAdminProfile = renderAdminProfile;
})();
