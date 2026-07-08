(function () {
  const pending = new Map();

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

  function request(action, payload) {
    return new Promise((resolve, reject) => {
      if (!window.chrome || !window.chrome.webview) {
        reject(new Error("WebView2 通信不可用。"));
        return;
      }

      const requestId = createRequestId();
      pending.set(requestId, { resolve, reject });

      window.chrome.webview.postMessage({
        requestId,
        action,
        payload: payload || {}
      });
    });
  }

  if (window.chrome && window.chrome.webview) {
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
        handlers.reject(new Error(response.error || "操作失败。"));
      }
    });
  }

  window.nativeApi = {
    request
  };
})();
