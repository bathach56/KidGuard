class ApiConfig {
  const ApiConfig({required this.baseUrl});

  static const defaultBaseUrl = String.fromEnvironment(
    'KIDGUARD_API_BASE_URL',
    defaultValue: 'http://localhost:5080',
  );

  final String baseUrl;

  Uri endpoint(String path, [Map<String, String>? queryParameters]) {
    final normalizedBase = baseUrl.endsWith('/')
        ? baseUrl.substring(0, baseUrl.length - 1)
        : baseUrl;
    final normalizedPath = path.startsWith('/') ? path : '/$path';

    return Uri.parse(
      '$normalizedBase$normalizedPath',
    ).replace(queryParameters: queryParameters);
  }
}
