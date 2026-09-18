# Package seeding with k6

The default run submits 100 packages per request and schedules 10,000 requests in one minute: **1,000,000 packages** in total, provided the API and database keep up.

Start the API and PostgreSQL first, then run:

```bash
k6 run scripts/k6/seed-packages.js
```

The script accepts these environment variables:

```bash
k6 run \
  -e BASE_URL=http://localhost:5051 \
  -e TOTAL_PACKAGES=1000000 \
  -e PACKAGES_PER_REQUEST=100 \
  -e DURATION=1m \
  scripts/k6/seed-packages.js
```

`TOTAL_PACKAGES` must be divisible by `PACKAGES_PER_REQUEST`. The `packages_created` metric counts only records in successful `204 No Content` responses. If k6 reports dropped iterations or failed requests, increase the API/database capacity or reduce the target rate; it should not be treated as a successful million-record seed.
