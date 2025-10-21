# AIPhotoCloud

> Cloud photo storage with an AI processing worker — updated README reflecting current `dev` work-in-progress.

[![Status](https://img.shields.io/badge/status-work%20in%20progress-orange)](https://github.com/sergeydragun/AIPhotoCloud)
[![License](https://img.shields.io/badge/license-MIT-lightgrey)]

---

## Summary

AIPhotoCloud is a small cloud-native photo storage system paired with an asynchronous AI processing worker. The repo is currently under active development (see `dev` branch or your latest PR). This README is written to reflect the current architecture: an Angular front-end (`PhotoCloud.WebApp`) that stores files in Azure Blob Storage (or Azurite for local testing) and dispatches processing work via RabbitMQ to an `AI.Worker` service that performs image analysis.

> Note: the project is intentionally lightweight and some pieces (AI inference code, worker plumbing) may be scaffolding/placeholders while the implementation evolves.

---

## High-level architecture

```
[Browser / Angular Frontend (PhotoCloud.WebApp)]
          │
          └──► Upload / Storage API (writes to Azure Blob Storage)
                    │
                    └──► Publish message to RabbitMQ (job/request)
                                │
                                └──► AI.Worker (consumes message, performs AI inference, writes metadata back to Azure)
```

Supporting services used in the development/test environment:

* **RabbitMQ** — message broker used to queue processing jobs.
* **Azurite** — local Azure Blob Storage emulator used in `docker-compose-tests-environment` for reproducible local testing.

---

## Components

* **PhotoCloud.WebApp**

  * The web application / client-facing API layer. The current UI is implemented with **Angular** (not React).
  * Accepts uploads from users, persists files to Azure Blob Storage, and publishes processing requests to RabbitMQ.

* **AI.Worker**

  * Background consumer that reads RabbitMQ messages, downloads images from Azure storage, runs AI processing (object detection / tagging / other), and stores analysis metadata (e.g. JSON sidecar files or database records) back to Azure.
  * AI inference may be implemented with ONNX / TensorFlow / PyTorch depending on the branch — treat the worker as the inference boundary.

* **docker-compose-tests-environment.yml**

  * A docker-compose environment used for local testing. It includes at least **rabbitmq** and **azurite** services so you can run the full pipeline locally without Azure.

---

## Quickstart — local development

The repository includes a `docker-compose` test environment that will start RabbitMQ and Azurite. This is the fastest way to run the system locally.

**1. Start test environment**

```bash
# from repo root
docker compose -f docker-compose-tests-environment.yml up --build
```

This will bring up services such as RabbitMQ and Azurite. Keep this running while you start the app services.

**2. Run PhotoCloud.WebApp (local dev mode)**

* The web app lives in `PhotoCloud.WebApp` and is an Angular application. Use the Angular CLI or your IDE to run it locally. Example (if Angular CLI is installed):

```bash
cd PhotoCloud.WebApp
npm install
npm run start
```

Ensure the web app is configured to use the local Azurite endpoints and RabbitMQ connection string (see **Configuration** below).

**3. Run AI.Worker**

Start the worker service using your runtime toolchain. The worker should be configured to read from the same RabbitMQ instance and Azurite storage.

```bash
# example (replace with actual run command used by the worker project)
cd AI.Worker
# build / run depending on the worker runtime
```

**4. Upload an image**

Open the Angular app in your browser and upload an image. The expected flow is:

1. WebApp uploads the file to Azure/Azurite.
2. WebApp publishes a processing message to RabbitMQ.
3. Worker consumes the message, runs AI analysis, and writes metadata back to storage.
4. WebApp displays the analysis results (once metadata is available).

---

## Configuration

Configure the following environment variables (or the equivalent application settings) for local dev and production:

* `AZURE_STORAGE_CONNECTION_STRING` — connection string for Azure Blob Storage (or Azurite local connection string for tests).
* `STORAGE_CONTAINER` — container name where photos and metadata are stored.
* `RABBITMQ_URL` — RabbitMQ connection URL (e.g. `amqp://guest:guest@rabbitmq:5672`).
* `MAX_UPLOAD_SIZE` — optional: maximum allowed upload size.
* `WORKER_CONCURRENCY` — optional: number of worker threads/consumers.

When using the `docker-compose-tests-environment.yml` file, update the web app and worker environment files (or `.env`) to point to the Azurite service (endpoint/connection string) and the RabbitMQ container name.

---

## API surface (recommended/typical)

Exact routes may change — use these as a friendly contract example. Replace with the repository's real routes when stabilizing the API.

* `POST /api/upload` — upload image(s). Returns storage URL and an identifier for the image/job.
* `GET  /api/images/{id}` — retrieve image metadata (including AI analysis results when available).
* `GET  /api/folders` — (optional) list folders/containers.

Responses typically return a small JSON object with `id`, `url`, and `status` (e.g. `pending` / `processing` / `done`), and, when ready, a `metadata` object containing detected labels, bounding boxes, and confidence scores.

---

## AI processing notes

* The worker is the place to implement AI inference. Common patterns:

  1. Download the original image from storage.
  2. Run inference (ONNXRuntime, TensorFlow, PyTorch, or a cloud AI service).
  3. Store results as a JSON sidecar next to the image or in a small database.
  4. Optionally emit an event or HTTP callback when analysis completes.

* Keep the worker idempotent and resilient (retry semantics for transient failures).

* If runs may be slow, consider using acknowledgement and requeue semantics in RabbitMQ to avoid losing work.

---

## Development tips

* Use Azurite for fast, local testing of Azure Blob Storage semantics.
* Use the provided `docker-compose-tests-environment.yml` to create a reproducible local stack (RabbitMQ + Azurite).
* Add health checks for the worker and WebApp to fail fast when dependent services are unavailable.
* Prefer writing metadata as compact JSON documents (`imageId.json`) or use a small DB (SQLite / Postgres) if you need indexing and queries.

---

## Roadmap / ideas

* Formalize the message schema for RabbitMQ (e.g. include `imageId`, `storagePath`, `requestedAnalyses`).
* Provide Docker Compose files for full-stack local environments (web + worker + rabbitmq + azurite).
* Add authentication and per-user storage with SAS tokens for Azure.
* Add a monitoring dashboard for queued jobs and worker throughput.
* Add CI steps that run `docker-compose-tests-environment` and run integration tests against it.

---

## Contributing

1. Work in the `dev` branch (or open PR as you did).
2. When an API or config changes, update the README's `API` and `Configuration` sections with exact values.
3. Add integration tests that use `docker-compose-tests-environment.yml` to validate end-to-end behavior.

---

## License & maintainer

**MIT**. Maintained by `sergeydragun`.

