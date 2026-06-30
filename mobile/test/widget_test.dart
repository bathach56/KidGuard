import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:kidguard_mobile/src/app.dart';

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

  testWidgets('shows parent login screen', (WidgetTester tester) async {
    await tester.pumpWidget(const KidGuardApp());

    expect(find.text('KidGuard'), findsOneWidget);
    expect(find.text('Parent Login'), findsOneWidget);
    expect(find.byType(TextFormField), findsNWidgets(2));
    expect(find.widgetWithText(FilledButton, 'Login'), findsOneWidget);
  });

  testWidgets('validates login form fields', (WidgetTester tester) async {
    await tester.pumpWidget(const KidGuardApp());

    await tester.tap(find.widgetWithText(FilledButton, 'Login'));
    await tester.pump();

    expect(find.text('Email is required.'), findsOneWidget);
    expect(find.text('Password is required.'), findsOneWidget);
  });

  testWidgets('toggles password visibility', (WidgetTester tester) async {
    await tester.pumpWidget(const KidGuardApp());

    EditableText passwordEditableText() {
      final passwordField = find.widgetWithText(TextFormField, 'Password');
      return tester.widget<EditableText>(
        find.descendant(
          of: passwordField,
          matching: find.byType(EditableText),
        ),
      );
    }

    expect(passwordEditableText().obscureText, isTrue);

    await tester.tap(find.byIcon(Icons.visibility_outlined));
    await tester.pump();

    expect(passwordEditableText().obscureText, isFalse);
  });

  testWidgets('opens dashboard after valid local login', (WidgetTester tester) async {
    await tester.pumpWidget(const KidGuardApp());

    await login(tester);

    expect(find.text('Dashboard'), findsOneWidget);
    expect(find.text('Parent Overview'), findsOneWidget);
    expect(find.text('Quick Actions'), findsOneWidget);
    expect(find.text('View Devices'), findsOneWidget);
    expect(find.text('Pair Device'), findsOneWidget);
    expect(find.text('View Logs'), findsOneWidget);
  });

  testWidgets('opens device list from dashboard', (WidgetTester tester) async {
    await tester.pumpWidget(const KidGuardApp());

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
}
