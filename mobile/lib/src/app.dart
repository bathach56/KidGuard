import 'package:flutter/material.dart';

import 'core/api/api_client.dart';
import 'core/api/api_config.dart';
import 'features/auth/data/api_auth_repository.dart';
import 'features/auth/domain/auth_repository.dart';
import 'features/auth/presentation/login_screen.dart';

class KidGuardApp extends StatelessWidget {
  KidGuardApp({super.key, AuthRepository? authRepository})
    : authRepository =
          authRepository ??
          ApiAuthRepository(
            ApiClient(
              config: const ApiConfig(baseUrl: ApiConfig.defaultBaseUrl),
            ),
          );

  final AuthRepository authRepository;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'KidGuard',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF2563EB)),
        inputDecorationTheme: InputDecorationTheme(
          border: OutlineInputBorder(borderRadius: BorderRadius.circular(8)),
        ),
        filledButtonTheme: FilledButtonThemeData(
          style: FilledButton.styleFrom(
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(8),
            ),
          ),
        ),
        useMaterial3: true,
      ),
      home: LoginScreen(authRepository: authRepository),
    );
  }
}
