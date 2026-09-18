# Alza task

## My understanding

- the task sounds kinda like leetcode problem with input/output using some interesting DSAs, but I interpreted it as a business requirement within distributed system => service which is responsible purely for planning deliveries to alzaboxes.
- Since there are no explicit ranges for the number of packages, I assume that the service should be able to handle a large number of packages (e.g. 1 million) and plan them to a fleet of vans (e.g. 120 vans, 2 trips per van, 5500 kg / 7 m³ per trip).
  - From my perspective, I assume planning is done a day before the actual delivery => ,,do půlnoci objednáš ráno v alzaboxu máš". It could be a cronjob resource deployed in k8s with a specific schedule.

- The solution does not consider:
  - Delivering of some low revenue packages to alzaboxes, dealing with some deadline which could be fixed for each package. In current way, there could be package that could never be delivered
  - Solution does not operate with multiple warehouses, it does not consider the fact that some packages could be in transit between warehouses and the warehouse where the planning is done. It only considers packages that are physically present in the warehouse at the time of planning.
  - Deletion of successfully delivered packages from the persistence.
  - Dynamicty changing van fleet size/capacity. It is configured via `VanConfig`, I can imagine in real system some service which would be responsible for managing the van fleet and its capacity.
  - Propagation of the planning result, just outputed for the sake of this task into a file `planning-result.json` next to the built binary.
  - the actual alzaboxes to which it should be delivered.
  - supporting multiple warehouses, I could imagine pararell jobs for each warehouse, but it is not implemented in this task.

## How planning works

- I can imagine a service which purpose is purely to plan deliveries to alzaboxes, service would most likely store packages in persistence to be able to plan them to vans or it would not persist those packages and fetch them from another service via REST API
  - In my case, I decided to implement a REST API which accepts packages and stores them in a database, or alternatively packages could be propagated via async communication (Messaging, ASB for example) to this service.

- Each package gets a rating: `Revenue / (Weight/MaxWeight + Volume/MaxVolume)` - revenue per unit of van capacity (weight + volume combined) it uses. A small package earning the same money as a big one rates higher, since it costs less van space to carry. This is computed per query, not stored, so it always reflects whatever van capacity is currently configured.
- Packages are fetched from the database ordered best-rating-first, in pages - not the whole backlog at once. Each page is bounded to `Weight <= maxRemainingWeight AND Volume <= maxRemainingVolume`, where those max values come from whichever open van trip currently has the most room, so we never fetch a package too big for every remaining trip.
- Each fetched package is placed into the first open trip that still has room for it. A trip that becomes full drops out of consideration.
- Once a page is used up, the max-weight/max-volume bound is recalculated from whatever capacity is actually left across the still-open trips, and the next page is fetched with that tighter bound - so as vans fill up, later fetches automatically stop pulling candidates that no longer fit anywhere.

- Once planning finishes, all assignments are written back in a single `UPDATE` statement (raw parameterized SQL using `UNNEST`) instead of one statement per package. This needs raw SQL specifically: each row gets a *different* `VanTripId`, and EF Core's LINQ provider (`ExecuteUpdate`) can only translate an update that sets every matched row to the same value or one derived from that row's own columns - it can't translate "look up this row's new value from a client-side list of (id, value) pairs." Confirmed by trying it: `Packages.Join(assignments, ...).ExecuteUpdateAsync(...)` throws at runtime with "could not be translated." The raw query is still fully parameterized (`ids`/`tripIds` bound as real array parameters via `UNNEST`, never string-concatenated).
  - The persistence schema is not something I would normally design for a real system, but it is sufficient for this task. In a real system, I would probably have a separate `VanTrip` table with a `TripId` primary key and a `VanId` foreign key, and the `Packages` table would have a nullable `TripId` foreign key. This would allow for more complex queries and better data integrity.


### Performance
- If we would imagine a real system, I believe the speed of planning for example approx 1 million packages to 120 vans (2 trips per van) would be fine if its in range of seconds => if some guy in warehouse would start filling up the vans a 10 seconds later, I believe it would not be a disaster.

- On my machine, with 1 million entities generated via the k6 script, it took arround 1.5seconds.

## Prerequisites

- .NET 8 SDK
- Docker (for Postgres, and for the Testcontainers-based integration test)

## Running it

### 1. Start Postgres

```bash
docker compose up -d
```

`docker-compose.yml` and the `SqlDatabase` section in each project's `appsettings*.json` have
`[FILL_UP]` placeholders for credentials - fill them in consistently (e.g. `alza`/`alza`) before
starting. `Alza.Delivery.Api` and `Alza.Delivery.DataMigrations` already have working defaults;
`Alza.Delivery.BackgroundJobs` does not.

### 2. Apply migrations

```bash
dotnet run --project Alza.Delivery.DataMigrations
```

### 3. Seed packages

Start the API

Then either run the k6 script (seeds 1,000,000 packages by default - see
`scripts/k6/README.md` for options):

```bash
k6 run scripts/k6/seed-packages.js
```

or POST directly:

```bash
curl -X POST http://localhost:5051/api/orders \
  -H "Content-Type: application/json" \
  -d '{"packages":[{"weight":12.5,"volume":0.3,"revenue":450}]}'
```

### 4. Run the planning job

```bash
dotnet run --project Alza.Delivery.BackgroundJobs
```

Van fleet size/capacity is configured via `VanConfig` in
`Alza.Delivery.BackgroundJobs/appsettings.Development.json` (defaults: 120 vans, 2 trips/van,
5500 kg / 7 m³ per trip). The job assigns packages, writes them to the
database, then exits after writing `planning-result.json` (total revenue + per-trip breakdown)
next to the built binary.
