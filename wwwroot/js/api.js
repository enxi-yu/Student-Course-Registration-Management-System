(function () {
  const pending = new Map();

  const routeMap = {
    "system.ping": { method: "GET", url: "/api/system/ping" },
    "system.testDbConnection": { method: "GET", url: "/api/system/database" },
    "app.useMockTeacherSession": { method: "POST", url: "/api/dev/session/teacher" },
    "app.useMockStudentSession": { method: "POST", url: "/api/dev/session/student" },
    "app.logout": { method: "POST", url: "/api/auth/logout" },
    "account.changePassword": { method: "POST", url: "/api/auth/password" },
    "teacher.getCurrentTeacher": { method: "GET", url: "/api/teacher/current" },
    "teacher.getDashboard": { method: "GET", url: "/api/teacher/dashboard" },
    "teacher.getMyCourses": { method: "GET", url: "/api/teacher/courses" },
    "teacher.getMySchedule": { method: "GET", url: "/api/teacher/schedule" },
    "teacher.getClassStudents": { method: "GET", url: "/api/teacher/classes/{classId}/students" },
    "teacher.exportClassStudentsCsv": { method: "GET", url: "/api/teacher/classes/{classId}/students/export" },
    "teacher.getCourseApplications": { method: "GET", url: "/api/teacher/course-applications" },
    "teacher.submitCourseApplication": { method: "POST", url: "/api/teacher/course-applications" },
    "score.getScoreSheet": { method: "GET", url: "/api/teacher/classes/{classId}/scores" },
    "score.saveScore": { method: "POST", url: "/api/teacher/scores" },
    "score.batchSaveScores": { method: "POST", url: "/api/teacher/classes/{classId}/scores/batch" }
  };

  function createRequestId() {
    if (window.crypto && typeof window.crypto.randomUUID === "function") {
      return window.crypto.randomUUID();
    }

    return String(Date.now()) + "-" + Math.random().toString(16).slice(2);
  }

  function normalizeMessage(data) {
    if (typeof data === "string") {
      return JSON.parse(data);
    }

    return data;
  }

  function hasWebViewBridge() {
    return !!(window.chrome && window.chrome.webview);
  }

  function request(action, payload) {
    if (hasWebViewBridge()) {
      return requestNative(action, payload);
    }

    return requestHttp(action, payload);
  }

  function requestNative(action, payload) {
    return new Promise((resolve, reject) => {
      const requestId = createRequestId();
      pending.set(requestId, { resolve, reject });

      window.chrome.webview.postMessage({
        requestId,
        action,
        payload: payload || {}
      });
    });
  }

  async function requestHttp(action, payload) {
    const route = routeMap[action];
    if (!route) {
      throw new Error("No HTTP route is mapped for action: " + action);
    }

    const requestPayload = payload || {};
    const url = buildUrl(route.url, requestPayload, route.method);
    const options = {
      method: route.method,
      headers: {
        "Accept": "application/json"
      },
      credentials: "include"
    };

    if (route.method !== "GET") {
      options.headers["Content-Type"] = "application/json";
      options.body = JSON.stringify(requestPayload);
    }

    const response = await fetch(url, options);
    const contentType = response.headers.get("content-type") || "";
    const data = contentType.includes("application/json")
      ? await response.json()
      : await response.text();

    if (!response.ok) {
      const message = data && data.message ? data.message : String(data || response.statusText);
      throw new Error(message);
    }

    if (action === "system.testDbConnection" && data && data.success === false) {
      throw new Error(data.error || data.message || "Oracle connection failed");
    }

    return data;
  }

  function buildUrl(template, payload, method) {
    let url = template.replace(/\{([^}]+)\}/g, (_, key) => encodeURIComponent(payload[key] || ""));

    if (method === "GET") {
      const usedKeys = Array.from(template.matchAll(/\{([^}]+)\}/g)).map((match) => match[1]);
      const query = Object.keys(payload)
        .filter((key) => usedKeys.indexOf(key) < 0)
        .filter((key) => payload[key] !== undefined && payload[key] !== null && payload[key] !== "")
        .map((key) => `${encodeURIComponent(key)}=${encodeURIComponent(payload[key])}`)
        .join("&");

      if (query) {
        url += (url.indexOf("?") >= 0 ? "&" : "?") + query;
      }
    }

    return url;
  }

  if (hasWebViewBridge()) {
    window.chrome.webview.addEventListener("message", (event) => {
      let response;

      try {
        response = normalizeMessage(event.data);
      } catch (error) {
        return;
      }

      if (!response || !response.requestId || !pending.has(response.requestId)) {
        return;
      }

      const handlers = pending.get(response.requestId);
      pending.delete(response.requestId);

      if (response.success) {
        handlers.resolve(response.data);
      } else {
        handlers.reject(new Error(response.error || "操作失败"));
      }
    });
  }

  window.nativeApi = {
    request,
    mode: hasWebViewBridge() ? "webview" : "http",
    routes: routeMap
  };
})();
