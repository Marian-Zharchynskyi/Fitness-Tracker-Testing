# k6 Performance Tests

This directory contains k6 performance tests for the Fitness Tracker API.

## Prerequisites

Install k6: https://k6.io/docs/get-started/installation/

## Running Tests

### Stats Load Test
Tests the user statistics endpoint under load:
```bash
k6 run stats-load-test.js
```

With custom base URL:
```bash
k6 run --env BASE_URL=http://localhost:5000 stats-load-test.js
```

### Workout Stress Test
Stress tests workout creation, retrieval, and exercise addition:
```bash
k6 run workout-stress-test.js
```

### Progress Load Test
Tests exercise progress tracking endpoints:
```bash
k6 run progress-load-test.js
```

## Test Scenarios

### Stats Load Test
- **Duration**: ~3.5 minutes
- **Max VUs**: 100
- **Thresholds**: 
  - 95th percentile response time < 500ms
  - Error rate < 10%

### Workout Stress Test
- **Duration**: ~8 minutes
- **Max VUs**: 300
- **Thresholds**: 
  - 95th percentile response time < 1000ms
  - HTTP failure rate < 10%
  - Error rate < 15%

### Progress Load Test
- **Duration**: ~3 minutes
- **Max VUs**: 80
- **Thresholds**: 
  - 95th percentile response time < 1000ms
  - Error rate < 10%

## Interpreting Results

k6 provides detailed metrics including:
- **http_req_duration**: Response time statistics
- **http_req_failed**: Failed request rate
- **iterations**: Number of complete test iterations
- **vus**: Virtual users (concurrent users)

Check if all thresholds pass (✓) for successful performance validation.
