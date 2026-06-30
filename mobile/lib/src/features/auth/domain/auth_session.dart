class AuthSession {
  const AuthSession({required this.accessToken, required this.expiresIn});

  factory AuthSession.fromJson(Object? json) {
    if (json is! Map<String, dynamic>) {
      throw const FormatException('Invalid auth session response.');
    }

    return AuthSession(
      accessToken: json['accessToken']?.toString() ?? '',
      expiresIn: json['expiresIn'] is int
          ? json['expiresIn'] as int
          : int.tryParse(json['expiresIn']?.toString() ?? '') ?? 0,
    );
  }

  final String accessToken;
  final int expiresIn;
}
