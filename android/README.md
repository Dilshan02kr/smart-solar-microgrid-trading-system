# Smart Solar Microgrid Android

Native Android client built with Kotlin, XML Views, AndroidX, ViewBinding, and a single-Activity Navigation Component architecture. It supports Prosumer booking/QR display and the server-authoritative Grid Operator dashboard, QR verification, and completion flow.

## Open and build

Open the `android/` directory in Android Studio. The project uses the included Gradle wrapper and Java 17 bytecode; Android Studio's bundled JBR 21 is supported.

From PowerShell:

```powershell
.\gradlew.bat assembleDebug
.\gradlew.bat lintDebug
.\gradlew.bat testDebugUnitTest
```

Do not commit `local.properties`, generated build directories, APKs, or AABs.

## Google Maps setup

Station discovery uses Google Maps SDK for Android and real coordinates returned by `GET /api/stations`. Create a Google Maps Platform key, enable **Maps SDK for Android** in its Google Cloud project, and add the key only to your untracked `android/local.properties` file:

```properties
MAPS_API_KEY=your_local_key
```

Keep any existing `sdk.dir` entry. The project compiles without a key and shows a setup message instead of initializing the map. Never commit `local.properties` or place the key in Kotlin, XML resources, README content, or tracked Gradle properties.

Restrict the key to **Android apps** using the application package and signing-certificate SHA-1. The base application ID is `com.smartsolar.microgrid`; debug builds use `com.smartsolar.microgrid.debug`. Obtain the relevant SHA-1 with:

```powershell
.\gradlew.bat signingReport
```

Restrict API usage to **Maps SDK for Android**. Google Cloud billing/API configuration may be required. Runtime testing needs an Internet-connected device or emulator with Google APIs/Play services.

This stage displays station markers and links them to the existing station-details flow. It intentionally does not request device location or add Places, geocoding, Directions, route drawing, or background tracking.

## API base URL

`API_BASE_URL` is a Gradle property and is exposed centrally as `BuildConfig.API_BASE_URL`. The checked-in development default is `http://10.0.2.2:5221/`, matching the backend HTTP launch profile through the emulator host alias; a trailing slash is added when absent.

Override it without editing source, for example:

```powershell
.\gradlew.bat assembleDebug -PAPI_BASE_URL=https://api.example.test/
```

The Android emulator uses `10.0.2.2` to reach the host machine; `localhost` points to the emulator itself. A physical device needs an HTTPS endpoint or a reachable LAN hostname/IP and a deliberately scoped debug network-security entry. Debug clear-text traffic is currently permitted only for `10.0.2.2` and `localhost`; release builds deny clear-text traffic.

Never place credentials, JWTs, API keys, or other secrets in Gradle properties committed to this repository.

## Architecture

- `core/network` owns the sole Retrofit/OkHttp construction path, bearer-token injection, redacted debug-only BASIC logging, and 401 invalidation. A 403 never clears the token.
- `core/session` isolates JWT persistence in private SharedPreferences. Passwords and form data are never stored.
- `core/error` normalizes the backend `{ code, message, errors }` response into safe UI errors.
- `data/remote` owns the exact authentication, Prosumer, station, energy-slot, and reservation contracts.
- `data/repository` restores session state, enforces active Prosumer role safety, normalizes network errors, and maps transport models to domain models.
- `data/local` uses `SQLiteOpenHelper` with a versioned `app_metadata` table. Tokens are not stored in SQLite.
- `domain/model` contains exact, safely parsed backend account, station, and reservation statuses.
- `ui/auth`, `ui/prosumer`, `ui/stations`, `ui/slots`, `ui/reservations`, `ui/launch`, and `ui/common` use Fragment -> ViewModel -> Repository flow and safe ViewBinding lifecycles.

Reservation mutations, operator verification, and completion are never queued offline; failures remain retryable server operations. SQLite remains limited to the existing metadata foundation. QR bitmaps and transaction references are not persisted locally, and decoded QR content is never trusted without server verification.

Future feature packages can be added under `ui/operator` and `ui/map` as their contracts are implemented.

All API services must be created from `NetworkModule`; Fragments must not create Retrofit clients or make direct network calls.
