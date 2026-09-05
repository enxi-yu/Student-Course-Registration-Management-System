(function(){
 const esc=window.sharedUi.escapeHtml;
 const avg=(xs,k)=>(xs.reduce((n,x)=>n+Number(x[k]||0),0)/xs.length).toFixed(1);
 async function load(root,picker){
  window.sharedUi.mountState(root,'loading','正在加载课程评价...');
  try{const rows=await window.nativeApi.request('teacher.getEvaluations',{semester:picker.value()});if(!rows.length){window.sharedUi.mountState(root,'empty','当前学期暂无学生评价');return;}
   const groups=new Map();rows.forEach(x=>{if(!groups.has(x.classId))groups.set(x.classId,[]);groups.get(x.classId).push(x);});
   root.innerHTML=[...groups.values()].map(xs=>{const x=xs[0],comments=xs.filter(a=>a.comment);return `<section class="panel"><div class="panel-heading-row"><div><h3 class="panel-title">${esc(x.courseName)} · ${esc(x.className)}</h3><p class="form-hint">${esc(window.academicSemester.format(x.semester))} · ${xs.length} 人参评</p></div><span class="term-badge">综合评分 ${avg(xs,'evalScore')}</span></div><div class="evaluation-metric-grid"><div class="metric-card"><span>教学设计</span><strong>${avg(xs,'d1Score')}</strong></div><div class="metric-card"><span>课堂讲授</span><strong>${avg(xs,'d2Score')}</strong></div><div class="metric-card"><span>教学互动</span><strong>${avg(xs,'d3Score')}</strong></div><div class="metric-card"><span>教学效果</span><strong>${avg(xs,'d4Score')}</strong></div></div><h4>文字反馈</h4>${comments.length?comments.map(c=>`<div class="evaluation-comment"><span>${esc(c.comment)}</span><small>${esc(c.evaluationTime)}</small></div>`).join(''):'<div class="empty-state">暂无文字反馈</div>'}</section>`;}).join('');
  }catch(e){window.sharedUi.mountState(root,'error',e.message||'评价加载失败',()=>load(root,picker));}
 }
 async function render(container){const selected=window.academicSemester.getCurrent().canonical,fields='<div class="field"><label>学年学期</label><div id="evaluation-semester"></div></div>',actions='<button class="primary-button" id="load-evaluations">查询评价</button>';container.innerHTML=window.sharedUi.filterBar(fields,actions)+'<div id="teacher-evaluation-root"></div>';const picker=window.academicSemester.mountPicker('evaluation-semester',{minimumStartYear:2024,selected});const root=document.getElementById('teacher-evaluation-root');document.getElementById('load-evaluations').onclick=()=>load(root,picker);await load(root,picker);}
 window.teacherPages=window.teacherPages||{};window.teacherPages.evaluations={render};
})();
