import 'package:flutter/material.dart';

import 'core/api/api_client.dart';
import 'core/api/api_config.dart';
import 'features/auth/data/api_auth_repository.dart';
import 'features/auth/domain/auth_repository.dart';
import 'features/auth/presentation/login_screen.dart';
import 'features/devices/data/api_device_repository.dart';
import 'features/devices/domain/device_repository.dart';

class KidGuardApp extends StatelessWidget {
  KidGuardApp({
    super.key,
    AuthRepository? authRepository,
    DeviceRepository? deviceRepository,
  }) : authRepository =
           authRepository ??
           ApiAuthRepository(
             ApiClient(
               config: const ApiConfig(baseUrl: ApiConfig.defaultBaseUrl),
             ),
           ),
       deviceRepository =
           deviceRepository ??
           ApiDeviceRepository(
             ApiClient(
               config: const ApiConfig(baseUrl: ApiConfig.defaultBaseUrl),
             ),
           );

  final AuthRepository authRepository;
  final DeviceRepository deviceRepository;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'KidGuard',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        scaffoldBackgroundColor: const Color(0xFFFFFBF7),
        appBarTheme: const AppBarTheme(
          backgroundColor: Color(0xFFFFFBF7),
          foregroundColor: Color(0xFF0F172A),
          elevation: 0,
          centerTitle: true,
          titleTextStyle: TextStyle(
            color: Color(0xFF0F172A),
            fontSize: 20,
            fontWeight: FontWeight.bold,
          ),
        ),
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color(0xFF4B56D2),
          primary: const Color(0xFF4B56D2),
          background: const Color(0xFFFFFBF7),
          surface: Colors.white,
          onBackground: const Color(0xFF0F172A),
          onSurface: const Color(0xFF0F172A),
          error: const Color(0xFFEF4444),
        ),
        inputDecorationTheme: InputDecorationTheme(
          filled: true,
          fillColor: Colors.white,
          labelStyle: const TextStyle(color: Color(0xFF64748B)),
          border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(8),
            borderSide: const BorderSide(color: Color(0xFFE2E8F0), width: 1.5),
          ),
          enabledBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(8),
            borderSide: const BorderSide(color: Color(0xFFE2E8F0), width: 1.5),
          ),
          focusedBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(8),
            borderSide: const BorderSide(color: Color(0xFF4B56D2), width: 1.5),
          ),
          errorBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(8),
            borderSide: const BorderSide(color: Color(0xFFEF4444), width: 1.5),
          ),
        ),
        filledButtonTheme: FilledButtonThemeData(
          style: FilledButton.styleFrom(
            backgroundColor: const Color(0xFF4B56D2),
            foregroundColor: Colors.white,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10),
            ),
            elevation: 0,
          ),
        ),
        useMaterial3: true,
      ),
      home: LoginScreen(
        authRepository: authRepository,
        deviceRepository: deviceRepository,
      ),
    );
  }
}
