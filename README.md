# Student-Course-Registration-Management-System

## 项目架构

```
StudentCourse.sln
└── Student-Course.csproj                    （ASP.NET Core 8.0 Web）

项目根目录/
├── Program.cs                               # 入口：注册DI、配置中间件
├── appsettings.json                         # Oracle 连接字符串
├── appsettings.Development.json
├── .gitignore
│
├── Infrastructure/                          # 共享：数据库连接层
│   └── DbConnectionFactory.cs
│
├── Student/                                  # 学生端模块（独立分区）
│   ├── Models/                               # 数据传输对象（DTO）
│   │   ├── UserSession.cs
│   │   ├── StudentInfo.cs
│   │   ├── StudentDashboardDto.cs
│   │   ├── StudentProfileRequests.cs
│   │   ├── CourseSelectionDto.cs
│   │   ├── CourseDetailDto.cs
│   │   ├── SelectionResultDto.cs
│   │   ├── ScheduleItemDto.cs
│   │   ├── EnrolledCourseDto.cs
│   │   ├── GpaSummaryDto.cs
│   │   └── CourseEvaluationDto.cs
│   │
│   ├── Repositories/                         # 数据访问层
│   │   ├── StudentProfileRepository.cs       # 模块一：学生信息 + Dashboard
│   │   ├── CourseSelectionRepository.cs      # 模块二：选课/退课 + 课表
│   │   ├── StudentGradeRepository.cs         # 模块三：成绩 + GPA
│   │   └── StudentEvaluationRepository.cs
│   │
│   ├── Services/                             # 业务逻辑层
│   │   ├── UserSessionContext.cs
│   │   ├── StudentProfileService.cs          # 模块一
│   │   ├── CourseSelectionService.cs         # 模块二
│   │   ├── StudentGradeService.cs            # 模块三
│   │   └── StudentEvaluationService.cs
│   │
│   ├── Controllers/                          # API 接口层
│   │   ├── SystemController.cs               # 系统：ping / 数据库测试 / Mock Session
│   │   ├── StudentProfileController.cs       # 模块一
│   │   ├── CourseSelectionController.cs      # 模块二
│   │   └── StudentGradeController.cs         # 模块三
│   │
│   └── wwwroot/                              # 前端 SPA
│       ├── index.html                        # 入口（跳转 /student.html）
│       ├── student.html                      # 学生端 SPA 壳
│       ├── css/
│       │   ├── student.css
│       │   └── site.css
│       └── js/
│           ├── api.js                        # 统一 API 通信层
│           ├── student-app.js                # SPA 路由
│           └── pages/
│               ├── student-dashboard.js      # 首页仪表盘
│               ├── student-profile.js        # 修改个人信息
│               ├── student-courses.js        # 选课中心
│               ├── student-schedule.js       # 我的课表
│               ├── student-grades.js         # 成绩查询
│               └── student-evaluation.js     # 课程评价
│
├── Pages/                                   # Razor Pages（最小骨架）
```

## API 路由

| 方法 | 路由 | 模块 | 说明 |
|------|------|------|------|
| GET | `/api/system/ping` | 系统 | 通信测试 |
| GET | `/api/system/database` | 系统 | Oracle 连接测试 |
| POST | `/api/dev/session/student` | 系统 | 开发用 Mock 学生登录 |
| POST | `/api/dev/session/teacher` | 系统 | 开发用 Mock 教师登录 |
| POST | `/api/auth/logout` | 系统 | 退出登录 |
| GET | `/api/student/current` | 模块一 | 当前学生信息 |
| GET | `/api/student/dashboard` | 模块一 | 首页仪表盘 |
| PUT | `/api/student/profile` | 模块一 | 修改联系方式 |
| POST | `/api/student/password` | 模块一 | 修改密码 |
| GET | `/api/student/courses/available` | 模块二 | 可选课程列表 |
| GET | `/api/student/courses/{id}` | 模块二 | 课程详情 |
| POST | `/api/student/courses/select` | 模块二 | 选课 |
| POST | `/api/student/courses/drop` | 模块二 | 退课 |
| GET | `/api/student/schedule` | 模块二 | 周课表 |
| GET | `/api/student/grades` | 模块三 | 已修课程成绩 |
| GET | `/api/student/gpa` | 模块三 | GPA 汇总 |
| GET | `/api/student/evaluations` | 模块三 | 课程评价 |
| POST | `/api/student/evaluations` | 模块三 | 提交评价 |
| GET | `/api/student/evaluations/history` | 模块三 | 评价历史 |

## 数据流

```
Oracle DB ←[Oracle.ManagedDataAccess.Core]→ Repository → Service → Controller → JSON
                                                                                  ↓
                                                                         fetch() ← api.js
                                                                                  ↓
                                                                         student-app.js
                                                                                  ↓
                                                                         pages/*.js → DOM
```

## 技术栈

- 后端：ASP.NET Core 8.0 · C# · Oracle.ManagedDataAccess.Core
- 前端：原生 JavaScript SPA · HTML5 · CSS3
- 数据库：Oracle
