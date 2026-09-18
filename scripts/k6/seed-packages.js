import http from 'k6/http';
import { check } from 'k6';
import { Counter } from 'k6/metrics';

const baseUrl = __ENV.BASE_URL || 'http://localhost:5051';
const packagesPerRequest = Number(__ENV.PACKAGES_PER_REQUEST || 100);
const targetPackages = Number(__ENV.TOTAL_PACKAGES || 1_000_000);
const duration = __ENV.DURATION || '1m';

if (!Number.isInteger(packagesPerRequest) || packagesPerRequest <= 0) {
    throw new Error('PACKAGES_PER_REQUEST must be a positive integer.');
}

if (!Number.isInteger(targetPackages) || targetPackages <= 0) {
    throw new Error('TOTAL_PACKAGES must be a positive integer.');
}

if (targetPackages % packagesPerRequest !== 0) {
    throw new Error('TOTAL_PACKAGES must be divisible by PACKAGES_PER_REQUEST for an exact seed count.');
}

const requestsPerMinute = targetPackages / packagesPerRequest;
const packagesCreated = new Counter('packages_created');

export const options = {
    discardResponseBodies: true,
    scenarios: {
        seed_packages: {
            executor: 'constant-arrival-rate',
            rate: requestsPerMinute,
            timeUnit: '1m',
            duration,
            preAllocatedVUs: Number(__ENV.PRE_ALLOCATED_VUS || 200),
            maxVUs: Number(__ENV.MAX_VUS || 1_000),
        },
    },
    thresholds: {
        http_req_failed: ['rate<0.01'],
        http_req_duration: ['p(95)<2000'],
    },
};

export default function () {
    const response = http.post(
        `${baseUrl}/api/orders`,
        JSON.stringify({ packages: createPackages(packagesPerRequest) }),
        {
            headers: { 'Content-Type': 'application/json' },
            tags: { operation: 'seed_packages' },
        },
    );

    const succeeded = check(response, {
        'order accepted': (result) => result.status === 204,
    });

    if (succeeded) {
        packagesCreated.add(packagesPerRequest);
    }
}

function createPackages(count) {
    const packages = new Array(count);

    for (let index = 0; index < count; index++) {
        packages[index] = {
            weight: round(randomBetween(0.1, 1_500), 3),
            volume: round(randomBetween(0.5, 3.5), 3),
            revenue: round(randomBetween(100, 1000), 2),
        };
    }

    return packages;
}

function randomBetween(minimum, maximum) {
    return minimum + Math.random() * (maximum - minimum);
}

function round(value, decimals) {
    const factor = 10 ** decimals;
    return Math.round(value * factor) / factor;
}
