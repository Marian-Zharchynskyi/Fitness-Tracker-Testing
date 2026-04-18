import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

const errorRate = new Rate('errors');

export const options = {
  vus: 1,
  duration: '10s',
  thresholds: {
    http_req_duration: ['p(95)<200'], // SLO: 95% of requests must complete below 200ms
    http_req_failed: ['rate<0.01'], // SLO: less than 1% of requests can fail
    errors: ['rate<0.01'], // SLO: less than 1% custom errors allowed
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5146';

export default function () {
  // Try to hit API to verify health
  const getResponse = http.get(`${BASE_URL}/users/get-all`);
  const getCheck = check(getResponse, {
    'users status is 200': (r) => r.status === 200,
    'users response time < 200ms': (r) => r.timings.duration < 200,
  });

  errorRate.add(!getCheck);
  sleep(1);
}
