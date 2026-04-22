import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

const errorRate = new Rate('errors');

export const options = {
  stages: [
    { duration: '30s', target: 20 },
    { duration: '1m', target: 50 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<1000'],
    http_req_failed: ['rate<0.1'],
    errors: ['rate<0.15'],
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5146';

export function setup() {
  // Arrange
  const usersResponse = http.get(`${BASE_URL}/users/get-all`);
  let userId = '00000000-0000-0000-0000-000000000001';
  
  if (usersResponse.status === 200) {
    try {
      const users = JSON.parse(usersResponse.body);
      if (users && users.length > 0) {
        userId = users[0].id;
      } else {
        const userPayload = JSON.stringify({
          name: `Test User ${Date.now()}`,
          email: `test${Date.now()}@example.com`,
          password: 'Password123!',
        });

        const userResponse = http.post(`${BASE_URL}/users/create`, userPayload, {
          headers: { 'Content-Type': 'application/json' },
        });
        
        if (userResponse.status === 200) {
          const user = JSON.parse(userResponse.body);
          userId = user.id;
        }
      }
    } catch (e) {
      console.log('Failed to parse users response');
    }
  }
  return { userId: userId };
}

export default function (data) {
  // Arrange
  const userId = data.userId;
  
  const workoutPayload = JSON.stringify({
    userId: userId,
    name: `Workout ${Date.now()}`,
    notes: 'Load test workout',
    date: new Date(Date.now() - Math.random() * 30 * 24 * 60 * 60 * 1000).toISOString(),
    durationMinutes: Math.floor(Math.random() * 90) + 15,
    caloriesBurned: Math.floor(Math.random() * 600) + 100,
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  const createResponse = http.post(`${BASE_URL}/api/workouts`, workoutPayload, params);
  // Assert
  
  const createCheck = check(createResponse, {
    'create workout status is 200': (r) => r.status === 200,
    'create workout response time < 1000ms': (r) => r.timings.duration < 1000,
    'workout has id': (r) => {
      try {
        const body = JSON.parse(r.body);
        return body.id !== undefined;
      } catch {
        return false;
      }
    },
  });
  
  errorRate.add(!createCheck);

  if (createResponse.status === 200) {
    const workout = JSON.parse(createResponse.body);
    
  // Act
    const getResponse = http.get(`${BASE_URL}/api/workouts/${workout.id}`);
  // Assert
    const getCheck = check(getResponse, {
      'get workout status is 200': (r) => r.status === 200,
      'get workout response time < 500ms': (r) => r.timings.duration < 500,
    });
    
    errorRate.add(!getCheck);

    const exercisePayload = JSON.stringify({
      name: 'Bench Press',
      sets: 3,
      reps: 10,
      weightKg: 80,
      durationSeconds: null,
    });

    const addExerciseResponse = http.post(
      `${BASE_URL}/api/workouts/${workout.id}/exercises`,
      exercisePayload,
      params
    );
  // Assert
    
    const exerciseCheck = check(addExerciseResponse, {
      'add exercise status is 200': (r) => r.status === 200,
      'add exercise response time < 1000ms': (r) => r.timings.duration < 1000,
    });
    
    errorRate.add(!exerciseCheck);
  }

  sleep(Math.random() * 2 + 1);
}
