import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

const errorRate = new Rate('errors');

export const options = {
  stages: [
    { duration: '30s', target: 20 },
    { duration: '1m', target: 50 },
    { duration: '1m', target: 80 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<1000'],
    errors: ['rate<0.1'],
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5146';

const exercises = [
  'Bench Press',
  'Squat',
  'Deadlift',
  'Pull-ups',
  'Push-ups',
  'Shoulder Press',
  'Barbell Row',
];

export default function () {
  // Arrange
  const userId = '00000000-0000-0000-0000-000000000001';
  const exercise = exercises[Math.floor(Math.random() * exercises.length)];
  
  // Act
  const progressResponse = http.get(
    `${BASE_URL}/users/${userId}/progress?exercise=${encodeURIComponent(exercise)}`
  );
  // Assert
  
  const progressCheck = check(progressResponse, {
    'progress status is 200': (r) => r.status === 200,
    'progress response time < 1000ms': (r) => r.timings.duration < 1000,
    'progress returns array': (r) => {
      try {
        const body = JSON.parse(r.body);
        return Array.isArray(body);
      } catch {
        return false;
      }
    },
  });
  
  errorRate.add(!progressCheck);

  // Arrange
  const workoutsResponse = http.get(`${BASE_URL}/users/${userId}/workouts`);
  // Assert
  
  const workoutsCheck = check(workoutsResponse, {
    'workouts status is 200': (r) => r.status === 200,
    'workouts response time < 1000ms': (r) => r.timings.duration < 1000,
  });
  
  errorRate.add(!workoutsCheck);

  sleep(1);
}
