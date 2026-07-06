import http from "k6/http";
import { check, group, sleep } from "k6";

export const options = {
  thresholds: {
    http_req_failed: ["rate<0.01"],
    // Azure Container Apps may scale to zero in this low-cost environment; smoke checks tolerate cold starts.
    http_req_duration: ["p(95)<60000"],
  },
  scenarios: {
    smoke: {
      executor: "constant-vus",
      vus: Number(__ENV.K6_VUS ?? 2),
      duration: __ENV.K6_DURATION ?? "30s",
    },
  },
};

const webUrl = (__ENV.TARGET_WEB_URL ?? "https://stayflow-prod-web.purplebay-4e22b9c6.westus3.azurecontainerapps.io").replace(/\/$/, "");
const apiUrl = (__ENV.TARGET_API_URL ?? "https://stayflow-prod-api.purplebay-4e22b9c6.westus3.azurecontainerapps.io").replace(/\/$/, "");
const adminEmail = __ENV.ADMIN_EMAIL ?? "admin@stayflow.local";
const adminPassword = __ENV.ADMIN_PASSWORD;

export default function () {
  group("public web", () => {
    const home = http.get(`${webUrl}/`);
    check(home, {
      "home returns 200": response => response.status === 200,
      "home renders StayFlow": response => response.body.includes("StayFlow Cloud"),
    });

    const signin = http.get(`${webUrl}/signin`);
    check(signin, {
      "signin returns 200": response => response.status === 200,
    });
  });

  group("api health", () => {
    const ready = http.get(`${apiUrl}/health/ready`);
    check(ready, {
      "ready returns 200": response => response.status === 200,
      "postgres is healthy": response => response.body.includes('"postgres"') && response.body.includes('"Healthy"'),
    });
  });

  if (adminPassword) {
    group("credential login", () => {
      const response = http.post(
        `${apiUrl}/account/login`,
        {
          email: adminEmail,
          password: adminPassword,
          returnUrl: "/",
        },
        {
          redirects: 0,
          headers: {
            Origin: webUrl,
            Referer: `${webUrl}/signin`,
          },
        },
      );

      check(response, {
        "login redirects after success": result => result.status === 302,
        "login sets identity cookie": result => result.headers["Set-Cookie"]?.includes("StayFlow.Identity") ?? false,
        "login does not return invalid credentials": result => !String(result.headers.Location ?? "").includes("invalid_credentials"),
      });
    });
  }

  sleep(1);
}
