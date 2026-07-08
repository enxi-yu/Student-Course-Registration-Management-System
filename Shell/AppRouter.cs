using System;
using StudentCourse.Models;
using StudentCourse.Services;

namespace StudentCourse.Shell
{
    public static class AppRouter
    {
        private static MainShellForm _mainShell;

        internal static void Attach(MainShellForm mainShell)
        {
            _mainShell = mainShell;
        }

        public static void ShowTeacherHome(UserSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            UserSessionContext.Set(session);

            if (_mainShell != null)
            {
                _mainShell.NavigateToTeacherHome();
            }
        }
    }
}
