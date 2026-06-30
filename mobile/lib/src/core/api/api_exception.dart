class ApiException implements Exception {
  const ApiException(this.message, {this.errors = const []});

  final String message;
  final List<String> errors;

  @override
  String toString() => message;
}
