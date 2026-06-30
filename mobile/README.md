# KidGuard Mobile

KidGuard parent mobile application built with Flutter.

## Run

Use the default local backend URL:

```powershell
flutter run -d windows
```

Use a custom backend URL without changing source code:

```powershell
flutter run -d windows --dart-define=KIDGUARD_API_BASE_URL=http://localhost:5080
```

## Verify

```powershell
flutter analyze
flutter test
flutter build windows
```

## API

The app consumes the backend API documented in `../docs/API_SPEC.md`.

Login currently calls `POST /auth/login` through `AuthRepository`.
