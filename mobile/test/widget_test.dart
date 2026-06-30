import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:kidguard_mobile/src/app.dart';
import 'package:kidguard_mobile/src/core/api/api_exception.dart';
import 'package:kidguard_mobile/src/features/auth/domain/auth_repository.dart';
import 'package:kidguard_mobile/src/features/auth/domain/auth_session.dart';
import 'package:kidguard_mobile/src/features/devices/domain/device_repository.dart';
import 'package:kidguard_mobile/src/features/devices/domain/device_summary.dart';
import 'package:kidguard_mobile/src/features/devices/domain/paired_device.dart';
import 'package:kidguard_mobile/src/features/logs/domain/device_log_entry.dart';

void main() {
  Future<void> login(WidgetTester tester) async {
    await tester.enterText(
      find.widgetWithText(TextFormField, 'Email'),
      'parent@gmail.com',
    );
    await tester.enterText(
      find.widgetWithText(TextFormField, 'Password'),
      '12345678',
    );
    await tester.tap(find.widgetWithText(FilledButton, 'Login'));
    await tester.pumpAndSettle();
  }

  Widget app({
    AuthRepository? authRepository,
    DeviceRepository? deviceRepository,
  }) {
    return KidGuardApp(
      authRepository: authRepository ?? const _FakeAuthRepository(),
      deviceRepository: deviceRepository ?? const _FakeDeviceRepository(),
    );
  }

  testWidgets('shows parent login screen', (WidgetTester tester) async {
    await tester.pumpWidget(app());

    expect(find.text('KidGuard'), findsOneWidget);
    expect(find.text('Parent Login'), findsOneWidget);
    expect(find.byType(TextFormField), findsNWidgets(2));
    expect(find.widgetWithText(FilledButton, 'Login'), findsOneWidget);
  });

  testWidgets('validates login form fields', (WidgetTester tester) async {
    await tester.pumpWidget(app());

    await tester.tap(find.widgetWithText(FilledButton, 'Login'));
    await tester.pump();

    expect(find.text('Email is required.'), findsOneWidget);
    expect(find.text('Password is required.'), findsOneWidget);
  });

  testWidgets('shows login API error', (WidgetTester tester) async {
    await tester.pumpWidget(
      app(authRepository: const _FakeAuthRepository(shouldFail: true)),
    );

    await tester.enterText(
      find.widgetWithText(TextFormField, 'Email'),
      'parent@gmail.com',
    );
    await tester.enterText(
      find.widgetWithText(TextFormField, 'Password'),
      'wrongpass',
    );
    await tester.tap(find.widgetWithText(FilledButton, 'Login'));
    await tester.pumpAndSettle();

    expect(find.text('Invalid email or password.'), findsOneWidget);
  });

  testWidgets('toggles password visibility', (WidgetTester tester) async {
    await tester.pumpWidget(app());

    EditableText passwordEditableText() {
      final passwordField = find.widgetWithText(TextFormField, 'Password');
      return tester.widget<EditableText>(
        find.descendant(of: passwordField, matching: find.byType(EditableText)),
      );
    }

    expect(passwordEditableText().obscureText, isTrue);

    await tester.tap(find.byIcon(Icons.visibility_outlined));
    await tester.pump();

    expect(passwordEditableText().obscureText, isFalse);
  });

  testWidgets('opens dashboard after valid login', (WidgetTester tester) async {
    await tester.pumpWidget(app());

    await login(tester);

    expect(find.text('Dashboard'), findsOneWidget);
    expect(find.text('Parent Overview'), findsOneWidget);
    expect(find.text('Quick Actions'), findsOneWidget);
    expect(find.text('View Devices'), findsOneWidget);
    expect(find.text('Pair Device'), findsOneWidget);
    expect(find.text('View Logs'), findsOneWidget);
  });
  testWidgets('pairs device from dashboard', (WidgetTester tester) async {
    await tester.pumpWidget(app());

    await login(tester);
    await tester.tap(find.text('Pair Device'));
    await tester.pumpAndSettle();
    await tester.enterText(
      find.widgetWithText(TextFormField, 'Pair Code'),
      'abcd1234',
    );
    await tester.tap(find.widgetWithText(FilledButton, 'Pair Device'));
    await tester.pumpAndSettle();

    expect(find.text('Device paired'), findsOneWidget);
    expect(find.text('New Windows PC'), findsOneWidget);
    expect(find.text('device-token-for-agent'), findsOneWidget);
  });

  testWidgets('shows pair device error state', (WidgetTester tester) async {
    await tester.pumpWidget(
      app(deviceRepository: const _FakeDeviceRepository(shouldFailPair: true)),
    );

    await login(tester);
    await tester.tap(find.text('Pair Device'));
    await tester.pumpAndSettle();
    await tester.enterText(
      find.widgetWithText(TextFormField, 'Pair Code'),
      'abcd1234',
    );
    await tester.tap(find.widgetWithText(FilledButton, 'Pair Device'));
    await tester.pumpAndSettle();

    expect(find.text('Invalid pair code.'), findsOneWidget);
  });

  testWidgets('opens device list from dashboard', (WidgetTester tester) async {
    await tester.pumpWidget(app());

    await login(tester);
    await tester.tap(find.text('View Devices'));
    await tester.pumpAndSettle();

    expect(find.text('Devices'), findsOneWidget);
    expect(find.text('Paired Devices'), findsOneWidget);
    expect(find.text('Study Room PC'), findsOneWidget);
    expect(find.text('Gaming Laptop'), findsOneWidget);
    expect(find.text('online'), findsOneWidget);
    expect(find.text('offline'), findsOneWidget);
    expect(find.text('study'), findsAtLeastNWidgets(1));
    expect(find.text('fun'), findsOneWidget);
  });

  testWidgets('shows device list error state', (WidgetTester tester) async {
    await tester.pumpWidget(
      app(deviceRepository: const _FakeDeviceRepository(shouldFail: true)),
    );

    await login(tester);
    await tester.tap(find.text('View Devices'));
    await tester.pumpAndSettle();

    expect(find.text('Unable to load devices'), findsOneWidget);
    expect(find.text('Retry'), findsOneWidget);
  });

  testWidgets('opens device detail from device list', (
    WidgetTester tester,
  ) async {
    await tester.pumpWidget(app());

    await login(tester);
    await tester.tap(find.text('View Devices'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Study Room PC'));
    await tester.pumpAndSettle();

    expect(find.text('Device Detail'), findsOneWidget);
    expect(find.text('Study Room PC'), findsOneWidget);
    expect(find.text('STUDY-PC'), findsOneWidget);
    expect(find.text('Protection Status'), findsOneWidget);
    expect(find.text('Current Mode'), findsOneWidget);
    expect(find.text('Recent Activity'), findsOneWidget);
    expect(find.text('Agent Version'), findsOneWidget);
  });

  testWidgets('shows device detail error state', (WidgetTester tester) async {
    await tester.pumpWidget(
      app(
        deviceRepository: const _FakeDeviceRepository(shouldFailDetail: true),
      ),
    );

    await login(tester);
    await tester.tap(find.text('View Devices'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Study Room PC'));
    await tester.pumpAndSettle();

    expect(find.text('Unable to load device'), findsOneWidget);
    expect(find.text('Retry'), findsOneWidget);
  });

  testWidgets('changes selected mode on device detail', (
    WidgetTester tester,
  ) async {
    await tester.pumpWidget(app());

    await login(tester);
    await tester.tap(find.text('View Devices'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Study Room PC'));
    await tester.pumpAndSettle();

    expect(find.text('Mode Control'), findsOneWidget);
    expect(find.text('fun'), findsAtLeastNWidgets(1));
    expect(find.text('study'), findsAtLeastNWidgets(1));
    expect(find.text('punishment'), findsOneWidget);

    await tester.tap(find.text('punishment'));
    await tester.pumpAndSettle();

    final currentModeRow = find.ancestor(
      of: find.text('Current Mode'),
      matching: find.byType(Row),
    );
    expect(
      find.descendant(of: currentModeRow, matching: find.text('punishment')),
      findsOneWidget,
    );
  });

  testWidgets('shows mode update error', (WidgetTester tester) async {
    await tester.pumpWidget(
      app(
        deviceRepository: const _FakeDeviceRepository(
          shouldFailModeUpdate: true,
        ),
      ),
    );

    await login(tester);
    await tester.tap(find.text('View Devices'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Study Room PC'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('punishment'));
    await tester.pumpAndSettle();

    expect(find.text('Unable to update mode.'), findsOneWidget);
    final currentModeRow = find.ancestor(
      of: find.text('Current Mode'),
      matching: find.byType(Row),
    );
    expect(
      find.descendant(of: currentModeRow, matching: find.text('study')),
      findsOneWidget,
    );
  });

  testWidgets('opens log list from dashboard', (WidgetTester tester) async {
    await tester.pumpWidget(app());

    await login(tester);
    await tester.tap(find.text('View Logs'));
    await tester.pumpAndSettle();

    expect(find.text('Logs'), findsOneWidget);
    expect(find.text('Activity Logs'), findsOneWidget);
    expect(find.text('steam.exe'), findsOneWidget);
    expect(find.text('blocked'), findsAtLeastNWidgets(1));
    expect(find.text('chrome.exe'), findsOneWidget);
  });

  testWidgets('shows device log error state', (WidgetTester tester) async {
    await tester.pumpWidget(
      app(deviceRepository: const _FakeDeviceRepository(shouldFailLogs: true)),
    );

    await login(tester);
    await tester.tap(find.text('View Devices'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Study Room PC'));
    await tester.pumpAndSettle();
    await tester.drag(find.byType(ListView).last, const Offset(0, -500));
    await tester.pumpAndSettle();
    await tester.tap(find.widgetWithText(OutlinedButton, 'View Logs'));
    await tester.pumpAndSettle();

    expect(find.text('Unable to load logs'), findsOneWidget);
    expect(find.text('Retry'), findsOneWidget);
  });

  testWidgets('opens device logs from device detail', (
    WidgetTester tester,
  ) async {
    await tester.pumpWidget(app());

    await login(tester);
    await tester.tap(find.text('View Devices'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Study Room PC'));
    await tester.pumpAndSettle();
    await tester.drag(find.byType(ListView).last, const Offset(0, -500));
    await tester.pumpAndSettle();
    final deviceLogButton = find.widgetWithText(OutlinedButton, 'View Logs');
    await tester.tap(deviceLogButton);
    await tester.pumpAndSettle();

    expect(find.text('Logs'), findsOneWidget);
    expect(find.text('Recent activity for Study Room PC.'), findsOneWidget);
    expect(find.text('discord.exe'), findsOneWidget);
    expect(find.text('punishment'), findsOneWidget);
  });
}

class _FakeAuthRepository implements AuthRepository {
  const _FakeAuthRepository({this.shouldFail = false});

  final bool shouldFail;

  @override
  Future<AuthSession> login({
    required String email,
    required String password,
  }) async {
    if (shouldFail) {
      throw const ApiException('Invalid email or password.');
    }

    return const AuthSession(accessToken: 'test-token', expiresIn: 3600);
  }
}

class _FakeDeviceRepository implements DeviceRepository {
  const _FakeDeviceRepository({
    this.shouldFail = false,
    this.shouldFailDetail = false,
    this.shouldFailModeUpdate = false,
    this.shouldFailLogs = false,
    this.shouldFailPair = false,
  });

  final bool shouldFail;
  final bool shouldFailDetail;
  final bool shouldFailModeUpdate;
  final bool shouldFailLogs;
  final bool shouldFailPair;

  static const devices = [
    DeviceSummary(
      deviceId: '11111111-1111-1111-1111-111111111111',
      name: 'Study Room PC',
      computerName: 'STUDY-PC',
      mode: 'study',
      isOnline: true,
      lastSeen: 'Online now',
    ),
    DeviceSummary(
      deviceId: '22222222-2222-2222-2222-222222222222',
      name: 'Gaming Laptop',
      computerName: 'GAME-LAPTOP',
      mode: 'fun',
      isOnline: false,
      lastSeen: 'Last seen 2 hours ago',
    ),
  ];

  @override
  Future<PairedDevice> pairDevice({
    required String accessToken,
    required String pairCode,
  }) async {
    if (shouldFailPair) {
      throw const ApiException('Invalid pair code.');
    }

    return const PairedDevice(
      deviceId: '33333333-3333-3333-3333-333333333333',
      name: 'New Windows PC',
      mode: 'fun',
      deviceToken: 'device-token-for-agent',
    );
  }

  @override
  Future<List<DeviceSummary>> getDevices({required String accessToken}) async {
    if (shouldFail) {
      throw const ApiException('Unable to load devices.');
    }

    return devices;
  }

  @override
  Future<DeviceSummary> getDevice({
    required String accessToken,
    required String deviceId,
  }) async {
    if (shouldFailDetail) {
      throw const ApiException('Unable to load device.');
    }

    return devices.firstWhere((device) => device.deviceId == deviceId);
  }

  @override
  Future<List<DeviceLogEntry>> getDeviceLogs({
    required String accessToken,
    required String deviceId,
    int page = 1,
    int pageSize = 20,
  }) async {
    if (shouldFailLogs) {
      throw const ApiException('Unable to load logs.');
    }

    return const [
      DeviceLogEntry(
        processName: 'steam.exe',
        action: 'blocked',
        mode: 'study',
        message: 'Blocked distracting application during study mode.',
        createdAt: '10:24',
      ),
      DeviceLogEntry(
        processName: 'discord.exe',
        action: 'blocked',
        mode: 'punishment',
        message: 'Blocked chat application during strict mode.',
        createdAt: '09:42',
      ),
    ];
  }

  @override
  Future<String> updateDeviceMode({
    required String accessToken,
    required String deviceId,
    required String mode,
  }) async {
    if (shouldFailModeUpdate) {
      throw const ApiException('Unable to update mode.');
    }

    return mode;
  }
}
