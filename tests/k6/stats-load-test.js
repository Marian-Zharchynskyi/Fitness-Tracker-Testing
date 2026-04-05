import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

const errorRate = new Rate('errors');

export const options = {
  stages: [
    { duration: '30s', target: 10 },
    { duration: '1m', target: 50 },
    { duration: '30s', target: 100 },
    { duration: '1m', target: 100 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],
    errors: ['rate<0.1'],
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5146';

export default function () {
  const userId = '00000000-0000-0000-0000-000000000001';
  
  const statsResponse = http.get(`${BASE_URL}/users/${userId}/stats`);
  
  const statsCheck = check(statsResponse, {
    'stats status is 200': (r) => r.status === 200,
    'stats response time < 500ms': (r) => r.timings.duration < 500,
    'stats has valid data': (r) => {
      const body = JSON.parse(r.body);
      return body.totalWorkouts !== undefined && 
             body.totalCaloriesBurned !== undefined && 
             body.averageDurationMinutes !== undefined;
    },
  });
  
  errorRate.add(!statsCheck);

  const startDate = '2024-01-01';
  const endDate = '2024-12-31';
  const filteredStatsResponse = http.get(
    `${BASE_URL}/users/${userId}/stats?startDate=${startDate}&endDate=${endDate}`
  );
  
  const filteredCheck = check(filteredStatsResponse, {
    'filtered stats status is 200': (r) => r.status === 200,
    'filtered stats response time < 500ms': (r) => r.timings.duration < 500,
  });
  
  errorRate.add(!filteredCheck);

  sleep(1);
}
