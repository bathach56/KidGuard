import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:kidguard_mobile/src/app.dart';
import 'package:kidguard_mobile/src/core/api/api_exception.dart';
import 'package:kidguard_mobile/src/features/auth/domain/auth_repository.dart';
import 'package:kidguard_mobile/src/features/auth/domain/auth_session.dart';

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

  Widget app({AuthRepository? authRepository}) {
    return KidGuardApp(authRepository: authRepository ?? _FakeAuthRepository());
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
      app(authRepository: _FakeAuthRepository(shouldFail: true)),
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
