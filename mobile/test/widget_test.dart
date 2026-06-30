import 'package:flutter_test/flutter_test.dart';
import 'package:kidguard_mobile/src/app.dart';

void main() {
  testWidgets('shows KidGuard startup page', (WidgetTester tester) async {
    await tester.pumpWidget(const KidGuardApp());

    expect(find.text('KidGuard'), findsOneWidget);
    expect(find.text('KidGuard Parent'), findsOneWidget);
    expect(find.text('Mobile project is ready.'), findsOneWidget);
  });
}
